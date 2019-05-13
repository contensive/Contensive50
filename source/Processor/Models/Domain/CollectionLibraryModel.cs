
using Contensive.Processor.Controllers;
using System;
using System.Collections.Generic;
using System.Xml;
//
namespace Contensive.Processor.Models.Domain {
    //
    //======================================================================================================
    /// <summary>
    /// data from the collection library (remote server that distributes collections)
    /// </summary>
    public class CollectionLibraryModel {
        public string name;
        public string guid;
        public string path;
        public DateTime lastChangeDate;
        public string version;
        public string description;
        public string contensiveVersion;

        //
        //====================================================================================================
        /// <summary>
        /// return a list of collections on the 
        /// </summary>
        public static List<CollectionLibraryModel> getCollectionLibraryList(CoreController core) {
            var result = new List<CollectionLibraryModel>();
            try {
                var LibCollections = new XmlDocument();
                try {
                    LibCollections.Load("http://support.contensive.com/GetCollectionList?iv=" + core.codeVersion() + "&includeSystem=1&includeNonPublic=1");
                } catch (Exception) {
                    string UserError = "There was an error reading the Collection Library. The site may be unavailable.";
                    LogController.logInfo(core, UserError);
                    ErrorController.addUserError(core, UserError);
                    return null;
                }
                if (GenericController.vbLCase(LibCollections.DocumentElement.Name) != GenericController.vbLCase(Constants.CollectionListRootNode)) {
                    string UserError = "There was an error reading the Collection Library file. The '" + Constants.CollectionListRootNode + "' element was not found.";
                    LogController.logInfo(core, UserError);
                    ErrorController.addUserError(core, UserError);
                    return null;
                }
                foreach (XmlNode collectionNode in LibCollections.DocumentElement.ChildNodes) {
                    if (collectionNode.Name.ToLower().Equals("collection")) {
                        //
                        // Read the collection
                        var collection = new CollectionLibraryModel();
                        result.Add(collection);
                        foreach (XmlNode CollectionNode in collectionNode.ChildNodes) {
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
                    }
                }
            } catch (Exception) {
                throw;
            }
            return result;
        }
    }
}