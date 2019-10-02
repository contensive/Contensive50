
using System;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using System.Xml;
using System.Collections.Generic;
using static Contensive.BaseClasses.CPFileSystemBaseClass;
using Contensive.Exceptions;

namespace Contensive.Addons.Housekeeping {
    //
    public class AddonFolderClass {
        //====================================================================================================
        //
        public static void housekeep(CoreController core) {
            try {
                string RegisterPathList = null;
                string RegisterPath = null;
                string[] RegisterPaths = null;
                string Path = null;
                int NodeCnt = 0;
                string Cmd = null;
                string CollectionRootPath = null;
                int Pos = 0;
                string LocalGuid = null;
                XmlDocument Doc = new XmlDocument();
                string CollectionPath = null;
                DateTime LastChangeDate = default(DateTime);
                string hint = "";
                string LocalName = null;
                int Ptr = 0;
                string collectionFileFilename = null;
                //
                LogController.logInfo(core, "Entering RegisterAddonFolder");
                //
                bool loadOK = true;
                try {
                    collectionFileFilename = core.addon.getPrivateFilesAddonPath() + "Collections.xml";
                    string collectionFileContent = core.privateFiles.readFileText(collectionFileFilename);
                    Doc.LoadXml(collectionFileContent);
                } catch (Exception) {
                    LogController.logInfo(core, "RegisterAddonFolder, Hint=[" + hint + "], Error loading Collections.xml file.");
                    loadOK = false;
                }
                if (loadOK) {
                    //
                    LogController.logInfo(core, "Collection.xml loaded ok");
                    //
                    if (GenericController.vbLCase(Doc.DocumentElement.Name) != GenericController.vbLCase(CollectionListRootNode)) {
                        LogController.logInfo(core, "RegisterAddonFolder, Hint=[" + hint + "], The Collections.xml file has an invalid root node, [" + Doc.DocumentElement.Name + "] was received and [" + CollectionListRootNode + "] was expected.");
                    } else {
                        //
                        LogController.logInfo(core, "Collection.xml root name ok");
                        //
                        if (true) {
                            //If genericController.vbLCase(.name) <> "collectionlist" Then
                            //    Call AppendClassLog(core,"Server", "", "RegisterAddonFolder, basename was not collectionlist, [" & .name & "].")
                            //Else
                            NodeCnt = 0;
                            RegisterPathList = "";
                            foreach (XmlNode LocalListNode in Doc.DocumentElement.ChildNodes) {
                                //
                                // Get the collection path
                                //
                                CollectionPath = "";
                                LocalGuid = "";
                                LocalName = "no name found";
                                switch (GenericController.vbLCase(LocalListNode.Name)) {
                                    case "collection":
                                        LocalGuid = "";
                                        foreach (XmlNode CollectionNode in LocalListNode.ChildNodes) {
                                            switch (GenericController.vbLCase(CollectionNode.Name)) {
                                                case "name":
                                                    //
                                                    LocalName = GenericController.vbLCase(CollectionNode.InnerText);
                                                    break;
                                                case "guid":
                                                    //
                                                    LocalGuid = GenericController.vbLCase(CollectionNode.InnerText);
                                                    break;
                                                case "path":
                                                    //
                                                    CollectionPath = GenericController.vbLCase(CollectionNode.InnerText);
                                                    break;
                                                case "lastchangedate":
                                                    LastChangeDate = GenericController.encodeDate(CollectionNode.InnerText);
                                                    break;
                                            }
                                        }
                                        break;
                                }
                                //
                                LogController.logInfo(core, "Node[" + NodeCnt + "], LocalName=[" + LocalName + "], LastChangeDate=[" + LastChangeDate + "], CollectionPath=[" + CollectionPath + "], LocalGuid=[" + LocalGuid + "]");
                                //
                                // Go through all subpaths of the collection path, register the version match, unregister all others
                                //
                                //fs = New fileSystemClass
                                if (string.IsNullOrEmpty(CollectionPath)) {
                                    //
                                    LogController.logInfo(core, "no collection path, skipping");
                                    //
                                } else {
                                    CollectionPath = GenericController.vbLCase(CollectionPath);
                                    CollectionRootPath = CollectionPath;
                                    Pos = CollectionRootPath.LastIndexOf("\\") + 1;
                                    if (Pos <= 0) {
                                        //
                                        LogController.logInfo(core, "CollectionPath has no '\\', skipping");
                                        //
                                    } else {
                                        CollectionRootPath = CollectionRootPath.Left(Pos - 1);
                                        Path = core.addon.getPrivateFilesAddonPath() + CollectionRootPath + "\\";
                                        List<FolderDetail> folderList = new List<FolderDetail>();
                                        if (core.privateFiles.pathExists(Path)) {
                                            folderList = core.privateFiles.getFolderList(Path);
                                        }
                                        if (folderList.Count == 0) {
                                            //
                                            LogController.logInfo(core, "no subfolders found in physical path [" + Path + "], skipping");
                                            //
                                        } else {
                                            int folderPtr = -1;
                                            foreach (FolderDetail dir in folderList) {
                                                folderPtr += 1;
                                                //
                                                // -- check for empty foler name
                                                if (string.IsNullOrEmpty(dir.Name)) {
                                                    //
                                                    LogController.logInfo(core, "....empty folder skipped [" + dir.Name + "]");
                                                    continue;
                                                }
                                                //
                                                // -- preserve folder in use
                                                if (CollectionRootPath + "\\" + dir.Name == CollectionPath) {
                                                    LogController.logInfo(core, "....active folder preserved [" + dir.Name + "]");
                                                    continue;
                                                }
                                                //
                                                // preserve last three folders
                                                if (folderPtr >= (folderList.Count - 3)) {
                                                    LogController.logInfo(core, "....last 3 folders reserved [" + dir.Name + "]");
                                                    continue;
                                                }
                                                //
                                                LogController.logInfo(core, "....Deleting unused folder [" + Path + dir.Name + "]");
                                                core.privateFiles.deleteFolder(Path + dir.Name);
                                            }
                                        }
                                    }
                                }
                                NodeCnt = NodeCnt + 1;
                            }
                        }
                    }
                }
                //
                LogController.logInfo(core, "Exiting RegisterAddonFolder");
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
        }
        //
    }
}