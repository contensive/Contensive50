
using System;
using System.Xml;
using System.Collections.Generic;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.Processor.Models.Domain;
using System.Linq;
using static Contensive.BaseClasses.CPFileSystemBaseClass;
using Contensive.Processor.Exceptions;
using Contensive.BaseClasses;
using System.Reflection;
using NLog;
using Contensive.Models.Db;
using System.Globalization;
using System.Text;

namespace Contensive.Processor.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// install addon collections.
    /// </summary>
    public class CollectionInstallTemplateController {
        //
        //======================================================================================================
        //
        public static void installNode(CoreController core, XmlNode rootNode, int collectionId, ref bool return_UpgradeOK, ref string return_ErrorMessage, ref bool collectionIncludesDiagnosticAddons) {
            return_ErrorMessage = "";
            return_UpgradeOK = true;
            try {
                string Basename = toLCase(rootNode.Name);
                if (Basename == "template") {
                    bool IsFound = false;
                    string recordName = XmlController.getXMLAttribute(core, ref IsFound, rootNode, "name", "No Name");
                    if (string.IsNullOrEmpty(recordName)) {
                        recordName = "No Name";
                    }
                    string recordGuid = XmlController.getXMLAttribute(core, ref IsFound, rootNode, "guid", recordName);
                    if (string.IsNullOrEmpty(recordGuid)) {
                        recordGuid = recordName;
                    }
                    var record = DbBaseModel.create<PageTemplateModel>(core.cpParent, recordGuid);
                    if (record == null) {
                        record = DbBaseModel.createByUniqueName<PageTemplateModel>(core.cpParent, recordName);
                    }
                    if (record == null) {
                        record = DbBaseModel.addDefault<PageTemplateModel>(core.cpParent);
                    }
                    record.ccguid = recordGuid;
                    record.name = recordName;
                    record.bodyHTML = rootNode.InnerText;
                    record.collectionId = collectionId;
                    record.isSecure = XmlController.getXMLAttributeBoolean(core, ref IsFound, rootNode, "issecure", false);
                    record.save(core.cpParent);
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
        }
        //
    }
}
