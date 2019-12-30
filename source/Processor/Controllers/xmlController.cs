
using System;
using System.Xml;
using static Contensive.Processor.Controllers.GenericController;
//
namespace Contensive.Processor.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// static class controller
    /// </summary>
    public class XmlController {
        //
        //====================================================================================================
        /// <summary>
        ///  Get an XML nodes attribute based on its name
        /// </summary>
        public static string getXMLAttribute(CoreController core, ref bool Found, XmlNode Node, string Name, string DefaultIfNotFound) {
            string returnAttr = "";
            try {
                XmlNode ResultNode = null;
                string UcaseName = null;
                //
                Found = false;
                ResultNode = Node.Attributes.GetNamedItem(Name);
                if (ResultNode == null) {
                    UcaseName = GenericController.toUCase(Name);
                    foreach (XmlAttribute NodeAttribute in Node.Attributes) {
                        if (GenericController.toUCase(NodeAttribute.Name) == UcaseName) {
                            returnAttr = NodeAttribute.Value;
                            Found = true;
                            break;
                        }
                    }
                    if (!Found) {
                        returnAttr = DefaultIfNotFound;
                    }
                } else {
                    returnAttr = ResultNode.Value;
                    Found = true;
                }
            } catch (Exception ex) {
                LogController.logError( core,ex);
                throw;
            }
            return returnAttr;
        }
        //
        //====================================================================================================
        //
        public static double getXMLAttributeNumber(CoreController core, bool Found, XmlNode Node, string Name, string DefaultIfNotFound) {
            return encodeNumber(getXMLAttribute(core, ref Found, Node, Name, DefaultIfNotFound));
        }
        //
        //====================================================================================================
        //
        public static bool getXMLAttributeBoolean(CoreController core, bool Found, XmlNode Node, string Name, bool DefaultIfNotFound) {
            return GenericController.encodeBoolean(getXMLAttribute(core, ref Found, Node, Name, encodeText(DefaultIfNotFound)));
        }
        //
        //====================================================================================================
        //
        public static int getXMLAttributeInteger(CoreController core, bool Found, XmlNode Node, string Name, int DefaultIfNotFound) {
            return GenericController.encodeInteger(getXMLAttribute(core, ref Found, Node, Name, DefaultIfNotFound.ToString()));
        }

    }
    //
}