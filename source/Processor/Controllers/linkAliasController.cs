﻿
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
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.BaseClasses;
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
            List<Models.Db.LinkAliasModel> linkAliasList = LinkAliasModel.createList(core, PageID, QueryStringSuffix);
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
        //
        public static void addLinkAlias(CoreController core, string linkAlias, int PageID, string QueryStringSuffix, bool OverRideDuplicate, bool DupCausesWarning) {
            string tempVar = "";
            addLinkAlias(core, linkAlias, PageID, QueryStringSuffix, OverRideDuplicate, DupCausesWarning, ref tempVar);
        }
        //
        //====================================================================================================
        //
        public static void addLinkAlias(CoreController core, string linkAlias, int PageID, string QueryStringSuffix, bool OverRideDuplicate) {
            string tempVar = "";
            addLinkAlias(core, linkAlias, PageID, QueryStringSuffix, OverRideDuplicate, false, ref tempVar);
        }
        //
        //====================================================================================================
        //
        public static void addLinkAlias(CoreController core, string linkAlias, int PageID, string QueryStringSuffix) {
            string tempVar = "";
            addLinkAlias(core, linkAlias, PageID, QueryStringSuffix, false, false, ref tempVar);
        }
        //
        //====================================================================================================
        /// <summary>
        /// add a link alias to a page as the primary
        /// </summary>
        public static void addLinkAlias(CoreController core, string linkAlias, int PageID, string QueryStringSuffix, bool OverRideDuplicate, bool DupCausesWarning, ref string return_WarningMessage) {
            string hint = "";
            try {
                const string SafeStringLc = "0123456789abcdefghijklmnopqrstuvwxyz-_/";
                bool AllowLinkAlias = core.siteProperties.getBoolean("allowLinkAlias", false);
                //
                string WorkingLinkAlias = linkAlias;
                if (!string.IsNullOrEmpty(WorkingLinkAlias)) {
                    //
                    // remove nonsafe URL characters
                    string Src = WorkingLinkAlias.Replace('\t', ' ');
                    WorkingLinkAlias = "";
                    for (int srcPtr = 0; srcPtr < Src.Length; srcPtr++) {
                        string TestChr = Src.Substring(srcPtr, 1).ToLowerInvariant() ;
                        if (!SafeStringLc.Contains(TestChr)) {
                            TestChr = "\t";
                        }
                        WorkingLinkAlias += TestChr;
                    }
                    int Ptr = 0;
                    while (WorkingLinkAlias.Contains("\t\t") && (Ptr < 100)) {
                        WorkingLinkAlias = GenericController.vbReplace(WorkingLinkAlias, "\t\t", "\t");
                        Ptr = Ptr + 1;
                    }
                    if (WorkingLinkAlias.Substring(WorkingLinkAlias.Length - 1) == "\t") {
                        WorkingLinkAlias = WorkingLinkAlias.Left(WorkingLinkAlias.Length - 1);
                    }
                    if (WorkingLinkAlias.Left(1) == "\t") {
                        WorkingLinkAlias = WorkingLinkAlias.Substring(1);
                    }
                    WorkingLinkAlias = GenericController.vbReplace(WorkingLinkAlias, "\t", "-");
                    if (!string.IsNullOrEmpty(WorkingLinkAlias)) {
                        //
                        // Make sure there is not a folder or page in the wwwroot that matches this Alias
                        //
                        if (WorkingLinkAlias.Left(1) != "/") {
                            WorkingLinkAlias = "/" + WorkingLinkAlias;
                        }
                        //
                        if (GenericController.vbLCase(WorkingLinkAlias) == GenericController.vbLCase("/" + core.appConfig.name)) {
                            //
                            // This alias points to the cclib folder
                            //
                            if (AllowLinkAlias) {
                                return_WarningMessage = ""
                                    + "The Link Alias being created (" + WorkingLinkAlias + ") can not be used because there is a virtual directory in your website directory that already uses this name."
                                    + " Please change it to ensure the Link Alias is unique. To set or change the Link Alias, use the Link Alias tab and select a name not used by another page.";
                            }
                        } else if (GenericController.vbLCase(WorkingLinkAlias) == "/cclib") {
                            //
                            // This alias points to the cclib folder
                            //
                            if (AllowLinkAlias) {
                                return_WarningMessage = ""
                                    + "The Link Alias being created (" + WorkingLinkAlias + ") can not be used because there is a virtual directory in your website directory that already uses this name."
                                    + " Please change it to ensure the Link Alias is unique. To set or change the Link Alias, use the Link Alias tab and select a name not used by another page.";
                            }
                        } else if (core.wwwFiles.pathExists(core.appConfig.localWwwPath + "\\" + WorkingLinkAlias.Substring(1))) {
                            //ElseIf appRootFiles.pathExists(serverConfig.clusterPath & appConfig.appRootFilesPath & "\" & Mid(WorkingLinkAlias, 2)) Then
                            //
                            // This alias points to a different link, call it an error
                            //
                            if (AllowLinkAlias) {
                                return_WarningMessage = ""
                                    + "The Link Alias being created (" + WorkingLinkAlias + ") can not be used because there is a folder in your website directory that already uses this name."
                                    + " Please change it to ensure the Link Alias is unique. To set or change the Link Alias, use the Link Alias tab and select a name not used by another page.";
                            }
                        } else {
                            //
                            // Make sure there is one here for this
                            //
                            int linkAliasId = 0;
                            using (var csData = new CsModel(core)) {
                                csData.open("Link Aliases", "name=" + DbController.encodeSQLText(WorkingLinkAlias), "", false, 0, "Name,PageID,QueryStringSuffix");
                                if (!csData.ok()) {
                                    //
                                    // Alias not found, create a Link Aliases
                                    //
                                    csData.close();
                                    csData.insert("Link Aliases");
                                    if (csData.ok()) {
                                        csData.set("Name", WorkingLinkAlias);
                                        csData.set("Pageid", PageID);
                                        csData.set("QueryStringSuffix", QueryStringSuffix);
                                    }
                                } else {
                                    //
                                    // Alias found, verify the pageid & QueryStringSuffix
                                    //
                                    int CurrentLinkAliasID = 0;
                                    bool resaveLinkAlias = false;
                                    int LinkAliasPageID = csData.getInteger("pageID");
                                    if ((csData.getText("QueryStringSuffix").ToLowerInvariant() == QueryStringSuffix.ToLowerInvariant()) && (PageID == LinkAliasPageID)) {
                                        //
                                        // it maches a current entry for this link alias, if the current entry is not the highest number id,
                                        //   remove it and add this one
                                        //
                                        CurrentLinkAliasID = csData.getInteger("id");



                                        using (var CS2 = new CsModel(core)) {
                                            CS2.openSql("select top 1 id from ccLinkAliases where pageid=" + LinkAliasPageID + " order by id desc");
                                            if (CS2.ok()) {
                                                resaveLinkAlias = (CurrentLinkAliasID != CS2.getInteger("id"));
                                            }
                                            CS2.close();
                                            if (resaveLinkAlias) {
                                                core.db.executeQuery("delete from ccLinkAliases where id=" + CurrentLinkAliasID);
                                                CS2.close();
                                                CS2.insert("Link Aliases");
                                                if (CS2.ok()) {
                                                    CS2.set("Name", WorkingLinkAlias);
                                                    CS2.set("Pageid", PageID);
                                                    CS2.set("QueryStringSuffix", QueryStringSuffix);
                                                }
                                            }
                                        }

                                    } else {
                                        //
                                        // Does not match, this is either a change, or a duplicate that needs to be blocked
                                        //
                                        if (OverRideDuplicate) {
                                            //
                                            // change the Link Alias to the new link
                                            csData.set("Pageid", PageID);
                                            csData.set("QueryStringSuffix", QueryStringSuffix);
                                        } else if (AllowLinkAlias) {
                                            //
                                            // This alias points to a different link, and link aliasing is in use, call it an error (but save record anyway)
                                            //
                                            if (DupCausesWarning) {
                                                if (LinkAliasPageID == 0) {
                                                    int PageContentCID = Models.Domain.ContentMetadataModel.getContentId(core, "Page Content");
                                                    return_WarningMessage = ""
                                                        + "This page has been saved, but the Link Alias could not be created (" + WorkingLinkAlias + ") because it is already in use for another page."
                                                        + " To use Link Aliasing (friendly page names) for this page, the Link Alias value must be unique on this site. To set or change the Link Alias, clicke the Link Alias tab and select a name not used by another page or a folder in your website.";
                                                } else {
                                                    int PageContentCID = Models.Domain.ContentMetadataModel.getContentId(core, "Page Content");
                                                    return_WarningMessage = ""
                                                        + "This page has been saved, but the Link Alias could not be created (" + WorkingLinkAlias + ") because it is already in use for another page (<a href=\"?af=4&cid=" + PageContentCID + "&id=" + LinkAliasPageID + "\">edit</a>)."
                                                        + " To use Link Aliasing (friendly page names) for this page, the Link Alias value must be unique. To set or change the Link Alias, click the Link Alias tab and select a name not used by another page or a folder in your website.";
                                                }
                                            }
                                        }
                                    }
                                }
                                linkAliasId = csData.getInteger("id");
                                csData.close();
                            }
                            core.cache.invalidateDbRecord(linkAliasId, LinkAliasModel.contentTableName);
                            //
                            // -- force route reload if this is a webserver page
                            Models.Domain.RouteMapModel.invalidateCache(core);
                            core.routeMapCacheClear();
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex, "addLinkAlias exception, hint [" + hint + "]");
                throw;
            }
        }
        //
    }
}

