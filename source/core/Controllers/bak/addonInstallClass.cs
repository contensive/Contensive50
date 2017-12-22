

using System.Xml;
using Controllers;

using Models;
using Models.Entity;
// 

namespace Contensive.Core {
    
    // 
    // ====================================================================================================
    // '' <summary>
    // '' install addon collections
    // '' </summary>
    public class addonInstallClass {
        
        // 
        // ====================================================================================================
        // 
        //    DownloadCollectionFiles
        // 
        //    Download Library collection files into a folder
        //        Download Collection file and all attachments (DLLs) into working folder
        //        Unzips any collection files
        //        Returns true if it all downloads OK
        // 
        private static bool DownloadCollectionFiles(coreClass cpCore, string workingPath, string CollectionGuid, ref DateTime return_CollectionLastChangeDate, ref string return_ErrorMessage) {
            DownloadCollectionFiles = false;
            try {
                // 
                int CollectionFileCnt;
                string CollectionFilePath;
                XmlDocument Doc = new XmlDocument();
                string URL;
                string ResourceFilename;
                string ResourceLink;
                string CollectionVersion;
                string CollectionFileLink;
                XmlDocument CollectionFile = new XmlDocument();
                string Collectionname;
                int Pos;
                string UserError;
                XmlNode CDefSection;
                XmlNode CDefInterfaces;
                XmlNode ActiveXNode;
                string errorPrefix;
                int downloadRetry;
                const int downloadRetryMax = 3;
                // 
                logController.appendInstallLog(cpCore, ("downloading collection [" 
                                + (CollectionGuid + "]")));
                // 
                // ---------------------------------------------------------------------------------------------------------------
                //  Request the Download file for this collection
                // ---------------------------------------------------------------------------------------------------------------
                // 
                Doc = new XmlDocument();
                URL = ("http://support.contensive.com/GetCollection?iv=" 
                            + (cpCore.codeVersion() + ("&guid=" + CollectionGuid)));
                errorPrefix = ("DownloadCollectionFiles, Error reading the collection library status file from the server for Collect" +
                "ion [" 
                            + (CollectionGuid + ("], download URL [" 
                            + (URL + "]. "))));
                downloadRetry = 0;
                int downloadDelay = 2000;
                for (
                ; (downloadRetry < downloadRetryMax); 
                ) {
                    try {
                        DownloadCollectionFiles = true;
                        return_ErrorMessage = "";
                        Threading.Thread.Sleep(downloadDelay);
                        // 
                        //  -- download file
                        System.Net.WebRequest rq = System.Net.WebRequest.Create(URL);
                        rq.Timeout = 60000;
                        System.Net.WebResponse response = rq.GetResponse();
                        IO.Stream responseStream = response.GetResponseStream();
                        XmlTextReader reader = new XmlTextReader(responseStream);
                        Doc.Load(reader);
                        break; //Warning!!! Review that break works as 'Exit Do' as it could be in a nested instruction like switch
                        // Call Doc.Load(URL)
                    }
                    catch (Exception ex) {
                        // 
                        //  this error could be data related, and may not be critical. log issue and continue
                        // 
                        downloadDelay += 2000;
                        return_ErrorMessage = ("There was an error while requesting the download details for collection [" 
                                    + (CollectionGuid + "]"));
                        DownloadCollectionFiles = false;
                        logController.appendInstallLog(cpCore, (errorPrefix + ("There was a parse error reading the response [" 
                                        + (ex.ToString() + "]"))));
                    }
                    
                    downloadRetry++;
                }
                
                if (string.IsNullOrEmpty(return_ErrorMessage)) {
                    // 
                    //  continue if no errors
                    // 
                    // With...
                    if ((Doc.DocumentElement.Name.ToLower() != genericController.vbLCase(DownloadFileRootNode))) {
                        return_ErrorMessage = ("The collection file from the server was Not valid for collection [" 
                                    + (CollectionGuid + "]"));
                        DownloadCollectionFiles = false;
                        logController.appendInstallLog(cpCore, (errorPrefix + ("The response has a basename [" 
                                        + (Doc.DocumentElement.Name + ("] but [" 
                                        + (DownloadFileRootNode + "] was expected."))))));
                    }
                    else {
                        // 
                        // ------------------------------------------------------------------
                        //  Parse the Download File and download each file into the working folder
                        // ------------------------------------------------------------------
                        // 
                        if ((Doc.DocumentElement.ChildNodes.Count == 0)) {
                            return_ErrorMessage = "The collection library status file from the server has a valid basename, but no childnodes.";
                            logController.appendInstallLog(cpCore, (errorPrefix + "The collection library status file from the server has a valid basename, but no childnodes. The colle" +
                                "ction was probably Not found"));
                            DownloadCollectionFiles = false;
                        }
                        else {
                            // With...
                            foreach (CDefSection in Doc.DocumentElement.ChildNodes) {
                                switch (genericController.vbLCase(CDefSection.Name)) {
                                    case "collection":
                                        ResourceFilename = "";
                                        ResourceLink = "";
                                        Collectionname = "";
                                        CollectionGuid = "";
                                        CollectionVersion = "";
                                        CollectionFileLink = "";
                                        foreach (CDefInterfaces in CDefSection.ChildNodes) {
                                            switch (genericController.vbLCase(CDefInterfaces.Name)) {
                                                case "name":
                                                    Collectionname = CDefInterfaces.InnerText;
                                                    break;
                                                case "help":
                                                    cpCore.privateFiles.saveFile((workingPath + "Collection.hlp"), CDefInterfaces.InnerText);
                                                    break;
                                                case "guid":
                                                    CollectionGuid = CDefInterfaces.InnerText;
                                                    break;
                                                case "lastchangedate":
                                                    return_CollectionLastChangeDate = genericController.EncodeDate(CDefInterfaces.InnerText);
                                                    break;
                                                case "version":
                                                    CollectionVersion = CDefInterfaces.InnerText;
                                                    break;
                                                case "collectionfilelink":
                                                    CollectionFileLink = CDefInterfaces.InnerText;
                                                    CollectionFileCnt = (CollectionFileCnt + 1);
                                                    if ((CollectionFileLink != "")) {
                                                        Pos = InStrRev(CollectionFileLink, "/");
                                                        if (((Pos <= 0) 
                                                                    && (Pos < CollectionFileLink.Length))) {
                                                            // 
                                                            //  Skip this file because the collecion file link has no slash (no file)
                                                            // 
                                                            logController.appendInstallLog(cpCore, (errorPrefix + ("Collection [" 
                                                                            + (Collectionname + ("] was Not installed because the Collection File Link does Not point to a valid file [" 
                                                                            + (CollectionFileLink + "]"))))));
                                                        }
                                                        else {
                                                            CollectionFilePath = (workingPath + CollectionFileLink.Substring(Pos));
                                                            cpCore.privateFiles.SaveRemoteFile(CollectionFileLink, CollectionFilePath);
                                                            //  BuildCollectionFolder takes care of the unzipping.
                                                            // If genericController.vbLCase(Right(CollectionFilePath, 4)) = ".zip" Then
                                                            //     Call UnzipAndDeleteFile_AndWait(CollectionFilePath)
                                                            // End If
                                                            // DownloadCollectionFiles = True
                                                        }
                                                        
                                                    }
                                                    
                                                    break;
                                                case "activexdll":
                                                case "resourcelink":
                                                    ResourceFilename = "";
                                                    ResourceLink = "";
                                                    foreach (ActiveXNode in CDefInterfaces.ChildNodes) {
                                                        switch (genericController.vbLCase(ActiveXNode.Name)) {
                                                            case "filename":
                                                                ResourceFilename = ActiveXNode.InnerText;
                                                                break;
                                                            case "link":
                                                                ResourceLink = ActiveXNode.InnerText;
                                                                break;
                                                        }
                                                    }
                                                    
                                                    if ((ResourceLink == "")) {
                                                        UserError = ("There was an error processing a collection in the download file [" 
                                                                    + (Collectionname + ("]. An ActiveXDll node with filename [" 
                                                                    + (ResourceFilename + "] contained no \'Link\' attribute."))));
                                                        logController.appendInstallLog(cpCore, (errorPrefix + UserError));
                                                    }
                                                    else {
                                                        if ((ResourceFilename == "")) {
                                                            // 
                                                            //  Take Filename from Link
                                                            // 
                                                            Pos = InStrRev(ResourceLink, "/");
                                                            if ((Pos != 0)) {
                                                                ResourceFilename = ResourceLink.Substring(Pos);
                                                            }
                                                            
                                                        }
                                                        
                                                        if ((ResourceFilename == "")) {
                                                            UserError = ("There was an error processing a collection in the download file [" 
                                                                        + (Collectionname + ("]. The ActiveX filename attribute was empty, and the filename could not be read from the link [" 
                                                                        + (ResourceLink + "]."))));
                                                            logController.appendInstallLog(cpCore, (errorPrefix + UserError));
                                                        }
                                                        else {
                                                            cpCore.privateFiles.SaveRemoteFile(ResourceLink, (workingPath + ResourceFilename));
                                                        }
                                                        
                                                    }
                                                    
                                                    break;
                                            }
                                        }
                                        
                                        break;
                                }
                            }
                            
                            if ((CollectionFileCnt == 0)) {
                                logController.appendInstallLog(cpCore, (errorPrefix + "The collection was requested and downloaded, but was not installed because the download file did not " +
                                    "have a collection root node."));
                            }
                            
                        }
                        
                    }
                    
                }
                
                // 
                //  no - register anything that downloaded correctly - if this collection contains an import, and one of the imports has a problem, all the rest need to continue
                // 
                // '
                // If Not DownloadCollectionFiles Then
                //     '
                //     ' Must clear these out, if there is an error, a reset will keep the error message from making it to the page
                //     '
                //     Return_IISResetRequired = False
                //     Return_RegisterList = ""
                // End If
                // 
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return DownloadCollectionFiles;
        }
        
        // 
        // ====================================================================================================
        //    Upgrade all Apps from a Library Collection
        //    If the collection is not in the local collection, download it, otherwise, use what is in local (do not check for updates)
        //      -Does not check if local collections are up-to-date, assume they are. (builderAllLocalCollectionsFromLib2 upgrades local collections)
        //    If TargetAppName is blank, force install on all apps. (force install means install if missing)
        //    Go through each app and call UpgradeAllAppsFromLocalCollect with allowupgrade FALSE (if found in app already, skip the app)
        //        If Collection is already installed on an App, do not builder.
        //    Returns true if no errors during upgrade
        // =========================================================================================================================
        // 
        public static bool installCollectionFromRemoteRepo(coreClass cpCore, string CollectionGuid, ref string return_ErrorMessage, string ImportFromCollectionsGuidList, bool IsNewBuild, ref List<string> nonCriticalErrorList) {
            bool UpgradeOK = true;
            try {
                // 
                string CollectionVersionFolderName;
                string workingPath;
                DateTime CollectionLastChangeDate;
                List<string> collectionGuidList = new List<string>();
                if ((CollectionGuid.Length < 38)) {
                    if ((CollectionGuid.Length == 32)) {
                        CollectionGuid = (CollectionGuid.Substring(0, 8) + ("-" 
                                    + (CollectionGuid.Substring(8, 4) + ("-" 
                                    + (CollectionGuid.Substring(12, 4) + ("-" 
                                    + (CollectionGuid.Substring(16, 4) + ("-" + CollectionGuid.Substring(20)))))))));
                    }
                    
                    if ((CollectionGuid.Length == 36)) {
                        CollectionGuid = ("{" 
                                    + (CollectionGuid + "}"));
                    }
                    
                }
                
                // 
                //  Install it if it is not already here
                // 
                CollectionVersionFolderName = GetCollectionPath(cpCore, CollectionGuid);
                if ((CollectionVersionFolderName == "")) {
                    // 
                    //  Download all files for this collection and build the collection folder(s)
                    // 
                    workingPath = (cpCore.addon.getPrivateFilesAddonPath() + ("temp_" 
                                + (genericController.GetRandomInteger() + "\\")));
                    cpCore.privateFiles.createPath(workingPath);
                    // 
                    UpgradeOK = addonInstallClass.DownloadCollectionFiles(cpCore, workingPath, CollectionGuid, CollectionLastChangeDate, return_ErrorMessage);
                    if (!UpgradeOK) {
                        UpgradeOK = UpgradeOK;
                    }
                    else {
                        UpgradeOK = BuildLocalCollectionReposFromFolder(cpCore, workingPath, CollectionLastChangeDate, collectionGuidList, return_ErrorMessage, false);
                        if (!UpgradeOK) {
                            UpgradeOK = UpgradeOK;
                        }
                        
                    }
                    
                    // 
                    cpCore.privateFiles.DeleteFileFolder(workingPath);
                }
                
                // 
                //  Upgrade the server from the collection files
                // 
                if (UpgradeOK) {
                    UpgradeOK = installCollectionFromLocalRepo(cpCore, CollectionGuid, cpCore.siteProperties.dataBuildVersion, return_ErrorMessage, ImportFromCollectionsGuidList, IsNewBuild, nonCriticalErrorList);
                    if (!UpgradeOK) {
                        UpgradeOK = UpgradeOK;
                    }
                    
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
                throw ex;
            }
            
            return UpgradeOK;
        }
        
        // 
        // ====================================================================================================
        // 
        //  Upgrades all collections, registers and resets the server if needed
        // 
        public static bool UpgradeLocalCollectionRepoFromRemoteCollectionRepo(coreClass cpCore, ref string return_ErrorMessage, ref string return_RegisterList, ref bool return_IISResetRequired, bool IsNewBuild, ref List<string> nonCriticalErrorList) {
            bool returnOk = true;
            try {
                bool localCollectionUpToDate;
                string[] GuidArray;
                int GuidCnt;
                int GuidPtr;
                int RequestPtr;
                string SupportURL;
                string GuidList;
                DateTime CollectionLastChangeDate;
                string workingPath;
                string LocalFile;
                string LocalGuid;
                string LocalLastChangeDateStr;
                DateTime LocalLastChangeDate;
                string LibName = String.Empty;
                bool LibSystem;
                string LibGUID;
                string LibLastChangeDateStr;
                string LibContensiveVersion = String.Empty;
                DateTime LibLastChangeDate;
                XmlNode LibListNode;
                XmlNode LocalListNode;
                XmlNode CollectionNode;
                XmlNode LocalLastChangeNode;
                XmlDocument LibraryCollections = new XmlDocument();
                XmlDocument LocalCollections = new XmlDocument();
                XmlDocument Doc = new XmlDocument();
                string Copy;
                bool allowLogging;
                // Dim builder As New coreBuilderClass(cpCore)
                // 
                // -----------------------------------------------------------------------------------------------
                //    Load LocalCollections from the Collections.xml file
                // -----------------------------------------------------------------------------------------------
                // 
                allowLogging = false;
                if (allowLogging) {
                    logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), Enter");
                }
                
                LocalFile = getCollectionListFile(cpCore);
                if ((LocalFile != "")) {
                    LocalCollections = new XmlDocument();
                    try {
                        LocalCollections.LoadXml(LocalFile);
                    }
                    catch (Exception ex) {
                        if (allowLogging) {
                            logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), parse error reading collections.xml");
                        }
                        
                        Copy = "Error loading privateFiles\\addons\\Collections.xml";
                        logController.appendInstallLog(cpCore, Copy);
                        return_ErrorMessage = (return_ErrorMessage + ("<P>" 
                                    + (Copy + "</P>")));
                        returnOk = false;
                    }
                    
                    if (returnOk) {
                        if ((genericController.vbLCase(LocalCollections.DocumentElement.Name) != genericController.vbLCase(CollectionListRootNode))) {
                            if (allowLogging) {
                                logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), The addons\\Collections.xml file has an invalid root node");
                            }
                            
                            Copy = ("The addons\\Collections.xml has an invalid root node, [" 
                                        + (LocalCollections.DocumentElement.Name + ("] was received and [" 
                                        + (CollectionListRootNode + "] was expected."))));
                            logController.appendInstallLog(cpCore, Copy);
                            return_ErrorMessage = (return_ErrorMessage + ("<P>" 
                                        + (Copy + "</P>")));
                            returnOk = false;
                        }
                        else {
                            // 
                            //  Get a list of the collection guids on this server
                            // 
                            GuidCnt = 0;
                            // With...
                            if ((genericController.vbLCase(LocalCollections.DocumentElement.Name) == "collectionlist")) {
                                foreach (LocalListNode in LocalCollections.DocumentElement.ChildNodes) {
                                    switch (genericController.vbLCase(LocalListNode.Name)) {
                                        case "collection":
                                            foreach (CollectionNode in LocalListNode.ChildNodes) {
                                                if ((genericController.vbLCase(CollectionNode.Name) == "guid")) {
                                                    object Preserve;
                                                    GuidArray[GuidCnt];
                                                    GuidArray[GuidCnt] = CollectionNode.InnerText;
                                                    GuidCnt = (GuidCnt + 1);
                                                    break;
                                                }
                                                
                                            }
                                            
                                            break;
                                    }
                                }
                                
                            }
                            
                            if (allowLogging) {
                                logController.appendLog(cpCore, ("UpgradeAllLocalCollectionsFromLib3(), collection.xml file has " 
                                                + (GuidCnt + " collection nodes.")));
                            }
                            
                            if ((GuidCnt > 0)) {
                                // 
                                //  Request collection updates 10 at a time
                                // 
                                GuidPtr = 0;
                                while ((GuidPtr < GuidCnt)) {
                                    RequestPtr = 0;
                                    GuidList = "";
                                    while (((GuidPtr < GuidCnt) 
                                                && (RequestPtr < 10))) {
                                        GuidList = (GuidList + ("," + GuidArray[GuidPtr]));
                                        GuidPtr = (GuidPtr + 1);
                                        RequestPtr = (RequestPtr + 1);
                                    }
                                    
                                    // 
                                    //  Request these 10 from the support library
                                    // 
                                    // If genericController.vbInstr(1, GuidList, "58c9", vbTextCompare) <> 0 Then
                                    //     GuidList = GuidList
                                    // End If
                                    if ((GuidList != "")) {
                                        GuidList = GuidList.Substring(1);
                                        // 
                                        // -----------------------------------------------------------------------------------------------
                                        //    Load LibraryCollections from the Support Site
                                        // -----------------------------------------------------------------------------------------------
                                        // 
                                        if (allowLogging) {
                                            logController.appendLog(cpCore, ("UpgradeAllLocalCollectionsFromLib3(), requesting Library updates for [" 
                                                            + (GuidList + "]")));
                                        }
                                        
                                        // hint = "Getting CollectionList"
                                        LibraryCollections = new XmlDocument();
                                        SupportURL = ("http://support.contensive.com/GetCollectionList?iv=" 
                                                    + (cpCore.codeVersion() + ("&guidlist=" + EncodeRequestVariable(GuidList))));
                                        bool loadOK = true;
                                        try {
                                            LibraryCollections.Load(SupportURL);
                                        }
                                        catch (Exception ex) {
                                            if (allowLogging) {
                                                logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), Error downloading or loading GetCollectionList from Support.");
                                            }
                                            
                                            Copy = "Error downloading or loading GetCollectionList from Support.";
                                            logController.appendInstallLog(cpCore, (Copy + (", the request was [" 
                                                            + (SupportURL + "]"))));
                                            return_ErrorMessage = (return_ErrorMessage + ("<P>" 
                                                        + (Copy + "</P>")));
                                            returnOk = false;
                                            loadOK = false;
                                        }
                                        
                                        if (loadOK) {
                                            if (true) {
                                                if ((genericController.vbLCase(LibraryCollections.DocumentElement.Name) != genericController.vbLCase(CollectionListRootNode))) {
                                                    Copy = ("The GetCollectionList support site remote method returned an xml file with an invalid root node, [" 
                                                                + (LibraryCollections.DocumentElement.Name + ("] was received and [" 
                                                                + (CollectionListRootNode + "] was expected."))));
                                                    if (allowLogging) {
                                                        logController.appendLog(cpCore, ("UpgradeAllLocalCollectionsFromLib3(), " + Copy));
                                                    }
                                                    
                                                    logController.appendInstallLog(cpCore, (Copy + (", the request was [" 
                                                                    + (SupportURL + "]"))));
                                                    return_ErrorMessage = (return_ErrorMessage + ("<P>" 
                                                                + (Copy + "</P>")));
                                                    returnOk = false;
                                                }
                                                else {
                                                    // With...
                                                    if ((genericController.vbLCase(LocalCollections.DocumentElement.Name) != "collectionlist")) {
                                                        logController.appendLog(cpCore, ("UpgradeAllLocalCollectionsFromLib3(), The Library response did not have a collectioinlist top node, t" +
                                                            "he request was [" 
                                                                        + (SupportURL + "]")));
                                                    }
                                                    else {
                                                        // 
                                                        // -----------------------------------------------------------------------------------------------
                                                        //  Search for Collection Updates Needed
                                                        // -----------------------------------------------------------------------------------------------
                                                        // 
                                                        foreach (LocalListNode in LocalCollections.DocumentElement.ChildNodes) {
                                                            localCollectionUpToDate = false;
                                                            if (allowLogging) {
                                                                logController.appendLog(cpCore, ("UpgradeAllLocalCollectionsFromLib3(), Process local collection.xml node [" 
                                                                                + (LocalListNode.Name + "]")));
                                                            }
                                                            
                                                            switch (genericController.vbLCase(LocalListNode.Name)) {
                                                                case "collection":
                                                                    LocalGuid = "";
                                                                    LocalLastChangeDateStr = "";
                                                                    LocalCollections.DocumentElement.MinValue;
                                                                    LocalLastChangeNode = null;
                                                                    foreach (CollectionNode in LocalListNode.ChildNodes) {
                                                                        switch (genericController.vbLCase(CollectionNode.Name)) {
                                                                            case "guid":
                                                                                LocalGuid = genericController.vbLCase(CollectionNode.InnerText);
                                                                                // LocalGUID = genericController.vbReplace(LocalGUID, "{", "")
                                                                                // LocalGUID = genericController.vbReplace(LocalGUID, "}", "")
                                                                                // LocalGUID = genericController.vbReplace(LocalGUID, "-", "")
                                                                                break;
                                                                            case "lastchangedate":
                                                                                LocalLastChangeDateStr = CollectionNode.InnerText;
                                                                                LocalLastChangeNode = CollectionNode;
                                                                                break;
                                                                        }
                                                                    }
                                                                    
                                                                    if ((LocalGuid != "")) {
                                                                        if (!IsDate(LocalLastChangeDateStr)) {
                                                                            LocalCollections.DocumentElement.MinValue;
                                                                        }
                                                                        else {
                                                                            LocalLastChangeDate = genericController.EncodeDate(LocalLastChangeDateStr);
                                                                        }
                                                                        
                                                                    }
                                                                    
                                                                    if (allowLogging) {
                                                                        logController.appendLog(cpCore, ("UpgradeAllLocalCollectionsFromLib3(), node is collection, LocalGuid [" 
                                                                                        + (LocalGuid + ("], LocalLastChangeDateStr [" 
                                                                                        + (LocalLastChangeDateStr + "]")))));
                                                                    }
                                                                    
                                                                    // 
                                                                    //  go through each collection on the Library and find the local collection guid
                                                                    // 
                                                                    foreach (LibListNode in LibraryCollections.DocumentElement.ChildNodes) {
                                                                        if (localCollectionUpToDate) {
                                                                            break;
                                                                        }
                                                                        
                                                                        switch (genericController.vbLCase(LibListNode.Name)) {
                                                                            case "collection":
                                                                                LibGUID = "";
                                                                                LibLastChangeDateStr = "";
                                                                                LocalCollections.DocumentElement.MinValue;
                                                                                foreach (CollectionNode in LibListNode.ChildNodes) {
                                                                                    switch (genericController.vbLCase(CollectionNode.Name)) {
                                                                                        case "name":
                                                                                            LibName = genericController.vbLCase(CollectionNode.InnerText);
                                                                                            break;
                                                                                        case "system":
                                                                                            LibSystem = genericController.EncodeBoolean(CollectionNode.InnerText);
                                                                                            break;
                                                                                        case "guid":
                                                                                            LibGUID = genericController.vbLCase(CollectionNode.InnerText);
                                                                                            // LibGUID = genericController.vbReplace(LibGUID, "{", "")
                                                                                            // LibGUID = genericController.vbReplace(LibGUID, "}", "")
                                                                                            // LibGUID = genericController.vbReplace(LibGUID, "-", "")
                                                                                            break;
                                                                                        case "lastchangedate":
                                                                                            LibLastChangeDateStr = CollectionNode.InnerText;
                                                                                            LibLastChangeDateStr = LibLastChangeDateStr;
                                                                                            break;
                                                                                        case "contensiveversion":
                                                                                            LibContensiveVersion = CollectionNode.InnerText;
                                                                                            break;
                                                                                    }
                                                                                }
                                                                                
                                                                                if ((LibGUID != "")) {
                                                                                    if ((genericController.vbInstr(1, LibGUID, "58c9", vbTextCompare) != 0)) {
                                                                                        LibGUID = LibGUID;
                                                                                    }
                                                                                    
                                                                                    if (((LibGUID != "") 
                                                                                                && ((LibGUID == LocalGuid) 
                                                                                                && ((LibContensiveVersion == "") 
                                                                                                || (LibContensiveVersion <= cpCore.codeVersion()))))) {
                                                                                        // 
                                                                                        //  LibCollection matches the LocalCollection - process the upgrade
                                                                                        // 
                                                                                        if (allowLogging) {
                                                                                            logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), Library collection node found that matches");
                                                                                        }
                                                                                        
                                                                                        if ((genericController.vbInstr(1, LibGUID, "58c9", vbTextCompare) != 0)) {
                                                                                            LibGUID = LibGUID;
                                                                                        }
                                                                                        
                                                                                        if (!IsDate(LibLastChangeDateStr)) {
                                                                                            LocalCollections.DocumentElement.MinValue;
                                                                                        }
                                                                                        else {
                                                                                            LibLastChangeDate = genericController.EncodeDate(LibLastChangeDateStr);
                                                                                        }
                                                                                        
                                                                                        //  TestPoint 1.1 - Test each collection for upgrade
                                                                                        if ((LibLastChangeDate > LocalLastChangeDate)) {
                                                                                            // 
                                                                                            //  LibLastChangeDate <>0, and it is > local lastchangedate
                                                                                            // 
                                                                                            workingPath = (cpCore.addon.getPrivateFilesAddonPath() + ("\\temp_" 
                                                                                                        + (genericController.GetRandomInteger() + "\\")));
                                                                                            if (allowLogging) {
                                                                                                logController.appendLog(cpCore, ("UpgradeAllLocalCollectionsFromLib3(), matching library collection is newer, start upgrade [" 
                                                                                                                + (workingPath + "].")));
                                                                                            }
                                                                                            
                                                                                            logController.appendInstallLog(cpCore, ("Upgrading Collection [" 
                                                                                                            + (LibGUID + ("], Library name [" 
                                                                                                            + (LibName + ("], because LocalChangeDate [" 
                                                                                                            + (LocalLastChangeDate + ("] < LibraryChangeDate [" 
                                                                                                            + (LibLastChangeDate + "]")))))))));
                                                                                            // 
                                                                                            //  Upgrade Needed
                                                                                            // 
                                                                                            cpCore.privateFiles.createPath(workingPath);
                                                                                            // 
                                                                                            returnOk = addonInstallClass.DownloadCollectionFiles(cpCore, workingPath, LibGUID, CollectionLastChangeDate, return_ErrorMessage);
                                                                                            if (allowLogging) {
                                                                                                logController.appendLog(cpCore, ("UpgradeAllLocalCollectionsFromLib3(), DownloadCollectionFiles returned " + returnOk));
                                                                                            }
                                                                                            
                                                                                            if (returnOk) {
                                                                                                List<string> listGuidList = new List<string>();
                                                                                                returnOk = BuildLocalCollectionReposFromFolder(cpCore, workingPath, CollectionLastChangeDate, listGuidList, return_ErrorMessage, allowLogging);
                                                                                                if (allowLogging) {
                                                                                                    logController.appendLog(cpCore, ("UpgradeAllLocalCollectionsFromLib3(), BuildLocalCollectionFolder returned " + returnOk));
                                                                                                }
                                                                                                
                                                                                                // 
                                                                                                if (allowLogging) {
                                                                                                    logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), working folder not deleted because debugging. Delete tmp folder" +
                                                                                                        "s when finished.");
                                                                                                }
                                                                                                else {
                                                                                                    cpCore.privateFiles.DeleteFileFolder(workingPath);
                                                                                                }
                                                                                                
                                                                                                // 
                                                                                                //  Upgrade the apps from the collection files, do not install on any apps
                                                                                                // 
                                                                                                if (returnOk) {
                                                                                                    returnOk = installCollectionFromLocalRepo(cpCore, LibGUID, cpCore.siteProperties.dataBuildVersion, return_ErrorMessage, "", IsNewBuild, nonCriticalErrorList);
                                                                                                    if (allowLogging) {
                                                                                                        logController.appendLog(cpCore, ("UpgradeAllLocalCollectionsFromLib3(), UpgradeAllAppsFromLocalCollection returned " + returnOk));
                                                                                                    }
                                                                                                    
                                                                                                    // 
                                                                                                    //  make sure this issue is logged and clear the flag to let other local collections install
                                                                                                    // 
                                                                                                    if (!returnOk) {
                                                                                                        if (allowLogging) {
                                                                                                            logController.appendLog(cpCore, ("UpgradeAllLocalCollectionsFromLib3(), for this local collection, process returned " + returnOk));
                                                                                                        }
                                                                                                        
                                                                                                        logController.appendInstallLog(cpCore, ("There was a problem upgrading Collection [" 
                                                                                                                        + (LibGUID + ("], Library name [" 
                                                                                                                        + (LibName + ("], error message [" 
                                                                                                                        + (return_ErrorMessage + ("], will clear error and continue with the next collection, the request was [" 
                                                                                                                        + (SupportURL + "]")))))))));
                                                                                                        returnOk = true;
                                                                                                    }
                                                                                                    
                                                                                                }
                                                                                                
                                                                                                // 
                                                                                                //  this local collection has been resolved, go to the next local collection
                                                                                                // 
                                                                                                localCollectionUpToDate = true;
                                                                                                if (!returnOk) {
                                                                                                    logController.appendInstallLog(cpCore, ("There was a problem upgrading Collection [" 
                                                                                                                    + (LibGUID + ("], Library name [" 
                                                                                                                    + (LibName + ("], error message [" 
                                                                                                                    + (return_ErrorMessage + "], will clear error and continue with the next collection")))))));
                                                                                                    returnOk = true;
                                                                                                }
                                                                                                
                                                                                            }
                                                                                            
                                                                                        }
                                                                                        
                                                                                    }
                                                                                    
                                                                                }
                                                                                
                                                                                break;
                                                                        }
                                                                        // With...
                                                                        ((Exception)(ex));
                                                                        cpCore.handleException(ex);
                                                                        throw;
                                                                        try {
                                                                            return returnOk;
                                                                        }
                                                                        
                                                                        // 
                                                                        // ====================================================================================================
                                                                        //    Upgrade a collection from the files in a working folder
                                                                        ((bool)(BuildLocalCollectionReposFromFolder(((coreClass)(cpCore)), ((string)(sourcePrivateFolderPath)), ((DateTime)(CollectionLastChangeDate)), ref ((List<string>)(return_CollectionGUIDList)), ref ((string)(return_ErrorMessage)), ((bool)(allowLogging)))));
                                                                        bool success = false;
                                                                        try {
                                                                            if (cpCore.privateFiles.pathExists(sourcePrivateFolderPath)) {
                                                                                logController.appendInstallLog(cpCore, ("BuildLocalCollectionFolder, processing files in private folder [" 
                                                                                                + (sourcePrivateFolderPath + "]")));
                                                                                IO.FileInfo[] SrcFileNamelist = cpCore.privateFiles.getFileList(sourcePrivateFolderPath);
                                                                                foreach (IO.FileInfo file in SrcFileNamelist) {
                                                                                    if (((file.Extension == ".zip") 
                                                                                                || (file.Extension == ".xml"))) {
                                                                                        string collectionGuid = "";
                                                                                        success = BuildLocalCollectionRepoFromFile(cpCore, (sourcePrivateFolderPath + file.Name), CollectionLastChangeDate, collectionGuid, return_ErrorMessage, allowLogging);
                                                                                        return_CollectionGUIDList.Add(collectionGuid);
                                                                                    }
                                                                                    
                                                                                }
                                                                                
                                                                            }
                                                                            
                                                                        }
                                                                        catch (Exception ex) {
                                                                            cpCore.handleException(ex);
                                                                            throw;
                                                                        }
                                                                        
                                                                        return success;
                                                                        // 
                                                                        // 
                                                                        // 
                                                                        ((bool)(BuildLocalCollectionRepoFromFile(((coreClass)(cpCore)), ((string)(collectionPathFilename)), ((DateTime)(CollectionLastChangeDate)), ref ((string)(return_CollectionGUID)), ref ((string)(return_ErrorMessage)), ((bool)(allowLogging)))));
                                                                        bool result = true;
                                                                        try {
                                                                            string ResourceType;
                                                                            string CollectionVersionFolderName = String.Empty;
                                                                            DateTime ChildCollectionLastChangeDate;
                                                                            string ChildWorkingPath;
                                                                            string ChildCollectionGUID;
                                                                            string ChildCollectionName;
                                                                            bool Found;
                                                                            XmlDocument CollectionFile = new XmlDocument();
                                                                            bool UpdatingCollection;
                                                                            string Collectionname = String.Empty;
                                                                            DateTime NowTime;
                                                                            int NowPart;
                                                                            IO.FileInfo[] SrcFileNamelist;
                                                                            string TimeStamp;
                                                                            int Pos;
                                                                            string CollectionFolder;
                                                                            string CollectionGuid = String.Empty;
                                                                            string AOGuid;
                                                                            string AOName;
                                                                            bool IsFound;
                                                                            string Filename;
                                                                            XmlNode CDefSection;
                                                                            XmlDocument Doc = new XmlDocument();
                                                                            XmlNode CDefInterfaces;
                                                                            bool StatusOK;
                                                                            string CollectionFileBaseName;
                                                                            xmlController XMLTools = new xmlController(cpCore);
                                                                            string CollectionFolderName = "";
                                                                            bool CollectionFileFound = false;
                                                                            bool ZipFileFound = false;
                                                                            string collectionPath = "";
                                                                            string collectionFilename = "";
                                                                            if (allowLogging) {
                                                                                logController.appendLog(cpCore, "BuildLocalCollectionFolder(), Enter");
                                                                            }
                                                                            
                                                                            // 
                                                                            cpCore.privateFiles.splitPathFilename(collectionPathFilename, collectionPath, collectionFilename);
                                                                            if (!cpCore.privateFiles.pathExists(collectionPath)) {
                                                                                // 
                                                                                //  The working folder is not there
                                                                                // 
                                                                                result = false;
                                                                                return_ErrorMessage = "<p>There was a problem with the installation. The installation folder is not valid.</p>";
                                                                                if (allowLogging) {
                                                                                    logController.appendLog(cpCore, ("BuildLocalCollectionFolder(), " + return_ErrorMessage));
                                                                                }
                                                                                
                                                                                logController.appendInstallLog(cpCore, ("BuildLocalCollectionFolder, CheckFileFolder was false for the private folder [" 
                                                                                                + (collectionPath + "]")));
                                                                            }
                                                                            else {
                                                                                logController.appendInstallLog(cpCore, ("BuildLocalCollectionFolder, processing files in private folder [" 
                                                                                                + (collectionPath + "]")));
                                                                                // 
                                                                                //  move collection file to a temp directory
                                                                                // 
                                                                                string tmpInstallPath = ("tmpInstallCollection" 
                                                                                            + (genericController.createGuid().Replace("{", "").Replace("}", "").Replace("-", "") + "\\"));
                                                                                cpCore.privateFiles.copyFile(collectionPathFilename, (tmpInstallPath + collectionFilename));
                                                                                if ((collectionFilename.ToLower().Substring((collectionFilename.Length - 4)) == ".zip")) {
                                                                                    cpCore.privateFiles.UnzipFile((tmpInstallPath + collectionFilename));
                                                                                    cpCore.privateFiles.deleteFile((tmpInstallPath + collectionFilename));
                                                                                }
                                                                                
                                                                                // 
                                                                                //  install the individual files
                                                                                // 
                                                                                SrcFileNamelist = cpCore.privateFiles.getFileList(tmpInstallPath);
                                                                                if (true) {
                                                                                    // 
                                                                                    //  Process all non-zip files
                                                                                    // 
                                                                                    foreach (IO.FileInfo file in SrcFileNamelist) {
                                                                                        Filename = file.Name;
                                                                                        logController.appendInstallLog(cpCore, ("BuildLocalCollectionFolder, processing files, filename=[" 
                                                                                                        + (Filename + "]")));
                                                                                        if ((genericController.vbLCase(Filename.Substring((Filename.Length - 4))) == ".xml")) {
                                                                                            // 
                                                                                            logController.appendInstallLog(cpCore, ("BuildLocalCollectionFolder, processing xml file [" 
                                                                                                            + (Filename + "]")));
                                                                                            // hint = hint & ",320"
                                                                                            CollectionFile = new XmlDocument();
                                                                                            object loadOk = true;
                                                                                            try {
                                                                                                CollectionFile.LoadXml(cpCore.privateFiles.readFile((tmpInstallPath + Filename)));
                                                                                            }
                                                                                            catch (Exception ex) {
                                                                                                // 
                                                                                                //  There was a parse error in this xml file. Set the return message and the flag
                                                                                                //  If another xml files shows up, and process OK it will cover this error
                                                                                                // 
                                                                                                // hint = hint & ",330"
                                                                                                return_ErrorMessage = ("There was a problem installing the Collection File [" 
                                                                                                            + (tmpInstallPath 
                                                                                                            + (Filename + ("]. The error reported was [" 
                                                                                                            + (ex.Message + "].")))));
                                                                                                logController.appendInstallLog(cpCore, ("BuildLocalCollectionFolder, error reading collection [" 
                                                                                                                + (collectionPathFilename + "]")));
                                                                                                // StatusOK = False
                                                                                                loadOk = false;
                                                                                            }
                                                                                            
                                                                                            if (loadOk) {
                                                                                                // hint = hint & ",400"
                                                                                                CollectionFileBaseName = genericController.vbLCase(CollectionFile.DocumentElement.Name);
                                                                                                if (((CollectionFileBaseName != "contensivecdef") 
                                                                                                            && ((CollectionFileBaseName != CollectionFileRootNode) 
                                                                                                            && (CollectionFileBaseName != genericController.vbLCase(CollectionFileRootNodeOld))))) {
                                                                                                    // 
                                                                                                    //  Not a problem, this is just not a collection file
                                                                                                    // 
                                                                                                    logController.appendInstallLog(cpCore, ("BuildLocalCollectionFolder, xml base name wrong [" 
                                                                                                                    + (CollectionFileBaseName + "]")));
                                                                                                }
                                                                                                else {
                                                                                                    // 
                                                                                                    //  Collection File
                                                                                                    // 
                                                                                                    // hint = hint & ",420"
                                                                                                    // With...
                                                                                                    Collectionname = GetXMLAttribute(cpCore, IsFound, CollectionFile.DocumentElement, "name", "");
                                                                                                    if ((Collectionname == "")) {
                                                                                                        // 
                                                                                                        //  ----- Error condition -- it must have a collection name
                                                                                                        // 
                                                                                                        result = false;
                                                                                                        return_ErrorMessage = "<p>There was a problem with this Collection. The collection file does not have a collection name.</p>" +
                                                                                                        "";
                                                                                                        logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, collection has no name");
                                                                                                    }
                                                                                                    else {
                                                                                                        // 
                                                                                                        // ------------------------------------------------------------------
                                                                                                        //  Build Collection folder structure in /Add-ons folder
                                                                                                        // ------------------------------------------------------------------
                                                                                                        // 
                                                                                                        // hint = hint & ",440"
                                                                                                        CollectionFileFound = true;
                                                                                                        CollectionGuid = GetXMLAttribute(cpCore, IsFound, CollectionFile.DocumentElement, "guid", Collectionname);
                                                                                                        if ((CollectionGuid == "")) {
                                                                                                            // 
                                                                                                            //  I hope I do not regret this
                                                                                                            // 
                                                                                                            CollectionGuid = Collectionname;
                                                                                                        }
                                                                                                        
                                                                                                        CollectionVersionFolderName = GetCollectionPath(cpCore, CollectionGuid);
                                                                                                        if ((CollectionVersionFolderName != "")) {
                                                                                                            // 
                                                                                                            //  This is an upgrade
                                                                                                            // 
                                                                                                            // hint = hint & ",450"
                                                                                                            UpdatingCollection = true;
                                                                                                            Pos = genericController.vbInstr(1, CollectionVersionFolderName, "\\");
                                                                                                            if ((Pos > 0)) {
                                                                                                                CollectionFolderName = CollectionVersionFolderName.Substring(0, (Pos - 1));
                                                                                                            }
                                                                                                            
                                                                                                        }
                                                                                                        else {
                                                                                                            // 
                                                                                                            //  This is an install
                                                                                                            // 
                                                                                                            // hint = hint & ",460"
                                                                                                            CollectionFolderName = CollectionGuid;
                                                                                                            CollectionFolderName = genericController.vbReplace(CollectionFolderName, "{", "");
                                                                                                            CollectionFolderName = genericController.vbReplace(CollectionFolderName, "}", "");
                                                                                                            CollectionFolderName = genericController.vbReplace(CollectionFolderName, "-", "");
                                                                                                            CollectionFolderName = genericController.vbReplace(CollectionFolderName, " ", "");
                                                                                                            CollectionFolderName = (Collectionname + ("_" + CollectionFolderName));
                                                                                                        }
                                                                                                        
                                                                                                        CollectionFolder = (cpCore.addon.getPrivateFilesAddonPath() 
                                                                                                                    + (CollectionFolderName + "\\"));
                                                                                                        if (!cpCore.privateFiles.pathExists(CollectionFolder)) {
                                                                                                            // 
                                                                                                            //  Create collection folder
                                                                                                            // 
                                                                                                            // hint = hint & ",470"
                                                                                                            cpCore.privateFiles.createPath(CollectionFolder);
                                                                                                        }
                                                                                                        
                                                                                                        // 
                                                                                                        //  create a collection 'version' folder for these new files
                                                                                                        // 
                                                                                                        TimeStamp = "";
                                                                                                        NowTime = DateTime.Now();
                                                                                                        NowPart = NowTime.Year;
                                                                                                        NowPart.ToString();
                                                                                                        NowPart = NowTime.Month;
                                                                                                        if ((NowPart < 10)) {
                                                                                                            
                                                                                                        }
                                                                                                        
                                                                                                        "0";
                                                                                                        NowPart.ToString();
                                                                                                        NowPart = NowTime.Day;
                                                                                                        if ((NowPart < 10)) {
                                                                                                            
                                                                                                        }
                                                                                                        
                                                                                                        "0";
                                                                                                        NowPart.ToString();
                                                                                                        NowPart = NowTime.Hour;
                                                                                                        if ((NowPart < 10)) {
                                                                                                            
                                                                                                        }
                                                                                                        
                                                                                                        "0";
                                                                                                        NowPart.ToString();
                                                                                                        NowPart = NowTime.Minute;
                                                                                                        if ((NowPart < 10)) {
                                                                                                            
                                                                                                        }
                                                                                                        
                                                                                                        "0";
                                                                                                        NowPart.ToString();
                                                                                                        NowPart = NowTime.Second;
                                                                                                        if ((NowPart < 10)) {
                                                                                                            
                                                                                                        }
                                                                                                        
                                                                                                        "0";
                                                                                                        NowPart.ToString();
                                                                                                        CollectionVersionFolderName = (CollectionFolderName + ("\\" + TimeStamp));
                                                                                                        string CollectionVersionFolder = (cpCore.addon.getPrivateFilesAddonPath() + CollectionVersionFolderName);
                                                                                                        string CollectionVersionPath = (CollectionVersionFolder + "\\");
                                                                                                        cpCore.privateFiles.createPath(CollectionVersionPath);
                                                                                                        cpCore.privateFiles.copyFolder(tmpInstallPath, CollectionVersionFolder);
                                                                                                        // StatusOK = True
                                                                                                        // 
                                                                                                        //  Install activeX and search for importcollections
                                                                                                        // 
                                                                                                        // hint = hint & ",500"
                                                                                                        foreach (CDefSection in CollectionFile.DocumentElement.ChildNodes) {
                                                                                                            switch (genericController.vbLCase(CDefSection.Name)) {
                                                                                                                case "resource":
                                                                                                                    break;
                                                                                                                case "interfaces":
                                                                                                                    break;
                                                                                                                case "getcollection":
                                                                                                                case "importcollection":
                                                                                                                    ChildCollectionName = GetXMLAttribute(cpCore, Found, CDefSection, "name", "");
                                                                                                                    ChildCollectionGUID = GetXMLAttribute(cpCore, Found, CDefSection, "guid", CDefSection.InnerText);
                                                                                                                    if ((ChildCollectionGUID == "")) {
                                                                                                                        ChildCollectionGUID = CDefSection.InnerText;
                                                                                                                    }
                                                                                                                    
                                                                                                                    string statusMsg = ("Installing collection [" 
                                                                                                                                + (ChildCollectionName + (", " 
                                                                                                                                + (ChildCollectionGUID + ("] referenced from collection [" 
                                                                                                                                + (Collectionname + "]"))))));
                                                                                                                    logController.appendInstallLog(cpCore, ("BuildLocalCollectionFolder, getCollection or importcollection, childCollectionName [" 
                                                                                                                                    + (ChildCollectionName + ("], childCollectionGuid [" 
                                                                                                                                    + (ChildCollectionGUID + "]")))));
                                                                                                                    if ((genericController.vbInstr(1, CollectionVersionPath, ChildCollectionGUID, vbTextCompare) == 0)) {
                                                                                                                        if ((ChildCollectionGUID == "")) {
                                                                                                                            // 
                                                                                                                            //  -- Needs a GUID to install
                                                                                                                            result = false;
                                                                                                                            return_ErrorMessage = (statusMsg + ". The installation can not continue because an imported collection could not be downloaded because it" +
                                                                                                                            " does not include a valid GUID.");
                                                                                                                            logController.appendInstallLog(cpCore, ("BuildLocalCollectionFolder, return message [" 
                                                                                                                                            + (return_ErrorMessage + "]")));
                                                                                                                        }
                                                                                                                        else if ((GetCollectionPath(cpCore, ChildCollectionGUID) == "")) {
                                                                                                                            logController.appendInstallLog(cpCore, ("BuildLocalCollectionFolder, [" 
                                                                                                                                            + (ChildCollectionGUID + "], not found so needs to be installed")));
                                                                                                                            // 
                                                                                                                            //  If it is not already installed, download and install it also
                                                                                                                            // 
                                                                                                                            ChildWorkingPath = (CollectionVersionPath + ("\\" 
                                                                                                                                        + (ChildCollectionGUID + "\\")));
                                                                                                                            StatusOK = addonInstallClass.DownloadCollectionFiles(cpCore, ChildWorkingPath, ChildCollectionGUID, ChildCollectionLastChangeDate, return_ErrorMessage);
                                                                                                                            if (!StatusOK) {
                                                                                                                                logController.appendInstallLog(cpCore, ("BuildLocalCollectionFolder, [" 
                                                                                                                                                + (statusMsg + ("], downloadCollectionFiles returned error state, message [" 
                                                                                                                                                + (return_ErrorMessage + "]")))));
                                                                                                                                if ((return_ErrorMessage == "")) {
                                                                                                                                    return_ErrorMessage = (statusMsg + (". The installation can not continue because there was an unknown error while downloading the necessar" +
                                                                                                                                    "y collection file, [" 
                                                                                                                                                + (ChildCollectionGUID + "].")));
                                                                                                                                }
                                                                                                                                else {
                                                                                                                                    return_ErrorMessage = (statusMsg + (". The installation can not continue because there was an error while downloading the necessary collec" +
                                                                                                                                    "tion file, guid [" 
                                                                                                                                                + (ChildCollectionGUID + ("]. The error was [" 
                                                                                                                                                + (return_ErrorMessage + "]")))));
                                                                                                                                }
                                                                                                                                
                                                                                                                            }
                                                                                                                            else {
                                                                                                                                logController.appendInstallLog(cpCore, ("BuildLocalCollectionFolder, [" 
                                                                                                                                                + (ChildCollectionGUID + "], downloadCollectionFiles returned OK")));
                                                                                                                                // 
                                                                                                                                //  install the downloaded file
                                                                                                                                // 
                                                                                                                                List<string> ChildCollectionGUIDList = new List<string>();
                                                                                                                                StatusOK = BuildLocalCollectionReposFromFolder(cpCore, ChildWorkingPath, ChildCollectionLastChangeDate, ChildCollectionGUIDList, return_ErrorMessage, allowLogging);
                                                                                                                                if (!StatusOK) {
                                                                                                                                    logController.appendInstallLog(cpCore, ("BuildLocalCollectionFolder, [" 
                                                                                                                                                    + (statusMsg + ("], BuildLocalCollectionFolder returned error state, message [" 
                                                                                                                                                    + (return_ErrorMessage + "]")))));
                                                                                                                                    if ((return_ErrorMessage == "")) {
                                                                                                                                        return_ErrorMessage = (statusMsg + (". The installation can not continue because there was an unknown error installing the included collec" +
                                                                                                                                        "tion file, guid [" 
                                                                                                                                                    + (ChildCollectionGUID + "].")));
                                                                                                                                    }
                                                                                                                                    else {
                                                                                                                                        return_ErrorMessage = (statusMsg + (". The installation can not continue because there was an unknown error installing the included collec" +
                                                                                                                                        "tion file, guid [" 
                                                                                                                                                    + (ChildCollectionGUID + ("]. The error was [" 
                                                                                                                                                    + (return_ErrorMessage + "]")))));
                                                                                                                                    }
                                                                                                                                    
                                                                                                                                }
                                                                                                                                
                                                                                                                            }
                                                                                                                            
                                                                                                                            // 
                                                                                                                            //  -- remove child installation working folder
                                                                                                                            cpCore.privateFiles.DeleteFileFolder(ChildWorkingPath);
                                                                                                                        }
                                                                                                                        else {
                                                                                                                            // 
                                                                                                                            // 
                                                                                                                            // 
                                                                                                                            logController.appendInstallLog(cpCore, ("BuildLocalCollectionFolder, [" 
                                                                                                                                            + (ChildCollectionGUID + "], already installed")));
                                                                                                                        }
                                                                                                                        
                                                                                                                    }
                                                                                                                    
                                                                                                                    break;
                                                                                                            }
                                                                                                            if ((return_ErrorMessage != "")) {
                                                                                                                // 
                                                                                                                //  if error, no more nodes in this collection file
                                                                                                                // 
                                                                                                                result = false;
                                                                                                                break;
                                                                                                            }
                                                                                                            
                                                                                                        }
                                                                                                        
                                                                                                    }
                                                                                                    
                                                                                                }
                                                                                                
                                                                                            }
                                                                                            
                                                                                        }
                                                                                        
                                                                                        if ((return_ErrorMessage != "")) {
                                                                                            // 
                                                                                            //  if error, no more files
                                                                                            // 
                                                                                            result = false;
                                                                                            break;
                                                                                        }
                                                                                        
                                                                                    }
                                                                                    
                                                                                    if (((return_ErrorMessage == "") 
                                                                                                && !CollectionFileFound)) {
                                                                                        // 
                                                                                        //  no errors, but the collection file was not found
                                                                                        // 
                                                                                        if (ZipFileFound) {
                                                                                            // 
                                                                                            //  zip file found but did not qualify
                                                                                            // 
                                                                                            return_ErrorMessage = "<p>There was a problem with the installation. The collection zip file was downloaded, but it did not " +
                                                                                            "include a valid collection xml file.</p>";
                                                                                        }
                                                                                        else {
                                                                                            // 
                                                                                            //  zip file not found
                                                                                            // 
                                                                                            return_ErrorMessage = "<p>There was a problem with the installation. The collection zip was not downloaded successfully.</p>" +
                                                                                            "";
                                                                                        }
                                                                                        
                                                                                        // StatusOK = False
                                                                                    }
                                                                                    
                                                                                }
                                                                                
                                                                                // 
                                                                                //  delete the working folder
                                                                                // 
                                                                                cpCore.privateFiles.DeleteFileFolder(tmpInstallPath);
                                                                            }
                                                                            
                                                                            // 
                                                                            //  If the collection parsed correctly, update the Collections.xml file
                                                                            // 
                                                                            if ((return_ErrorMessage == "")) {
                                                                                UpdateConfig(cpCore, Collectionname, CollectionGuid, CollectionLastChangeDate, CollectionVersionFolderName);
                                                                            }
                                                                            else {
                                                                                // 
                                                                                //  there was an error processing the collection, be sure to save description in the log
                                                                                // 
                                                                                result = false;
                                                                                logController.appendInstallLog(cpCore, ("BuildLocalCollectionFolder, ERROR Exiting, ErrorMessage [" 
                                                                                                + (return_ErrorMessage + "]")));
                                                                            }
                                                                            
                                                                            // 
                                                                            logController.appendInstallLog(cpCore, ("BuildLocalCollectionFolder, Exiting with ErrorMessage [" 
                                                                                            + (return_ErrorMessage + "]")));
                                                                            // 
                                                                            BuildLocalCollectionRepoFromFile = (return_ErrorMessage == "");
                                                                            return_CollectionGUID = CollectionGuid;
                                                                        }
                                                                        catch (Exception ex) {
                                                                            cpCore.handleException(ex);
                                                                            throw;
                                                                        }
                                                                        
                                                                        return result;
                                                                    }
                                                                    
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
                    
                }
                
            }
            
        }
    }
}