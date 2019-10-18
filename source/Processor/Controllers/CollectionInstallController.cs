﻿
using System;
using System.Xml;
using System.Collections.Generic;

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
using Contensive.Models.Db;
using System.Globalization;

namespace Contensive.Processor.Controllers {
    //
    // install = means everything nessesary
    // buildfolder = means download and build out site
    //
    // todo: rework how adds are installed, this change can be done after weave launch
    // - current addon folder is called local addon folder and not in shared environment /local/addons
    // - add a node to the (local) collection.xml with last collection installation datetime (files added after this starts install)
    // - in private files, new folder with zip files to install /private/collectionInstall
    // - local server checks the list and runs install on new zips, if remote file system, download and install
    // - addon manager just copies zip file into the /private/collectionInstall folder
    //
    // todo -- To make it easy to add code to a site, be able to upload DLL files. Get the class names, find the collection and install in the correct collection folder
    //
    // todo -- Even in collection files, auto discover DLL file classes and create addons out of them. Create/update collections, create collection xml and install.
    //
    //====================================================================================================
    /// <summary>
    /// install addon collections.
    /// </summary>
    public class CollectionInstallController {
        /// <summary>
        /// class logger initialization
        /// </summary>
        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();
        //
        //======================================================================================================
        /// <summary>
        /// Install the base collection to this applicaiton
        /// copy the base collection from the program files folder to a private folder
        /// calls installCollectionFromFile
        /// </summary>
        /// <param name="core"></param>
        /// <param name="contextLog">For logging. A list of reasons why this installation was called, the last explaining this call, the one before explain the reason the caller was installed.</param>
        /// <param name="isNewBuild"></param>
        /// <param name="reinstallDependencies"></param>
        /// <param name="nonCriticalErrorList"></param>
        /// <param name="logPrefix"></param>
        /// <param name="collectionsInstalledList">A list of collection guids that are already installed this pass. All collections that install will be added to it. </param>
        public static void installBaseCollection(CoreController core, Stack<string> contextLog, bool isNewBuild, bool reinstallDependencies, ref List<string> nonCriticalErrorList, string logPrefix) {
            try {
                contextLog.Push("installBaseCollection");
                traceContextLog(core, contextLog);
                //
                // -- new build
                // 20171029 -- upgrading should restore base collection fields as a fix to deleted required fields
                const string baseCollectionFilename = "aoBase51.xml";
                string baseCollectionXml = core.programFiles.readFileText(baseCollectionFilename);
                if (string.IsNullOrEmpty(baseCollectionXml)) {
                    //
                    // -- base collection notfound
                    throw new GenericException("Cannot load [" + core.programFiles.localAbsRootPath + "aoBase51.xml]");
                }
                {
                    //
                    // -- Special Case - must install base collection metadata first because it builds the system that the system needs to do everything else
                    LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", installBaseCollection, install metadata first to verify system requirements");
                    MetadataMiniCollectionModel.installMetaDataMiniCollectionFromXml(true, core, baseCollectionXml, isNewBuild, true, reinstallDependencies, ref nonCriticalErrorList, logPrefix);
                }
                {
                    //
                    // now treat as a regular collection and install - to pickup everything else 
                    string installPrivatePath = "installBaseCollection" + GenericController.GetRandomInteger(core).ToString() + "\\";
                    try {
                        core.privateFiles.createPath(installPrivatePath);
                        core.programFiles.copyFile(baseCollectionFilename, installPrivatePath + baseCollectionFilename, core.privateFiles);
                        string installErrorMessage = "";
                        string installedCollectionGuid = "";
                        var collectionsInstalledList = new List<string>();
                        if (!installCollectionFromPrivateFile(core, contextLog, installPrivatePath + baseCollectionFilename, ref installErrorMessage, ref installedCollectionGuid, isNewBuild, reinstallDependencies, ref nonCriticalErrorList, logPrefix, ref collectionsInstalledList)) {
                            throw new GenericException(installErrorMessage);
                        }
                    } catch (Exception) {
                        throw;
                    } finally {
                        //
                        // -- remove temp folder
                        core.privateFiles.deleteFolder(installPrivatePath);
                        //
                        // -- invalidate cache
                        core.cache.invalidateAll();
                    }
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            } finally {
                contextLog.Pop();
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Primary collection installation method. 
        /// If collection not already installed during this install, mark it installed and install
        /// </summary>
        /// <param name="core"></param>
        /// <param name="contextLog"></param>
        /// <param name="collectionGuid"></param>
        /// <param name="return_ErrorMessage"></param>
        /// <param name="IsNewBuild"></param>
        /// <param name="reinstallDependencies"></param>
        /// <param name="nonCriticalErrorList"></param>
        /// <param name="logPrefix"></param>
        /// <param name="collectionsInstalledList"></param>
        /// <param name="includeBaseMetaDataInstall"></param>
        /// <param name="collectionsDownloaded">Collections downloaded but not installed yet. Do not need to download them again.</param>
        /// <returns></returns>
        public static bool installCollectionFromCollectionFolder(CoreController core, Stack<string> contextLog, string collectionGuid, ref string return_ErrorMessage, bool IsNewBuild, bool reinstallDependencies, ref List<string> nonCriticalErrorList, string logPrefix, ref List<string> collectionsInstalledList, bool includeBaseMetaDataInstall, ref List<string> collectionsDownloaded) {
            bool result = false;
            try {
                //
                contextLog.Push(MethodInfo.GetCurrentMethod().Name + ", [" + collectionGuid + "]");
                traceContextLog(core, contextLog);
                //
                if (collectionsInstalledList.Contains(collectionGuid.ToLower(CultureInfo.InvariantCulture))) {
                    //
                    // -- this collection has already been installed during this installation process. Skip and return success
                    LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", [" + collectionGuid + "] was not installed because it was previously installed during this installation.");
                    //return_ErrorMessage += "<P>The collection was not installed from the local collections because the folder containing the Add-on's resources could not be found. It may not be installed locally.</P>";
                    return true;
                }
                // -- collection needs to be 
                if (!collectionsInstalledList.Contains(collectionGuid.ToLower(CultureInfo.InvariantCulture))) { collectionsInstalledList.Add(collectionGuid.ToLower(CultureInfo.InvariantCulture)); }
                //
                var collectionFolderConfig = CollectionFolderModel.getCollectionFolderConfig(core, collectionGuid);
                if ((collectionFolderConfig == null) || string.IsNullOrEmpty(collectionFolderConfig.path)) {
                    LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", installCollectionFromAddonCollectionFolder [" + collectionGuid + "], collection folder not found.");
                    return_ErrorMessage += "<P>The collection was not installed from the local collections because the folder containing the Add-on's resources could not be found. It may not be installed locally.</P>";
                    return false;
                } else {
                    //
                    // Search Local Collection Folder for collection config file (xml file)
                    //
                    string CollectionVersionFolder = core.addon.getPrivateFilesAddonPath() + collectionFolderConfig.path + "\\";
                    List<FileDetail> srcFileInfoArray = core.privateFiles.getFileList(CollectionVersionFolder);
                    if (srcFileInfoArray.Count == 0) {
                        LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", installCollectionFromAddonCollectionFolder [" + collectionGuid + "], collection folder is empty.");
                        return_ErrorMessage += "<P>The collection was not installed because the folder containing the Add-on's resources was empty.</P>";
                        return false;
                    } else {
                        //
                        // collect list of DLL files and add them to the exec files if they were missed
                        List<string> assembliesInZip = new List<string>();
                        foreach (FileDetail file in srcFileInfoArray) {
                            if (file.Extension.ToLowerInvariant() == "dll") {
                                if (!assembliesInZip.Contains(file.Name.ToLowerInvariant())) {
                                    LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", installCollectionFromAddonCollectionFolder [" + collectionGuid + "], adding DLL from folder[" + file.Name.ToLowerInvariant() + "].");
                                    assembliesInZip.Add(file.Name.ToLowerInvariant());
                                }
                            }
                        }
                        //
                        // -- Process the other files
                        LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", installCollectionFromAddonCollectionFolder [" + collectionGuid + "], process xml files.");
                        bool CollectionblockNavigatorNode_fileValueOK = false;
                        foreach (FileDetail file in srcFileInfoArray) {
                            if (file.Extension == ".xml") {
                                //
                                // -- XML file -- open it to figure out if it is one we can use
                                XmlDocument Doc = new XmlDocument();
                                string CollectionFilename = file.Name;
                                bool loadOK = true;
                                string collectionFileContent = core.privateFiles.readFileText(CollectionVersionFolder + file.Name);
                                try {
                                    Doc.LoadXml(collectionFileContent);
                                } catch (Exception) {
                                    //
                                    // error - Need a way to reach the user that submitted the file
                                    //
                                    LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", installCollectionFromAddonCollectionFolder, skipping xml file, not valid collection metadata, [" + core.privateFiles.localAbsRootPath + CollectionVersionFolder + file.Name + "].");
                                    loadOK = false;
                                }
                                if (loadOK) {
                                    if ((Doc.DocumentElement.Name.ToLowerInvariant() == GenericController.vbLCase(CollectionFileRootNode)) || (Doc.DocumentElement.Name.ToLowerInvariant() == GenericController.vbLCase(CollectionFileRootNodeOld))) {
                                        //
                                        //------------------------------------------------------------------------------------------------------
                                        // Collection File - import from sub so it can be re-entrant
                                        //------------------------------------------------------------------------------------------------------
                                        //
                                        bool IsFound = false;
                                        string CollectionName = XmlController.GetXMLAttribute(core, IsFound, Doc.DocumentElement, "name", "");
                                        if (string.IsNullOrEmpty(CollectionName)) {
                                            //
                                            // ----- Error condition -- it must have a collection name
                                            //
                                            //Call AppendAddonLog("UpgradeAppFromLocalCollection, collection has no name")
                                            LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", installCollectionFromAddonCollectionFolder [" + CollectionName + "], collection has no name");
                                            return_ErrorMessage += "<P>The collection was not installed because the collection name in the xml collection file is blank</P>";
                                            return false;
                                        } else {
                                            bool CollectionSystem_fileValueOK = false;
                                            bool CollectionUpdatable_fileValueOK = false;
                                            //												Dim CollectionblockNavigatorNode_fileValueOK As Boolean
                                            bool CollectionSystem = GenericController.encodeBoolean(XmlController.GetXMLAttribute(core, CollectionSystem_fileValueOK, Doc.DocumentElement, "system", ""));
                                            int Parent_NavId = BuildController.verifyNavigatorEntry(core, new MetadataMiniCollectionModel.MiniCollectionMenuModel() {
                                                Guid = addonGuidManageAddon,
                                                name = "Manage Add-ons",
                                                AdminOnly = false,
                                                DeveloperOnly = false,
                                                NewWindow = false,
                                                Active = true,
                                            }, 0);
                                            bool CollectionUpdatable = GenericController.encodeBoolean(XmlController.GetXMLAttribute(core, CollectionUpdatable_fileValueOK, Doc.DocumentElement, "updatable", ""));
                                            string onInstallAddonGuid = XmlController.GetXMLAttribute(core, CollectionUpdatable_fileValueOK, Doc.DocumentElement, "OnInstallAddonGuid", "");
                                            bool CollectionblockNavigatorNode = GenericController.encodeBoolean(XmlController.GetXMLAttribute(core, CollectionblockNavigatorNode_fileValueOK, Doc.DocumentElement, "blockNavigatorNode", ""));
                                            string FileGuid = XmlController.GetXMLAttribute(core, IsFound, Doc.DocumentElement, "guid", CollectionName);
                                            if (string.IsNullOrEmpty(FileGuid)) {
                                                FileGuid = CollectionName;
                                            }
                                            if (collectionGuid.ToLowerInvariant() != GenericController.vbLCase(FileGuid)) {
                                                //
                                                //
                                                //
                                                LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", installCollectionFromAddonCollectionFolder [" + CollectionName + "], Collection file contains incorrect GUID, correct GUID [" + collectionGuid.ToLowerInvariant() + "], incorrect GUID in file [" + GenericController.vbLCase(FileGuid) + "]");
                                                return_ErrorMessage += "<P>The collection was not installed because the unique number identifying the collection, called the guid, does not match the collection requested.</P>";
                                                return false;
                                            } else {
                                                if (string.IsNullOrEmpty(collectionGuid)) {
                                                    //
                                                    // I hope I do not regret this
                                                    //
                                                    collectionGuid = CollectionName;
                                                }
                                                //
                                                //-------------------------------------------------------------------------------
                                                LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", installCollectionFromAddonCollectionFolder [" + CollectionName + "], stage-1, save resourses and process collection dependencies");
                                                // Go through all collection nodes
                                                // Process ImportCollection Nodes - so includeaddon nodes will work
                                                // these must be processes regardless of the state of this collection in this app
                                                // Get Resource file list
                                                //-------------------------------------------------------------------------------
                                                //
                                                string wwwFileList = "";
                                                string ContentFileList = "";
                                                string ExecFileList = "";
                                                bool collectionIncludesDiagnosticAddon = false;
                                                foreach (XmlNode MetaDataSection in Doc.DocumentElement.ChildNodes) {
                                                    switch (MetaDataSection.Name.ToLowerInvariant()) {
                                                        case "resource": {
                                                                //
                                                                // set wwwfilelist, contentfilelist, execfilelist
                                                                //
                                                                string resourceType = XmlController.GetXMLAttribute(core, IsFound, MetaDataSection, "type", "");
                                                                string resourcePath = XmlController.GetXMLAttribute(core, IsFound, MetaDataSection, "path", "");
                                                                string filename = XmlController.GetXMLAttribute(core, IsFound, MetaDataSection, "name", "");
                                                                //
                                                                LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", installCollectionFromAddonCollectionFolder [" + CollectionName + "], resource found, name [" + filename + "], type [" + resourceType + "], path [" + resourcePath + "]");
                                                                //
                                                                filename = FileController.convertToDosSlash(filename);
                                                                string SrcPath = "";
                                                                string dstPath = resourcePath;
                                                                int Pos = GenericController.vbInstr(1, filename, "\\");
                                                                if (Pos != 0) {
                                                                    //
                                                                    // Source path is in filename
                                                                    //
                                                                    SrcPath = filename.Left(Pos - 1);
                                                                    filename = filename.Substring(Pos);
                                                                    if (string.IsNullOrEmpty(resourcePath)) {
                                                                        //
                                                                        // -- No Resource Path give, use the same folder structure from source
                                                                        dstPath = SrcPath;
                                                                    } else {
                                                                        //
                                                                        // -- Copy file to resource path
                                                                        dstPath = resourcePath;
                                                                    }
                                                                }
                                                                //
                                                                // -- if the filename in the collection file is the wrong case, correct it now
                                                                filename = core.privateFiles.correctFilenameCase(CollectionVersionFolder + SrcPath + filename);
                                                                //
                                                                // == normalize dst
                                                                string dstDosPath = FileController.normalizeDosPath(dstPath);
                                                                //
                                                                // -- 
                                                                switch (resourceType.ToLowerInvariant()) {
                                                                    case "www":
                                                                        wwwFileList += Environment.NewLine + dstDosPath + filename;
                                                                        LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", CollectionName [" + CollectionName + "], GUID [" + collectionGuid + "], pass 1, copying file to www, src [" + CollectionVersionFolder + SrcPath + "], dst [" + core.appConfig.localWwwPath + dstDosPath + "].");
                                                                        core.privateFiles.copyFile(CollectionVersionFolder + SrcPath + filename, dstDosPath + filename, core.wwwFiles);
                                                                        if (GenericController.vbLCase(filename.Substring(filename.Length - 4)) == ".zip") {
                                                                            LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", installCollectionFromAddonCollectionFolder [" + CollectionName + "], GUID [" + collectionGuid + "], pass 1, unzipping www file [" + core.appConfig.localWwwPath + dstDosPath + filename + "].");
                                                                            core.wwwFiles.unzipFile(dstDosPath + filename);
                                                                        }
                                                                        break;
                                                                    case "file":
                                                                    case "content":
                                                                        ContentFileList += Environment.NewLine + dstDosPath + filename;
                                                                        LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", CollectionName [" + CollectionName + "], GUID [" + collectionGuid + "], pass 1, copying file to content, src [" + CollectionVersionFolder + SrcPath + "], dst [" + dstDosPath + "].");
                                                                        core.privateFiles.copyFile(CollectionVersionFolder + SrcPath + filename, dstDosPath + filename, core.cdnFiles);
                                                                        if (GenericController.vbLCase(filename.Substring(filename.Length - 4)) == ".zip") {
                                                                            LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", CollectionName [" + CollectionName + "], GUID [" + collectionGuid + "], pass 1, unzipping content file [" + dstDosPath + filename + "].");
                                                                            core.cdnFiles.unzipFile(dstDosPath + filename);
                                                                        }
                                                                        break;
                                                                    default:
                                                                        if (assembliesInZip.Contains(filename.ToLowerInvariant())) {
                                                                            assembliesInZip.Remove(filename.ToLowerInvariant());
                                                                        }
                                                                        ExecFileList = ExecFileList + Environment.NewLine + filename;
                                                                        break;
                                                                }
                                                                break;
                                                            }
                                                        case "getcollection":
                                                        case "importcollection": {
                                                                //
                                                                // Get path to this collection and call into it
                                                                //
                                                                bool Found = false;
                                                                string ChildCollectionName = XmlController.GetXMLAttribute(core, Found, MetaDataSection, "name", "");
                                                                string ChildCollectionGUId = XmlController.GetXMLAttribute(core, Found, MetaDataSection, "guid", MetaDataSection.InnerText);
                                                                if (string.IsNullOrEmpty(ChildCollectionGUId)) {
                                                                    ChildCollectionGUId = MetaDataSection.InnerText;
                                                                }
                                                                if (collectionsInstalledList.Contains(ChildCollectionGUId.ToLower(CultureInfo.InvariantCulture))) {
                                                                    //
                                                                    // circular import detected, this collection is already imported
                                                                    //
                                                                    LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", installCollectionFromAddonCollectionFolder [" + CollectionName + "], Circular import detected. This collection attempts to import a collection that had previously been imported. A collection can not import itself. The collection is [" + CollectionName + "], GUID [" + collectionGuid + "], pass 1. The collection to be imported is [" + ChildCollectionName + "], GUID [" + ChildCollectionGUId + "]");
                                                                } else {
                                                                    //
                                                                    // -- all included collections should already be installed, because buildfolder is called before call
                                                                    installCollectionFromCollectionFolder(core, contextLog, ChildCollectionGUId, ref return_ErrorMessage, IsNewBuild, reinstallDependencies, ref nonCriticalErrorList, logPrefix, ref collectionsInstalledList, false, ref collectionsDownloaded);
                                                                }
                                                                break;
                                                            }
                                                    }
                                                }
                                                //
                                                // -- any assemblies found in the zip that were not part of the resources section need to be added
                                                foreach (string filename in assembliesInZip) {
                                                    ExecFileList = ExecFileList + Environment.NewLine + filename;
                                                }
                                                //
                                                //-------------------------------------------------------------------------------
                                                LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", installCollectionFromAddonCollectionFolder [" + CollectionName + "], stage-2, determine if this collection is already installed");
                                                //-------------------------------------------------------------------------------
                                                //
                                                bool OKToInstall = false;
                                                AddonCollectionModel collection = AddonCollectionModel.create<AddonCollectionModel>(core.cpParent, collectionGuid);
                                                if (collection != null) {
                                                    //
                                                    // Upgrade addon
                                                    //
                                                    if (collectionFolderConfig.lastChangeDate == DateTime.MinValue) {
                                                        LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", installCollectionFromAddonCollectionFolder [" + CollectionName + "], GUID [" + collectionGuid + "], App has the collection, but the installedDate could not be determined, so it will upgrade.");
                                                        OKToInstall = true;
                                                    } else if (collectionFolderConfig.lastChangeDate > collection.modifiedDate) {
                                                        LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", installCollectionFromAddonCollectionFolder [" + CollectionName + "], GUID [" + collectionGuid + "], App has an older version of collection. It will be upgraded.");
                                                        OKToInstall = true;
                                                    } else if (reinstallDependencies) {
                                                        LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", installCollectionFromAddonCollectionFolder [" + CollectionName + "], GUID [" + collectionGuid + "], App has an up-to-date version of collection, but the repair option is true so it will be reinstalled.");
                                                        OKToInstall = true;
                                                    } else {
                                                        LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", installCollectionFromAddonCollectionFolder [" + CollectionName + "], GUID [" + collectionGuid + "], App has an up-to-date version of collection. It will not be upgraded, but all imports in the new version will be checked.");
                                                        OKToInstall = false;
                                                    }
                                                } else {
                                                    //
                                                    // Install new on this application
                                                    //
                                                    collection = AddonCollectionModel.addEmpty<AddonCollectionModel>(core.cpParent);
                                                    LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", installCollectionFromAddonCollectionFolder [" + CollectionName + "], GUID [" + collectionGuid + "], App does not have this collection so it will be installed.");
                                                    OKToInstall = true;
                                                }
                                                string DataRecordList = "";
                                                if (!OKToInstall) {
                                                    //
                                                    // Do not install, but still check all imported collections to see if they need to be installed
                                                    // imported collections moved in front this check
                                                    //
                                                } else {
                                                    //
                                                    //-------------------------------------------------------------------------------
                                                    LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", installCollectionFromAddonCollectionFolder [" + CollectionName + "], stage-3, prepare to import full collection");
                                                    //-------------------------------------------------------------------------------
                                                    //
                                                    {
                                                        string CollectionHelpLink = "";
                                                        foreach (XmlNode metaDataSection in Doc.DocumentElement.ChildNodes) {
                                                            if (metaDataSection.Name.ToLowerInvariant() == "helplink") {
                                                                //
                                                                // only save the first
                                                                CollectionHelpLink = metaDataSection.InnerText;
                                                                break;
                                                            }
                                                        }
                                                        //
                                                        // ----- set or clear all fields
                                                        collection.name = CollectionName;
                                                        collection.help = "";
                                                        collection.ccguid = collectionGuid;
                                                        collection.lastChangeDate = collectionFolderConfig.lastChangeDate;
                                                        if (CollectionSystem_fileValueOK) {
                                                            collection.system = CollectionSystem;
                                                        }
                                                        if (CollectionUpdatable_fileValueOK) {
                                                            collection.updatable = CollectionUpdatable;
                                                        }
                                                        if (CollectionblockNavigatorNode_fileValueOK) {
                                                            collection.blockNavigatorNode = CollectionblockNavigatorNode;
                                                        }
                                                        collection.helpLink = CollectionHelpLink;
                                                        //
                                                        MetadataController.deleteContentRecords(core, "Add-on Collection CDef Rules", "CollectionID=" + collection.id);
                                                        MetadataController.deleteContentRecords(core, "Add-on Collection Parent Rules", "ParentID=" + collection.id);
                                                        //
                                                        // Store all resource found, new way and compatibility way
                                                        //
                                                        collection.contentFileList = ContentFileList;
                                                        collection.execFileList = ExecFileList;
                                                        collection.wwwFileList = wwwFileList;
                                                        //
                                                        // ----- remove any current navigator nodes installed by the collection previously
                                                        //
                                                        if (collection.id != 0) {
                                                            MetadataController.deleteContentRecords(core, NavigatorEntryModel.tableMetadata.contentName, "installedbycollectionid=" + collection.id);
                                                        }
                                                        collection.save(core.cpParent);
                                                    }
                                                    //
                                                    //-------------------------------------------------------------------------------
                                                    LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", installCollectionFromAddonCollectionFolder [" + CollectionName + "], stage-4, isolate and process schema-relatednodes (metadata,index,etc)");
                                                    //-------------------------------------------------------------------------------
                                                    //
                                                    bool isBaseCollection = (baseCollectionGuid.ToLowerInvariant() == collectionGuid.ToLowerInvariant());
                                                    if (!isBaseCollection || includeBaseMetaDataInstall) {
                                                        string metaDataMiniCollection = "";
                                                        foreach (XmlNode metaDataSection in Doc.DocumentElement.ChildNodes) {
                                                            switch (metaDataSection.Name.ToLower(CultureInfo.InvariantCulture)) {
                                                                case "contensivecdef":
                                                                    //
                                                                    // old metadata section -- take the inner
                                                                    //
                                                                    foreach (XmlNode ChildNode in metaDataSection.ChildNodes) {
                                                                        metaDataMiniCollection += Environment.NewLine + ChildNode.OuterXml;
                                                                    }
                                                                    break;
                                                                case "cdef":
                                                                case "sqlindex":
                                                                case "style":
                                                                case "styles":
                                                                case "stylesheet":
                                                                case "adminmenu":
                                                                case "menuentry":
                                                                case "navigatorentry":
                                                                    //
                                                                    // handled by Upgrade class
                                                                    metaDataMiniCollection += metaDataSection.OuterXml;
                                                                    break;
                                                            }
                                                        }
                                                        //
                                                        // -- install metadataMiniCollection
                                                        if (!string.IsNullOrEmpty(metaDataMiniCollection)) {
                                                            //
                                                            // -- Use the upgrade code to import this part
                                                            metaDataMiniCollection = "<" + CollectionFileRootNode + ">" + metaDataMiniCollection + "</" + CollectionFileRootNode + ">";
                                                            MetadataMiniCollectionModel.installMetaDataMiniCollectionFromXml(false, core, metaDataMiniCollection, IsNewBuild, reinstallDependencies, isBaseCollection, ref nonCriticalErrorList, logPrefix);
                                                            //
                                                            // -- Process nodes to save Collection data
                                                            XmlDocument NavDoc = new XmlDocument();
                                                            loadOK = true;
                                                            try {
                                                                NavDoc.LoadXml(metaDataMiniCollection);
                                                            } catch (Exception) {
                                                                //
                                                                // error - Need a way to reach the user that submitted the file
                                                                //
                                                                LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", installCollectionFromAddonCollectionFolder [" + CollectionName + "], creating navigator entries, there was an error parsing the portion of the collection that contains metadata. Navigator entry creation was aborted. [There was an error reading the Meta data file.]");
                                                                result = false;
                                                                return_ErrorMessage += "<P>The collection was not installed because the xml collection file has an error.</P>";
                                                                return false;
                                                                //loadOK = false;
                                                            }
                                                            if (loadOK) {
                                                                foreach (XmlNode metaDataNode in NavDoc.DocumentElement.ChildNodes) {
                                                                    switch (GenericController.vbLCase(metaDataNode.Name)) {
                                                                        case "cdef":
                                                                            string ContentName = XmlController.GetXMLAttribute(core, IsFound, metaDataNode, "name", "");
                                                                            //
                                                                            // setup metadata rule
                                                                            //
                                                                            int ContentId = ContentMetadataModel.getContentId(core, ContentName);
                                                                            if (ContentId > 0) {
                                                                                using (var csData = new CsModel(core)) {
                                                                                    csData.insert("Add-on Collection CDef Rules");
                                                                                    if (csData.ok()) {
                                                                                        csData.set("Contentid", ContentId);
                                                                                        csData.set("CollectionID", collection.id);
                                                                                    }
                                                                                }
                                                                            }
                                                                            break;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    //
                                                    //-------------------------------------------------------------------------------
                                                    LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", installCollectionFromAddonCollectionFolder [" + CollectionName + "], stage-5, create data records from data nodes, ignore fields");
                                                    //-------------------------------------------------------------------------------
                                                    //
                                                    {
                                                        foreach (XmlNode metaDataSection in Doc.DocumentElement.ChildNodes) {
                                                            switch (GenericController.vbLCase(metaDataSection.Name)) {
                                                                case "data": {
                                                                        //
                                                                        // import content
                                                                        //   This can only be done with matching guid
                                                                        //
                                                                        foreach (XmlNode ContentNode in metaDataSection.ChildNodes) {
                                                                            if (GenericController.vbLCase(ContentNode.Name) == "record") {
                                                                                //
                                                                                // Data.Record node
                                                                                //
                                                                                string ContentName = XmlController.GetXMLAttribute(core, IsFound, ContentNode, "content", "");
                                                                                if (string.IsNullOrEmpty(ContentName)) {
                                                                                    LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", installCollectionFromAddonCollectionFolder [" + CollectionName + "], install collection file contains a data.record node with a blank content attribute.");
                                                                                    result = false;
                                                                                    return_ErrorMessage += "<P>Collection file [" + CollectionName + "] contains a data.record node with a blank content attribute.</P>";
                                                                                    return false;
                                                                                } else {
                                                                                    string ContentRecordGuid = XmlController.GetXMLAttribute(core, IsFound, ContentNode, "guid", "");
                                                                                    string ContentRecordName = XmlController.GetXMLAttribute(core, IsFound, ContentNode, "name", "");
                                                                                    if ((string.IsNullOrEmpty(ContentRecordGuid)) && (string.IsNullOrEmpty(ContentRecordName))) {
                                                                                        LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", installCollectionFromAddonCollectionFolder [" + CollectionName + "], install collection file contains a data record node with neither guid nor name. It must have either a name or a guid attribute. The content is [" + ContentName + "]");
                                                                                        result = false;
                                                                                        return_ErrorMessage += "<P>The collection [" + CollectionName + "] was not installed because the Collection file contains a data record node with neither name nor guid. This is not allowed. The content is [" + ContentName + "].</P>";
                                                                                        return false;
                                                                                    } else {
                                                                                        //
                                                                                        // create or update the record
                                                                                        //
                                                                                        ContentMetadataModel metaData = Models.Domain.ContentMetadataModel.createByUniqueName(core, ContentName);
                                                                                        using (var csData = new CsModel(core)) {
                                                                                            if (!string.IsNullOrEmpty(ContentRecordGuid)) {
                                                                                                csData.open(ContentName, "ccguid=" + DbController.encodeSQLText(ContentRecordGuid));
                                                                                            } else {
                                                                                                csData.open(ContentName, "name=" + DbController.encodeSQLText(ContentRecordName));
                                                                                            }
                                                                                            bool recordfound = true;
                                                                                            if (!csData.ok()) {
                                                                                                //
                                                                                                // Insert the new record
                                                                                                //
                                                                                                recordfound = false;
                                                                                                csData.close();
                                                                                                csData.insert(ContentName);
                                                                                            }
                                                                                            if (csData.ok()) {
                                                                                                //
                                                                                                // Update the record
                                                                                                //
                                                                                                if (recordfound && (!string.IsNullOrEmpty(ContentRecordGuid))) {
                                                                                                    //
                                                                                                    // found by guid, use guid in list and save name
                                                                                                    //
                                                                                                    csData.set("name", ContentRecordName);
                                                                                                    DataRecordList = DataRecordList + Environment.NewLine + ContentName + "," + ContentRecordGuid;
                                                                                                } else if (recordfound) {
                                                                                                    //
                                                                                                    // record found by name, use name is list but do not add guid
                                                                                                    //
                                                                                                    DataRecordList = DataRecordList + Environment.NewLine + ContentName + "," + ContentRecordName;
                                                                                                } else {
                                                                                                    //
                                                                                                    // record was created
                                                                                                    //
                                                                                                    csData.set("ccguid", ContentRecordGuid);
                                                                                                    csData.set("name", ContentRecordName);
                                                                                                    DataRecordList = DataRecordList + Environment.NewLine + ContentName + "," + ContentRecordGuid;
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                        break;
                                                                    }
                                                            }
                                                        }
                                                    }
                                                    //
                                                    //-------------------------------------------------------------------------------
                                                    LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", installCollectionFromAddonCollectionFolder [" + CollectionName + "], stage-6, install addon nodes, set importcollection relationships");
                                                    //-------------------------------------------------------------------------------
                                                    //
                                                    foreach (XmlNode metaDataSection in Doc.DocumentElement.ChildNodes) {
                                                        switch (GenericController.vbLCase(metaDataSection.Name)) {
                                                            case "cdef":
                                                            case "data":
                                                            case "help":
                                                            case "resource":
                                                            case "helplink":
                                                                //
                                                                // ignore - processed in previous passes
                                                                break;
                                                            case "getcollection":
                                                            case "importcollection":
                                                                //
                                                                // processed, but add rule for collection record
                                                                bool Found = false;
                                                                string ChildCollectionName = XmlController.GetXMLAttribute(core, Found, metaDataSection, "name", "");
                                                                string ChildCollectionGUId = XmlController.GetXMLAttribute(core, Found, metaDataSection, "guid", metaDataSection.InnerText);
                                                                if (string.IsNullOrEmpty(ChildCollectionGUId)) {
                                                                    ChildCollectionGUId = metaDataSection.InnerText;
                                                                }
                                                                if (!string.IsNullOrEmpty(ChildCollectionGUId)) {
                                                                    int ChildCollectionId = 0;
                                                                    using (var csData = new CsModel(core)) {
                                                                        csData.open("Add-on Collections", "ccguid=" + DbController.encodeSQLText(ChildCollectionGUId));
                                                                        if (csData.ok()) {
                                                                            ChildCollectionId = csData.getInteger("id");
                                                                        }
                                                                        csData.close();
                                                                        if (ChildCollectionId != 0) {
                                                                            csData.insert("Add-on Collection Parent Rules");
                                                                            if (csData.ok()) {
                                                                                csData.set("ParentID", collection.id);
                                                                                csData.set("ChildID", ChildCollectionId);
                                                                            }
                                                                            csData.close();
                                                                        }
                                                                    }
                                                                }
                                                                break;
                                                            case "scriptingmodule":
                                                            case "scriptingmodules":
                                                                result = false;
                                                                return_ErrorMessage += "<P>Collection [" + CollectionName + "] includes a scripting module which is no longer supported. Move scripts to the code tab.</P>";
                                                                return false;
                                                            case "sharedstyle":
                                                                result = false;
                                                                return_ErrorMessage += "<P>Collection [" + CollectionName + "] includes a shared style which is no longer supported. Move styles to the default styles tab.</P>";
                                                                return false;
                                                            case "addon":
                                                            case "add-on":
                                                                //
                                                                // Add-on Node, do part 1 of 2
                                                                //   (include add-on node must be done after all add-ons are installed)
                                                                //
                                                                InstallAddonNode(core, metaDataSection, "ccguid", core.siteProperties.dataBuildVersion, collection.id, ref result, ref return_ErrorMessage, ref collectionIncludesDiagnosticAddon);
                                                                if (!result) { return result; }
                                                                break;
                                                            case "interfaces":
                                                                //
                                                                // Legacy Interface Node
                                                                //
                                                                foreach (XmlNode metaDataInterfaces in metaDataSection.ChildNodes) {
                                                                    InstallAddonNode(core, metaDataInterfaces, "ccguid", core.siteProperties.dataBuildVersion, collection.id, ref result, ref return_ErrorMessage, ref collectionIncludesDiagnosticAddon);
                                                                    if (!result) { return result; }
                                                                }
                                                                break;
                                                        }
                                                    }
                                                    //
                                                    //-------------------------------------------------------------------------------
                                                    LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", installCollectionFromAddonCollectionFolder [" + CollectionName + "], stage-7, set addon dependency relationships");
                                                    //-------------------------------------------------------------------------------
                                                    //
                                                    foreach (XmlNode collectionNode in Doc.DocumentElement.ChildNodes) {
                                                        switch (collectionNode.Name.ToLowerInvariant()) {
                                                            case "addon":
                                                            case "add-on":
                                                                //
                                                                // Add-on Node, do part 1, verify the addon in the table with name and guid
                                                                setAddonDependencies(core, collectionNode, "ccguid", core.siteProperties.dataBuildVersion, collection.id, ref result, ref return_ErrorMessage);
                                                                if (!result) { return result; }
                                                                break;
                                                            case "interfaces":
                                                                //
                                                                // Legacy Interface Node
                                                                //
                                                                foreach (XmlNode metaDataInterfaces in collectionNode.ChildNodes) {
                                                                    setAddonDependencies(core, metaDataInterfaces, "ccguid", core.siteProperties.dataBuildVersion, collection.id, ref result, ref return_ErrorMessage);
                                                                    if (!result) { return result; }
                                                                }
                                                                break;
                                                        }
                                                    }
                                                    //
                                                    //-------------------------------------------------------------------------------
                                                    LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", installCollectionFromAddonCollectionFolder [" + CollectionName + "], stage-8, process data nodes, set record fields");
                                                    //-------------------------------------------------------------------------------
                                                    //
                                                    foreach (XmlNode metaDataSection in Doc.DocumentElement.ChildNodes) {
                                                        switch (GenericController.vbLCase(metaDataSection.Name)) {
                                                            case "data":
                                                                int recordPtr = 0;
                                                                foreach (XmlNode ContentNode in metaDataSection.ChildNodes) {
                                                                    if (ContentNode.Name.ToLowerInvariant() == "record") {
                                                                        recordPtr += 1;
                                                                        string ContentName = XmlController.GetXMLAttribute(core, IsFound, ContentNode, "content", "");
                                                                        if (string.IsNullOrEmpty(ContentName)) {
                                                                            LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", installCollectionFromAddonCollectionFolder [" + CollectionName + "], install collection file contains a data.record node with a blank content attribute.");
                                                                            result = false;
                                                                            return_ErrorMessage += "<P>Collection file contains a data.record node with a blank content attribute.</P>";
                                                                            return false;
                                                                        } else {
                                                                            string ContentRecordGuid = XmlController.GetXMLAttribute(core, IsFound, ContentNode, "guid", "");
                                                                            string ContentRecordName = XmlController.GetXMLAttribute(core, IsFound, ContentNode, "name", "");
                                                                            if ((!string.IsNullOrEmpty(ContentRecordGuid)) || (!string.IsNullOrEmpty(ContentRecordName))) {
                                                                                ContentMetadataModel metaData = Models.Domain.ContentMetadataModel.createByUniqueName(core, ContentName);
                                                                                using (var csData = new CsModel(core)) {
                                                                                    if (!string.IsNullOrEmpty(ContentRecordGuid)) {
                                                                                        csData.open(ContentName, "ccguid=" + DbController.encodeSQLText(ContentRecordGuid));
                                                                                    } else {
                                                                                        csData.open(ContentName, "name=" + DbController.encodeSQLText(ContentRecordName));
                                                                                    }
                                                                                    if (csData.ok()) {
                                                                                        //
                                                                                        // Update the record
                                                                                        foreach (XmlNode FieldNode in ContentNode.ChildNodes) {
                                                                                            if (FieldNode.Name.ToLowerInvariant() == "field") {
                                                                                                bool IsFieldFound = false;
                                                                                                string FieldName = XmlController.GetXMLAttribute(core, IsFound, FieldNode, "name", "").ToLowerInvariant();
                                                                                                CPContentBaseClass.FieldTypeIdEnum fieldTypeId = 0;
                                                                                                int FieldLookupContentId = -1;
                                                                                                foreach (var keyValuePair in metaData.fields) {
                                                                                                    Models.Domain.ContentFieldMetadataModel field = keyValuePair.Value;
                                                                                                    if (GenericController.vbLCase(field.nameLc) == FieldName) {
                                                                                                        fieldTypeId = field.fieldTypeId;
                                                                                                        FieldLookupContentId = field.lookupContentId;
                                                                                                        IsFieldFound = true;
                                                                                                        break;
                                                                                                    }
                                                                                                }
                                                                                                if (IsFieldFound) {
                                                                                                    string fieldValue = FieldNode.InnerText;
                                                                                                    switch (fieldTypeId) {
                                                                                                        case CPContentBaseClass.FieldTypeIdEnum.AutoIdIncrement:
                                                                                                        case CPContentBaseClass.FieldTypeIdEnum.Redirect: {
                                                                                                                //
                                                                                                                // not supported
                                                                                                                break;
                                                                                                            }
                                                                                                        case CPContentBaseClass.FieldTypeIdEnum.Lookup: {
                                                                                                                //
                                                                                                                // read in text value, if a guid, use it, otherwise assume name
                                                                                                                if (!string.IsNullOrWhiteSpace(fieldValue)) {
                                                                                                                    if (FieldLookupContentId != 0) {
                                                                                                                        var lookupContentMetadata = ContentMetadataModel.create(core, FieldLookupContentId);
                                                                                                                        if (lookupContentMetadata != null) {
                                                                                                                            int fieldLookupId = lookupContentMetadata.getRecordId(core, fieldValue);
                                                                                                                            if (fieldLookupId <= 0) {
                                                                                                                                return_ErrorMessage += "<P>Warning: In collection [" + CollectionName + "], data section, record number [" + recordPtr + "], the lookup field [" + FieldName + "], value [" + fieldValue + "] was not found in lookup content [" + lookupContentMetadata.name + "].</P>";
                                                                                                                                return false;
                                                                                                                            } else {
                                                                                                                                csData.set(FieldName, fieldLookupId);
                                                                                                                            }
                                                                                                                        }
                                                                                                                    } else if (fieldValue.IsNumeric()) {
                                                                                                                        //
                                                                                                                        // must be lookup list
                                                                                                                        csData.set(FieldName, fieldValue);
                                                                                                                    }

                                                                                                                }
                                                                                                                break;
                                                                                                            }
                                                                                                        default: {
                                                                                                                csData.set(FieldName, fieldValue);
                                                                                                                break;
                                                                                                            }
                                                                                                    }
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                                break;
                                                        }
                                                    }
                                                    // todo - install navigator entries here, after addons installed to pickup nodes with addon references
                                                    //
                                                    //----------------------------------------------------------------------------------------------------------------------
                                                    LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", verify all navigator menu entries for updated addons");
                                                    //----------------------------------------------------------------------------------------------------------------------
                                                    //
                                                    //var emptyList = new List<string>();
                                                    var defaultMiniCollection = new MetadataMiniCollectionModel();
                                                    MetadataMiniCollectionModel Collection = MetadataMiniCollectionModel.loadXML(core, collectionFileContent, isBaseCollection, false, IsNewBuild, defaultMiniCollection, "");
                                                    foreach (var kvp in Collection.menus) {
                                                        BuildController.verifyNavigatorEntry(core, kvp.Value, 0);
                                                    }
                                                    //
                                                    // --- end of pass
                                                }
                                                collection.dataRecordList = DataRecordList;
                                                collection.save(core.cpParent);
                                                //
                                                // -- test for diagnostic addon, warn if missing
                                                if (!collectionIncludesDiagnosticAddon) {
                                                    //
                                                    // -- log warning. This collection does not have an install addon
                                                    LogController.logWarn(core, "Collection does not include a Diagnostic addon, [" + collection.name + "]");
                                                }
                                                //
                                                // -- execute onInstall addon if found
                                                if (string.IsNullOrEmpty(onInstallAddonGuid)) {
                                                    //
                                                    // -- log warning. This collection does not have an install addon
                                                    LogController.logWarn(core, "Collection does not include an install addon, [" + collection.name + "]");
                                                } else {
                                                    //
                                                    // -- install the install addon
                                                    var addon = DbBaseModel.create<AddonModel>(core.cpParent, onInstallAddonGuid);
                                                    if (addon != null) {
                                                        var executeContext = new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                                                            addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextSimple,
                                                            errorContextMessage = "calling onInstall Addon [" + addon.name + "] for collection [" + collection.name + "]"
                                                        };
                                                        core.addon.execute(addon, executeContext);
                                                    }
                                                }
                                                //
                                                LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", installCollectionFromAddonCollectionFolder [" + CollectionName + "], upgrade complete, flush cache");
                                                //
                                                // -- import complete, flush caches
                                                core.cache.invalidateAll();
                                                result = true;
                                            }
                                        }
                                    }
                                    //
                                    // -- invalidate cache
                                    core.cache.invalidateAll();
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                //
                // Log error and exit with failure. This way any other upgrading will still continue
                LogController.logError(core, ex);
                throw;
            } finally {
                contextLog.Pop();
            }
            return result;
        }
        //
        //======================================================================================================
        /// <summary>
        /// Installs all collections found in a source folder.
        /// Builds the Collection Folder. 
        /// Calls installCollectionFromCollectionFolder.
        /// </summary>
        /// <param name="core"></param>
        /// <param name="installPrivatePath"></param>
        /// <param name="return_ErrorMessage"></param>
        /// <param name="collectionsInstalledList">a list of the collections installed to the database during this installation (dependencies etc.). The collections installed are added to this list</param>
        /// <param name="IsNewBuild"></param>
        /// <param name="reinstallDependencies"></param>
        /// <param name="nonCriticalErrorList"></param>
        /// <param name="logPrefix"></param>
        /// <param name="includeBaseMetaDataInstall"></param>
        /// <param name="collectionsDownloaded">List of collections that have been downloaded during this istall pass but have not been installed yet. Do no need to download them again.</param>
        /// <returns></returns>
        public static bool installCollectionsFromPrivateFolder(CoreController core, Stack<string> contextLog, string installPrivatePath, ref string return_ErrorMessage, ref List<string> collectionsInstalledList, bool IsNewBuild, bool reinstallDependencies, ref List<string> nonCriticalErrorList, string logPrefix, bool includeBaseMetaDataInstall, ref List<string> collectionsDownloaded) {
            bool returnSuccess = false;
            try {
                contextLog.Push(MethodInfo.GetCurrentMethod().Name + ", [" + installPrivatePath + "]");
                traceContextLog(core, contextLog);
                DateTime CollectionLastChangeDate = DateTime.Now;
                //
                // -- collectionsToInstall = collections stored in the collection folder that need to be stored in the Db
                var collectionsToInstall = new List<string>();
                var collectionsBuildingFolder = new List<string>();
                returnSuccess = CollectionFolderController.buildCollectionFoldersFromCollectionZips(core, contextLog, installPrivatePath, CollectionLastChangeDate, ref collectionsToInstall, ref return_ErrorMessage, ref collectionsInstalledList, ref collectionsBuildingFolder);
                if (!returnSuccess) {
                    //
                    // BuildLocal failed, log it and do not upgrade
                    //
                    LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", BuildLocalCollectionFolder returned false with Error Message [" + return_ErrorMessage + "], exiting without calling UpgradeAllAppsFromLocalCollection");
                } else {
                    foreach (string collectionGuid in collectionsToInstall) {
                        if (!installCollectionFromCollectionFolder(core, contextLog, collectionGuid, ref return_ErrorMessage, IsNewBuild, reinstallDependencies, ref nonCriticalErrorList, logPrefix, ref collectionsInstalledList, includeBaseMetaDataInstall, ref collectionsDownloaded)) {
                            LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", UpgradeAllAppsFromLocalCollection returned false with Error Message [" + return_ErrorMessage + "].");
                            break;
                        }
                        //
                        // -- invalidate cache
                        core.cache.invalidateAll();
                    }
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
                returnSuccess = false;
                if (string.IsNullOrEmpty(return_ErrorMessage)) {
                    return_ErrorMessage = "There was an unexpected error installing the collection, details [" + ex.Message + "]";
                }
            } finally {
                contextLog.Pop();
            }
            return returnSuccess;
        }
        //
        //======================================================================================================
        /// <summary>
        /// Installs a collectionZip from a file in privateFiles.
        /// Builds the Collection Folder. 
        /// Calls installCollectionFromCollectionFolder.
        /// </summary>
        public static bool installCollectionFromPrivateFile(CoreController core, Stack<string> contextLog, string pathFilename, ref string return_ErrorMessage, ref string return_CollectionGUID, bool IsNewBuild, bool reinstallDependencies, ref List<string> nonCriticalErrorList, string logPrefix, ref List<string> collectionsInstalledList) {
            bool returnSuccess = true;
            try {
                contextLog.Push(MethodInfo.GetCurrentMethod().Name + ", [" + pathFilename + "]");
                traceContextLog(core, contextLog);
                DateTime CollectionLastChangeDate;
                //
                // -- build the collection folder and download/install all collection dependencies, return list collectionsDownloaded
                CollectionLastChangeDate = DateTime.Now;
                var collectionsDownloaded = new List<string>();
                var collectionsBuildingFolder = new List<string>();
                if (!CollectionFolderController.buildCollectionFolderFromCollectionZip(core, contextLog, pathFilename, CollectionLastChangeDate, ref return_ErrorMessage, ref collectionsDownloaded, ref collectionsInstalledList, ref collectionsBuildingFolder)) {
                    //
                    // BuildLocal failed, log it and do not upgrade
                    //
                    returnSuccess = false;
                    LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", BuildLocalCollectionFolder returned false with Error Message [" + return_ErrorMessage + "], exiting without calling UpgradeAllAppsFromLocalCollection");
                } else if (collectionsDownloaded.Count > 0) {
                    return_CollectionGUID = collectionsDownloaded.First();
                    foreach (var collection in collectionsDownloaded) {
                        if (!installCollectionFromCollectionFolder(core, contextLog, collection, ref return_ErrorMessage, IsNewBuild, reinstallDependencies, ref nonCriticalErrorList, logPrefix, ref collectionsInstalledList, true, ref collectionsDownloaded)) {
                            //
                            // Upgrade all apps failed
                            //
                            returnSuccess = false;
                            LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", UpgradeAllAppsFromLocalCollection returned false with Error Message [" + return_ErrorMessage + "].");
                        }
                    }
                    LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", Collection(s) installed successfully.");
                }
                //
                // -- invalidate cache
                core.cache.invalidateAll();
            } catch (Exception ex) {
                LogController.logError(core, ex);
                returnSuccess = false;
                if (string.IsNullOrEmpty(return_ErrorMessage)) {
                    return_ErrorMessage = "There was an unexpected error installing the collection, details [" + ex.Message + "]";
                }
            } finally {
                contextLog.Pop();
            }
            return returnSuccess;
        }
        //
        //======================================================================================================
        /// <summary>
        /// copy resources from install folder to www folder
        /// </summary>
        private static void copyPrivateFilesSrcPathToWwwDstPath(CoreController core, string privateFilesSrcPath, string wwwFilesDstPath, string BlockFileList, string BlockFolderList) {
            try {

                string privateFilesSrcFolder = null;
                string wwwFilesDstDstFolder = null;
                //
                privateFilesSrcFolder = privateFilesSrcPath;
                if (privateFilesSrcFolder.Substring(privateFilesSrcFolder.Length - 1) == "\\") {
                    privateFilesSrcFolder = privateFilesSrcFolder.Left(privateFilesSrcFolder.Length - 1);
                }
                //
                wwwFilesDstDstFolder = wwwFilesDstPath;
                if (wwwFilesDstDstFolder.Substring(wwwFilesDstDstFolder.Length - 1) == "\\") {
                    wwwFilesDstDstFolder = wwwFilesDstDstFolder.Left(wwwFilesDstDstFolder.Length - 1);
                }
                //
                if (core.privateFiles.pathExists(privateFilesSrcFolder)) {
                    List<FileDetail> FileInfoArray = core.privateFiles.getFileList(privateFilesSrcFolder);
                    foreach (FileDetail file in FileInfoArray) {
                        if ((file.Extension == "dll") || (file.Extension == "exe") || (file.Extension == "zip")) {
                            //
                            // can not copy dll or exe
                            //
                            //Filename = Filename
                        } else if (("," + BlockFileList + ",").IndexOf("," + file.Name + ",", System.StringComparison.OrdinalIgnoreCase) != -1) {
                            //
                            // can not copy the current collection file
                            //
                            //file.Name = file.Name
                        } else {
                            //
                            // copy this file to destination
                            //
                            core.privateFiles.copyFile(privateFilesSrcPath + file.Name, wwwFilesDstPath + file.Name, core.wwwFiles);
                        }
                    }
                    //
                    // copy folders to dst
                    //
                    List<FolderDetail> FolderInfoArray = core.privateFiles.getFolderList(privateFilesSrcFolder);
                    foreach (FolderDetail folder in FolderInfoArray) {
                        if (("," + BlockFolderList + ",").IndexOf("," + folder.Name + ",", System.StringComparison.OrdinalIgnoreCase) == -1) {
                            copyPrivateFilesSrcPathToWwwDstPath(core, privateFilesSrcPath + folder.Name + "\\", wwwFilesDstPath + folder.Name + "\\", BlockFileList, "");
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
        }
        //
        //======================================================================================================
        //
        private static string GetCollectionFileList(CoreController core, string privateFilesSrcPath, string SubFolder, string ExcludeFileList) {
            string result = "";
            try {
                string privateFilesSrcFolder;
                //
                privateFilesSrcFolder = privateFilesSrcPath + SubFolder;
                if (privateFilesSrcFolder.Substring(privateFilesSrcFolder.Length - 1) == "\\") {
                    privateFilesSrcFolder = privateFilesSrcFolder.Left(privateFilesSrcFolder.Length - 1);
                }
                //
                if (core.privateFiles.pathExists(privateFilesSrcFolder)) {
                    List<FileDetail> FileInfoArray = core.privateFiles.getFileList(privateFilesSrcFolder);
                    foreach (FileDetail file in FileInfoArray) {
                        if (("," + ExcludeFileList + ",").IndexOf("," + file.Name + ",", System.StringComparison.OrdinalIgnoreCase) != -1) {
                            //
                            // do not copy the current collection file
                        } else {
                            result += Environment.NewLine + SubFolder + file.Name;
                        }
                    }
                    //
                    // get subfolders
                    List<FolderDetail> FolderInfoArray = core.privateFiles.getFolderList(privateFilesSrcFolder);
                    foreach (FolderDetail folder in FolderInfoArray) {
                        result += GetCollectionFileList(core, privateFilesSrcPath, SubFolder + folder.Name + "\\", ExcludeFileList);
                    }
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
            return result;
        }
        //
        //======================================================================================================
        //
        private static void InstallAddonNode(CoreController core, XmlNode AddonNode, string AddonGuidFieldName, string ignore_BuildVersion, int CollectionID, ref bool return_UpgradeOK, ref string return_ErrorMessage, ref bool collectionIncludesDiagnosticAddons) {
            // todo - return bool
            return_ErrorMessage = "";
            return_UpgradeOK = true;
            try {
                string Basename = GenericController.vbLCase(AddonNode.Name);
                if ((Basename == "page") || (Basename == "process") || (Basename == "addon") || (Basename == "add-on")) {
                    bool IsFound = false;
                    string addonName = XmlController.GetXMLAttribute(core, IsFound, AddonNode, "name", "No Name");
                    if (string.IsNullOrEmpty(addonName)) {
                        addonName = "No Name";
                    }
                    string addonGuid = XmlController.GetXMLAttribute(core, IsFound, AddonNode, "guid", addonName);
                    if (string.IsNullOrEmpty(addonGuid)) {
                        addonGuid = addonName;
                    }
                    string navTypeName = XmlController.GetXMLAttribute(core, IsFound, AddonNode, "type", "");
                    int navTypeId = GetListIndex(navTypeName, navTypeIDList);
                    if (navTypeId == 0) {
                        navTypeId = NavTypeIDAddon;
                    }
                    using (var cs = new CsModel(core)) {
                        string Criteria = "(" + AddonGuidFieldName + "=" + DbController.encodeSQLText(addonGuid) + ")";
                        cs.open(AddonModel.tableMetadata.contentName, Criteria, "", false);
                        if (cs.ok()) {
                            //
                            // Update the Addon
                            //
                            LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", UpgradeAppFromLocalCollection, GUID match with existing Add-on, Updating Add-on [" + addonName + "], Guid [" + addonGuid + "]");
                        } else {
                            //
                            // not found by GUID - search name against name to update legacy Add-ons
                            //
                            cs.close();
                            Criteria = "(name=" + DbController.encodeSQLText(addonName) + ")and(" + AddonGuidFieldName + " is null)";
                            cs.open(AddonModel.tableMetadata.contentName, Criteria, "", false);
                            if (cs.ok()) {
                                LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", UpgradeAppFromLocalCollection, Add-on name matched an existing Add-on that has no GUID, Updating legacy Aggregate Function to Add-on [" + addonName + "], Guid [" + addonGuid + "]");
                            }
                        }
                        if (!cs.ok()) {
                            //
                            // not found by GUID or by name, Insert a new addon
                            //
                            cs.close();
                            cs.insert(AddonModel.tableMetadata.contentName);
                            if (cs.ok()) {
                                LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", UpgradeAppFromLocalCollection, Creating new Add-on [" + addonName + "], Guid [" + addonGuid + "]");
                            }
                        }
                        if (!cs.ok()) {
                            //
                            // Could not create new Add-on
                            //
                            LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", UpgradeAppFromLocalCollection, Add-on could not be created, skipping Add-on [" + addonName + "], Guid [" + addonGuid + "]");
                        } else {
                            int addonId = cs.getInteger("ID");
                            MetadataController.deleteContentRecords(core, "Add-on Include Rules", "addonid=" + addonId);
                            MetadataController.deleteContentRecords(core, "Add-on Content Trigger Rules", "addonid=" + addonId);
                            //
                            cs.set("collectionid", CollectionID);
                            cs.set(AddonGuidFieldName, addonGuid);
                            cs.set("name", addonName);
                            cs.set("navTypeId", navTypeId);
                            string ArgumentList = "";
                            string StyleSheet = "";
                            if (AddonNode.ChildNodes.Count > 0) {
                                foreach (XmlNode PageInterfaceWithinLoop in AddonNode.ChildNodes) {
                                    if (!(PageInterfaceWithinLoop is XmlComment)) {
                                        XmlNode PageInterface = PageInterfaceWithinLoop;
                                        string test = null;
                                        int scriptinglanguageid = 0;
                                        string ScriptingCode = null;
                                        string fieldName = null;
                                        string NodeName = null;
                                        string NewValue = null;
                                        string menuNameSpace = null;
                                        string FieldValue = "";
                                        string ScriptingEntryPoint = null;
                                        int ScriptingTimeout = 0;
                                        string ScriptingLanguage = null;
                                        switch (GenericController.vbLCase(PageInterfaceWithinLoop.Name)) {
                                            case "activexdll":
                                                //
                                                // This is handled in BuildLocalCollectionFolder
                                                //
                                                break;
                                            case "editors":
                                                //
                                                // list of editors
                                                //
                                                foreach (XmlNode TriggerNode in PageInterfaceWithinLoop.ChildNodes) {
                                                    //
                                                    int fieldTypeId = 0;
                                                    string fieldType = null;
                                                    switch (GenericController.vbLCase(TriggerNode.Name)) {
                                                        case "type":
                                                            fieldType = TriggerNode.InnerText;
                                                            fieldTypeId = MetadataController.getRecordIdByUniqueName(core, "Content Field Types", fieldType);
                                                            if (fieldTypeId > 0) {
                                                                using (var CS2 = new CsModel(core)) {
                                                                    Criteria = "(addonid=" + addonId + ")and(contentfieldTypeID=" + fieldTypeId + ")";
                                                                    CS2.open("Add-on Content Field Type Rules", Criteria);
                                                                    if (!CS2.ok()) {
                                                                        CS2.insert("Add-on Content Field Type Rules");
                                                                    }
                                                                    if (CS2.ok()) {
                                                                        CS2.set("addonid", addonId);
                                                                        CS2.set("contentfieldTypeID", fieldTypeId);
                                                                    }
                                                                }
                                                            }
                                                            break;
                                                    }
                                                }
                                                break;
                                            case "processtriggers":
                                                //
                                                // list of events that trigger a process run for this addon
                                                //
                                                foreach (XmlNode TriggerNode in PageInterfaceWithinLoop.ChildNodes) {
                                                    int TriggerContentId = 0;
                                                    string ContentNameorGuid = null;
                                                    switch (GenericController.vbLCase(TriggerNode.Name)) {
                                                        case "contentchange":
                                                            TriggerContentId = 0;
                                                            ContentNameorGuid = TriggerNode.InnerText;
                                                            if (string.IsNullOrEmpty(ContentNameorGuid)) {
                                                                ContentNameorGuid = XmlController.GetXMLAttribute(core, IsFound, TriggerNode, "guid", "");
                                                                if (string.IsNullOrEmpty(ContentNameorGuid)) {
                                                                    ContentNameorGuid = XmlController.GetXMLAttribute(core, IsFound, TriggerNode, "name", "");
                                                                }
                                                            }
                                                            using (var CS2 = new CsModel(core)) {
                                                                Criteria = "(ccguid=" + DbController.encodeSQLText(ContentNameorGuid) + ")";
                                                                CS2.open("Content", Criteria);
                                                                if (!CS2.ok()) {
                                                                    Criteria = "(ccguid is null)and(name=" + DbController.encodeSQLText(ContentNameorGuid) + ")";
                                                                    CS2.open("content", Criteria);
                                                                }
                                                                if (CS2.ok()) {
                                                                    TriggerContentId = CS2.getInteger("ID");
                                                                }
                                                            }
                                                            if (TriggerContentId != 0) {
                                                                using (var CS2 = new CsModel(core)) {
                                                                    Criteria = "(addonid=" + addonId + ")and(contentid=" + TriggerContentId + ")";
                                                                    CS2.open("Add-on Content Trigger Rules", Criteria);
                                                                    if (!CS2.ok()) {
                                                                        CS2.insert("Add-on Content Trigger Rules");
                                                                        if (CS2.ok()) {
                                                                            CS2.set("addonid", addonId);
                                                                            CS2.set("contentid", TriggerContentId);
                                                                        }
                                                                    }
                                                                    CS2.close();
                                                                }
                                                            }
                                                            break;
                                                    }
                                                }
                                                break;
                                            case "scripting":
                                                //
                                                // include add-ons - NOTE - import collections must be run before interfaces
                                                // when importing a collectin that will be used for an include
                                                //
                                                ScriptingLanguage = XmlController.GetXMLAttribute(core, IsFound, PageInterfaceWithinLoop, "language", "");
                                                if (ScriptingLanguage.ToLower(CultureInfo.InvariantCulture) == "jscript") {
                                                    scriptinglanguageid = (int)AddonController.ScriptLanguages.Javascript;
                                                } else {
                                                    scriptinglanguageid = (int)AddonController.ScriptLanguages.VBScript;
                                                }
                                                cs.set("scriptinglanguageid", scriptinglanguageid);
                                                ScriptingEntryPoint = XmlController.GetXMLAttribute(core, IsFound, PageInterfaceWithinLoop, "entrypoint", "");
                                                cs.set("ScriptingEntryPoint", ScriptingEntryPoint);
                                                ScriptingTimeout = GenericController.encodeInteger(XmlController.GetXMLAttribute(core, IsFound, PageInterfaceWithinLoop, "timeout", "5000"));
                                                cs.set("ScriptingTimeout", ScriptingTimeout);
                                                ScriptingCode = "";
                                                foreach (XmlNode ScriptingNode in PageInterfaceWithinLoop.ChildNodes) {
                                                    switch (GenericController.vbLCase(ScriptingNode.Name)) {
                                                        case "code":
                                                            ScriptingCode += ScriptingNode.InnerText;
                                                            break;
                                                    }
                                                }
                                                cs.set("ScriptingCode", ScriptingCode);
                                                break;
                                            case "activexprogramid":
                                                //
                                                // save program id
                                                //
                                                FieldValue = PageInterfaceWithinLoop.InnerText;
                                                cs.set("ObjectProgramID", FieldValue);
                                                break;
                                            case "navigator":
                                                //
                                                // create a navigator entry with a parent set to this
                                                //
                                                cs.save();
                                                menuNameSpace = XmlController.GetXMLAttribute(core, IsFound, PageInterfaceWithinLoop, "NameSpace", "");
                                                if (!string.IsNullOrEmpty(menuNameSpace)) {
                                                    string NavIconTypeString = XmlController.GetXMLAttribute(core, IsFound, PageInterfaceWithinLoop, "type", "");
                                                    if (string.IsNullOrEmpty(NavIconTypeString)) {
                                                        NavIconTypeString = "Addon";
                                                    }
                                                    //Dim builder As New coreBuilderClass(core)
                                                    BuildController.verifyNavigatorEntry(core, new MetadataMiniCollectionModel.MiniCollectionMenuModel() {
                                                        menuNameSpace = menuNameSpace,
                                                        name = addonName,
                                                        AdminOnly = false,
                                                        DeveloperOnly = false,
                                                        NewWindow = false,
                                                        Active = true,
                                                        AddonName = addonName,
                                                        NavIconType = NavIconTypeString,
                                                        NavIconTitle = addonName
                                                    }, CollectionID);
                                                }
                                                break;
                                            case "argument":
                                            case "argumentlist":
                                                //
                                                // multiple argumentlist elements are concatinated with crlf
                                                //
                                                NewValue = encodeText(PageInterfaceWithinLoop.InnerText).Trim(' ');
                                                if (!string.IsNullOrEmpty(NewValue)) {
                                                    if (string.IsNullOrEmpty(ArgumentList)) {
                                                        ArgumentList = NewValue;
                                                    } else if (NewValue != FieldValue) {
                                                        ArgumentList = ArgumentList + Environment.NewLine + NewValue;
                                                    }
                                                }
                                                break;
                                            case "style":
                                                //
                                                // import exclusive style
                                                //
                                                NodeName = XmlController.GetXMLAttribute(core, IsFound, PageInterfaceWithinLoop, "name", "");
                                                NewValue = encodeText(PageInterfaceWithinLoop.InnerText).Trim(' ');
                                                if (NewValue.Left(1) != "{") {
                                                    NewValue = "{" + NewValue;
                                                }
                                                if (NewValue.Substring(NewValue.Length - 1) != "}") {
                                                    NewValue = NewValue + "}";
                                                }
                                                StyleSheet = StyleSheet + Environment.NewLine + NodeName + " " + NewValue;
                                                break;
                                            case "stylesheet":
                                            case "styles":
                                                //
                                                // import exclusive stylesheet if more then whitespace
                                                //
                                                test = PageInterfaceWithinLoop.InnerText;
                                                test = GenericController.vbReplace(test, " ", "");
                                                test = GenericController.vbReplace(test, "\r", "");
                                                test = GenericController.vbReplace(test, "\n", "");
                                                test = GenericController.vbReplace(test, "\t", "");
                                                if (!string.IsNullOrEmpty(test)) {
                                                    StyleSheet = StyleSheet + Environment.NewLine + PageInterfaceWithinLoop.InnerText;
                                                }
                                                break;
                                            case "template":
                                            case "content":
                                            case "admin":
                                                //
                                                // these add-ons will be "non-developer only" in navigation
                                                //
                                                fieldName = PageInterfaceWithinLoop.Name;
                                                FieldValue = PageInterfaceWithinLoop.InnerText;
                                                if (!cs.isFieldSupported(fieldName)) {
                                                    //
                                                    // Bad field name - need to report it somehow
                                                    //
                                                } else {
                                                    cs.set(fieldName, FieldValue);
                                                    if (GenericController.encodeBoolean(PageInterfaceWithinLoop.InnerText)) {
                                                        //
                                                        // if template, admin or content - let non-developers have navigator entry
                                                        //
                                                    }
                                                }
                                                break;
                                            case "icon":
                                                //
                                                // icon
                                                //
                                                FieldValue = XmlController.GetXMLAttribute(core, IsFound, PageInterfaceWithinLoop, "link", "");
                                                if (!string.IsNullOrEmpty(FieldValue)) {
                                                    //
                                                    // Icons can be either in the root of the website or in content files
                                                    //
                                                    FieldValue = GenericController.vbReplace(FieldValue, "\\", "/"); // make it a link, not a file
                                                    if (GenericController.vbInstr(1, FieldValue, "://") != 0) {
                                                        //
                                                        // the link is an absolute URL, leave it link this
                                                        //
                                                    } else {
                                                        if (FieldValue.Left(1) != "/") {
                                                            //
                                                            // make sure it starts with a slash to be consistance
                                                            //
                                                            FieldValue = "/" + FieldValue;
                                                        }
                                                        if (FieldValue.Left(17) == "/contensivefiles/") {
                                                            //
                                                            // in content files, start link without the slash
                                                            //
                                                            FieldValue = FieldValue.Substring(17);
                                                        }
                                                    }
                                                    cs.set("IconFilename", FieldValue);
                                                    {
                                                        cs.set("IconWidth", GenericController.encodeInteger(XmlController.GetXMLAttribute(core, IsFound, PageInterfaceWithinLoop, "width", "0")));
                                                        cs.set("IconHeight", GenericController.encodeInteger(XmlController.GetXMLAttribute(core, IsFound, PageInterfaceWithinLoop, "height", "0")));
                                                        cs.set("IconSprites", GenericController.encodeInteger(XmlController.GetXMLAttribute(core, IsFound, PageInterfaceWithinLoop, "sprites", "0")));
                                                    }
                                                }
                                                break;
                                            case "includeaddon":
                                            case "includeadd-on":
                                            case "include addon":
                                            case "include add-on":
                                                //
                                                // processed in phase2 of this routine, after all the add-ons are installed
                                                //
                                                break;
                                            case "form":
                                                //
                                                // The value of this node is the xml instructions to create a form. Take then
                                                //   entire node, children and all, and save them in the formxml field.
                                                //   this replaces the settings add-on type, and soo to be report add-on types as well.
                                                //   this removes the ccsettingpages and settingcollectionrules, etc.
                                                //
                                                {
                                                    cs.set("formxml", PageInterfaceWithinLoop.InnerXml);
                                                }
                                                break;
                                            case "javascript":
                                            case "javascriptinhead":
                                                //
                                                // these all translate to JSFilename
                                                //
                                                fieldName = "jsfilename";
                                                cs.set(fieldName, PageInterfaceWithinLoop.InnerText);

                                                break;
                                            case "iniframe":
                                                //
                                                // typo - field is inframe
                                                //
                                                fieldName = "inframe";
                                                cs.set(fieldName, PageInterfaceWithinLoop.InnerText);
                                                break;
                                            case "diagnostic": {
                                                    bool fieldValue = encodeBoolean(PageInterfaceWithinLoop.InnerText);
                                                    cs.set("diagnostic", fieldValue);
                                                    collectionIncludesDiagnosticAddons = collectionIncludesDiagnosticAddons || fieldValue;
                                                }
                                                break;
                                            default:
                                                //
                                                // All the other fields should match the Db fields
                                                //
                                                fieldName = PageInterfaceWithinLoop.Name;
                                                FieldValue = PageInterfaceWithinLoop.InnerText;
                                                if (!cs.isFieldSupported(fieldName)) {
                                                    //
                                                    // Bad field name - need to report it somehow
                                                    //
                                                    LogController.logError(core, new ApplicationException("bad field found [" + fieldName + "], in addon node [" + addonName + "], of collection [" + MetadataController.getRecordName(core, "add-on collections", CollectionID) + "]"));
                                                } else {
                                                    cs.set(fieldName, FieldValue);
                                                }
                                                break;
                                        }
                                    }
                                }
                            }
                            cs.set("ArgumentList", ArgumentList);
                            cs.set("StylesFilename", StyleSheet);
                        }
                        cs.close();
                    }
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
        }
        //
        //======================================================================================================
        /// <summary>
        /// process the include add-on node of the add-on nodes. 
        /// this is the second pass, so all add-ons should be added
        /// no errors for missing addones, except the include add-on case
        /// </summary>
        private static string setAddonDependencies(CoreController core, XmlNode AddonNode, string AddonGuidFieldName, string ignore_BuildVersion, int CollectionID, ref bool ReturnUpgradeOK, ref string ReturnErrorMessage) {
            string result = "";
            try {
                string Basename = GenericController.vbLCase(AddonNode.Name);
                if ((Basename == "page") || (Basename == "process") || (Basename == "addon") || (Basename == "add-on")) {
                    bool IsFound = false;
                    string AOName = XmlController.GetXMLAttribute(core, IsFound, AddonNode, "name", "No Name");
                    if (string.IsNullOrEmpty(AOName)) { AOName = "No Name"; }
                    string AOGuid = XmlController.GetXMLAttribute(core, IsFound, AddonNode, "guid", AOName);
                    if (string.IsNullOrEmpty(AOGuid)) { AOGuid = AOName; }
                    string AddOnType = XmlController.GetXMLAttribute(core, IsFound, AddonNode, "type", "");
                    string Criteria = "(" + AddonGuidFieldName + "=" + DbController.encodeSQLText(AOGuid) + ")";
                    using (var csData = new CsModel(core)) {
                        if (csData.open(AddonModel.tableMetadata.contentName, Criteria, "", false)) {
                            //
                            // Update the Addon
                            LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", UpgradeAppFromLocalCollection, GUID match with existing Add-on, Updating Add-on [" + AOName + "], Guid [" + AOGuid + "]");
                        } else {
                            //
                            // not found by GUID - search name against name to update legacy Add-ons
                            Criteria = "(name=" + DbController.encodeSQLText(AOName) + ")and(" + AddonGuidFieldName + " is null)";
                            csData.open(AddonModel.tableMetadata.contentName, Criteria, "", false);
                        }
                        if (!csData.ok()) {
                            //
                            // Could not find add-on
                            LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", UpgradeAppFromLocalCollection, Add-on could not be created, skipping Add-on [" + AOName + "], Guid [" + AOGuid + "]");
                        } else {
                            if (AddonNode.ChildNodes.Count > 0) {
                                foreach (XmlNode PageInterface in AddonNode.ChildNodes) {
                                    switch (GenericController.vbLCase(PageInterface.Name)) {
                                        case "includeaddon":
                                        case "includeadd-on":
                                        case "include addon":
                                        case "include add-on":
                                            //
                                            // include add-ons - NOTE - import collections must be run before interfaces
                                            // when importing a collectin that will be used for an include
                                            //
                                            {
                                                string IncludeAddonName = XmlController.GetXMLAttribute(core, IsFound, PageInterface, "name", "");
                                                string IncludeAddonGuid = XmlController.GetXMLAttribute(core, IsFound, PageInterface, "guid", IncludeAddonName);
                                                int IncludeAddonId = 0;
                                                Criteria = "";
                                                if (!string.IsNullOrEmpty(IncludeAddonGuid)) {
                                                    Criteria = AddonGuidFieldName + "=" + DbController.encodeSQLText(IncludeAddonGuid);
                                                    if (string.IsNullOrEmpty(IncludeAddonName)) {
                                                        IncludeAddonName = "Add-on " + IncludeAddonGuid;
                                                    }
                                                } else if (!string.IsNullOrEmpty(IncludeAddonName)) {
                                                    Criteria = "(name=" + DbController.encodeSQLText(IncludeAddonName) + ")";
                                                }
                                                if (!string.IsNullOrEmpty(Criteria)) {
                                                    using (var CS2 = new CsModel(core)) {
                                                        CS2.open(AddonModel.tableMetadata.contentName, Criteria);
                                                        if (CS2.ok()) {
                                                            IncludeAddonId = CS2.getInteger("ID");
                                                        }
                                                    }
                                                    bool AddRule = false;
                                                    if (IncludeAddonId == 0) {
                                                        string UserError = "The include add-on [" + IncludeAddonName + "] could not be added because it was not found. If it is in the collection being installed, it must appear before any add-ons that include it.";
                                                        LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", UpgradeAddFromLocalCollection_InstallAddonNode, UserError [" + UserError + "]");
                                                        ReturnUpgradeOK = false;
                                                        ReturnErrorMessage = ReturnErrorMessage + "<P>The collection was not installed because the add-on [" + AOName + "] requires an included add-on [" + IncludeAddonName + "] which could not be found. If it is in the collection being installed, it must appear before any add-ons that include it.</P>";
                                                    } else {
                                                        using (var cs3 = new CsModel(core)) {
                                                            AddRule = !cs3.openSql("select ID from ccAddonIncludeRules where Addonid=" + csData.getInteger("id") + " and IncludedAddonID=" + IncludeAddonId);
                                                        }
                                                    }
                                                    if (AddRule) {
                                                        using (var cs3 = new CsModel(core)) {
                                                            cs3.insert("Add-on Include Rules");
                                                            if (cs3.ok()) {
                                                                cs3.set("Addonid", csData.getInteger("id"));
                                                                cs3.set("IncludedAddonID", IncludeAddonId);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
            return result;
            //
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
            logger.Log(LogLevel.Info, LogController.getMessageLine(core, string.Join(",", contextLog)));
        }
    }
}
