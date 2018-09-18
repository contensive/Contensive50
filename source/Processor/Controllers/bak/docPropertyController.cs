


using Controllers;

namespace Controllers {
    
    // 
    // ====================================================================================================
    // '' <summary>
    // '' doc properties are properties limited in scope to this single hit, or viewing
    // '' </summary>
    public class docPropertyController {
        
        // 
        private coreClass cpCore;
        
        // 
        private Dictionary<string, docPropertiesClass> docPropertiesDict = new Dictionary<string, docPropertiesClass>();
        
        public docPropertyController(coreClass cpCore) {
            this.cpCore = cpCore;
        }
        
        // 
        // ====================================================================================================
        // 
        public void setProperty(string key, int value) {
            this.setProperty(key, value.ToString());
        }
        
        // 
        // ====================================================================================================
        // 
        public void setProperty(string key, DateTime value) {
            this.setProperty(key, value.ToString);
        }
        
        // 
        // ====================================================================================================
        // 
        public void setProperty(string key, bool value) {
            this.setProperty(key, value.ToString());
        }
        
        // 
        // ====================================================================================================
        // 
        public void setProperty(string key, string value) {
            this.setProperty(key, value, false);
        }
        
        // 
        // ====================================================================================================
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
                prop.NameValue = (key + ("=" + value));
                prop.Value = value;
                this.setProperty(key, prop);
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
        }
        
        // 
        // ====================================================================================================
        // 
        public void setProperty(string key, docPropertiesClass value) {
            string propKey = this.encodeDocPropertyKey(key);
            if (!string.IsNullOrEmpty(propKey)) {
                if (docPropertiesDict.ContainsKey(propKey)) {
                    docPropertiesDict.Remove(propKey);
                }
                
                docPropertiesDict.Add(propKey, value);
            }
            
        }
        
        // 
        // ====================================================================================================
        // 
        public bool containsKey(string RequestName) {
            return docPropertiesDict.ContainsKey(this.encodeDocPropertyKey(RequestName));
        }
        
        // 
        // ====================================================================================================
        // 
        public List<string> getKeyList() {
            List<string> keyList = new List<string>();
            foreach (KeyValuePair<string, docPropertiesClass> kvp in docPropertiesDict) {
                keyList.Add(kvp.Key);
            }
            
            return keyList;
        }
        
        // 
        // =============================================================================================
        // 
        public double getNumber(string RequestName) {
            try {
                return genericController.EncodeNumber(this.getProperty(RequestName).Value);
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return 0;
        }
        
        // 
        // =============================================================================================
        // 
        public int getInteger(string RequestName) {
            try {
                return genericController.EncodeInteger(this.getProperty(RequestName).Value);
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return 0;
        }
        
        // 
        // =============================================================================================
        // 
        public string getText(string RequestName) {
            try {
                return genericController.encodeText(this.getProperty(RequestName).Value);
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return String.Empty;
        }
        
        // 
        // =============================================================================================
        // 
        public string getRenderedActiveContent(string RequestName) {
            try {
                return cpCore.html.convertEditorResponseToActiveContent(genericController.encodeText(this.getProperty(RequestName).Value));
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return String.Empty;
        }
        
        // 
        // =============================================================================================
        // 
        public bool getBoolean(string RequestName) {
            try {
                return genericController.EncodeBoolean(this.getProperty(RequestName).Value);
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return false;
        }
        
        // 
        // =============================================================================================
        // 
        public DateTime getDate(string RequestName) {
            try {
                return genericController.EncodeDate(this.getProperty(RequestName).Value);
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return;
            MinValue;
        }
        
        // 
        // ====================================================================================================
        // 
        public docPropertiesClass getProperty(string RequestName) {
            try {
                string Key;
                // 
                Key = this.encodeDocPropertyKey(RequestName);
                if (!string.IsNullOrEmpty(Key)) {
                    if (docPropertiesDict.ContainsKey(Key)) {
                        return docPropertiesDict[Key];
                    }
                    
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return new docPropertiesClass();
        }
        
        // 
        // ====================================================================================================
        // 
        private string encodeDocPropertyKey(string sourceKey) {
            string returnResult = "";
            try {
                if (!string.IsNullOrEmpty(sourceKey)) {
                    returnResult = sourceKey.ToLower();
                    if (cpCore.webServer.requestSpaceAsUnderscore) {
                        returnResult = genericController.vbReplace(returnResult, " ", "_");
                    }
                    
                    if (cpCore.webServer.requestDotAsUnderscore) {
                        returnResult = genericController.vbReplace(returnResult, ".", "_");
                    }
                    
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return returnResult;
        }
        
        // 
        // 
        // 
        // ==========================================================================================
        // '' <summary>
        // '' add querystring to the doc properties
        // '' </summary>
        // '' <param name="QS"></param>
        public void addQueryString(string QS) {
            try {
                // 
                string[] ampSplit;
                int ampSplitCount;
                string[] ValuePair;
                string key;
                int Ptr;
                // 
                ampSplit = QS.Split("&");
                ampSplitCount = (UBound(ampSplit) + 1);
                for (Ptr = 0; (Ptr 
                            <= (ampSplitCount - 1)); Ptr++) {
                    string nameValuePair = ampSplit[Ptr];
                    docPropertiesClass docProperty = new docPropertiesClass();
                    // With...
                    if (!string.IsNullOrEmpty(nameValuePair)) {
                        if ((genericController.vbInstr(1, nameValuePair, "=") != 0)) {
                            ValuePair = nameValuePair.Split("=");
                            key = DecodeResponseVariable(ValuePair[0].ToString());
                            if ((key != "")) {
                                docProperty.Name = key;
                                if ((UBound(ValuePair) > 0)) {
                                    docProperty.Value = DecodeResponseVariable(ValuePair[1].ToString());
                                }
                                
                                false.IsFile = false;
                                docProperty.IsForm = false;
                                cpCore.docProperties.setProperty(key, docProperty);
                            }
                            
                        }
                        
                    }
                    
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
        }
        
        // 
        // ====================================================================================================
        // '' <summary>
        // '' return the docProperties collection as the legacy optionString
        // '' </summary>
        // '' <returns></returns>
        public string getLegacyOptionStringFromVar() {
            string returnString = "";
            try {
                foreach (string key in this.getKeyList()) {
                    // With...
                    ("" + ("&" 
                                + (genericController.encodeLegacyOptionStringArgument(key) + ("=" + encodeLegacyOptionStringArgument(this.getProperty(key).Value)))));
                }
                
            }
            catch (Exception ex) {
                throw ex;
            }
            
            return returnString;
        }
    }
}