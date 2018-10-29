
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

namespace Contensive.Processor.Controllers {
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
    public class CollectionController {
        //
        //
        //====================================================================================================
        /// <summary>
        /// download a collection from the collection library to a path
        /// </summary>
        /// <param name="core"></param>
        /// <param name="workingPath"></param>
        /// <param name="CollectionGuid"></param>
        /// <param name="return_CollectionLastChangeDate"></param>
        /// <param name="return_ErrorMessage"></param>
        /// <returns></returns>
        private static bool downloadCollectionFiles(CoreController core, string workingPath, string CollectionGuid, ref DateTime return_CollectionLastChangeDate, ref string return_ErrorMessage) {
            bool tempDownloadCollectionFiles = false;
            tempDownloadCollectionFiles = false;
            try {
                LogController.logInfo(core, "downloading collection [" + CollectionGuid + "]");
                //
                // Request the Download file for this collection
                XmlDocument Doc = new XmlDocument();
                string URL = "http://support.contensive.com/GetCollection?iv=" + core.codeVersion() + "&guid=" + CollectionGuid;
                string errorPrefix = "DownloadCollectionFiles, Error reading the collection library status file from the server for Collection [" + CollectionGuid + "], download URL [" + URL + "]. ";
                int downloadRetry = 0;
                int downloadDelay = 2000;
                const int downloadRetryMax = 3;
                do {
                    try {
                        tempDownloadCollectionFiles = true;
                        return_ErrorMessage = "";
                        //
                        // -- pause for a second between fetches to pace the server (<10 hits in 10 seconds)
                        Thread.Sleep(downloadDelay);
                        //
                        // -- download file
                        System.Net.WebRequest rq = System.Net.WebRequest.Create(URL);
                        rq.Timeout = 60000;
                        System.Net.WebResponse response = rq.GetResponse();
                        Stream responseStream = response.GetResponseStream();
                        XmlTextReader reader = new XmlTextReader(responseStream);
                        Doc.Load(reader);
                        break;
                    } catch (Exception ex) {
                        //
                        // this error could be data related, and may not be critical. log issue and continue
                        downloadDelay += 2000;
                        return_ErrorMessage = "There was an error while requesting the download details for collection [" + CollectionGuid + "]";
                        tempDownloadCollectionFiles = false;
                        LogController.logInfo(core, errorPrefix + "There was a parse error reading the response [" + ex.ToString() + "]");
                    }
                    downloadRetry += 1;
                } while (downloadRetry < downloadRetryMax);
                if (string.IsNullOrEmpty(return_ErrorMessage)) {
                    //
                    // continue if no errors
                    if (Doc.DocumentElement.Name.ToLowerInvariant() != GenericController.vbLCase(DownloadFileRootNode)) {
                        return_ErrorMessage = "The collection file from the server was not valid for collection [" + CollectionGuid + "]";
                        tempDownloadCollectionFiles = false;
                        LogController.logInfo(core, errorPrefix + "The response has a basename [" + Doc.DocumentElement.Name + "] but [" + DownloadFileRootNode + "] was expected.");
                    } else {
                        //
                        // Parse the Download File and download each file into the working folder
                        if (Doc.DocumentElement.ChildNodes.Count == 0) {
                            return_ErrorMessage = "The collection library status file from the server has a valid basename, but no childnodes.";
                            LogController.logInfo(core, errorPrefix + "The collection library status file from the server has a valid basename, but no childnodes. The collection was probably Not found");
                            tempDownloadCollectionFiles = false;
                        } else {
                            //
                            int CollectionFileCnt = 0;
                            foreach (XmlNode CDefSection in Doc.DocumentElement.ChildNodes) {
                                string ResourceFilename = null;
                                string ResourceLink = null;
                                string CollectionVersion = null;
                                string CollectionFileLink = null;
                                string Collectionname = null;
                                switch (GenericController.vbLCase(CDefSection.Name)) {
                                    case "collection":
                                        //
                                        // Read in the interfaces and save to Add-ons
                                        ResourceFilename = "";
                                        ResourceLink = "";
                                        Collectionname = "";
                                        CollectionGuid = "";
                                        CollectionVersion = "";
                                        CollectionFileLink = "";
                                        foreach (XmlNode CDefInterfaces in CDefSection.ChildNodes) {
                                            int Pos = 0;
                                            string UserError = null;
                                            switch (GenericController.vbLCase(CDefInterfaces.Name)) {
                                                case "name":
                                                    Collectionname = CDefInterfaces.InnerText;
                                                    break;
                                                case "help":
                                                    if (!string.IsNullOrWhiteSpace(CDefInterfaces.InnerText)) {
                                                        core.privateFiles.saveFile(workingPath + "Collection.hlp", CDefInterfaces.InnerText);
                                                    }
                                                    break;
                                                case "guid":
                                                    CollectionGuid = CDefInterfaces.InnerText;
                                                    break;
                                                case "lastchangedate":
                                                    return_CollectionLastChangeDate = GenericController.encodeDate(CDefInterfaces.InnerText);
                                                    break;
                                                case "version":
                                                    CollectionVersion = CDefInterfaces.InnerText;
                                                    break;
                                                case "collectionfilelink":
                                                    CollectionFileLink = CDefInterfaces.InnerText;
                                                    CollectionFileCnt = CollectionFileCnt + 1;
                                                    if (!string.IsNullOrEmpty(CollectionFileLink)) {
                                                        Pos = CollectionFileLink.LastIndexOf("/") + 1;
                                                        if ((Pos <= 0) && (Pos < CollectionFileLink.Length)) {
                                                            //
                                                            // Skip this file because the collecion file link has no slash (no file)
                                                            LogController.logInfo(core, errorPrefix + "Collection [" + Collectionname + "] was not installed because the Collection File Link does not point to a valid file [" + CollectionFileLink + "]");
                                                        } else {
                                                            string CollectionFilePath = workingPath + CollectionFileLink.Substring(Pos);
                                                            core.privateFiles.saveHttpRequestToFile(CollectionFileLink, CollectionFilePath);
                                                        }
                                                    }
                                                    break;
                                                case "activexdll":
                                                case "resourcelink":
                                                    //
                                                    // save the filenames and download them only if OKtoinstall
                                                    ResourceFilename = "";
                                                    ResourceLink = "";
                                                    foreach (XmlNode ActiveXNode in CDefInterfaces.ChildNodes) {
                                                        switch (GenericController.vbLCase(ActiveXNode.Name)) {
                                                            case "filename":
                                                                ResourceFilename = ActiveXNode.InnerText;
                                                                break;
                                                            case "link":
                                                                ResourceLink = ActiveXNode.InnerText;
                                                                break;
                                                        }
                                                    }
                                                    if (string.IsNullOrEmpty(ResourceLink)) {
                                                        UserError = "There was an error processing a collection in the download file [" + Collectionname + "]. An ActiveXDll node with filename [" + ResourceFilename + "] contained no 'Link' attribute.";
                                                        LogController.logInfo(core, errorPrefix + UserError);
                                                    } else {
                                                        if (string.IsNullOrEmpty(ResourceFilename)) {
                                                            //
                                                            // Take Filename from Link
                                                            Pos = ResourceLink.LastIndexOf("/") + 1;
                                                            if (Pos != 0) {
                                                                ResourceFilename = ResourceLink.Substring(Pos);
                                                            }
                                                        }
                                                        if (string.IsNullOrEmpty(ResourceFilename)) {
                                                            UserError = "There was an error processing a collection in the download file [" + Collectionname + "]. The ActiveX filename attribute was empty, and the filename could not be read from the link [" + ResourceLink + "].";
                                                            LogController.logInfo(core, errorPrefix + UserError);
                                                        } else {
                                                            core.privateFiles.saveHttpRequestToFile(ResourceLink, workingPath + ResourceFilename);
                                                        }
                                                    }
                                                    break;
                                            }
                                        }
                                        break;
                                }
                            }
                            if (CollectionFileCnt == 0) {
                                LogController.logInfo(core, errorPrefix + "The collection was requested and downloaded, but was not installed because the download file did not have a collection root node.");
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return tempDownloadCollectionFiles;
        }
        //
        //====================================================================================================
        /// <summary>
        /// install a collection given its guid. Will download if needed.
        /// </summary>
        /// <param name="core"></param>
        /// <param name="collectionGuid"></param>
        /// <param name="return_ErrorMessage"></param>
        /// <param name="ImportFromCollectionsGuidList"></param>
        /// <param name="IsNewBuild"></param>
        /// <param name="repair"></param>
        /// <param name="nonCriticalErrorList"></param>
        /// <param name="logPrefix"></param>
        /// <param name="blockCollectionList"></param>
        /// <returns></returns>
        public static bool installCollectionFromRemoteRepo(CoreController core, string collectionGuid, ref string return_ErrorMessage, string ImportFromCollectionsGuidList, bool IsNewBuild, bool repair, ref List<string> nonCriticalErrorList, string logPrefix, ref List<string> blockCollectionList) {
            bool UpgradeOK = true;
            try {
                if (string.IsNullOrWhiteSpace(collectionGuid)) {
                    LogController.logWarn(core, "installCollectionFromRemoteRepo, collectionGuid is null");
                } else {
                    //
                    // normalize guid
                    if (collectionGuid.Length < 38) {
                        if (collectionGuid.Length == 32) {
                            collectionGuid = collectionGuid.Left(8) + "-" + collectionGuid.Substring(8, 4) + "-" + collectionGuid.Substring(12, 4) + "-" + collectionGuid.Substring(16, 4) + "-" + collectionGuid.Substring(20);
                        }
                        if (collectionGuid.Length == 36) {
                            collectionGuid = "{" + collectionGuid + "}";
                        }
                    }
                    //
                    // Install it if it is not already here
                    //
                    string CollectionVersionFolderName = GetCollectionPath(core, collectionGuid);
                    if (string.IsNullOrEmpty(CollectionVersionFolderName)) {
                        //
                        // Download all files for this collection and build the collection folder(s)
                        //
                        string workingPath = core.addon.getPrivateFilesAddonPath() + "temp_" + GenericController.GetRandomInteger(core) + "\\";
                        core.privateFiles.createPath(workingPath);
                        //
                        DateTime CollectionLastChangeDate = default(DateTime);
                        UpgradeOK = downloadCollectionFiles(core, workingPath, collectionGuid, ref CollectionLastChangeDate, ref return_ErrorMessage);
                        if (!UpgradeOK) {
                            //UpgradeOK = UpgradeOK;
                        } else {
                            List<string> collectionGuidList = new List<string>();
                            UpgradeOK = buildLocalCollectionReposFromFolder(core, workingPath, CollectionLastChangeDate, ref collectionGuidList, ref return_ErrorMessage, false);
                            if (!UpgradeOK) {
                                //UpgradeOK = UpgradeOK;
                            }
                        }
                        //
                        core.privateFiles.deleteFolder(workingPath);
                    }
                    //
                    // Upgrade the server from the collection files
                    //
                    if (UpgradeOK) {
                        UpgradeOK = installCollectionFromLocalRepo(core, collectionGuid, core.siteProperties.dataBuildVersion, ref return_ErrorMessage, ImportFromCollectionsGuidList, IsNewBuild, repair, ref nonCriticalErrorList, logPrefix, ref blockCollectionList,true);
                        if (!UpgradeOK) {
                            //UpgradeOK = UpgradeOK;
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return UpgradeOK;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Upgrades all collections, registers and resets the server if needed
        /// </summary>
        public static bool upgradeLocalCollectionRepoFromRemoteCollectionRepo(CoreController core, ref string return_ErrorMessage, bool IsNewBuild, bool repair, ref List<string> nonCriticalErrorList, string logPrefix, ref List<string> blockCollectionList) {
            bool returnOk = true;
            try {
                bool localCollectionUpToDate = false;
                string SupportURL = null;
                string GuidList = null;
                DateTime CollectionLastChangeDate = default(DateTime);
                string workingPath = null;
                string LocalGuid = null;
                DateTime LocalLastChangeDate = default(DateTime);
                string LibName = "";
                bool LibSystem = false;
                string LibGUID = null;
                string LibLastChangeDateStr = null;
                string LibContensiveVersion = "";
                DateTime LibLastChangeDate = default(DateTime);
                XmlDocument LibraryCollections = new XmlDocument();
                string Copy = null;
                //
                //-----------------------------------------------------------------------------------------------
                //   Load LocalCollections from the Collections.xml file
                //-----------------------------------------------------------------------------------------------
                //
                var localCollectionStoreList = new List<CollectionStoreClass>();
                if ( getLocalCollectionStoreList(core, ref localCollectionStoreList, ref return_ErrorMessage)) {
                    if (localCollectionStoreList.Count > 0) {
                        //
                        // Request collection updates 10 at a time
                        //
                        int packageSize = 0;
                        int packageNumber = 0;
                        foreach ( var collectionStore in localCollectionStoreList ) {
                            GuidList = GuidList + "," + collectionStore.guid;
                            packageSize += 1;
                            if (( packageSize>=10 ) || ( collectionStore == localCollectionStoreList.Last())) {
                                packageNumber += 1;
                                //
                                // -- send package of 10, or the last set
                                if (!string.IsNullOrEmpty(GuidList)) {
                                    LogController.logInfo(core, "Fetch collection details for collections [" + GuidList + "]");
                                    GuidList = GuidList.Substring(1);
                                    //
                                    //-----------------------------------------------------------------------------------------------
                                    //   Load LibraryCollections from the Support Site
                                    //-----------------------------------------------------------------------------------------------
                                    //
                                    LibraryCollections = new XmlDocument();
                                    SupportURL = "http://support.contensive.com/GetCollectionList?iv=" + core.codeVersion() + "&guidlist=" + encodeRequestVariable(GuidList);
                                    bool loadOK = true;
                                    if ( packageNumber>1 ) {
                                        Thread.Sleep(2000);
                                    }
                                    try {
                                        LibraryCollections.Load(SupportURL);
                                    } catch (Exception) {
                                        Copy = "Error downloading or loading GetCollectionList from Support.";
                                        LogController.logInfo(core, Copy + ", the request was [" + SupportURL + "]");
                                        return_ErrorMessage = return_ErrorMessage + "<P>" + Copy + "</P>";
                                        returnOk = false;
                                        loadOK = false;
                                    }
                                    if (loadOK) {
                                        {
                                            if (GenericController.vbLCase(LibraryCollections.DocumentElement.Name) != GenericController.vbLCase(CollectionListRootNode)) {
                                                Copy = "The GetCollectionList support site remote method returned an xml file with an invalid root node, [" + LibraryCollections.DocumentElement.Name + "] was received and [" + CollectionListRootNode + "] was expected.";
                                                LogController.logInfo(core, Copy + ", the request was [" + SupportURL + "]");
                                                return_ErrorMessage = return_ErrorMessage + "<P>" + Copy + "</P>";
                                                returnOk = false;
                                            } else {
                                                //
                                                // -- Search for Collection Updates Needed
                                                foreach (var localTestCollection in localCollectionStoreList) {
                                                    localCollectionUpToDate = false;
                                                    LocalGuid = localTestCollection.guid.ToLowerInvariant();
                                                    LocalLastChangeDate = localTestCollection.lastChangeDate;
                                                    //
                                                    // go through each collection on the Library and find the local collection guid
                                                    //
                                                    foreach (XmlNode LibListNode in LibraryCollections.DocumentElement.ChildNodes) {
                                                        if (localCollectionUpToDate) {
                                                            break;
                                                        }
                                                        switch (GenericController.vbLCase(LibListNode.Name)) {
                                                            case "collection":
                                                                LibGUID = "";
                                                                LibLastChangeDateStr = "";
                                                                LibLastChangeDate = DateTime.MinValue;
                                                                foreach (XmlNode CollectionNode in LibListNode.ChildNodes) {
                                                                    switch (GenericController.vbLCase(CollectionNode.Name)) {
                                                                        case "name":
                                                                            //
                                                                            LibName = GenericController.vbLCase(CollectionNode.InnerText);
                                                                            break;
                                                                        case "system":
                                                                            //
                                                                            LibSystem = GenericController.encodeBoolean(CollectionNode.InnerText);
                                                                            break;
                                                                        case "guid":
                                                                            //
                                                                            LibGUID = GenericController.vbLCase(CollectionNode.InnerText);
                                                                            //LibGUID = genericController.vbReplace(LibGUID, "{", "")
                                                                            //LibGUID = genericController.vbReplace(LibGUID, "}", "")
                                                                            //LibGUID = genericController.vbReplace(LibGUID, "-", "")
                                                                            break;
                                                                        case "lastchangedate":
                                                                            //
                                                                            LibLastChangeDateStr = CollectionNode.InnerText;
                                                                            //LibLastChangeDateStr = LibLastChangeDateStr;
                                                                            break;
                                                                        case "contensiveversion":
                                                                            //
                                                                            LibContensiveVersion = CollectionNode.InnerText;
                                                                            break;
                                                                    }
                                                                }
                                                                if (!string.IsNullOrEmpty(LibGUID)) {
                                                                    if ((!string.IsNullOrEmpty(LibGUID)) && (LibGUID == LocalGuid) && ((string.IsNullOrEmpty(LibContensiveVersion)) || (string.CompareOrdinal(LibContensiveVersion, core.codeVersion()) <= 0))) {
                                                                        LogController.logInfo(core, "verify collection [" + LibGUID + "]");
                                                                        //
                                                                        // LibCollection matches the LocalCollection - process the upgrade
                                                                        //
                                                                        if (GenericController.vbInstr(1, LibGUID, "58c9", 1) != 0) {
                                                                            //LibGUID = LibGUID;
                                                                        }
                                                                        if (!DateController.IsDate(LibLastChangeDateStr)) {
                                                                            LibLastChangeDate = DateTime.MinValue;
                                                                        } else {
                                                                            LibLastChangeDate = GenericController.encodeDate(LibLastChangeDateStr);
                                                                        }
                                                                        // TestPoint 1.1 - Test each collection for upgrade
                                                                        if (LibLastChangeDate > LocalLastChangeDate) {
                                                                            //
                                                                            // LibLastChangeDate <>0, and it is > local lastchangedate
                                                                            //
                                                                            workingPath = core.addon.getPrivateFilesAddonPath() + "\\temp_" + GenericController.GetRandomInteger(core) + "\\";
                                                                            LogController.logInfo(core, "Upgrading Collection [" + LibGUID + "], Library name [" + LibName + "], because LocalChangeDate [" + LocalLastChangeDate + "] < LibraryChangeDate [" + LibLastChangeDate + "]");
                                                                            //
                                                                            // Upgrade Needed
                                                                            //
                                                                            core.privateFiles.createPath(workingPath);
                                                                            //
                                                                            returnOk = downloadCollectionFiles(core, workingPath, LibGUID, ref CollectionLastChangeDate, ref return_ErrorMessage);
                                                                            if (returnOk) {
                                                                                List<string> listGuidList = new List<string>();
                                                                                returnOk = buildLocalCollectionReposFromFolder(core, workingPath, CollectionLastChangeDate, ref listGuidList, ref return_ErrorMessage, false);
                                                                            }
                                                                            //
                                                                            core.privateFiles.deleteFolder(workingPath);
                                                                            //
                                                                            // Upgrade the apps from the collection files, do not install on any apps
                                                                            //
                                                                            if (returnOk) {
                                                                                returnOk = installCollectionFromLocalRepo(core, LibGUID, core.siteProperties.dataBuildVersion, ref return_ErrorMessage, "", IsNewBuild, repair, ref nonCriticalErrorList, logPrefix, ref blockCollectionList, true);
                                                                            }
                                                                            //
                                                                            // make sure this issue is logged and clear the flag to let other local collections install
                                                                            //
                                                                            if (!returnOk) {
                                                                                LogController.logInfo(core, "There was a problem upgrading Collection [" + LibGUID + "], Library name [" + LibName + "], error message [" + return_ErrorMessage + "], will clear error and continue with the next collection, the request was [" + SupportURL + "]");
                                                                                returnOk = true;
                                                                            }
                                                                        }
                                                                        //
                                                                        // this local collection has been resolved, go to the next local collection
                                                                        //
                                                                        localCollectionUpToDate = true;
                                                                        //
                                                                        if (!returnOk) {
                                                                            LogController.logInfo(core, "There was a problem upgrading Collection [" + LibGUID + "], Library name [" + LibName + "], error message [" + return_ErrorMessage + "], will clear error and continue with the next collection");
                                                                            returnOk = true;
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
                                }
                                packageSize = 0;
                                GuidList = "";
                            }
                        }
                    }
                };
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return returnOk;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Upgrade a collection from the files in a working folder
        /// </summary>
        public static bool buildLocalCollectionReposFromFolder(CoreController core, string sourcePrivateFolderPath, DateTime CollectionLastChangeDate, ref List<string> return_CollectionGUIDList, ref string return_ErrorMessage, bool allowLogging) {
            bool success = false;
            try {
                if (core.privateFiles.pathExists(sourcePrivateFolderPath)) {
                    LogController.logInfo(core, "BuildLocalCollectionFolder, processing files in private folder [" + sourcePrivateFolderPath + "]");
                    List<CPFileSystemClass.FileDetail> SrcFileNamelist = core.privateFiles.getFileList(sourcePrivateFolderPath);
                    foreach (CPFileSystemClass.FileDetail file in SrcFileNamelist) {
                        if ((file.Extension == ".zip") || (file.Extension == ".xml")) {
                            string collectionGuid = "";
                            success = buildLocalCollectionRepoFromFile(core, sourcePrivateFolderPath + file.Name, CollectionLastChangeDate, ref collectionGuid, ref return_ErrorMessage, allowLogging);
                            return_CollectionGUIDList.Add(collectionGuid);
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return success;
        }
        //
        //====================================================================================================
        //
        public static bool buildLocalCollectionRepoFromFile(CoreController core, string collectionPathFilename, DateTime CollectionLastChangeDate, ref string return_CollectionGUID, ref string return_ErrorMessage, bool allowLogging) {
            bool tempBuildLocalCollectionRepoFromFile = false;
            bool result = true;
            try {
                //XmlDocument Doc = new XmlDocument();
                //collectionXmlController XMLTools = new collectionXmlController(core);
                //
                // process all xml files in this workingfolder
                //
                if (allowLogging) {
                    LogController.logInfo(core, "BuildLocalCollectionFolder(), Enter");
                }
                string collectionPath = "";
                string collectionFilename = "";
                //
                core.privateFiles.splitDosPathFilename(collectionPathFilename, ref collectionPath, ref collectionFilename);
                string CollectionVersionFolderName = "";
                string Collectionname = "";
                string CollectionGuid = "";
                if (!core.privateFiles.pathExists(collectionPath)) {
                    //
                    // The working folder is not there
                    //
                    result = false;
                    return_ErrorMessage = "<p>There was a problem with the installation. The installation folder is not valid.</p>";
                    if (allowLogging) {
                        LogController.logInfo(core, "BuildLocalCollectionFolder(), " + return_ErrorMessage);
                    }
                    LogController.logInfo(core, "BuildLocalCollectionFolder, CheckFileFolder was false for the private folder [" + collectionPath + "]");
                } else {
                    LogController.logInfo(core, "BuildLocalCollectionFolder, processing files in private folder [" + collectionPath + "]");
                    //
                    // move collection file to a temp directory
                    //
                    string tmpInstallPath = "tmpInstallCollection" + GenericController.getGUIDString() + "\\";
                    core.privateFiles.copyFile(collectionPathFilename, tmpInstallPath + collectionFilename);
                    if (collectionFilename.ToLowerInvariant().Substring(collectionFilename.Length - 4) == ".zip") {
                        core.privateFiles.UnzipFile(tmpInstallPath + collectionFilename);
                        core.privateFiles.deleteFile(tmpInstallPath + collectionFilename);
                    }
                    //
                    // install the individual files
                    //
                    List<FileDetail> SrcFileNamelist = core.privateFiles.getFileList(tmpInstallPath);
                    if (true) {
                        bool CollectionFileFound = false;
                        //
                        // Process all non-zip files
                        //
                        foreach (FileDetail file in SrcFileNamelist) {
                            string Filename = file.Name;
                            LogController.logInfo(core, "BuildLocalCollectionFolder, processing files, filename=[" + Filename + "]");
                            if (GenericController.vbLCase(Filename.Substring(Filename.Length - 4)) == ".xml") {
                                //
                                LogController.logInfo(core, "BuildLocalCollectionFolder, processing xml file [" + Filename + "]");
                                XmlDocument CollectionFile = new XmlDocument();
                                //hint = hint & ",320"
                                CollectionFile = new XmlDocument();
                                bool loadOk = true;
                                try {
                                    CollectionFile.LoadXml(core.privateFiles.readFileText(tmpInstallPath + Filename));
                                } catch (Exception ex) {
                                    //
                                    // There was a parse error in this xml file. Set the return message and the flag
                                    // If another xml files shows up, and process OK it will cover this error
                                    //
                                    //hint = hint & ",330"
                                    return_ErrorMessage = "There was a problem installing the Collection File [" + tmpInstallPath + Filename + "]. The error reported was [" + ex.Message + "].";
                                    LogController.logInfo(core, "BuildLocalCollectionFolder, error reading collection [" + collectionPathFilename + "]");
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
                                        LogController.logInfo(core, "BuildLocalCollectionFolder, xml base name wrong [" + CollectionFileBaseName + "]");
                                    } else {
                                        bool IsFound = false;
                                        //
                                        // Collection File
                                        //
                                        //hint = hint & ",420"
                                        Collectionname = XmlController.GetXMLAttribute(core, IsFound, CollectionFile.DocumentElement, "name", "");
                                        if (string.IsNullOrEmpty(Collectionname)) {
                                            //
                                            // ----- Error condition -- it must have a collection name
                                            //
                                            result = false;
                                            return_ErrorMessage = "<p>There was a problem with this Collection. The collection file does not have a collection name.</p>";
                                            LogController.logInfo(core, "BuildLocalCollectionFolder, collection has no name");
                                        } else {
                                            //
                                            //------------------------------------------------------------------
                                            // Build Collection folder structure in /Add-ons folder
                                            //------------------------------------------------------------------
                                            //
                                            //hint = hint & ",440"
                                            CollectionFileFound = true;
                                            CollectionGuid = XmlController.GetXMLAttribute(core, IsFound, CollectionFile.DocumentElement, "guid", Collectionname);
                                            if (string.IsNullOrEmpty(CollectionGuid)) {
                                                //
                                                // I hope I do not regret this
                                                //
                                                CollectionGuid = Collectionname;
                                            }
                                            CollectionGuid = CollectionGuid.ToLowerInvariant();
                                            CollectionVersionFolderName = GetCollectionPath(core, CollectionGuid);
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
                                                CollectionFolderName = CollectionGuid;
                                                CollectionFolderName = GenericController.vbReplace(CollectionFolderName, "{", "");
                                                CollectionFolderName = GenericController.vbReplace(CollectionFolderName, "}", "");
                                                CollectionFolderName = GenericController.vbReplace(CollectionFolderName, "-", "");
                                                CollectionFolderName = GenericController.vbReplace(CollectionFolderName, " ", "");
                                                CollectionFolderName = Collectionname + "_" + CollectionFolderName;
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

                                            core.privateFiles.copyFolder(tmpInstallPath, CollectionVersionFolder);
                                            //StatusOK = True
                                            //
                                            // Install activeX and search for importcollections
                                            //
                                            //hint = hint & ",500"
                                            foreach (XmlNode CDefSection in CollectionFile.DocumentElement.ChildNodes) {
                                                string ChildCollectionGUID = null;
                                                string ChildCollectionName = null;
                                                bool Found = false;
                                                switch (GenericController.vbLCase(CDefSection.Name)) {
                                                    case "resource":
                                                        //
                                                        // resource node, if executable node, save to RegisterList
                                                        //
                                                        //hint = hint & ",510"
                                                        //ResourceType = genericController.vbLCase(xmlController.GetXMLAttribute(core, IsFound, CDefSection, "type", ""))
                                                        //Dim resourceFilename As String = Trim(xmlController.GetXMLAttribute(core, IsFound, CDefSection, "name", ""))
                                                        //Dim resourcePathFilename As String = CollectionVersionPath & resourceFilename
                                                        //If resourceFilename = "" Then
                                                        //    '
                                                        //    ' filename is blank
                                                        //    '
                                                        //    'hint = hint & ",511"
                                                        //ElseIf Not core.privateFiles.fileExists(resourcePathFilename) Then
                                                        //    '
                                                        //    ' resource is not here
                                                        //    '
                                                        //    'hint = hint & ",513"
                                                        //    result = False
                                                        //    return_ErrorMessage = "<p>There was a problem with the Collection File. The resource referenced in the collection file [" & resourceFilename & "] was not included in the resource files.</p>"
                                                        //    Call logController.appendInstallLog(core, "BuildLocalCollectionFolder, The resource referenced in the collection file [" & resourceFilename & "] was not included in the resource files.")
                                                        //    'StatusOK = False
                                                        //Else
                                                        //    Select Case ResourceType
                                                        //        Case "executable"
                                                        //            '
                                                        //            ' Executable resources - add to register list
                                                        //            '
                                                        //            'hint = hint & ",520"
                                                        //            If False Then
                                                        //                '
                                                        //                ' file is already installed
                                                        //                '
                                                        //                'hint = hint & ",521"
                                                        //            Else
                                                        //                '
                                                        //                ' Add the file to be registered
                                                        //                '
                                                        //            End If
                                                        //        Case "www"
                                                        //        Case "file"
                                                        //    End Select
                                                        //End If
                                                        break;
                                                    case "interfaces":
                                                        //
                                                        // Compatibility only - this is deprecated - Install ActiveX found in Add-ons
                                                        //
                                                        //hint = hint & ",530"
                                                        //For Each CDefInterfaces In CDefSection.ChildNodes
                                                        //    AOName =xmlController.GetXMLAttribute(core, IsFound, CDefInterfaces, "name", "No Name")
                                                        //    If AOName = "" Then
                                                        //        AOName = "No Name"
                                                        //    End If
                                                        //    AOGuid =xmlController.GetXMLAttribute(core, IsFound, CDefInterfaces, "guid", AOName)
                                                        //    If AOGuid = "" Then
                                                        //        AOGuid = AOName
                                                        //    End If
                                                        //Next
                                                        break;
                                                    case "getcollection":
                                                    case "importcollection":
                                                        //
                                                        // -- Download Collection file into install folder
                                                        ChildCollectionName = XmlController.GetXMLAttribute(core, Found, CDefSection, "name", "");
                                                        ChildCollectionGUID = XmlController.GetXMLAttribute(core, Found, CDefSection, "guid", CDefSection.InnerText);
                                                        if (string.IsNullOrEmpty(ChildCollectionGUID)) {
                                                            ChildCollectionGUID = CDefSection.InnerText;
                                                        }
                                                        string statusMsg = "Installing collection [" + ChildCollectionName + ", " + ChildCollectionGUID + "] referenced from collection [" + Collectionname + "]";
                                                        LogController.logInfo(core, "BuildLocalCollectionFolder, getCollection or importcollection, childCollectionName [" + ChildCollectionName + "], childCollectionGuid [" + ChildCollectionGUID + "]");
                                                        if (GenericController.vbInstr(1, CollectionVersionPath, ChildCollectionGUID, 1) == 0) {
                                                            if (string.IsNullOrEmpty(ChildCollectionGUID)) {
                                                                //
                                                                // -- Needs a GUID to install
                                                                result = false;
                                                                return_ErrorMessage = statusMsg + ". The installation can not continue because an imported collection could not be downloaded because it does not include a valid GUID.";
                                                                LogController.logInfo(core, "BuildLocalCollectionFolder, return message [" + return_ErrorMessage + "]");
                                                            } else if (GetCollectionPath(core, ChildCollectionGUID) == "") {
                                                                LogController.logInfo(core, "BuildLocalCollectionFolder, [" + ChildCollectionGUID + "], not found so needs to be installed");
                                                                //
                                                                // If it is not already installed, download and install it also
                                                                //
                                                                string ChildWorkingPath = CollectionVersionPath + "\\" + ChildCollectionGUID + "\\";
                                                                DateTime ChildCollectionLastChangeDate = default(DateTime);
                                                                //
                                                                // down an imported collection file
                                                                //
                                                                bool StatusOK = downloadCollectionFiles(core, ChildWorkingPath, ChildCollectionGUID, ref ChildCollectionLastChangeDate, ref return_ErrorMessage);
                                                                if (!StatusOK) {

                                                                    LogController.logInfo(core, "BuildLocalCollectionFolder, [" + statusMsg + "], downloadCollectionFiles returned error state, message [" + return_ErrorMessage + "]");
                                                                    if (string.IsNullOrEmpty(return_ErrorMessage)) {
                                                                        return_ErrorMessage = statusMsg + ". The installation can not continue because there was an unknown error while downloading the necessary collection file, [" + ChildCollectionGUID + "].";
                                                                    } else {
                                                                        return_ErrorMessage = statusMsg + ". The installation can not continue because there was an error while downloading the necessary collection file, guid [" + ChildCollectionGUID + "]. The error was [" + return_ErrorMessage + "]";
                                                                    }
                                                                } else {
                                                                    LogController.logInfo(core, "BuildLocalCollectionFolder, [" + ChildCollectionGUID + "], downloadCollectionFiles returned OK");
                                                                    //
                                                                    // install the downloaded file
                                                                    //
                                                                    List<string> ChildCollectionGUIDList = new List<string>();
                                                                    StatusOK = buildLocalCollectionReposFromFolder(core, ChildWorkingPath, ChildCollectionLastChangeDate, ref ChildCollectionGUIDList, ref return_ErrorMessage, allowLogging);
                                                                    if (!StatusOK) {
                                                                        LogController.logInfo(core, "BuildLocalCollectionFolder, [" + statusMsg + "], BuildLocalCollectionFolder returned error state, message [" + return_ErrorMessage + "]");
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
                                                            } else {
                                                                //
                                                                //
                                                                //
                                                                LogController.logInfo(core, "BuildLocalCollectionFolder, [" + ChildCollectionGUID + "], already installed");
                                                            }
                                                        }
                                                        break;
                                                }
                                                if (!string.IsNullOrEmpty(return_ErrorMessage)) {
                                                    //
                                                    // if error, no more nodes in this collection file
                                                    //
                                                    result = false;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(return_ErrorMessage)) {
                                //
                                // if error, no more files
                                //
                                result = false;
                                break;
                            }
                        }
                        if ((string.IsNullOrEmpty(return_ErrorMessage)) && (!CollectionFileFound)) {
                            bool ZipFileFound = false;
                            //
                            // no errors, but the collection file was not found
                            //
                            if (ZipFileFound) {
                                //
                                // zip file found but did not qualify
                                //
                                return_ErrorMessage = "<p>There was a problem with the installation. The collection zip file was downloaded, but it did not include a valid collection xml file.</p>";
                            } else {
                                //
                                // zip file not found
                                //
                                return_ErrorMessage = "<p>There was a problem with the installation. The collection zip was not downloaded successfully.</p>";
                            }
                            //StatusOK = False
                        }
                    }
                    //
                    // delete the working folder
                    //
                    core.privateFiles.deleteFolder(tmpInstallPath);
                }
                //
                // If the collection parsed correctly, update the Collections.xml file
                //
                if (string.IsNullOrEmpty(return_ErrorMessage)) {
                    UpdateConfig(core, Collectionname, CollectionGuid, CollectionLastChangeDate, CollectionVersionFolderName);
                } else {
                    //
                    // there was an error processing the collection, be sure to save description in the log
                    //
                    result = false;
                    LogController.logInfo(core, "BuildLocalCollectionFolder, ERROR Exiting, ErrorMessage [" + return_ErrorMessage + "]");
                }
                //
                LogController.logInfo(core, "BuildLocalCollectionFolder, Exiting with ErrorMessage [" + return_ErrorMessage + "]");
                //
                tempBuildLocalCollectionRepoFromFile = (string.IsNullOrEmpty(return_ErrorMessage));
                return_CollectionGUID = CollectionGuid;
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Upgrade Application from a local collection
        /// </summary>
        public static bool installCollectionFromLocalRepo(CoreController core, string CollectionGuid, string ignore_BuildVersion, ref string return_ErrorMessage, string ImportFromCollectionsGuidList, bool IsNewBuild, bool repair, ref List<string> nonCriticalErrorList, string logPrefix, ref List<string> blockCollectionList, bool includeCDefInstall) {
            bool result = false;
            try {
                //
                LogController.logInfo(core, "installCollectionFromLocalRep [" + CollectionGuid + "]");
                //
                string CollectionVersionFolderName = "";
                DateTime CollectionLastChangeDate = default(DateTime);
                string tempVar = "";
                getCollectionConfig(core, CollectionGuid, ref CollectionVersionFolderName, ref CollectionLastChangeDate, ref tempVar);
                if (string.IsNullOrEmpty(CollectionVersionFolderName)) {
                    LogController.logInfo(core, "installCollectionFromLocalRep [" + CollectionGuid + "], collection folder not found.");
                    return_ErrorMessage = return_ErrorMessage + "<P>The collection was not installed from the local collections because the folder containing the Add-on's resources could not be found. It may not be installed locally.</P>";
                } else {
                    //
                    // Search Local Collection Folder for collection config file (xml file)
                    //
                    string CollectionVersionFolder = core.addon.getPrivateFilesAddonPath() + CollectionVersionFolderName + "\\";
                    List<FileDetail> srcFileInfoArray = core.privateFiles.getFileList(CollectionVersionFolder);
                    if (srcFileInfoArray.Count == 0) {
                        LogController.logInfo(core, "installCollectionFromLocalRep [" + CollectionGuid + "], collection folder is empty.");
                        return_ErrorMessage = return_ErrorMessage + "<P>The collection was not installed because the folder containing the Add-on's resources was empty.</P>";
                    } else {
                        //
                        // collect list of DLL files and add them to the exec files if they were missed
                        List<string> assembliesInZip = new List<string>();
                        foreach (FileDetail file in srcFileInfoArray) {
                            if (file.Extension.ToLowerInvariant() == "dll") {
                                if (!assembliesInZip.Contains(file.Name.ToLowerInvariant())) {
                                    LogController.logInfo(core, "installCollectionFromLocalRep [" + CollectionGuid + "], adding DLL from folder[" + file.Name.ToLowerInvariant() + "].");
                                    assembliesInZip.Add(file.Name.ToLowerInvariant());
                                }
                            }
                        }
                        //
                        // -- Process the other files
                        LogController.logInfo(core, "installCollectionFromLocalRep [" + CollectionGuid + "], process xml files.");
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
                                    LogController.logInfo(core, "installCollectionFromLocalRep, skipping xml file, not valid collection metadata, [" + core.privateFiles.localAbsRootPath + CollectionVersionFolder + file.Name + "].");
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
                                        string Collectionname =XmlController.GetXMLAttribute(core, IsFound, Doc.DocumentElement, "name", "");
                                        if (string.IsNullOrEmpty(Collectionname)) {
                                            //
                                            // ----- Error condition -- it must have a collection name
                                            //
                                            //Call AppendAddonLog("UpgradeAppFromLocalCollection, collection has no name")
                                            LogController.logInfo(core, "installCollectionFromLocalRep [" + Collectionname + "], collection has no name");
                                            return_ErrorMessage = return_ErrorMessage + "<P>The collection was not installed because the collection name in the xml collection file is blank</P>";
                                        } else {
                                            bool CollectionSystem_fileValueOK = false;
                                            bool CollectionUpdatable_fileValueOK = false;
                                            //												Dim CollectionblockNavigatorNode_fileValueOK As Boolean
                                            bool CollectionSystem = GenericController.encodeBoolean(XmlController.GetXMLAttribute(core, CollectionSystem_fileValueOK, Doc.DocumentElement, "system", ""));
                                            int Parent_NavID = AppBuilderController.verifyNavigatorEntry(core, addonGuidManageAddon, "", "Manage Add-ons", "", "", "", false, false, false, true, "", "", "", 0);
                                            bool CollectionUpdatable = GenericController.encodeBoolean(XmlController.GetXMLAttribute(core, CollectionUpdatable_fileValueOK, Doc.DocumentElement, "updatable", ""));
                                            bool CollectionblockNavigatorNode = GenericController.encodeBoolean(XmlController.GetXMLAttribute(core, CollectionblockNavigatorNode_fileValueOK, Doc.DocumentElement, "blockNavigatorNode", ""));
                                            string FileGuid =XmlController.GetXMLAttribute(core, IsFound, Doc.DocumentElement, "guid", Collectionname);
                                            if (string.IsNullOrEmpty(FileGuid)) {
                                                FileGuid = Collectionname;
                                            }
                                            if (CollectionGuid.ToLowerInvariant() != GenericController.vbLCase(FileGuid)) {
                                                //
                                                //
                                                //
                                                LogController.logInfo(core, "installCollectionFromLocalRep [" + Collectionname + "], Collection file contains incorrect GUID, correct GUID [" + CollectionGuid.ToLowerInvariant() + "], incorrect GUID in file [" + GenericController.vbLCase(FileGuid) + "]");
                                                return_ErrorMessage = return_ErrorMessage + "<P>The collection was not installed because the unique number identifying the collection, called the guid, does not match the collection requested.</P>";
                                            } else {
                                                if (string.IsNullOrEmpty(CollectionGuid)) {
                                                    //
                                                    // I hope I do not regret this
                                                    //
                                                    CollectionGuid = Collectionname;
                                                }
                                                string onInstallAddonGuid = "";
                                                //
                                                //-------------------------------------------------------------------------------
                                                LogController.logInfo(core, "installCollectionFromLocalRep [" + Collectionname + "], stage-1, save resourses and process collection dependencies");
                                                // Go through all collection nodes
                                                // Process ImportCollection Nodes - so includeaddon nodes will work
                                                // these must be processes regardless of the state of this collection in this app
                                                // Get Resource file list
                                                //-------------------------------------------------------------------------------
                                                //
                                                string wwwFileList = "";
                                                string ContentFileList = "";
                                                string ExecFileList = "";
                                                foreach (XmlNode CDefSection in Doc.DocumentElement.ChildNodes) {
                                                    switch (CDefSection.Name.ToLowerInvariant()) {
                                                        case "resource": {
                                                                //
                                                                // set wwwfilelist, contentfilelist, execfilelist
                                                                //
                                                                string resourceType = XmlController.GetXMLAttribute(core, IsFound, CDefSection, "type", "");
                                                                string resourcePath = XmlController.GetXMLAttribute(core, IsFound, CDefSection, "path", "");
                                                                string filename = XmlController.GetXMLAttribute(core, IsFound, CDefSection, "name", "");
                                                                //
                                                                LogController.logInfo(core, "installCollectionFromLocalRep [" + Collectionname + "], resource found, name [" + filename + "], type [" + resourceType + "], path [" + resourcePath + "]");
                                                                //
                                                                filename = GenericController.convertToDosSlash(filename);
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
                                                                        wwwFileList += "\r\n" + dstDosPath + filename;
                                                                        LogController.logInfo(core, "installCollectionFromLocalRep [" + Collectionname + "], GUID [" + CollectionGuid + "], pass 1, copying file to www, src [" + CollectionVersionFolder + SrcPath + "], dst [" + core.appConfig.localWwwPath + dstDosPath + "].");
                                                                        core.privateFiles.copyFile(CollectionVersionFolder + SrcPath + filename, dstDosPath + filename, core.appRootFiles);
                                                                        if (GenericController.vbLCase(filename.Substring(filename.Length - 4)) == ".zip") {
                                                                            LogController.logInfo(core, "installCollectionFromLocalRep [" + Collectionname + "], GUID [" + CollectionGuid + "], pass 1, unzipping www file [" + core.appConfig.localWwwPath + dstDosPath + filename + "].");
                                                                            core.appRootFiles.UnzipFile(dstDosPath + filename);
                                                                        }
                                                                        break;
                                                                    case "file":
                                                                    case "content":
                                                                        ContentFileList += "\r\n" + dstDosPath + filename;
                                                                        LogController.logInfo(core, "installCollectionFromLocalRep [" + Collectionname + "], GUID [" + CollectionGuid + "], pass 1, copying file to content, src [" + CollectionVersionFolder + SrcPath + "], dst [" + dstDosPath + "].");
                                                                        core.privateFiles.copyFile(CollectionVersionFolder + SrcPath + filename, dstDosPath + filename, core.cdnFiles);
                                                                        if (GenericController.vbLCase(filename.Substring(filename.Length - 4)) == ".zip") {
                                                                            LogController.logInfo(core, "installCollectionFromLocalRep [" + Collectionname + "], GUID [" + CollectionGuid + "], pass 1, unzipping content file [" + dstDosPath + filename + "].");
                                                                            core.cdnFiles.UnzipFile(dstDosPath + filename);
                                                                        }
                                                                        break;
                                                                    default:
                                                                        if (assembliesInZip.Contains(filename.ToLowerInvariant())) {
                                                                            assembliesInZip.Remove(filename.ToLowerInvariant());
                                                                        }
                                                                        ExecFileList = ExecFileList + "\r\n" + filename;
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
                                                                string ChildCollectionName = XmlController.GetXMLAttribute(core, Found, CDefSection, "name", "");
                                                                string ChildCollectionGUID = XmlController.GetXMLAttribute(core, Found, CDefSection, "guid", CDefSection.InnerText);
                                                                if (string.IsNullOrEmpty(ChildCollectionGUID)) {
                                                                    ChildCollectionGUID = CDefSection.InnerText;
                                                                }
                                                                if ((ImportFromCollectionsGuidList + "," + CollectionGuid).IndexOf(ChildCollectionGUID, System.StringComparison.OrdinalIgnoreCase) != -1) {
                                                                    //
                                                                    // circular import detected, this collection is already imported
                                                                    //
                                                                    LogController.logInfo(core, "installCollectionFromLocalRep [" + Collectionname + "], Circular import detected. This collection attempts to import a collection that had previously been imported. A collection can not import itself. The collection is [" + Collectionname + "], GUID [" + CollectionGuid + "], pass 1. The collection to be imported is [" + ChildCollectionName + "], GUID [" + ChildCollectionGUID + "]");
                                                                } else {
                                                                    installCollectionFromRemoteRepo(core, ChildCollectionGUID, ref return_ErrorMessage, ImportFromCollectionsGuidList, IsNewBuild, repair, ref nonCriticalErrorList, logPrefix, ref blockCollectionList);
                                                                    //if (true) {
                                                                    //    installCollectionFromRemoteRepo(core, ChildCollectionGUID, ref return_ErrorMessage, ImportFromCollectionsGuidList, IsNewBuild, ref nonCriticalErrorList);
                                                                    //} else {
                                                                    //    if (string.IsNullOrEmpty(ChildCollectionGUID)) {
                                                                    //        logController.appendInstallLog(core, "The importcollection node [" + ChildCollectionName + "] can not be upgraded because it does not include a valid guid.");
                                                                    //    } else {
                                                                    //        //
                                                                    //        // This import occurred while upgrading an application from the local collections (Db upgrade or AddonManager)
                                                                    //        // Its OK to install it if it is missing, but you do not need to upgrade the local collections from the Library
                                                                    //        //
                                                                    //        // 5/18/2008 -----------------------------------
                                                                    //        // See if it is in the local collections storage. If yes, just upgrade this app with it. If not,
                                                                    //        // it must be downloaded and the entire server must be upgraded
                                                                    //        //
                                                                    //        string ChildCollectionVersionFolderName = "";
                                                                    //        DateTime ChildCollectionLastChangeDate = default(DateTime);
                                                                    //        string tempVar2 = "";
                                                                    //        GetCollectionConfig(core, ChildCollectionGUID, ref ChildCollectionVersionFolderName, ref ChildCollectionLastChangeDate, ref tempVar2);
                                                                    //        if (!string.IsNullOrEmpty(ChildCollectionVersionFolderName)) {
                                                                    //            //
                                                                    //            // It is installed in the local collections, update just this site
                                                                    //            //
                                                                    //            result &= installCollectionFromLocalRepo(core, ChildCollectionGUID, core.siteProperties.dataBuildVersion, ref return_ErrorMessage, ImportFromCollectionsGuidList + "," + CollectionGuid, IsNewBuild, ref nonCriticalErrorList);
                                                                    //        }
                                                                    //    }
                                                                    //}
                                                                }
                                                                break;
                                                            }
                                                    }
                                                }
                                                //
                                                // -- any assemblies found in the zip that were not part of the resources section need to be added
                                                foreach (string filename in assembliesInZip) {
                                                    ExecFileList = ExecFileList + "\r\n" + filename;
                                                }
                                                //
                                                //-------------------------------------------------------------------------------
                                                LogController.logInfo(core, "installCollectionFromLocalRep [" + Collectionname + "], stage-2, determine if this collection is already installed");
                                                //-------------------------------------------------------------------------------
                                                //
                                                bool OKToInstall = false;
                                                AddonCollectionModel collection = AddonCollectionModel.create(core, CollectionGuid);
                                                if (collection != null) {
                                                    //
                                                    // Upgrade addon
                                                    //
                                                    if (CollectionLastChangeDate == DateTime.MinValue) {
                                                        LogController.logInfo(core, "installCollectionFromLocalRep [" + Collectionname + "], GUID [" + CollectionGuid + "], App has the collection, but the new version has no lastchangedate, so it will upgrade to this unknown (manual) version.");
                                                        OKToInstall = true;
                                                    } else if (collection.lastChangeDate < CollectionLastChangeDate) {
                                                        LogController.logInfo(core, "installCollectionFromLocalRep [" + Collectionname + "], GUID [" + CollectionGuid + "], App has an older version of collection. It will be upgraded.");
                                                        OKToInstall = true;
                                                    } else if(repair) {
                                                        LogController.logInfo(core, "installCollectionFromLocalRep [" + Collectionname + "], GUID [" + CollectionGuid + "], App has an up-to-date version of collection, but the repair option is true so it will be reinstalled.");
                                                        OKToInstall = true;
                                                    } else {
                                                        LogController.logInfo(core, "installCollectionFromLocalRep [" + Collectionname + "], GUID [" + CollectionGuid + "], App has an up-to-date version of collection. It will not be upgraded, but all imports in the new version will be checked.");
                                                        OKToInstall = false;
                                                    }
                                                } else {
                                                    //
                                                    // Install new on this application
                                                    //
                                                    collection = AddonCollectionModel.addEmpty(core);
                                                    LogController.logInfo(core, "installCollectionFromLocalRep [" + Collectionname + "], GUID [" + CollectionGuid + "], App does not have this collection so it will be installed.");
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
                                                    LogController.logInfo(core, "installCollectionFromLocalRep [" + Collectionname + "], stage-3, prepare to import full collection");
                                                    //-------------------------------------------------------------------------------
                                                    //
                                                    {
                                                        string CollectionHelpLink = "";
                                                        foreach (XmlNode CDefSection in Doc.DocumentElement.ChildNodes) {
                                                            if (CDefSection.Name.ToLowerInvariant() == "helplink") {
                                                                //
                                                                // only save the first
                                                                CollectionHelpLink = CDefSection.InnerText;
                                                                break;
                                                            }
                                                        }
                                                        //
                                                        // ----- set or clear all fields
                                                        collection.name = Collectionname;
                                                        collection.help = "";
                                                        collection.ccguid = CollectionGuid;
                                                        collection.lastChangeDate = CollectionLastChangeDate;
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
                                                        core.db.deleteContentRecords("Add-on Collection CDef Rules", "CollectionID=" + collection.id);
                                                        core.db.deleteContentRecords("Add-on Collection Parent Rules", "ParentID=" + collection.id);
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
                                                            core.db.deleteContentRecords(cnNavigatorEntries, "installedbycollectionid=" + collection.id);
                                                        }
                                                    }
                                                    //
                                                    //-------------------------------------------------------------------------------
                                                    LogController.logInfo(core, "installCollectionFromLocalRep [" + Collectionname + "], stage-4, isolate and process schema-relatednodes (cdef,index,etc)");
                                                    //-------------------------------------------------------------------------------
                                                    //
                                                    bool includeCDefInstall = true;
                                                    if (includeCDefInstall) {
                                                        string CDefMiniCollection = "";
                                                        foreach (XmlNode CDefSection in Doc.DocumentElement.ChildNodes) {
                                                            switch (GenericController.vbLCase(CDefSection.Name)) {
                                                                case "contensivecdef":
                                                                    //
                                                                    // old cdef xection -- take the inner
                                                                    //
                                                                    foreach (XmlNode ChildNode in CDefSection.ChildNodes) {
                                                                        CDefMiniCollection += "\r\n" + ChildNode.OuterXml;
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
                                                                    CDefMiniCollection += CDefSection.OuterXml;
                                                                    break;
                                                            }
                                                        }
                                                        //
                                                        // -- install CdefMiniCollection
                                                        if (!string.IsNullOrEmpty(CDefMiniCollection)) {
                                                            //
                                                            // -- Use the upgrade code to import this part
                                                            CDefMiniCollection = "<" + CollectionFileRootNode + ">" + CDefMiniCollection + "</" + CollectionFileRootNode + ">";
                                                            bool isBaseCollection = (baseCollectionGuid.ToLowerInvariant() == CollectionGuid.ToLowerInvariant());
                                                            CDefMiniCollectionModel.installCDefMiniCollectionFromXml(false, core, CDefMiniCollection, IsNewBuild, repair, isBaseCollection, ref nonCriticalErrorList, logPrefix, ref blockCollectionList);
                                                            //
                                                            // -- Process nodes to save Collection data
                                                            XmlDocument NavDoc = new XmlDocument();
                                                            loadOK = true;
                                                            try {
                                                                NavDoc.LoadXml(CDefMiniCollection);
                                                            } catch (Exception) {
                                                                //
                                                                // error - Need a way to reach the user that submitted the file
                                                                //
                                                                LogController.logInfo(core, "installCollectionFromLocalRep [" + Collectionname + "], creating navigator entries, there was an error parsing the portion of the collection that contains cdef. Navigator entry creation was aborted. [There was an error reading the Meta data file.]");
                                                                result = false;
                                                                return_ErrorMessage = return_ErrorMessage + "<P>The collection was not installed because the xml collection file has an error.</P>";
                                                                loadOK = false;
                                                            }
                                                            if (loadOK) {
                                                                foreach (XmlNode CDefNode in NavDoc.DocumentElement.ChildNodes) {
                                                                    switch (GenericController.vbLCase(CDefNode.Name)) {
                                                                        case "cdef":
                                                                            string ContentName = XmlController.GetXMLAttribute(core, IsFound, CDefNode, "name", "");
                                                                            //
                                                                            // setup cdef rule
                                                                            //
                                                                            int ContentID = CdefController.getContentId(core, ContentName);
                                                                            if (ContentID > 0) {
                                                                                int CS = core.db.csInsertRecord("Add-on Collection CDef Rules", 0);
                                                                                if (core.db.csOk(CS)) {
                                                                                    core.db.csSet(CS, "Contentid", ContentID);
                                                                                    core.db.csSet(CS, "CollectionID", collection.id);
                                                                                }
                                                                                core.db.csClose(ref CS);
                                                                            }
                                                                            break;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    //
                                                    //-------------------------------------------------------------------------------
                                                    LogController.logInfo(core, "installCollectionFromLocalRep [" + Collectionname + "], stage-5, create data records from data nodes, ignore fields");
                                                    //-------------------------------------------------------------------------------
                                                    //
                                                    {
                                                        foreach (XmlNode CDefSection in Doc.DocumentElement.ChildNodes) {
                                                            switch (GenericController.vbLCase(CDefSection.Name)) {
                                                                case "data": {
                                                                        //
                                                                        // import content
                                                                        //   This can only be done with matching guid
                                                                        //
                                                                        foreach (XmlNode ContentNode in CDefSection.ChildNodes) {
                                                                            if (GenericController.vbLCase(ContentNode.Name) == "record") {
                                                                                //
                                                                                // Data.Record node
                                                                                //
                                                                                string ContentName = XmlController.GetXMLAttribute(core, IsFound, ContentNode, "content", "");
                                                                                if (string.IsNullOrEmpty(ContentName)) {
                                                                                    LogController.logInfo(core, "installCollectionFromLocalRep [" + Collectionname + "], install collection file contains a data.record node with a blank content attribute.");
                                                                                    result = false;
                                                                                    return_ErrorMessage = return_ErrorMessage + "<P>Collection file contains a data.record node with a blank content attribute.</P>";
                                                                                } else {
                                                                                    string ContentRecordGuid = XmlController.GetXMLAttribute(core, IsFound, ContentNode, "guid", "");
                                                                                    string ContentRecordName = XmlController.GetXMLAttribute(core, IsFound, ContentNode, "name", "");
                                                                                    if ((string.IsNullOrEmpty(ContentRecordGuid)) && (string.IsNullOrEmpty(ContentRecordName))) {
                                                                                        LogController.logInfo(core, "installCollectionFromLocalRep [" + Collectionname + "], install collection file contains a data record node with neither guid nor name. It must have either a name or a guid attribute. The content is [" + ContentName + "]");
                                                                                        result = false;
                                                                                        return_ErrorMessage = return_ErrorMessage + "<P>The collection was not installed because the Collection file contains a data record node with neither name nor guid. This is not allowed. The content is [" + ContentName + "].</P>";
                                                                                    } else {
                                                                                        //
                                                                                        // create or update the record
                                                                                        //
                                                                                        Models.Domain.CDefModel CDef = Models.Domain.CDefModel.create(core, ContentName);
                                                                                        int cs = -1;
                                                                                        if (!string.IsNullOrEmpty(ContentRecordGuid)) {
                                                                                            cs = core.db.csOpen(ContentName, "ccguid=" + core.db.encodeSQLText(ContentRecordGuid));
                                                                                        } else {
                                                                                            cs = core.db.csOpen(ContentName, "name=" + core.db.encodeSQLText(ContentRecordName));
                                                                                        }
                                                                                        bool recordfound = true;
                                                                                        if (!core.db.csOk(cs)) {
                                                                                            //
                                                                                            // Insert the new record
                                                                                            //
                                                                                            recordfound = false;
                                                                                            core.db.csClose(ref cs);
                                                                                            cs = core.db.csInsertRecord(ContentName, 0);
                                                                                        }
                                                                                        if (core.db.csOk(cs)) {
                                                                                            //
                                                                                            // Update the record
                                                                                            //
                                                                                            if (recordfound && (!string.IsNullOrEmpty(ContentRecordGuid))) {
                                                                                                //
                                                                                                // found by guid, use guid in list and save name
                                                                                                //
                                                                                                core.db.csSet(cs, "name", ContentRecordName);
                                                                                                DataRecordList = DataRecordList + "\r\n" + ContentName + "," + ContentRecordGuid;
                                                                                            } else if (recordfound) {
                                                                                                //
                                                                                                // record found by name, use name is list but do not add guid
                                                                                                //
                                                                                                DataRecordList = DataRecordList + "\r\n" + ContentName + "," + ContentRecordName;
                                                                                            } else {
                                                                                                //
                                                                                                // record was created
                                                                                                //
                                                                                                core.db.csSet(cs, "ccguid", ContentRecordGuid);
                                                                                                core.db.csSet(cs, "name", ContentRecordName);
                                                                                                DataRecordList = DataRecordList + "\r\n" + ContentName + "," + ContentRecordGuid;
                                                                                            }
                                                                                        }
                                                                                        core.db.csClose(ref cs);
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
                                                    LogController.logInfo(core, "installCollectionFromLocalRep [" + Collectionname + "], stage-6, install addon nodes, set importcollection relationships");
                                                    //-------------------------------------------------------------------------------
                                                    //
                                                    foreach (XmlNode CDefSection in Doc.DocumentElement.ChildNodes) {
                                                        switch (GenericController.vbLCase(CDefSection.Name)) {
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
                                                                string ChildCollectionName =XmlController.GetXMLAttribute(core, Found, CDefSection, "name", "");
                                                                string ChildCollectionGUID =XmlController.GetXMLAttribute(core, Found, CDefSection, "guid", CDefSection.InnerText);
                                                                if (string.IsNullOrEmpty(ChildCollectionGUID)) {
                                                                    ChildCollectionGUID = CDefSection.InnerText;
                                                                }
                                                                if (!string.IsNullOrEmpty(ChildCollectionGUID)) {
                                                                    int ChildCollectionID = 0;
                                                                    int cs = -1;
                                                                    cs = core.db.csOpen("Add-on Collections", "ccguid=" + core.db.encodeSQLText(ChildCollectionGUID));
                                                                    if (core.db.csOk(cs)) {
                                                                        ChildCollectionID = core.db.csGetInteger(cs, "id");
                                                                    }
                                                                    core.db.csClose(ref cs);
                                                                    if (ChildCollectionID != 0) {
                                                                        cs = core.db.csInsertRecord("Add-on Collection Parent Rules", 0);
                                                                        if (core.db.csOk(cs)) {
                                                                            core.db.csSet(cs, "ParentID", collection.id);
                                                                            core.db.csSet(cs, "ChildID", ChildCollectionID);
                                                                        }
                                                                        core.db.csClose(ref cs);
                                                                    }
                                                                }
                                                                break;
                                                            case "scriptingmodule":
                                                            case "scriptingmodules":
                                                                result = false;
                                                                return_ErrorMessage = return_ErrorMessage + "<P>Collection includes a scripting module which is no longer supported. Move scripts to the code tab.</P>";
                                                                //    '
                                                                //    ' Scripting modules
                                                                //    '
                                                                //    ScriptingModuleID = 0
                                                                //    ScriptingName =xmlController.GetXMLAttribute(core,IsFound, CDefSection, "name", "No Name")
                                                                //    If ScriptingName = "" Then
                                                                //        ScriptingName = "No Name"
                                                                //    End If
                                                                //    ScriptingGuid =xmlController.GetXMLAttribute(core,IsFound, CDefSection, "guid", AOName)
                                                                //    If ScriptingGuid = "" Then
                                                                //        ScriptingGuid = ScriptingName
                                                                //    End If
                                                                //    Criteria = "(ccguid=" & core.db.encodeSQLText(ScriptingGuid) & ")"
                                                                //    ScriptingModuleID = 0
                                                                //    CS = core.db.cs_open("Scripting Modules", Criteria)
                                                                //    If core.db.cs_ok(CS) Then
                                                                //        '
                                                                //        ' Update the Addon
                                                                //        '
                                                                //        Call logcontroller.appendInstallLog(core, "UpgradeAppFromLocalCollection, GUID match with existing scripting module, Updating module [" & ScriptingName & "], Guid [" & ScriptingGuid & "]")
                                                                //    Else
                                                                //        '
                                                                //        ' not found by GUID - search name against name to update legacy Add-ons
                                                                //        '
                                                                //        Call core.db.cs_Close(CS)
                                                                //        Criteria = "(name=" & core.db.encodeSQLText(ScriptingName) & ")and(ccguid is null)"
                                                                //        CS = core.db.cs_open("Scripting Modules", Criteria)
                                                                //        If core.db.cs_ok(CS) Then
                                                                //            Call logcontroller.appendInstallLog(core, "UpgradeAppFromLocalCollection, Scripting Module matched an existing Module that has no GUID, Updating to [" & ScriptingName & "], Guid [" & ScriptingGuid & "]")
                                                                //        End If
                                                                //    End If
                                                                //    If Not core.db.cs_ok(CS) Then
                                                                //        '
                                                                //        ' not found by GUID or by name, Insert a new
                                                                //        '
                                                                //        Call core.db.cs_Close(CS)
                                                                //        CS = core.db.cs_insertRecord("Scripting Modules", 0)
                                                                //        If core.db.cs_ok(CS) Then
                                                                //            Call logcontroller.appendInstallLog(core, "UpgradeAppFromLocalCollection, Creating new Scripting Module [" & ScriptingName & "], Guid [" & ScriptingGuid & "]")
                                                                //        End If
                                                                //    End If
                                                                //    If Not core.db.cs_ok(CS) Then
                                                                //        '
                                                                //        ' Could not create new
                                                                //        '
                                                                //        Call logcontroller.appendInstallLog(core, "UpgradeAppFromLocalCollection, Scripting Module could not be created, skipping Scripting Module [" & ScriptingName & "], Guid [" & ScriptingGuid & "]")
                                                                //    Else
                                                                //        ScriptingModuleID = core.db.cs_getInteger(CS, "ID")
                                                                //        Call core.db.cs_set(CS, "code", CDefSection.InnerText)
                                                                //        Call core.db.cs_set(CS, "name", ScriptingName)
                                                                //        Call core.db.cs_set(CS, "ccguid", ScriptingGuid)
                                                                //    End If
                                                                //    Call core.db.cs_Close(CS)
                                                                //    If ScriptingModuleID <> 0 Then
                                                                //        '
                                                                //        ' Add Add-on Collection Module Rule
                                                                //        '
                                                                //        CS = core.db.cs_insertRecord("Add-on Collection Module Rules", 0)
                                                                //        If core.db.cs_ok(CS) Then
                                                                //            Call core.db.cs_set(CS, "Collectionid", CollectionID)
                                                                //            Call core.db.cs_set(CS, "ScriptingModuleID", ScriptingModuleID)
                                                                //        End If
                                                                //        Call core.db.cs_Close(CS)
                                                                //    End If
                                                                break;
                                                            case "sharedstyle":
                                                                result = false;
                                                                return_ErrorMessage = return_ErrorMessage + "<P>Collection includes a shared style which is no longer supported. Move styles to the default styles tab.</P>";

                                                                //    '
                                                                //    ' added 9/3/2012
                                                                //    ' Shared Style
                                                                //    '
                                                                //    sharedStyleId = 0
                                                                //    NodeName =xmlController.GetXMLAttribute(core,IsFound, CDefSection, "name", "No Name")
                                                                //    If NodeName = "" Then
                                                                //        NodeName = "No Name"
                                                                //    End If
                                                                //    nodeGuid =xmlController.GetXMLAttribute(core,IsFound, CDefSection, "guid", AOName)
                                                                //    If nodeGuid = "" Then
                                                                //        nodeGuid = NodeName
                                                                //    End If
                                                                //    Criteria = "(ccguid=" & core.db.encodeSQLText(nodeGuid) & ")"
                                                                //    ScriptingModuleID = 0
                                                                //    CS = core.db.cs_open("Shared Styles", Criteria)
                                                                //    If core.db.cs_ok(CS) Then
                                                                //        '
                                                                //        ' Update the Addon
                                                                //        '
                                                                //        Call logcontroller.appendInstallLog(core, "UpgradeAppFromLocalCollection, GUID match with existing shared style, Updating [" & NodeName & "], Guid [" & nodeGuid & "]")
                                                                //    Else
                                                                //        '
                                                                //        ' not found by GUID - search name against name to update legacy Add-ons
                                                                //        '
                                                                //        Call core.db.cs_Close(CS)
                                                                //        Criteria = "(name=" & core.db.encodeSQLText(NodeName) & ")and(ccguid is null)"
                                                                //        CS = core.db.cs_open("shared styles", Criteria)
                                                                //        If core.db.cs_ok(CS) Then
                                                                //            Call logcontroller.appendInstallLog(core, "UpgradeAppFromLocalCollection, shared style matched an existing Module that has no GUID, Updating to [" & NodeName & "], Guid [" & nodeGuid & "]")
                                                                //        End If
                                                                //    End If
                                                                //    If Not core.db.cs_ok(CS) Then
                                                                //        '
                                                                //        ' not found by GUID or by name, Insert a new
                                                                //        '
                                                                //        Call core.db.cs_Close(CS)
                                                                //        CS = core.db.cs_insertRecord("shared styles", 0)
                                                                //        If core.db.cs_ok(CS) Then
                                                                //            Call logcontroller.appendInstallLog(core, "UpgradeAppFromLocalCollection, Creating new shared style [" & NodeName & "], Guid [" & nodeGuid & "]")
                                                                //        End If
                                                                //    End If
                                                                //    If Not core.db.cs_ok(CS) Then
                                                                //        '
                                                                //        ' Could not create new
                                                                //        '
                                                                //        Call logcontroller.appendInstallLog(core, "UpgradeAppFromLocalCollection, shared style could not be created, skipping shared style [" & NodeName & "], Guid [" & nodeGuid & "]")
                                                                //    Else
                                                                //        sharedStyleId = core.db.cs_getInteger(CS, "ID")
                                                                //        Call core.db.cs_set(CS, "StyleFilename", CDefSection.InnerText)
                                                                //        Call core.db.cs_set(CS, "name", NodeName)
                                                                //        Call core.db.cs_set(CS, "ccguid", nodeGuid)
                                                                //        Call core.db.cs_set(CS, "alwaysInclude",xmlController.GetXMLAttribute(core,IsFound, CDefSection, "alwaysinclude", "0"))
                                                                //        Call core.db.cs_set(CS, "prefix",xmlController.GetXMLAttribute(core,IsFound, CDefSection, "prefix", ""))
                                                                //        Call core.db.cs_set(CS, "suffix",xmlController.GetXMLAttribute(core,IsFound, CDefSection, "suffix", ""))
                                                                //        Call core.db.cs_set(CS, "suffix",xmlController.GetXMLAttribute(core,IsFound, CDefSection, "suffix", ""))
                                                                //        Call core.db.cs_set(CS, "sortOrder",xmlController.GetXMLAttribute(core,IsFound, CDefSection, "sortOrder", ""))
                                                                //    End If
                                                                //    Call core.db.cs_Close(CS)
                                                                break;
                                                            case "addon":
                                                            case "add-on":
                                                                //
                                                                // Add-on Node, do part 1 of 2
                                                                //   (include add-on node must be done after all add-ons are installed)
                                                                //
                                                                InstallCollectionFromLocalRepo_addonNode_installAddon(core, CDefSection, "ccguid", core.siteProperties.dataBuildVersion, collection.id, ref result, ref return_ErrorMessage);
                                                                if (!result) {
                                                                    //result = result;
                                                                }
                                                                break;
                                                            case "interfaces":
                                                                //
                                                                // Legacy Interface Node
                                                                //
                                                                foreach (XmlNode CDefInterfaces in CDefSection.ChildNodes) {
                                                                    InstallCollectionFromLocalRepo_addonNode_installAddon(core, CDefInterfaces, "ccguid", core.siteProperties.dataBuildVersion, collection.id, ref result, ref return_ErrorMessage);
                                                                    if (!result) {
                                                                        //result = result;
                                                                    }
                                                                }
                                                                //Case "otherxml", "importcollection", "sqlindex", "style", "styles", "stylesheet", "adminmenu", "menuentry", "navigatorentry"
                                                                //    '
                                                                //    ' otherxml
                                                                //    '
                                                                //    If genericController.vbLCase(CDefSection.OuterXml) <> "<otherxml></otherxml>" Then
                                                                //        OtherXML = OtherXML & vbCrLf & CDefSection.OuterXml
                                                                //    End If
                                                                //    'Case Else
                                                                //    '    '
                                                                //    '    ' Unknown node in collection file
                                                                //    '    '
                                                                //    '    OtherXML = OtherXML & vbCrLf & CDefSection.OuterXml
                                                                //    '    Call logcontroller.appendInstallLog(core, "Addon Collection for [" & Collectionname & "] contained an unknown node [" & CDefSection.Name & "]. This node will be ignored.")
                                                                break;
                                                        }
                                                    }
                                                    //
                                                    //-------------------------------------------------------------------------------
                                                    LogController.logInfo(core, "installCollectionFromLocalRep [" + Collectionname + "], stage-7, set addon dependency relationships");
                                                    //-------------------------------------------------------------------------------
                                                    //
                                                    foreach (XmlNode collectionNode in Doc.DocumentElement.ChildNodes) {
                                                        switch (collectionNode.Name.ToLowerInvariant()) {
                                                            case "addon":
                                                            case "add-on":
                                                                //
                                                                // Add-on Node, do part 1, verify the addon in the table with name and guid
                                                                string addonName =XmlController.GetXMLAttribute(core, IsFound, collectionNode, "name", collectionNode.Name);
                                                                if (addonName.ToLowerInvariant()=="_oninstall") {
                                                                    onInstallAddonGuid =XmlController.GetXMLAttribute(core, IsFound, collectionNode, "guid", collectionNode.Name);
                                                                }
                                                                installCollectionFromLocalRepo_addonNode_setAddonDependencies(core, collectionNode, "ccguid", core.siteProperties.dataBuildVersion, collection.id, ref result, ref return_ErrorMessage);
                                                                break;
                                                            case "interfaces":
                                                                //
                                                                // Legacy Interface Node
                                                                //
                                                                foreach (XmlNode CDefInterfaces in collectionNode.ChildNodes) {
                                                                    installCollectionFromLocalRepo_addonNode_setAddonDependencies(core, CDefInterfaces, "ccguid", core.siteProperties.dataBuildVersion, collection.id, ref result, ref return_ErrorMessage);
                                                                    if (!result) {
                                                                        //result = result;
                                                                    }
                                                                }
                                                                break;
                                                        }
                                                    }
                                                    //
                                                    //-------------------------------------------------------------------------------
                                                    LogController.logInfo(core, "installCollectionFromLocalRep [" + Collectionname + "], stage-8, process data nodes, set record fields");
                                                    //-------------------------------------------------------------------------------
                                                    //
                                                    foreach (XmlNode CDefSection in Doc.DocumentElement.ChildNodes) {
                                                        switch (GenericController.vbLCase(CDefSection.Name)) {
                                                            case "data":
                                                                foreach (XmlNode ContentNode in CDefSection.ChildNodes) {
                                                                    if (ContentNode.Name.ToLowerInvariant() == "record") {
                                                                        string ContentName =XmlController.GetXMLAttribute(core, IsFound, ContentNode, "content", "");
                                                                        if (string.IsNullOrEmpty(ContentName)) {
                                                                            LogController.logInfo(core, "installCollectionFromLocalRep [" + Collectionname + "], install collection file contains a data.record node with a blank content attribute.");
                                                                            result = false;
                                                                            return_ErrorMessage = return_ErrorMessage + "<P>Collection file contains a data.record node with a blank content attribute.</P>";
                                                                        } else {
                                                                            string ContentRecordGuid =XmlController.GetXMLAttribute(core, IsFound, ContentNode, "guid", "");
                                                                            string ContentRecordName =XmlController.GetXMLAttribute(core, IsFound, ContentNode, "name", "");
                                                                            if ((!string.IsNullOrEmpty(ContentRecordGuid)) || (!string.IsNullOrEmpty(ContentRecordName))) {
                                                                                Models.Domain.CDefModel CDef = Models.Domain.CDefModel.create(core, ContentName);
                                                                                int cs = -1;
                                                                                if (!string.IsNullOrEmpty(ContentRecordGuid)) {
                                                                                    cs = core.db.csOpen(ContentName, "ccguid=" + core.db.encodeSQLText(ContentRecordGuid));
                                                                                } else {
                                                                                    cs = core.db.csOpen(ContentName, "name=" + core.db.encodeSQLText(ContentRecordName));
                                                                                }
                                                                                if (core.db.csOk(cs)) {
                                                                                    //
                                                                                    // Update the record
                                                                                    foreach (XmlNode FieldNode in ContentNode.ChildNodes) {
                                                                                        if (FieldNode.Name.ToLowerInvariant() == "field") {
                                                                                            bool IsFieldFound = false;
                                                                                            string FieldName =XmlController.GetXMLAttribute(core, IsFound, FieldNode, "name", "").ToLowerInvariant();
                                                                                            int fieldTypeId = -1;
                                                                                            int FieldLookupContentID = -1;
                                                                                            foreach (var keyValuePair in CDef.fields) {
                                                                                                Models.Domain.CDefFieldModel field = keyValuePair.Value;
                                                                                                if (GenericController.vbLCase(field.nameLc) == FieldName) {
                                                                                                    fieldTypeId = field.fieldTypeId;
                                                                                                    FieldLookupContentID = field.lookupContentID;
                                                                                                    IsFieldFound = true;
                                                                                                    break;
                                                                                                }
                                                                                            }
                                                                                            if (IsFieldFound) {
                                                                                                string FieldValue = FieldNode.InnerText;
                                                                                                switch (fieldTypeId) {
                                                                                                    case _fieldTypeIdAutoIdIncrement:
                                                                                                    case _fieldTypeIdRedirect: {
                                                                                                            //
                                                                                                            // not supported
                                                                                                            break;
                                                                                                        }
                                                                                                    case _fieldTypeIdLookup: {
                                                                                                            //
                                                                                                            // read in text value, if a guid, use it, otherwise assume name
                                                                                                            if (FieldLookupContentID != 0) {
                                                                                                                string FieldLookupContentName = CdefController.getContentNameByID(core, FieldLookupContentID);
                                                                                                                if (!string.IsNullOrEmpty(FieldLookupContentName)) {
                                                                                                                    if ((FieldValue.Left(1) == "{") && (FieldValue.Substring(FieldValue.Length - 1) == "}") && CdefController.isContentFieldSupported(core, FieldLookupContentName, "ccguid")) {
                                                                                                                        //
                                                                                                                        // Lookup by guid
                                                                                                                        int fieldLookupId = GenericController.encodeInteger(core.db.getRecordIDByGuid(FieldLookupContentName, FieldValue));
                                                                                                                        if (fieldLookupId <= 0) {
                                                                                                                            return_ErrorMessage = return_ErrorMessage + "<P>Warning: There was a problem translating field [" + FieldName + "] in record [" + ContentName + "] because the record it refers to was not found in this site.</P>";
                                                                                                                        } else {
                                                                                                                            core.db.csSet(cs, FieldName, fieldLookupId);
                                                                                                                        }
                                                                                                                    } else {
                                                                                                                        //
                                                                                                                        // lookup by name
                                                                                                                        if (!string.IsNullOrEmpty(FieldValue)) {
                                                                                                                            int fieldLookupId = core.db.getRecordID(FieldLookupContentName, FieldValue);
                                                                                                                            if (fieldLookupId <= 0) {
                                                                                                                                return_ErrorMessage = return_ErrorMessage + "<P>Warning: There was a problem translating field [" + FieldName + "] in record [" + ContentName + "] because the record it refers to was not found in this site.</P>";
                                                                                                                            } else {
                                                                                                                                core.db.csSet(cs, FieldName, fieldLookupId);
                                                                                                                            }
                                                                                                                        }
                                                                                                                    }
                                                                                                                }
                                                                                                            } else if (FieldValue.IsNumeric()) {
                                                                                                                //
                                                                                                                // must be lookup list
                                                                                                                core.db.csSet(cs, FieldName, FieldValue);
                                                                                                            }
                                                                                                            break;
                                                                                                        }
                                                                                                    default: {
                                                                                                            core.db.csSet(cs, FieldName, FieldValue);
                                                                                                            break;
                                                                                                        }
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                                core.db.csClose(ref cs);
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                                break;
                                                        }
                                                    }
                                                    //
                                                    // --- end of pass
                                                    //
                                                }
                                                collection.dataRecordList = DataRecordList;
                                                collection.save(core);
                                                //
                                                // -- execute onInstall addon if found
                                                if (!string.IsNullOrEmpty( onInstallAddonGuid )) {
                                                    var addon = Models.Db.AddonModel.create(core, onInstallAddonGuid);
                                                    if ( addon != null) {
                                                        var executeContext = new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                                                            addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextSimple,
                                                            errorContextMessage = "calling onInstall Addon [" + addon.name + "] for collection [" + collection.name + "]"
                                                        };
                                                        core.addon.execute(addon, executeContext);
                                                    }
                                                }
                                                //
                                                LogController.logInfo(core, "installCollectionFromLocalRep [" + Collectionname + "], upgrade complete, flush cache");
                                                //
                                                // -- import complete, flush caches
                                                core.cache.invalidateAll();
                                                result = true; ;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                //
                // Log error and exit with failure. This way any other upgrading will still continue
                LogController.handleError( core,ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        //
        private static void UpdateConfig(CoreController core, string Collectionname, string CollectionGuid, DateTime CollectionUpdatedDate, string CollectionVersionFolderName) {
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
                    Doc.LoadXml(getLocalCollectionStoreListXml(core));
                } catch (Exception) {
                    LogController.logInfo(core, "UpdateConfig, Error loading Collections.xml file.");
                }
                if (loadOK) {
                    if (GenericController.vbLCase(Doc.DocumentElement.Name) != GenericController.vbLCase(CollectionListRootNode)) {
                        LogController.logInfo(core, "UpdateConfig, The Collections.xml file has an invalid root node, [" + Doc.DocumentElement.Name + "] was received and [" + CollectionListRootNode + "] was expected.");
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
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        private static string GetCollectionPath(CoreController core, string CollectionGuid) {
            string result = "";
            try {
                DateTime LastChangeDate = default(DateTime);
                string Collectionname = "";
                getCollectionConfig(core, CollectionGuid, ref result, ref LastChangeDate, ref Collectionname);
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return the collection path, lastChangeDate, and collectionName given the guid
        /// </summary>
        public static void getCollectionConfig(CoreController core, string CollectionGuid, ref string return_CollectionPath, ref DateTime return_LastChagnedate, ref string return_CollectionName) {
            try {
                string LocalGuid = "";
                XmlDocument Doc = new XmlDocument();
                string collectionPath = "";
                DateTime lastChangeDate = default(DateTime);
                string hint = "";
                string localName = null;
                bool loadOK = false;
                //
                return_CollectionPath = "";
                return_LastChagnedate = DateTime.MinValue;
                loadOK = true;
                try {
                    Doc.LoadXml(getLocalCollectionStoreListXml(core));
                } catch (Exception) {
                    LogController.logInfo(core, "GetCollectionConfig, Hint=[" + hint + "], Error loading Collections.xml file.");
                    loadOK = false;
                }
                if (loadOK) {
                    if (GenericController.vbLCase(Doc.DocumentElement.Name) != GenericController.vbLCase(CollectionListRootNode)) {
                        LogController.logInfo(core, "Hint=[" + hint + "], The Collections.xml file has an invalid root node");
                    } else {
                        foreach (XmlNode LocalListNode in Doc.DocumentElement.ChildNodes) {
                            localName = "no name found";
                            switch (GenericController.vbLCase(LocalListNode.Name)) {
                                case "collection":
                                    LocalGuid = "";
                                    foreach (XmlNode CollectionNode in LocalListNode.ChildNodes) {
                                        switch (GenericController.vbLCase(CollectionNode.Name)) {
                                            case "name":
                                                //
                                                // no - cannot change the case if files are already saved
                                                localName = CollectionNode.InnerText;
                                                //LocalName = genericController.vbLCase(CollectionNode.InnerText);
                                                break;
                                            case "guid":
                                                //
                                                // no - cannot change the case if files are already saved
                                                LocalGuid = CollectionNode.InnerText;
                                                //LocalGuid = genericController.vbLCase(CollectionNode.InnerText);
                                                break;
                                            case "path":
                                                //
                                                // no - cannot change the case if files are already saved
                                                collectionPath = CollectionNode.InnerText;
                                                //CollectionPath = genericController.vbLCase(CollectionNode.InnerText);
                                                break;
                                            case "lastchangedate":
                                                lastChangeDate = GenericController.encodeDate(CollectionNode.InnerText);
                                                break;
                                        }
                                    }
                                    break;
                            }
                            if (CollectionGuid.ToLowerInvariant() == LocalGuid.ToLowerInvariant()) {
                                return_CollectionPath = collectionPath;
                                return_LastChagnedate = lastChangeDate;
                                return_CollectionName = localName;
                                break;
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //======================================================================================================
        /// <summary>
        /// Installs Addons in a source folder
        /// </summary>
        public static bool installCollectionsFromPrivateFolder(CoreController core, string installPrivatePath, ref string return_ErrorMessage, ref List<string> return_CollectionGUIDList, bool IsNewBuild, bool repair, ref List<string> nonCriticalErrorList, string logPrefix, ref List<string> blockCollectionList) {
            bool returnSuccess = false;
            try {
                DateTime CollectionLastChangeDate;
                //
                CollectionLastChangeDate = DateTime.Now;
                returnSuccess = buildLocalCollectionReposFromFolder(core, installPrivatePath, CollectionLastChangeDate, ref return_CollectionGUIDList, ref return_ErrorMessage, false);
                if (!returnSuccess) {
                    //
                    // BuildLocal failed, log it and do not upgrade
                    //
                    LogController.logInfo(core, "BuildLocalCollectionFolder returned false with Error Message [" + return_ErrorMessage + "], exiting without calling UpgradeAllAppsFromLocalCollection");
                } else {
                    foreach (string collectionGuid in return_CollectionGUIDList) {
                        if (!installCollectionFromLocalRepo(core, collectionGuid, core.siteProperties.dataBuildVersion, ref return_ErrorMessage, "", IsNewBuild, repair, ref nonCriticalErrorList, logPrefix, ref blockCollectionList, true)) {
                            LogController.logInfo(core, "UpgradeAllAppsFromLocalCollection returned false with Error Message [" + return_ErrorMessage + "].");
                            break;
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                returnSuccess = false;
                if (string.IsNullOrEmpty(return_ErrorMessage)) {
                    return_ErrorMessage = "There was an unexpected error installing the collection, details [" + ex.Message + "]";
                }
            }
            return returnSuccess;
        }
        //
        //======================================================================================================
        /// <summary>
        /// Installs Addons in a source file
        /// </summary>
        public static bool installCollectionsFromPrivateFile(CoreController core, string pathFilename, ref string return_ErrorMessage, ref string return_CollectionGUID, bool IsNewBuild, bool repair, ref List<string> nonCriticalErrorList, string logPrefix, ref List<string> blockCollectionList) {
            bool returnSuccess = false;
            try {
                DateTime CollectionLastChangeDate;
                //
                CollectionLastChangeDate = DateTime.Now;
                returnSuccess = buildLocalCollectionRepoFromFile(core, pathFilename, CollectionLastChangeDate, ref return_CollectionGUID, ref return_ErrorMessage, false);
                if (!returnSuccess) {
                    //
                    // BuildLocal failed, log it and do not upgrade
                    //
                    LogController.logInfo(core, "BuildLocalCollectionFolder returned false with Error Message [" + return_ErrorMessage + "], exiting without calling UpgradeAllAppsFromLocalCollection");
                } else {
                    returnSuccess = installCollectionFromLocalRepo(core, return_CollectionGUID, core.siteProperties.dataBuildVersion, ref return_ErrorMessage, "", IsNewBuild, repair, ref nonCriticalErrorList, logPrefix, ref blockCollectionList, true);
                    if (!returnSuccess) {
                        //
                        // Upgrade all apps failed
                        //
                        LogController.logInfo(core, "UpgradeAllAppsFromLocalCollection returned false with Error Message [" + return_ErrorMessage + "].");
                    } else {
                        returnSuccess = true;
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                returnSuccess = false;
                if (string.IsNullOrEmpty(return_ErrorMessage)) {
                    return_ErrorMessage = "There was an unexpected error installing the collection, details [" + ex.Message + "]";
                }
            }
            return returnSuccess;
        }
        //
        //======================================================================================================
        //
        private static int getNavIDByGuid(CoreController core, string ccGuid) {
            int navId = 0;
            try {
                int CS;
                //
                CS = core.db.csOpen(cnNavigatorEntries, "ccguid=" + core.db.encodeSQLText(ccGuid), "ID",true,0,false,false, "ID");
                if (core.db.csOk(CS)) {
                    navId = core.db.csGetInteger(CS, "id");
                }
                core.db.csClose(ref CS);
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return navId;
        }
        //
        //======================================================================================================
        /// <summary>
        /// copy resources from install folder to www folder
        /// </summary>
        private static void copyInstallPathToDstPath(CoreController core, string SrcPath, string DstPath, string BlockFileList, string BlockFolderList) {
            try {
                
                string SrcFolder = null;
                string DstFolder = null;
                //
                SrcFolder = SrcPath;
                if (SrcFolder.Substring(SrcFolder.Length - 1) == "\\") {
                    SrcFolder = SrcFolder.Left( SrcFolder.Length - 1);
                }
                //
                DstFolder = DstPath;
                if (DstFolder.Substring(DstFolder.Length - 1) == "\\") {
                    DstFolder = DstFolder.Left( DstFolder.Length - 1);
                }
                //
                if (core.privateFiles.pathExists(SrcFolder)) {
                    List< FileDetail> FileInfoArray = core.privateFiles.getFileList(SrcFolder);
                    foreach (FileDetail file in FileInfoArray) {
                        if ((file.Extension == "dll") || (file.Extension == "exe") || (file.Extension == "zip")) {
                            //
                            // can not copy dll or exe
                            //
                            //Filename = Filename
                        } else if (("," + BlockFileList + ",").IndexOf("," + file.Name + ",", System.StringComparison.OrdinalIgnoreCase)  != -1) {
                            //
                            // can not copy the current collection file
                            //
                            //file.Name = file.Name
                        } else {
                            //
                            // copy this file to destination
                            //
                            core.privateFiles.copyFile(SrcPath + file.Name, DstPath + file.Name, core.appRootFiles);
                        }
                    }
                    //
                    // copy folders to dst
                    //
                    List<FolderDetail> FolderInfoArray = core.privateFiles.getFolderList(SrcFolder);
                    foreach (FolderDetail folder in FolderInfoArray) {
                        if (("," + BlockFolderList + ",").IndexOf("," + folder.Name + ",", System.StringComparison.OrdinalIgnoreCase)  == -1) {
                            copyInstallPathToDstPath(core, SrcPath + folder.Name + "\\", DstPath + folder.Name + "\\", BlockFileList, "");
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //======================================================================================================
        //
        private static string GetCollectionFileList(CoreController core, string SrcPath, string SubFolder, string ExcludeFileList) {
            string result = "";
            try {
                string SrcFolder;
                //
                SrcFolder = SrcPath + SubFolder;
                if (SrcFolder.Substring(SrcFolder.Length - 1) == "\\") {
                    SrcFolder = SrcFolder.Left( SrcFolder.Length - 1);
                }
                //
                if (core.privateFiles.pathExists(SrcFolder)) {
                    List<FileDetail> FileInfoArray = core.privateFiles.getFileList(SrcFolder);
                    foreach (FileDetail file in FileInfoArray) {
                        if (("," + ExcludeFileList + ",").IndexOf("," + file.Name + ",", System.StringComparison.OrdinalIgnoreCase)  != -1) {
                            //
                            // can not copy the current collection file
                            //
                            //Filename = Filename
                        } else {
                            //
                            // copy this file to destination
                            //
                            result += "\r\n" + SubFolder + file.Name;
                            //runAtServer.IPAddress = "127.0.0.1"
                            //runAtServer.Port = "4531"
                            //QS = "SrcFile=" & encodeRequestVariable(SrcPath & Filename) & "&DstFile=" & encodeRequestVariable(DstPath & Filename)
                            //Call runAtServer.ExecuteCmd("CopyFile", QS)
                            //Call core.app.privateFiles.CopyFile(SrcPath & Filename, DstPath & Filename)
                        }
                    }
                    //
                    // copy folders to dst
                    //
                    List<FolderDetail> FolderInfoArray = core.privateFiles.getFolderList(SrcFolder);
                    foreach (FolderDetail folder in FolderInfoArray) {
                        result += GetCollectionFileList(core, SrcPath, SubFolder + folder.Name + "\\", ExcludeFileList);
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return result;
        }
        //
        //======================================================================================================
        //
        private static void InstallCollectionFromLocalRepo_addonNode_installAddon(CoreController core, XmlNode AddonNode, string AddonGuidFieldName, string ignore_BuildVersion, int CollectionID, ref bool return_UpgradeOK, ref string return_ErrorMessage) {
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
                    string Criteria = "(" + AddonGuidFieldName + "=" + core.db.encodeSQLText(addonGuid) + ")";
                    int CS = core.db.csOpen(cnAddons, Criteria, "", false);
                    if (core.db.csOk(CS)) {
                        //
                        // Update the Addon
                        //
                        LogController.logInfo(core, "UpgradeAppFromLocalCollection, GUID match with existing Add-on, Updating Add-on [" + addonName + "], Guid [" + addonGuid + "]");
                    } else {
                        //
                        // not found by GUID - search name against name to update legacy Add-ons
                        //
                        core.db.csClose(ref CS);
                        Criteria = "(name=" + core.db.encodeSQLText(addonName) + ")and(" + AddonGuidFieldName + " is null)";
                        CS = core.db.csOpen(cnAddons, Criteria,"", false);
                        if (core.db.csOk(CS)) {
                            LogController.logInfo(core, "UpgradeAppFromLocalCollection, Add-on name matched an existing Add-on that has no GUID, Updating legacy Aggregate Function to Add-on [" + addonName + "], Guid [" + addonGuid + "]");
                        }
                    }
                    if (!core.db.csOk(CS)) {
                        //
                        // not found by GUID or by name, Insert a new addon
                        //
                        core.db.csClose(ref CS);
                        CS = core.db.csInsertRecord(cnAddons, 0);
                        if (core.db.csOk(CS)) {
                            LogController.logInfo(core, "UpgradeAppFromLocalCollection, Creating new Add-on [" + addonName + "], Guid [" + addonGuid + "]");
                        }
                    }
                    if (!core.db.csOk(CS)) {
                        //
                        // Could not create new Add-on
                        //
                        LogController.logInfo(core, "UpgradeAppFromLocalCollection, Add-on could not be created, skipping Add-on [" + addonName + "], Guid [" + addonGuid + "]");
                    } else {
                        int addonId = core.db.csGetInteger(CS, "ID");
                        //
                        // Initialize the add-on
                        // Delete any existing related records - so if this is an update with removed relationships, those are removed
                        //
                        //Call core.db.deleteContentRecords("Shared Styles Add-on Rules", "addonid=" & addonId)
                        //Call core.db.deleteContentRecords("Add-on Scripting Module Rules", "addonid=" & addonId)
                        core.db.deleteContentRecords("Add-on Include Rules", "addonid=" + addonId);
                        core.db.deleteContentRecords("Add-on Content Trigger Rules", "addonid=" + addonId);
                        //
                        core.db.csSet(CS, "collectionid", CollectionID);
                        core.db.csSet(CS, AddonGuidFieldName, addonGuid);
                        core.db.csSet(CS, "name", addonName);
                        core.db.csSet(CS, "navTypeId", navTypeId);
                        string ArgumentList = "";
                        string StyleSheet = "";
                        if (AddonNode.ChildNodes.Count > 0) {
                            foreach (XmlNode PageInterfaceWithinLoop in AddonNode.ChildNodes) {
                                XmlNode PageInterface = PageInterfaceWithinLoop;
                                string test = null;
                                int scriptinglanguageid = 0;
                                string ScriptingCode = null;
                                string FieldName = null;
                                string NodeName = null;
                                string NewValue = null;
                                string menuNameSpace = null;
                                string FieldValue = "";
                                int CS2 = 0;
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
                                            int fieldTypeID = 0;
                                            string fieldType = null;
                                            switch (GenericController.vbLCase(TriggerNode.Name)) {
                                                case "type":
                                                    fieldType = TriggerNode.InnerText;
                                                    fieldTypeID = core.db.getRecordID("Content Field Types", fieldType);
                                                    if (fieldTypeID > 0) {
                                                        Criteria = "(addonid=" + addonId + ")and(contentfieldTypeID=" + fieldTypeID + ")";
                                                        CS2 = core.db.csOpen("Add-on Content Field Type Rules", Criteria);
                                                        if (!core.db.csOk(CS2)) {
                                                            core.db.csClose(ref CS2);
                                                            CS2 = core.db.csInsertRecord("Add-on Content Field Type Rules", 0);
                                                        }
                                                        if (core.db.csOk(CS2)) {
                                                            core.db.csSet(CS2, "addonid", addonId);
                                                            core.db.csSet(CS2, "contentfieldTypeID", fieldTypeID);
                                                        }
                                                        core.db.csClose(ref CS2);
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
                                            int TriggerContentID = 0;
                                            string ContentNameorGuid = null;
                                            switch (GenericController.vbLCase(TriggerNode.Name)) {
                                                case "contentchange":
                                                    TriggerContentID = 0;
                                                    ContentNameorGuid = TriggerNode.InnerText;
                                                    if (string.IsNullOrEmpty(ContentNameorGuid)) {
                                                        ContentNameorGuid = XmlController.GetXMLAttribute(core, IsFound, TriggerNode, "guid", "");
                                                        if (string.IsNullOrEmpty(ContentNameorGuid)) {
                                                            ContentNameorGuid = XmlController.GetXMLAttribute(core, IsFound, TriggerNode, "name", "");
                                                        }
                                                    }
                                                    Criteria = "(ccguid=" + core.db.encodeSQLText(ContentNameorGuid) + ")";
                                                    CS2 = core.db.csOpen("Content", Criteria);
                                                    if (!core.db.csOk(CS2)) {
                                                        core.db.csClose(ref CS2);
                                                        Criteria = "(ccguid is null)and(name=" + core.db.encodeSQLText(ContentNameorGuid) + ")";
                                                        CS2 = core.db.csOpen("content", Criteria);
                                                    }
                                                    if (core.db.csOk(CS2)) {
                                                        TriggerContentID = core.db.csGetInteger(CS2, "ID");
                                                    }
                                                    core.db.csClose(ref CS2);
                                                    //If TriggerContentID = 0 Then
                                                    //    CS2 = core.db.cs_insertRecord("Scripting Modules", 0)
                                                    //    If core.db.cs_ok(CS2) Then
                                                    //        Call core.db.cs_set(CS2, "name", ScriptingNameorGuid)
                                                    //        Call core.db.cs_set(CS2, "ccguid", ScriptingNameorGuid)
                                                    //        TriggerContentID = core.db.cs_getInteger(CS2, "ID")
                                                    //    End If
                                                    //    Call core.db.cs_Close(CS2)
                                                    //End If
                                                    if (TriggerContentID == 0) {
                                                        //
                                                        // could not find the content
                                                        //
                                                    } else {
                                                        Criteria = "(addonid=" + addonId + ")and(contentid=" + TriggerContentID + ")";
                                                        CS2 = core.db.csOpen("Add-on Content Trigger Rules", Criteria);
                                                        if (!core.db.csOk(CS2)) {
                                                            core.db.csClose(ref CS2);
                                                            CS2 = core.db.csInsertRecord("Add-on Content Trigger Rules", 0);
                                                            if (core.db.csOk(CS2)) {
                                                                core.db.csSet(CS2, "addonid", addonId);
                                                                core.db.csSet(CS2, "contentid", TriggerContentID);
                                                            }
                                                        }
                                                        core.db.csClose(ref CS2);
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
                                        //
                                        // -- todo - need to look this up correctly
                                        if ( ScriptingLanguage.ToLowerInvariant()=="jscript") {
                                            scriptinglanguageid = (int)AddonController.ScriptLanguages.Javascript;
                                        } else {
                                            scriptinglanguageid = (int)AddonController.ScriptLanguages.VBScript;
                                        }
                                        //scriptinglanguageid = core.db.getRecordID("scripting languages", ScriptingLanguage);
                                        core.db.csSet(CS, "scriptinglanguageid", scriptinglanguageid);
                                        ScriptingEntryPoint = XmlController.GetXMLAttribute(core, IsFound, PageInterfaceWithinLoop, "entrypoint", "");
                                        core.db.csSet(CS, "ScriptingEntryPoint", ScriptingEntryPoint);
                                        ScriptingTimeout = GenericController.encodeInteger(XmlController.GetXMLAttribute(core, IsFound, PageInterfaceWithinLoop, "timeout", "5000"));
                                        core.db.csSet(CS, "ScriptingTimeout", ScriptingTimeout);
                                        ScriptingCode = "";
                                        //Call core.app.csv_SetCS(CS, "ScriptingCode", ScriptingCode)
                                        foreach (XmlNode ScriptingNode in PageInterfaceWithinLoop.ChildNodes) {
                                            switch (GenericController.vbLCase(ScriptingNode.Name)) {
                                                case "code":
                                                    ScriptingCode = ScriptingCode + ScriptingNode.InnerText;
                                                    //Case "includemodule"

                                                    //    ScriptingModuleID = 0
                                                    //    ScriptingNameorGuid = ScriptingNode.InnerText
                                                    //    If ScriptingNameorGuid = "" Then
                                                    //        ScriptingNameorGuid =xmlController.GetXMLAttribute(core,IsFound, ScriptingNode, "guid", "")
                                                    //        If ScriptingNameorGuid = "" Then
                                                    //            ScriptingNameorGuid =xmlController.GetXMLAttribute(core,IsFound, ScriptingNode, "name", "")
                                                    //        End If
                                                    //    End If
                                                    //    Criteria = "(ccguid=" & core.db.encodeSQLText(ScriptingNameorGuid) & ")"
                                                    //    CS2 = core.db.cs_open("Scripting Modules", Criteria)
                                                    //    If Not core.db.cs_ok(CS2) Then
                                                    //        Call core.db.cs_Close(CS2)
                                                    //        Criteria = "(ccguid is null)and(name=" & core.db.encodeSQLText(ScriptingNameorGuid) & ")"
                                                    //        CS2 = core.db.cs_open("Scripting Modules", Criteria)
                                                    //    End If
                                                    //    If core.db.cs_ok(CS2) Then
                                                    //        ScriptingModuleID = core.db.cs_getInteger(CS2, "ID")
                                                    //    End If
                                                    //    Call core.db.cs_Close(CS2)
                                                    //    If ScriptingModuleID = 0 Then
                                                    //        CS2 = core.db.cs_insertRecord("Scripting Modules", 0)
                                                    //        If core.db.cs_ok(CS2) Then
                                                    //            Call core.db.cs_set(CS2, "name", ScriptingNameorGuid)
                                                    //            Call core.db.cs_set(CS2, "ccguid", ScriptingNameorGuid)
                                                    //            ScriptingModuleID = core.db.cs_getInteger(CS2, "ID")
                                                    //        End If
                                                    //        Call core.db.cs_Close(CS2)
                                                    //    End If
                                                    //    Criteria = "(addonid=" & addonId & ")and(scriptingmoduleid=" & ScriptingModuleID & ")"
                                                    //    CS2 = core.db.cs_open("Add-on Scripting Module Rules", Criteria)
                                                    //    If Not core.db.cs_ok(CS2) Then
                                                    //        Call core.db.cs_Close(CS2)
                                                    //        CS2 = core.db.cs_insertRecord("Add-on Scripting Module Rules", 0)
                                                    //        If core.db.cs_ok(CS2) Then
                                                    //            Call core.db.cs_set(CS2, "addonid", addonId)
                                                    //            Call core.db.cs_set(CS2, "scriptingmoduleid", ScriptingModuleID)
                                                    //        End If
                                                    //    End If
                                                    //    Call core.db.cs_Close(CS2)
                                                    break;
                                            }
                                        }
                                        core.db.csSet(CS, "ScriptingCode", ScriptingCode);
                                        break;
                                    case "activexprogramid":
                                        //
                                        // save program id
                                        //
                                        FieldValue = PageInterfaceWithinLoop.InnerText;
                                        core.db.csSet(CS, "ObjectProgramID", FieldValue);
                                        break;
                                    case "navigator":
                                        //
                                        // create a navigator entry with a parent set to this
                                        //
                                        core.db.csSave(CS);
                                        menuNameSpace = XmlController.GetXMLAttribute(core, IsFound, PageInterfaceWithinLoop, "NameSpace", "");
                                        if (!string.IsNullOrEmpty(menuNameSpace)) {
                                            string NavIconTypeString = XmlController.GetXMLAttribute(core, IsFound, PageInterfaceWithinLoop, "type", "");
                                            if (string.IsNullOrEmpty(NavIconTypeString)) {
                                                NavIconTypeString = "Addon";
                                            }
                                            //Dim builder As New coreBuilderClass(core)
                                            AppBuilderController.verifyNavigatorEntry(core, "", menuNameSpace, addonName, "", "", "", false, false, false, true, addonName, NavIconTypeString, addonName, CollectionID);
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
                                                ArgumentList = ArgumentList + "\r\n" + NewValue;
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
                                        StyleSheet = StyleSheet + "\r\n" + NodeName + " " + NewValue;
                                        //Case "includesharedstyle"
                                        //    '
                                        //    ' added 9/3/2012
                                        //    '
                                        //    sharedStyleId = 0
                                        //    nodeNameOrGuid =xmlController.GetXMLAttribute(core,IsFound, PageInterface, "guid", "")
                                        //    If nodeNameOrGuid = "" Then
                                        //        nodeNameOrGuid =xmlController.GetXMLAttribute(core,IsFound, PageInterface, "name", "")
                                        //    End If
                                        //    Criteria = "(ccguid=" & core.db.encodeSQLText(nodeNameOrGuid) & ")"
                                        //    CS2 = core.db.cs_open("shared styles", Criteria)
                                        //    If Not core.db.cs_ok(CS2) Then
                                        //        Call core.db.cs_Close(CS2)
                                        //        Criteria = "(ccguid is null)and(name=" & core.db.encodeSQLText(nodeNameOrGuid) & ")"
                                        //        CS2 = core.db.cs_open("shared styles", Criteria)
                                        //    End If
                                        //    If core.db.cs_ok(CS2) Then
                                        //        sharedStyleId = core.db.cs_getInteger(CS2, "ID")
                                        //    End If
                                        //    Call core.db.cs_Close(CS2)
                                        //    If sharedStyleId = 0 Then
                                        //        CS2 = core.db.cs_insertRecord("shared styles", 0)
                                        //        If core.db.cs_ok(CS2) Then
                                        //            Call core.db.cs_set(CS2, "name", nodeNameOrGuid)
                                        //            Call core.db.cs_set(CS2, "ccguid", nodeNameOrGuid)
                                        //            sharedStyleId = core.db.cs_getInteger(CS2, "ID")
                                        //        End If
                                        //        Call core.db.cs_Close(CS2)
                                        //    End If
                                        //    Criteria = "(addonid=" & addonId & ")and(StyleId=" & sharedStyleId & ")"
                                        //    CS2 = core.db.cs_open("Shared Styles Add-on Rules", Criteria)
                                        //    If Not core.db.cs_ok(CS2) Then
                                        //        Call core.db.cs_Close(CS2)
                                        //        CS2 = core.db.cs_insertRecord("Shared Styles Add-on Rules", 0)
                                        //        If core.db.cs_ok(CS2) Then
                                        //            Call core.db.cs_set(CS2, "addonid", addonId)
                                        //            Call core.db.cs_set(CS2, "StyleId", sharedStyleId)
                                        //        End If
                                        //    End If
                                        //    Call core.db.cs_Close(CS2)
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
                                            StyleSheet = StyleSheet + "\r\n" + PageInterfaceWithinLoop.InnerText;
                                        }
                                        break;
                                    case "template":
                                    case "content":
                                    case "admin":
                                        //
                                        // these add-ons will be "non-developer only" in navigation
                                        //
                                        FieldName = PageInterfaceWithinLoop.Name;
                                        FieldValue = PageInterfaceWithinLoop.InnerText;
                                        if (!core.db.csIsFieldSupported(CS, FieldName)) {
                                            //
                                            // Bad field name - need to report it somehow
                                            //
                                        } else {
                                            core.db.csSet(CS, FieldName, FieldValue);
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
                                            core.db.csSet(CS, "IconFilename", FieldValue);
                                            if (true) {
                                                core.db.csSet(CS, "IconWidth", GenericController.encodeInteger(XmlController.GetXMLAttribute(core, IsFound, PageInterfaceWithinLoop, "width", "0")));
                                                core.db.csSet(CS, "IconHeight", GenericController.encodeInteger(XmlController.GetXMLAttribute(core, IsFound, PageInterfaceWithinLoop, "height", "0")));
                                                core.db.csSet(CS, "IconSprites", GenericController.encodeInteger(XmlController.GetXMLAttribute(core, IsFound, PageInterfaceWithinLoop, "sprites", "0")));
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
                                        if (true) {
                                            core.db.csSet(CS, "formxml", PageInterfaceWithinLoop.InnerXml);
                                        }
                                        break;
                                    case "javascript":
                                    case "javascriptinhead":
                                        //
                                        // these all translate to JSFilename
                                        //
                                        FieldName = "jsfilename";
                                        core.db.csSet(CS, FieldName, PageInterfaceWithinLoop.InnerText);

                                        break;
                                    case "iniframe":
                                        //
                                        // typo - field is inframe
                                        //
                                        FieldName = "inframe";
                                        core.db.csSet(CS, FieldName, PageInterfaceWithinLoop.InnerText);
                                        break;
                                    default:
                                        //
                                        // All the other fields should match the Db fields
                                        //
                                        FieldName = PageInterfaceWithinLoop.Name;
                                        FieldValue = PageInterfaceWithinLoop.InnerText;
                                        if (!core.db.csIsFieldSupported(CS, FieldName)) {
                                            //
                                            // Bad field name - need to report it somehow
                                            //
                                            LogController.handleError(core, new ApplicationException("bad field found [" + FieldName + "], in addon node [" + addonName + "], of collection [" + core.db.getRecordName("add-on collections", CollectionID) + "]"));
                                        } else {
                                            core.db.csSet(CS, FieldName, FieldValue);
                                        }
                                        break;
                                }
                            }
                        }
                        core.db.csSet(CS, "ArgumentList", ArgumentList);
                        core.db.csSet(CS, "StylesFilename", StyleSheet);
                        // these are dynamic now
                        //            '
                        //            ' Setup special setting/tool/report Navigator Entry
                        //            '
                        //            If navTypeId = NavTypeIDTool Then
                        //                AddonNavID = GetNonRootNavigatorID(asv, AOName, GetNavIDByGuid(asv, "{801F1F07-20E6-4A5D-AF26-71007CCB834F}"), addonid, 0, NavIconTypeTool, AOName & " Add-on", NavDeveloperOnly, 0, "", 0, 0, CollectionID, navAdminOnly)
                        //            End If
                        //            If navTypeId = NavTypeIDReport Then
                        //                AddonNavID = GetNonRootNavigatorID(asv, AOName, GetNavIDByGuid(asv, "{2ED078A2-6417-46CB-8572-A13F64C4BF18}"), addonid, 0, NavIconTypeReport, AOName & " Add-on", NavDeveloperOnly, 0, "", 0, 0, CollectionID, navAdminOnly)
                        //            End If
                        //            If navTypeId = NavTypeIDSetting Then
                        //                AddonNavID = GetNonRootNavigatorID(asv, AOName, GetNavIDByGuid(asv, "{5FDDC758-4A15-4F98-8333-9CE8B8BFABC4}"), addonid, 0, NavIconTypeSetting, AOName & " Add-on", NavDeveloperOnly, 0, "", 0, 0, CollectionID, navAdminOnly)
                        //            End If
                    }
                    core.db.csClose(ref CS);
                    //
                    // -- if this is needed, the installation xml files are available in the addon install folder. - I do not believe this is important
                    //       as if a collection is missing a dependancy, there is an error and you would expect to have to reinstall.
                    //
                    // Addon is now fully installed
                    // Go through all collection files on this site and see if there are
                    // any Dependencies on this add-on that need to be attached
                    // src args are those for the addon that includes the current addon
                    //   - if this addon is the target of another add-on's  "includeaddon" node
                    //
                    //Doc = New XmlDocument
                    //CS = core.db.cs_open("Add-on Collections")
                    //Do While core.db.cs_ok(CS)
                    //    CollectionFile = core.db.cs_get(CS, "InstallFile")
                    //    If CollectionFile <> "" Then
                    //        Try
                    //            Call Doc.LoadXml(CollectionFile)
                    //            If Doc.DocumentElement.HasChildNodes Then
                    //                For Each TestObject In Doc.DocumentElement.ChildNodes
                    //                    '
                    //                    ' 20161002 - maybe this should be testing for an xmlElemetn, not node
                    //                    '
                    //                    If (TypeOf (TestObject) Is XmlElement) Then
                    //                        SrcMainNode = DirectCast(TestObject, XmlElement)
                    //                        If genericController.vbLCase(SrcMainNode.Name) = "addon" Then
                    //                            SrcAddonGuid = SrcMainNode.GetAttribute("guid")
                    //                            SrcAddonName = SrcMainNode.GetAttribute("name")
                    //                            If SrcMainNode.HasChildNodes Then
                    //                                '//On Error //Resume Next
                    //                                For Each TestObject2 In SrcMainNode.ChildNodes
                    //                                    'For Each SrcAddonNode In SrcMainNode.childNodes
                    //                                    If TypeOf TestObject2 Is XmlNode Then
                    //                                        SrcAddonNode = DirectCast(TestObject2, XmlElement)
                    //                                        If True Then
                    //                                            'If Err.Number <> 0 Then
                    //                                            '    ' this is to catch nodes that are not elements
                    //                                            '    Err.Clear
                    //                                            'Else
                    //                                            'On Error GoTo ErrorTrap
                    //                                            If genericController.vbLCase(SrcAddonNode.Name) = "includeaddon" Then
                    //                                                TestGuid = SrcAddonNode.GetAttribute("guid")
                    //                                                TestName = SrcAddonNode.GetAttribute("name")
                    //                                                Criteria = ""
                    //                                                If TestGuid <> "" Then
                    //                                                    If TestGuid = addonGuid Then
                    //                                                        Criteria = "(" & AddonGuidFieldName & "=" & core.db.encodeSQLText(SrcAddonGuid) & ")"
                    //                                                    End If
                    //                                                ElseIf TestName <> "" Then
                    //                                                    If TestName = addonName Then
                    //                                                        Criteria = "(name=" & core.db.encodeSQLText(SrcAddonName) & ")"
                    //                                                    End If
                    //                                                End If
                    //                                                If Criteria <> "" Then
                    //                                                    '$$$$$ cache this
                    //                                                    CS2 = core.db.cs_open(cnAddons, Criteria, "ID")
                    //                                                    If core.db.cs_ok(CS2) Then
                    //                                                        SrcAddonID = core.db.cs_getInteger(CS2, "ID")
                    //                                                    End If
                    //                                                    Call core.db.cs_Close(CS2)
                    //                                                    AddRule = False
                    //                                                    If SrcAddonID = 0 Then
                    //                                                        UserError = "The add-on being installed is referenced by another add-on in collection [], but this add-on could not be found by the respoective criteria [" & Criteria & "]"
                    //                                                        Call logcontroller.appendInstallLog(core,  "UpgradeAddFromLocalCollection_InstallAddonNode, UserError [" & UserError & "]")
                    //                                                    Else
                    //                                                        CS2 = core.db.cs_openCsSql_rev("default", "select ID from ccAddonIncludeRules where Addonid=" & SrcAddonID & " and IncludedAddonID=" & addonId)
                    //                                                        AddRule = Not core.db.cs_ok(CS2)
                    //                                                        Call core.db.cs_Close(CS2)
                    //                                                    End If
                    //                                                    If AddRule Then
                    //                                                        CS2 = core.db.cs_insertRecord("Add-on Include Rules", 0)
                    //                                                        If core.db.cs_ok(CS2) Then
                    //                                                            Call core.db.cs_set(CS2, "Addonid", SrcAddonID)
                    //                                                            Call core.db.cs_set(CS2, "IncludedAddonID", addonId)
                    //                                                        End If
                    //                                                        Call core.db.cs_Close(CS2)
                    //                                                    End If
                    //                                                End If
                    //                                            End If
                    //                                        End If
                    //                                    End If
                    //                                Next
                    //                            End If
                    //                        End If
                    //                    Else
                    //                        CS = CS
                    //                    End If
                    //                Next
                    //            End If
                    //        Catch ex As Exception
                    //            core.handleExceptionAndContinue(ex) : Throw
                    //        End Try
                    //    End If
                    //    Call core.db.cs_goNext(CS)
                    //Loop
                    //Call core.db.cs_Close(CS)
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
        private static string installCollectionFromLocalRepo_addonNode_setAddonDependencies(CoreController core, XmlNode AddonNode, string AddonGuidFieldName, string ignore_BuildVersion, int CollectionID, ref bool ReturnUpgradeOK, ref string ReturnErrorMessage) {
            string result = "";
            try {
                bool AddRule = false;
                string IncludeAddonName = null;
                string IncludeAddonGuid = null;
                int IncludeAddonID = 0;
                string UserError = null;
                int CS2 = 0;
                int CS = 0;
                string Criteria = null;
                bool IsFound = false;
                string AOName = null;
                string AOGuid = null;
                string AddOnType = null;
                int addonId = 0;
                string Basename;
                //
                Basename = GenericController.vbLCase(AddonNode.Name);
                if ((Basename == "page") || (Basename == "process") || (Basename == "addon") || (Basename == "add-on")) {
                    AOName =XmlController.GetXMLAttribute(core, IsFound, AddonNode, "name", "No Name");
                    if (string.IsNullOrEmpty(AOName)) {
                        AOName = "No Name";
                    }
                    AOGuid =XmlController.GetXMLAttribute(core, IsFound, AddonNode, "guid", AOName);
                    if (string.IsNullOrEmpty(AOGuid)) {
                        AOGuid = AOName;
                    }
                    AddOnType =XmlController.GetXMLAttribute(core, IsFound, AddonNode, "type", "");
                    Criteria = "(" + AddonGuidFieldName + "=" + core.db.encodeSQLText(AOGuid) + ")";
                    CS = core.db.csOpen(cnAddons, Criteria,"", false);
                    if (core.db.csOk(CS)) {
                        //
                        // Update the Addon
                        //
                        LogController.logInfo(core, "UpgradeAppFromLocalCollection, GUID match with existing Add-on, Updating Add-on [" + AOName + "], Guid [" + AOGuid + "]");
                    } else {
                        //
                        // not found by GUID - search name against name to update legacy Add-ons
                        //
                        core.db.csClose(ref CS);
                        Criteria = "(name=" + core.db.encodeSQLText(AOName) + ")and(" + AddonGuidFieldName + " is null)";
                        CS = core.db.csOpen(cnAddons, Criteria,"", false);
                    }
                    if (!core.db.csOk(CS)) {
                        //
                        // Could not find add-on
                        //
                        LogController.logInfo(core, "UpgradeAppFromLocalCollection, Add-on could not be created, skipping Add-on [" + AOName + "], Guid [" + AOGuid + "]");
                    } else {
                        addonId = core.db.csGetInteger(CS, "ID");
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
                                        if (true) {
                                            IncludeAddonName =XmlController.GetXMLAttribute(core, IsFound, PageInterface, "name", "");
                                            IncludeAddonGuid =XmlController.GetXMLAttribute(core, IsFound, PageInterface, "guid", IncludeAddonName);
                                            IncludeAddonID = 0;
                                            Criteria = "";
                                            if (!string.IsNullOrEmpty(IncludeAddonGuid)) {
                                                Criteria = AddonGuidFieldName + "=" + core.db.encodeSQLText(IncludeAddonGuid);
                                                if (string.IsNullOrEmpty(IncludeAddonName)) {
                                                    IncludeAddonName = "Add-on " + IncludeAddonGuid;
                                                }
                                            } else if (!string.IsNullOrEmpty(IncludeAddonName)) {
                                                Criteria = "(name=" + core.db.encodeSQLText(IncludeAddonName) + ")";
                                            }
                                            if (!string.IsNullOrEmpty(Criteria)) {
                                                CS2 = core.db.csOpen(cnAddons, Criteria);
                                                if (core.db.csOk(CS2)) {
                                                    IncludeAddonID = core.db.csGetInteger(CS2, "ID");
                                                }
                                                core.db.csClose(ref CS2);
                                                AddRule = false;
                                                if (IncludeAddonID == 0) {
                                                    UserError = "The include add-on [" + IncludeAddonName + "] could not be added because it was not found. If it is in the collection being installed, it must appear before any add-ons that include it.";
                                                    LogController.logInfo(core, "UpgradeAddFromLocalCollection_InstallAddonNode, UserError [" + UserError + "]");
                                                    ReturnUpgradeOK = false;
                                                    ReturnErrorMessage = ReturnErrorMessage + "<P>The collection was not installed because the add-on [" + AOName + "] requires an included add-on [" + IncludeAddonName + "] which could not be found. If it is in the collection being installed, it must appear before any add-ons that include it.</P>";
                                                } else {
                                                    CS2 = core.db.csOpenSql( "select ID from ccAddonIncludeRules where Addonid=" + addonId + " and IncludedAddonID=" + IncludeAddonID);
                                                    AddRule = !core.db.csOk(CS2);
                                                    core.db.csClose(ref CS2);
                                                }
                                                if (AddRule) {
                                                    CS2 = core.db.csInsertRecord("Add-on Include Rules", 0);
                                                    if (core.db.csOk(CS2)) {
                                                        core.db.csSet(CS2, "Addonid", addonId);
                                                        core.db.csSet(CS2, "IncludedAddonID", IncludeAddonID);
                                                    }
                                                    core.db.csClose(ref CS2);
                                                }
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                    }
                    core.db.csClose(ref CS);
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return result;
            //
        }
        //
        //======================================================================================================
        /// <summary>
        /// Import CDef on top of current configuration and the base configuration
        /// </summary>
        public static void installBaseCollection(CoreController core, bool isNewBuild, bool isRepairMode, ref List<string> nonCriticalErrorList, string logPrefix, ref List<string> blockCollectionList) {
            try {
                //
                // -- new build
                // 20171029 -- upgrading should restore base collection fields as a fix to deleted required fields
                const string baseCollectionFilename = "aoBase5.xml";
                string baseCollectionXml = core.programFiles.readFileText(baseCollectionFilename);
                if (string.IsNullOrEmpty(baseCollectionXml)) {
                    //
                    // -- base collection notfound
                    throw new ApplicationException("Cannot load [" + core.programFiles.localAbsRootPath + "aoBase5.xml]");
                } else {
                    {
                        //
                        // -- Special Case - must install base collection cdef first because it builds the system that the system needs to do everything else
                        LogController.logInfo(core, "installBaseCollection, install CDef first to verify system requirements");
                        CDefMiniCollectionModel.installCDefMiniCollectionFromXml(true, core, baseCollectionXml, isNewBuild, true, isRepairMode, ref nonCriticalErrorList, logPrefix, ref blockCollectionList);
                    }
                    //
                    // now treat as a regular collection and install - to pickup everything else 
                    string installPrivatePath = "installBaseCollection" + GenericController.GetRandomInteger(core).ToString() + "\\";
                    core.privateFiles.createPath(installPrivatePath);
                    core.programFiles.copyFile(baseCollectionFilename, installPrivatePath + baseCollectionFilename, core.privateFiles);
                    List<string> installedCollectionGuidList = new List<string>();
                    string installErrorMessage = "";
                    if (!installCollectionsFromPrivateFolder(core, installPrivatePath, ref installErrorMessage, ref installedCollectionGuidList, isNewBuild, isRepairMode, ref nonCriticalErrorList, logPrefix, ref blockCollectionList)) {
                        throw new ApplicationException(installErrorMessage);
                    }
                    core.privateFiles.deleteFolder(installPrivatePath);
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return the collectionList file stored in the root of the addon folder.
        /// </summary>
        /// <returns></returns>
        public static string getLocalCollectionStoreListXml(CoreController core) {
            string returnXml = "";
            try {
                string LastChangeDate = "";
                string FolderName = null;
                string collectionFilePathFilename = null;
                string CollectionGuid = null;
                string Collectionname = null;
                //
                collectionFilePathFilename = core.addon.getPrivateFilesAddonPath() + "Collections.xml";
                returnXml = core.privateFiles.readFileText(collectionFilePathFilename);
                if (string.IsNullOrWhiteSpace(returnXml)) {
                    List<FolderDetail> FolderList = core.privateFiles.getFolderList(core.addon.getPrivateFilesAddonPath());
                    if (FolderList.Count > 0) {
                        foreach (FolderDetail folder in FolderList) {
                            FolderName = folder.Name;
                            if (FolderName.Length > 34) {
                                if (GenericController.vbLCase(FolderName.Left(4)) != "temp") {
                                    CollectionGuid = FolderName.Substring(FolderName.Length - 32);
                                    Collectionname = FolderName.Left(FolderName.Length - CollectionGuid.Length - 1);
                                    CollectionGuid = CollectionGuid.Left(8) + "-" + CollectionGuid.Substring(8, 4) + "-" + CollectionGuid.Substring(12, 4) + "-" + CollectionGuid.Substring(16, 4) + "-" + CollectionGuid.Substring(20);
                                    CollectionGuid = "{" + CollectionGuid + "}";
                                    List<FolderDetail> SubFolderList = core.privateFiles.getFolderList(core.addon.getPrivateFilesAddonPath() + "\\" + FolderName);
                                    if (SubFolderList.Count>0) {
                                        FolderDetail lastSubFolder = SubFolderList.Last<FolderDetail>();
                                        FolderName = FolderName + "\\" + lastSubFolder.Name;
                                        LastChangeDate = lastSubFolder.Name.Substring(4, 2) + "/" + lastSubFolder.Name.Substring(6, 2) + "/" + lastSubFolder.Name.Left(4);
                                        if (!DateController.IsDate(LastChangeDate)) {
                                            LastChangeDate = "";
                                        }
                                    }
                                    returnXml = returnXml + "\r\n\t<Collection>";
                                    returnXml = returnXml + "\r\n\t\t<name>" + Collectionname + "</name>";
                                    returnXml = returnXml + "\r\n\t\t<guid>" + CollectionGuid + "</guid>";
                                    returnXml = returnXml + "\r\n\t\t<lastchangedate>" + LastChangeDate + "</lastchangedate>";
                                    returnXml += "\r\n\t\t<path>" + FolderName + "</path>";
                                    returnXml = returnXml + "\r\n\t</Collection>";
                                }
                            }
                        }
                    }
                    returnXml = "<CollectionList>" + returnXml + "\r\n</CollectionList>";
                    core.privateFiles.saveFile(collectionFilePathFilename, returnXml);
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return returnXml;
        }
        //
        //====================================================================================================
        /// <summary>
        /// get a list of collections available on the server
        /// </summary>
        public static bool getLocalCollectionStoreList(CoreController core, ref List<CollectionStoreClass> localCollectionStoreList, ref string return_ErrorMessage) {
            bool returnOk = true;
            try {
                //
                //-----------------------------------------------------------------------------------------------
                //   Load LocalCollections from the Collections.xml file
                //-----------------------------------------------------------------------------------------------
                //
                string localCollectionStoreListXml = getLocalCollectionStoreListXml(core);
                if (!string.IsNullOrEmpty(localCollectionStoreListXml)) {
                    XmlDocument LocalCollections = new XmlDocument();
                    try {
                        LocalCollections.LoadXml(localCollectionStoreListXml);
                    } catch (Exception) {
                        string Copy = "Error loading privateFiles\\addons\\Collections.xml";
                        LogController.logInfo(core, Copy);
                        return_ErrorMessage = return_ErrorMessage + "<P>" + Copy + "</P>";
                        returnOk = false;
                    }
                    if (returnOk) {
                        if (GenericController.vbLCase(LocalCollections.DocumentElement.Name) != GenericController.vbLCase(CollectionListRootNode)) {
                            string Copy = "The addons\\Collections.xml has an invalid root node, [" + LocalCollections.DocumentElement.Name + "] was received and [" + CollectionListRootNode + "] was expected.";
                            LogController.logInfo(core, Copy);
                            return_ErrorMessage = return_ErrorMessage + "<P>" + Copy + "</P>";
                            returnOk = false;
                        } else {
                            //
                            // Get a list of the collection guids on this server
                            //
                            if (GenericController.vbLCase(LocalCollections.DocumentElement.Name) == "collectionlist") {
                                foreach (XmlNode LocalListNode in LocalCollections.DocumentElement.ChildNodes) {
                                    switch (GenericController.vbLCase(LocalListNode.Name)) {
                                        case "collection":
                                            var collection = new CollectionStoreClass();
                                            localCollectionStoreList.Add(collection);
                                            foreach (XmlNode CollectionNode in LocalListNode.ChildNodes) {
                                                if (CollectionNode.Name.ToLowerInvariant() == "name") {
                                                    collection.name = CollectionNode.InnerText;
                                                } else if (CollectionNode.Name.ToLowerInvariant() == "guid") {
                                                    collection.guid = CollectionNode.InnerText;
                                                } else if (CollectionNode.Name.ToLowerInvariant() == "path") {
                                                    collection.path = CollectionNode.InnerText;
                                                } else if (CollectionNode.Name.ToLowerInvariant() == "lastchangedate") {
                                                    collection.lastChangeDate = GenericController.encodeDate( CollectionNode.InnerText );
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
                LogController.handleError( core,ex);
                throw;
            }
            return returnOk;
        }
        //
        //====================================================================================================
        /// <summary>
        /// return a list of collections on the 
        /// </summary>
        public static bool getRemoteCollectionStoreList(CoreController core, ref List<CollectionStoreClass> remoteCollectionStoreList) {
            bool result = false;
            try {
                var LibCollections = new XmlDocument();
                bool parseError = false;
                try {
                    LibCollections.Load("http://support.contensive.com/GetCollectionList?iv=" + core.codeVersion());
                } catch (Exception) {
                    string UserError = "There was an error reading the Collection Library. The site may be unavailable.";
                    LogController.logInfo(core, UserError);
                    ErrorController.addUserError(core, UserError);
                    parseError = true;
                }
                if (!parseError) {
                    if (GenericController.vbLCase(LibCollections.DocumentElement.Name) != GenericController.vbLCase(CollectionListRootNode)) {
                        string UserError = "There was an error reading the Collection Library file. The '" + CollectionListRootNode + "' element was not found.";
                        LogController.logInfo(core, UserError);
                        ErrorController.addUserError(core, UserError);
                    } else {
                        foreach (XmlNode CDef_Node in LibCollections.DocumentElement.ChildNodes) {
                            var collection = new CollectionStoreClass();
                            remoteCollectionStoreList.Add(collection);
                            switch (GenericController.vbLCase(CDef_Node.Name)) {
                                case "collection":
                                    //
                                    // Read the collection
                                    //
                                    foreach (XmlNode CollectionNode in CDef_Node.ChildNodes) {
                                        switch (GenericController.vbLCase(CollectionNode.Name)) {
                                            case "name":
                                                collection.name = CollectionNode.InnerText;
                                                break;
                                            case "guid":
                                                collection.guid = CollectionNode.InnerText;
                                                break;
                                            case "version":
                                                collection.version = CollectionNode.InnerText;
                                                break;
                                            case "description":
                                                collection.description = CollectionNode.InnerText;
                                                break;
                                            case "contensiveversion":
                                                collection.contensiveVersion = CollectionNode.InnerText;
                                                break;
                                            case "lastchangedate":
                                                collection.lastChangeDate = GenericController.encodeDate(CollectionNode.InnerText);
                                                break;
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                }
            } catch (Exception) {
                throw;
            }
            return result;
        }
        //
        //======================================================================================================
        /// <summary>
        /// data from local collection repository
        /// </summary>
        public class CollectionStoreClass {
            public string name;
            public string guid;
            public string path;
            public DateTime lastChangeDate;
            public string version;
            public string description;
            public string contensiveVersion;
        }
    }
}
