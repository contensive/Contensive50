
//using static Contensive.Core.coreCommonModule;

//using Microsoft.VisualBasic;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Data;
//using System.Diagnostics;

//namespace Contensive.Core
//{
//    //
//    //====================================================================================================
//    /// <summary>
//    /// classSummary
//    /// </summary>
//    public class coreUserClass
//    {
//        //
//        private cpCoreClass cpCore;
//        //
//        //====================================================================================================
//        /// <summary>
//        /// id for the user. When this property is set, all public user. properteries are updated for this selected id
//        /// </summary>
//        /// <returns></returns>
//        public int id
//        {
//            get { return _id; }
//            set
//            {
//                if ((_id != value))
//                {
//                    _id = initializeUser(value);
//                }
//            }
//        }
//        private int _id = 0;
//        //
//        // simple shared properties, derived from the userId when .id set (through initilizeUser method)
//        //
//        //
//        internal string name = "";
//        //
//        internal bool isAdmin = false;
//        //
//        internal bool isDeveloper = false;
//        // The members Organization
//        internal int organizationId = 0;
//        //
//        internal int languageId = 0;
//        //
//        internal string language = "";
//        // stored in visit record - Is this the first visit for this member
//        internal bool isNew = false;
//        //
//        internal string email = "";
//        //
//        // Allow bulk mail
//        internal bool allowBulkEmail = false;
//        //
//        internal bool allowToolsPanel = false;
//        // if true, and setup AllowAutoLogin then use cookie to login
//        internal bool autoLogin = false;
//        //
//        internal int adminMenuModeID = 0;
//        //
//        // depricated - true only during the page that the join was completed - use for redirections and GroupAdds
//        internal bool userAdded = false;
//        internal string username = "";
//        internal string password = "";
//        internal int contentControlID = 0;
//        //
//        // if not empty, add to head
//        internal string styleFilename = "";
//        // if true, future visits will be marked exclude from analytics
//        internal bool excludeFromAnalytics = false;
//        //
//        public string main_IsEditingContentList = "";
//        public string main_IsNotEditingContentList = "";
//        //
//        //-----------------------------------------------------------------------
//        // ----- Member Private
//        //-----------------------------------------------------------------------
//        //
//        //
//        public bool active = false;
//        //
//        public int visits = 0;
//        // The last visit by the Member (the beginning of this visit
//        public System.DateTime lastVisit = System.DateTime.MinValue;
//        //
//        public string company = "";
//        public string user_Title = "";
//        public string main_MemberAddress = "";
//        public string main_MemberCity = "";
//        public string main_MemberState = "";
//        public string main_MemberZip = "";
//        public string main_MemberCountry = "";
//        //
//        public string main_MemberPhone = "";
//        public string main_MemberFax = "";
//        //
//        //-----------------------------------------------------------------------
//        // ----- Member Commerce properties
//        //-----------------------------------------------------------------------
//        //
//        // Billing Address for purchases
//        public string billEmail = "";
//        //
//        public string billPhone = "";
//        //
//        public string billFax = "";
//        //
//        public string billCompany = "";
//        //
//        public string billAddress = "";
//        //
//        public string billCity = "";
//        //
//        public string billState = "";
//        //
//        public string billZip = "";
//        //
//        public string billCountry = "";
//        //
//        // Mailing Address
//        public string shipName = "";
//        //
//        public string shipCompany = "";
//        //
//        public string shipAddress = "";
//        //
//        public string shipCity = "";
//        //
//        public string shipState = "";
//        //
//        public string shipZip = "";
//        //
//        public string shipCountry = "";
//        //
//        public string shipPhone = "";
//        //
//        //----------------------------------------------------------------------------------------------------
//        //
//        // Values entered with the login form
//        public string loginForm_Username = "";
//        //   =
//        public string loginForm_Password = "";
//        //   =
//        public string loginForm_Email = "";
//        //   =
//        public bool loginForm_AutoLogin = false;
//        //
//        //
//        public const int main_maxVisitLoginAttempts = 20;
//        // prevent main_ProcessLoginFormDefault from running twice (multiple user messages, popups, etc.)
//        public bool main_loginFormDefaultProcessed = false;
//        //
//        //------------------------------------------------------------------------
//        // ----- local cache to speed up user.main_IsContentManager
//        //------------------------------------------------------------------------
//        //
//        // If ContentId in this list, they are not a content manager
//        private string contentAccessRights_NotList = "";
//        // If ContentId in this list, they are a content manager
//        private string contentAccessRights_List = "";
//        // If in _List, test this for allowAdd
//        private string contentAccessRights_AllowAddList = "";
//        // If in _List, test this for allowDelete
//        private string contentAccessRights_AllowDeleteList = "";
//        //
//        //========================================================================
//        /// <summary>
//        /// is Guest
//        /// </summary>
//        /// <returns></returns>
//        public bool isGuest()
//        {
//            return !isAuthenticatedMember();
//        }
//        //
//        //========================================================================
//        /// <summary>
//        /// Is Recognized (not new and not authenticted)
//        /// </summary>
//        /// <returns></returns>
//        public bool isRecognized()
//        {
//            return !isNew;
//        }
//        //
//        //========================================================================
//        /// <summary>
//        /// authenticated
//        /// </summary>
//        /// <returns></returns>
//        public bool isAuthenticated()
//        {
//            return cpCore.visit_isAuthenticated;
//        }
//        //
//        //========================================================================
//        /// <summary>
//        /// true if editing any content
//        /// </summary>
//        /// <returns></returns>
//        public bool isEditingAnything()
//        {
//            return isEditing("");
//        }
//        //
//        //========================================================================
//        /// <summary>
//        /// True if editing a specific content
//        /// </summary>
//        /// <param name="ContentNameOrId"></param>
//        /// <returns></returns>
//        public bool isEditing(string ContentNameOrId)
//        {
//            bool returnResult = false;
//            try
//            {
//                if (true)
//                {
//                    string localContentNameOrId = null;
//                    string cacheTestName = null;
//                    //
//                    if (!cpCore.visit_initialized)
//                    {
//                        cpCore.testPoint("...visit not initialized");
//                    }
//                    else {
//                        //
//                        // always false until visit loaded
//                        //
//                        localContentNameOrId = EncodeText(ContentNameOrId);
//                        cacheTestName = localContentNameOrId;
//                        if (string.IsNullOrEmpty(cacheTestName))
//                        {
//                            cacheTestName = "iseditingall";
//                        }
//                        cacheTestName = vbLCase(cacheTestName);
//                        if (coreCommonModule.IsInDelimitedString(main_IsEditingContentList, cacheTestName, ","))
//                        {
//                            cpCore.testPoint("...is in main_IsEditingContentList");
//                            returnResult = true;
//                        }
//                        else if (coreCommonModule.IsInDelimitedString(main_IsNotEditingContentList, cacheTestName, ","))
//                        {
//                            cpCore.testPoint("...is in main_IsNotEditingContentList");
//                        }
//                        else {
//                            if (isAuthenticated())
//                            {
//                                if (!cpCore.pageManager_printVersion)
//                                {
//                                    if ((cpCore.visitProperty.getBoolean("AllowEditing") | cpCore.visitProperty.getBoolean("AllowAdvancedEditor")))
//                                    {
//                                        if (!string.IsNullOrEmpty(localContentNameOrId))
//                                        {
//                                            if (vbIsNumeric(localContentNameOrId))
//                                            {
//                                                localContentNameOrId = cpCore.metaData.getContentNameByID(EncodeInteger(localContentNameOrId));
//                                            }
//                                        }
//                                        returnResult = isAuthenticatedContentManager(localContentNameOrId);
//                                    }
//                                }
//                            }
//                            if (returnResult)
//                            {
//                                main_IsEditingContentList = main_IsEditingContentList + "," + cacheTestName;
//                            }
//                            else {
//                                main_IsNotEditingContentList = main_IsNotEditingContentList + "," + cacheTestName;
//                            }
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                cpCore.handleExceptionAndRethrow(ex);
//            }
//            return returnResult;
//        }
//        //
//        //========================================================================
//        /// <summary>
//        /// true if editing with the quick editor
//        /// </summary>
//        /// <param name="ContentName"></param>
//        /// <returns></returns>
//        public bool isQuickEditing(string ContentName)
//        {
//            bool returnResult = false;
//            try
//            {
//                if ((!cpCore.pageManager_printVersion))
//                {
//                    if (isAuthenticatedContentManager(EncodeText(ContentName)))
//                    {
//                        returnResult = cpCore.visitProperty.getBoolean("AllowQuickEditor");
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                cpCore.handleExceptionAndRethrow(ex);
//            }
//            return returnResult;
//        }
//        //
//        //========================================================================
//        // main_IsAdvancedEditing( ContentName )
//        /// <summary>
//        /// true if advanded editing
//        /// </summary>
//        /// <param name="ContentName"></param>
//        /// <returns></returns>
//        public bool isAdvancedEditing(string ContentName)
//        {
//            bool returnResult = false;
//            try
//            {
//                if ((!cpCore.pageManager_printVersion))
//                {
//                    if (isAuthenticatedContentManager(EncodeText(ContentName)))
//                    {
//                        returnResult = cpCore.visitProperty.getBoolean("AllowAdvancedEditor");
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                cpCore.handleExceptionAndRethrow(ex);
//            }
//            return returnResult;
//        }
//        //
//        //========================================================================
//        //   main_IsAdmin
//        //   true if:
//        //       Is Authenticated
//        //       Is Member
//        //       Member has admin or developer status
//        //========================================================================
//        //
//        public bool isAuthenticatedAdmin()
//        {
//            bool returnIs = false;
//            try
//            {
//                if ((!isAuthenticatedAdmin_cache_isLoaded) & cpCore.visit_initialized)
//                {
//                    isAuthenticatedAdmin_cache = isAuthenticated() & (isAdmin | isDeveloper);
//                    isAuthenticatedAdmin_cache_isLoaded = true;
//                }
//                returnIs = isAuthenticatedAdmin_cache;
//            }
//            catch (Exception ex)
//            {
//                cpCore.handleExceptionAndRethrow(ex);
//            }
//            return returnIs;
//        }
//        // true if member is administrator
//        private bool isAuthenticatedAdmin_cache = false;
//        // true if main_IsAdminCache is initialized
//        private bool isAuthenticatedAdmin_cache_isLoaded = false;
//        //
//        //========================================================================
//        //   main_IsDeveloper
//        //========================================================================
//        //
//        public bool isAuthenticatedDeveloper()
//        {
//            bool returnIs = false;
//            try
//            {
//                if ((!isAuthenticatedDeveloper_cache_isLoaded) & cpCore.visit_initialized)
//                {
//                    isAuthenticatedDeveloper_cache = (isAuthenticated() & isDeveloper);
//                    isAuthenticatedDeveloper_cache_isLoaded = true;
//                }
//                returnIs = isAuthenticatedDeveloper_cache;
//            }
//            catch (Exception ex)
//            {
//                cpCore.handleExceptionAndRethrow(ex);
//            }
//            return returnIs;
//        }
//        //
//        private bool isAuthenticatedDeveloper_cache = false;
//        private bool isAuthenticatedDeveloper_cache_isLoaded = false;
//        //
//        //=============================================================================
//        //   main_SaveMember()
//        //       Saves the member properties that are loaded during main_OpenMember
//        //=============================================================================
//        //
//        public void saveMember()
//        {
//            try
//            {
//                string SQL = null;
//                //
//                if (cpCore.visit_initialized)
//                {
//                    if ((id > 0))
//                    {
//                        SQL = "UPDATE ccMembers SET " + " Name=" + cpCore.db.encodeSQLText(name) + ",username=" + cpCore.db.encodeSQLText(username) + ",email=" + cpCore.db.encodeSQLText(email) + ",password=" + cpCore.db.encodeSQLText(password) + ",OrganizationID=" + cpCore.db.encodeSQLNumber(organizationId) + ",LanguageID=" + cpCore.db.encodeSQLNumber(languageId) + ",Active=" + cpCore.db.encodeSQLBoolean(active) + ",Company=" + cpCore.db.encodeSQLText(company) + ",Visits=" + cpCore.db.encodeSQLNumber(visits) + ",LastVisit=" + cpCore.db.encodeSQLDate(lastVisit) + ",AllowBulkEmail=" + cpCore.db.encodeSQLBoolean(allowBulkEmail) + ",AdminMenuModeID=" + cpCore.db.encodeSQLNumber(adminMenuModeID) + ",AutoLogin=" + cpCore.db.encodeSQLBoolean(autoLogin);
//                        // 6/18/2009 - removed notes from base
//                        //           & ",SendNotes=" & encodeSQLBoolean(MemberSendNotes)
//                        SQL += "" + ",BillEmail=" + cpCore.db.encodeSQLText(billEmail) + ",BillPhone=" + cpCore.db.encodeSQLText(billPhone) + ",BillFax=" + cpCore.db.encodeSQLText(billFax) + ",BillCompany=" + cpCore.db.encodeSQLText(billCompany) + ",BillAddress=" + cpCore.db.encodeSQLText(billAddress) + ",BillCity=" + cpCore.db.encodeSQLText(billCity) + ",BillState=" + cpCore.db.encodeSQLText(billState) + ",BillZip=" + cpCore.db.encodeSQLText(billZip) + ",BillCountry=" + cpCore.db.encodeSQLText(billCountry);
//                        SQL += "" + ",ShipName=" + cpCore.db.encodeSQLText(shipName) + ",ShipCompany=" + cpCore.db.encodeSQLText(shipCompany) + ",ShipAddress=" + cpCore.db.encodeSQLText(shipAddress) + ",ShipCity=" + cpCore.db.encodeSQLText(shipCity) + ",ShipState=" + cpCore.db.encodeSQLText(shipState) + ",ShipZip=" + cpCore.db.encodeSQLText(shipZip) + ",ShipCountry=" + cpCore.db.encodeSQLText(shipCountry) + ",ShipPhone=" + cpCore.db.encodeSQLText(shipPhone);
//                        if (true)
//                        {
//                            SQL += ",ExcludeFromAnalytics=" + cpCore.db.encodeSQLBoolean(excludeFromAnalytics);
//                        }
//                        SQL += " WHERE ID=" + id + ";";
//                        cpCore.db.executeSql(SQL);
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                cpCore.handleExceptionAndRethrow(ex);
//            }
//        }
//        //
//        //=============================================================================
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <returns></returns>
//        public string getLoginForm()
//        {
//            string returnHtml = "";
//            try
//            {
//                //
//                int loginAddonID = 0;
//                bool isAddonOk = false;
//                string QS = null;
//                //
//                loginAddonID = cpCore.siteProperties.getinteger("Login Page AddonID");
//                if (loginAddonID != 0)
//                {
//                    //
//                    // Custom Login
//                    //
//                    returnHtml = cpCore.addon.addon_execute_legacy2(loginAddonID, "", "", cpCoreClass.addonContextEnum.ContextPage, "", 0, "", "", false, 0,
//                    "", ref isAddonOk, null);
//                    if (!isAddonOk)
//                    {
//                        loginAddonID = 0;
//                    }
//                    else if ((string.IsNullOrEmpty(returnHtml)) & (isAddonOk))
//                    {
//                        //
//                        // login successful, redirect back to this page (without a method)
//                        //
//                        QS = cpCore.web_RefreshQueryString;
//                        QS = ModifyQueryString(QS, "method", "");
//                        QS = ModifyQueryString(QS, "RequestBinary", "");
//                        //
//                        cpCore.web_Redirect2("?" + QS, "Login form success", false);
//                    }
//                }
//                if (loginAddonID == 0)
//                {
//                    //
//                    // ----- When page loads, set focus on login username
//                    //
//                    returnHtml = getLoginForm_Default();
//                }
//            }
//            catch (Exception ex)
//            {
//                cpCore.handleExceptionAndRethrow(ex);
//            }
//            return returnHtml;
//        }
//        //
//        //=============================================================================
//        /// <summary>
//        /// a simple email password form
//        /// </summary>
//        /// <returns></returns>
//        public string getSendPasswordForm()
//        {
//            string returnResult = "";
//            try
//            {
//                string QueryString = null;
//                //
//                if (cpCore.siteProperties.getBoolean("allowPasswordEmail", true))
//                {
//                    returnResult = "" + cr + "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">" + cr2 + "<tr>" + cr3 + "<td style=\"text-align:right;vertical-align:middle;width:30%;padding:4px\" align=\"right\" width=\"30%\">" + SpanClassAdminNormal + "Email</span></td>" + cr3 + "<td style=\"text-align:left;vertical-align:middle;width:70%;padding:4px\" align=\"left\"  width=\"70%\"><input NAME=\"" + "email\" VALUE=\"" + cpcore.html.html_EncodeHTML(loginForm_Email) + "\" SIZE=\"20\" MAXLENGTH=\"50\"></td>" + cr2 + "</tr>" + cr2 + "<tr>" + cr3 + "<td colspan=\"2\">&nbsp;</td>" + cr2 + "</tr>" + cr2 + "<tr>" + cr3 + "<td colspan=\"2\">" + kmaIndent(kmaIndent(cpCore.main_GetPanelButtons(ButtonSendPassword, "Button"))) + cr3 + "</td>" + cr2 + "</tr>" + cr + "</table>" + "";
//                    //
//                    // write out all of the form input (except state) to hidden fields so they can be read after login
//                    //
//                    //
//                    returnResult = "" + returnResult + cpCore.html_GetFormInputHidden("Type", FormTypeSendPassword) + "";
//                    foreach (KeyValuePair<string, docPropertiesClass> kvp in cpCore.docProperties.docPropertiesDict)
//                    {
//                        var _with1 = kvp.Value;
//                        if (_with1.IsForm)
//                        {
//                            switch (vbUCase(_with1.Name))
//                            {
//                                case "S":
//                                case "MA":
//                                case "MB":
//                                case "USERNAME":
//                                case "PASSWORD":
//                                case "EMAIL":
//                                    break;
//                                default:
//                                    returnResult = returnResult + cpCore.html_GetFormInputHidden(_with1.Name, _with1.Value);
//                                    break;
//                            }
//                        }
//                    }
//                    //
//                    QueryString = cpCore.web_RefreshQueryString;
//                    QueryString = ModifyQueryString(QueryString, "S", "");
//                    QueryString = ModifyQueryString(QueryString, "ccIPage", "");
//                    returnResult = "" + cpCore.html_GetFormStart(QueryString) + kmaIndent(returnResult) + cr + "</form>" + "";
//                }
//            }
//            catch (Exception ex)
//            {
//                cpCore.handleExceptionAndRethrow(ex);
//            }
//            return returnResult;
//        }
//        //
//        //===============================================================================================================================
//        //   Is Group Member of a GroupIDList
//        //   admins are always returned true
//        //===============================================================================================================================
//        //
//        public bool isMemberOfGroupIdList(int MemberID, bool isAuthenticated, string GroupIDList)
//        {
//            return isMemberOfGroupIdList(MemberID, isAuthenticated, GroupIDList, true);
//        }
//        //
//        //===============================================================================================================================
//        //   Is Group Member of a GroupIDList
//        //===============================================================================================================================
//        //
//        public bool isMemberOfGroupIdList(int MemberID, bool isAuthenticated, string GroupIDList, bool adminReturnsTrue)
//        {
//            bool returnREsult = false;
//            try
//            {
//                //
//                int CS = 0;
//                string SQL = null;
//                string Criteria = null;
//                string WorkingIDList = null;
//                //
//                returnREsult = false;
//                if (isAuthenticated)
//                {
//                    WorkingIDList = GroupIDList;
//                    WorkingIDList = vbReplace(WorkingIDList, " ", "");
//                    while (vbInstr(1, WorkingIDList, ",,") != 0)
//                    {
//                        WorkingIDList = vbReplace(WorkingIDList, ",,", ",");
//                    }
//                    if ((!string.IsNullOrEmpty(WorkingIDList)))
//                    {
//                        if (vbMid(WorkingIDList, 1) == ",")
//                        {
//                            if (vbLen(WorkingIDList) <= 1)
//                            {
//                                WorkingIDList = "";
//                            }
//                            else {
//                                WorkingIDList = vbMid(WorkingIDList, 2);
//                            }
//                        }
//                    }
//                    if ((!string.IsNullOrEmpty(WorkingIDList)))
//                    {
//                        if (vbRight(WorkingIDList, 1) == ",")
//                        {
//                            if (vbLen(WorkingIDList) <= 1)
//                            {
//                                WorkingIDList = "";
//                            }
//                            else {
//                                WorkingIDList = vbMid(WorkingIDList, 1, vbLen(WorkingIDList) - 1);
//                            }
//                        }
//                    }
//                    if ((string.IsNullOrEmpty(WorkingIDList)))
//                    {
//                        if (adminReturnsTrue)
//                        {
//                            //
//                            // check if memberid is admin
//                            //
//                            SQL = "select top 1 m.id" + " from ccmembers m" + " where" + " (m.id=" + MemberID + ")" + " and(m.active<>0)" + " and(" + " (m.admin<>0)" + " or(m.developer<>0)" + " )" + " ";
//                            CS = cpCore.db.openCsSql_rev("default", SQL);
//                            returnREsult = cpCore.db.cs_Ok(CS);
//                            cpCore.db.cs_Close(ref CS);
//                        }
//                    }
//                    else {
//                        //
//                        // check if they are admin or in the group list
//                        //
//                        if (vbInstr(1, WorkingIDList, ",") != 0)
//                        {
//                            Criteria = "r.GroupID in (" + WorkingIDList + ")";
//                        }
//                        else {
//                            Criteria = "r.GroupID=" + WorkingIDList;
//                        }
//                        Criteria = "" + "(" + Criteria + ")" + " and(r.id is not null)" + " and((r.DateExpires is null)or(r.DateExpires>" + cpCore.db.encodeSQLDate(DateTime.Now) + "))" + " ";
//                        if (adminReturnsTrue)
//                        {
//                            Criteria = "(" + Criteria + ")or(m.admin<>0)or(m.developer<>0)";
//                        }
//                        Criteria = "" + "(" + Criteria + ")" + " and(m.active<>0)" + " and(m.id=" + MemberID + ")";
//                        //
//                        SQL = "select top 1 m.id" + " from ccmembers m" + " left join ccMemberRules r on r.Memberid=m.id" + " where" + Criteria;
//                        CS = cpCore.db.openCsSql_rev("default", SQL);
//                        returnREsult = cpCore.db.cs_Ok(CS);
//                        cpCore.db.cs_Close(ref CS);
//                    }
//                }

//            }
//            catch (Exception ex)
//            {
//                cpCore.handleExceptionAndRethrow(ex);
//            }
//            return returnREsult;
//        }
//        //
//        //========================================================================
//        // Member Open
//        //   Attempts to open the Member record based on the iRecordID
//        //   If successful, MemberID is set to the iRecordID
//        //========================================================================
//        //
//        public int initializeUser(int recordId)
//        {
//            int returnRecordId = 0;
//            try
//            {
//                int CS = 0;
//                //
//                if (recordId != 0)
//                {
//                    //
//                    // attempt to read in Member record if logged on
//                    // dont just do main_CheckMember() -- in case a pretty login is needed
//                    //
//                    CS = cpCore.csOpenRecord("People", recordId);
//                    if (cpCore.db.cs_Ok(CS))
//                    {
//                        name = cpCore.db.cs_getText(CS, "Name");
//                        isDeveloper = cpCore.db.cs_getBoolean(CS, "Developer");
//                        isAdmin = cpCore.db.cs_getBoolean(CS, "Admin");
//                        contentControlID = cpCore.db.cs_getInteger(CS, "ContentControlID");
//                        organizationId = cpCore.db.cs_getInteger(CS, "OrganizationID");
//                        languageId = cpCore.db.cs_getInteger(CS, "LanguageID");
//                        language = cpCore.main_cs_getEncodedField(CS, "LanguageID");
//                        //
//                        shipName = cpCore.db.cs_getText(CS, "ShipName");
//                        shipCompany = cpCore.db.cs_getText(CS, "ShipCompany");
//                        shipAddress = cpCore.db.cs_getText(CS, "ShipAddress");
//                        shipCity = cpCore.db.cs_getText(CS, "ShipCity");
//                        shipState = cpCore.db.cs_getText(CS, "ShipState");
//                        shipZip = cpCore.db.cs_getText(CS, "ShipZip");
//                        shipCountry = cpCore.db.cs_getText(CS, "ShipCountry");
//                        shipPhone = cpCore.db.cs_getText(CS, "ShipPhone");
//                        //
//                        billCompany = cpCore.db.cs_getText(CS, "BillCompany");
//                        billAddress = cpCore.db.cs_getText(CS, "BillAddress");
//                        billCity = cpCore.db.cs_getText(CS, "BillCity");
//                        billState = cpCore.db.cs_getText(CS, "BillState");
//                        billZip = cpCore.db.cs_getText(CS, "BillZip");
//                        billCountry = cpCore.db.cs_getText(CS, "BillCountry");
//                        billEmail = cpCore.db.cs_getText(CS, "BillEmail");
//                        billPhone = cpCore.db.cs_getText(CS, "BillPhone");
//                        billFax = cpCore.db.cs_getText(CS, "BillFax");
//                        //
//                        allowBulkEmail = cpCore.db.cs_getBoolean(CS, "AllowBulkEmail");
//                        allowToolsPanel = cpCore.db.cs_getBoolean(CS, "AllowToolsPanel");
//                        adminMenuModeID = cpCore.db.cs_getInteger(CS, "AdminMenuModeID");
//                        autoLogin = cpCore.db.cs_getBoolean(CS, "AutoLogin");
//                        //
//                        styleFilename = cpCore.db.cs_getText(CS, "StyleFilename");
//                        if (!string.IsNullOrEmpty(styleFilename))
//                        {
//                            cpCore.main_AddStylesheetLink(cpCore.web_requestProtocol + cpCore.webServer.requestDomain + cpCore.csv_getVirtualFileLink(cpCore.serverconfig.appConfig.cdnFilesNetprefix, styleFilename));
//                        }
//                        excludeFromAnalytics = cpCore.db.cs_getBoolean(CS, "ExcludeFromAnalytics");
//                        returnRecordId = recordId;
//                    }
//                    cpCore.db.cs_Close(ref CS);
//                }
//            }
//            catch (Exception ex)
//            {
//                cpCore.handleExceptionAndRethrow(ex);
//            }
//            return returnRecordId;
//        }
//        //
//        //========================================================================
//        // ----- Returns true if the visitor is an admin, or authenticated and in the group named
//        //========================================================================
//        //
//        public bool IsMemberOfGroup2(string GroupName, int checkMemberID = 0)
//        {
//            bool returnREsult = false;
//            try
//            {
//                int iMemberID = 0;
//                iMemberID = EncodeInteger(checkMemberID);
//                if (iMemberID == 0)
//                {
//                    iMemberID = id;
//                }
//                returnREsult = isMemberOfGroupList("," + cpCore.group_GetGroupID(EncodeText(GroupName)), iMemberID, true);
//            }
//            catch (Exception ex)
//            {
//                cpCore.handleExceptionAndRethrow(ex);
//            }
//            return returnREsult;
//        }
//        //
//        //========================================================================
//        // ----- Returns true if the visitor is a member, and in the group named
//        //========================================================================
//        //
//        public bool isMemberOfGroup(string GroupName, int checkMemberID = 0, bool adminReturnsTrue = false)
//        {
//            bool returnREsult = false;
//            try
//            {
//                int iMemberID = 0;
//                iMemberID = checkMemberID;
//                if (iMemberID == 0)
//                {
//                    iMemberID = id;
//                }
//                returnREsult = isMemberOfGroupList("," + cpCore.group_GetGroupID(EncodeText(GroupName)), iMemberID, adminReturnsTrue);
//            }
//            catch (Exception ex)
//            {
//                cpCore.handleExceptionAndRethrow(ex);
//            }
//            return returnREsult;
//        }

//        //
//        //========================================================================
//        // ----- Returns true if the visitor is an admin, or authenticated and in the group list
//        //========================================================================
//        //
//        public bool isMemberOfGroupList(string GroupIDList, int checkMemberID = 0, bool adminReturnsTrue = false)
//        {
//            bool returnREsult = false;
//            try
//            {
//                if (checkMemberID == 0)
//                {
//                    checkMemberID = id;
//                }
//                returnREsult = isMemberOfGroupIdList(checkMemberID, isAuthenticated(), GroupIDList, adminReturnsTrue);
//            }
//            catch (Exception ex)
//            {
//                cpCore.handleExceptionAndRethrow(ex);
//            }
//            return returnREsult;
//        }
//        //
//        //========================================================================
//        //   IsMember
//        //   true if the user is authenticated and is a trusted people (member content)
//        //========================================================================
//        //
//        public bool isAuthenticatedMember()
//        {
//            bool returnREsult = false;
//            try
//            {
//                if ((!property_user_isMember_isLoaded) & (cpCore.visit_initialized))
//                {
//                    property_user_isMember = isAuthenticated() & cpCore.IsWithinContent(contentControlID, cpCore.main_GetContentID("members"));
//                    property_user_isMember_isLoaded = true;
//                }
//                returnREsult = property_user_isMember;
//            }
//            catch (Exception ex)
//            {
//                cpCore.handleExceptionAndRethrow(ex);
//            }
//            return returnREsult;
//        }
//        private bool property_user_isMember = false;
//        private bool property_user_isMember_isLoaded = false;
//        //
//        //========================================================================
//        // ----- Process the login form
//        //========================================================================
//        //
//        public bool processFormLoginDefault()
//        {
//            bool returnREsult = false;
//            try
//            {
//                int LocalMemberID = 0;
//                returnREsult = false;
//                //
//                if (!main_loginFormDefaultProcessed)
//                {
//                    //
//                    // Processing can happen
//                    //   1) early in init() -- legacy
//                    //   2) as well as at the front of main_GetLoginForm - to support addon Login forms
//                    // This flag prevents the default form from processing twice
//                    //
//                    main_loginFormDefaultProcessed = true;
//                    loginForm_Username = cpCore.docProperties.getText("username");
//                    loginForm_Password = cpCore.docProperties.getText("password");
//                    loginForm_AutoLogin = cpCore.main_GetStreamBoolean2("autologin");
//                    //
//                    if ((cpCore.visit_loginAttempts < main_maxVisitLoginAttempts) & (cpCore.visit_cookieSupport))
//                    {
//                        LocalMemberID = authenticateGetId(loginForm_Username, loginForm_Password);
//                        if (LocalMemberID == 0)
//                        {
//                            cpCore.visit_loginAttempts = cpCore.visit_loginAttempts + 1;
//                            cpCore.visit_save();
//                        }
//                        else {
//                            returnREsult = authenticateById(LocalMemberID, loginForm_AutoLogin);
//                            if (returnREsult)
//                            {
//                                cpCore.log_LogActivity2("successful username/password login", id, organizationId);
//                            }
//                            else {
//                                cpCore.log_LogActivity2("bad username/password login", id, organizationId);
//                            }
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                cpCore.handleExceptionAndRethrow(ex);
//            }
//            return returnREsult;
//        }
//        //
//        //========================================================================
//        // ----- Process the send password form
//        //========================================================================
//        //
//        public void processFormSendPassword()
//        {
//            try
//            {
//                loginForm_Email = cpCore.docProperties.getText("email");
//                sendPassword(loginForm_Email);
//            }
//            catch (Exception ex)
//            {
//                cpCore.handleExceptionAndRethrow(ex);
//            }
//        }
//        //
//        //=============================================================================
//        // Send the Member his username and password
//        //=============================================================================
//        //
//        public bool sendPassword(object Email)
//        {
//            bool returnREsult = false;
//            try
//            {
//                string sqlCriteria = null;
//                string Message = null;
//                int CS = 0;
//                string MethodName = null;
//                string workingEmail = null;
//                string FromAddress = null;
//                string subject = null;
//                bool allowEmailLogin = false;
//                string Password = null;
//                string Username = null;
//                bool updateUser = false;
//                int atPtr = 0;
//                int Cnt = 0;
//                int Index = 0;
//                string EMailName = null;
//                bool usernameOK = false;
//                int recordCnt = 0;
//                string hint = null;
//                int Ptr = 0;
//                //
//                const string passwordChrs = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ012345678999999";
//                const int passwordChrsLength = 62;
//                //
//                //hint = "100"
//                workingEmail = EncodeText(Email);
//                //
//                returnREsult = false;
//                if (string.IsNullOrEmpty(workingEmail))
//                {
//                    //hint = "110"
//                    cpCore.error_AddUserError("Please enter your email address before requesting your username and password.");
//                }
//                else {
//                    //hint = "120"
//                    atPtr = vbInstr(1, workingEmail, "@");
//                    if (atPtr < 2)
//                    {
//                        //
//                        // email not valid
//                        //
//                        //hint = "130"
//                        cpCore.error_AddUserError("Please enter a valid email address before requesting your username and password.");
//                    }
//                    else {
//                        //hint = "140"
//                        EMailName = vbMid(workingEmail, 1, atPtr - 1);
//                        //
//                        cpCore.log_LogActivity2("password request for email " + workingEmail, id, organizationId);
//                        //
//                        allowEmailLogin = cpCore.siteProperties.getBoolean("allowEmailLogin", false);
//                        recordCnt = 0;
//                        sqlCriteria = "(email=" + cpCore.db.encodeSQLText(workingEmail) + ")";
//                        if (true)
//                        {
//                            sqlCriteria = sqlCriteria + "and((dateExpires is null)or(dateExpires>" + cpCore.db.encodeSQLDate(DateTime.Now) + "))";
//                        }
//                        CS = cpCore.db.csOpen("People", sqlCriteria, "ID", SelectFieldList: "username,password", PageSize: 1);
//                        if (!cpCore.db.cs_Ok(CS))
//                        {
//                            //
//                            // valid login account for this email not found
//                            //
//                            if ((Strings.LCase(vbMid(workingEmail, atPtr + 1)) == "contensive.com"))
//                            {
//                                //
//                                // look for expired account to renew
//                                //
//                                cpCore.db.cs_Close(ref CS);
//                                CS = cpCore.db.csOpen("People", "((email=" + cpCore.db.encodeSQLText(workingEmail) + "))", "ID", PageSize: 1);
//                                if (cpCore.db.cs_Ok(CS))
//                                {
//                                    //
//                                    // renew this old record
//                                    //
//                                    //hint = "150"
//                                    cpCore.db.SetCSField(CS, "developer", "1");
//                                    cpCore.db.SetCSField(CS, "admin", "1");
//                                    cpCore.db.SetCSField(CS, "dateExpires", DateTime.Now.AddDays(7).Date.ToString());
//                                }
//                                else {
//                                    //
//                                    // inject support record
//                                    //
//                                    //hint = "150"
//                                    cpCore.db.cs_Close( ref CS);
//                                    CS = cpCore.db.cs_insertRecord("people");
//                                    cpCore.db.SetCSField(CS, "name", "Contensive Support");
//                                    cpCore.db.SetCSField(CS, "email", workingEmail);
//                                    cpCore.db.SetCSField(CS, "developer", "1");
//                                    cpCore.db.SetCSField(CS, "admin", "1");
//                                    cpCore.db.SetCSField(CS, "dateExpires", DateTime.Now.AddDays(7).Date.ToString());
//                                }
//                                cpCore.db.SaveCSRecord(CS);
//                            }
//                            else {
//                                //hint = "155"
//                                cpCore.error_AddUserError("No current user was found matching this email address. Please try again. ");
//                            }
//                        }
//                        if (cpCore.db.cs_Ok(CS))
//                        {
//                            //hint = "160"
//                            FromAddress = cpCore.siteProperties.getText("EmailFromAddress", "info@" + cpCore.main_ServerDomain);
//                            subject = "Password Request at " + cpCore.main_ServerDomain;
//                            Message = "";
//                            while (cpCore.db.cs_Ok(CS))
//                            {
//                                //hint = "170"
//                                updateUser = false;
//                                if (string.IsNullOrEmpty(Message))
//                                {
//                                    //hint = "180"
//                                    Message = "This email was sent in reply to a request at " + cpCore.main_ServerDomain + " for the username and password associated with this email address. ";
//                                    Message = Message + "If this request was made by you, please return to the login screen and use the following:" + Constants.vbCrLf;
//                                    Message = Message + Constants.vbCrLf;
//                                }
//                                else {
//                                    //hint = "190"
//                                    Message = Message + Constants.vbCrLf;
//                                    Message = Message + "Additional user accounts with the same email address: " + Constants.vbCrLf;
//                                }
//                                //
//                                // username
//                                //
//                                //hint = "200"
//                                Username = cpCore.db.cs_getText(CS, "Username");
//                                usernameOK = true;
//                                if (!allowEmailLogin)
//                                {
//                                    //hint = "210"
//                                    if (Username != Username.Trim())
//                                    {
//                                        //hint = "220"
//                                        Username = Username.Trim();
//                                        updateUser = true;
//                                    }
//                                    if (string.IsNullOrEmpty(Username))
//                                    {
//                                        //hint = "230"
//                                        //username = emailName & Int(Rnd() * 9999)
//                                        usernameOK = false;
//                                        Ptr = 0;
//                                        while (!usernameOK & (Ptr < 100))
//                                        {
//                                            //hint = "240"
//                                            Username = EMailName + Conversion.Int(VBMath.Rnd() * 9999);
//                                            usernameOK = !cpCore.main_IsLoginOK(Username, "test");
//                                            Ptr = Ptr + 1;
//                                        }
//                                        //hint = "250"
//                                        if (usernameOK)
//                                        {
//                                            updateUser = true;
//                                        }
//                                    }
//                                    //hint = "260"
//                                    Message = Message + " username: " + Username + Constants.vbCrLf;
//                                }
//                                //hint = "270"
//                                if (usernameOK)
//                                {
//                                    //
//                                    // password
//                                    //
//                                    //hint = "280"
//                                    Password = cpCore.db.cs_getText(CS, "Password");
//                                    if (Password.Trim() != Password)
//                                    {
//                                        //hint = "290"
//                                        Password = Password.Trim();
//                                        updateUser = true;
//                                    }
//                                    //hint = "300"
//                                    if (string.IsNullOrEmpty(Password))
//                                    {
//                                        //hint = "310"
//                                        for (Ptr = 0; Ptr <= 8; Ptr++)
//                                        {
//                                            //hint = "320"
//                                            Index = Convert.ToInt32(VBMath.Rnd() * passwordChrsLength);
//                                            Password = Password + vbMid(passwordChrs, Index, 1);
//                                        }
//                                        //hint = "330"
//                                        updateUser = true;
//                                    }
//                                    //hint = "340"
//                                    Message = Message + " password: " + Password + Constants.vbCrLf;
//                                    returnREsult = true;
//                                    if (updateUser)
//                                    {
//                                        //hint = "350"
//                                        cpCore.db.cs_set(CS, "username", Username);
//                                        cpCore.db.cs_set(CS, "password", Password);
//                                    }
//                                    recordCnt = recordCnt + 1;
//                                }
//                                cpCore.db.csGoNext(CS);
//                            }
//                        }
//                    }
//                }
//                //hint = "360"
//                if (returnREsult)
//                {
//                    cpCore.main_SendEmail(workingEmail, FromAddress, subject, Message, 0, true, false);
//                    //    main_ClosePageHTML = main_ClosePageHTML & main_GetPopupMessage(app.publicFiles.ReadFile("ccLib\Popup\PasswordSent.htm"), 300, 300, "no")
//                }
//            }
//            catch (Exception ex)
//            {
//                cpCore.handleExceptionAndRethrow(ex);
//            }
//            return returnREsult;
//        }

//        public coreUserClass(cpCoreClass cpCore) : base()
//        {
//            this.cpCore = cpCore;
//        }
//        //
//        //========================================================================
//        // main_IsContentManager2
//        //   If ContentName is missing, returns true if this is an authenticated member with
//        //       content management over anything
//        //   If ContentName is given, it only tests this content
//        //========================================================================
//        //
//        public bool isAuthenticatedContentManager(string ContentName = "")
//        {
//            bool returnIsContentManager = false;
//            try
//            {
//                string SQL = null;
//                int CS = 0;
//                bool notImplemented_allowAdd = false;
//                bool notImplemented_allowDelete = false;
//                //
//                // REFACTOR -- add a private dictionary with contentname=>result, plus a authenticationChange flag that makes properties like this invalid
//                //
//                returnIsContentManager = false;
//                if (string.IsNullOrEmpty(ContentName))
//                {
//                    if (isAuthenticated())
//                    {
//                        if (isAuthenticatedAdmin())
//                        {
//                            returnIsContentManager = true;
//                        }
//                        else {
//                            //
//                            // Is a CM for any content def
//                            //
//                            if ((!_isAuthenticatedContentManagerAnything_loaded) | (_isAuthenticatedContentManagerAnything_userId != id))
//                            {
//                                SQL = "SELECT ccGroupRules.ContentID" + " FROM ccGroupRules RIGHT JOIN ccMemberRules ON ccGroupRules.GroupID = ccMemberRules.GroupID" + " WHERE (" + "(ccMemberRules.MemberID=" + cpCore.db.encodeSQLNumber(id) + ")" + " AND(ccMemberRules.active<>0)" + " AND(ccGroupRules.active<>0)" + " AND(ccGroupRules.ContentID Is not Null)" + " AND((ccMemberRules.DateExpires is null)OR(ccMemberRules.DateExpires>" + cpCore.db.encodeSQLDate(cpCore.main_PageStartTime) + "))" + ")";
//                                CS = cpCore.db.cs_openSql(SQL);
//                                _isAuthenticatedContentManagerAnything = cpCore.db.cs_Ok(CS);
//                                cpCore.db.cs_Close(ref CS);
//                                //
//                                _isAuthenticatedContentManagerAnything_userId = id;
//                                _isAuthenticatedContentManagerAnything_loaded = true;
//                            }
//                            returnIsContentManager = _isAuthenticatedContentManagerAnything;
//                        }
//                    }
//                }
//                else {
//                    //
//                    // Specific Content called out
//                    //
//                    cpCore.user.getContentAccessRights(ContentName, ref returnIsContentManager, ref notImplemented_allowAdd,ref  notImplemented_allowDelete);
//                }
//            }
//            catch (Exception ex)
//            {
//                cpCore.handleExceptionAndRethrow(ex);
//            }
//            return returnIsContentManager;
//        }
//        private bool _isAuthenticatedContentManagerAnything_loaded = false;
//        private int _isAuthenticatedContentManagerAnything_userId;
//        private bool _isAuthenticatedContentManagerAnything;
//        //
//        //========================================================================
//        // Member Login (by username and password)
//        //
//        //   See main_GetLoginMemberID and main_LoginMemberByID
//        //========================================================================
//        //
//        public bool authenticate(string loginFieldValue, string password, bool AllowAutoLogin = false)
//        {
//            bool returnREsult = false;
//            try
//            {
//                int LocalMemberID = 0;
//                //
//                returnREsult = false;
//                LocalMemberID = authenticateGetId(loginFieldValue, password);
//                if (LocalMemberID != 0)
//                {
//                    returnREsult = authenticateById(LocalMemberID, AllowAutoLogin);
//                    if (returnREsult)
//                    {
//                        cpCore.log_LogActivity2("successful password login", id, organizationId);
//                        isAuthenticatedAdmin_cache_isLoaded = false;
//                        property_user_isMember_isLoaded = false;
//                    }
//                    else {
//                        cpCore.log_LogActivity2("unsuccessful login (loginField:" + loginFieldValue + "/password:" + password + ")", id, organizationId);
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                cpCore.handleExceptionAndRethrow(ex);
//            }
//            return returnREsult;
//        }
//        //
//        //========================================================================
//        //   Member Login By ID
//        //
//        //========================================================================
//        //
//        public bool authenticateById(int irecordID, bool AllowAutoLogin = false)
//        {
//            bool returnREsult = false;
//            try
//            {
//                int CS = 0;
//                //
//                returnREsult = recognizeById(irecordID);
//                if (returnREsult)
//                {
//                    //
//                    // Log them in
//                    //
//                    cpCore.visit_isAuthenticated = true;
//                    cpCore.visit_save();
//                    isAuthenticatedAdmin_cache_isLoaded = false;
//                    property_user_isMember_isLoaded = false;
//                    isAuthenticatedDeveloper_cache_isLoaded = false;
//                    //
//                    // Write Cookies in case Visit Tracking is off
//                    //
//                    if (cpCore.visit_startTime == System.DateTime.MinValue)
//                    {
//                        cpCore.visit_startTime = cpCore.main_PageStartTime;
//                    }
//                    if (!cpCore.siteProperties.allowVisitTracking)
//                    {
//                        cpCore.visit_init(true);
//                    }
//                    //
//                    // Change autologin if included, selected, and allowed
//                    //
//                    if (AllowAutoLogin ^ autoLogin)
//                    {
//                        if (cpCore.siteProperties.getBoolean("AllowAutoLogin"))
//                        {
//                            CS = cpCore.csOpenRecord("people", irecordID);
//                            if (cpCore.db.cs_Ok(CS))
//                            {
//                                cpCore.db.cs_set(CS, "AutoLogin", AllowAutoLogin);
//                            }
//                            cpCore.db.cs_Close(ref CS);
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                cpCore.handleExceptionAndRethrow(ex);
//            }
//            return returnREsult;
//        }
//        //
//        //========================================================================
//        //   RecognizeMember
//        //
//        //   the current member to be non-authenticated, but recognized
//        //========================================================================
//        //
//        public bool recognizeById(int RecordID)
//        {
//            bool returnREsult = false;
//            try
//            {
//                int CS = 0;
//                string SQL = null;
//                //
//                SQL = "select" + " ccMembers.*" + " ,ccLanguages.name as LanguageName" + " from" + " ccMembers" + " left join ccLanguages on ccMembers.LanguageID=ccLanguages.ID" + " where" + " (ccMembers.active<>" + SQLFalse + ")" + " and(ccMembers.ID=" + RecordID + ")";
//                SQL += "" + " and((ccMembers.dateExpires is null)or(ccMembers.dateExpires>" + cpCore.db.encodeSQLDate(DateTime.Now) + "))" + "";
//                CS = cpCore.db.cs_openSql(SQL);
//                if (cpCore.db.cs_Ok(CS))
//                {
//                    if (cpCore.visit_Id == 0)
//                    {
//                        //
//                        // Visit was blocked during init, init the visit DateTime.Now
//                        //
//                        cpCore.visit_init(true);
//                    }
//                    //
//                    // ----- Member was recognized
//                    //   REFACTOR -- when the id is set, the user object is populated, so the rest of this can be removed (verify these are all set in the load
//                    //
//                    id = (cpCore.db.cs_getInteger(CS, "ID"));
//                    name = (cpCore.db.cs_getText(CS, "Name"));
//                    username = (cpCore.db.cs_getText(CS, "username"));
//                    email = (cpCore.db.cs_getText(CS, "Email"));
//                    password = (cpCore.db.cs_getText(CS, "Password"));
//                    organizationId = (cpCore.db.cs_getInteger(CS, "OrganizationID"));
//                    languageId = (cpCore.db.cs_getInteger(CS, "LanguageID"));
//                    active = (cpCore.db.cs_getBoolean(CS, "Active"));
//                    company = (cpCore.db.cs_getText(CS, "Company"));
//                    visits = (cpCore.db.cs_getInteger(CS, "Visits"));
//                    lastVisit = (cpCore.db.cs_getDate(CS, "LastVisit"));
//                    allowBulkEmail = (cpCore.db.cs_getBoolean(CS, "AllowBulkEmail"));
//                    allowToolsPanel = (cpCore.db.cs_getBoolean(CS, "AllowToolsPanel"));
//                    adminMenuModeID = (cpCore.db.cs_getInteger(CS, "AdminMenuModeID"));
//                    autoLogin = (cpCore.db.cs_getBoolean(CS, "AutoLogin"));
//                    isDeveloper = (cpCore.db.cs_getBoolean(CS, "Developer"));
//                    isAdmin = (cpCore.db.cs_getBoolean(CS, "Admin"));
//                    contentControlID = (cpCore.db.cs_getInteger(CS, "ContentControlID"));
//                    languageId = (cpCore.db.cs_getInteger(CS, "LanguageID"));
//                    language = (cpCore.db.cs_getText(CS, "LanguageName"));
//                    styleFilename = cpCore.db.cs_getText(CS, "StyleFilename");
//                    if (!string.IsNullOrEmpty(styleFilename))
//                    {
//                        cpCore.main_AddStylesheetLink(cpCore.web_requestProtocol + cpCore.webServer.requestDomain + cpCore.csv_getVirtualFileLink(cpCore.serverconfig.appConfig.cdnFilesNetprefix, styleFilename));
//                    }
//                    excludeFromAnalytics = cpCore.db.cs_getBoolean(CS, "ExcludeFromAnalytics");
//                    //
//                    visits = visits + 1;
//                    if (visits == 1)
//                    {
//                        isNew = true;
//                    }
//                    else {
//                        isNew = false;
//                    }
//                    lastVisit = cpCore.main_PageStartTime;
//                    //cpCore.main_VisitMemberID = id
//                    cpCore.visit_loginAttempts = 0;
//                    cpCore.visitor_memberID = id;
//                    cpCore.visit_excludeFromAnalytics = cpCore.visit_excludeFromAnalytics | cpCore.visit_isBot | excludeFromAnalytics | isAdmin | isDeveloper;
//                    cpCore.visit_save();
//                    cpCore.visitor_save();
//                    saveMemberBase();
//                    returnREsult = true;
//                }
//                cpCore.db.cs_Close(ref CS);
//            }
//            catch (Exception ex)
//            {
//                cpCore.handleExceptionAndRethrow(ex);
//            }
//            return returnREsult;
//        }
//        //
//        //========================================================================
//        // ----- Create a new default user and save it
//        //       If failure, MemberID is 0
//        //       If successful, main_VisitMemberID and main_VisitorMemberID must be set to MemberID
//        //========================================================================
//        //
//        public void createUser()
//        {
//            try
//            {
//                int CSMember = 0;
//                int CSlanguage = 0;
//                //
//                createUserDefaults(cpCore.visit_name);
//                //
//                id = 0;
//                CSMember = cpCore.db.cs_insertRecord("people");
//                if (!cpCore.db.cs_Ok(CSMember))
//                {
//                    cpCore.handleExceptionAndRethrow(new ApplicationException("main_CreateUser, Error inserting new people record, could not main_CreateUser"));
//                }
//                else {
//                    id = cpCore.db.cs_getInteger(CSMember, "id");
//                    cpCore.db.cs_set(CSMember, "CreatedByVisit", true);
//                    //
//                    active = true;
//                    cpCore.db.cs_set(CSMember, "active", active);
//                    //
//                    visits = 1;
//                    cpCore.db.cs_set(CSMember, "Visits", visits);
//                    //
//                    lastVisit = cpCore.main_PageStartTime;
//                    cpCore.db.cs_set(CSMember, "LastVisit", lastVisit);
//                    //
//                    //
//                    CSlanguage = cpCore.csOpenRecord("Languages", cpCore.web_GetBrowserLanguageID(), SelectFieldList: "Name");
//                    if (cpCore.db.cs_Ok(CSlanguage))
//                    {
//                        languageId = cpCore.db.cs_getInteger(CSlanguage, "ID");
//                        language = cpCore.db.cs_getText(CSlanguage, "Name");
//                        cpCore.db.cs_set(CSMember, "LanguageID", languageId);
//                    }
//                    cpCore.db.cs_Close(ref CSlanguage);
//                    //
//                    userAdded = true;
//                    isNew = true;
//                    styleFilename = "";
//                    excludeFromAnalytics = false;
//                    //
//                    cpCore.db.cs_Close(ref CSMember);
//                    //
//                    //cpCore.main_VisitMemberID = id
//                    cpCore.visitor_memberID = id;
//                    cpCore.visit_isAuthenticated = false;
//                    cpCore.visit_save();
//                    cpCore.visitor_save();
//                    //
//                    isAuthenticatedAdmin_cache_isLoaded = false;
//                    property_user_isMember_isLoaded = false;
//                    isAuthenticatedDeveloper_cache_isLoaded = false;
//                }
//            }
//            catch (Exception ex)
//            {
//                cpCore.handleExceptionAndRethrow(ex);
//            }
//        }
//        //
//        //========================================================================
//        //   Creates the internal records for the user, but does not create
//        //   a people record to save them
//        //========================================================================
//        //
//        public void createUserDefaults(string DefaultName)
//        {
//            try
//            {
//                int CSlanguage = 0;
//                //
//                id = 0;
//                name = DefaultName;
//                isAdmin = false;
//                isDeveloper = false;
//                organizationId = 0;
//                languageId = 0;
//                language = "";
//                isNew = false;
//                email = "";
//                allowBulkEmail = false;
//                allowToolsPanel = false;
//                autoLogin = false;
//                adminMenuModeID = 0;
//                loginForm_Username = "";
//                loginForm_Password = "";
//                loginForm_Email = "";
//                loginForm_AutoLogin = false;
//                userAdded = false;
//                username = "";
//                password = "";
//                contentControlID = 0;
//                active = false;
//                visits = 0;
//                lastVisit = cpCore.visit_startTime;
//                company = "";
//                user_Title = "";
//                main_MemberAddress = "";
//                main_MemberCity = "";
//                main_MemberState = "";
//                main_MemberZip = "";
//                main_MemberCountry = "";
//                main_MemberPhone = "";
//                main_MemberFax = "";
//                //
//                active = true;
//                //
//                visits = 1;
//                //
//                lastVisit = cpCore.main_PageStartTime;
//                //
//                //
//                CSlanguage = cpCore.csOpenRecord("Languages", cpCore.web_GetBrowserLanguageID(), SelectFieldList: "Name");
//                if (cpCore.db.cs_Ok(CSlanguage))
//                {
//                    languageId = cpCore.db.cs_getInteger(CSlanguage, "ID");
//                    language = cpCore.db.cs_getText(CSlanguage, "Name");
//                }
//                cpCore.db.cs_Close(ref CSlanguage);
//                //
//                userAdded = true;
//                isNew = true;
//                styleFilename = "";
//                excludeFromAnalytics = false;
//                //
//                isAuthenticatedAdmin_cache_isLoaded = false;
//                property_user_isMember_isLoaded = false;
//                isAuthenticatedDeveloper_cache_isLoaded = false;
//            }
//            catch (Exception ex)
//            {
//                cpCore.handleExceptionAndRethrow(ex);
//            }
//        }
//        //
//        //=============================================================================
//        //   main_SaveMemberBase()
//        //       Saves the current Member record to the database
//        //=============================================================================
//        //
//        public void saveMemberBase()
//        {
//            try
//            {
//                string SQL = null;
//                //
//                if (cpCore.visit_initialized)
//                {
//                    if ((id > 0))
//                    {
//                        SQL = "UPDATE ccMembers SET " + " Name=" + cpCore.db.encodeSQLText(name) + ",username=" + cpCore.db.encodeSQLText(username) + ",email=" + cpCore.db.encodeSQLText(email) + ",password=" + cpCore.db.encodeSQLText(password) + ",OrganizationID=" + cpCore.db.encodeSQLNumber(organizationId) + ",LanguageID=" + cpCore.db.encodeSQLNumber(languageId) + ",Active=" + cpCore.db.encodeSQLBoolean(active) + ",Company=" + cpCore.db.encodeSQLText(company) + ",Visits=" + cpCore.db.encodeSQLNumber(visits) + ",LastVisit=" + cpCore.db.encodeSQLDate(lastVisit) + ",AllowBulkEmail=" + cpCore.db.encodeSQLBoolean(allowBulkEmail) + ",AllowToolsPanel=" + cpCore.db.encodeSQLBoolean(allowToolsPanel) + ",AdminMenuModeID=" + cpCore.db.encodeSQLNumber(adminMenuModeID) + ",AutoLogin=" + cpCore.db.encodeSQLBoolean(autoLogin);
//                        SQL += ",ExcludeFromAnalytics=" + cpCore.db.encodeSQLBoolean(excludeFromAnalytics);
//                        SQL += " WHERE ID=" + id + ";";
//                        cpCore.db.executeSql(SQL);
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                cpCore.handleExceptionAndRethrow(ex);
//            }
//        }
//        //
//        //========================================================================
//        // Member Logout
//        //   Create and assign a guest Member identity
//        //========================================================================
//        //
//        public void logout()
//        {
//            try
//            {
//                cpCore.log_LogActivity2("logout", id, organizationId);
//                //
//                // Clear MemberID for this page
//                //
//                createUser();
//                //
//                // Clear cached permissions
//                //
//                isAuthenticatedAdmin_cache_isLoaded = false;
//                // true if main_IsAdminCache is initialized
//                property_user_isMember_isLoaded = false;
//                isAuthenticatedDeveloper_cache_isLoaded = false;
//            }
//            catch (Exception ex)
//            {
//                cpCore.handleExceptionAndRethrow(ex);
//            }
//        }
//        //
//        //========================================================================
//        // ----- Process the send password form
//        //========================================================================
//        //
//        public void processFormJoin()
//        {
//            try
//            {
//                string ErrorMessage = "";
//                int CS = 0;
//                string FirstName = null;
//                string LastName = null;
//                string FullName = null;
//                string Email = null;
//                int errorCode = 0;
//                //
//                loginForm_Username = cpCore.docProperties.getText("username");
//                loginForm_Password = cpCore.docProperties.getText("password");
//                //
//                if (!EncodeBoolean(cpCore.siteProperties.getBoolean("AllowMemberJoin", false)))
//                {
//                    cpCore.error_AddUserError("This site does not accept public main_MemberShip.");
//                }
//                else {
//                    if (!isNewLoginOK(loginForm_Username, loginForm_Password, ref ErrorMessage, ref errorCode))
//                    {
//                        cpCore.error_AddUserError(ErrorMessage);
//                    }
//                    else {
//                        if (!cpCore.error_IsUserError())
//                        {
//                            CS = cpCore.db.csOpen("people", "ID=" + cpCore.db.encodeSQLNumber(cpCore.user.id));
//                            if (!cpCore.db.cs_Ok(CS))
//                            {
//                                cpCore.handleExceptionAndRethrow(new Exception("Could not open the current members account to set the username and password."));
//                            }
//                            else {
//                                if ((!string.IsNullOrEmpty(cpCore.db.cs_getText(CS, "username"))) | (!string.IsNullOrEmpty(cpCore.db.cs_getText(CS, "password"))) | (cpCore.db.cs_getBoolean(CS, "admin")) | (cpCore.db.cs_getBoolean(CS, "developer")))
//                                {
//                                    //
//                                    // if the current account can be logged into, you can not join 'into' it
//                                    //
//                                    logout();
//                                }
//                                FirstName = cpCore.docProperties.getText("firstname");
//                                LastName = cpCore.docProperties.getText("firstname");
//                                FullName = FirstName + " " + LastName;
//                                Email = cpCore.docProperties.getText("email");
//                                cpCore.db.cs_set(CS, "FirstName", FirstName);
//                                cpCore.db.cs_set(CS, "LastName", LastName);
//                                cpCore.db.cs_set(CS, "Name", FullName);
//                                cpCore.db.cs_set(CS, "username", loginForm_Username);
//                                cpCore.db.cs_set(CS, "password", loginForm_Password);
//                                cpCore.user.authenticateById(cpCore.user.id);
//                            }
//                            cpCore.db.cs_Close(ref CS);
//                        }
//                    }
//                }
//                cpCore.cache.invalidateTagCommaList("People");
//            }
//            catch (Exception ex)
//            {
//                cpCore.handleExceptionAndRethrow(ex);
//            }
//        }
//        //
//        //========================================================================
//        //   Print the login form in an intercept page
//        //========================================================================
//        //
//        public string getLoginPage(bool forceDefaultLogin)
//        {
//            string returnREsult = "";
//            try
//            {
//                string Body = null;
//                string head = null;
//                string bodyTag = null;
//                //
//                // ----- Default Login
//                //
//                if (forceDefaultLogin)
//                {
//                    Body = getLoginForm_Default();
//                }
//                else {
//                    Body = getLoginForm();
//                }
//                Body = "" + cr + "<p class=\"ccAdminNormal\">You are attempting to enter an access controlled area. Continue only if you have authority to enter this area. Information about your visit will be recorded for security purposes.</p>" + Body + "";
//                //
//                Body = "" + cpCore.main_GetPanel(Body, "ccPanel", "ccPanelHilite", "ccPanelShadow", "400", 15) + cr + "<p>&nbsp;</p>" + cr + "<p>&nbsp;</p>" + cr + "<p style=\"text-align:center\"><a href=\"http://www.Contensive.com\" target=\"_blank\"><img src=\"/ccLib/images/ccLibLogin.GIF\" width=\"80\" height=\"33\" border=\"0\" alt=\"Contensive Content Control\" ></A></p>" + cr + "<p style=\"text-align:center\" class=\"ccAdminSmall\">The content on this web site is managed and delivered by the Contensive Site Management Server. If you do not have member access, please use your back button to return to the public area.</p>" + "";
//                //
//                // --- create an outer table to hold the form
//                //
//                Body = "" + cr + "<div class=\"ccCon\" style=\"width:400px;margin:100px auto 0 auto;\">" + kmaIndent(cpCore.main_GetPanelHeader("Login")) + kmaIndent(Body) + "</div>";
//                //
//                cpCore.main_SetMetaContent(0, 0);
//                cpCore.main_AddPagetitle2("Login", "loginPage");
//                head = cpCore.main_GetHTMLInternalHead(false);
//                if (!string.IsNullOrEmpty(cpCore.pageManager_TemplateBodyTag))
//                {
//                    bodyTag = cpCore.pageManager_TemplateBodyTag;
//                }
//                else {
//                    bodyTag = TemplateDefaultBodyTag;
//                }
//                //Call AppendLog("call main_getEndOfBody, from main_getLoginPage2 ")
//                returnREsult = cpCore.main_assembleHtmlDoc(cpCore.main_docType, head, bodyTag, Body + cpCore.main_GetEndOfBody(false, false, false, false));
//            }
//            catch (Exception ex)
//            {
//                cpCore.handleExceptionAndRethrow(ex);
//            }
//            return returnREsult;
//        }
//        //
//        //========================================================================
//        //   default login form
//        //========================================================================
//        //
//        public string getLoginForm_Default()
//        {
//            string returnHtml = "";
//            try
//            {
//                string Panel = null;
//                string usernameMsg = null;
//                string QueryString = null;
//                string loginForm = null;
//                string Caption = null;
//                string formType = null;
//                bool needLoginForm = false;
//                //
//                // ----- process the previous form, if login OK, return blank (signal for page refresh)
//                //
//                needLoginForm = true;
//                formType = cpCore.docProperties.getText("type");
//                if (formType == FormTypeLogin)
//                {
//                    if (processFormLoginDefault())
//                    {
//                        returnHtml = "";
//                        needLoginForm = false;
//                    }
//                }
//                if (needLoginForm)
//                {
//                    //
//                    // ----- When page loads, set focus on login username
//                    //
//                    cpCore.web_addRefreshQueryString("method", "");
//                    loginForm = "";
//                    cpCore.main_AddOnLoadJavascript2("document.getElementById('LoginUsernameInput').focus()", "login");
//                    //
//                    // ----- Error Messages
//                    //
//                    if (EncodeBoolean(cpCore.siteProperties.getBoolean("allowEmailLogin", false)))
//                    {
//                        usernameMsg = "<b>To login, enter your username or email address with your password.</b></p>";
//                    }
//                    else {
//                        usernameMsg = "<b>To login, enter your username and password.</b></p>";
//                    }
//                    //
//                    QueryString = cpCore.webServer.requestQueryString;
//                    QueryString = ModifyQueryString(QueryString, RequestNameHardCodedPage, "", false);
//                    QueryString = ModifyQueryString(QueryString, "requestbinary", "", false);
//                    //
//                    // ----- Username
//                    //
//                    if (EncodeBoolean(cpCore.siteProperties.getBoolean("allowEmailLogin", false)))
//                    {
//                        Caption = "Username&nbsp;or&nbsp;Email";
//                    }
//                    else {
//                        Caption = "Username";
//                    }
//                    //
//                    loginForm = loginForm + cr + "<tr>" + cr2 + "<td style=\"text-align:right;vertical-align:middle;width:30%;padding:4px\" align=\"right\" width=\"30%\">" + SpanClassAdminNormal + Caption + "&nbsp;</span></td>" + cr2 + "<td style=\"text-align:left;vertical-align:middle;width:70%;padding:4px\" align=\"left\"  width=\"70%\"><input ID=\"LoginUsernameInput\" NAME=\"" + "username\" VALUE=\"" + cpcore.html.html_EncodeHTML(loginForm_Username) + "\" SIZE=\"20\" MAXLENGTH=\"50\" ></td>" + cr + "</tr>";
//                    //
//                    // ----- Password
//                    //
//                    if (EncodeBoolean(cpCore.siteProperties.getBoolean("allowNoPasswordLogin", false)))
//                    {
//                        Caption = "Password&nbsp;(optional)";
//                    }
//                    else {
//                        Caption = "Password";
//                    }
//                    loginForm = loginForm + cr + "<tr>" + cr2 + "<td style=\"text-align:right;vertical-align:middle;width:30%;padding:4px\" align=\"right\">" + SpanClassAdminNormal + Caption + "&nbsp;</span></td>" + cr2 + "<td style=\"text-align:left;vertical-align:middle;width:70%;padding:4px\" align=\"left\" ><input NAME=\"" + "password\" VALUE=\"\" SIZE=\"20\" MAXLENGTH=\"50\" type=\"password\"></td>" + cr + "</tr>" + "";
//                    //
//                    // ----- autologin support
//                    //
//                    if (EncodeBoolean(cpCore.siteProperties.getBoolean("AllowAutoLogin", false)))
//                    {
//                        loginForm = loginForm + cr + "<tr>" + cr2 + "<td align=\"right\">&nbsp;</td>" + cr2 + "<td align=\"left\" >" + cr3 + "<table border=\"0\" cellpadding=\"5\" cellspacing=\"0\" width=\"100%\">" + cr4 + "<tr>" + cr5 + "<td valign=\"top\" width=\"20\"><input type=\"checkbox\" name=\"" + "autologin\" value=\"ON\" checked></td>" + cr5 + "<td valign=\"top\" width=\"100%\">" + SpanClassAdminNormal + "Login automatically from this computer</span></td>" + cr4 + "</tr>" + cr3 + "</table>" + cr2 + "</td>" + cr + "</tr>";
//                    }
//                    loginForm = loginForm + cr + "<tr>" + cr2 + "<td colspan=\"2\">&nbsp;</td>" + cr + "</tr>" + "";
//                    loginForm = "" + cr + "<table border=\"0\" cellpadding=\"5\" cellspacing=\"0\" width=\"100%\">" + kmaIndent(loginForm) + cr + "</table>" + "";
//                    loginForm = loginForm + cpCore.html_GetFormInputHidden("Type", FormTypeLogin) + cpCore.html_GetFormInputHidden("email", loginForm_Email) + cpCore.main_GetPanelButtons(ButtonLogin, "Button") + "";
//                    loginForm = "" + cpCore.html_GetFormStart(QueryString) + kmaIndent(loginForm) + cr + "</form>" + "";

//                    //-------

//                    Panel = "" + cpCore.error_GetUserError() + cr + "<p class=\"ccAdminNormal\">" + usernameMsg + loginForm + "";
//                    //
//                    // ----- Password Form
//                    //
//                    if (EncodeBoolean(cpCore.siteProperties.getBoolean("allowPasswordEmail", true)))
//                    {
//                        Panel = "" + Panel + cr + "<p class=\"ccAdminNormal\"><b>Forget your password?</b></p>" + cr + "<p class=\"ccAdminNormal\">If you are a member of the system and can not remember your password, enter your email address below and we will email your matching username and password.</p>" + getSendPasswordForm() + "";
//                    }
//                    //
//                    returnHtml = "" + cr + "<div class=\"ccLoginFormCon\">" + kmaIndent(Panel) + cr + "</div>" + "";
//                }
//            }
//            catch (Exception ex)
//            {
//                cpCore.handleExceptionAndRethrow(ex);
//            }
//            return returnHtml;
//        }
//        //
//        //========================================================================
//        //   same as main_GetLoginForm
//        //========================================================================
//        //
//        public string getLoginPanel()
//        {
//            return getLoginForm();
//        }
//        //'
//        //'========================================================================
//        //'   Member Check
//        //'       Check for visit authentication.
//        //'       If the visit is not authenticated (logged in with username/password),
//        //'       block the page with the login form (not a loginpage so there is no <html><body>
//        //'========================================================================
//        //'
//        //Public Sub user_CheckMember()
//        //    Try
//        //        If Not isAuthenticated() Then
//        //            Call cpCore.writeAltBuffer(user_GetLoginPage2(False))
//        //            Call cpCore.main_CloseStream()
//        //        End If
//        //    Catch ex As Exception
//        //        cpCore.handleExceptionAndRethrow(ex)
//        //    End Try
//        //End Sub
//        //
//        //===================================================================================================
//        //   Returns the ID of a member given their Username and Password
//        //
//        //   If the Id can not be found, user errors are added with main_AddUserError and 0 is returned (false)
//        //===================================================================================================
//        //
//        public int authenticateGetId(string username, string password)
//        {
//            int returnUserId = 0;
//            try
//            {
//                const string badLoginUserError = "Your login was not successful. Please try again.";
//                //
//                string SQL = null;
//                bool recordIsAdmin = false;
//                bool recordIsDeveloper = false;
//                string Criteria = null;
//                int CS = 0;
//                string iPassword = null;
//                bool allowEmailLogin = false;
//                bool allowNoPasswordLogin = false;
//                string iLoginFieldValue = null;
//                //
//                iLoginFieldValue = EncodeText(username);
//                iPassword = EncodeText(password);
//                //
//                returnUserId = 0;
//                allowEmailLogin = cpCore.siteProperties.getBoolean("allowEmailLogin");
//                allowNoPasswordLogin = cpCore.siteProperties.getBoolean("allowNoPasswordLogin");
//                if (string.IsNullOrEmpty(iLoginFieldValue))
//                {
//                    //
//                    // ----- loginFieldValue blank, stop here
//                    //
//                    if (allowEmailLogin)
//                    {
//                        cpCore.error_AddUserError("A valid login requires a non-blank username or email.");
//                    }
//                    else {
//                        cpCore.error_AddUserError("A valid login requires a non-blank username.");
//                    }
//                }
//                else if ((!allowNoPasswordLogin) & (string.IsNullOrEmpty(iPassword)))
//                {
//                    //
//                    // ----- password blank, stop here
//                    //
//                    cpCore.error_AddUserError("A valid login requires a non-blank password.");
//                }
//                else if ((cpCore.visit_loginAttempts >= main_maxVisitLoginAttempts))
//                {
//                    //
//                    // ----- already tried 5 times
//                    //
//                    cpCore.error_AddUserError(badLoginUserError);
//                }
//                else {
//                    if (allowEmailLogin)
//                    {
//                        //
//                        // login by username or email
//                        //
//                        Criteria = "((username=" + cpCore.db.encodeSQLText(iLoginFieldValue) + ")or(email=" + cpCore.db.encodeSQLText(iLoginFieldValue) + "))";
//                    }
//                    else {
//                        //
//                        // login by username only
//                        //
//                        Criteria = "(username=" + cpCore.db.encodeSQLText(iLoginFieldValue) + ")";
//                    }
//                    if (true)
//                    {
//                        Criteria = Criteria + "and((dateExpires is null)or(dateExpires>" + cpCore.db.encodeSQLDate(DateTime.Now) + "))";
//                    }
//                    CS = cpCore.db.csOpen("People", Criteria, "id", SelectFieldList: "ID ,password,admin,developer", PageSize: 2);
//                    if (!cpCore.db.cs_Ok(CS))
//                    {
//                        //
//                        // ----- loginFieldValue not found, stop here
//                        //
//                        cpCore.error_AddUserError(badLoginUserError);
//                    }
//                    else if ((!EncodeBoolean(cpCore.siteProperties.getBoolean("AllowDuplicateUsernames", false))) & (cpCore.db.cs_getRowCount(CS) > 1))
//                    {
//                        //
//                        // ----- AllowDuplicates is false, and there are more then one record
//                        //
//                        cpCore.error_AddUserError("This user account can not be used because the username is not unique on this website. Please contact the site administrator.");
//                    }
//                    else {
//                        //
//                        // ----- search all found records for the correct password
//                        //
//                        while (cpCore.db.cs_Ok(CS))
//                        {
//                            returnUserId = 0;
//                            //
//                            // main_Get Id if password good
//                            //
//                            if ((string.IsNullOrEmpty(iPassword)))
//                            {
//                                //
//                                // no-password-login -- allowNoPassword + no password given + account has no password + account not admin/dev/cm
//                                //
//                                recordIsAdmin = cpCore.db.cs_getBoolean(CS, "admin");
//                                recordIsDeveloper = !cpCore.db.cs_getBoolean(CS, "admin");
//                                if (allowNoPasswordLogin & (string.IsNullOrEmpty(cpCore.db.cs_getText(CS, "password"))) & (!recordIsAdmin) & (recordIsDeveloper))
//                                {
//                                    returnUserId = cpCore.db.cs_getInteger(CS, "ID");
//                                    //
//                                    // verify they are in no content manager groups
//                                    //
//                                    SQL = "SELECT ccGroupRules.ContentID" + " FROM ccGroupRules RIGHT JOIN ccMemberRules ON ccGroupRules.GroupID = ccMemberRules.GroupID" + " WHERE (" + "(ccMemberRules.MemberID=" + cpCore.db.encodeSQLNumber(returnUserId) + ")" + " AND(ccMemberRules.active<>0)" + " AND(ccGroupRules.active<>0)" + " AND(ccGroupRules.ContentID Is not Null)" + " AND((ccMemberRules.DateExpires is null)OR(ccMemberRules.DateExpires>" + cpCore.db.encodeSQLDate(cpCore.main_PageStartTime) + "))" + ");";
//                                    CS = cpCore.db.cs_openSql(SQL);
//                                    if (cpCore.db.cs_Ok(CS))
//                                    {
//                                        returnUserId = 0;
//                                    }
//                                    cpCore.db.cs_Close(ref CS);
//                                }
//                            }
//                            else {
//                                //
//                                // password login
//                                //
//                                if (vbLCase(cpCore.db.cs_getText(CS, "password")) == vbLCase(iPassword))
//                                {
//                                    returnUserId = cpCore.db.cs_getInteger(CS, "ID");
//                                }
//                            }
//                            if (returnUserId != 0)
//                            {
//                                break; // TODO: might not be correct. Was : Exit Do
//                            }
//                            cpCore.db.csGoNext(CS);
//                        }
//                        if (returnUserId == 0)
//                        {
//                            cpCore.error_AddUserError(badLoginUserError);
//                        }
//                    }
//                    cpCore.db.cs_Close(ref CS);
//                }
//            }
//            catch (Exception ex)
//            {
//                cpCore.handleExceptionAndRethrow(ex);
//            }
//            return returnUserId;
//        }
//        //
//        //====================================================================================================
//        //   Checks the username and password for a new login
//        //       returns true if this can be used
//        //       returns false, and a User Error response if it can not be used
//        //
//        public bool isNewLoginOK(string Username, string Password, ref string returnErrorMessage, ref int returnErrorCode)
//        {
//            bool returnOk = false;
//            try
//            {
//                int CSPointer = 0;
//                //
//                returnOk = false;
//                if (string.IsNullOrEmpty(Username))
//                {
//                    //
//                    // ----- username blank, stop here
//                    //
//                    returnErrorCode = 1;
//                    returnErrorMessage = "A valid login requires a non-blank username.";
//                }
//                else if (string.IsNullOrEmpty(Password))
//                {
//                    //
//                    // ----- password blank, stop here
//                    //
//                    returnErrorCode = 4;
//                    returnErrorMessage = "A valid login requires a non-blank password.";
//                    //    ElseIf Not main_VisitCookieSupport Then
//                    //        '
//                    //        ' No Cookie Support, can not log in
//                    //        '
//                    //        errorCode = 2
//                    //        errorMessage = "You currently have cookie support disabled in your browser. Without cookies, your browser can not support the level of security required to login."

//                }
//                else {
//                    CSPointer = cpCore.db.csOpen("People", "username=" + cpCore.db.encodeSQLText(Username), "", false, SelectFieldList: "ID", PageSize: 2);
//                    if (cpCore.db.cs_Ok(CSPointer))
//                    {
//                        //
//                        // ----- username was found, stop here
//                        //
//                        returnErrorCode = 3;
//                        returnErrorMessage = "The username you supplied is currently in use.";
//                    }
//                    else {
//                        returnOk = true;
//                    }
//                    cpCore.db.cs_Close(ref CSPointer);
//                }
//            }
//            catch (Exception ex)
//            {
//                cpCore.handleExceptionAndRethrow(ex);
//            }
//            return returnOk;
//        }
//        //
//        //====================================================================================================
//        // main_GetContentAccessRights( ContentIdOrName, returnAllowEdit, returnAllowAdd, returnAllowDelete )
//        //
//        public void getContentAccessRights(string ContentName, ref bool returnAllowEdit, ref bool returnAllowAdd, ref bool returnAllowDelete)
//        {
//            try
//            {
//                int ContentID = 0;
//                coreMetaDataClass.CDefClass CDef = default(coreMetaDataClass.CDefClass);
//                //
//                returnAllowEdit = false;
//                returnAllowAdd = false;
//                returnAllowDelete = false;
//                if (true)
//                {
//                    if (!isAuthenticated())
//                    {
//                        //
//                        // no authenticated, you are not a conent manager
//                        //
//                    }
//                    else if (string.IsNullOrEmpty(ContentName))
//                    {
//                        //
//                        // no content given, do not handle the general case -- use user.main_IsContentManager2()
//                        //
//                    }
//                    else if (isAuthenticatedDeveloper())
//                    {
//                        //
//                        // developers are always content managers
//                        //
//                        returnAllowEdit = true;
//                        returnAllowAdd = true;
//                        returnAllowDelete = true;
//                    }
//                    else if (isAuthenticatedAdmin())
//                    {
//                        //
//                        // admin is content manager if the CDef is not developer only
//                        //
//                        CDef = cpCore.metaData.getCdef(ContentName);
//                        if (CDef.Id != 0)
//                        {
//                            if (!CDef.DeveloperOnly)
//                            {
//                                returnAllowEdit = true;
//                                returnAllowAdd = true;
//                                returnAllowDelete = true;
//                            }
//                        }
//                    }
//                    else {
//                        //
//                        // Authenticated and not admin or developer
//                        //
//                        ContentID = cpCore.main_GetContentID(ContentName);
//                        getContentAccessRights_NonAdminByContentId(ContentID, ref returnAllowEdit, ref returnAllowAdd, ref returnAllowDelete, "");
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                cpCore.handleExceptionAndRethrow(ex);
//            }
//        }
//        //
//        //====================================================================================================
//        // main_GetContentAccessRights_NonAdminByContentId
//        //   Checks if the member is a content manager for the specific content,
//        //   Which includes transversing up the tree to find the next rule that applies'
//        //   Member must be checked for authenticated and main_IsAdmin already
//        //========================================================================
//        //
//        private void getContentAccessRights_NonAdminByContentId(int ContentID, ref bool returnAllowEdit, ref bool returnAllowAdd, ref bool returnAllowDelete, string usedContentIdList)
//        {
//            try
//            {
//                string SQL = null;
//                int CSPointer = 0;
//                int ParentID = 0;
//                string ContentName = null;
//                coreMetaDataClass.CDefClass CDef = default(coreMetaDataClass.CDefClass);
//                //
//                returnAllowEdit = false;
//                returnAllowAdd = false;
//                returnAllowDelete = false;
//                if (coreCommonModule.IsInDelimitedString(usedContentIdList, Convert.ToString(ContentID), ","))
//                {
//                    //
//                    // failed usedContentIdList test, this content id was in the child path
//                    //
//                    cpCore.handleExceptionAndRethrow(new ArgumentException("ContentID [" + ContentID + "] was found to be in it's own parentid path."));
//                }
//                else if (ContentID < 1)
//                {
//                    //
//                    // ----- not a valid contentname
//                    //
//                }
//                else if (coreCommonModule.IsInDelimitedString(contentAccessRights_NotList, Convert.ToString(ContentID), ","))
//                {
//                    //
//                    // ----- was previously found to not be a Content Manager
//                    //
//                }
//                else if (coreCommonModule.IsInDelimitedString(contentAccessRights_List, Convert.ToString(ContentID), ","))
//                {
//                    //
//                    // ----- was previously found to be a Content Manager
//                    //
//                    returnAllowEdit = true;
//                    returnAllowAdd = coreCommonModule.IsInDelimitedString(contentAccessRights_AllowAddList, Convert.ToString(ContentID), ",");
//                    returnAllowDelete = coreCommonModule.IsInDelimitedString(contentAccessRights_AllowDeleteList, Convert.ToString(ContentID), ",");
//                }
//                else {
//                    //
//                    // ----- Must test it
//                    //
//                    SQL = "SELECT ccGroupRules.ContentID,allowAdd,allowDelete" + " FROM ccGroupRules RIGHT JOIN ccMemberRules ON ccGroupRules.GroupID = ccMemberRules.GroupID" + " WHERE (" + " (ccMemberRules.MemberID=" + cpCore.db.encodeSQLNumber(id) + ")" + " AND(ccMemberRules.active<>0)" + " AND(ccGroupRules.active<>0)" + " AND(ccGroupRules.ContentID=" + ContentID + ")" + " AND((ccMemberRules.DateExpires is null)OR(ccMemberRules.DateExpires>" + cpCore.db.encodeSQLDate(cpCore.main_PageStartTime) + "))" + ");";
//                    CSPointer = cpCore.db.cs_openSql(SQL);
//                    if (cpCore.db.cs_Ok(CSPointer))
//                    {
//                        returnAllowEdit = true;
//                        returnAllowAdd = cpCore.db.cs_getBoolean(CSPointer, "allowAdd");
//                        returnAllowDelete = cpCore.db.cs_getBoolean(CSPointer, "allowDelete");
//                    }
//                    cpCore.db.cs_Close(ref CSPointer);
//                    //
//                    if (!returnAllowEdit)
//                    {
//                        //
//                        // ----- Not a content manager for this one, check the parent
//                        //
//                        ContentName = cpCore.metaData.getContentNameByID(ContentID);
//                        if (!string.IsNullOrEmpty(ContentName))
//                        {
//                            CDef = cpCore.metaData.getCdef(ContentName);
//                            ParentID = CDef.parentID;
//                            if (ParentID > 0)
//                            {
//                                getContentAccessRights_NonAdminByContentId(ParentID, ref returnAllowEdit, ref returnAllowAdd, ref returnAllowDelete, usedContentIdList + "," + Convert.ToString(ContentID));
//                            }
//                        }
//                    }
//                    if (returnAllowEdit)
//                    {
//                        //
//                        // ----- Was found to be true
//                        //
//                        contentAccessRights_List += "," + Convert.ToString(ContentID);
//                        if (returnAllowAdd)
//                        {
//                            contentAccessRights_AllowAddList += "," + Convert.ToString(ContentID);
//                        }
//                        if (returnAllowDelete)
//                        {
//                            contentAccessRights_AllowDeleteList += "," + Convert.ToString(ContentID);
//                        }
//                    }
//                    else {
//                        //
//                        // ----- Was found to be false
//                        //
//                        contentAccessRights_NotList += "," + Convert.ToString(ContentID);
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                cpCore.handleExceptionAndRethrow(ex);
//            }
//        }
//        //
//    }
//}

////=======================================================
////Service provided by Telerik (www.telerik.com)
////Conversion powered by NRefactory.
////Twitter: @telerik
////Facebook: facebook.com/telerik
////=======================================================
