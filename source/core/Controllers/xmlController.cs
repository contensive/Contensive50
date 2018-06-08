
using System;
using System.Reflection;
using System.Xml;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Contensive.Processor;
using Contensive.Processor.Models.DbModels;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.genericController;
using static Contensive.Processor.constants;
//
namespace Contensive.Processor.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// static class controller
    /// </summary>
    public class xmlController {
        //
        //====================================================================================================
        /// <summary>
        ///  Get an XML nodes attribute based on its name
        /// </summary>
        public static string GetXMLAttribute(coreController core, bool Found, XmlNode Node, string Name, string DefaultIfNotFound) {
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
                    UcaseName = genericController.vbUCase(Name);
                    foreach (XmlAttribute NodeAttribute in Node.Attributes) {
                        if (genericController.vbUCase(NodeAttribute.Name) == UcaseName) {
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
                logController.handleError( core,ex);
                throw;
            }
            return returnAttr;
        }
        //
        //====================================================================================================
        //
        public static double GetXMLAttributeNumber(coreController core, bool Found, XmlNode Node, string Name, string DefaultIfNotFound) {
            return encodeNumber(GetXMLAttribute(core, Found, Node, Name, DefaultIfNotFound));
        }
        //
        //====================================================================================================
        //
        public static bool GetXMLAttributeBoolean(coreController core, bool Found, XmlNode Node, string Name, bool DefaultIfNotFound) {
            return genericController.encodeBoolean(GetXMLAttribute(core, Found, Node, Name, encodeText(DefaultIfNotFound)));
        }
        //
        //====================================================================================================
        //
        public static int GetXMLAttributeInteger(coreController core, bool Found, XmlNode Node, string Name, int DefaultIfNotFound) {
            return genericController.encodeInteger(GetXMLAttribute(core, Found, Node, Name, DefaultIfNotFound.ToString()));
        }

    }
    //
}