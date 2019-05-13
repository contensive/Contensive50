
using Contensive.Processor.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using static Contensive.BaseClasses.CPFileSystemBaseClass;
//
namespace Contensive.Processor.Models.Domain {
    /// <summary>
    /// This model represents the folder where collections are stored, and dotnet assemblies are run for this application
    /// </summary>
    public class CollectionFolderModel {
        public string name { get; set; }
        public string guid { get; set; }
        public string path { get; set; }
        public DateTime lastChangeDate { get; set; }
        public DateTime installedDate { get; set; }

        //
        //====================================================================================================
        /// <summary>
        /// Return the collection path, lastChangeDate, and collectionName given the guid.
        /// if the collection is not found, returns null
        /// </summary>
        public static CollectionFolderModel getCollectionFolderConfig(CoreController core, string collectionGuid) {
            try {
                XmlDocument doc = new XmlDocument();
                try {
                    doc.LoadXml(getCollectionFolderConfigXml(core));
                } catch (Exception) {
                    LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", Error loading CollectionFolderConfig file.");
                    return null;
                }
                if (doc.DocumentElement.Name.ToLower() != GenericController.vbLCase(Constants.CollectionListRootNode)) {
                    LogController.logInfo(core, MethodInfo.GetCurrentMethod().Name + ", The Collections.xml file has an invalid root node");
                    return null;
                }
                foreach (XmlNode configNode in doc.DocumentElement.ChildNodes) {
                    if (configNode.Name.ToLowerInvariant().Equals("collection")) {
                        //
                        // -- set defaults
                        var result = new CollectionFolderModel {
                            name = string.Empty,
                            guid = string.Empty,
                            path = string.Empty,
                            installedDate = DateTime.Now,
                            lastChangeDate = DateTime.Now
                        };
                        foreach (XmlNode collectionNode in configNode.ChildNodes) {
                            switch (GenericController.vbLCase(collectionNode.Name)) {
                                case "name":
                                    result.name = collectionNode.InnerText;
                                    break;
                                case "guid":
                                    result.guid = collectionNode.InnerText;
                                    break;
                                case "path":
                                    result.path = collectionNode.InnerText;
                                    break;
                                case "lastchangedate":
                                    result.lastChangeDate = GenericController.encodeDate(collectionNode.InnerText);
                                    break;
                                case "installeddate":
                                    result.installedDate = GenericController.encodeDate(collectionNode.InnerText);
                                    break;
                            }
                        }
                        //
                        // -- if this is the collection requested, exit now
                        if (collectionGuid.ToLowerInvariant().Equals(result.guid.ToLowerInvariant())) { return result; }
                    }
                }
                return null;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return the collectionList file stored in the root of the addon folder.
        /// </summary>
        /// <returns></returns>
        public static string getCollectionFolderConfigXml(CoreController core) {
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
                    //
                    LogController.logInfo(core, "Collection Folder XML is blank, rebuild start");
                    //                     
                    List<FolderDetail> FolderList = core.privateFiles.getFolderList(core.addon.getPrivateFilesAddonPath());
                    //
                    LogController.logInfo(core, "Collection Folder XML rebuild, FolderList.count [" + FolderList.Count + "]");
                    //                     
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
                                    if (SubFolderList.Count > 0) {
                                        FolderDetail lastSubFolder = SubFolderList.Last<FolderDetail>();
                                        FolderName = FolderName + "\\" + lastSubFolder.Name;
                                        LastChangeDate = lastSubFolder.Name.Substring(4, 2) + "/" + lastSubFolder.Name.Substring(6, 2) + "/" + lastSubFolder.Name.Left(4);
                                        if (!GenericController.IsDate(LastChangeDate)) {
                                            LastChangeDate = "";
                                        }
                                    }
                                    returnXml += "\r\n\t<Collection>";
                                    returnXml += "\r\n\t\t<name>" + Collectionname + "</name>";
                                    returnXml += "\r\n\t\t<guid>" + CollectionGuid + "</guid>";
                                    returnXml += "\r\n\t\t<lastchangedate>" + LastChangeDate + "</lastchangedate>";
                                    returnXml += "\r\n\t\t<installeddate>" + folder.DateCreated.ToShortDateString() + "</installeddate>";
                                    returnXml += "\r\n\t\t<path>" + FolderName + "</path>";
                                    returnXml += "\r\n\t</Collection>";
                                }
                            }
                        }
                    }
                    returnXml = "<CollectionList>" + returnXml + "\r\n</CollectionList>";
                    core.privateFiles.saveFile(collectionFilePathFilename, returnXml);
                    //
                    LogController.logInfo(core, "Collection Folder XML is blank, rebuild finished and saved");
                    //                     
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnXml;
        }

        //
        //====================================================================================================
        /// <summary>
        /// get a list of collections available on the server
        /// </summary>
        public static bool getCollectionFolderConfigCollectionList(CoreController core, ref List<CollectionLibraryModel> localCollectionStoreList, ref string return_ErrorMessage) {
            bool returnOk = true;
            try {
                //
                //-----------------------------------------------------------------------------------------------
                //   Load LocalCollections from the Collections.xml file
                //-----------------------------------------------------------------------------------------------
                //
                string localCollectionStoreListXml = getCollectionFolderConfigXml(core);
                if (!string.IsNullOrEmpty(localCollectionStoreListXml)) {
                    XmlDocument LocalCollections = new XmlDocument();
                    try {
                        LocalCollections.LoadXml(localCollectionStoreListXml);
                    } catch (Exception) {
                        string Copy = "Error loading privateFiles\\addons\\Collections.xml";
                        LogController.logInfo(core, Copy);
                        return_ErrorMessage += "<P>" + Copy + "</P>";
                        returnOk = false;
                    }
                    if (returnOk) {
                        if (GenericController.vbLCase(LocalCollections.DocumentElement.Name) != GenericController.vbLCase(Constants.CollectionListRootNode)) {
                            string Copy = "The addons\\Collections.xml has an invalid root node, [" + LocalCollections.DocumentElement.Name + "] was received and [" + Constants.CollectionListRootNode + "] was expected.";
                            LogController.logInfo(core, Copy);
                            return_ErrorMessage += "<P>" + Copy + "</P>";
                            returnOk = false;
                        } else {
                            //
                            // Get a list of the collection guids on this server
                            //
                            if (GenericController.vbLCase(LocalCollections.DocumentElement.Name) == "collectionlist") {
                                foreach (XmlNode LocalListNode in LocalCollections.DocumentElement.ChildNodes) {
                                    switch (GenericController.vbLCase(LocalListNode.Name)) {
                                        case "collection":
                                            var collection = new CollectionLibraryModel();
                                            localCollectionStoreList.Add(collection);
                                            foreach (XmlNode CollectionNode in LocalListNode.ChildNodes) {
                                                if (CollectionNode.Name.ToLowerInvariant() == "name") {
                                                    collection.name = CollectionNode.InnerText;
                                                } else if (CollectionNode.Name.ToLowerInvariant() == "guid") {
                                                    collection.guid = CollectionNode.InnerText;
                                                } else if (CollectionNode.Name.ToLowerInvariant() == "path") {
                                                    collection.path = CollectionNode.InnerText;
                                                } else if (CollectionNode.Name.ToLowerInvariant() == "lastchangedate") {
                                                    collection.lastChangeDate = GenericController.encodeDate(CollectionNode.InnerText);
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
                LogController.handleError(core, ex);
                throw;
            }
            return returnOk;
        }
    }
}