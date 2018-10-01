
using System;
using System.Reflection;
using System.Xml;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Contensive.Processor;
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.constants;
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
        public static string GetXMLAttribute(CoreController core, bool Found, XmlNode Node, string Name, string DefaultIfNotFound) {
            string returnAttr = "";
            try {
                //todo  NOTE: Commented this declaration since looping variables in 'foreach' loops are declared in the 'foreach' header in C#:
                //				XmlAttribute NodeAttribute = null;
                XmlNode ResultNode = null;
                string UcaseName = null;
                //
                Found = false;
                ResultNode = Node.Attributes.GetNamedItem(Name);
                if (ResultNode == null) {
                    UcaseName = GenericController.vbUCase(Name);
                    foreach (XmlAttribute NodeAttribute in Node.Attributes) {
                        if (GenericController.vbUCase(NodeAttribute.Name) == UcaseName) {
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
                LogController.handleError( core,ex);
                throw;
            }
            return returnAttr;
        }
        //
        //====================================================================================================
        //
        public static double GetXMLAttributeNumber(CoreController core, bool Found, XmlNode Node, string Name, string DefaultIfNotFound) {
            return encodeNumber(GetXMLAttribute(core, Found, Node, Name, DefaultIfNotFound));
        }
        //
        //====================================================================================================
        //
        public static bool GetXMLAttributeBoolean(CoreController core, bool Found, XmlNode Node, string Name, bool DefaultIfNotFound) {
            return GenericController.encodeBoolean(GetXMLAttribute(core, Found, Node, Name, encodeText(DefaultIfNotFound)));
        }
        //
        //====================================================================================================
        //
        public static int GetXMLAttributeInteger(CoreController core, bool Found, XmlNode Node, string Name, int DefaultIfNotFound) {
            return GenericController.encodeInteger(GetXMLAttribute(core, Found, Node, Name, DefaultIfNotFound.ToString()));
        }

    }
    //
}