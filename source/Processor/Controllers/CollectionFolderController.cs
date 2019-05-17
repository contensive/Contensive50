
using System;
using System.Xml;
using System.Collections.Generic;
using Contensive.Processor.Models.Db;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using System.IO;
using System.Data;
using System.Threading;
using Contensive.Processor.Models.Domain;
using System.Linq;
using static Contensive.BaseClasses.CPFileSystemBaseClass;
using Contensive.Processor.Exceptions;
using Contensive.BaseClasses;
using System.Reflection;
using NLog;

namespace Contensive.Processor.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// manage the collection folder. The collection folder is in private files. This is where collections are installed, and where addon assemblys are run.
    /// </summary>
    public class CollectionFolderController {
        /// <summary>
        /// class logger initialization
        /// </summary>
        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();
        //
        //====================================================================================================
        /// <summary>
        /// Build collection folders for all collection files in a privatefiles folder
        /// - enumerate zip files in a folder and call buildCollectionFolderFromCollectionZip
        /// </summary>
        /// <param name="core"></param>
        /// <param name="sourcePrivateFolderPath"></param>
        /// <param name="CollectionLastChangeDate"></param>
        /// <param name="collectionsToInstall">A list of collection guids that need to be installed in the database. Any collection folders built are added to he collectionsToInstall list.</param>
        /// <param name="return_ErrorMessage"></param>
        /// <param name="collectionsInstalledList"></param>
        /// <param name="collectionsBuildingFolder">list of collection guids in the process of folder building. use to block recursive loop.</param>
        /// <returns></returns>
        public static bool buildCollectionFoldersFromCollectionZips(CoreController core, Stack<string> contextLog, string sourcePrivateFolderPath, DateTime CollectionLastChangeDate, ref List<string> collectionsToInstall, ref string return_ErrorMessage, ref List<string> collectionsInstalledList, ref List<string> collectionsBuildingFolder) {
            bool success = false;
            try {
                //
                contextLog.Push(MethodInfo.GetCurrentMethod().Name + ", [" + sourcePrivateFolderPath + "]");
                traceContextLog(core, contextLog);
                //
                if (core.privateFiles.pathExists(sourcePrivateFolderPath)) {
                    LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", BuildLocalCollectionFolder, processing files in private folder [" + sourcePrivateFolderPath + "]");
                    List<CPFileSystemClass.FileDetail> SrcFileNamelist = core.privateFiles.getFileList(sourcePrivateFolderPath);
                    foreach (CPFileSystemClass.FileDetail file in SrcFileNamelist) {
                        if ((file.Extension == ".zip") || (file.Extension == ".xml")) {
                            success = buildCollectionFolderFromCollectionZip(core, contextLog, sourcePrivateFolderPath + file.Name, CollectionLastChangeDate, ref return_ErrorMessage, ref collectionsToInstall, ref collectionsInstalledList, ref collectionsBuildingFolder);
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            } finally {
                contextLog.Pop();
            }
            return success;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Builds collection folders for a collectionZip file
        /// unzip a folder and if the collection is not in the collections installed or the collectionsToInstall, save the collection to the appropriate collection folder and add it to the collectionsToInstall
        /// </summary>
        /// <param name="core"></param>
        /// <param name="sourcePrivateFolderPathFilename"></param>
        /// <param name="CollectionLastChangeDate"></param>
        /// <param name="collectionGuid"></param>
        /// <param name="return_ErrorMessage"></param>
        /// <param name="collectionsDownloaded">collection guids that have been saved to the collection folder and need to be saved to the database druing this install.</param>
        /// <param name="collectionsInstalledList">collection guids that have been saved to the database during this install.</param>
        /// <param name="collectionsBuildingFolder">folder building is recursive. These are the collection guids whose folders are currently being built.</param>
        /// <returns></returns>
        public static bool buildCollectionFolderFromCollectionZip(CoreController core, Stack<string> contextLog, string sourcePrivateFolderPathFilename, DateTime CollectionLastChangeDate, ref string return_ErrorMessage, ref List<string> collectionsDownloaded, ref List<string> collectionsInstalledList, ref List<string> collectionsBuildingFolder) {
            try {
                //
                contextLog.Push(MethodInfo.GetCurrentMethod().Name + ", [" + sourcePrivateFolderPathFilename + "]");
                traceContextLog(core, contextLog);
                //
                string collectionPath = "";
                string collectionFilename = "";
                core.privateFiles.splitDosPathFilename(sourcePrivateFolderPathFilename, ref collectionPath, ref collectionFilename);
                string CollectionVersionFolderName = "";
                if (!core.privateFiles.pathExists(collectionPath)) {
                    //
                    // The working folder is not there
                    return_ErrorMessage = "<p>There was a problem with the installation. The installation folder is not valid.</p>";
                    LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", BuildLocalCollectionFolder, CheckFileFolder was false for the private folder [" + collectionPath + "]");
                } else {
                    LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", BuildLocalCollectionFolder, processing files in private folder [" + collectionPath + "]");
                    //
                    // move collection file to a temp directory
                    //
                    string tmpInstallPath = "tmpInstallCollection" + GenericController.getGUIDNaked() + "\\";
                    core.privateFiles.copyFile(sourcePrivateFolderPathFilename, tmpInstallPath + collectionFilename);
                    if (collectionFilename.ToLowerInvariant().Substring(collectionFilename.Length - 4) == ".zip") {
                        core.privateFiles.UnzipFile(tmpInstallPath + collectionFilename);
                        core.privateFiles.deleteFile(tmpInstallPath + collectionFilename);
                    }
                    {
                        //
                        // -- find xml file in temp folder and process it
                        bool CollectionFileFound = false;
                        foreach (FileDetail file in core.privateFiles.getFileList(tmpInstallPath)) {
                            if (file.Name.Substring(file.Name.Length - 4).ToLower() == ".xml") {
                                //
                                LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", build collection folder for Collection file [" + file.Name + "]");
                                //
                                XmlDocument CollectionFile = new XmlDocument();
                                //hint = hint & ",320"
                                CollectionFile = new XmlDocument();
                                bool loadOk = true;
                                try {
                                    CollectionFile.LoadXml(core.privateFiles.readFileText(tmpInstallPath + file.Name));
                                } catch (Exception ex) {
                                    //
                                    // There was a parse error in this xml file. Set the return message and the flag
                                    // If another xml files shows up, and process OK it will cover this error
                                    //
                                    //hint = hint & ",330"
                                    return_ErrorMessage = "There was a problem installing the Collection File [" + tmpInstallPath + file.Name + "]. The error reported was [" + ex.Message + "].";
                                    LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", BuildLocalCollectionFolder, error reading collection [" + sourcePrivateFolderPathFilename + "]");
                                    //StatusOK = False
                                    loadOk = false;
                                }
                                if (loadOk) {
                                    //hint = hint & ",400"
                                    string CollectionFileBaseName = GenericController.vbLCase(CollectionFile.DocumentElement.Name);
                                    if ((CollectionFileBaseName != "contensivecdef") && (CollectionFileBaseName != CollectionFileRootNode) && (CollectionFileBaseName != GenericController.vbLCase(CollectionFileRootNodeOld))) {
                                        //
                                        // Not a problem, this is just not a collection file
                                        //
                                        LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", BuildLocalCollectionFolder, xml base name wrong [" + CollectionFileBaseName + "]");
                                    } else {
                                        bool IsFound = false;
                                        //
                                        // Collection File
                                        //
                                        string Collectionname = XmlController.GetXMLAttribute(core, IsFound, CollectionFile.DocumentElement, "name", "");
                                        string collectionGuid = XmlController.GetXMLAttribute(core, IsFound, CollectionFile.DocumentElement, "guid", Collectionname);
                                        if ((!collectionsInstalledList.Contains(collectionGuid.ToLower())) && (!collectionsDownloaded.Contains(collectionGuid.ToLower()))) {
                                            if (string.IsNullOrEmpty(Collectionname)) {
                                                //
                                                // ----- Error condition -- it must have a collection name
                                                //
                                                return_ErrorMessage = "<p>There was a problem with this Collection. The collection file does not have a collection name.</p>";
                                                LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", BuildLocalCollectionFolder, collection has no name");
                                            } else {
                                                //
                                                //------------------------------------------------------------------
                                                // Build Collection folder structure in /Add-ons folder
                                                //------------------------------------------------------------------
                                                //
                                                collectionsDownloaded.Add(collectionGuid.ToLower());
                                                CollectionFileFound = true;
                                                if (string.IsNullOrEmpty(collectionGuid)) {
                                                    //
                                                    // must have a guid
                                                    collectionGuid = Collectionname;
                                                }
                                                //
                                                //
                                                CollectionVersionFolderName = verifyCollectionVersionFolderName(core, collectionGuid, Collectionname);
                                                string CollectionVersionFolder = core.addon.getPrivateFilesAddonPath() + CollectionVersionFolderName;
                                                //
                                                core.privateFiles.copyFolder(tmpInstallPath, CollectionVersionFolder);
                                                //
                                                foreach (XmlNode metaDataSection in CollectionFile.DocumentElement.ChildNodes) {
                                                    string ChildCollectionGUID = null;
                                                    string ChildCollectionName = null;
                                                    bool Found = false;
                                                    switch (GenericController.vbLCase(metaDataSection.Name)) {
                                                        case "resource":
                                                            break;
                                                        case "getcollection":
                                                        case "importcollection":
                                                            //
                                                            // -- Download Collection file into install folder
                                                            ChildCollectionName = XmlController.GetXMLAttribute(core, Found, metaDataSection, "name", "");
                                                            ChildCollectionGUID = XmlController.GetXMLAttribute(core, Found, metaDataSection, "guid", metaDataSection.InnerText);
                                                            if (string.IsNullOrEmpty(ChildCollectionGUID)) {
                                                                ChildCollectionGUID = metaDataSection.InnerText;
                                                            }
                                                            ChildCollectionGUID = GenericController.normalizeGuid(ChildCollectionGUID);
                                                            string statusMsg = "Installing collection [" + ChildCollectionName + ", " + ChildCollectionGUID + "] referenced from collection [" + Collectionname + "]";
                                                            LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", BuildLocalCollectionFolder, getCollection or importcollection, childCollectionName [" + ChildCollectionName + "], childCollectionGuid [" + ChildCollectionGUID + "]");
                                                            if (GenericController.vbInstr(1, CollectionVersionFolder, ChildCollectionGUID, 1) == 0) {
                                                                if (string.IsNullOrEmpty(ChildCollectionGUID)) {
                                                                    //
                                                                    // -- Needs a GUID to install
                                                                    return_ErrorMessage = statusMsg + ". The installation can not continue because an imported collection could not be downloaded because it does not include a valid GUID.";
                                                                    LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", BuildLocalCollectionFolder, return message [" + return_ErrorMessage + "]");
                                                                } else {
                                                                    if ((!collectionsBuildingFolder.Contains(ChildCollectionGUID)) && (!collectionsDownloaded.Contains(ChildCollectionGUID)) && (!collectionsInstalledList.Contains(ChildCollectionGUID))) {
                                                                        //
                                                                        // -- add to the list of building folders to block recursive loop
                                                                        collectionsBuildingFolder.Add(ChildCollectionGUID);
                                                                        LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", BuildLocalCollectionFolder, [" + ChildCollectionGUID + "], not found so needs to be installed");
                                                                        //
                                                                        // If it is not already installed, download and install it also
                                                                        //
                                                                        string ChildWorkingPath = CollectionVersionFolder + "\\" + ChildCollectionGUID + "\\";
                                                                        DateTime ChildCollectionLastChangeDate = default(DateTime);
                                                                        //
                                                                        // down an imported collection file
                                                                        //
                                                                        if (!CollectionLibraryController.downloadCollectionFromLibrary(core, ChildWorkingPath, ChildCollectionGUID, ref ChildCollectionLastChangeDate, ref return_ErrorMessage)) {

                                                                            LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", BuildLocalCollectionFolder, [" + statusMsg + "], downloadCollectionFiles returned error state, message [" + return_ErrorMessage + "]");
                                                                            if (string.IsNullOrEmpty(return_ErrorMessage)) {
                                                                                return_ErrorMessage = statusMsg + ". The installation can not continue because there was an unknown error while downloading the necessary collection file, [" + ChildCollectionGUID + "].";
                                                                            } else {
                                                                                return_ErrorMessage = statusMsg + ". The installation can not continue because there was an error while downloading the necessary collection file, guid [" + ChildCollectionGUID + "]. The error was [" + return_ErrorMessage + "]";
                                                                            }
                                                                        } else {
                                                                            LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", BuildLocalCollectionFolder, [" + ChildCollectionGUID + "], downloadCollectionFiles returned OK");
                                                                            //
                                                                            // install the downloaded file
                                                                            //
                                                                            if (!buildCollectionFoldersFromCollectionZips(core, contextLog, ChildWorkingPath, ChildCollectionLastChangeDate, ref collectionsDownloaded, ref return_ErrorMessage, ref collectionsInstalledList, ref collectionsBuildingFolder)) {
                                                                                LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", BuildLocalCollectionFolder, [" + statusMsg + "], BuildLocalCollectionFolder returned error state, message [" + return_ErrorMessage + "]");
                                                                                if (string.IsNullOrEmpty(return_ErrorMessage)) {
                                                                                    return_ErrorMessage = statusMsg + ". The installation can not continue because there was an unknown error installing the included collection file, guid [" + ChildCollectionGUID + "].";
                                                                                } else {
                                                                                    return_ErrorMessage = statusMsg + ". The installation can not continue because there was an unknown error installing the included collection file, guid [" + ChildCollectionGUID + "]. The error was [" + return_ErrorMessage + "]";
                                                                                }
                                                                            }
                                                                        }
                                                                        //
                                                                        // -- remove child installation working folder
                                                                        core.privateFiles.deleteFolder(ChildWorkingPath);
                                                                        //
                                                                        // -- no longer building this folder
                                                                        collectionsBuildingFolder.Remove(ChildCollectionGUID);
                                                                    }
                                                                }
                                                            }
                                                            break;
                                                    }
                                                    if (!string.IsNullOrEmpty(return_ErrorMessage)) { break; }
                                                }
                                            }
                                        }
                                        //
                                        // If the collection parsed correctly, update the Collections.xml file
                                        //
                                        if (string.IsNullOrEmpty(return_ErrorMessage)) {
                                            updateCollectionFolderConfig(core, Collectionname, collectionGuid, CollectionLastChangeDate, CollectionVersionFolderName);
                                        } else {
                                            //
                                            // there was an error processing the collection, be sure to save description in the log
                                            LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", BuildLocalCollectionFolder, ERROR Exiting, ErrorMessage [" + return_ErrorMessage + "]");
                                        }

                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(return_ErrorMessage)) { break; }
                        }
                        if ((string.IsNullOrEmpty(return_ErrorMessage)) && (!CollectionFileFound)) {
                            bool ZipFileFound = false;
                            //
                            // no errors, but the collection file was not found
                            if (ZipFileFound) {
                                //
                                // zip file found but did not qualify
                                return_ErrorMessage = "<p>There was a problem with the installation. The collection zip file was downloaded, but it did not include a valid collection xml file.</p>";
                            } else {
                                //
                                // zip file not found
                                return_ErrorMessage = "<p>There was a problem with the installation. The collection zip was not downloaded successfully.</p>";
                            }
                        }
                    }
                    //
                    // delete the working folder
                    core.privateFiles.deleteFolder(tmpInstallPath);
                }
                //
                LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", BuildLocalCollectionFolder, Exiting with ErrorMessage [" + return_ErrorMessage + "]");
                //
                return string.IsNullOrEmpty(return_ErrorMessage);
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            } finally {
                contextLog.Pop();
            }
        }
        //
        //====================================================================================================
        //
        public static void updateCollectionFolderConfig(CoreController core, string Collectionname, string CollectionGuid, DateTime CollectionUpdatedDate, string CollectionVersionFolderName) {
            try {
                //
                bool loadOK = true;
                string LocalFilename = null;
                string LocalGuid = null;
                XmlDocument Doc = new XmlDocument();
                XmlNode NewCollectionNode = null;
                XmlNode NewAttrNode = null;
                bool CollectionFound = false;
                //
                loadOK = true;
                try {
                    Doc.LoadXml(CollectionFolderModel.getCollectionFolderConfigXml(core));
                } catch (Exception) {
                    LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", UpdateConfig, Error loading Collections.xml file.");
                }
                if (loadOK) {
                    if (GenericController.vbLCase(Doc.DocumentElement.Name) != GenericController.vbLCase(CollectionListRootNode)) {
                        LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", UpdateConfig, The Collections.xml file has an invalid root node, [" + Doc.DocumentElement.Name + "] was received and [" + CollectionListRootNode + "] was expected.");
                    } else {
                        if (GenericController.vbLCase(Doc.DocumentElement.Name) == "collectionlist") {
                            CollectionFound = false;
                            foreach (XmlNode LocalListNode in Doc.DocumentElement.ChildNodes) {
                                switch (GenericController.vbLCase(LocalListNode.Name)) {
                                    case "collection":
                                        LocalGuid = "";
                                        foreach (XmlNode CollectionNode in LocalListNode.ChildNodes) {
                                            switch (GenericController.vbLCase(CollectionNode.Name)) {
                                                case "guid":
                                                    //
                                                    LocalGuid = GenericController.vbLCase(CollectionNode.InnerText);
                                                    goto ExitLabel1;
                                            }
                                        }
                                        ExitLabel1:
                                        if (GenericController.vbLCase(LocalGuid) == GenericController.vbLCase(CollectionGuid)) {
                                            CollectionFound = true;
                                            foreach (XmlNode CollectionNode in LocalListNode.ChildNodes) {
                                                switch (GenericController.vbLCase(CollectionNode.Name)) {
                                                    case "name":
                                                        CollectionNode.InnerText = Collectionname;
                                                        break;
                                                    case "lastchangedate":
                                                        CollectionNode.InnerText = CollectionUpdatedDate.ToString();
                                                        break;
                                                    case "path":
                                                        CollectionNode.InnerText = CollectionVersionFolderName;
                                                        break;
                                                }
                                            }
                                            goto ExitLabel2;
                                        }
                                        break;
                                }
                            }
                            ExitLabel2:
                            if (!CollectionFound) {
                                NewCollectionNode = Doc.CreateNode(XmlNodeType.Element, "collection", "");
                                //
                                NewAttrNode = Doc.CreateNode(XmlNodeType.Element, "name", "");
                                NewAttrNode.InnerText = Collectionname;
                                NewCollectionNode.AppendChild(NewAttrNode);
                                //
                                NewAttrNode = Doc.CreateNode(XmlNodeType.Element, "lastchangedate", "");
                                NewAttrNode.InnerText = CollectionUpdatedDate.ToString();
                                NewCollectionNode.AppendChild(NewAttrNode);
                                //
                                NewAttrNode = Doc.CreateNode(XmlNodeType.Element, "guid", "");
                                NewAttrNode.InnerText = CollectionGuid;
                                NewCollectionNode.AppendChild(NewAttrNode);
                                //
                                NewAttrNode = Doc.CreateNode(XmlNodeType.Element, "path", "");
                                NewAttrNode.InnerText = CollectionVersionFolderName;
                                NewCollectionNode.AppendChild(NewAttrNode);
                                //
                                Doc.DocumentElement.AppendChild(NewCollectionNode);
                            }
                            //
                            // Save the result
                            //
                            LocalFilename = core.addon.getPrivateFilesAddonPath() + "Collections.xml";
                            //LocalFilename = GetProgramPath & "\Addons\Collections.xml"
                            core.privateFiles.saveFile(LocalFilename, Doc.OuterXml);
                            //Doc.Save(core.privateFiles.localAbsRootPath + LocalFilename);
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Get the collection folder stored in the collection config file (xml file at root of the collection folder)
        /// </summary>
        /// <param name="core"></param>
        /// <param name="CollectionGuid"></param>
        /// <returns></returns>
        public static string GetCollectionConfigFolderPath(CoreController core, string CollectionGuid) {
            var collectionFolder = CollectionFolderModel.getCollectionFolderConfig(core, CollectionGuid);
            if (collectionFolder != null) { return collectionFolder.path; }
            return string.Empty;
        }
        //
        //======================================================================================================
        /// <summary>
        /// determine or create a collection version path (/private/addons/collectionFolder/collectionVersion) 
        /// </summary>
        /// <param name="core"></param>
        /// <param name="collectionGuid"></param>
        /// <param name="CollectionName"></param>
        /// <returns></returns>
        public static string verifyCollectionVersionFolderName(CoreController core, string collectionGuid, string CollectionName) {
            collectionGuid = GenericController.normalizeGuid(collectionGuid);
            string CollectionVersionFolderName = GetCollectionConfigFolderPath(core, collectionGuid);
            string CollectionFolderName = "";
            if (!string.IsNullOrEmpty(CollectionVersionFolderName)) {
                //
                // This is an upgrade
                //
                int Pos = GenericController.vbInstr(1, CollectionVersionFolderName, "\\");
                if (Pos > 0) {
                    CollectionFolderName = CollectionVersionFolderName.Left(Pos - 1);
                }
            } else {
                //
                // This is an install
                //
                //hint = hint & ",460"
                CollectionFolderName = collectionGuid;
                CollectionFolderName = GenericController.vbReplace(CollectionFolderName, "{", "");
                CollectionFolderName = GenericController.vbReplace(CollectionFolderName, "}", "");
                CollectionFolderName = GenericController.vbReplace(CollectionFolderName, "-", "");
                CollectionFolderName = GenericController.vbReplace(CollectionFolderName, " ", "");
                CollectionFolderName = CollectionName + "_" + CollectionFolderName;
                CollectionFolderName = CollectionFolderName.ToLowerInvariant();
            }
            string CollectionFolder = core.addon.getPrivateFilesAddonPath() + CollectionFolderName + "\\";
            core.privateFiles.verifyPath(CollectionFolder);
            //
            // create a collection 'version' folder for these new files
            string TimeStamp = "";
            DateTime NowTime = default(DateTime);
            NowTime = DateTime.Now;
            int NowPart = NowTime.Year;
            TimeStamp += NowPart.ToString();
            NowPart = NowTime.Month;
            if (NowPart < 10) {
                TimeStamp += "0";
            }
            TimeStamp += NowPart.ToString();
            NowPart = NowTime.Day;
            if (NowPart < 10) {
                TimeStamp += "0";
            }
            TimeStamp += NowPart.ToString();
            NowPart = NowTime.Hour;
            if (NowPart < 10) {
                TimeStamp += "0";
            }
            TimeStamp += NowPart.ToString();
            NowPart = NowTime.Minute;
            if (NowPart < 10) {
                TimeStamp += "0";
            }
            TimeStamp += NowPart.ToString();
            NowPart = NowTime.Second;
            if (NowPart < 10) {
                TimeStamp += "0";
            }
            TimeStamp += NowPart.ToString();
            CollectionVersionFolderName = CollectionFolderName + "\\" + TimeStamp;
            string CollectionVersionFolder = core.addon.getPrivateFilesAddonPath() + CollectionVersionFolderName;
            string CollectionVersionPath = CollectionVersionFolder + "\\";
            core.privateFiles.createPath(CollectionVersionPath);
            return CollectionVersionFolderName;
        }
        //
        //======================================================================================================
        /// <summary>
        /// log the contextLog stack
        /// ContextLog stack is a tool to trace the collection installation to trace recursion
        /// </summary>
        /// <param name="core"></param>
        /// <param name="contextLog"></param>
        private static void traceContextLog(CoreController core, Stack<string> contextLog) {
            logger.Log(LogLevel.Info, LogController.getLogMsg(core, string.Join(",", contextLog)));
        }
    }
}
