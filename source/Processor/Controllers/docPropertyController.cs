
using System;
using System.Collections.Generic;
using static Contensive.Processor.Controllers.GenericController;

namespace Contensive.Processor.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// doc properties are properties limited in scope to this single hit, or viewing
    /// </summary>
    public class DocPropertyController {
        //
        public enum DocPropertyTypesEnum {
            serverVariable = 1,
            header = 2,
            form = 3,
            file = 4,
            queryString = 5,
            userDefined = 6
        }
        //
        private CoreController core;
        //
        private Dictionary<string, DocPropertiesClass> docPropertiesDict = new Dictionary<string, DocPropertiesClass>();
        //
        public DocPropertyController(CoreController core) : base() {
            this.core = core;
        }
        //
        //====================================================================================================
        //
        public void setProperty(string key, int value, DocPropertyTypesEnum propertyType) {
            setProperty(key, value.ToString(), propertyType);
        }
        //
        public void setProperty(string key, int value) {
            setProperty(key, value.ToString(), DocPropertyTypesEnum.userDefined);
        }
        //
        //====================================================================================================
        //
        public void setProperty(string key, double value, DocPropertyTypesEnum propertyType) {
            setProperty(key, value.ToString(), propertyType);
        }
        //
        public void setProperty(string key, double value) {
            setProperty(key, value.ToString(), DocPropertyTypesEnum.userDefined);
        }
        //
        //====================================================================================================
        //
        public void setProperty(string key, DateTime value, DocPropertyTypesEnum propertyType) {
            setProperty(key, value.ToString(), propertyType);
        }
        //
        public void setProperty(string key, DateTime value) {
            setProperty(key, value.ToString(), DocPropertyTypesEnum.userDefined);
        }
        //
        //====================================================================================================
        //
        public void setProperty(string key, bool value, DocPropertyTypesEnum propertyType) {
            setProperty(key, value.ToString(), propertyType);
        }
        //
        public void setProperty(string key, bool value) {
            setProperty(key, value.ToString(), DocPropertyTypesEnum.userDefined);
        }
        //
        //====================================================================================================
        //
        public void setProperty(string key, string value) {
            setProperty(key, value, DocPropertyTypesEnum.userDefined);
        }
        //
        //====================================================================================================
        //
        public void setProperty(string key, string value, DocPropertyTypesEnum propertyType) {
            try {
                DocPropertiesClass prop = new DocPropertiesClass {
                    NameValue = key,
                    FileSize = 0,
                    fileType = "",
                    Name = key,
                    propertyType = propertyType
                };
                prop.NameValue = key + "=" + value;
                prop.Value = value;
                setProperty(key, prop);
            } catch (Exception ex) {
                LogController.logError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public void setProperty(string key, DocPropertiesClass value) {
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
            foreach (KeyValuePair<string, DocPropertiesClass> kvp in docPropertiesDict) {
                keyList.Add(kvp.Key);
            }
            return keyList;
        }
        //
        //=============================================================================================
        //
        public double getNumber(string RequestName) {
            try {
                return GenericController.encodeNumber(getProperty(RequestName).Value);
            } catch (Exception ex) {
                LogController.logError( core,ex);
                throw;
            }
        }
        //
        //=============================================================================================
        //
        public int getInteger(string RequestName) {
            try {
                return GenericController.encodeInteger(getProperty(RequestName).Value);
            } catch (Exception ex) {
                LogController.logError( core,ex);
                throw;
            }
        }
        //
        //=============================================================================================
        //
        public string getText(string RequestName) {
            try {
                return GenericController.encodeText(getProperty(RequestName).Value);
            } catch (Exception ex) {
                LogController.logError( core,ex);
                throw;
            }
        }
        //
        //=============================================================================================
        //
        public string getRenderedActiveContent(string RequestName) {
            try {
                return ActiveContentController.processWysiwygResponseForSave(core, GenericController.encodeText(getProperty(RequestName).Value));
            } catch (Exception ex) {
                LogController.logError( core,ex);
                throw;
            }
        }
        //
        //=============================================================================================
        //
        public bool getBoolean(string RequestName) {
            try {
                return GenericController.encodeBoolean(getProperty(RequestName).Value);
            } catch (Exception ex) {
                LogController.logError( core,ex);
                throw;
            }
        }
        //
        //=============================================================================================
        //
        public DateTime getDate(string RequestName) {
            try {
                return GenericController.encodeDate(getProperty(RequestName).Value);
            } catch (Exception ex) {
                LogController.logError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public DocPropertiesClass getProperty(string RequestName) {
            try {
                string Key = encodeDocPropertyKey(RequestName);
                if (!string.IsNullOrEmpty(Key)) {
                    if (docPropertiesDict.ContainsKey(Key)) {
                        return docPropertiesDict[Key];
                    }
                }
            } catch (Exception ex) {
                LogController.logError( core,ex);
                throw;
            }
            return new DocPropertiesClass();
        }
        //
        //====================================================================================================
        //
        private string encodeDocPropertyKey(string sourceKey) {
            string returnResult = "";
            try {
                if (!string.IsNullOrEmpty(sourceKey)) {
                    returnResult = sourceKey.ToLowerInvariant();
                }
            } catch (Exception ex) {
                LogController.logError( core,ex);
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
                string[] ampSplit = QS.Split('&');
                for (int Ptr = 0; Ptr < ampSplit.GetUpperBound(0) + 1; Ptr++) {
                    string nameValuePair = ampSplit[Ptr];
                    if (!string.IsNullOrEmpty(nameValuePair)) {
                        if (GenericController.vbInstr(1, nameValuePair, "=") != 0) {
                            string[] ValuePair = nameValuePair.Split('=');
                            string key = decodeResponseVariable(encodeText(ValuePair[0]));
                            if (!string.IsNullOrEmpty(key)) {
                                DocPropertiesClass docProperty = new DocPropertiesClass {
                                    Name = key,
                                    propertyType = DocPropertyTypesEnum.queryString,
                                    Value = (ValuePair.GetUpperBound(0) > 0) ? decodeResponseVariable(encodeText(ValuePair[1])) : ""
                                };
                                core.docProperties.setProperty(key, docProperty);
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.logError(core,ex);
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
                    returnString += "&" + GenericController.encodeLegacyOptionStringArgument(key) + "=" + encodeLegacyOptionStringArgument(getProperty(key).Value);
                }
            } catch (Exception ex) {
                throw (ex);
            }
            return returnString;
        }
    }


}