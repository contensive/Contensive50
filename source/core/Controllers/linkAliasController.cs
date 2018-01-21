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
using Contensive.Core;
using Contensive.Core.Models.DbModels;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
using Contensive.BaseClasses;
//
//
namespace Contensive.Core.Controllers {
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
    public class linkAliasController {
        //
        //====================================================================================================
        /// <summary>
        /// Returns the Alias link (SourceLink) from the actual link (DestinationLink)
        /// </summary>
        public static string getLinkAlias(coreController cpcore, int PageID, string QueryStringSuffix, string DefaultLink) {
            string linkAlias = DefaultLink;
            List<Models.DbModels.linkAliasModel> linkAliasList = linkAliasModel.createList(cpcore, PageID, QueryStringSuffix);
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
        public static void addLinkAlias(coreController cpcore, string linkAlias, int PageID, string QueryStringSuffix, bool OverRideDuplicate, bool DupCausesWarning) {
            string tempVar = "";
            addLinkAlias(cpcore, linkAlias, PageID, QueryStringSuffix, OverRideDuplicate, DupCausesWarning, ref tempVar);
        }
        //
        //====================================================================================================
        //
        public static void addLinkAlias(coreController cpcore, string linkAlias, int PageID, string QueryStringSuffix, bool OverRideDuplicate) {
            string tempVar = "";
            addLinkAlias(cpcore, linkAlias, PageID, QueryStringSuffix, OverRideDuplicate, false, ref tempVar);
        }
        //
        //====================================================================================================
        //
        public static void addLinkAlias(coreController cpcore, string linkAlias, int PageID, string QueryStringSuffix) {
            string tempVar = "";
            addLinkAlias(cpcore, linkAlias, PageID, QueryStringSuffix, false, false, ref tempVar);
        }
        //
        //====================================================================================================
        /// <summary>
        /// add a link alias to a page as the primary
        /// </summary>
        public static void addLinkAlias(coreController cpcore, string linkAlias, int PageID, string QueryStringSuffix, bool OverRideDuplicate, bool DupCausesWarning, ref string return_WarningMessage) {
            try {
                const string SafeStringLc = "0123456789abcdefghijklmnopqrstuvwxyz-_/";
                bool AllowLinkAlias = cpcore.siteProperties.getBoolean("allowLinkAlias", false);
                //
                string WorkingLinkAlias = linkAlias;
                if (!string.IsNullOrEmpty(WorkingLinkAlias)) {
                    //
                    // remove nonsafe URL characters
                    string Src = WorkingLinkAlias.Replace('\t', ' ');
                    WorkingLinkAlias = "";
                    for (int srcPtr = 0; srcPtr < Src.Length; srcPtr++) {
                        string TestChr = Src.Substring(srcPtr, 1).ToLower() ;
                        if (!SafeStringLc.Contains(TestChr)) {
                            TestChr = "\t";
                        }
                        WorkingLinkAlias += TestChr;
                    }
                    int Ptr = 0;
                    while (WorkingLinkAlias.Contains("\t\t") && (Ptr < 100)) {
                        WorkingLinkAlias = genericController.vbReplace(WorkingLinkAlias, "\t\t", "\t");
                        Ptr = Ptr + 1;
                    }
                    if (WorkingLinkAlias.Substring(WorkingLinkAlias.Length - 1) == "\t") {
                        WorkingLinkAlias = WorkingLinkAlias.Left(WorkingLinkAlias.Length - 1);
                    }
                    if (WorkingLinkAlias.Left(1) == "\t") {
                        WorkingLinkAlias = WorkingLinkAlias.Substring(1);
                    }
                    WorkingLinkAlias = genericController.vbReplace(WorkingLinkAlias, "\t", "-");
                    if (!string.IsNullOrEmpty(WorkingLinkAlias)) {
                        //
                        // Make sure there is not a folder or page in the wwwroot that matches this Alias
                        //
                        if (WorkingLinkAlias.Left(1) != "/") {
                            WorkingLinkAlias = "/" + WorkingLinkAlias;
                        }
                        //
                        if (genericController.vbLCase(WorkingLinkAlias) == genericController.vbLCase("/" + cpcore.serverConfig.appConfig.name)) {
                            //
                            // This alias points to the cclib folder
                            //
                            if (AllowLinkAlias) {
                                return_WarningMessage = ""
                                    + "The Link Alias being created (" + WorkingLinkAlias + ") can not be used because there is a virtual directory in your website directory that already uses this name."
                                    + " Please change it to ensure the Link Alias is unique. To set or change the Link Alias, use the Link Alias tab and select a name not used by another page.";
                            }
                        } else if (genericController.vbLCase(WorkingLinkAlias) == "/cclib") {
                            //
                            // This alias points to the cclib folder
                            //
                            if (AllowLinkAlias) {
                                return_WarningMessage = ""
                                    + "The Link Alias being created (" + WorkingLinkAlias + ") can not be used because there is a virtual directory in your website directory that already uses this name."
                                    + " Please change it to ensure the Link Alias is unique. To set or change the Link Alias, use the Link Alias tab and select a name not used by another page.";
                            }
                        } else if (cpcore.appRootFiles.pathExists(cpcore.serverConfig.appConfig.appRootFilesPath + "\\" + WorkingLinkAlias.Substring(1))) {
                            //ElseIf appRootFiles.pathExists(serverConfig.clusterPath & serverconfig.appConfig.appRootFilesPath & "\" & Mid(WorkingLinkAlias, 2)) Then
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
                            int CS = cpcore.db.csOpen("Link Aliases", "name=" + cpcore.db.encodeSQLText(WorkingLinkAlias), "", false, 0, false, false, "Name,PageID,QueryStringSuffix");
                            if (!cpcore.db.csOk(CS)) {
                                //
                                // Alias not found, create a Link Aliases
                                //
                                cpcore.db.csClose(ref CS);
                                CS = cpcore.db.csInsertRecord("Link Aliases", 0);
                                if (cpcore.db.csOk(CS)) {
                                    cpcore.db.csSet(CS, "Name", WorkingLinkAlias);
                                    //Call app.csv_SetCS(CS, "Link", Link)
                                    cpcore.db.csSet(CS, "Pageid", PageID);
                                    if (true) {
                                        cpcore.db.csSet(CS, "QueryStringSuffix", QueryStringSuffix);
                                    }
                                }
                            } else {
                                //
                                // Alias found, verify the pageid & QueryStringSuffix
                                //
                                int CurrentLinkAliasID = 0;
                                bool resaveLinkAlias = false;
                                int CS2 = 0;
                                int LinkAliasPageID = cpcore.db.csGetInteger(CS, "pageID");
                                if ((cpcore.db.csGetText(CS, "QueryStringSuffix").ToLower() == QueryStringSuffix.ToLower()) && (PageID == LinkAliasPageID)) {
                                    //
                                    // it maches a current entry for this link alias, if the current entry is not the highest number id,
                                    //   remove it and add this one
                                    //
                                    CurrentLinkAliasID = cpcore.db.csGetInteger(CS, "id");
                                    CS2 = cpcore.db.csOpenSql_rev("default", "select top 1 id from ccLinkAliases where pageid=" + LinkAliasPageID + " order by id desc");
                                    if (cpcore.db.csOk(CS2)) {
                                        resaveLinkAlias = (CurrentLinkAliasID != cpcore.db.csGetInteger(CS2, "id"));
                                    }
                                    cpcore.db.csClose(ref CS2);
                                    if (resaveLinkAlias) {
                                        cpcore.db.executeQuery("delete from ccLinkAliases where id=" + CurrentLinkAliasID);
                                        cpcore.db.csClose(ref CS);
                                        CS = cpcore.db.csInsertRecord("Link Aliases", 0);
                                        if (cpcore.db.csOk(CS)) {
                                            cpcore.db.csSet(CS, "Name", WorkingLinkAlias);
                                            cpcore.db.csSet(CS, "Pageid", PageID);
                                            if (true) {
                                                cpcore.db.csSet(CS, "QueryStringSuffix", QueryStringSuffix);
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
                                        //
                                        //Call app.csv_SetCS(CS, "Link", Link)
                                        cpcore.db.csSet(CS, "Pageid", PageID);
                                        if (true) {
                                            cpcore.db.csSet(CS, "QueryStringSuffix", QueryStringSuffix);
                                        }
                                    } else if (AllowLinkAlias) {
                                        //
                                        // This alias points to a different link, and link aliasing is in use, call it an error (but save record anyway)
                                        //
                                        if (DupCausesWarning) {
                                            if (LinkAliasPageID == 0) {
                                                int PageContentCID = Models.Complex.cdefModel.getContentId(cpcore, "Page Content");
                                                return_WarningMessage = ""
                                                    + "This page has been saved, but the Link Alias could not be created (" + WorkingLinkAlias + ") because it is already in use for another page."
                                                    + " To use Link Aliasing (friendly page names) for this page, the Link Alias value must be unique on this site. To set or change the Link Alias, clicke the Link Alias tab and select a name not used by another page or a folder in your website.";
                                            } else {
                                                int PageContentCID = Models.Complex.cdefModel.getContentId(cpcore, "Page Content");
                                                return_WarningMessage = ""
                                                    + "This page has been saved, but the Link Alias could not be created (" + WorkingLinkAlias + ") because it is already in use for another page (<a href=\"?af=4&cid=" + PageContentCID + "&id=" + LinkAliasPageID + "\">edit</a>)."
                                                    + " To use Link Aliasing (friendly page names) for this page, the Link Alias value must be unique. To set or change the Link Alias, click the Link Alias tab and select a name not used by another page or a folder in your website.";
                                            }
                                        }
                                    }
                                }
                            }
                            int linkAliasId = cpcore.db.csGetInteger(CS, "id");
                            cpcore.db.csClose(ref CS);
                            cpcore.cache.invalidateContent_Entity(cpcore, linkAliasModel.contentTableName, linkAliasId);
                        }
                    }
                }
            } catch (Exception) {

                throw;
            }
        }
        //
    }
}

