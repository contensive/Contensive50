
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Core;
using Contensive.Core.Models.DbModels;
using Contensive.Core.Controllers;
using Contensive.Core.Models.Context;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
//
namespace Contensive.Core.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// Site Properties
    /// </summary>
    public class sitePropertiesController {
        //
        private coreController core;
        //
        //====================================================================================================
        /// <summary>
        /// new
        /// </summary>
        /// <param name="core"></param>
        public sitePropertiesController(coreController core) : base() {
            this.core = core;
        }
        public int defaultRouteId {
            get {
                return getInteger(spDefaultRouteAddonId);
            }
            set {
                setProperty(spDefaultRouteAddonId, value);
            }
        }
        //
        //====================================================================================================
        //
        private bool dbNotReady {
            get {
                return (core.appConfig.appStatus != appConfigModel.appStatusEnum.OK);
            }
        }
        //
        //====================================================================================================
        //
        private int integerPropertyBase(string propertyName, int defaultValue, ref int? localStore) {
            if (dbNotReady) {
                //
                // -- Db not available yet, return default
                return defaultValue;
            } else if (localStore == null) {
                //
                // -- load local store 
                localStore = getInteger(propertyName, defaultValue);
            }
            return encodeInteger(localStore);
        }
        //
        //====================================================================================================
        //
        private bool booleanPropertyBase(string propertyName, bool defaultValue, ref bool? localStore) {
            if (dbNotReady) {
                //
                // -- Db not available yet, return default
                return defaultValue;
            } else if (localStore == null) {
                //
                // -- load local store 
                localStore = getBoolean(propertyName, defaultValue);
            }
            return encodeBoolean(localStore);
        }
        //
        //====================================================================================================
        //
        private string textPropertyBase(string propertyName, string defaultValue, ref string localStore) {
            if (dbNotReady) {
                //
                // -- Db not available yet, return default
                return defaultValue;
            } else if (localStore == null) {
                //
                // -- load local store 
                localStore = getText(propertyName, defaultValue);
            }
            return localStore;
        }
        //
        //====================================================================================================
        //
        internal int landingPageID {
            get {
                return integerPropertyBase("LandingPageID", 0, ref _landingPageID);
            }
        }
        private int? _landingPageID = null;
        //
        //====================================================================================================
        //
        internal bool trackGuestsWithoutCookies {
            get {
                return booleanPropertyBase("track guests without cookies", false, ref _trackGuestsWithoutCookies);
            }
        }
        private bool? _trackGuestsWithoutCookies = null;
        //
        //====================================================================================================
        //
        internal bool AllowAutoLogin {
            get {
                return booleanPropertyBase("allowAutoLogin", false, ref _AllowAutoLogin);
            }
        }
        private bool? _AllowAutoLogin = null;
        //
        //====================================================================================================
        //
        internal int maxVisitLoginAttempts {
            get {
                return integerPropertyBase("maxVisitLoginAttempts", 20, ref _maxVisitLoginAttempts);
            }
        }
        private int? _maxVisitLoginAttempts = null;
        //
        //====================================================================================================
        //
        public string LoginIconFilename {
            get {
                return textPropertyBase("LoginIconFilename", "/ccLib/images/ccLibLogin.GIF", ref _LoginIconFilename);
            }
        }
        private string _LoginIconFilename = null;
        //
        //====================================================================================================
        //
        public bool allowVisitTracking {
            get {
                return booleanPropertyBase("allowVisitTracking", true, ref _allowVisitTracking);
            }
        }
        private bool? _allowVisitTracking;
        //
        //====================================================================================================
        //
        public bool allowTransactionLog {
            get {
                return booleanPropertyBase("UseContentWatchLink", false, ref _allowTransactionLog);
            }
        }
        private bool? _allowTransactionLog = null;
        //
        //====================================================================================================
        /// <summary>
        /// trap errors (hide errors) - when true, errors will be logged and code resumes next. When false, errors are re-thrown
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool trapErrors {
            get {
                return booleanPropertyBase("TrapErrors", true, ref _trapErrors);
            }
        }
        private bool? _trapErrors = null;
        //
        //====================================================================================================
        //
        public string serverPageDefault {
            get {
                return textPropertyBase(siteproperty_serverPageDefault_name, siteproperty_serverPageDefault_defaultValue, ref _ServerPageDefault_local);
            }
        }
        private string _ServerPageDefault_local = null;
        //
        //====================================================================================================
        //
        internal int defaultWrapperID {
            get {
                return integerPropertyBase("DefaultWrapperID", 0, ref _defaultWrapperID);
            }
        }
        private int? _defaultWrapperID = null;
        //
        //====================================================================================================
        /// <summary>
        /// allowLinkAlias
        /// </summary>
        /// <returns></returns>
        internal bool allowLinkAlias {
            get {
                return booleanPropertyBase("allowLinkAlias", true, ref _allowLinkAlias_Local);
            }
        }
        private bool? _allowLinkAlias_Local = null;
        //
        //====================================================================================================
        //
        internal int childListAddonID {
            get {
                try {
                    if (dbNotReady) {
                        //
                        // -- db not ready, return 0
                        return 0;
                    } else {
                        if (_childListAddonID == null) {
                            _childListAddonID = getInteger("ChildListAddonID", 0);
                            if (_childListAddonID == 0) {
                                int CS = core.db.csOpen(cnAddons, "ccguid='" + addonGuidChildList + "'", "", true, 0, false, false, "ID");
                                if (core.db.csOk(CS)) {
                                    _childListAddonID = core.db.csGetInteger(CS, "ID");
                                }
                                core.db.csClose(ref CS);
                                if (_childListAddonID == 0) {
                                    CS = core.db.csInsertRecord(cnAddons);
                                    if (core.db.csOk(CS)) {
                                        _childListAddonID = core.db.csGetInteger(CS, "ID");
                                        core.db.csSet(CS, "name", "Child Page List");
                                        core.db.csSet(CS, "ArgumentList", "Name");
                                        core.db.csSet(CS, "CopyText", "<ac type=\"childlist\" name=\"$name$\">");
                                        core.db.csSet(CS, "Content", "1");
                                        core.db.csSet(CS, "StylesFilename", "");
                                        core.db.csSet(CS, "ccguid", addonGuidChildList);
                                    }
                                    core.db.csClose(ref CS);
                                }
                                setProperty("ChildListAddonID", encodeText(_childListAddonID));
                            }
                        }
                    }
                } catch (Exception ex) {
                    core.handleException(ex);
                    throw;
                }
                return encodeInteger(_childListAddonID);
            }
        }
        private int? _childListAddonID = null;
        //
        //====================================================================================================
        //
        public string docTypeDeclaration {
            get {
                return textPropertyBase("DocTypeDeclaration", DTDDefault, ref _docTypeDeclaration);
            }
        }
        private string _docTypeDeclaration = null;
        //
        //====================================================================================================
        //
        public bool useContentWatchLink {
            get {
                return booleanPropertyBase("UseContentWatchLink", false, ref _useContentWatchLink);
            }
        }
        private bool? _useContentWatchLink = null;
        //
        //====================================================================================================
        //
        public bool allowTestPointLogging {
            get {
                return booleanPropertyBase("AllowTestPointLogging", false, ref _allowTestPointLogging);
            }
        }
        private bool? _allowTestPointLogging = null;
        //
        //====================================================================================================
        //
        public int defaultFormInputWidth {
            get {
                return integerPropertyBase("DefaultFormInputWidth", 60, ref _defaultFormInputWidth);
            }
        }
        private int? _defaultFormInputWidth = null;
        //
        //====================================================================================================
        //
        public int selectFieldWidthLimit {
            get {
                return integerPropertyBase("SelectFieldWidthLimit", 200, ref _selectFieldWidthLimit);
            }
        }
        private int? _selectFieldWidthLimit = null;
        //
        //====================================================================================================
        //
        public int selectFieldLimit {
            get {
                return integerPropertyBase("SelectFieldLimit", 1000, ref _selectFieldLimit);
            }
        }
        private int? _selectFieldLimit = null;
        //
        //====================================================================================================
        //
        public int defaultFormInputTextHeight {
            get {
                return integerPropertyBase("DefaultFormInputTextHeight", 1, ref _defaultFormInputTextHeight);
            }
        }
        private int? _defaultFormInputTextHeight = null;
        //
        //====================================================================================================
        //
        public string emailAdmin {
            get {
                return textPropertyBase("EmailAdmin", "webmaster@" + core.webServer.requestDomain, ref _emailAdmin);
            }
        }
        private string _emailAdmin = null;
        //
        //========================================================================
        /// <summary>
        /// Set a site property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="Value"></param>
        public void setProperty(string propertyName, string Value) {
            try {
                if (dbNotReady) {
                    //
                    // -- cannot set property
                    throw new ApplicationException("Cannot set site property before Db is ready.");
                } else {
                    if (!string.IsNullOrEmpty(propertyName.Trim())) {
                        if (propertyName.ToLower().Equals("adminurl")) {
                            //
                            // -- intercept adminUrl for compatibility, always use admin route instead
                        } else {
                            //
                            // -- set value in Db
                            string SQLNow = core.db.encodeSQLDate(DateTime.Now);
                            string SQL = "UPDATE ccSetup Set FieldValue=" + core.db.encodeSQLText(Value) + ",ModifiedDate=" + SQLNow + " WHERE name=" + core.db.encodeSQLText(propertyName);
                            int recordsAffected = 0;
                            core.db.executeNonQuery(SQL, "", ref recordsAffected);
                            if (recordsAffected == 0) {
                                SQL = "INSERT INTO ccSetup (ACTIVE,CONTENTCONTROLID,NAME,FIELDVALUE,ModifiedDate,DateAdded)VALUES("
                            + SQLTrue + "," + core.db.encodeSQLNumber(Models.Complex.cdefModel.getContentId(core, "site properties")) + "," + core.db.encodeSQLText(propertyName.ToUpper()) + "," + core.db.encodeSQLText(Value) + "," + SQLNow + "," + SQLNow + ");";
                                core.db.executeQuery(SQL);
                            }
                            //
                            // -- set simple lazy cache
                            string cacheName = "siteproperty" + propertyName.Trim().ToLower();
                            if (nameValueDict.ContainsKey(cacheName)) {
                                nameValueDict.Remove(cacheName);
                            }
                            nameValueDict.Add(cacheName, Value);
                            //
                            // -- set cache, no memory cache not used, instead load all into local cache on load
                            //core.cache.setObject(cacheName, Value)
                        }
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// Set a site property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="Value"></param>
        public void setProperty(string propertyName, bool Value) {
            if (Value) {
                setProperty(propertyName, "true");
            } else {
                setProperty(propertyName, "false");
            }
        }
        //
        //========================================================================
        /// <summary>
        /// Set a site property from an integer
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="Value"></param>
        public void setProperty(string propertyName, int Value) {
            setProperty(propertyName, Value.ToString());
        }
        //
        //========================================================================
        /// <summary>
        /// Set a site property from a date 
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="Value"></param>
        public void setProperty(string propertyName, DateTime Value) {
            setProperty(propertyName, Value.ToString());
        }
        //
        //========================================================================
        /// <summary>
        /// get site property without a cache check, return as text. If not found, set and return default value
        /// </summary>
        /// <param name="PropertyName"></param>
        /// <param name="DefaultValue"></param>
        /// <param name="memberId"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public string getTextFromDb(string PropertyName, string DefaultValue, ref bool return_propertyFound) {
            string returnString = "";
            try {
                using (DataTable dt = core.db.executeQuery("select FieldValue from ccSetup where name=" + core.db.encodeSQLText(PropertyName) + " order by id")) {
                    if (dt.Rows.Count > 0) {
                        returnString = genericController.encodeText(dt.Rows[0]["FieldValue"]);
                        return_propertyFound = true;
                    } else if (!string.IsNullOrEmpty(DefaultValue)) {
                        // do not set - set may have to save, and save needs contentId, which now loads ondemand, which checks cache, which does a getSiteProperty.
                        setProperty(PropertyName, DefaultValue);
                        returnString = DefaultValue;
                        return_propertyFound = true;
                    } else {
                        returnString = "";
                        return_propertyFound = false;
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return returnString;
        }
        //
        //========================================================================
        /// <summary>
        /// get site property, return as text. If not found, set and return default value
        /// </summary>
        /// <param name="PropertyName"></param>
        /// <param name="DefaultValue"></param>
        /// <param name="memberId"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public string getText(string PropertyName, string DefaultValue) {
            string returnString = "";
            try {
                if (dbNotReady) {
                    //
                    // -- if not ready, return default 
                    returnString = DefaultValue;
                } else {
                    if (PropertyName.ToLower().Equals("adminurl")) {
                        returnString = "/" + core.appConfig.adminRoute;
                    } else {
                        string cacheName = "siteproperty" + PropertyName.Trim().ToLower();
                        if (string.IsNullOrEmpty(PropertyName.Trim())) {
                            //
                            // -- bad property name 
                            returnString = DefaultValue;
                        } else {
                            //
                            // -- test simple lazy cache to keep from reading the same property mulitple times on one doc
                            if (nameValueDict.ContainsKey(cacheName)) {
                                //
                                // -- property in memory cache
                                returnString = nameValueDict[cacheName];
                            } else {
                                //
                                // -- read property from cache, no, with preloaded local cache, this will never be used
                                if (false) {
                                    //Dim returnObj As Object = core.cache.getObject(Of String)(cacheName)
                                    //If (returnObj IsNot Nothing) Then
                                    //
                                    // -- found in cache, save in simple cache and return
                                    //returnString = encodeText(returnObj)
                                    //nameValueDict.Add(cacheName, returnString)
                                } else {
                                    //
                                    // -- not found in cache, read property from Db
                                    bool propertyFound = false;
                                    returnString = getTextFromDb(PropertyName, DefaultValue, ref propertyFound);
                                    if (propertyFound) {
                                        //
                                        // -- found in Db, already saved in local cache, memory cache not used
                                        // nameValueDict.Add(cacheName, returnString)
                                        //core.cache.setObject(cacheName, returnString)
                                    } else {
                                        //
                                        // -- property not found in db, if default is not blank, write it and set cache
                                        returnString = DefaultValue;
                                        nameValueDict.Add(cacheName, returnString);
                                        if (!string.IsNullOrEmpty(returnString)) {
                                            setProperty(cacheName, DefaultValue);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return returnString;
        }
        //
        //========================================================================
        /// <summary>
        /// get site property and return string
        /// </summary>
        /// <param name="PropertyName"></param>
        /// <returns></returns>
        public string getText(string PropertyName) {
            return getText(PropertyName, string.Empty);
        }
        //
        //========================================================================
        /// <summary>
        /// get site property and return integer
        /// </summary>
        /// <param name="PropertyName"></param>
        /// <param name="DefaultValue"></param>
        /// <returns></returns>
        public int getInteger(string PropertyName, int DefaultValue = 0) {
            return genericController.encodeInteger(getText(PropertyName, DefaultValue.ToString()));
        }
        //
        //========================================================================
        /// <summary>
        /// get site property and return double
        /// </summary>
        /// <param name="PropertyName"></param>
        /// <param name="DefaultValue"></param>
        /// <returns></returns>
        public double getNumber(string PropertyName, double DefaultValue = 0) {
            return genericController.encodeNumber(getText(PropertyName, DefaultValue.ToString()));
        }
        //
        //========================================================================
        /// <summary>
        /// get site property and return boolean
        /// </summary>
        /// <param name="PropertyName"></param>
        /// <param name="DefaultValue"></param>
        /// <returns></returns>
        public bool getBoolean(string PropertyName, bool DefaultValue = false) {
            return genericController.encodeBoolean(getText(PropertyName, DefaultValue.ToString()));
        }
        //
        //========================================================================
        /// <summary>
        /// get a site property as a date 
        /// </summary>
        /// <param name="PropertyName"></param>
        /// <param name="DefaultValue"></param>
        /// <returns></returns>
        public DateTime getDate(string PropertyName, DateTime DefaultValue = default(DateTime)) {
            return genericController.encodeDate(getText(PropertyName, DefaultValue.ToString()));
        }
        //
        //====================================================================================================
        /// <summary>
        /// allowCache site property, not cached (to make it available to the cache process)
        /// </summary>
        /// <returns></returns>
        public bool allowCache_notCached {
            get {
                if (dbNotReady) {
                    return false;
                } else {
                    if (_allowCache_notCached == null) {
                        bool propertyFound = false;
                        _allowCache_notCached = genericController.encodeBoolean(getTextFromDb("AllowBake", "0", ref propertyFound));
                    }
                    return encodeBoolean(_allowCache_notCached);
                }
            }
        }
        private bool? _allowCache_notCached = null;
        //
        //====================================================================================================
        /// <summary>
        /// The code version used to update the database last
        /// </summary>
        /// <returns></returns>
        public string dataBuildVersion {
            get {
                return textPropertyBase("BuildVersion", "", ref _buildVersion);
                //Dim returnString = ""
                //Try
                //    If Not _dataBuildVersion_Loaded Then
                //        _dataBuildVersion = getText("BuildVersion", "")
                //        If _dataBuildVersion = "" Then
                //            _dataBuildVersion = "0.0.000"
                //        End If
                //        _dataBuildVersion_Loaded = True
                //    End If
                //    returnString = _dataBuildVersion
                //Catch ex As Exception
                //    core.handleException(ex); : Throw
                //End Try
                //Return returnString
            }
            set {
                setProperty("BuildVersion", value);
                _buildVersion = null;
            }
        }
        private string _buildVersion = null;
        //
        //====================================================================================================
        //
        internal Dictionary<string, string> nameValueDict {
            get {
                if (dbNotReady) {
                    throw new ApplicationException("Cannot access site property collection if database is not ready.");
                } else {
                    if (_nameValueDict == null) {
                        _nameValueDict = new Dictionary<string, string>();
                        csController cs = new csController(core);
                        if (cs.openSQL("select name,FieldValue from ccsetup where (active>0) order by id")) {
                            do {
                                string name = cs.getText("name").Trim().ToLower();
                                if (!string.IsNullOrEmpty(name)) {
                                    if (!_nameValueDict.ContainsKey(name)) {
                                        _nameValueDict.Add(name, cs.getText("FieldValue"));
                                    }
                                }
                                cs.goNext();
                            } while (cs.ok());
                        }
                        cs.Close();
                    }
                }
                return _nameValueDict;
            }
        }
        private Dictionary<string, string> _nameValueDict = null;
    }
}