﻿
using Contensive.BaseClasses;
using Contensive.Models.Db;
using Contensive.Processor.Models.Domain;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Contensive.Processor.Controllers {
    /// <summary>
    /// Export Controller
    /// </summary>
    public static class ExportController {
        // 
        // ====================================================================================================
        /// <summary>
        /// create the colleciton zip file and return the pathFilename in the Cdn
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static string createCollectionZip_returnCdnPathFilename(CPBaseClass cp, AddonCollectionModel collection) {
            string cdnExportZip_Filename = "";
            try {
                if ((collection == null)) {
                    // 
                    // -- exit with error
                    cp.UserError.Add("The collection you selected could not be found");
                    return string.Empty;
                }
                using (CPCSBaseClass CS = cp.CSNew()) {
                    CS.OpenRecord("Add-on Collections", collection.id);
                    if (!CS.OK()) {
                        // 
                        // -- exit with error
                        cp.UserError.Add("The collection you selected could not be found");
                        return string.Empty;
                    }
                    var collectionXml = new StringBuilder("<?xml version=\"1.0\" encoding=\"windows-1252\"?>");
                    string CollectionGuid = CS.GetText("ccGuid");
                    if (CollectionGuid == "") {
                        CollectionGuid = cp.Utils.CreateGuid();
                        CS.SetField("ccGuid", CollectionGuid);
                    }
                    string onInstallAddonGuid = "";
                    if ((CS.FieldOK("onInstallAddonId"))) {
                        int onInstallAddonId = CS.GetInteger("onInstallAddonId");
                        if ((onInstallAddonId > 0)) {
                            AddonModel addon = AddonModel.create<AddonModel>(cp, onInstallAddonId);
                            if ((addon != null))
                                onInstallAddonGuid = addon.ccguid;
                        }
                    }
                    string CollectionName = CS.GetText("name");
                    collectionXml.Append(System.Environment.NewLine + "<Collection");
                    collectionXml.Append(" name=\"" + CollectionName + "\"");
                    collectionXml.Append(" guid=\"" + CollectionGuid + "\"");
                    collectionXml.Append(" system=\"" + GenericController.getYesNo(CS.GetBoolean("system")) + "\"");
                    collectionXml.Append(" updatable=\"" + GenericController.getYesNo(CS.GetBoolean("updatable")) + "\"");
                    collectionXml.Append(" blockNavigatorNode=\"" + GenericController.getYesNo(CS.GetBoolean("blockNavigatorNode")) + "\"");
                    collectionXml.Append(" onInstallAddonGuid=\"" + onInstallAddonGuid + "\"");
                    collectionXml.Append(">");
                    cdnExportZip_Filename = encodeFilename(cp, CollectionName + ".zip");
                    List<string> tempPathFileList = new List<string>();
                    string tempExportPath = "CollectionExport" + Guid.NewGuid().ToString() + @"\";
                    // 
                    // --resource executable files
                    string wwwFileList = CS.GetText("wwwFileList");
                    string ContentFileList = CS.GetText("ContentFileList");
                    List<string> execFileList = ExportResourceListController.getResourceFileList(cp, CS.GetText("execFileList"), CollectionGuid);
                    string execResourceNodeList = ExportResourceListController.getResourceNodeList(cp, execFileList, CollectionGuid, tempPathFileList, tempExportPath);
                    // 
                    // -- helpLink
                    if (CS.FieldOK("HelpLink"))
                        collectionXml.Append(System.Environment.NewLine + "\t" + "<HelpLink>" + System.Net.WebUtility.HtmlEncode(CS.GetText("HelpLink")) + "</HelpLink>");
                    // 
                    // -- Help
                    collectionXml.Append(System.Environment.NewLine + "\t" + "<Help>" + System.Net.WebUtility.HtmlEncode(CS.GetText("Help")) + "</Help>");
                    // 
                    // -- Addons
                    string IncludeSharedStyleGuidList = "";
                    string IncludeModuleGuidList = "";
                    foreach (var addon in DbBaseModel.createList<AddonModel>(cp, "collectionid=" + collection.id)) {
                        //
                        // -- style sheet is in the wwwroot
                        if (!string.IsNullOrEmpty(addon.stylesLinkHref)) {
                            string filename = addon.stylesLinkHref.Replace("/", "\\");
                            if (filename.Substring(0, 1).Equals(@"\")) { filename = filename.Substring(1); }
                            if (!cp.WwwFiles.FileExists(filename)) {
                                cp.WwwFiles.Save(filename, @"/* css file created as exported for addon [" + addon.name + "], collection [" + collection.name + "] in site [" + cp.Site.Name + "] */");
                            }
                            wwwFileList += System.Environment.NewLine + addon.stylesLinkHref;
                        }
                        //
                        // -- js is in the wwwroot
                        if (!string.IsNullOrEmpty(addon.jsHeadScriptSrc)) {
                            string filename = addon.jsHeadScriptSrc.Replace("/", "\\");
                            if (filename.Substring(0, 1).Equals(@"\")) { filename = filename.Substring(1); }
                            if (!cp.WwwFiles.FileExists(filename)) {
                                cp.WwwFiles.Save(filename, @"// javascript file created as exported for addon [" + addon.name + "], collection [" + collection.name + "] in site [" + cp.Site.Name + "]");
                            }
                            wwwFileList += System.Environment.NewLine + addon.jsHeadScriptSrc;
                        }
                        collectionXml.Append(ExportAddonController.getAddonNode(cp, addon.id, ref IncludeModuleGuidList, ref IncludeSharedStyleGuidList));
                    }
                    // 
                    // -- Data Records
                    List<CollectionDataExportModel> dataRecordObjList = new List<CollectionDataExportModel>();
                    string dataRecordCrlfList = CS.GetText("DataRecordList");
                    if (!string.IsNullOrEmpty(dataRecordCrlfList)) {
                        //
                        // -- save collection record datarecord list to collection xml
                        collectionXml.Append(System.Environment.NewLine + "\t" + "<DataRecordList>" + encodeCData(dataRecordCrlfList) + "</DataRecordList>");
                        //
                        // -- first, create dataRecordList of records in the Collection's DataRecord tab
                        foreach (var dataRecord in Strings.Split(dataRecordCrlfList, Environment.NewLine).ToList()) {
                            if (string.IsNullOrEmpty(dataRecord)) {
                                //
                                // -- row is empty, skit pit
                                continue;
                            }
                            string[] dataSplit = Strings.Split(dataRecord, ",");
                            CollectionDataExportModel dataRecordObj = new CollectionDataExportModel { contentName = dataSplit[0] };
                            dataRecordObjList.Add(dataRecordObj);
                            if (dataSplit.Length.Equals(1)) {
                                //
                                // -- row is just contentName, exports the entire table
                                continue;
                            }
                            if (GenericController.isGuid(dataSplit[1])) {
                                //
                                // -- row is contentNae,Guid
                                dataRecordObj.recordGuid = dataSplit[1];
                                continue;
                            }
                            //
                            // -- row is contentName, recordName
                            dataRecordObj.recordName = dataSplit[1];
                        }
                    }
                    //
                    // -- add records from any content that supports the field 'collectionid' or 'installedbycollectionid'

                    foreach (var field in new List<string>() { "collectionId", "installedbycollectionid" }) {
                        using (var csTable = cp.CSNew()) {
                            if (csTable.OpenSQL("select c.name as contentName, c.id as contentId, t.name as tableName from cccontent c left join cctables t on t.id=c.ContentTableID left join ccfields f on f.ContentID=c.id  where (c.name<>'add-ons')and(f.name=" + cp.Db.EncodeSQLText(field) + ")")) {
                                do {
                                    using (var csRecord = cp.CSNew()) {
                                        if (csRecord.OpenSQL("select ccguid from " + csTable.GetText("tableName") + " where (" + field + "=" + collection.id + ")and((contentcontrolid=" + csTable.GetInteger("contentid") + ")or(contentcontrolid=0))")) {
                                            do {
                                                dataRecordObjList.Add(new CollectionDataExportModel {
                                                    contentName = csTable.GetText("contentName"),
                                                    recordGuid = csRecord.GetText("ccguid")
                                                });
                                                csRecord.GoNext();
                                            } while (csRecord.OK());
                                        }
                                    }
                                    csTable.GoNext();
                                } while (csTable.OK());
                            }
                        }
                    }
                    //
                    // -- add all datarecords to the xml export
                    collectionXml.Append(ExportDataRecordController.getNodeList(cp, dataRecordObjList, tempPathFileList, tempExportPath));
                    // 
                    // CDef
                    foreach (Contensive.Models.Db.ContentModel content in createListFromCollection(cp, collection.id)) {
                        if ((string.IsNullOrEmpty(content.ccguid))) {
                            content.ccguid = cp.Utils.CreateGuid();
                            content.save(cp);
                        }
                        XmlController xmlTool = new XmlController(cp);
                        string Node = xmlTool.getXMLContentDefinition3(content.name);
                        // 
                        // remove the <collection> top node
                        // 
                        int Pos = Strings.InStr(1, Node, "<cdef", CompareMethod.Text);
                        if (Pos > 0) {
                            Node = Strings.Mid(Node, Pos);
                            Pos = Strings.InStr(1, Node, "</cdef>", CompareMethod.Text);
                            if (Pos > 0) {
                                Node = Strings.Mid(Node, 1, Pos + 6);
                                collectionXml.Append(System.Environment.NewLine + "\t" + Node);
                            }
                        }
                    }
                    // 
                    // Import Collections
                    {
                        string Node = "";
                        using (CPCSBaseClass CS3 = cp.CSNew()) {
                            if (CS3.Open("Add-on Collection Parent Rules", "parentid=" + collection.id)) {
                                do {
                                    using (CPCSBaseClass CS2 = cp.CSNew()) {
                                        if (CS2.OpenRecord("Add-on Collections", CS3.GetInteger("childid"))) {
                                            string Guid = CS2.GetText("ccGuid");
                                            if (Guid == "") {
                                                Guid = cp.Utils.CreateGuid();
                                                CS2.SetField("ccGuid", Guid);
                                            }

                                            Node = Node + System.Environment.NewLine + "\t" + "<ImportCollection name=\"" + System.Net.WebUtility.HtmlEncode(CS2.GetText("name")) + "\">" + Guid + "</ImportCollection>";
                                        }

                                        CS2.Close();
                                    }

                                    CS3.GoNext();
                                }
                                while (CS3.OK());
                            }
                            CS3.Close();
                        }
                        collectionXml.Append(Node);
                    }
                    // 
                    // wwwFileList
                    if (wwwFileList != "") {
                        string[] Files = Strings.Split(wwwFileList, System.Environment.NewLine);
                        for (int Ptr = 0; Ptr <= Information.UBound(Files); Ptr++) {
                            string pathFilename = Files[Ptr];
                            if (pathFilename != "") {
                                pathFilename = Strings.Replace(pathFilename, @"\", "/");
                                string path = "";
                                string filename = pathFilename;
                                int Pos = Strings.InStrRev(pathFilename, "/");
                                if (Pos > 0) {
                                    filename = Strings.Mid(pathFilename, Pos + 1);
                                    path = Strings.Mid(pathFilename, 1, Pos - 1);
                                }
                                string fileExtension = System.IO.Path.GetExtension(filename);
                                pathFilename = Strings.Replace(pathFilename, "/", @"\");
                                if (tempPathFileList.Contains(tempExportPath + filename)) {
                                    //
                                    // -- the path already has a file with this name
                                    cp.UserError.Add("There was an error exporting this collection because there were multiple files with the same filename [" + filename + "]");
                                } else if (fileExtension.ToUpperInvariant().Equals(".ZIP")) {
                                    //
                                    // -- zip files come from the collection folder
                                    CoreController core = ((CPClass)cp).core;
                                    string addonPath = AddonController.getPrivateFilesAddonPath();
                                    string collectionPath = CollectionFolderController.getCollectionConfigFolderPath(core, collection.ccguid);
                                    if (!cp.PrivateFiles.FileExists(addonPath + collectionPath + filename)) {
                                        //
                                        // - not there
                                        cp.UserError.Add("There was an error exporting this collection because the zip file [" + pathFilename + "] was not found in the collection path [" + collectionPath + "].");
                                    } else {
                                        // 
                                        // -- copy file from here
                                        cp.PrivateFiles.Copy(addonPath + collectionPath + filename, tempExportPath + filename, cp.TempFiles);
                                        tempPathFileList.Add(tempExportPath + filename);
                                        collectionXml.Append(System.Environment.NewLine + "\t" + "<Resource name=\"" + System.Net.WebUtility.HtmlEncode(filename) + "\" type=\"www\" path=\"" + System.Net.WebUtility.HtmlEncode(path) + "\" />");
                                    }
                                } else if ((!cp.WwwFiles.FileExists(pathFilename))) {
                                    cp.UserError.Add("There was an error exporting this collection because the www file [" + pathFilename + "] was not found.");

                                } else {
                                    cp.WwwFiles.Copy(pathFilename, tempExportPath + filename, cp.TempFiles);
                                    tempPathFileList.Add(tempExportPath + filename);
                                    collectionXml.Append(System.Environment.NewLine + "\t" + "<Resource name=\"" + System.Net.WebUtility.HtmlEncode(filename) + "\" type=\"www\" path=\"" + System.Net.WebUtility.HtmlEncode(path) + "\" />");
                                }
                            }
                        }
                    }
                    // 
                    // ContentFileList
                    // 
                    if (true) {
                        if (ContentFileList != "") {
                            string[] Files = Strings.Split(ContentFileList, System.Environment.NewLine);
                            for (var Ptr = 0; Ptr <= Information.UBound(Files); Ptr++) {
                                string PathFilename = Files[Ptr];
                                if (PathFilename != "") {
                                    PathFilename = Strings.Replace(PathFilename, @"\", "/");
                                    string Path = "";
                                    string Filename = PathFilename;
                                    int Pos = Strings.InStrRev(PathFilename, "/");
                                    if (Pos > 0) {
                                        Filename = Strings.Mid(PathFilename, Pos + 1);
                                        Path = Strings.Mid(PathFilename, 1, Pos - 1);
                                    }
                                    if (tempPathFileList.Contains(tempExportPath + Filename))
                                        cp.UserError.Add("There was an error exporting this collection because there were multiple files with the same filename [" + Filename + "]");
                                    else if ((!cp.CdnFiles.FileExists(PathFilename)))
                                        cp.UserError.Add("There was an error exporting this collection because the cdn file [" + PathFilename + "] was not found.");
                                    else {
                                        cp.CdnFiles.Copy(PathFilename, tempExportPath + Filename, cp.TempFiles);
                                        tempPathFileList.Add(tempExportPath + Filename);
                                        collectionXml.Append(System.Environment.NewLine + "\t" + "<Resource name=\"" + System.Net.WebUtility.HtmlEncode(Filename) + "\" type=\"content\" path=\"" + System.Net.WebUtility.HtmlEncode(Path) + "\" />");
                                    }
                                }
                            }
                        }
                    }
                    // 
                    // ExecFileListNode
                    // 
                    collectionXml.Append(execResourceNodeList);
                    // 
                    // Other XML
                    // 
                    string OtherXML;
                    OtherXML = CS.GetText("otherxml");
                    if (Strings.Trim(OtherXML) != "")
                        collectionXml.Append(System.Environment.NewLine + OtherXML);
                    collectionXml.Append(System.Environment.NewLine + "</Collection>");
                    CS.Close();
                    string tempExportXml_Filename = encodeFilename(cp, CollectionName + ".xml");
                    // 
                    // Save the installation file and add it to the archive
                    // 
                    cp.TempFiles.Save(tempExportPath + tempExportXml_Filename, collectionXml.ToString());
                    if (!tempPathFileList.Contains(tempExportPath + tempExportXml_Filename))
                        tempPathFileList.Add(tempExportPath + tempExportXml_Filename);
                    string tempExportZip_Filename = encodeFilename(cp, CollectionName + ".zip");
                    // 
                    // -- zip up the folder to make the collection zip file in temp filesystem
                    zipTempCdnFile(cp, tempExportPath + tempExportZip_Filename, tempPathFileList);
                    // 
                    // -- copy the collection zip file to the cdn filesystem as the download link
                    cp.TempFiles.Copy(tempExportPath + tempExportZip_Filename, cdnExportZip_Filename, cp.CdnFiles);
                    // 
                    // -- delete the temp folder
                    cp.TempFiles.DeleteFolder(tempExportPath);
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return cdnExportZip_Filename;
        }
        // 
        // ====================================================================================================
        //
        public static string getNode(string NodeName, string NodeContent, bool deprecated) {
            string xmlContent = (string.IsNullOrWhiteSpace(NodeContent)) ? "" : encodeCData(NodeContent);
            return Environment.NewLine + "\t" + (deprecated ? "<!-- deprecated -->" : "") + "<" + NodeName + ">" + xmlContent + "</" + NodeName + ">";
        }
        //
        public static string getNode(string NodeName, string NodeContent)
            => getNode(NodeName, NodeContent, false);
        // 
        // ====================================================================================================
        //
        public static string getNode(string NodeName, int NodeContent, bool deprecated) {
            return System.Environment.NewLine + "\t" + (deprecated ? "<!-- deprecated -->" : "") + "<" + NodeName + ">" + NodeContent + "</" + NodeName + ">";
        }
        //
        public static string getNode(string NodeName, int NodeContent)
            => getNode(NodeName, NodeContent, false);
        // 
        // ====================================================================================================
        //
        public static string getNode(string NodeName, bool NodeContent, bool deprecated) {
            return System.Environment.NewLine + "\t" + (deprecated ? "<!-- deprecated -->" : "") + "<" + NodeName + ">" + GenericController.getYesNo(NodeContent) + "</" + NodeName + ">";
        }
        //
        public static string getNode(string NodeName, bool NodeContent)
            => getNode(NodeName, NodeContent, false);
        // 
        // ====================================================================================================
        //
        public static string getNodeLookupContentName(CPBaseClass cp, string nodeName, int recordId, string contentName) {
            using (CPCSBaseClass cs = cp.CSNew()) {
                if (cs.OpenRecord(contentName, recordId, "name", false)) {
                    return getNode(nodeName, cs.GetText("name"), false);
                }
            }
            return getNode(nodeName, "", false);
        }
        // 
        // ====================================================================================================
        public static string replaceMany(CPBaseClass cp, string Source, string[] ArrayOfSource, string[] ArrayOfReplacement) {
            try {
                int Count = Information.UBound(ArrayOfSource) + 1;
                string result = Source;
                for (int Pointer = 0; Pointer <= Count - 1; Pointer++)
                    result = Strings.Replace(result, ArrayOfSource[Pointer], ArrayOfReplacement[Pointer]);
                return result;
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex, "replaceMany");
                return string.Empty;
            }
        }
        // 
        // ====================================================================================================
        public static string encodeFilename(CPBaseClass cp, string Filename) {
            string result = "";
            try {
                string[] Source;
                string[] Replacement;
                // 
                Source = new[] { "\"", "*", "/", ":", "<", ">", "?", @"\", "|" };
                Replacement = new[] { "_", "_", "_", "_", "_", "_", "_", "_", "_" };
                // 
                result = replaceMany(cp, Filename, Source, Replacement);
                if (Strings.Len(result) > 254)
                    result = Strings.Left(result, 254);
                return result;
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex, "encodeFilename");
                return string.Empty;
            }
        }
        // 
        // ====================================================================================================
        // 
        public static void GetLocalCollectionArgs(CPBaseClass cp, string CollectionGuid, ref string Return_CollectionPath, ref DateTime Return_LastChangeDate) {
            try {
                const string CollectionListRootNode = "collectionlist";
                Return_CollectionPath = "";
                Return_LastChangeDate = DateTime.MinValue;
                System.Xml.XmlDocument Doc = new System.Xml.XmlDocument() { XmlResolver = null };
                Doc.LoadXml(cp.PrivateFiles.Read(@"addons\Collections.xml"));
                if (true) {
                    if (Strings.LCase(Doc.DocumentElement.Name) != Strings.LCase(CollectionListRootNode)) {
                    } else {
                        var withBlock = Doc.DocumentElement;
                        if (Strings.LCase(withBlock.Name) != "collectionlist") {
                        } else {
                            // hint = hint & ",checking nodes [" & .childNodes.length & "]"
                            foreach (System.Xml.XmlNode LocalListNode in withBlock.ChildNodes) {
                                string LocalName = "no name found";
                                string LocalGuid = "";
                                string CollectionPath = "";
                                DateTime LastChangeDate = default;
                                switch (Strings.LCase(LocalListNode.Name)) {
                                    case "collection": {
                                            LocalGuid = "";
                                            foreach (System.Xml.XmlNode CollectionNode in LocalListNode.ChildNodes) {
                                                switch (Strings.LCase(CollectionNode.Name)) {
                                                    case "name": {
                                                            // 
                                                            LocalName = Strings.LCase(CollectionNode.InnerText);
                                                            break;
                                                        }

                                                    case "guid": {
                                                            // 
                                                            LocalGuid = Strings.LCase(CollectionNode.InnerText);
                                                            break;
                                                        }

                                                    case "path": {
                                                            // 
                                                            CollectionPath = Strings.LCase(CollectionNode.InnerText);
                                                            break;
                                                        }

                                                    case "lastchangedate": {
                                                            LastChangeDate = cp.Utils.EncodeDate(CollectionNode.InnerText);
                                                            break;
                                                        }
                                                }
                                            }

                                            break;
                                        }
                                }
                                // hint = hint & ",checking node [" & LocalName & "]"
                                if (Strings.LCase(CollectionGuid) == LocalGuid) {
                                    Return_CollectionPath = CollectionPath;
                                    Return_LastChangeDate = LastChangeDate;
                                    break;
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex, "GetLocalCollectionArgs");
            }
        }
        // 
        // ====================================================================================================
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string encodeCData(string source) {
            if (string.IsNullOrWhiteSpace(source)) return "";
            return "<![CDATA[" + Strings.Replace(source, "]]>", "]]]]><![CDATA[>") + "]]>";
        }
        // 
        // =======================================================================================
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="zipTempPathFilename"></param>
        /// <param name="addTempPathFilename"></param>
        public static void zipTempCdnFile(CPBaseClass cp, string zipTempPathFilename, List<string> addTempPathFilename) {
            try {
                ICSharpCode.SharpZipLib.Zip.ZipFile z;
                if (cp.TempFiles.FileExists(zipTempPathFilename))
                    // 
                    // update existing zip with list of files
                    z = new ICSharpCode.SharpZipLib.Zip.ZipFile(cp.TempFiles.PhysicalFilePath + zipTempPathFilename);
                else
                    // 
                    // create new zip
                    z = ICSharpCode.SharpZipLib.Zip.ZipFile.Create(cp.TempFiles.PhysicalFilePath + zipTempPathFilename);
                z.BeginUpdate();
                foreach (var pathFilename in addTempPathFilename)
                    z.Add(cp.TempFiles.PhysicalFilePath + pathFilename, System.IO.Path.GetFileName(pathFilename));
                z.CommitUpdate();
                z.Close();
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
        }
        // 
        // =======================================================================================
        /// <summary>
        /// Indent every line by 1 tab
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="Source"></param>
        /// <returns></returns>
        public static string tabIndent(CPBaseClass cp, string Source) {
            return Source;
            //
            int posStart = Strings.InStr(1, Source, "<![CDATA[", CompareMethod.Text);
            if (posStart == 0) {
                // 
                // no cdata
                posStart = Strings.InStr(1, Source, "<textarea", CompareMethod.Text);
                if (posStart == 0)
                    // 
                    // no textarea
                    // 
                    return Strings.Replace(Source, System.Environment.NewLine + "\t", System.Environment.NewLine + "\t" + "\t");
                else {
                    // 
                    // text area found, isolate it and indent before and after
                    int posEnd = Strings.InStr(posStart, Source, "</textarea>", CompareMethod.Text);
                    if (posEnd == 0) {
                        //
                        // -- append target, no post
                        return ""
                            + tabIndent(cp, Strings.Mid(Source, 1, posStart - 1))
                            + Strings.Mid(Source, posStart);
                    } else {
                        //
                        // -- append target
                        return ""
                            + tabIndent(cp, Strings.Mid(Source, 1, posStart - 1))
                            + Strings.Mid(Source, posStart, posEnd - posStart + Strings.Len("</textarea>"))
                            + tabIndent(cp, Strings.Mid(Source, posEnd + Strings.Len("</textarea>")));
                    }
                }
            } else {
                // 
                // cdata found, isolate it and indent before and after
                int posEnd = Strings.InStr(posStart, Source, "]]>", CompareMethod.Text);
                if (posEnd == 0) {
                    return ""
                        + tabIndent(cp, Strings.Mid(Source, 1, posStart - 1))
                        + Strings.Mid(Source, posStart);
                } else {
                    return ""
                        + tabIndent(cp, Strings.Mid(Source, 1, posStart - 1))
                        + Strings.Mid(Source, posStart, posEnd - posStart + Strings.Len("]]>"))
                        + tabIndent(cp, Strings.Mid(Source, posEnd + 3));
                }
            }
        }
        //
        // =======================================================================================
        /// <summary>
        /// createListFromCollection
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="collectionId"></param>
        /// <returns></returns>
        public static List<ContentModel> createListFromCollection(CPBaseClass cp, int collectionId) {
            return DbBaseModel.createList<ContentModel>(cp, "id in (select distinct contentId from ccAddonCollectionCDefRules where collectionid=" + collectionId + ")", "name");
        }

    }
}
