
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualBasic;
using Contensive.BaseClasses;
using Contensive.Models.Db;

namespace Contensive.Processor.Controllers {
    public static class ExportController {
        // 
        // ====================================================================================================
        /// <summary>
        /// create the colleciton zip file and return the pathFilename in the Cdn
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="collectionId"></param>
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
                    string collectionXml = "<?xml version=\"1.0\" encoding=\"windows-1252\"?>";
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
                    collectionXml += "\r\n" + "<Collection";
                    collectionXml += " name=\"" + CollectionName + "\"";
                    collectionXml += " guid=\"" + CollectionGuid + "\"";
                    collectionXml += " system=\"" + getYesNo(cp, CS.GetBoolean("system")) + "\"";
                    collectionXml += " updatable=\"" + getYesNo(cp, CS.GetBoolean("updatable")) + "\"";
                    collectionXml += " blockNavigatorNode=\"" + getYesNo(cp, CS.GetBoolean("blockNavigatorNode")) + "\"";
                    collectionXml += " onInstallAddonGuid=\"" + onInstallAddonGuid + "\"";
                    collectionXml += ">";
                    cdnExportZip_Filename = encodeFilename(cp, CollectionName + ".zip");
                    List<string> tempPathFileList = new List<string>();
                    string tempExportPath = "CollectionExport" + Guid.NewGuid().ToString() + @"\";
                    // 
                    // --resource executable files
                    string resourceNodeList = ExportResourceListController.getResourceList(cp, CS.GetText("execFileList"), CollectionGuid, tempPathFileList, tempExportPath);
                    // 
                    // helpLink
                    // 
                    if (CS.FieldOK("HelpLink"))
                        collectionXml += "\r\n" + "\t" + "<HelpLink>" + System.Net.WebUtility.HtmlEncode(CS.GetText("HelpLink")) + "</HelpLink>";
                    // 
                    // Help
                    // 
                    collectionXml += "\r\n" + "\t" + "<Help>" + System.Net.WebUtility.HtmlEncode(CS.GetText("Help")) + "</Help>";
                    // 
                    // Addons
                    // 
                    string IncludeSharedStyleGuidList = "";
                    string IncludeModuleGuidList = "";
                    using (CPCSBaseClass CS2 = cp.CSNew()) {
                        CS2.Open("Add-ons", "collectionid=" + collection.id, "name", true, "id");
                        while (CS2.OK()) {
                            collectionXml += ExportAddonController.getAddonNode(cp, CS2.GetInteger("id"), ref IncludeModuleGuidList, ref IncludeSharedStyleGuidList);
                            CS2.GoNext();
                        }
                    }
                    // 
                    // Layouts
                    foreach (var layout in DbBaseModel.createList<LayoutModel>(cp, "(installedByCollectionId=" + collection.id + ")")) {
                        collectionXml += ExportLayoutController.get(cp, layout);
                    }
                    // 
                    // Data Records
                    string DataRecordList = CS.GetText("DataRecordList");
                    collectionXml += ExportDataRecordController.getNodeList(cp, DataRecordList, tempPathFileList, tempExportPath);
                    // 
                    // CDef
                    foreach (Contensive.Models.Db.ContentModel content in createListFromCollection(cp, collection.id)) {
                        if ((string.IsNullOrEmpty(content.ccguid))) {
                            content.ccguid = cp.Utils.CreateGuid();
                            content.save(cp);
                        }
                        XmlController xmlTool = new XmlController(cp);
                        string Node = xmlTool.GetXMLContentDefinition3(content.name);
                        // 
                        // remove the <collection> top node
                        // 
                        int Pos = Strings.InStr(1, Node, "<cdef", CompareMethod.Text);
                        if (Pos > 0) {
                            Node = Strings.Mid(Node, Pos);
                            Pos = Strings.InStr(1, Node, "</cdef>", CompareMethod.Text);
                            if (Pos > 0) {
                                Node = Strings.Mid(Node, 1, Pos + 6);
                                collectionXml += "\r\n" + "\t" + Node;
                            }
                        }
                    }
                    // 
                    // Scripting Modules
                    if (IncludeModuleGuidList != "") {
                        string[] Modules = Strings.Split(IncludeModuleGuidList, "\r\n");
                        for (var Ptr = 0; Ptr <= Information.UBound(Modules); Ptr++) {
                            string ModuleGuid = Modules[Ptr];
                            if (ModuleGuid != "") {
                                using (CPCSBaseClass CS2 = cp.CSNew()) {
                                    CS2.Open("Scripting Modules", "ccguid=" + cp.Db.EncodeSQLText(ModuleGuid));
                                    if (CS2.OK()) {
                                        string Code = CS2.GetText("code").Trim();
                                        Code = EncodeCData(cp, Code);
                                        collectionXml += "\r\n" + "\t" + "<ScriptingModule Name=\"" + System.Net.WebUtility.HtmlEncode(CS2.GetText("name")) + "\" guid=\"" + ModuleGuid + "\">" + Code + "</ScriptingModule>";
                                    }
                                    CS2.Close();
                                }
                            }
                        }
                    }
                    // 
                    // shared styles
                    string[] recordGuids;
                    string recordGuid;
                    if ((IncludeSharedStyleGuidList != "")) {
                        recordGuids = Strings.Split(IncludeSharedStyleGuidList, "\r\n");
                        for (var Ptr = 0; Ptr <= Information.UBound(recordGuids); Ptr++) {
                            recordGuid = recordGuids[Ptr];
                            if (recordGuid != "") {
                                using (CPCSBaseClass CS2 = cp.CSNew()) {
                                    CS2.Open("Shared Styles", "ccguid=" + cp.Db.EncodeSQLText(recordGuid));
                                    if (CS2.OK())
                                        collectionXml += "\r\n" + "\t" + "<SharedStyle"
                                            + " Name=\"" + System.Net.WebUtility.HtmlEncode(CS2.GetText("name")) + "\""
                                            + " guid=\"" + recordGuid + "\""
                                            + " alwaysInclude=\"" + CS2.GetBoolean("alwaysInclude") + "\""
                                            + " prefix=\"" + System.Net.WebUtility.HtmlEncode(CS2.GetText("prefix")) + "\""
                                            + " suffix=\"" + System.Net.WebUtility.HtmlEncode(CS2.GetText("suffix")) + "\""
                                            + " sortOrder=\"" + System.Net.WebUtility.HtmlEncode(CS2.GetText("sortOrder")) + "\""
                                            + ">"
                                            + EncodeCData(cp, CS2.GetText("styleFilename").Trim())
                                            + "</SharedStyle>";
                                    CS2.Close();
                                }
                            }
                        }
                    }
                    // 
                    // Import Collections
                    if (true) {
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

                                            Node = Node + "\r\n" + "\t" + "<ImportCollection name=\"" + System.Net.WebUtility.HtmlEncode(CS2.GetText("name")) + "\">" + Guid + "</ImportCollection>";
                                        }

                                        CS2.Close();
                                    }

                                    CS3.GoNext();
                                }
                                while (CS3.OK());
                            }
                            CS3.Close();
                        }
                        collectionXml += Node;
                    }
                    // 
                    // wwwFileList
                    if (true) {
                        string wwwFileList = CS.GetText("wwwFileList");
                        if (wwwFileList != "") {
                            string[] Files = Strings.Split(wwwFileList, "\r\n");
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
                                        if (!cp.PrivateFiles.FileExists(addonPath + collectionPath + filename  )) {
                                            //
                                            // - not there
                                            cp.UserError.Add("There was an error exporting this collection because the zip file [" + pathFilename + "] was not found in the collection path [" + collectionPath + "].");
                                        } else {
                                            // 
                                            // -- copy file from here
                                            cp.PrivateFiles.Copy(addonPath + collectionPath + filename, tempExportPath + filename, cp.TempFiles);
                                            tempPathFileList.Add(tempExportPath + filename);
                                            collectionXml += "\r\n" + "\t" + "<Resource name=\"" + System.Net.WebUtility.HtmlEncode(filename) + "\" type=\"www\" path=\"" + System.Net.WebUtility.HtmlEncode(path) + "\" />";
                                        }
                                    } else if ((!cp.WwwFiles.FileExists(pathFilename))) {
                                        cp.UserError.Add("There was an error exporting this collection because the www file [" + pathFilename + "] was not found.");

                                    } else {
                                        cp.WwwFiles.Copy(pathFilename, tempExportPath + filename, cp.TempFiles);
                                        tempPathFileList.Add(tempExportPath + filename);
                                        collectionXml += "\r\n" + "\t" + "<Resource name=\"" + System.Net.WebUtility.HtmlEncode(filename) + "\" type=\"www\" path=\"" + System.Net.WebUtility.HtmlEncode(path) + "\" />";
                                    }
                                }
                            }
                        }
                    }
                    // 
                    // ContentFileList
                    // 
                    if (true) {
                        string ContentFileList = CS.GetText("ContentFileList");
                        if (ContentFileList != "") {
                            string[] Files = Strings.Split(ContentFileList, "\r\n");
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
                                        collectionXml += "\r\n" + "\t" + "<Resource name=\"" + System.Net.WebUtility.HtmlEncode(Filename) + "\" type=\"content\" path=\"" + System.Net.WebUtility.HtmlEncode(Path) + "\" />";
                                    }
                                }
                            }
                        }
                    }
                    // 
                    // ExecFileListNode
                    // 
                    collectionXml += resourceNodeList;
                    // 
                    // Other XML
                    // 
                    string OtherXML;
                    OtherXML = CS.GetText("otherxml");
                    if (Strings.Trim(OtherXML) != "")
                        collectionXml += "\r\n" + OtherXML;
                    collectionXml += "\r\n" + "</Collection>";
                    CS.Close();
                    string tempExportXml_Filename = encodeFilename(cp, CollectionName + ".xml");
                    // 
                    // Save the installation file and add it to the archive
                    // 
                    cp.TempFiles.Save(tempExportPath + tempExportXml_Filename, collectionXml);
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
        /// <summary>
        ///         ''' create a simple text node with a name and content
        ///         ''' </summary>
        ///         ''' <param name="NodeName"></param>
        ///         ''' <param name="NodeContent"></param>
        ///         ''' <returns></returns>
        ///         ''' <remarks></remarks>
        public static string getNodeText(CPBaseClass cp, string NodeName, string NodeContent, bool deprecated = false) {
            string result = "";
            try {
                string prefix = "";
                if ((deprecated))
                    prefix = "<!-- deprecated -->";
                if (NodeContent == "")
                    return result + "\r\n" + "\t" + prefix + "<" + NodeName + "></" + NodeName + ">";
                else
                    return result + "\r\n" + "\t" + prefix + "<" + NodeName + ">" + EncodeCData(cp, NodeContent) + "</" + NodeName + ">";
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex, "getNodeText");
                return string.Empty;
            }
        }
        // 
        // ====================================================================================================
        /// <summary>
        ///         ''' create a simple boolean node with a name and content
        ///         ''' </summary>
        ///         ''' <param name="NodeName"></param>
        ///         ''' <param name="NodeContent"></param>
        ///         ''' <returns></returns>
        ///         ''' <remarks></remarks>
        public static string getNodeBoolean(CPBaseClass cp, string NodeName, bool NodeContent) {
            try {
                return "\r\n" + "\t" + "<" + NodeName + ">" + getYesNo(cp, NodeContent) + "</" + NodeName + ">";
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex, "GetNodeBoolean");
                return string.Empty;
            }
        }
        // 
        // ====================================================================================================
        /// <summary>
        ///         ''' create a simple integer node with a name and content
        ///         ''' </summary>
        ///         ''' <param name="NodeName"></param>
        ///         ''' <param name="NodeContent"></param>
        ///         ''' <returns></returns>
        ///         ''' <remarks></remarks>
        public static string GetNodeInteger(CPBaseClass cp, string NodeName, int NodeContent) {
            try {
                return "\r\n" + "\t" + "<" + NodeName + ">" + System.Convert.ToString(NodeContent) + "</" + NodeName + ">";
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex, "GetNodeInteger");
                return string.Empty;
            }
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
        // 
        public static string EncodeCData(CPBaseClass cp, string Source) {
            try {
                string result = Source;
                if (result != "")
                    result = "<![CDATA[" + Strings.Replace(result, "]]>", "]]]]><![CDATA[>") + "]]>";
                return result;
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex, "EncodeCData");
                return string.Empty;
            }
        }
        // 
        // ====================================================================================================
        public static string getYesNo(CPBaseClass cp, bool Key) {
            return Key ? "Yes" : "No";
        }
        // 
        // =======================================================================================
        /// <summary>
        ///         ''' zip
        ///         ''' </summary>
        ///         ''' <param name="PathFilename"></param>
        ///         ''' <remarks></remarks>
        public static void UnzipFile(CPBaseClass cp, string PathFilename) {
            try {
                // 
                ICSharpCode.SharpZipLib.Zip.FastZip fastZip = new ICSharpCode.SharpZipLib.Zip.FastZip();
                string fileFilter = null;

                fastZip.ExtractZip(PathFilename, getPath(cp, PathFilename), fileFilter);                // 
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex, "UnzipFile");
            }
        }        // 
        // 
        // =======================================================================================
        /// <summary>
        ///         ''' unzip
        ///         ''' </summary>
        ///         ''' <param name="zipTempPathFilename"></param>
        ///         ''' <param name="addTempPathFilename"></param>
        ///         ''' <remarks></remarks>
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
        // 
        public static string getPath(CPBaseClass cp, string pathFilename) {
            int Position = Strings.InStrRev(pathFilename, @"\");
            if (Position != 0)
                return Strings.Mid(pathFilename, 1, Position);
            return string.Empty;
        }
        // 
        // =======================================================================================
        // 
        public static string getFilename(CPBaseClass cp, string PathFilename) {
            int pos = Strings.InStrRev(PathFilename, "/");
            if (pos != 0)
                return Strings.Mid(PathFilename, pos + 1);
            return PathFilename;
        }
        // 
        // =======================================================================================
        // 
        // Indent every line by 1 tab
        // 
        public static string tabIndent(CPBaseClass cp, string Source) {
            int posStart = Strings.InStr(1, Source, "<![CDATA[", CompareMethod.Text);
            if (posStart == 0) {
                // 
                // no cdata
                posStart = Strings.InStr(1, Source, "<textarea", CompareMethod.Text);
                if (posStart == 0)
                    // 
                    // no textarea
                    // 
                    return Strings.Replace(Source, "\r\n" + "\t", "\r\n" + "\t" + "\t");
                else {
                    // 
                    // text area found, isolate it and indent before and after
                    // 
                    int posEnd = Strings.InStr(posStart, Source, "</textarea>", CompareMethod.Text);
                    string pre = Strings.Mid(Source, 1, posStart - 1);
                    string post = "";
                    string target;
                    if (posEnd == 0)
                        target = Strings.Mid(Source, posStart);
                    else {
                        target = Strings.Mid(Source, posStart, posEnd - posStart + Strings.Len("</textarea>"));
                        post = Strings.Mid(Source, posEnd + Strings.Len("</textarea>"));
                    }
                    return tabIndent(cp, pre) + target + tabIndent(cp, post);
                }
            } else {
                // 
                // cdata found, isolate it and indent before and after
                // 
                int posEnd = Strings.InStr(posStart, Source, "]]>", CompareMethod.Text);
                string pre = Strings.Mid(Source, 1, posStart - 1);
                string post = "";
                string target;
                if (posEnd == 0)
                    target = Strings.Mid(Source, posStart);
                else {
                    target = Strings.Mid(Source, posStart, posEnd - posStart + Strings.Len("]]>"));
                    post = Strings.Mid(Source, posEnd + 3);
                }
                return tabIndent(cp, pre) + target + tabIndent(cp, post);
            }
        }
        //
        public static List<ContentModel> createListFromCollection(CPBaseClass cp, int collectionId) {
            return DbBaseModel.createList<ContentModel>(cp, "id in (select distinct contentId from ccAddonCollectionCDefRules where collectionid=" + collectionId + ")", "name");
        }

    }
}
