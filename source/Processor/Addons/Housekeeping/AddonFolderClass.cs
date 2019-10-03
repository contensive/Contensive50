
using System;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using System.Xml;
using System.Collections.Generic;
using static Contensive.BaseClasses.CPFileSystemBaseClass;
using Contensive.Exceptions;
using System.Globalization;

namespace Contensive.Addons.Housekeeping {
    //
    public static class AddonFolderClass {
        //====================================================================================================
        //
        public static void housekeep(CoreController core) {
            try {
                //
                LogController.logInfo(core, "Entering RegisterAddonFolder");
                //
                bool loadOK = true;
                XmlDocument Doc = new XmlDocument();
                string hint = "";
                try {
                    string collectionFileFilename = core.addon.getPrivateFilesAddonPath() + "Collections.xml";
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
                        {
                            //If genericController.vbLCase(.name) <> "collectionlist" Then
                            //    Call AppendClassLog(core,"Server", "", "RegisterAddonFolder, basename was not collectionlist, [" & .name & "].")
                            //Else
                            int NodeCnt = 0;
                            foreach (XmlNode LocalListNode in Doc.DocumentElement.ChildNodes) {
                                //
                                // Get the collection path
                                //
                                string CollectionPath = "";
                                string LocalGuid = "";
                                string LocalName = "no name found";
                                DateTime LastChangeDate = default(DateTime);
                                if (LocalListNode.Name.ToLower().Equals("collection")) {
                                    LocalGuid = "";
                                    foreach (XmlNode CollectionNode in LocalListNode.ChildNodes) {
                                        switch (CollectionNode.Name.ToLower()) {
                                            case "name":
                                                //
                                                LocalName = CollectionNode.InnerText.ToLower();
                                                break;
                                            case "guid":
                                                //
                                                LocalGuid = CollectionNode.InnerText.ToLower();
                                                break;
                                            case "path":
                                                //
                                                CollectionPath = CollectionNode.InnerText.ToLower();
                                                break;
                                            case "lastchangedate":
                                                LastChangeDate = GenericController.encodeDate(CollectionNode.InnerText);
                                                break;
                                            default:
                                                break;
                                        }
                                    }
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
                                    string CollectionRootPath = CollectionPath;
                                    int Pos = CollectionRootPath.LastIndexOf("\\") + 1;
                                    if (Pos <= 0) {
                                        //
                                        LogController.logInfo(core, "CollectionPath has no '\\', skipping");
                                        //
                                    } else {
                                        CollectionRootPath = CollectionRootPath.Left(Pos - 1);
                                        string Path = core.addon.getPrivateFilesAddonPath() + CollectionRootPath + "\\";
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