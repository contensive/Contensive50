
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Contensive.BaseClasses;
using Contensive.Models.Db;
using Contensive.Processor;
using Contensive.Processor.Controllers;


namespace Contensive.Addons.AdminNavigator {
    public class GetNodeClass : AddonBaseClass {
        //
        public struct NavigatorEnvironment {
            public string adminUrl;
            public bool isDeveloper;
            public string buildVersion;
            public string addonEditAddonUrlPrefix;
            public string addonEditCollectionUrlPrefix;
            public string contentFieldEditToolPrefix;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return the full navigator
        /// </summary>
        /// <param name="CP"></param>
        /// <returns></returns>
        public override object Execute(CPBaseClass CP) {
            string returnHtml = "";
            try {
                //
                // setup environment
                NavigatorEnvironment env = new NavigatorEnvironment {
                    adminUrl = CP.Site.GetText("adminurl"),
                    buildVersion = CP.Site.GetText("buildversion"),
                    isDeveloper = CP.User.IsDeveloper
                };
                if (env.isDeveloper) {
                    env.addonEditAddonUrlPrefix = env.adminUrl + "?cid=" + CP.Content.GetID("add-ons") + "&af=4&id=";
                    env.addonEditCollectionUrlPrefix = env.adminUrl + "?cid=" + CP.Content.GetID("add-on collections") + "&af=4&id=";
                    env.contentFieldEditToolPrefix = env.adminUrl + "?af=105&contentid=";
                }
                //
                var OpenNodeList = new List<string>(CP.Visit.GetText("AdminNavOpenNodeList").Split(','));
                string ParentNode = CP.Doc.GetText("nodeid");
                if (!string.IsNullOrEmpty(ParentNode)) {
                    if (OpenNodeList.Count.Equals(0)) {
                        OpenNodeList.Add(ParentNode);
                        CP.Visit.SetProperty("AdminNavOpenNodeList", String.Join(",", OpenNodeList.ToArray()));
                    } else {
                        if (!OpenNodeList.Contains(ParentNode.ToString())) {
                            OpenNodeList.Add(ParentNode);
                            CP.Visit.SetProperty("AdminNavOpenNodeList", String.Join(",", OpenNodeList.ToArray()));
                        }
                    }
                }

                string NavigatorJS = "";
                returnHtml = GetNodeList(CP, env, ParentNode, OpenNodeList, ref NavigatorJS);
                if (!string.IsNullOrEmpty(NavigatorJS)) {
                    NavigatorJS = "if(jQuery"
                        + "if(window.navDrop) {"
                        + NavigatorJS + "};";
                    CP.Doc.AddHeadJavascript(NavigatorJS);
                }
            } catch (Exception ex) {
                CP.Site.ErrorReport(ex);
            }
            return returnHtml;
        }
        //
        //====================================================================================================
        /// <summary>
        /// get Node list?
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="env"></param>
        /// <param name="ParentNode"></param>
        /// <param name="OpenNodeList"></param>
        /// <param name="Return_NavigatorJS"></param>
        /// <returns></returns>
        internal string GetNodeList(CPBaseClass cp, NavigatorEnvironment env, string ParentNode, List<string> OpenNodeList, ref string Return_NavigatorJS) {
            string returnNav = "";
            try {
                const bool AutoManageAddons = true;
                var Index = new Dictionary<int, int>();
                //
                {
                    string BakeName = null;
                    if (env.isDeveloper) {
                        BakeName = "AdminNav EmptyNodeList Dev";
                    } else if (cp.User.IsAdmin) {
                        BakeName = "AdminNav EmptyNodeList Admin";
                    } else {
                        BakeName = "AdminNav EmptyNodeList CM" + cp.User.Id;
                    }
                    List<string> EmptyNodeList = cp.Cache.GetObject<List<string>>(BakeName);
                    string SQL = null;
                    if (EmptyNodeList.Count > 0) {
                        cp.Site.TestPoint("adminNavigator, emptyNodeList from cache=[" + EmptyNodeList + "]");
                    } else {
                        using (CPCSBaseClass cs2 = cp.CSNew()) {
                            SQL = "select n.ID from ccMenuEntries n left join ccMenuEntries c on c.parentid=n.id Where c.ID Is Null group by n.id";
                            if (cs2.OpenSQL(SQL)) {
                                do {
                                    EmptyNodeList.Add(cs2.GetInteger("id").ToString());
                                    cs2.GoNext();
                                }
                                while (cs2.OK());
                            }
                            cs2.Close();
                        }
                        cp.Site.TestPoint("adminNavigator, emptyNodeList from db=[" + EmptyNodeList + "]");
                        cp.Cache.Store(BakeName, EmptyNodeList, NavigatorEntryModel.tableMetadata.tableNameLower);
                    }
                    //string EmptyNodeListInitial = EmptyNodeList;
                    string TopParentNode = ParentNode;
                    List<string> parentNodeStack = new List<string>();
                    if (string.IsNullOrWhiteSpace(TopParentNode)) {
                        //
                        // bad call
                        parentNodeStack.Add("");
                    } else {
                        //
                        // load ParentNodes with argument
                        parentNodeStack.Add(TopParentNode);
                    }
                    int LegacyMenuControlID = cp.Content.GetID("Menu Entries");
                    //
                    string NodeNavigatorJS = null;
                    string ATag = null;
                    string FieldList = null;
                    string IconNoSubNodes = null;
                    string NavigatorID = "";
                    int CollectionID = 0;
                    string s = null;
                    string Name = null;
                    int ContentID = 0;
                    int addonid = 0;
                    string Link = null;
                    string Criteria = null;
                    bool BlockSubNodes = false;
                    common.NodeTypeEnum NodeType = 0;
                    int NavIconType = 0;
                    string NavIconTitle = null;
                    string NavIconTitleHtmlEncoded = null;
                    int ContentControlID = 0;
                    CPCSBaseClass csChildList = cp.CSNew();
                    var linkSuffixList = new List<string>();
                    string NodeIDString = "";
                    switch (parentNodeStack[0]) {
                        //
                        // Open CS so:
                        //   Name = the caption that is displayed for the entry
                        //   ID (NavigatorID) = the NavigatorEntry the record represents
                        //       if the node has no navigation entry, NavigatorID=0 if there are no child nodes
                        //       this number is used for the open/close javascript, as well as stored to remember open/close state
                        //       during future hits, as the menu is built, this is checked in the open/close list for a match
                        //       NavigatorID=0 will only work if the node has not child nodes
                        //   AddonID = the ID of the addon that should be run if this entry is selected, 0 otherwise
                        //   CollectionID, if this is the manage add-ons section, this is the collection node
                        //   NewWindow = 0 or 1, if the link opens in new window
                        //   ContentID = the id of the content to be opened in list mode if the entry is clicked
                        //   Link = URL to link the menu entry
                        //   NodeIDString = unique string that represents this node
                        //       Navigator Entries - 'n'+EntryID
                        //       Collections = 'c'+CollectionID
                        //       Add-ons = 'a'+AddonID
                        //       CDefs = 'd'+ContentID
                        //
                        case common.NodeIDManageAddons:
                            //
                            // Special Case: clicked on Manage Add-ons ("manageaddons")
                            // Link to Add-on Manager
                            //
                            NodeIDString = "";
                            addonid = cp.Content.GetRecordID("Add-ons", "Add-on Manager");
                            s = s + GetNode(cp, env, 0, 0, 0, 0, 0, "?addonguid=" + common.AddonManagerGuid, addonid, 0, "Add-on Manager", LegacyMenuControlID, EmptyNodeList, "", common.NavIconTypeAddon, "Add-on Manager", AutoManageAddons, common.NodeTypeEnum.NodeTypeAddon, false, false, OpenNodeList, NodeIDString, ref NodeNavigatorJS, new List<string>());
                            Return_NavigatorJS = Return_NavigatorJS + NodeNavigatorJS;
                            //
                            // List Collections
                            //
                            FieldList = "Name,0 as id,ccaddoncollections.id as collectionid,0 as AddonID,0 as NewWindow,0 as ContentID,'' as LinkPage," + common.NavIconTypeFolder + " as NavIconType,Name as NavIconTitle,0 as SettingPageID,0 as HelpAddonID,0 as HelpCollectionID,0 as contentcontrolid,blockNavigatorNode,system";
                            //FieldList = "Name,id as collectionid,0 as ID,0 as AddonID,0 as NewWindow,0 as ContentID,'' as LinkPage," & NavIconTypeFolder & " as NavIconType,Name as NavIconTitle,0 as SettingPageID,0 as HelpAddonID,0 as HelpCollectionID,0 as contentcontrolid"
                            Criteria = "((system=0)or(system is null))";
                            if (!env.isDeveloper) {
                                //Criteria = "((system=0)or(system is null))"
                                if (string.CompareOrdinal(env.buildVersion, "4.1.512") >= 0) {
                                    Criteria = Criteria + "and((blockNavigatorNode=0)or(blockNavigatorNode is null))";
                                }
                            }
                            CPCSBaseClass cs3 = cp.CSNew();
                            NodeType = common.NodeTypeEnum.NodeTypeCollection;
                            BlockSubNodes = false;
                            if (cs3.Open("Add-on Collections", Criteria, "name", true, FieldList, Constants.sqlPageSizeDefault, 1)) {
                                do {
                                    Name = Convert.ToString(cs3.GetText("name")).Trim(' ');
                                    NavIconTitle = Name;
                                    CollectionID = cs3.GetInteger("collectionid");
                                    NodeIDString = common.NodeIDManageAddonsCollectionPrefix + "." + CollectionID;
                                    NavIconTitleHtmlEncoded = System.Net.WebUtility.HtmlEncode(NavIconTitle);
                                    linkSuffixList = new List<string>();
                                    if (env.isDeveloper) {
                                        linkSuffixList.Add("<a href=\"" + env.addonEditCollectionUrlPrefix + CollectionID + "\">edit</a>");
                                        if (cs3.GetBoolean("system")) {
                                            linkSuffixList.Add("sys");
                                        }
                                        if (cs3.GetBoolean("blockNavigatorNode")) {
                                            linkSuffixList.Add("dev");
                                        }
                                        //linkSuffixList = "&nbsp;(" + linkSuffixList + ")";
                                    }
                                    s = s + GetNode(cp, env, CollectionID, 0, 0, 0, 0, "", 0, 0, Name, LegacyMenuControlID, EmptyNodeList, "", common.NavIconTypeAddon, NavIconTitleHtmlEncoded, AutoManageAddons, common.NodeTypeEnum.NodeTypeCollection, false, false, OpenNodeList, NodeIDString, ref NodeNavigatorJS, linkSuffixList);
                                    Return_NavigatorJS = Return_NavigatorJS + NodeNavigatorJS;
                                    cs3.GoNext();
                                }
                                while (cs3.OK());
                            }
                            cs3.Close();
                            //CS = Main.openCSContent("Add-on Collections", Criteria, , , , , FieldList)
                            //NodeType = NodeTypeCollection
                            //BlockSubNodes = False
                            //Do While Main.iscsok(CS)
                            //    Name = Trim(csx.getText( "name"))
                            //    NavIconTitle = Name
                            //    CollectionID = csx.getInteger( "collectionid")
                            //    NodeIDString = NodeIDManageAddonsCollectionPrefix & "." & CollectionID
                            //    NavIconTitleHtmlEncoded = System.Net.WebUtility.HtmlEncode(NavIconTitle)
                            //    s = s & GetNavigatorNode(cp,CollectionID, 0, 0, 0, 0, "", 0, 0, Name, LegacyMenuControlID, EmptyNodeList, "", NavIconTypeAddon, NavIconTitleHtmlEncoded, AutoManageAddons, NodeTypeEnum.NodeTypeCollection, false, False, OpenNodeList, NodeIDString, NodeNavigatorJS,"")
                            //    Return_NavigatorJS = Return_NavigatorJS & NodeNavigatorJS
                            //    Call Main.NextCSRecord(CS)
                            //Loop
                            //Call Main.closeCs(CS)
                            //
                            // Advanced folder to contain edit links to create addons and collections
                            //
                            NodeIDString = common.NodeIDManageAddonsAdvanced.ToString();
                            s = s + GetNode(cp, env, 0, 0, 0, 0, 0, "", 0, 0, "Advanced", LegacyMenuControlID, EmptyNodeList, "", common.NavIconTypeFolder, "Add-ons With No Collection", AutoManageAddons, common.NodeTypeEnum.NodeTypeEntry, false, false, OpenNodeList, NodeIDString, ref NodeNavigatorJS, new List<string>());
                            Return_NavigatorJS = Return_NavigatorJS + NodeNavigatorJS;
                            break;
                        case common.NodeIDManageAddonsCollectionPrefix:
                            //
                            // Special Case: clicked on Manage Add-ons.collection
                            // ParentNode(1) is the id of the collection they clicked on
                            // List all add-ons
                            // List all CDef
                            // Add Collection Help
                            // Add Layouts associated with collection
                            //
                            string nodeHtml = "";
                            string cacheName = null;
                            CollectionID = 0;
                            if (parentNodeStack.Count > 0) {
                                CollectionID = cp.Utils.EncodeInteger(parentNodeStack[1]);
                            }
                            cacheName = "addonNav." + common.NodeIDManageAddonsCollectionPrefix + "." + CollectionID + "." + cp.User.Id.ToString();
                            nodeHtml = cp.Cache.GetText(cacheName);
                            if (string.IsNullOrEmpty(nodeHtml)) {
                                //
                                // Help Icon
                                //
                                Name = "Help";
                                NodeIDString = "";
                                NavIconTitleHtmlEncoded = System.Net.WebUtility.HtmlEncode(NavIconTitle);
                                nodeHtml += GetNode(cp, env, 0, 0, CollectionID, 0, 0, "", 0, 0, Name, LegacyMenuControlID, EmptyNodeList, "", common.NavIconTypeHelp, NavIconTitleHtmlEncoded, AutoManageAddons, common.NodeTypeEnum.NodeTypeEntry, false, true, OpenNodeList, NodeIDString, ref NodeNavigatorJS, new List<string>());
                                Return_NavigatorJS = Return_NavigatorJS + NodeNavigatorJS;
                                //
                                // List out add-ons in this collection
                                //
                                NodeIDString = "";
                                FieldList = "*";
                                //FieldList = "Name,id as collectionid,0 as ID,0 as AddonID,0 as NewWindow,0 as ContentID,'' as LinkPage," & NavIconTypeFolder & " as NavIconType,Name as NavIconTitle,0 as SettingPageID,0 as HelpAddonID,0 as HelpCollectionID,0 as contentcontrolid"
                                Criteria = "(collectionid=" + CollectionID + ")";
                                if (!env.isDeveloper) {
                                    Criteria = Criteria + "and(admin<>0)";
                                    //Criteria = Criteria & "and((template<>0)or(page<>0)or(admin<>0))"
                                }
                                CPCSBaseClass cs4 = cp.CSNew();
                                if (cs4.Open("add-ons", Criteria, "name", true, FieldList)) {
                                    do {
                                        Name = Convert.ToString(cs4.GetText("name")).Trim(' ');
                                        linkSuffixList = new List<string>();
                                        if (env.isDeveloper) {
                                            linkSuffixList.Add("<a href=\"" + env.addonEditAddonUrlPrefix + cs4.GetInteger("id") + "\">edit</a>");
                                            if (!cs4.GetBoolean("admin")) {
                                                linkSuffixList.Add("dev");
                                            }
                                        }
                                        addonid = cs4.GetInteger("ID");
                                        NavIconTitleHtmlEncoded = System.Net.WebUtility.HtmlEncode(Name);
                                        ContentControlID = cs4.GetInteger("contentControlId");
                                        switch (cs4.GetInteger("navtypeid")) {
                                            case 2:
                                                NavIconType = common.NavIconTypeReport;
                                                break;
                                            case 3:
                                                NavIconType = common.NavIconTypeSetting;
                                                break;
                                            case 4:
                                                NavIconType = common.NavIconTypeTool;
                                                break;
                                            default:
                                                NavIconType = common.NavIconTypeAddon;
                                                break;
                                        }
                                        nodeHtml += GetNode(cp, env, 0, ContentControlID, 0, 0, 0, "", addonid, 0, Name, LegacyMenuControlID, EmptyNodeList, "", NavIconType, NavIconTitleHtmlEncoded, AutoManageAddons, common.NodeTypeEnum.NodeTypeAddon, false, false, OpenNodeList, NodeIDString, ref NodeNavigatorJS, linkSuffixList);
                                        Return_NavigatorJS = Return_NavigatorJS + NodeNavigatorJS;
                                        cs4.GoNext();
                                    }
                                    while (cs4.OK());
                                }
                                cs4.Close();
                                //
                                // List out cdefs connected to this collection
                                //
                                NodeIDString = "";
                                Criteria = "(collectionid=" + CollectionID + ")";
                                if (env.isDeveloper) {
                                } else if (cp.User.IsAdmin) {
                                    Criteria = Criteria + "and(developeronly=0)";
                                } else {
                                    Criteria = Criteria + "and(developeronly=0)and(adminonly=0)";
                                }
                                int LastContentID = 0;
                                bool DupsFound = false;
                                LastContentID = -1;
                                DupsFound = false;
                                SQL = "select c.id,c.name,c.contentcontrolid,c.developeronly,c.adminonly from ccContent c left join ccAddonCollectionCDefRules r on r.contentid=c.id where " + Criteria + " order by c.name";
                                CPCSBaseClass cs7 = cp.CSNew();
                                if (cs7.OpenSQL(SQL)) {
                                    do {
                                        Name = Convert.ToString(cs7.GetText("name")).Trim(' ');
                                        ContentID = cs7.GetInteger("id");
                                        if (ContentID == LastContentID) {
                                            DupsFound = true;
                                        } else {
                                            linkSuffixList = new List<string>();
                                            if (env.isDeveloper) {
                                                linkSuffixList.Add("<a href=\"" + env.contentFieldEditToolPrefix + ContentID + "\">edit</a>");
                                                if (cs7.GetBoolean("developeronly")) {
                                                    linkSuffixList.Add("dev");
                                                } else if (cs7.GetBoolean("adminonly")) {
                                                    linkSuffixList.Add("adm");
                                                }
                                            }
                                            NavIconTitleHtmlEncoded = System.Net.WebUtility.HtmlEncode(Name);
                                            ContentControlID = cs7.GetInteger("contentControlId");
                                            nodeHtml += GetNode(cp, env, 0, ContentControlID, 0, 0, ContentID, "", 0, 0, Name, LegacyMenuControlID, EmptyNodeList, "", common.NavIconTypeContent, NavIconTitleHtmlEncoded, AutoManageAddons, common.NodeTypeEnum.NodeTypeContent, false, true, OpenNodeList, NodeIDString, ref NodeNavigatorJS, linkSuffixList);
                                            Return_NavigatorJS = Return_NavigatorJS + NodeNavigatorJS;
                                        }
                                        LastContentID = ContentID;
                                        cs7.GoNext();
                                    }
                                    while (cs7.OK());
                                }
                                cs7.Close();
                                //
                                // list all data records associated to this collection
                                //
                                string dataRecordList = "";
                                List<string> dataRecords = new List<string>();
                                string[] dataRecordParts = null;
                                //Dim dataRecordId As Integer
                                //Dim dataRecordGuid As String
                                string dataRecordName = null;
                                string dataRecordCdefName = null;
                                int dataRecordCdefID = 0;
                                string sqlCriteria = "";

                                if (cs7.Open("add-on collections", "id=" + CollectionID)) {
                                    dataRecordList = cs7.GetText("dataRecordList");
                                }
                                cs7.Close();
                                if (!string.IsNullOrEmpty(dataRecordList)) {
                                    dataRecords.AddRange(dataRecordList.Split(Environment.NewLine.ToCharArray()));
                                    foreach (string dataRecord in dataRecords) {
                                        dataRecordParts = dataRecord.Split(",".ToCharArray());
                                        dataRecordCdefName = dataRecordParts[0];
                                        if (!string.IsNullOrEmpty(dataRecordCdefName)) {
                                            dataRecordCdefID = cp.Content.GetID(dataRecordCdefName);
                                            if (dataRecordCdefID != 0) {
                                                sqlCriteria = "";
                                                if (dataRecordParts.Length >= 2) {
                                                    // 
                                                    // contentname,(id or guid)
                                                    //
                                                    if (dataRecordParts[1].Substring(0, 1) == "{") {
                                                        sqlCriteria = "ccguid=" + DbController.encodeSQLText(dataRecordParts[1]);
                                                    } else {
                                                        sqlCriteria = "name=" + DbController.encodeSQLText(dataRecordParts[1]);
                                                    }
                                                }
                                                if (cs7.Open(dataRecordCdefName, sqlCriteria)) {
                                                    do {
                                                        dataRecordName = cs7.GetText("name");


                                                        NavIconTitleHtmlEncoded = System.Net.WebUtility.HtmlEncode("Edit '" + dataRecordName + "' in '" + dataRecordCdefName + "'");
                                                        IconNoSubNodes = common.IconRecord;
                                                        IconNoSubNodes = IconNoSubNodes.Replace("{title}", NavIconTitleHtmlEncoded);
                                                        Link = "?id=" + cs7.GetInteger("id").ToString() + "&cid=" + dataRecordCdefID.ToString() + "&af=4";
                                                        ATag = "<a href=\"" + Link + "\" title=\"" + NavIconTitleHtmlEncoded + "\">";
                                                        nodeHtml += common.cr + "<div class=\"ccNavLink ccNavLinkEmpty\">" + ATag + IconNoSubNodes + "</a>&nbsp;" + ATag + dataRecordCdefName + ":" + dataRecordName + "</a></div>";
                                                        //nodeHtml &= GetNode(cp, env, 0, 0, 0, 0, 0, "/admin?af=4&cid=" & dataRecordCdefID.ToString & "&id=" & cs7.GetInteger("id"), 0, 0, dataRecordCdefName & ":" & cs7.GetText("name"), LegacyMenuControlID, EmptyNodeList, "", NavIconTypeContent, "Data", AutoManageAddons, NodeTypeEnum.NodeTypeContent, False, True, OpenNodeList, NodeIDString, NodeNavigatorJS, "")
                                                        cs7.GoNext();
                                                    }
                                                    while (cs7.OK());

                                                }
                                                cs7.Close();
                                            }
                                        }
                                    }
                                }
                                //
                                if (DupsFound) {
                                    SQL = "select b.id from ccAddonCollectionCDefRules a,ccAddonCollectionCDefRules b where (a.id<b.id) and (a.contentid=b.contentid) and (a.collectionid=b.collectionid)";
                                    SQL = "delete from ccAddonCollectionCDefRules where id in (" + SQL + ")";
                                    cp.Db.ExecuteNonQuery(SQL);
                                }
                                var dependentList = new List<string> {
                                    AddonModel.tableMetadata.tableNameLower,
                                    AddonCollectionModel.tableMetadata.tableNameLower
                                };
                                cp.Cache.Store(cacheName, nodeHtml, dependentList);
                            }
                            s += nodeHtml;
                            break;
                        case common.NodeIDManageAddonsAdvanced:
                            //
                            // Special Case: clicked on Manage Add-ons.advanced
                            //   edit links for Add-ons, Add-on Collections
                            //
                            // Folder to Add-ons without Collections
                            //
                            NodeIDString = common.NodeIDAddonsNoCollection.ToString();
                            s = s + GetNode(cp, env, 0, 0, 0, 0, 0, "", 0, 0, "Add-ons With No Collection", LegacyMenuControlID, EmptyNodeList, "", common.NavIconTypeAddon, "Add-ons With No Collection", AutoManageAddons, common.NodeTypeEnum.NodeTypeEntry, false, false, OpenNodeList, NodeIDString, ref NodeNavigatorJS, new List<string>());
                            Return_NavigatorJS = Return_NavigatorJS + NodeNavigatorJS;
                            //
                            Name = "Add-ons";
                            s = s + GetNode(cp, env, 0, 0, 0, 0, cp.Content.GetID(Name), "", 0, 0, Name, LegacyMenuControlID, EmptyNodeList, "", common.NavIconTypeContent, Name, AutoManageAddons, common.NodeTypeEnum.NodeTypeEntry, false, false, OpenNodeList, "", ref NodeNavigatorJS, new List<string>());
                            Return_NavigatorJS = Return_NavigatorJS + NodeNavigatorJS;
                            //
                            Name = "Add-on Collections";
                            s = s + GetNode(cp, env, 0, 0, 0, 0, cp.Content.GetID(Name), "", 0, 0, Name, LegacyMenuControlID, EmptyNodeList, "", common.NavIconTypeContent, Name, AutoManageAddons, common.NodeTypeEnum.NodeTypeEntry, false, false, OpenNodeList, "", ref NodeNavigatorJS, new List<string>());
                            Return_NavigatorJS = Return_NavigatorJS + NodeNavigatorJS;
                            break;
                        case common.NodeIDAddonsNoCollection:
                            //
                            // special case: Add-on List that do not have collections
                            //
                            CollectionID = 0;
                            FieldList = "0 as ContentControlID,A.Name as Name,A.ID as ID,A.ID as AddonID,0 as NewWindow,0 as ContentID,'' as LinkPage," + common.NavIconTypeAddon + " as NavIconType,A.Name as NavIconTitle,0 as SettingPageID,0 as HelpAddonID,0 as HelpCollectionID,0 as collectionid";
                            SQL = "select"
                            + " " + FieldList + " from ccAggregateFunctions A"
                            + " left join ccAddonCollections C on C.ID=A.CollectionID"
                            + " where C.ID is null"
                            + " order by A.Name";
                            NodeType = common.NodeTypeEnum.NodeTypeAddon;
                            BlockSubNodes = true;
                            NodeIDString = "";
                            CPCSBaseClass cs5 = cp.CSNew();
                            if (cs5.OpenSQL(SQL)) {
                                do {
                                    Name = Convert.ToString(cs5.GetText("name")).Trim(' ');
                                    addonid = cs5.GetInteger("AddonID");
                                    NavIconTitleHtmlEncoded = System.Net.WebUtility.HtmlEncode(Name);
                                    ContentControlID = cs5.GetInteger("contentControlId");
                                    s = s + GetNode(cp, env, 0, ContentControlID, 0, 0, 0, "", addonid, 0, Name, LegacyMenuControlID, EmptyNodeList, "", common.NavIconTypeAddon, NavIconTitleHtmlEncoded, AutoManageAddons, common.NodeTypeEnum.NodeTypeAddon, false, false, OpenNodeList, NodeIDString, ref NodeNavigatorJS, new List<string>());
                                    Return_NavigatorJS = Return_NavigatorJS + NodeNavigatorJS;
                                    cs5.GoNext();
                                }
                                while (cs5.OK());
                            }
                            cs5.Close();
                            break;
                        case common.NodeIDLegacyMenu:
                            //
                            // Special Case: build old top menus under this Navigator entry
                            //
                            BlockSubNodes = false;
                            SQL = GetMenuSQL(cp, "(parentid=0)or(parentid is null)", common.LegacyMenuContentName);
                            if (!csChildList.OpenSQL(SQL)) {
                                //
                                // Empty list, add to EmptyNodeList
                                //
                                EmptyNodeList.Add(TopParentNode);
                            }
                            break;
                        case common.NodeIDAllContentList:
                            //
                            // special case: all content
                            //
                            FieldList = "Name,ID,0 as AddonID,0 as NewWindow,ID as ContentID,'' as LinkPage," + common.NavIconTypeContent + " as NavIconType,Name as NavIconTitle,0 as SettingPageID,0 as HelpAddonID,0 as HelpCollectionID,0 as contentcontrolid,0 as collectionid";
                            SQL = "select " + FieldList + " from cccontent order by name";
                            csChildList.OpenSQL(SQL);
                            NodeType = common.NodeTypeEnum.NodeTypeContent;
                            BlockSubNodes = true;
                            break;
                        case "":
                            //
                            // Navigator Entries, list home(s) plus all roots
                            //
                            NodeType = common.NodeTypeEnum.NodeTypeEntry;
                            BlockSubNodes = false;
                            Link = System.Net.WebUtility.HtmlEncode("http://" + cp.Site.Domain);
                            s = s + "<div class=ccNavLink><A href=\"" + Link + "\">" + common.IconPublicHome + "</A>&nbsp;<A href=\"" + Link + "\">Public Home</A></div>";
                            Link = System.Net.WebUtility.HtmlEncode(cp.Site.GetText("adminUrl"));
                            s = s + "<div class=ccNavLink><A href=\"" + Link + "\">" + common.IconAdminHome + "</A>&nbsp;<A href=\"" + Link + "\">Admin Home</A></div>";
                            CPCSBaseClass cs8 = cp.CSNew();
                            if (cs8.OpenSQL(GetMenuSQL(cp, "((Parentid=0)or(Parentid is null))", common.NavigatorContentName))) {
                                do {
                                    Name = Convert.ToString(cs8.GetText("name")).Trim(' ');
                                    NavigatorID = cs8.GetInteger("ID").ToString();
                                    NavIconTitleHtmlEncoded = System.Net.WebUtility.HtmlEncode(Name);
                                    NodeIDString = NavigatorID.ToString();
                                    if (AutoManageAddons) {
                                        //
                                        // special cases - root nodes that do not just deliver menu entries
                                        //
                                        switch (Name.ToLower(CultureInfo.InvariantCulture)) {
                                            case "manage add-ons":
                                                NodeIDString = common.NodeIDManageAddons.ToString();
                                                break;
                                            case "settings":
                                                NodeIDString = common.NodeIDSettings.ToString();
                                                break;
                                            case "tools":
                                                NodeIDString = common.NodeIDTools.ToString();
                                                break;
                                            case "reports":
                                                NodeIDString = common.NodeIDReports.ToString();
                                                break;
                                        }
                                    }
                                    s = s + GetNode(cp, env, 0, 0, 0, 0, 0, "", 0, 0, Name, LegacyMenuControlID, EmptyNodeList, NavigatorID, common.NavIconTypeFolder, NavIconTitleHtmlEncoded, AutoManageAddons, common.NodeTypeEnum.NodeTypeEntry, false, false, OpenNodeList, NodeIDString, ref NodeNavigatorJS, new List<string>());
                                    Return_NavigatorJS = Return_NavigatorJS + NodeNavigatorJS;
                                    cs8.GoNext();
                                }
                                while (cs8.OK());
                            }
                            cs8.Close();
                            //
                            // Add a Legacy Menu node to the top-most parent menu at the very end
                            //
                            if (cp.Utils.EncodeBoolean(cp.Site.GetText("AllowNavigatorLegacyEntry", "0"))) {
                                Name = "Legacy Menu";
                                NavIconTitleHtmlEncoded = "Legacy Menu";
                                NodeIDString = common.NodeIDLegacyMenu.ToString();
                                s = s + GetNode(cp, env, 0, 0, 0, 0, 0, "", 0, 0, Name, LegacyMenuControlID, EmptyNodeList, "", common.NavIconTypeFolder, NavIconTitleHtmlEncoded, AutoManageAddons, common.NodeTypeEnum.NodeTypeEntry, false, BlockSubNodes, OpenNodeList, NodeIDString, ref NodeNavigatorJS, new List<string>());
                                Return_NavigatorJS = Return_NavigatorJS + NodeNavigatorJS;
                            }
                            break;
                        case common.NodeIDSettings:
                            //
                            // list setting nodes, includes menu nodes with setting parents, and addons with type=setting sorted in
                            //
                            s = s + getNodeListMixed(cp, env, EmptyNodeList, TopParentNode, LegacyMenuControlID, AutoManageAddons, NodeType, OpenNodeList, 3, cp.Content.GetRecordID("navigator entries", "settings"), common.NavIconTypeSetting, ref NodeNavigatorJS);
                            Return_NavigatorJS = Return_NavigatorJS + NodeNavigatorJS;
                            break;
                        case common.NodeIDTools:
                            //
                            // list setting nodes, includes menu nodes with setting parents, and addons with type=setting sorted in
                            s = s + getNodeListMixed(cp, env, EmptyNodeList, TopParentNode, LegacyMenuControlID, AutoManageAddons, NodeType, OpenNodeList, 4, cp.Content.GetRecordID("navigator entries", "tools"), common.NavIconTypeTool, ref NodeNavigatorJS);
                            Return_NavigatorJS = Return_NavigatorJS + NodeNavigatorJS;
                            break;
                        case common.NodeIDReports:
                            //
                            // list setting nodes, includes menu nodes with setting parents, and addons with type=setting sorted in
                            s = s + getNodeListMixed(cp, env, EmptyNodeList, TopParentNode, LegacyMenuControlID, AutoManageAddons, NodeType, OpenNodeList, 2, cp.Content.GetRecordID("navigator entries", "reports"), common.NavIconTypeReport, ref NodeNavigatorJS);
                            Return_NavigatorJS = Return_NavigatorJS + NodeNavigatorJS;
                            break;
                        default:
                            //
                            // numeric node (default case) - list navigator records with parent=TopParentNode
                            if (TopParentNode.IsNumeric()) {
                                if (!EmptyNodeList.Contains(TopParentNode)) {
                                    //
                                    // Navigator Entries, child under TopParentNode
                                    SQL = GetMenuSQL(cp, "parentid=" + TopParentNode, "");
                                    BlockSubNodes = false;
                                    if (!csChildList.OpenSQL(SQL)) {
                                        //
                                        // Empty list, add to EmptyNodeList
                                        EmptyNodeList.Add(TopParentNode);
                                    }
                                }
                            }
                            break;
                    }
                    //
                    // ----- List Navigator Nodes, if not already displayed
                    //
                    if ((!csChildList.OK()) && (NodeType == common.NodeTypeEnum.NodeTypeEntry)) {
                        //
                        // No child nodes, if this node includes a CID, list the first 20 content records with a 'more'
                        //
                        ContentID = 0;
                        if (TopParentNode.IsNumeric()) {
                            csChildList.Close();
                            if (csChildList.Open(common.NavigatorContentName, "id=" + TopParentNode)) {
                                ContentID = csChildList.GetInteger("ContentID");
                            }
                            if (ContentID != 0) {
                                string ContentName = cp.Content.GetRecordName("content", ContentID);
                                if (!string.IsNullOrEmpty(ContentName)) {
                                    //ContentTableName =cp.Content.GetTable(ContentName)
                                    csChildList.Close();
                                    int Ptr = 0;
                                    if (csChildList.Open(ContentName, "", "name", true, "ID,Name,ContentControlID", 20, 1)) {
                                        if (EmptyNodeList.Contains(TopParentNode)) {
                                            EmptyNodeList.Remove(TopParentNode);
                                        }
                                        do {
                                            NavigatorID = csChildList.GetInteger("ID").ToString();
                                            string RecordName = csChildList.GetText("Name");
                                            if (string.IsNullOrEmpty(RecordName)) {
                                                RecordName = "Record " + NavigatorID;
                                            }
                                            //
                                            if (RecordName.Length > 53) {
                                                RecordName = RecordName.Substring(0, 25) + "..." + RecordName.Substring(RecordName.Length - 25);
                                            }
                                            NavIconTitleHtmlEncoded = System.Net.WebUtility.HtmlEncode("Edit '" + RecordName + "' in '" + ContentName + "'");
                                            IconNoSubNodes = common.IconRecord;
                                            IconNoSubNodes = IconNoSubNodes.Replace("{title}", NavIconTitleHtmlEncoded);
                                            Link = "?id=" + NavigatorID + "&cid=" + csChildList.GetInteger("contentControlId") + "&af=4";
                                            ATag = "<a href=\"" + Link + "\" title=\"" + NavIconTitleHtmlEncoded + "\">";
                                            s = s + "<div class=\"ccNavLink ccNavLinkEmpty\">" + ATag + IconNoSubNodes + "</a>&nbsp;" + ATag + RecordName + "</a></div>";
                                            Ptr = Ptr + 1;
                                            csChildList.GoNext();
                                        }
                                        while (csChildList.OK() && Ptr < 20);
                                        if (Ptr == 20) {
                                            NavIconTitleHtmlEncoded = System.Net.WebUtility.HtmlEncode("Open All '" + common.NavigatorContentName + "'");
                                            Link = "?cid=" + ContentID;
                                            string IconClosed = null;
                                            s = s + "<div class=\"ccNavLink ccNavLinkEmpty\">" + IconClosed + "&nbsp;<a href=\"" + Link + "\" title=\"" + NavIconTitleHtmlEncoded + "\">more...</a></div>";
                                        }
                                    }
                                }
                            }
                        }
                    } else if (csChildList.OK()) {
                        //
                        // List out child menus
                        //
                        do {
                            CollectionID = csChildList.GetInteger("CollectionID");
                            NavigatorID = csChildList.GetInteger("ID").ToString();
                            Name = Convert.ToString(csChildList.GetText("name")).Trim(' ');
                            bool NewWindow = csChildList.GetBoolean("newwindow");
                            ContentID = csChildList.GetInteger("ContentID");
                            Link = Convert.ToString(csChildList.GetText("LinkPage")).Trim(' ');
                            addonid = csChildList.GetInteger("AddonID");
                            NavIconType = csChildList.GetInteger("NavIconType");
                            NavIconTitle = csChildList.GetText("NavIconTitle");
                            int HelpAddonID = csChildList.GetInteger("HelpAddonID");
                            if (HelpAddonID != 0) {
                                //HelpAddonID = HelpAddonID;
                            }
                            int helpCollectionID = csChildList.GetInteger("HelpCollectionID");
                            if (string.IsNullOrEmpty(NavIconTitle)) {
                                NavIconTitle = Name;
                            }
                            ContentControlID = csChildList.GetInteger("contentControlId");
                            if (Name.ToLower(CultureInfo.InvariantCulture) == "all content") {
                                //
                                // special case: any Navigator Entry named 'all content' returns the content list
                                //
                                NodeIDString = common.NodeIDAllContentList;
                            } else {
                                NodeIDString = NavigatorID.ToString();
                            }
                            linkSuffixList = new List<string>();
                            if ((ContentID != 0) && (env.isDeveloper)) {
                                linkSuffixList.Add("<a href=\"" + env.contentFieldEditToolPrefix + ContentID + "\">edit</a>");
                            }
                            NavIconTitleHtmlEncoded = System.Net.WebUtility.HtmlEncode(NavIconTitle);
                            int SettingPageID = 0;
                            s = s + GetNode(cp, env, CollectionID, ContentControlID, helpCollectionID, HelpAddonID, ContentID, Link, addonid, SettingPageID, Name, LegacyMenuControlID, EmptyNodeList, NavigatorID, NavIconType, NavIconTitleHtmlEncoded, AutoManageAddons, NodeType, NewWindow, BlockSubNodes, OpenNodeList, NodeIDString, ref NodeNavigatorJS, linkSuffixList);
                            Return_NavigatorJS = Return_NavigatorJS + NodeNavigatorJS;
                            csChildList.GoNext();
                        }
                        while (csChildList.OK());
                        csChildList.Close();
                    }
                    //
                    //
                    //
                    cp.Cache.Store(BakeName, EmptyNodeList, NavigatorEntryModel.tableMetadata.tableNameLower);
                    returnNav = s;
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return returnNav;
        }
        //
        //====================================================================================================
        //
        internal string getNodeListMixed(CPBaseClass cp, NavigatorEnvironment env, List<string> EmptyNodeList, string TopParentNode, int LegacyMenuControlID, bool AutoManageAddons, common.NodeTypeEnum NodeType, List<string> OpenNodeList, int AddonNavTypeID, int MenuParentNodeID, int AdminNavIconTypeSetting, ref string Return_DraggableJS) {
            string returnNav = "";
            try {
                string NodeDraggableJS = null;
                var SortNodes = new List<common.SortNodeType>();
                int SortPtr = 0;
                string NodeIDString = null;
                string SQL = null;
                string Criteria = null;
                bool BlockSubNodes = false;
                //
                // list mixed nodes (settings/reports/tools), includes menu nodes and addons with type='setting' sorted in
                //
                if (EmptyNodeList.Contains(TopParentNode)) {
                    //EmptyNodeList = EmptyNodeList;
                } else {
                    //
                    // Add addons to node list
                    //
                    NodeIDString = "";
                    Criteria = "(navtypeid=" + AddonNavTypeID + ")";
                    if ((AddonNavTypeID == 2) || (AddonNavTypeID == 3) || (AddonNavTypeID == 4)) {
                        //
                        // if setting, report or tool, "admin" is not needed
                        //
                    } else {
                        //
                        // for Manage Addons node, admin is needed for non-developer
                        //
                        if (!cp.User.IsDeveloper) {
                            Criteria = Criteria + "and(admin<>0)";
                        }
                    }
                    CPCSBaseClass cs10 = cp.CSNew();
                    if (cs10.Open("add-ons", Criteria, "name")) {
                        do {
                            SortNodes.Add(new common.SortNodeType() {
                                Name = Convert.ToString(cs10.GetText("name")).Trim(' '),
                                addonid = cs10.GetInteger("ID"),
                                navIconTitle = SortNodes[SortPtr].Name,
                                contentControlId = cs10.GetInteger("contentControlId"),
                                navIconType = AdminNavIconTypeSetting
                            });
                            cs10.GoNext();
                        }
                        while (cs10.OK());
                    }
                    cs10.Close();
                    //
                    SQL = GetMenuSQL(cp, "parentid=" + MenuParentNodeID, "");
                    CPCSBaseClass cs11 = cp.CSNew();
                    if (cs11.OpenSQL(SQL)) {
                        do {
                            string name = Convert.ToString(cs11.GetText("name")).Trim(' ');
                            string title = cs11.GetText("NavIconTitle");
                            if (string.IsNullOrWhiteSpace(title)) {
                                title = name;
                            }

                            if (name.ToLower(CultureInfo.InvariantCulture) == "all content") {
                                //
                                // special case: any Navigator Entry named 'all content' returns the content list
                                //
                                NodeIDString = common.NodeIDAllContentList;
                            }
                            SortNodes.Add(new common.SortNodeType() {
                                Name = name,
                                navigatorId = cs11.GetInteger("ID").ToString(),
                                collectionId = cs11.GetInteger("CollectionID"),
                                newWindow = cs11.GetBoolean("newwindow"),
                                contentId = cs11.GetInteger("ContentID"),
                                link = Convert.ToString(cs11.GetText("LinkPage")).Trim(' '),
                                addonid = cs11.GetInteger("AddonID"),
                                navIconType = cs11.GetInteger("NavIconType"),
                                navIconTitle = title,
                                helpAddonId = cs11.GetInteger("HelpAddonID"),
                                helpCollectionId = cs11.GetInteger("HelpCollectionID"),
                                contentControlId = cs11.GetInteger("contentControlId"),
                                nodeIdString = NodeIDString
                            });
                            SortPtr = SortPtr + 1;
                            cs11.GoNext();
                        }
                        while (cs11.OK());
                    }
                    cs11.Close();
                    if (SortNodes.Count == 0) {
                        //
                        // Empty list, add to EmptyNodeList
                        //
                        EmptyNodeList.Add(TopParentNode);
                    } else {
                        foreach (var sortNode in SortNodes) {
                            returnNav = returnNav + GetNode(cp, env, sortNode.collectionId, sortNode.contentControlId, sortNode.helpCollectionId, sortNode.helpAddonId, sortNode.contentId, sortNode.link, sortNode.addonid, 0, sortNode.Name, LegacyMenuControlID, EmptyNodeList, sortNode.navigatorId, sortNode.navIconType, System.Net.WebUtility.HtmlEncode(sortNode.navIconTitle), AutoManageAddons, NodeType, sortNode.newWindow, BlockSubNodes, OpenNodeList, sortNode.nodeIdString, ref NodeDraggableJS, new List<string>());
                            Return_DraggableJS = Return_DraggableJS + NodeDraggableJS;

                        }
                    }
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return returnNav;
        }
        //
        //====================================================================================================
        //
        private string GetNode(CPBaseClass cp, NavigatorEnvironment env, int CollectionID, int ContentControlID, int helpCollectionID, int HelpAddonID, int ContentID, string Link, int addonid, int ignore, string Name, int LegacyMenuControlID, List<string> EmptyNodeList, string NavigatorID, int NavIconType, string NavIconTitleHtmlEncoded, bool AutoManageAddons, common.NodeTypeEnum NodeType, bool NewWindow, bool BlockSubNodes, List<string> OpenNodeList, string nodeId, ref string Return_NavigatorJS, List<string> linkSuffixList) {
            string result = "";
            try {
                bool BlockNode= false;
                Return_NavigatorJS = "";
                string WorkingName = Name;
                bool IsVisible = (CollectionID != 0) | (ContentControlID == LegacyMenuControlID) | (helpCollectionID != 0) | (HelpAddonID != 0) | (ContentID != 0) | (!string.IsNullOrEmpty(Link)) | (addonid != 0) | (WorkingName.ToLower(CultureInfo.InvariantCulture) == "all content") || (WorkingName.ToLower(CultureInfo.InvariantCulture) == "add-ons with no collection");
                if (!IsVisible) {
                    //
                    // IsVisible if it is not in the EmptyNodeList (has child entries)
                    IsVisible = EmptyNodeList.Contains(NavigatorID);
                }
                if (IsVisible) {
                    //
                    // hide the legacy node 'switch to navigator'
                    //
                    IsVisible = WorkingName.ToLower(CultureInfo.InvariantCulture) != "switch to navigator";
                }
                if (IsVisible) {
                    string IconNoSubNodes = null;
                    string IconOpened = null;
                    string IconClosed = null;
                    //
                    // Setup Icons
                    //
                    switch (NavIconType) {
                        case common.NavIconTypeCustom:
                            //
                            // reserved for future addition of a custom Icon field
                            // not done now because there is no facility now to import files during collection build
                            //
                            break;
                        case common.NavIconTypeAdvanced:
                            IconOpened = common.IconAdvancedOpened;
                            IconClosed = common.IconAdvancedClosed;
                            IconNoSubNodes = common.IconAdvanced;
                            break;
                        case common.NavIconTypeContent:
                            IconOpened = common.IconContentOpened;
                            IconClosed = common.IconContentClosed;
                            IconNoSubNodes = common.IconContent;
                            break;
                        case common.NavIconTypeEmail:
                            IconOpened = common.IconEmailOpened;
                            IconClosed = common.IconEmailClosed;
                            IconNoSubNodes = common.IconEmail;
                            break;
                        case common.NavIconTypeUser:
                            IconOpened = common.IconUsersOpened;
                            IconClosed = common.IconUsersClosed;
                            IconNoSubNodes = common.IconUsers;
                            break;
                        case common.NavIconTypeReport:
                            IconOpened = common.IconReportsOpened;
                            IconClosed = common.IconReportsClosed;
                            IconNoSubNodes = common.IconReports;
                            break;
                        case common.NavIconTypeSetting:
                            IconOpened = common.IconSettingsOpened;
                            IconClosed = common.IconSettingsClosed;
                            IconNoSubNodes = common.IconSettings;
                            break;
                        case common.NavIconTypeTool:
                            IconOpened = common.IconToolsOpened;
                            IconClosed = common.IconToolsClosed;
                            IconNoSubNodes = common.IconTools;
                            break;
                        case common.NavIconTypeRecord:
                            IconOpened = common.IconRecordOpened;
                            IconClosed = common.IconRecordClosed;
                            IconNoSubNodes = common.IconRecord;
                            break;
                        case common.NavIconTypeAddon:
                            IconOpened = common.IconAddonsOpened;
                            IconClosed = common.IconAddonsClosed;
                            IconNoSubNodes = common.IconAddons;
                            break;
                        case common.NavIconTypeHelp:
                            IconOpened = common.IconHelp;
                            IconClosed = common.IconHelp;
                            IconNoSubNodes = common.IconHelp;
                            break;
                        default: //NavIconTypeFolder
                            IconOpened = common.IconFolderOpened;
                            IconClosed = common.IconFolderClosed;
                            IconNoSubNodes = common.IconFolderNoSubNodes;
                            break;
                    }
                    IconOpened = IconOpened.Replace("{title}", "Close " + NavIconTitleHtmlEncoded);
                    IconClosed = IconClosed.Replace("{title}", "Open " + NavIconTitleHtmlEncoded);
                    IconNoSubNodes = IconNoSubNodes.Replace("{title}", NavIconTitleHtmlEncoded);
                    string DivIDBase = null;
                    //
                    // NodeIDString - the unique string that is passed by here as ParentNode to get all the child nodes
                    //   is always the navigator entry ID, unless it is a hardcoded subsection
                    // DIVID must be unique for this entire menu, but does not need to be recognized in a call back to the server
                    //
                    //
                    // set flag for 'hardcoded' lists - like add-ons
                    //
                    if (AutoManageAddons && (WorkingName.ToLower(CultureInfo.InvariantCulture) == "manage add-ons")) {
                        //
                        // test special case - replace manage add-ons branch
                        //
                        DivIDBase = NavigatorID.ToString();
                        //NodeIDString = NodeIDManageAddons
                    } else {
                        switch (WorkingName.ToLower(CultureInfo.InvariantCulture)) {
                            case "legacy menu":
                                //
                                // special case - if node has this name, a click to it calls back with NodeIDLegacyMenu
                                //

                                DivIDBase = NavigatorID.ToString();
                                //NodeIDString = NodeIDLegacyMenu
                                break;
                            case "add-ons with no collection":
                                //
                                // any Navigator Entry named 'all content' returns the content list
                                //
                                DivIDBase = NavigatorID.ToString();
                                //NodeIDString = NodeIDAddonsNoCollection
                                break;
                            case "all content":
                                //
                                // any Navigator Entry named 'all content' returns the content list
                                //
                                DivIDBase = NavigatorID.ToString();
                                //NodeIDString = NodeIDAllContentList
                                break;
                            default:
                                //
                                // This entry is made from a navigator entry record
                                //
                                switch (NodeType) {
                                    case common.NodeTypeEnum.NodeTypeAddon:
                                        //
                                        // List of Addons
                                        //
                                        //NodeIDString = ""
                                        DivIDBase = "a" + NavigatorID;
                                        break;
                                    case common.NodeTypeEnum.NodeTypeCollection:
                                        //
                                        // List of Collections
                                        //
                                        //NodeIDString = "collection." & CollectionID
                                        DivIDBase = "c" + CollectionID;
                                        break;
                                    case common.NodeTypeEnum.NodeTypeContent:
                                        //
                                        // List of content
                                        //
                                        //NodeIDString = ""
                                        DivIDBase = "d" + NavigatorID;
                                        break;
                                    default:
                                        //
                                        // List of Navigator Entries
                                        //
                                        //NodeIDString = CStr(NavigatorID)
                                        DivIDBase = "n" + NavigatorID;
                                        break;
                                }
                                break;
                        }
                    }
                    //
                    // check name for length
                    //
                    if (WorkingName.Length > 53) {
                        WorkingName = WorkingName.Substring(0, 25) + "..." + WorkingName.Substring(WorkingName.Length - 25);
                    }
                    string NavLinkHTMLId = null;
                    string workingNameHtmlEncoded = null;
                    //
                    // setup link
                    //
                    if (!string.IsNullOrEmpty(Link)) {
                        NavLinkHTMLId = "n" + NavigatorID;
                        workingNameHtmlEncoded = System.Net.WebUtility.HtmlEncode(WorkingName);
                        if (NewWindow) {
                            workingNameHtmlEncoded = "<a name=\"navLink\" id=\"" + NavLinkHTMLId + "\" href=\"" + Link + "\" target=\"_blank\" title=\"Open '" + workingNameHtmlEncoded + "'\">" + workingNameHtmlEncoded + "</a>";
                        } else {
                            workingNameHtmlEncoded = "<a name=\"navLink\" id=\"" + NavLinkHTMLId + "\" href=\"" + Link + "\" title=\"Open '" + workingNameHtmlEncoded + "'\">" + workingNameHtmlEncoded + "</a>";
                        }
                    } else {
                        //
                        // If link page, use this
                        //
                        if (addonid != 0) {
                            //
                            // link to addon
                            //
                            string addonLink = "";
                            CPCSBaseClass cs12 = cp.CSNew();
                            string AddonGuid = null;
                            if (cs12.Open("Add-ons", "id=" + addonid, "", true, "remotemethod,name,ccguid")) {
                                AddonGuid = cs12.GetText("ccguid");
                                string AddonName = cs12.GetText("name");
                                if (cs12.GetBoolean("remotemethod")) {
                                    NewWindow = true;
                                    addonLink = cp.Site.GetText("adminUrl") + "?" + common.RequestNameRemoteMethodAddon + "=" + cp.Utils.EncodeRequestVariable(AddonName);
                                }

                            }
                            cs12.Close();
                            if (string.IsNullOrEmpty(addonLink)) {
                                if (!string.IsNullOrEmpty(AddonGuid)) {
                                    addonLink = cp.Utils.ModifyLinkQueryString(cp.Site.GetText("adminUrl"), "addonguid", AddonGuid, true);
                                } else {
                                    addonLink = cp.Utils.ModifyLinkQueryString(cp.Site.GetText("adminUrl"), "addonid", addonid.ToString(), true);
                                }
                            }
                            NavLinkHTMLId = "a" + addonid;
                            workingNameHtmlEncoded = System.Net.WebUtility.HtmlEncode(WorkingName);
                            if (NewWindow) {
                                workingNameHtmlEncoded = "<a name=\"navLink\" id=\"" + NavLinkHTMLId + "\" href=\"" + addonLink + "\" target=\"_blank\" title=\"Run '" + workingNameHtmlEncoded + "'\">" + workingNameHtmlEncoded + "</a>";
                            } else {
                                workingNameHtmlEncoded = "<a name=\"navLink\" id=\"" + NavLinkHTMLId + "\" href=\"" + addonLink + "\" title=\"Run '" + workingNameHtmlEncoded + "'\">" + workingNameHtmlEncoded + "</a>";
                            }
                        } else if (ContentID != 0) {
                            //
                            // go edit the content
                            //
                            string contentLink = cp.Utils.ModifyLinkQueryString(cp.Site.GetText("adminUrl"), "cid", ContentID.ToString(), true);
                            NavLinkHTMLId = "c" + ContentID;
                            workingNameHtmlEncoded = System.Net.WebUtility.HtmlEncode(WorkingName);
                            if (NewWindow) {
                                workingNameHtmlEncoded = "<a name=\"navLink\" id=\"" + NavLinkHTMLId + "\" href=\"" + contentLink + "\" target=\"_blank\" title=\"List All '" + NavIconTitleHtmlEncoded + "'\">" + NavIconTitleHtmlEncoded + "</a>";
                            } else {
                                workingNameHtmlEncoded = "<a name=\"navLink\" id=\"" + NavLinkHTMLId + "\" href=\"" + contentLink + "\" title=\"List All '" + NavIconTitleHtmlEncoded + "'\">" + NavIconTitleHtmlEncoded + "</a>";
                            }
                        } else if (HelpAddonID != 0) {
                            //
                            // go to Addon Help
                            //
                            string helpLink = cp.Utils.ModifyLinkQueryString(cp.Site.GetText("adminUrl"), "helpaddonid", HelpAddonID.ToString(), true);
                            workingNameHtmlEncoded = System.Net.WebUtility.HtmlEncode(WorkingName);
                            if (NewWindow) {
                                workingNameHtmlEncoded = "<a href=\"" + helpLink + "\" target=\"_blank\" title=\"Help for Add-on '" + NavIconTitleHtmlEncoded + "'\">" + NavIconTitleHtmlEncoded + "</a>";
                            } else {
                                workingNameHtmlEncoded = "<a href=\"" + helpLink + "\" title=\"Help for Add-on '" + NavIconTitleHtmlEncoded + "'\">" + NavIconTitleHtmlEncoded + "</a>";
                            }
                        } else if (helpCollectionID != 0) {
                            //
                            // go to Collection Help
                            //
                            CPCSBaseClass cs13 = cp.CSNew();
                            if (!cs13.Open("add-on collections", "id=" + helpCollectionID, "name", true, "name,helpLink,help")) {
                                BlockNode = true;
                            } else {
                                //
                                string collectionName = cs13.GetText("name");
                                string collectionHelpLink = cs13.GetText("helpLink");
                                string collectionHelp = cs13.GetText("help");
                                //
                                WorkingName = collectionName;
                                workingNameHtmlEncoded = System.Net.WebUtility.HtmlEncode(WorkingName);
                                if (!string.IsNullOrEmpty(collectionHelpLink)) {
                                    NewWindow = true;
                                } else if (!string.IsNullOrEmpty(collectionHelp)) {
                                    collectionHelpLink = cp.Utils.ModifyLinkQueryString(cp.Site.GetText("adminUrl"), "helpcollectionid", helpCollectionID.ToString(), true);
                                } else {
                                    BlockNode = true;
                                }
                                if (!BlockNode) {
                                    if (NewWindow) {
                                        workingNameHtmlEncoded = "<a href=\"" + collectionHelpLink + "\" target=\"_blank\" title=\"Help for Collection '" + workingNameHtmlEncoded + "'\">Help</a>";
                                    } else {
                                        workingNameHtmlEncoded = "<a href=\"" + collectionHelpLink + "\" title=\"Help for Collection '" + workingNameHtmlEncoded + "'\">Help</a>";
                                    }
                                }

                            }
                            cs13.Close();
                        } else {
                            workingNameHtmlEncoded =  System.Net.WebUtility.HtmlEncode(WorkingName);
                        }
                    }
                    //
                    if (!BlockNode) {
                        if (BlockSubNodes || (addonid != 0)) {
                            //
                            // This is a hardcoded item (like Add-on), it has no subnodes
                            result = result + "<div class=\"ccNavLink ccNavLinkEmpty\">" + IconNoSubNodes + workingNameHtmlEncoded + linkSuffixList + "</div>";
                        } else {
                            //
                            string DivIDClosed = DivIDBase + "a";
                            string DivIDOpened = DivIDBase + "b";
                            string DivIDContent = DivIDBase + "c";
                            string DivIDEmpty = DivIDBase + "d";

                            if ((ContentID == 0) && (EmptyNodeList.Contains(nodeId))) {
                                //
                                // In EmptyNodeList
                                //
                                result = result + "<div class=\"ccNavLink ccNavLinkEmpty\">" + IconNoSubNodes + workingNameHtmlEncoded + linkSuffixList + "</div>";
                            } else if (OpenNodeList.Contains(nodeId)) {
                                string NodeNavigatorJS = null;
                                //
                                // This node is open
                                //
                                string SubNav = GetNodeList(cp, env, nodeId, OpenNodeList, ref NodeNavigatorJS);
                                Return_NavigatorJS = Return_NavigatorJS + NodeNavigatorJS;
                                if (!string.IsNullOrEmpty(SubNav)) {
                                    //
                                    // display the subnav
                                    //
                                    result = result + "<div class=ccNavLink ID=" + DivIDClosed + " style=\"display:none;\"><A class=\"ccNavClosed\" href=\"#\" onclick=\"AdminNavOpenClick('" + DivIDClosed + "','" + DivIDOpened + "','" + DivIDContent + "','" + nodeId + "','" + DivIDEmpty + "');return false;\">" + IconClosed + "</A>&nbsp;" + workingNameHtmlEncoded + linkSuffixList + "</div>";
                                    result = result + "<div class=ccNavLink ID=" + DivIDOpened + "><A class=\"ccNavOpened\" href=\"#\" onclick=\"AdminNavCloseClick('" + DivIDOpened + "','" + DivIDClosed + "','" + DivIDContent + "','" + nodeId + "');return false;\">" + IconOpened + "</A>&nbsp;" + workingNameHtmlEncoded + linkSuffixList + "</div>";
                                    result = result + "<div class=ccNavLinkChild ID=" + DivIDContent + ">"
                                        + GenericController.nop(SubNav) + "</div>";
                                } else {
                                    //
                                    // it has a NO subnav
                                    //
                                    result = result + "<div class=\"ccNavLink ccNavLinkEmpty\">" + IconNoSubNodes + workingNameHtmlEncoded + linkSuffixList + "</div>";
                                }
                            } else {
                                //
                                // This node is closed
                                //
                                result = result + "<div class=ccNavLink ID=" + DivIDClosed + " ><A class=\"ccNavClosed\" href=\"#\" onclick=\"AdminNavOpenClick('" + DivIDClosed + "','" + DivIDOpened + "','" + DivIDContent + "','" + nodeId + "','','" + DivIDContent + "');return false;\">" + IconClosed + "</A>&nbsp;" + workingNameHtmlEncoded + linkSuffixList + "</div>";
                                result = result + "<div class=ccNavLink ID=" + DivIDOpened + " style=\"display:none;\"><A class=\"ccNavOpened\" href=\"#\" onclick=\"AdminNavCloseClick('" + DivIDOpened + "','" + DivIDClosed + "','" + DivIDContent + "','" + nodeId + "');return false;\">" + IconOpened + "</A>&nbsp;" + workingNameHtmlEncoded + linkSuffixList + "</div>";
                                result = result + "<div class=\"ccNavLink ccNavLinkEmpty\" ID=" + DivIDEmpty + " style=\"display:none;\">" + IconNoSubNodes + workingNameHtmlEncoded + linkSuffixList + "</div>";
                                result = result + "<div class=ccNavLinkChild ID=" + DivIDContent + " style=\"display:none;margin-left:20px;\">&nbsp;&nbsp;&nbsp;&nbsp;<img src=\"/cclib/images/ajax-loader-small.gif\" width=\"16\" height=\"16\"></div>";
                            }
                        }
                        if (!string.IsNullOrEmpty(NavLinkHTMLId)) {
                            Return_NavigatorJS = Return_NavigatorJS + "$(function(){"
                                + "$('#" + NavLinkHTMLId + "').draggable({"
                                    + "opacity: 0.50"
                                    + ",helper: 'clone'"
                                    + ",revert: 'invalid'"
                                    + ",stop: function(event, ui){"
                                        + "navDrop('" + NavLinkHTMLId + "',ui.position.left,ui.position.top);"
                                    + "}"
                                    + ",cursor: 'move'"
                                + "});"
                            + "});";
                        }
                    }
                }
                //
                //
                // ----- Error Trap
                //
            } catch {
                cp.Site.ErrorReport("Trap");
            }
            return result;
        }
        //
        //====================================================================================================
        //
        private string GetMenuSQL(CPBaseClass cp, string ParentCriteria, string MenuContentName) {
            try {
                string Criteria = "(Active<>0)";
                if (!string.IsNullOrEmpty(MenuContentName)) { Criteria = Criteria + "AND" + cp.Content.GetContentControlCriteria(MenuContentName); }
                if (cp.User.IsDeveloper) {
                    //
                    // ----- Developer
                } else if (cp.User.IsAdmin) {
                    //
                    // ----- Administrator
                    Criteria = Criteria + "AND((DeveloperOnly is null)or(DeveloperOnly=0))"
                        + "AND(ID in ("
                        + " SELECT AllowedEntries.ID"
                        + " FROM CCMenuEntries AllowedEntries LEFT JOIN ccContent ON AllowedEntries.ContentID = ccContent.ID"
                        + " Where ((ccContent.Active<>0)And((ccContent.DeveloperOnly is null)or(ccContent.DeveloperOnly=0)))"
                            + "OR(ccContent.ID Is Null)"
                        + "))";
                } else {
                    //
                    // ----- Content Manager
                    string ContentManagerIdCommaList = GetContentManagerIdCommaList(cp);
                    string CMCriteria = null;
                    if (string.IsNullOrEmpty(ContentManagerIdCommaList)) {
                        CMCriteria = "(1=0)";
                    } else {
                        if (ContentManagerIdCommaList.IndexOf(",") + 1 == 0) {
                            CMCriteria = "(ccContent.ID=" + ContentManagerIdCommaList + ")";
                        } else {
                            CMCriteria = "(ccContent.ID in (" + ContentManagerIdCommaList + "))";
                        }
                    }
                    Criteria = Criteria + "AND((DeveloperOnly is null)or(DeveloperOnly=0))"
                        + "AND((AdminOnly is null)or(AdminOnly=0))"
                        + "AND(ID in ("
                        + " SELECT AllowedEntries.ID"
                        + " FROM CCMenuEntries AllowedEntries LEFT JOIN ccContent ON AllowedEntries.ContentID = ccContent.ID"
                        + " Where (" + CMCriteria + "and(ccContent.Active<>0)And((ccContent.DeveloperOnly is null)or(ccContent.DeveloperOnly=0))And((ccContent.AdminOnly is null)or(ccContent.AdminOnly=0)))"
                            + "OR(ccContent.ID Is Null)"
                        + "))";
                }
                if (!string.IsNullOrEmpty(ParentCriteria)) { Criteria = "(" + ParentCriteria + ")AND" + Criteria; }
                string SelectList = "ccMenuEntries.contentcontrolid, ccMenuEntries.Name, ccMenuEntries.ID, ccMenuEntries.LinkPage, ccMenuEntries.ContentID, ccMenuEntries.NewWindow, ccMenuEntries.ParentID, ccMenuEntries.AddonID, ccMenuEntries.NavIconType, ccMenuEntries.NavIconTitle, HelpAddonID,HelpCollectionID,0 as collectionid";
                return "select " + SelectList + " from ccMenuEntries where " + Criteria + " order by ccMenuEntries.Name";
            } catch {
                cp.Site.ErrorReport("Trap");
            }
            //
            // -- should be unreachable
            return "";
        }
        //
        //====================================================================================================
        /// <summary>
        /// return a list of ids for each content the current user has memberRules to edit (like Page Content, etc.)
        /// Ignores content parent/child
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        private string GetContentManagerIdCommaList(CPBaseClass cp) {
            string result = "";
            try {
                string SQL = "Select ccGroupRules.ContentID as ID"
                    + " FROM ((ccmemberrules Left Join ccGroupRules on ccmemberrules.GroupID=ccGroupRules.GroupID)"
                    + " Left Join ccContent on ccGroupRules.ContentID=ccContent.ID)"
                    + " WHERE"
                        + " (ccmemberrules.MemberID=" + cp.User.Id + ")"
                        + " AND(ccGroupRules.Active<>0)"
                        + " AND(ccContent.Active<>0)"
                        + " AND(ccmemberrules.Active<>0)";
                CPCSBaseClass cs = cp.CSNew();
                if (cs.OpenSQL(SQL)) {
                    do {
                        result += "," + cs.GetInteger("id").ToString();
                        cs.GoNext();
                    }
                    while (cs.OK());
                }
                cs.Close();
                if (result.Length > 1) {
                    result = result.Substring(1);
                }
            } catch {
                //
            }
            return result;
        }
        //
        //====================================================================================================
        //
        private void errorReport(CPBaseClass cp, Exception ex, string method) {
            cp.Site.ErrorReport(ex, "Unexpected error in sampleClass." + method);
        }
    }
}
