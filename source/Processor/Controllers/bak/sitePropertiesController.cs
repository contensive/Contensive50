


using Controllers;

namespace Controllers {
    
    // 
    // ====================================================================================================
    // '' <summary>
    // '' Site Properties
    // '' </summary>
    public class sitePropertiesController {
        
        // 
        private coreClass cpCore;
        
        // 
        // ====================================================================================================
        // '' <summary>
        // '' new
        // '' </summary>
        // '' <param name="cpCore"></param>
        public sitePropertiesController(coreClass cpCore) {
            this.cpCore = cpCore;
        }
        
        // 
        // ====================================================================================================
        // 
        private bool dbNotReady {
            get {
                return (cpCore.serverConfig.appConfig.appStatus != Models.Entity.serverConfigModel.appStatusEnum.OK);
            }
        }
        
        private int integerPropertyBase(string propertyName, int defaultValue, ref int localStore, void Question) {
            if (dbNotReady) {
                // 
                //  -- Db not available yet, return default
                return defaultValue;
            }
            else if ((localStore == null)) {
                // 
                //  -- load local store 
                localStore = this.getinteger(propertyName, defaultValue);
            }
            
            return int.Parse(localStore);
        }
        
        // 
        // ====================================================================================================
        // 
        private bool booleanPropertyBase(string propertyName, bool defaultValue, ref bool localStore, void Question) {
            if (dbNotReady) {
                // 
                //  -- Db not available yet, return default
                return defaultValue;
            }
            else if ((localStore == null)) {
                // 
                //  -- load local store 
                localStore = this.getBoolean(propertyName, defaultValue);
            }
            
            return bool.Parse(localStore);
        }
        
        // 
        // ====================================================================================================
        // 
        private string textPropertyBase(string propertyName, string defaultValue, ref string localStore) {
            if (dbNotReady) {
                // 
                //  -- Db not available yet, return default
                return defaultValue;
            }
            else if ((localStore == null)) {
                // 
                //  -- load local store 
                localStore = this.getText(propertyName, defaultValue);
            }
            
            return localStore;
        }
        
        // 
        // ====================================================================================================
        // 
        internal int landingPageID {
            get {
                return this.integerPropertyBase("LandingPageID", 0, _landingPageID);
            }
        }
        
        private int _landingPageID;
        
        // 
        // ====================================================================================================
        // 
        internal bool trackGuestsWithoutCookies {
            get {
                return this.booleanPropertyBase("track guests without cookies", false, _trackGuestsWithoutCookies);
            }
        }
        
        private bool _trackGuestsWithoutCookies;
        
        // 
        // ====================================================================================================
        // 
        internal bool AllowAutoLogin {
            get {
                return this.booleanPropertyBase("allowAutoLogin", false, _AllowAutoLogin);
            }
        }
        
        private bool _AllowAutoLogin;
        
        // 
        // ====================================================================================================
        // 
        internal int maxVisitLoginAttempts {
            get {
                return this.integerPropertyBase("maxVisitLoginAttempts", 20, _maxVisitLoginAttempts);
            }
        }
        
        private int _maxVisitLoginAttempts;
        
        // 
        // ====================================================================================================
        // 
        public string LoginIconFilename {
            get {
                return this.textPropertyBase("LoginIconFilename", "/ccLib/images/ccLibLogin.GIF", _LoginIconFilename);
            }
        }
        
        private string _LoginIconFilename = null;
        
        public bool allowVisitTracking {
            get {
                return this.booleanPropertyBase("allowVisitTracking", true, _allowVisitTracking);
            }
        }
        
        private bool _allowVisitTracking;
        
        // 
        // ====================================================================================================
        // 
        public bool allowTransactionLog {
            get {
                return this.booleanPropertyBase("UseContentWatchLink", false, _allowTransactionLog);
            }
        }
        
        private bool _allowTransactionLog;
        
        // 
        // ====================================================================================================
        // '' <summary>
        // '' trap errors (hide errors) - when true, errors will be logged and code resumes next. When false, errors are re-thrown
        // '' </summary>
        // '' <value></value>
        // '' <returns></returns>
        // '' <remarks></remarks>
        public bool trapErrors {
            get {
                return this.booleanPropertyBase("TrapErrors", true, _trapErrors);
            }
        }
        
        // 
        // ====================================================================================================
        // 
        public string serverPageDefault {
            get {
                return this.textPropertyBase(siteproperty_serverPageDefault_name, siteproperty_serverPageDefault_defaultValue, _ServerPageDefault_local);
            }
        }
        
        private string _ServerPageDefault_local = null;
        
        internal int defaultWrapperID {
            get {
                return this.integerPropertyBase("DefaultWrapperID", 0, _defaultWrapperID);
            }
        }
        
        private int _defaultWrapperID;
        
        // 
        // ====================================================================================================
        // '' <summary>
        // '' allowLinkAlias
        // '' </summary>
        // '' <returns></returns>
        internal bool allowLinkAlias {
            get {
                return this.booleanPropertyBase("allowLinkAlias", true, _allowLinkAlias_Local);
            }
        }
        
        private bool _allowLinkAlias_Local;
        
        // 
        // ====================================================================================================
        // 
        internal int childListAddonID {
            get {
                try {
                    if (dbNotReady) {
                        // 
                        //  -- db not ready, return 0
                        return 0;
                    }
                    else if ((_childListAddonID == null)) {
                        _childListAddonID = this.getinteger("ChildListAddonID", 0);
                        if ((_childListAddonID == 0)) {
                            int CS = cpCore.db.csOpen(cnAddons, ("ccguid=\'" 
                                            + (addonGuidChildList + "\'")), ,, ,, ,, "ID");
                            if (cpCore.db.csOk(CS)) {
                                _childListAddonID = cpCore.db.csGetInteger(CS, "ID");
                            }
                            
                            cpCore.db.csClose(CS);
                            if ((_childListAddonID == 0)) {
                                CS = cpCore.db.csInsertRecord(cnAddons);
                                if (cpCore.db.csOk(CS)) {
                                    _childListAddonID = cpCore.db.csGetInteger(CS, "ID");
                                    cpCore.db.csSet(CS, "name", "Child Page List");
                                    cpCore.db.csSet(CS, "ArgumentList", "Name");
                                    cpCore.db.csSet(CS, "CopyText", "<ac type=\"childlist\" name=\"$name$\">");
                                    cpCore.db.csSet(CS, "Content", "1");
                                    cpCore.db.csSet(CS, "StylesFilename", "");
                                    cpCore.db.csSet(CS, "ccguid", addonGuidChildList);
                                }
                                
                                cpCore.db.csClose(CS);
                            }
                            
                            this.setProperty("ChildListAddonID", _childListAddonID.ToString());
                        }
                        
                    }
                    
                }
                catch (Exception ex) {
                    cpCore.handleException(ex);
                    throw;
                }
                
                return int.Parse(_childListAddonID);
            }
        }
        
        private int _childListAddonID;
        
        // 
        // ====================================================================================================
        // 
        public string docTypeDeclaration {
            get {
                return this.textPropertyBase("DocTypeDeclaration", DTDDefault, _docTypeDeclaration);
            }
        }
        
        private string _docTypeDeclaration = null;
        
        public bool useContentWatchLink {
            get {
                return this.booleanPropertyBase("UseContentWatchLink", false, _useContentWatchLink);
            }
        }
        
        private bool _useContentWatchLink;
        
        // 
        // ====================================================================================================
        // 
        public bool allowTestPointLogging {
            get {
                return this.booleanPropertyBase("AllowTestPointLogging", false, _allowTestPointLogging);
            }
        }
        
        private bool _allowTestPointLogging;
        
        // 
        // ====================================================================================================
        // 
        public int defaultFormInputWidth {
            get {
                return this.integerPropertyBase("DefaultFormInputWidth", 60, _defaultFormInputWidth);
            }
        }
        
        private int _defaultFormInputWidth;
        
        // 
        // ====================================================================================================
        // 
        public int selectFieldWidthLimit {
            get {
                return this.integerPropertyBase("SelectFieldWidthLimit", 200, _selectFieldWidthLimit);
            }
        }
        
        private int _selectFieldWidthLimit;
        
        // 
        // ====================================================================================================
        // 
        public int selectFieldLimit {
            get {
                return this.integerPropertyBase("SelectFieldLimit", 1000, _selectFieldLimit);
            }
        }
        
        private int _selectFieldLimit;
        
        // 
        // ====================================================================================================
        // 
        public int defaultFormInputTextHeight {
            get {
                return this.integerPropertyBase("DefaultFormInputTextHeight", 1, _defaultFormInputTextHeight);
            }
        }
        
        private int _defaultFormInputTextHeight;
        
        // 
        // ====================================================================================================
        // 
        public string emailAdmin {
            get {
                return this.textPropertyBase("EmailAdmin", ("webmaster@" + cpCore.webServer.requestDomain), _emailAdmin);
            }
        }
        
        private string _emailAdmin = null;
        
        public void setProperty(string propertyName, string Value) {
            try {
                if (dbNotReady) {
                    // 
                    //  -- cannot set property
                    throw new ApplicationException("Cannot set site property before Db is ready.");
                }
                else if (!string.IsNullOrEmpty(propertyName.Trim())) {
                    if (propertyName.ToLower.Equals("adminurl")) {
                        // 
                        //  -- intercept adminUrl for compatibility, always use admin route instead
                    }
                    else {
                        // 
                        //  -- set value in Db
                        string SQLNow = cpCore.db.encodeSQLDate(Now);
                        string SQL = ("UPDATE ccSetup Set FieldValue=" 
                                    + (cpCore.db.encodeSQLText(Value) + (",ModifiedDate=" 
                                    + (SQLNow + (" WHERE name=" + cpCore.db.encodeSQLText(propertyName))))));
                        int recordsAffected = 0;
                        cpCore.db.executeNonQuery(SQL, ,, recordsAffected);
                        if ((recordsAffected == 0)) {
                            SQL = ("INSERT INTO ccSetup (ACTIVE,CONTENTCONTROLID,NAME,FIELDVALUE,ModifiedDate,DateAdded)VALUES(" 
                                        + (SQLTrue + ("," 
                                        + (cpCore.db.encodeSQLNumber(models.complex.cdefmodel.getcontentid(cpcore, "site properties")) + ("," 
                                        + (cpCore.db.encodeSQLText(propertyName.ToUpper()) + ("," 
                                        + (cpCore.db.encodeSQLText(Value) + ("," 
                                        + (SQLNow + ("," 
                                        + (SQLNow + ");"))))))))))));
                            cpCore.db.executeQuery(SQL);
                        }
                        
                        // 
                        //  -- set simple lazy cache
                        string cacheName = ("siteproperty" + propertyName.Trim().ToLower());
                        if (nameValueDict.ContainsKey(cacheName)) {
                            nameValueDict.Remove(cacheName);
                        }
                        
                        nameValueDict.Add(cacheName, Value);
                        // 
                        //  -- set cache, no memory cache not used, instead load all into local cache on load
                        // cpCore.cache.setObject(cacheName, Value)
                    }
                    
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
        }
        
        // 
        // ========================================================================
        // '' <summary>
        // '' Set a site property
        // '' </summary>
        // '' <param name="propertyName"></param>
        // '' <param name="Value"></param>
        public void setProperty(string propertyName, bool Value) {
            if (Value) {
                this.setProperty(propertyName, "true");
            }
            else {
                this.setProperty(propertyName, "false");
            }
            
        }
        
        // 
        // ========================================================================
        // '' <summary>
        // '' Set a site property
        // '' </summary>
        // '' <param name="propertyName"></param>
        // '' <param name="Value"></param>
        public void setProperty(string propertyName, int Value) {
            this.setProperty(propertyName, Value.ToString);
        }
        
        // 
        // ========================================================================
        // '' <summary>
        // '' get site property without a cache check, return as text. If not found, set and return default value
        // '' </summary>
        // '' <param name="PropertyName"></param>
        // '' <param name="DefaultValue"></param>
        // '' <param name="memberId"></param>
        // '' <returns></returns>
        // '' <remarks></remarks>
        public string getTextFromDb(string PropertyName, string DefaultValue, ref bool return_propertyFound) {
            string returnString = "";
            try {
                Using;
                ((DataTable)(dt)) = cpCore.db.executeQuery(("select FieldValue from ccSetup where name=" 
                                + (cpCore.db.encodeSQLText(PropertyName) + " order by id")));
                if ((dt.Rows.Count > 0)) {
                    returnString = genericController.encodeText(dt.Rows[0].Item["FieldValue"]);
                    return_propertyFound = true;
                }
                else if ((DefaultValue != "")) {
                    //  do not set - set may have to save, and save needs contentId, which now loads ondemand, which checks cache, which does a getSiteProperty.
                    this.setProperty(PropertyName, DefaultValue);
                    returnString = DefaultValue;
                    return_propertyFound = true;
                }
                else {
                    returnString = "";
                    return_propertyFound = false;
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return returnString;
        }
        
        // 
        // ========================================================================
        // '' <summary>
        // '' get site property, return as text. If not found, set and return default value
        // '' </summary>
        // '' <param name="PropertyName"></param>
        // '' <param name="DefaultValue"></param>
        // '' <param name="memberId"></param>
        // '' <returns></returns>
        // '' <remarks></remarks>
        public string getText(string PropertyName, string DefaultValue) {
            string returnString = "";
            try {
                if (dbNotReady) {
                    // 
                    //  -- if not ready, return default 
                    returnString = DefaultValue;
                }
                else if (PropertyName.ToLower.Equals("adminurl")) {
                    returnString = ("/" + cpCore.serverConfig.appConfig.adminRoute);
                }
                else {
                    string cacheName = ("siteproperty" + PropertyName.Trim().ToLower());
                    if (string.IsNullOrEmpty(PropertyName.Trim())) {
                        // 
                        //  -- bad property name 
                        returnString = DefaultValue;
                    }
                    else {
                        // 
                        //  -- test simple lazy cache to keep from reading the same property mulitple times on one doc
                        if (nameValueDict.ContainsKey(cacheName)) {
                            // 
                            //  -- property in memory cache
                            returnString = nameValueDict(cacheName);
                        }
                        else {
                            // 
                            //  -- read property from cache, no, with preloaded local cache, this will never be used
                            if (false) {
                                // Dim returnObj As Object = cpCore.cache.getObject(Of String)(cacheName)
                                // If (returnObj IsNot Nothing) Then
                                // '
                                // ' -- found in cache, save in simple cache and return
                                // returnString = encodeText(returnObj)
                                // nameValueDict.Add(cacheName, returnString)
                            }
                            else {
                                // 
                                //  -- not found in cache, read property from Db
                                bool propertyFound = false;
                                returnString = this.getTextFromDb(PropertyName, DefaultValue, propertyFound);
                                if (propertyFound) {
                                    // 
                                    //  -- found in Db, already saved in local cache, memory cache not used
                                    //  nameValueDict.Add(cacheName, returnString)
                                    // cpCore.cache.setObject(cacheName, returnString)
                                }
                                else {
                                    // 
                                    //  -- property not found in db, if default is not blank, write it and set cache
                                    returnString = DefaultValue;
                                    nameValueDict.Add(cacheName, returnString);
                                    if ((returnString != "")) {
                                        this.setProperty(cacheName, DefaultValue);
                                    }
                                    
                                }
                                
                            }
                            
                        }
                        
                    }
                    
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return returnString;
        }
        
        // 
        // ========================================================================
        // '' <summary>
        // '' get site property and return string
        // '' </summary>
        // '' <param name="PropertyName"></param>
        // '' <returns></returns>
        public string getText(string PropertyName) {
            return this.getText(PropertyName, String.Empty);
        }
        
        // 
        // ========================================================================
        // '' <summary>
        // '' get site property and return integer
        // '' </summary>
        // '' <param name="PropertyName"></param>
        // '' <param name="DefaultValue"></param>
        // '' <returns></returns>
        public int getinteger(string PropertyName, int DefaultValue, void =, void 0) {
            return genericController.EncodeInteger(this.getText(PropertyName, DefaultValue.ToString));
            // Warning!!! Optional parameters not supported
        }
        
        // 
        // ========================================================================
        // '' <summary>
        // '' get site property and return boolean
        // '' </summary>
        // '' <param name="PropertyName"></param>
        // '' <param name="DefaultValue"></param>
        // '' <returns></returns>
        public bool getBoolean(string PropertyName, bool DefaultValue, void =, void False) {
            return genericController.EncodeBoolean(this.getText(PropertyName, DefaultValue.ToString));
            // Warning!!! Optional parameters not supported
        }
        
        // 
        // ========================================================================
        // '' <summary>
        // '' get a site property as a date 
        // '' </summary>
        // '' <param name="PropertyName"></param>
        // '' <param name="DefaultValue"></param>
        // '' <returns></returns>
        public DateTime getDate(string PropertyName, DateTime DefaultValue, void =, void Nothing) {
            return genericController.EncodeDate(this.getText(PropertyName, DefaultValue.ToString));
            // Warning!!! Optional parameters not supported
        }
        
        // 
        // ====================================================================================================
        // '' <summary>
        // '' allowCache site property, not cached (to make it available to the cache process)
        // '' </summary>
        // '' <returns></returns>
        public bool allowCache_notCached {
            get {
                if (dbNotReady) {
                    return false;
                }
                else {
                    if ((_allowCache_notCached == null)) {
                        bool propertyFound = false;
                        _allowCache_notCached = genericController.EncodeBoolean(this.getTextFromDb("AllowBake", "0", propertyFound));
                    }
                    
                    return bool.Parse(_allowCache_notCached);
                }
                
            }
        }
        
        private bool _allowCache_notCached;
        
        // 
        // ====================================================================================================
        // '' <summary>
        // '' The code version used to update the database last
        // '' </summary>
        // '' <returns></returns>
        public string dataBuildVersion {
            get {
                return this.textPropertyBase("BuildVersion", "", _buildVersion);
                // Dim returnString = ""
                // Try
                //     If Not _dataBuildVersion_Loaded Then
                //         _dataBuildVersion = getText("BuildVersion", "")
                //         If _dataBuildVersion = "" Then
                //             _dataBuildVersion = "0.0.000"
                //         End If
                //         _dataBuildVersion_Loaded = True
                //     End If
                //     returnString = _dataBuildVersion
                // Catch ex As Exception
                //     cpCore.handleException(ex) : Throw
                // End Try
                // Return returnString
            }
            set {
                this.setProperty("BuildVersion", value);
                _buildVersion = null;
            }
        }
        
        private string _buildVersion = null;
        
        internal Dictionary<string, string> nameValueDict {
            get {
                if (dbNotReady) {
                    throw new ApplicationException("Cannot access site property collection if database is not ready.");
                }
                else if ((_nameValueDict == null)) {
                    _nameValueDict = new Dictionary<string, string>();
                    csController cs = new csController(cpCore);
                    if (cs.openSQL("select name,FieldValue from ccsetup where (active>0) order by id")) {
                        for (
                        ; cs.ok(); 
                        ) {
                            string name = cs.getText("name").Trim().ToLower();
                            if (!string.IsNullOrEmpty(name)) {
                                if (!_nameValueDict.ContainsKey(name)) {
                                    _nameValueDict.Add(name, cs.getText("FieldValue"));
                                }
                                
                            }
                            
                            cs.goNext();
                        }
                        
                    }
                    
                    cs.Close();
                }
                
                return _nameValueDict;
            }
        }
        
        private Dictionary<string, string> _nameValueDict = null;
    }
}