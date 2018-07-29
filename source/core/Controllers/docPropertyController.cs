
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
    /// doc properties are properties limited in scope to this single hit, or viewing
    /// </summary>
    public class docPropertyController {
        //
        private CoreController core;
        //
        private Dictionary<string, docPropertiesClass> docPropertiesDict = new Dictionary<string, docPropertiesClass>();
        //
        public docPropertyController(CoreController core) : base() {
            this.core = core;
        }
        //
        //====================================================================================================
        //
        public void setProperty(string key, int value) {
            setProperty(key, value.ToString());
        }
        //
        //====================================================================================================
        //
        public void setProperty(string key, DateTime value) {
            setProperty(key, value.ToString());
        }
        //
        //====================================================================================================
        //
        public void setProperty(string key, bool value) {
            setProperty(key, value.ToString());
        }
        //
        //====================================================================================================
        //
        public void setProperty(string key, string value) {
            setProperty(key, value, false);
        }
        //
        //====================================================================================================
        //
        public void setProperty(string key, string value, bool isForm) {
            try {
                docPropertiesClass prop = new docPropertiesClass();
                prop.NameValue = key;
                prop.FileSize = 0;
                prop.fileType = "";
                prop.IsFile = false;
                prop.IsForm = isForm;
                prop.Name = key;
                prop.NameValue = key + "=" + value;
                prop.Value = value;
                setProperty(key, prop);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public void setProperty(string key, docPropertiesClass value) {
            string propKey = encodeDocPropertyKey(key);
            if (!string.IsNullOrEmpty(propKey)) {
                if (docPropertiesDict.ContainsKey(propKey)) {
                    docPropertiesDict.Remove(propKey);
                }
                docPropertiesDict.Add(propKey, value);
            }
        }
        //
        //====================================================================================================
        //
        public bool containsKey(string RequestName) {
            return docPropertiesDict.ContainsKey(encodeDocPropertyKey(RequestName));
        }
        //
        //====================================================================================================
        //
        public List<string> getKeyList() {
            List<string> keyList = new List<string>();
            foreach (KeyValuePair<string, docPropertiesClass> kvp in docPropertiesDict) {
                keyList.Add(kvp.Key);
            }
            return keyList;
        }
        //
        //=============================================================================================
        //
        public double getNumber(string RequestName) {
            try {
                return genericController.encodeNumber(getProperty(RequestName).Value);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //=============================================================================================
        //
        public int getInteger(string RequestName) {
            try {
                return genericController.encodeInteger(getProperty(RequestName).Value);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //=============================================================================================
        //
        public string getText(string RequestName) {
            try {
                return genericController.encodeText(getProperty(RequestName).Value);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //=============================================================================================
        //
        public string getRenderedActiveContent(string RequestName) {
            try {
                return activeContentController.processWysiwygResponseForSave(core, genericController.encodeText(getProperty(RequestName).Value));
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //=============================================================================================
        //
        public bool getBoolean(string RequestName) {
            try {
                return genericController.encodeBoolean(getProperty(RequestName).Value);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //=============================================================================================
        //
        public DateTime getDate(string RequestName) {
            try {
                return genericController.encodeDate(getProperty(RequestName).Value);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public docPropertiesClass getProperty(string RequestName) {
            try {
                string Key = encodeDocPropertyKey(RequestName);
                if (!string.IsNullOrEmpty(Key)) {
                    if (docPropertiesDict.ContainsKey(Key)) {
                        return docPropertiesDict[Key];
                    }
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
            return new docPropertiesClass();
        }
        //
        //====================================================================================================
        //
        private string encodeDocPropertyKey(string sourceKey) {
            string returnResult = "";
            try {
                if (!string.IsNullOrEmpty(sourceKey)) {
                    returnResult = sourceKey.ToLower();
                    if (core.webServer.requestSpaceAsUnderscore) {
                        returnResult = genericController.vbReplace(returnResult, " ", "_");
                    }
                    if (core.webServer.requestDotAsUnderscore) {
                        returnResult = genericController.vbReplace(returnResult, ".", "_");
                    }
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
            return returnResult;
        }
        //
        //
        //
        //==========================================================================================
        /// <summary>
        /// add querystring to the doc properties
        /// </summary>
        /// <param name="QS"></param>
        public void addQueryString(string QS) {
            try {
                //
                string[] ampSplit = null;
                int ampSplitCount = 0;
                string[] ValuePair = null;
                string key = null;
                int Ptr = 0;
                //
                ampSplit = QS.Split('&');
                ampSplitCount = ampSplit.GetUpperBound(0) + 1;
                for (Ptr = 0; Ptr < ampSplitCount; Ptr++) {
                    string nameValuePair = ampSplit[Ptr];
                    docPropertiesClass docProperty = new docPropertiesClass();
                    if (!string.IsNullOrEmpty(nameValuePair)) {
                        if (genericController.vbInstr(1, nameValuePair, "=") != 0) {
                            ValuePair = nameValuePair.Split('=');
                            key = decodeResponseVariable(encodeText(ValuePair[0]));
                            if (!string.IsNullOrEmpty(key)) {
                                docProperty.Name = key;
                                if (ValuePair.GetUpperBound(0) > 0) {
                                    docProperty.Value = decodeResponseVariable(encodeText(ValuePair[1]));
                                }
                                docProperty.IsForm = false;
                                docProperty.IsFile = false;
                                //core.webServer.readStreamJSForm = core.webServer.readStreamJSForm Or (UCase(.Name) = genericController.vbUCase(RequestNameJSForm))
                                core.docProperties.setProperty(key, docProperty);
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// return the docProperties collection as the legacy optionString
        /// </summary>
        /// <returns></returns>
        public string getLegacyOptionStringFromVar() {
            string returnString = "";
            try {
                foreach (string key in getKeyList()) {
                    returnString += "&" + genericController.encodeLegacyOptionStringArgument(key) + "=" + encodeLegacyOptionStringArgument(getProperty(key).Value);
                }
            } catch (Exception ex) {
                throw (ex);
            }
            return returnString;
        }
    }


}