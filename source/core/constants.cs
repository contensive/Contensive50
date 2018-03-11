
using System;
using System.Collections.Generic;

namespace Contensive.Core {
    public static class constants {
        //
        // code version for this build. This is saved in a site property and checked in the housekeeping event - checkDataVersion
        //
        public const int codeVersion = 0;
        //
        // -- content names
        public const string cnBlank = "";
        public const string cnDataSources = "data sources";
        public const string cnPeople = "people";
        public const string cnAddons = "Add-ons";
        public const string cnNavigatorEntries = "Navigator Entries";
        //
        public const string requestAppRootPath = "/";
        //
        // -- buttons
        public const string ButtonCreateFields = " Create Fields ";
        public const string ButtonRun = "     Run     ";
        public const string ButtonSelect = "  Select ";
        public const string ButtonFindAndReplace = " Find and Replace ";
        public const string ButtonIISReset = " IIS Reset ";
        public const string ButtonCancel = " Cancel ";
        //
        public const string protectedContentSetControlFieldList = "ID,CREATEDBY,DATEADDED,MODIFIEDBY,MODIFIEDDATE,CONTENTCONTROLID";
        //
        public const string HTMLEditorDefaultCopyStartMark = "<!-- cc -->";
        public const string HTMLEditorDefaultCopyEndMark = "<!-- /cc -->";
        public const string HTMLEditorDefaultCopyNoCr = HTMLEditorDefaultCopyStartMark + "<p><br></p>" + HTMLEditorDefaultCopyEndMark;
        public const string HTMLEditorDefaultCopyNoCr2 = "<p><br></p>";
        //
        public const string IconWidthHeight = " width=21 height=22 ";
        //
        public const string baseCollectionGuid = "{7C6601A7-9D52-40A3-9570-774D0D43D758}"; // part of software dist - base cdef plus addons with classes in in core library, plus depenancy on coreCollection
        public const string CoreCollectionGuid = "{8DAABAE6-8E45-4CEE-A42C-B02D180E799B}"; // contains core Contensive objects, loaded from Library
        public const string ApplicationCollectionGuid = "{C58A76E2-248B-4DE8-BF9C-849A960F79C6}"; // exported from application during upgrade
                                                                                                  //
                                                                                                  // -- navigator entries
        public const string addonGuidManageAddon = "{DBA354AB-5D3E-4882-8718-CF23CAAB7927}";
        //
        // -- addons
        public const string addonGuidHousekeep = "{7208D069-8FE3-4BD1-AB76-B25C40C89A45}";
        public const string addonGuidBaseStlyles = "{0dd7df28-4924-4881-a1d8-421824f5c2d1}";
        public const string addonGuidAdminSite = "{c2de2acf-ca39-4668-b417-aa491e7d8460}";
        public const string addonGuidDashboard = "{4BA7B4A2-ED6C-46C5-9C7B-8CE251FC8FF5}";
        public const string addonGuidPersonalization = "{C82CB8A6-D7B9-4288-97FF-934080F5FC9C}";
        public const string addonGuidTextBox = "{7010002E-5371-41F7-9C77-0BBFF1F8B728}";
        public const string addonGuidContentBox = "{E341695F-C444-4E10-9295-9BEEC41874D8}";
        public const string addonGuidDynamicMenu = "{DB1821B3-F6E4-4766-A46E-48CA6C9E4C6E}";
        public const string addonGuidChildList = "{D291F133-AB50-4640-9A9A-18DB68FF363B}";
        public const string addonGuidDynamicForm = "{8284FA0C-6C9D-43E1-9E57-8E9DD35D2DCC}";
        public const string addonGuidAddonManager = "{1DC06F61-1837-419B-AF36-D5CC41E1C9FD}";
        public const string addonGuidFormWizard = "{2B1384C4-FD0E-4893-B3EA-11C48429382F}";
        public const string addonGuidImportWizard = "{37F66F90-C0E0-4EAF-84B1-53E90A5B3B3F}";
        public const string addonGuidJQuery = "{9C882078-0DAC-48E3-AD4B-CF2AA230DF80}";
        public const string addonGuidJQueryUI = "{840B9AEF-9470-4599-BD47-7EC0C9298614}";
        public const string addonGuidImportProcess = "{5254FAC6-A7A6-4199-8599-0777CC014A13}";
        public const string addonGuidStructuredDataProcessor = "{65D58FE9-8B76-4490-A2BE-C863B372A6A4}";
        public const string addonGuidjQueryFancyBox = "{24C2DBCF-3D84-44B6-A5F7-C2DE7EFCCE3D}";
        public const string addonGuidSiteStructureGuid = "{8CDD7960-0FCA-4042-B5D8-3A65BE487AC4}";
        //Public Const addonGuidLoginDefaultPage   As String  = "{288a7ee1-9d93-4058-bcd9-c9cd29d25ec8}"
        // -- Login Page displays the currently selected login form addon
        public const string addonGuidLoginPage = "{288a7ee1-9d93-4058-bcd9-c9cd29d25ec8}";
        // -- Login Form, this is the addonGuid of the default login form. Login Page calls the addon
        public const string addonGuidLoginForm = "{E23C5941-19C2-4164-BCFD-83D6DD42F651}";
        public const string addonGuidPageManager = "{3a01572e-0f08-4feb-b189-18371752a3c3}";
        public const string addonGuidExportCSV = "{5C25F35D-A2A8-4791-B510-B1FFE0645004}";
        public const string addonGuidExportXML = "{DC7EF1EE-20EE-4468-BEB1-0DC30AC8DAD6}";
        //
        // -- content
        public const string DefaultLandingPageGuid = "{925F4A57-32F7-44D9-9027-A91EF966FB0D}";
        public const string DefaultLandingSectionGuid = "{D882ED77-DB8F-4183-B12C-F83BD616E2E1}";
        public const string DefaultTemplateGuid = "{47BE95E4-5D21-42CC-9193-A343241E2513}";
        public const string DefaultDynamicMenuGuid = "{E8D575B9-54AE-4BF9-93B7-C7E7FE6F2DB3}";
        //
        // -- instance id used when running addons in the addon site 
        public const string adminSiteInstanceId = "{E5418109-1206-43C5-A4F8-425E28BC629C}";
        //
        public const string fpoContentBox = "{1571E62A-972A-4BFF-A161-5F6075720791}";
        //
        public const string sfImageExtList = "jpg,jpeg,gif,png";
        //
        public const string PageChildListInstanceID = "{ChildPageList}";
        //
        public const string unixNewLine = "\n";
        public const string macNewLine = "\r";
        public const string windowsNewLine = "\r\n";
        //
        public const string cr = "\r\n\t";
        public const string cr2 = cr + "\t";
        public const string cr3 = cr2 + "\t";
        public const string cr4 = cr3 + "\t";
        public const string cr5 = cr4 + "\t";
        public const string cr6 = cr5 + "\t";
        //
        public const string AddonOptionConstructor_BlockNoAjax = "Wrapper=[Default:0|None:-1|ListID(Wrappers)]\r\ncss Container id\r\ncss Container class";
        public const string AddonOptionConstructor_Block = "Wrapper=[Default:0|None:-1|ListID(Wrappers)]\r\nAs Ajax=[If Add-on is Ajax:0|Yes:1]\r\ncss Container id\r\ncss Container class";
        public const string AddonOptionConstructor_Inline = "As Ajax=[If Add-on is Ajax:0|Yes:1]\r\ncss Container id\r\ncss Container class";
        //
        // Constants used as arguments to SiteBuilderClass.CreateNewSite
        //
        public const int SiteTypeBaseAsp = 1;
        public const int sitetypebaseaspx = 2;
        public const int SiteTypeDemoAsp = 3;
        public const int SiteTypeBasePhp = 4;
        //
        //Public Const AddonNewParse = True
        //
        public const string AddonOptionConstructor_ForBlockText = "AllowGroups=[listid(groups)]checkbox";
        public const string AddonOptionConstructor_ForBlockTextEnd = "";
        public const string BlockTextStartMarker = "<!-- BLOCKTEXTSTART -->";
        public const string BlockTextEndMarker = "<!-- BLOCKTEXTEND -->";
        //
        //Private Declare Sub Sleep Lib "kernel32" (ByVal dwMilliseconds As Integer)
        //Private Declare Function GetExitCodeProcess Lib "kernel32" (ByVal hProcess As Integer, ByVal lpExitCode As Integer) As Integer
        //Private Declare Function timeGetTime Lib "winmm.dll" () As Integer
        //Private Declare Function OpenProcess Lib "kernel32" (ByVal dwDesiredAccess As Integer, ByVal bInheritHandle As Integer, ByVal dwProcessId As Integer) As Integer
        //Private Declare Function CloseHandle Lib "kernel32" (ByVal hObject As Integer) As Integer
        //
        public const string InstallFolderName = "Install";
        public const string DownloadFileRootNode = "collectiondownload";
        public const string CollectionFileRootNode = "collection";
        public const string CollectionFileRootNodeOld = "addoncollection";
        public const string CollectionListRootNode = "collectionlist";
        //
        public const string LegacyLandingPageName = "Landing Page Content";
        public const string DefaultNewLandingPageName = "Home";
        public const string DefaultLandingSectionName = "Home";
        //
        public const string defaultLandingPageHtml = "";
        public const string defaultTemplateName = "Default";
        //Public Const defaultTemplateHtml As String = "{% {""addon"":{""addon"":""menu pages"",""name"":""Default""}} %}{% ""Content Box"" %}"
        public const string defaultTemplateHomeFilename = "ContensiveBase\\TemplateHomeDefault.html";
        //
        //
        //
        public const int ignoreInteger = 0;
        public const string ignoreString = "";
        //
        // ----- Errors Specific to the Contensive Objects
        //
        //Public Const ignoreInteger = KmaObjectError + 1
        //Public Const KmaccErrorServiceStopped = KmaObjectError + 2
        //
        public const string UserErrorHeadline = "<p class=\"ccError\">There was a problem with this page.</p>";
        //
        // ----- Errors connecting to server
        //
        //Public Const ccError_InvalidAppName = 100
        //Public Const ccError_ErrorAddingApp = 101
        //Public Const ccError_ErrorDeletingApp = 102
        //Public Const ccError_InvalidFieldName = 103     ' Invalid parameter name
        //Public Const ignoreString = 104
        //Public Const ignoreString = 105
        //Public Const ccError_NotConnected = 106             ' Attempt to execute a command without a connection
        //
        //
        //
        //Public Const ccStatusCode_Base = ignoreInteger
        //Public Const ccStatusCode_ControllerCreateFailed = ccStatusCode_Base + 1
        //Public Const ccStatusCode_ControllerInProcess = ccStatusCode_Base + 2
        //Public Const ccStatusCode_ControllerStartedWithoutService = ccStatusCode_Base + 3
        //
        // ----- Previous errors, can be replaced
        //
        //Public Const KmaError_UnderlyingObject_Msg  As String  = "An error occurred in an underlying routine."
        //Public Const KmaccErrorServiceStopped_Msg  As String  = "The Contensive CSv Service is not running."
        //Public Const KmaError_BadObject_Msg  As String  = "Server Object is not valid."
        //Public Const ignoreString  As String  = "Server is busy with internal builder."
        //
        //Public Const KmaError_InvalidArgument_Msg  As String  = "Invalid Argument"
        //Public Const KmaError_UnderlyingObject_Msg  As String  = "An error occurred in an underlying routine."
        //Public Const KmaccErrorServiceStopped_Msg  As String  = "The Contensive CSv Service is not running."
        //Public Const KmaError_BadObject_Msg  As String  = "Server Object is not valid."
        //Public Const ignoreString  As String  = "Server is busy with internal builder."
        //Public Const KmaError_InvalidArgument_Msg  As String  = "Invalid Argument"
        //
        //-----------------------------------------------------------------------
        //   GetApplicationList indexes
        //-----------------------------------------------------------------------
        //
        public const int AppList_Name = 0;
        public const int AppList_Status = 1;
        public const int AppList_ConnectionsActive = 2;
        public const int AppList_ConnectionString = 3;
        public const int AppList_DataBuildVersion = 4;
        public const int AppList_LicenseKey = 5;
        public const int AppList_RootPath = 6;
        public const int AppList_PhysicalFilePath = 7;
        public const int AppList_DomainName = 8;
        public const int AppList_DefaultPage = 9;
        public const int AppList_AllowSiteMonitor = 10;
        public const int AppList_HitCounter = 11;
        public const int AppList_ErrorCount = 12;
        public const int AppList_DateStarted = 13;
        public const int AppList_AutoStart = 14;
        public const int AppList_Progress = 15;
        public const int AppList_PhysicalWWWPath = 16;
        public const int AppListCount = 17;
        //
        //-----------------------------------------------------------------------
        //   System MemberID - when the system does an update, it uses this member
        //-----------------------------------------------------------------------
        //
        public const int SystemMemberID = 0;
        //
        //-----------------------------------------------------------------------
        // ----- old (OptionKeys for available Options)
        //-----------------------------------------------------------------------
        //
        public const int OptionKeyProductionLicense = 0;
        public const int OptionKeyDeveloperLicense = 1;
        //
        //-----------------------------------------------------------------------
        // ----- LicenseTypes, replaced OptionKeys
        //-----------------------------------------------------------------------
        //
        public const int LicenseTypeInvalid = -1;
        public const int LicenseTypeProduction = 0;
        public const int LicenseTypeTrial = 1;
        //
        //-----------------------------------------------------------------------
        // ----- Active Content Definitions
        //-----------------------------------------------------------------------
        //
        public const string ACTypeDate = "DATE";
        public const string ACTypeVisit = "VISIT";
        public const string ACTypeVisitor = "VISITOR";
        public const string ACTypeMember = "MEMBER";
        public const string ACTypeOrganization = "ORGANIZATION";
        public const string ACTypeChildList = "CHILDLIST";
        public const string ACTypeContact = "CONTACT";
        public const string ACTypeFeedback = "FEEDBACK";
        public const string ACTypeLanguage = "LANGUAGE";
        public const string ACTypeAggregateFunction = "AGGREGATEFUNCTION";
        public const string ACTypeAddon = "ADDON";
        public const string ACTypeImage = "IMAGE";
        public const string ACTypeDownload = "DOWNLOAD";
        public const string ACTypeEnd = "END";
        public const string ACTypeTemplateContent = "CONTENT";
        public const string ACTypeTemplateText = "TEXT";
        //Public Const ACTypeDynamicMenu  As String  = "DYNAMICMENU"
        public const string ACTypeWatchList = "WATCHLIST";
        public const string ACTypeRSSLink = "RSSLINK";
        public const string ACTypePersonalization = "PERSONALIZATION";
        public const string ACTypeDynamicForm = "DYNAMICFORM";
        //
        public const string ACTagEnd = "<ac type=\"" + ACTypeEnd + "\">";
        //
        // ----- PropertyType Definitions
        //
        public const int PropertyTypeMember = 0;
        public const int PropertyTypeVisit = 1;
        public const int PropertyTypeVisitor = 2;
        //
        //-----------------------------------------------------------------------
        // ----- Port Assignments
        //-----------------------------------------------------------------------
        //
        public const int WinsockPortWebOut = 4000;
        public const int WinsockPortServerFromWeb = 4001;
        public const int WinsockPortServerToClient = 4002;
        //
        public const int Port_ContentServerControlDefault = 4531;
        public const int Port_SiteMonitorDefault = 4532;
        //
        public const int RMBMethodHandShake = 1;
        public const int RMBMethodMessage = 3;
        public const int RMBMethodTestPoint = 4;
        public const int RMBMethodInit = 5;
        public const int RMBMethodClosePage = 6;
        public const int RMBMethodOpenCSContent = 7;
        //
        // ----- Position equates for the Remote Method Block
        //
        public const int RMBPositionLength = 0; // Length of the RMB
        public const int RMBPositionSourceHandle = 4; // Handle generated by the source of the command
        public const int RMBPositionMethod = 8; // Method in the method block
        public const int RMBPositionArgumentCount = 12; // The number of arguments in the Block
        public const int RMBPositionFirstArgument = 16;
        //
        // -- Default username/password
        //
        public const string defaultRootUserName = "root";
        public const string defaultRootUserUsername = "root";
        public const string defaultRootUserPassword = "contensive";
        public const string defaultRootUserGuid = "{4445cd14-904f-480f-a7b7-29d70d0c22ca}";
        //
        // -- Default site manage group
        //
        public const string defaultSiteManagerName = "root";
        public const string defaultSiteManagerGuid = "{0685bd36-fe24-4542-be42-27337af50da8}";
        //
        // -- Request Names
        //
        public const string rnRedirectContentId = "rc";
        public const string rnRedirectRecordId = "ri";
        public const string rnPageId = "bid";
        //
        //-----------------------------------------------------------------------
        //   Form Contension Strategy
        //        '       elements of the form are named  "ElementName"
        //
        //       This prevents form elements from different forms from interfearing
        //       with each other, and with developer generated forms.
        //
        //       All forms requires:
        //           a FormId (text), containing the formid string
        //           a [formid]Type (text), as defined in FormTypexxx in CommonModule
        //
        //       Forms have two primary sections: GetForm and ProcessForm
        //
        //       Any form that has a GetForm method, should have the process form
        //       in the cmc.main_init, selected with this [formid]type hidden (not the
        //       GetForm method). This is so the process can alter the stream
        //       output for areas before the GetForm call.
        //
        //       System forms, like tools panel, that may appear on any page, have
        //       their process call in the cmc.main_init.
        //
        //       Popup forms, like ImageSelector have their processform call in the
        //       cmc.main_init because no .asp page exists that might contain a call
        //       the process section.
        //
        //-----------------------------------------------------------------------
        //
        public const string FormTypeToolsPanel = "do30a8vl29";
        public const string FormTypeActiveEditor = "l1gk70al9n";
        public const string FormTypeImageSelector = "ila9c5s01m";
        public const string FormTypePageAuthoring = "2s09lmpalb";
        public const string FormTypeMyProfile = "89aLi180j5";
        public const string FormTypeLogin = "login";
        //Public Const FormTypeLogin As String = "l09H58a195"
        public const string FormTypeSendPassword = "lk0q56am09";
        public const string FormTypeJoin = "6df38abv00";
        public const string FormTypeHelpBubbleEditor = "9df019d77sA";
        public const string FormTypeAddonSettingsEditor = "4ed923aFGw9d";
        public const string FormTypeAddonStyleEditor = "ar5028jklkfd0s";
        public const string FormTypeSiteStyleEditor = "fjkq4w8794kdvse";
        //Public Const FormTypeAggregateFunctionProperties As String = "9wI751270"
        //
        //-----------------------------------------------------------------------
        //   Hardcoded profile form const
        //-----------------------------------------------------------------------
        //
        public const string rnMyProfileTopics = "profileTopics";
        //
        //-----------------------------------------------------------------------
        // Legacy - replaced with HardCodedPages
        //   Intercept Page Strategy
        //
        //       RequestnameInterceptpage = InterceptPage number from the input stream
        //       InterceptPage = Global variant with RequestnameInterceptpage value read during early Init
        //
        //       Intercept pages are complete pages that appear instead of what
        //       the physical page calls.
        //-----------------------------------------------------------------------
        //
        //Public Const RequestNameInterceptpage As String = "ccIPage"
        //
        public const string LegacyInterceptPageSNResourceLibrary = "s033l8dm15";
        public const string LegacyInterceptPageSNSiteExplorer = "kdif3318sd";
        public const string LegacyInterceptPageSNImageUpload = "ka983lm039";
        public const string LegacyInterceptPageSNMyProfile = "k09ddk9105";
        public const string LegacyInterceptPageSNLogin = "6ge42an09a";
        public const string LegacyInterceptPageSNPrinterVersion = "l6d09a10sP";
        public const string LegacyInterceptPageSNUploadEditor = "k0hxp2aiOZ";
        //
        //-----------------------------------------------------------------------
        // Ajax functions intercepted during init, answered and response closed
        // todo - convert built-in request name functions to remoteMethods
        //   These are hard-coded internal Contensive functions
        //   These should eventually be replaced with (HardcodedAddons) remote methods
        //   They should all be prefixed "cc"
        //   They are called with cj.ajax.qs(), setting RequestNameAjaxFunction=name in the qs
        //   These name=value pairs go in the QueryString argument of the javascript cj.ajax.qs() function
        //-----------------------------------------------------------------------
        //
        //Public Const RequestNameOpenSettingPage As String = "settingpageid"
        public const string RequestNameAjaxFunction = "ajaxfn";
        public const string RequestNameAjaxFastFunction = "ajaxfastfn";
        //
        public const string AjaxOpenAdminNav = "aps89102kd";
        public const string AjaxOpenAdminNavGetContent = "d8475jkdmfj2";
        public const string AjaxCloseAdminNav = "3857fdjdskf91";
        public const string AjaxAdminNavOpenNode = "8395j2hf6jdjf";
        public const string AjaxAdminNavOpenNodeGetContent = "eieofdwl34efvclaeoi234598";
        public const string AjaxAdminNavCloseNode = "w325gfd73fhdf4rgcvjk2";
        //
        public const string AjaxCloseIndexFilter = "k48smckdhorle0";
        public const string AjaxOpenIndexFilter = "Ls8jCDt87kpU45YH";
        public const string AjaxOpenIndexFilterGetContent = "llL98bbJQ38JC0KJm";
        public const string AjaxStyleEditorAddStyle = "ajaxstyleeditoradd";
        public const string AjaxPing = "ajaxalive";
        public const string AjaxGetFormEditTabContent = "ajaxgetformedittabcontent";
        public const string AjaxData = "data";
        public const string AjaxGetVisitProperty = "getvisitproperty";
        public const string AjaxSetVisitProperty = "setvisitproperty";
        public const string AjaxGetDefaultAddonOptionString = "ccGetDefaultAddonOptionString";
        public const string ajaxGetFieldEditorPreferenceForm = "ajaxgetfieldeditorpreference";
        //
        //-----------------------------------------------------------------------
        //
        // no - for now just use ajaxfn in the cj.ajax.qs call
        //   this is more work, and I do not see why it buys anything new or better
        //
        //   Hard-coded addons
        //       these are internal Contensive functions
        //       can be called with just /addonname?querystring
        //       call them with cj.ajax.addon() or cj.ajax.addonCallback()
        //       are first in the list of checks when a URL rewrite is detected in Init()
        //       should all be prefixed with 'cc'
        //-----------------------------------------------------------------------
        //
        //Public Const HardcodedAddonGetDefaultAddonOptionString As String = "ccGetDefaultAddonOptionString"
        //
        //-----------------------------------------------------------------------
        //   Remote Methods
        //       ?RemoteMethodAddon=string
        //       calls an addon (if marked to run as a remote method)
        //       blocks all other Contensive output (tools panel, javascript, etc)
        //-----------------------------------------------------------------------
        //
        public const string RequestNameRemoteMethodAddon = "remotemethodaddon";
        //
        //-----------------------------------------------------------------------
        //   Hard Coded Pages
        //       ?Method=string
        //       Querystring based so they can be added to URLs, preserving the current page for a return
        //       replaces output stream with html output
        //-----------------------------------------------------------------------
        //
        public const string RequestNameHardCodedPage = "method";
        //
        public const string HardCodedPageLogin = "login";
        public const string HardCodedPageLoginDefault = "logindefault";
        public const string HardCodedPageMyProfile = "myprofile";
        public const string HardCodedPagePrinterVersion = "printerversion";
        public const string HardCodedPageResourceLibrary = "resourcelibrary";
        public const string HardCodedPageLogoutLogin = "logoutlogin";
        public const string HardCodedPageLogout = "logout";
        public const string HardCodedPageSiteExplorer = "siteexplorer";
        //Public Const HardCodedPageForceMobile As String = "forcemobile"
        //Public Const HardCodedPageForceNonMobile As String = "forcenonmobile"
        public const string HardCodedPageNewOrder = "neworderpage";
        public const string HardCodedPageStatus = "status";
        //Public Const HardCodedPageGetJSPage As String = "getjspage"
        //Public Const HardCodedPageGetJSLogin As String = "getjslogin"
        public const string HardCodedPageRedirect = "redirect";
        public const string HardCodedPageExportAscii = "exportascii";
        //public const string HardCodedPagePayPalConfirm = "paypalconfirm";
        public const string HardCodedPageSendPassword = "sendpassword";
        //
        //-----------------------------------------------------------------------
        //   Option values
        //       does not effect output directly
        //-----------------------------------------------------------------------
        //
        public const string RequestNamePageOptions = "ccoptions";
        //
        public const string PageOptionForceMobile = "forcemobile";
        public const string PageOptionForceNonMobile = "forcenonmobile";
        public const string PageOptionLogout = "logout";
        public const string PageOptionPrinterVersion = "printerversion";
        //
        // convert to options later
        //
        public const string RequestNameDashboardReset = "ResetDashboard";
        //
        //-----------------------------------------------------------------------
        //   DataSource constants
        //-----------------------------------------------------------------------
        //
        public const int DefaultDataSourceID = -1;
        //
        //-----------------------------------------------------------------------
        // ----- Type compatibility between databases
        //       Boolean
        //           Access      YesNo       true=1, false=0
        //           SQL Server  bit         true=1, false=0
        //           MySQL       bit         true=1, false=0
        //           Oracle      integer(1)  true=1, false=0
        //           Note: false does not equal NOT true
        //       Integer (Number)
        //           Access      Long        8 bytes, about E308
        //           SQL Server  int
        //           MySQL       integer
        //           Oracle      integer(8)
        //       Float
        //           Access      Double      8 bytes, about E308
        //           SQL Server  Float
        //           MySQL
        //           Oracle
        //       Text
        //           Access
        //           SQL Server
        //           MySQL
        //           Oracle
        //-----------------------------------------------------------------------
        //
        //Public Const SQLFalse As String = "0"
        //Public Const SQLTrue As String = "1"
        //
        //-----------------------------------------------------------------------
        // ----- Style sheet definitions
        //-----------------------------------------------------------------------
        //
        public const string defaultStyleFilename = "ccDefault.r5.css";
        public const string StyleSheetStart = "<STYLE TYPE=\"text/css\">";
        public const string StyleSheetEnd = "</STYLE>";
        //
        public const string SpanClassAdminNormal = "<span class=\"ccAdminNormal\">";
        public const string SpanClassAdminSmall = "<span class=\"ccAdminSmall\">";
        //
        // remove these from ccWebx
        //
        public const string SpanClassNormal = "<span class=\"ccNormal\">";
        public const string SpanClassSmall = "<span class=\"ccSmall\">";
        public const string SpanClassLarge = "<span class=\"ccLarge\">";
        public const string SpanClassHeadline = "<span class=\"ccHeadline\">";
        public const string SpanClassList = "<span class=\"ccList\">";
        public const string SpanClassListCopy = "<span class=\"ccListCopy\">";
        public const string SpanClassError = "<span class=\"ccError\">";
        public const string SpanClassSeeAlso = "<span class=\"ccSeeAlso\">";
        public const string SpanClassEnd = "</span>";
        //
        //-----------------------------------------------------------------------
        // ----- XHTML definitions
        //-----------------------------------------------------------------------
        //
        public const string DTDTransitional = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">";
        //
        public const string BR = "<br>";
        //
        //-----------------------------------------------------------------------
        // AuthoringControl Types
        //-----------------------------------------------------------------------
        //
        public const int AuthoringControlsEditing = 1;
        public const int AuthoringControlsSubmitted = 2;
        public const int AuthoringControlsApproved = 3;
        public const int AuthoringControlsModified = 4;
        //
        //-----------------------------------------------------------------------
        // ----- Panel and header colors
        //-----------------------------------------------------------------------
        //
        //Public Const "ccPanel" As String = "#E0E0E0"    ' The background color of a panel (black copy visible on it)
        //Public Const "ccPanelHilite" As String = "#F8F8F8"  '
        //Public Const "ccPanelShadow" As String = "#808080"  '
        //
        //Public Const HeaderColorBase As String = "#0320B0"   ' The background color of a panel header (reverse copy visible)
        //Public Const "ccPanelHeaderHilite" As String = "#8080FF" '
        //Public Const "ccPanelHeaderShadow" As String = "#000000" '
        //
        //-----------------------------------------------------------------------
        // ----- Field type Definitions
        //       Field Types are numeric values that describe how to treat values
        //       stored as ContentFieldDefinitionType (FieldType property of FieldType Type.. ;)
        //-----------------------------------------------------------------------
        //
        public const int FieldTypeIdInteger = 1; // An long number
        public const int FieldTypeIdText = 2; // A text field (up to 255 characters)
        public const int FieldTypeIdLongText = 3; // A memo field (up to 8000 characters)
        public const int FieldTypeIdBoolean = 4; // A yes/no field
        public const int FieldTypeIdDate = 5; // A date field
        public const int FieldTypeIdFile = 6; // A filename of a file in the files directory.
        public const int FieldTypeIdLookup = 7; // A lookup is a FieldTypeInteger that indexes into another table
        public const int FieldTypeIdRedirect = 8; // creates a link to another section
        public const int FieldTypeIdCurrency = 9; // A Float that prints in dollars
        public const int FieldTypeIdFileText = 10; // Text saved in a file in the files area.
        public const int FieldTypeIdFileImage = 11; // A filename of a file in the files directory.
        public const int FieldTypeIdFloat = 12; // A float number
        public const int FieldTypeIdAutoIdIncrement = 13; //long that automatically increments with the new record
        public const int FieldTypeIdManyToMany = 14; // no database field - sets up a relationship through a Rule table to another table
        public const int FieldTypeIdMemberSelect = 15; // This ID is a ccMembers record in a group defined by the MemberSelectGroupID field
        public const int FieldTypeIdFileCSS = 16; // A filename of a CSS compatible file
        public const int FieldTypeIdFileXML = 17; // the filename of an XML compatible file
        public const int FieldTypeIdFileJavascript = 18; // the filename of a javascript compatible file
        public const int FieldTypeIdLink = 19; // Links used in href tags -- can go to pages or resources
        public const int FieldTypeIdResourceLink = 20; // Links used in resources, link <img or <object. Should not be pages
        public const int FieldTypeIdHTML = 21; // LongText field that expects HTML content
        public const int FieldTypeIdFileHTML = 22; // TextFile field that expects HTML content
        public const int FieldTypeIdMax = 22;
        //
        // ----- Field Descriptors for these type
        //       These are what are publicly displayed for each type
        //       See GetFieldTypeNameByType and vise-versa to translater
        //
        public const string FieldTypeNameInteger = "Integer";
        public const string FieldTypeNameText = "Text";
        public const string FieldTypeNameLongText = "LongText";
        public const string FieldTypeNameBoolean = "Boolean";
        public const string FieldTypeNameDate = "Date";
        public const string FieldTypeNameFile = "File";
        public const string FieldTypeNameLookup = "Lookup";
        public const string FieldTypeNameRedirect = "Redirect";
        public const string FieldTypeNameCurrency = "Currency";
        public const string FieldTypeNameImage = "Image";
        public const string FieldTypeNameFloat = "Float";
        public const string FieldTypeNameManyToMany = "ManyToMany";
        public const string FieldTypeNameTextFile = "TextFile";
        public const string FieldTypeNameCSSFile = "CSSFile";
        public const string FieldTypeNameXMLFile = "XMLFile";
        public const string FieldTypeNameJavascriptFile = "JavascriptFile";
        public const string FieldTypeNameLink = "Link";
        public const string FieldTypeNameResourceLink = "ResourceLink";
        public const string FieldTypeNameMemberSelect = "MemberSelect";
        public const string FieldTypeNameHTML = "HTML";
        public const string FieldTypeNameHTMLFile = "HTMLFile";
        //
        public const string FieldTypeNameLcaseInteger = "integer";
        public const string FieldTypeNameLcaseText = "text";
        public const string FieldTypeNameLcaseLongText = "longtext";
        public const string FieldTypeNameLcaseBoolean = "boolean";
        public const string FieldTypeNameLcaseDate = "date";
        public const string FieldTypeNameLcaseFile = "file";
        public const string FieldTypeNameLcaseLookup = "lookup";
        public const string FieldTypeNameLcaseRedirect = "redirect";
        public const string FieldTypeNameLcaseCurrency = "currency";
        public const string FieldTypeNameLcaseImage = "image";
        public const string FieldTypeNameLcaseFloat = "float";
        public const string FieldTypeNameLcaseManyToMany = "manytomany";
        public const string FieldTypeNameLcaseTextFile = "textfile";
        public const string FieldTypeNameLcaseCSSFile = "cssfile";
        public const string FieldTypeNameLcaseXMLFile = "xmlfile";
        public const string FieldTypeNameLcaseJavascriptFile = "javascriptfile";
        public const string FieldTypeNameLcaseLink = "link";
        public const string FieldTypeNameLcaseResourceLink = "resourcelink";
        public const string FieldTypeNameLcaseMemberSelect = "memberselect";
        public const string FieldTypeNameLcaseHTML = "html";
        public const string FieldTypeNameLcaseHTMLFile = "htmlfile";
        //
        //------------------------------------------------------------------------
        // ----- Payment Options
        //------------------------------------------------------------------------
        //
        public const int PayTypeCreditCardOnline = 0; // Pay by credit card online
        public const int PayTypeCreditCardByPhone = 1; // Phone in a credit card
        public const int PayTypeCreditCardByFax = 9; // Phone in a credit card
        public const int PayTypeCHECK = 2; // pay by check to be mailed
        public const int PayTypeCREDIT = 3; // pay on account
        public const int PayTypeNONE = 4; // order total is $0.00. Nothing due
        public const int PayTypeCHECKCOMPANY = 5; // pay by company check
        public const int PayTypeNetTerms = 6;
        public const int PayTypeCODCompanyCheck = 7;
        public const int PayTypeCODCertifiedFunds = 8;
        public const int PayTypePAYPAL = 10;
        public const int PAYDEFAULT = 0;
        //
        //------------------------------------------------------------------------
        // ----- Credit card options
        //------------------------------------------------------------------------
        //
        public const int CCTYPEVISA = 0; // Visa
        public const int CCTYPEMC = 1; // MasterCard
        public const int CCTYPEAMEX = 2; // American Express
        public const int CCTYPEDISCOVER = 3; // Discover
        public const int CCTYPENOVUS = 4; // Novus Card
        public const int CCTYPEDEFAULT = 0;
        //
        //------------------------------------------------------------------------
        // ----- Shipping Options
        //------------------------------------------------------------------------
        //
        public const int SHIPGROUND = 0; // ground
        public const int SHIPOVERNIGHT = 1; // overnight
        public const int SHIPSTANDARD = 2; // standard, whatever that is
        public const int SHIPOVERSEAS = 3; // to overseas
        public const int SHIPCANADA = 4; // to Canada
        public const int SHIPDEFAULT = 0;
        //
        //------------------------------------------------------------------------
        // Debugging info
        //------------------------------------------------------------------------
        //
        public const int TestPointTab = 2;
        public const char debug_TestPointTabChr = '-';
        public static double CPTickCountBase;
        //
        //------------------------------------------------------------------------
        //   project width button defintions
        //------------------------------------------------------------------------
        //
        public const string ButtonApply = "  Apply ";
        public const string ButtonLogin = "  Login  ";
        public const string ButtonLogout = "  Logout  ";
        public const string ButtonSendPassword = "  Send Password  ";
        public const string ButtonJoin = "   Join   ";
        public const string ButtonSave = "  Save  ";
        public const string ButtonOK = "     OK     ";
        public const string ButtonReset = "  Reset  ";
        public const string ButtonSaveAddNew = " Save + Add ";
        //Public Const ButtonSaveAddNew As String = " Save > Add "
        //Public Const ButtonCancel As String = " Cancel "
        public const string ButtonRestartContensiveApplication = " Restart Contensive Application ";
        public const string ButtonCancelAll = "  Cancel  ";
        public const string ButtonFind = "   Find   ";
        public const string ButtonDelete = "  Delete  ";
        public const string ButtonDeletePerson = " Delete Person ";
        public const string ButtonDeleteRecord = " Delete Record ";
        public const string ButtonDeleteEmail = " Delete Email ";
        public const string ButtonDeletePage = " Delete Page ";
        public const string ButtonFileChange = "   Upload   ";
        public const string ButtonFileDelete = "    Delete    ";
        public const string ButtonClose = "  Close   ";
        public const string ButtonAdd = "   Add    ";
        public const string ButtonAddChildPage = " Add Child ";
        public const string ButtonAddSiblingPage = " Add Sibling ";
        public const string ButtonContinue = " Continue >> ";
        public const string ButtonBack = "  << Back  ";
        public const string ButtonNext = "   Next   ";
        public const string ButtonPrevious = " Previous ";
        public const string ButtonFirst = "  First   ";
        public const string ButtonSend = "  Send   ";
        public const string ButtonSendTest = "Send Test";
        public const string ButtonCreateDuplicate = " Create Duplicate ";
        public const string ButtonActivate = "  Activate   ";
        public const string ButtonDeactivate = "  Deactivate   ";
        public const string ButtonOpenActiveEditor = "Active Edit";
        //Public Const ButtonPublish As String = " Publish Changes "
        //Public Const ButtonAbortEdit As String = " Abort Edits "
        //Public Const ButtonPublishSubmit As String = " Submit for Publishing "
        //Public Const ButtonPublishApprove As String = " Approve for Publishing "
        //Public Const ButtonPublishDeny As String = " Deny for Publishing "
        //Public Const ButtonWorkflowPublishApproved As String = " Publish Approved Records "
        //Public Const ButtonWorkflowPublishSelected As String = " Publish Selected Records "
        public const string ButtonSetHTMLEdit = " Edit WYSIWYG ";
        public const string ButtonSetTextEdit = " Edit HTML ";
        public const string ButtonRefresh = " Refresh ";
        public const string ButtonOrder = " Order ";
        public const string ButtonSearch = " Search ";
        public const string ButtonSpellCheck = " Spell Check ";
        public const string ButtonLibraryUpload = " Upload ";
        public const string ButtonCreateReport = " Create Report ";
        public const string ButtonClearTrapLog = " Clear Trap Log ";
        public const string ButtonNewSearch = " New Search ";
        public const string ButtonSaveandInvalidateCache = " Save and Invalidate Cache ";
        public const string ButtonImportTemplates = " Import Templates ";
        public const string ButtonRSSRefresh = " Update RSS Feeds Now ";
        public const string ButtonRequestDownload = " Request Download ";
        public const string ButtonFinish = " Finish ";
        public const string ButtonRegister = " Register ";
        public const string ButtonBegin = "Begin";
        public const string ButtonAbort = "Abort";
        public const string ButtonCreateGUID = " Create GUID ";
        public const string ButtonEnable = " Enable ";
        public const string ButtonDisable = " Disable ";
        public const string ButtonMarkReviewed = " Mark Reviewed ";
        //
        //------------------------------------------------------------------------
        //   member actions
        //------------------------------------------------------------------------
        //
        public const int MemberActionNOP = 0;
        public const int MemberActionLogin = 1;
        public const int MemberActionLogout = 2;
        public const int MemberActionForceLogin = 3;
        public const int MemberActionSendPassword = 4;
        public const int MemberActionForceLogout = 5;
        public const int MemberActionToolsApply = 6;
        public const int MemberActionJoin = 7;
        public const int MemberActionSaveProfile = 8;
        public const int MemberActionEditProfile = 9;
        //
        //-----------------------------------------------------------------------
        // ----- note pad info
        //-----------------------------------------------------------------------
        //
        public const int NoteFormList = 1;
        public const int NoteFormRead = 2;
        //
        public const string NoteButtonPrevious = " Previous ";
        public const string NoteButtonNext = "   Next   ";
        public const string NoteButtonDelete = "  Delete  ";
        public const string NoteButtonClose = "  Close   ";
        //                       ' Submit button is created in CommonDim, so it is simple
        public const string NoteButtonSubmit = "Submit";
        //
        //-----------------------------------------------------------------------
        // ----- Admin site storage
        //-----------------------------------------------------------------------
        //
        public const int AdminMenuModeHidden = 0; // menu is hidden
        public const int AdminMenuModeLeft = 1; // menu on the left
        public const int AdminMenuModeTop = 2; // menu as dropdowns from the top
                                               //
                                               // ----- AdminActions - describes the form processing to do
                                               //
        public const int AdminActionNop = 0; // do nothing
        public const int AdminActionDelete = 4; // delete record
        public const int AdminActionFind = 5;
        public const int AdminActionDeleteFilex = 6;
        public const int AdminActionUpload = 7;
        public const int AdminActionSaveNormal = 3; // save fields to database
        public const int AdminActionSaveEmail = 8; // save email record (and update EmailGroups) to database
        public const int AdminActionSaveMember = 11;
        public const int AdminActionSaveSystem = 12;
        public const int AdminActionSavePaths = 13; // Save a record that is in the BathBlocking Format
        public const int AdminActionSendEmail = 9;
        public const int AdminActionSendEmailTest = 10;
        public const int AdminActionNext = 14;
        public const int AdminActionPrevious = 15;
        public const int AdminActionFirst = 16;
        public const int AdminActionSaveContent = 17;
        public const int AdminActionSaveField = 18; // Save a single field, fieldname = fn input
        public const int AdminActionPublish = 19; // Publish record live
        public const int AdminActionAbortEdit = 20; // Publish record live
        public const int AdminActionPublishSubmit = 21; // Submit for Workflow Publishing
        public const int AdminActionPublishApprove = 22; // Approve for Workflow Publishing
                                                         //Public Const AdminActionWorkflowPublishApproved = 23    ' Publish what was approved
        public const int AdminActionSetHTMLEdit = 24; // Set Member Property for this field to HTML Edit
        public const int AdminActionSetTextEdit = 25; // Set Member Property for this field to Text Edit
        public const int AdminActionSave = 26; // Save Record
        public const int AdminActionActivateEmail = 27; // Activate a Conditional Email
        public const int AdminActionDeactivateEmail = 28; // Deactivate a conditional email
        public const int AdminActionDuplicate = 29; // Duplicate the (sent email) record
        public const int AdminActionDeleteRows = 30; // Delete from rows of records, row0 is boolean, rowid0 is ID, rowcnt is count
        public const int AdminActionSaveAddNew = 31; // Save Record and add a new record
        public const int AdminActionReloadCDef = 32; // Load Content Definitions
                                                     // Public Const AdminActionWorkflowPublishSelected = 33 ' Publish what was selected
        public const int AdminActionMarkReviewed = 34; // Mark the record reviewed without making any changes
        public const int AdminActionEditRefresh = 35; // reload the page just like a save, but do not save
                                                      //
                                                      // ----- Adminforms (0-99)
                                                      //
        public const int AdminFormRoot = 0; // intro page
        public const int AdminFormIndex = 1; // record list page
        public const int AdminFormHelp = 2; // popup help window
        public const int AdminFormUpload = 3; // encoded file upload form
        public const int AdminFormEdit = 4; // Edit form for system format records
        public const int AdminFormEditSystem = 5; // Edit form for system format records
        public const int AdminFormEditNormal = 6; // record edit page
        public const int AdminFormEditEmail = 7; // Edit form for Email format records
        public const int AdminFormEditMember = 8; // Edit form for Member format records
        public const int AdminFormEditPaths = 9; // Edit form for Paths format records
        public const int AdminFormClose = 10; // Special Case - do a window close instead of displaying a form
        public const int AdminFormReports = 12; // Call Reports form (admin only)
                                                //Public Const AdminFormSpider = 13          ' Call Spider
        public const int AdminFormEditContent = 14; // Edit form for Content records
        public const int AdminFormDHTMLEdit = 15; // ActiveX DHTMLEdit form
        public const int AdminFormEditPageContent = 16;
        public const int AdminFormPublishing = 17; // Workflow Authoring Publish Control form
        public const int AdminFormQuickStats = 18; // Quick Stats (from Admin root)
        public const int AdminFormResourceLibrary = 19; // Resource Library without Selects
        public const int AdminFormEDGControl = 20; // Control Form for the EDG publishing controls
        public const int AdminFormSpiderControl = 21; // Control Form for the Content Spider
        public const int AdminFormContentChildTool = 22; // Admin Create Content Child tool
        public const int AdminformPageContentMap = 23; // Map all content to a single map
        public const int AdminformHousekeepingControl = 24; // Housekeeping control
        public const int AdminFormCommerceControl = 25;
        public const int AdminFormContactManager = 26;
        public const int AdminFormStyleEditor = 27;
        public const int AdminFormEmailControl = 28;
        public const int AdminFormCommerceInterface = 29;
        public const int AdminFormDownloads = 30;
        public const int AdminformRSSControl = 31;
        public const int AdminFormMeetingSmart = 32;
        public const int AdminFormMemberSmart = 33;
        public const int AdminFormEmailWizard = 34;
        public const int AdminFormImportWizard = 35;
        public const int AdminFormCustomReports = 36;
        public const int AdminFormFormWizard = 37;
        public const int AdminFormLegacyAddonManager = 38;
        public const int AdminFormIndex_SubFormAdvancedSearch = 39;
        public const int AdminFormIndex_SubFormSetColumns = 40;
        public const int AdminFormPageControl = 41;
        public const int AdminFormSecurityControl = 42;
        public const int AdminFormEditorConfig = 43;
        public const int AdminFormBuilderCollection = 44;
        public const int AdminFormClearCache = 45;
        public const int AdminFormMobileBrowserControl = 46;
        public const int AdminFormMetaKeywordTool = 47;
        public const int AdminFormIndex_SubFormExport = 48;
        //
        // ----- AdminFormTools (11,100-199)
        //
        public const int AdminFormTools = 11; // Call Tools form (developer only)
        public const int AdminFormToolRoot = 11; // These should match for compatibility
        public const int AdminFormToolCreateContentDefinition = 101;
        public const int AdminFormToolContentTest = 102;
        public const int AdminFormToolConfigureMenu = 103;
        public const int AdminFormToolConfigureListing = 104;
        public const int AdminFormToolConfigureEdit = 105;
        public const int AdminFormToolManualQuery = 106;
        public const int AdminFormToolWriteUpdateMacro = 107;
        public const int AdminFormToolDuplicateContent = 108;
        public const int AdminFormToolDuplicateDataSource = 109;
        public const int AdminFormToolDefineContentFieldsFromTable = 110;
        public const int AdminFormToolContentDiagnostic = 111;
        public const int AdminFormToolCreateChildContent = 112;
        public const int AdminFormToolClearContentWatchLink = 113;
        public const int AdminFormToolSyncTables = 114;
        public const int AdminFormToolBenchmark = 115;
        public const int AdminFormToolSchema = 116;
        public const int AdminFormToolContentFileView = 117;
        public const int AdminFormToolDbIndex = 118;
        public const int AdminFormToolContentDbSchema = 119;
        public const int AdminFormToolLogFileView = 120;
        public const int AdminFormToolLoadCDef = 121;
        public const int AdminFormToolLoadTemplates = 122;
        public const int AdminformToolFindAndReplace = 123;
        public const int AdminformToolCreateGUID = 124;
        public const int AdminformToolIISReset = 125;
        public const int AdminFormToolRestart = 126;
        public const int AdminFormToolWebsiteFileView = 127;
        //
        // ----- Define the index column structure
        //       IndexColumnVariant( 0, n ) is the first column on the left
        //       IndexColumnVariant( 0, IndexColumnField ) = the index into the fields array
        //
        public const int IndexColumnField = 0; // The field displayed in the column
        public const int IndexColumnWIDTH = 1; // The width of the column
        public const int IndexColumnSORTPRIORITY = 2; // lowest columns sorts first
        public const int IndexColumnSORTDIRECTION = 3; // direction of the sort on this column
        public const int IndexColumnSATTRIBUTEMAX = 3; // the number of attributes here
        public const int IndexColumnsMax = 50;
        //
        // ----- ReportID Constants, moved to ccCommonModule
        //
        public const int ReportFormRoot = 1;
        public const int ReportFormDailyVisits = 2;
        public const int ReportFormWeeklyVisits = 12;
        public const int ReportFormSitePath = 4;
        public const int ReportFormSearchKeywords = 5;
        public const int ReportFormReferers = 6;
        public const int ReportFormBrowserList = 8;
        public const int ReportFormAddressList = 9;
        public const int ReportFormContentProperties = 14;
        public const int ReportFormSurveyList = 15;
        public const int ReportFormOrdersList = 13;
        public const int ReportFormOrderDetails = 21;
        public const int ReportFormVisitorList = 11;
        public const int ReportFormMemberDetails = 16;
        public const int ReportFormPageList = 10;
        public const int ReportFormVisitList = 3;
        public const int ReportFormVisitDetails = 17;
        public const int ReportFormVisitorDetails = 20;
        public const int ReportFormSpiderDocList = 22;
        public const int ReportFormSpiderErrorList = 23;
        public const int ReportFormEDGDocErrors = 24;
        public const int ReportFormDownloadLog = 25;
        public const int ReportFormSpiderDocDetails = 26;
        public const int ReportFormSurveyDetails = 27;
        public const int ReportFormEmailDropList = 28;
        public const int ReportFormPageTraffic = 29;
        public const int ReportFormPagePerformance = 30;
        public const int ReportFormEmailDropDetails = 31;
        public const int ReportFormEmailOpenDetails = 32;
        public const int ReportFormEmailClickDetails = 33;
        public const int ReportFormGroupList = 34;
        public const int ReportFormGroupMemberList = 35;
        public const int ReportFormTrapList = 36;
        public const int ReportFormCount = 36;
        //
        //=============================================================================
        // Page Scope Meetings Related Storage
        //=============================================================================
        //
        public const int MeetingFormIndex = 0;
        public const int MeetingFormAttendees = 1;
        public const int MeetingFormLinks = 2;
        public const int MeetingFormFacility = 3;
        public const int MeetingFormHotel = 4;
        public const int MeetingFormDetails = 5;
        //
        //------------------------------------------------------------------------------
        // Form actions
        //------------------------------------------------------------------------------
        //
        // ----- DataSource Types
        //
        public const int DataSourceTypeODBCSQL99 = 0;
        public const int DataSourceTypeODBCAccess = 1;
        public const int DataSourceTypeODBCSQLServer = 2;
        public const int DataSourceTypeODBCMySQL = 3;
        public const int DataSourceTypeXMLFile = 4; // Use MSXML Interface to open a file

        //
        // Document (HTML, graphic, etc) retrieved from site
        //
        public const int ResponseHeaderCountMax = 20;
        public const int ResponseCookieCountMax = 20;
        //
        // ----- text delimiter that divides the text and html parts of an email message stored in the queue folder
        //
        public const string EmailTextHTMLDelimiter = "\r\n ----- End Text Begin HTML -----\r\n";
        //
        //------------------------------------------------------------------------
        //   Common RequestName Variables
        //------------------------------------------------------------------------
        //
        public const string RequestNameDynamicFormID = "dformid";
        //
        public const string RequestNameRunAddon = "addonid";
        public const string RequestNameEditReferer = "EditReferer";
        //Public Const RequestNameRefreshBlock As String = "ccFormRefreshBlockSN"
        public const string RequestNameCatalogOrder = "CatalogOrderID";
        public const string RequestNameCatalogCategoryID = "CatalogCatID";
        public const string RequestNameCatalogForm = "CatalogFormID";
        public const string RequestNameCatalogItemID = "CatalogItemID";
        public const string RequestNameCatalogItemAge = "CatalogItemAge";
        public const string RequestNameCatalogRecordTop = "CatalogTop";
        public const string RequestNameCatalogFeatured = "CatalogFeatured";
        public const string RequestNameCatalogSpan = "CatalogSpan";
        public const string RequestNameCatalogKeywords = "CatalogKeywords";
        public const string RequestNameCatalogSource = "CatalogSource";
        //
        public const string RequestNameLibraryFileID = "fileEID";
        public const string RequestNameDownloadID = "downloadid";
        public const string RequestNameLibraryUpload = "LibraryUpload";
        public const string RequestNameLibraryName = "LibraryName";
        public const string RequestNameLibraryDescription = "LibraryDescription";

        public const string RequestNameRootPage = "RootPageName";
        public const string RequestNameRootPageID = "RootPageID";
        public const string RequestNameContent = "ContentName";
        public const string RequestNameOrderByClause = "OrderByClause";
        public const string RequestNameAllowChildPageList = "AllowChildPageList";
        //
        public const string RequestNameCRKey = "crkey";
        public const string RequestNameAdminForm = "af";
        public const string RequestNameAdminSubForm = "subform";
        public const string RequestNameButton = "button";
        public const string RequestNameAdminSourceForm = "asf";
        public const string RequestNameAdminFormSpelling = "SpellingRequest";
        public const string RequestNameInlineStyles = "InlineStyles";
        public const string RequestNameAllowCSSReset = "AllowCSSReset";
        //
        public const string RequestNameReportForm = "rid";
        //
        public const string RequestNameToolContentID = "ContentID";
        //
        public const string RequestNameCut = "a904o2pa0cut";
        public const string RequestNamePaste = "dp29a7dsa6paste";
        public const string RequestNamePasteParentContentID = "dp29a7dsa6cid";
        public const string RequestNamePasteParentRecordID = "dp29a7dsa6rid";
        public const string RequestNamePasteFieldList = "dp29a7dsa6key";
        public const string RequestNameCutClear = "dp29a7dsa6clear";
        //
        public const string RequestNameRequestBinary = "RequestBinary";
        // removed -- this was an old method of blocking form input for file uploads
        //Public Const RequestNameFormBlock As String = "RB"
        public const string RequestNameJSForm = "RequestJSForm";
        public const string RequestNameJSProcess = "ProcessJSForm";
        //
        public const string RequestNameFolderID = "FolderID";
        //
        public const string rnEmailMemberID = "emi8s9Kj";
        public const string rnEmailOpenFlag = "eof9as88";
        public const string rnEmailOpenCssFlag = "8aa41pM3";
        public const string rnEmailClickFlag = "ecf34Msi";
        public const string rnEmailBlockRecipientEmail = "9dq8Nh61";
        public const string rnEmailBlockRequestDropID = "BlockEmailRequest";
        public const string RequestNameVisitTracking = "s9lD1088";
        public const string RequestNameBlockContentTracking = "BlockContentTracking";
        public const string RequestNameCookieDetectVisitID = "f92vo2a8d";

        public const string RequestNamePageNumber = "PageNumber";
        public const string RequestNamePageSize = "PageSize";
        //
        public const string RequestValueNull = "[NULL]";
        //
        public const string SpellCheckUserDictionaryFilename = "SpellCheck\\UserDictionary.txt";
        //
        public const string RequestNameStateString = "vstate";
        //
        // ----- Actions
        //
        public const int ToolsActionMenuMove = 1;
        public const int ToolsActionAddField = 2; // Add a field to the Index page
        public const int ToolsActionRemoveField = 3;
        public const int ToolsActionMoveFieldRight = 4;
        public const int ToolsActionMoveFieldLeft = 5;
        public const int ToolsActionSetAZ = 6;
        public const int ToolsActionSetZA = 7;
        public const int ToolsActionExpand = 8;
        public const int ToolsActionContract = 9;
        public const int ToolsActionEditMove = 10;
        public const int ToolsActionRunQuery = 11;
        public const int ToolsActionDuplicateDataSource = 12;
        public const int ToolsActionDefineContentFieldFromTableFieldsFromTable = 13;
        public const int ToolsActionFindAndReplace = 14;
        public const int ToolsActionIISReset = 15;
        //
        //=======================================================================
        //   sitepropertyNames
        //=======================================================================
        //
        public const string siteproperty_serverPageDefault_name = "serverPageDefault";
        public const string siteproperty_serverPageDefault_defaultValue = "default.aspx";
        public const string spAllowPageWithoutSectionDisplay = "Allow Page Without Section Display";
        public const bool spAllowPageWithoutSectionDisplay_default = true;
        public const string spDefaultRouteAddonId = "Default Route AddonId";
        //
        //=======================================================================
        //   content replacements
        //=======================================================================
        //
        public const string contentReplaceEscapeStart = "{%";
        public const string contentReplaceEscapeEnd = "%}";
        //
        public class fieldEditorType {
            public int fieldId;
            public int addonid;
        }
        //
        private const int TimerStackMax = 20;
        //
        public const string TextSearchStartTagDefault = "<!--TextSearchStart-->";
        public const string TextSearchEndTagDefault = "<!--TextSearchEnd-->";
        //
        //-------------------------------------------------------------------------------------
        //   Email
        //-------------------------------------------------------------------------------------
        //
        public const int EmailLogTypeDrop = 1; // Email was dropped
        public const int EmailLogTypeOpen = 2; // System detected the email was opened
        public const int EmailLogTypeClick = 3; // System detected a click from a link on the email
        public const int EmailLogTypeBounce = 4; // Email was processed by bounce processing
        public const int EmailLogTypeBlockRequest = 5; // recipient asked us to stop sending email
        public const int EmailLogTypeImmediateSend = 6; // Email was dropped                                                        
        public const string DefaultSpamFooter = "<p>To block future emails from this site, <link>click here</link></p>";        
        public const string FeedbackFormNotSupportedComment = "<!--\r\nFeedback form is not supported in this context\r\n-->";
        //
        //-------------------------------------------------------------------------------------
        //   Page Content constants
        //-------------------------------------------------------------------------------------
        //
        public const string ContentBlockCopyName = "Content Block Copy";
        //
        public const string BubbleCopy_AdminAddPage = "Use the Add page to create new content records. The save button puts you in edit mode. The OK button creates the record and exits.";
        public const string BubbleCopy_AdminIndexPage = "Use the Admin Listing page to locate content records through the Admin Site.";
        public const string BubbleCopy_SpellCheckPage = "Use the Spell Check page to verify and correct spelling throught the content.";
        public const string BubbleCopy_AdminEditPage = "Use the Edit page to add and modify content.";
        //
        //
        public const string TemplateDefaultName = "Default";
        //Public Const TemplateDefaultBody As String = "<!--" & vbCrLf & "Default Template - edit this Page Template, or select a different template for your page or section" & vbCrLf & "-->{{DYNAMICMENU?MENU=}}<br>{{CONTENT}}"
        public const string TemplateDefaultBody = ""
            + "\r\n\t<!--"
            + "\r\n\tDefault Template - edit this Page Template, or select a different template for your page or section"
            + "\r\n\t-->"
            + "\r\n\t{% {\"addon\":{\"addon\":\"menu\",\"menu\":\"Default\"}} %}"
            + "\r\n\t{% \"content box\" %}";
        public const string TemplateDefaultBodyTag = "<body class=\"ccBodyWeb\">";
        //
        //=======================================================================
        //   Internal Tab interface storage
        //=======================================================================
        //
        // Admin Navigator Nodes
        //
        public const int NavigatorNodeCollectionList = -1;
        public const int NavigatorNodeAddonList = -1;
        //
        // Pointers into index of PCC (Page Content Cache) array
       //
        public const string DTDDefault = "<!DOCTYPE html>";
        //
        // innova Editor feature list
        //
        public const string InnovaEditorFeaturefilename = "innova\\EditorConfig.txt";
        public const string InnovaEditorFeatureList = "FullScreen,Preview,Print,Search,Cut,Copy,Paste,PasteWord,PasteText,SpellCheck,Undo,Redo,Image,Flash,Media,CustomObject,CustomTag,Bookmark,Hyperlink,HTMLSource,XHTMLSource,Numbering,Bullets,Indent,Outdent,JustifyLeft,JustifyCenter,JustifyRight,JustifyFull,Table,Guidelines,Absolute,Characters,Line,Form,RemoveFormat,ClearAll,StyleAndFormatting,TextFormatting,ListFormatting,BoxFormatting,ParagraphFormatting,CssText,Styles,Paragraph,FontName,FontSize,Bold,Italic,Underline,Strikethrough,Superscript,Subscript,ForeColor,BackColor";
        public const string InnovaEditorPublicFeatureList = "FullScreen,Preview,Print,Search,Cut,Copy,Paste,PasteWord,PasteText,SpellCheck,Undo,Redo,Bookmark,Hyperlink,HTMLSource,XHTMLSource,Numbering,Bullets,Indent,Outdent,JustifyLeft,JustifyCenter,JustifyRight,JustifyFull,Table,Guidelines,Absolute,Characters,Line,Form,RemoveFormat,ClearAll,StyleAndFormatting,TextFormatting,ListFormatting,BoxFormatting,ParagraphFormatting,CssText,Styles,Paragraph,FontName,FontSize,Bold,Italic,Underline,Strikethrough,Superscript,Subscript,ForeColor,BackColor";
        //
        public const string DynamicStylesFilename = "templates/styles.css";
        public const string AdminSiteStylesFilename = "templates/AdminSiteStyles.css";
        public const string EditorStyleRulesFilenamePattern = "templates/EditorStyleRules$TemplateID$.js";
        //
        // ----- This should match the Lookup List in the NavIconType field in the Navigator Entry content definition
        //
        public const string navTypeIDList = "Add-on,Report,Setting,Tool";
        public const int NavTypeIDAddon = 1;
        public const int NavTypeIDReport = 2;
        public const int NavTypeIDSetting = 3;
        public const int NavTypeIDTool = 4;
        //
        public const string NavIconTypeList = "Custom,Advanced,Content,Folder,Email,User,Report,Setting,Tool,Record,Addon,help";
        public const int NavIconTypeCustom = 1;
        public const int NavIconTypeAdvanced = 2;
        public const int NavIconTypeContent = 3;
        public const int NavIconTypeFolder = 4;
        public const int NavIconTypeEmail = 5;
        public const int NavIconTypeUser = 6;
        public const int NavIconTypeReport = 7;
        public const int NavIconTypeSetting = 8;
        public const int NavIconTypeTool = 9;
        public const int NavIconTypeRecord = 10;
        public const int NavIconTypeAddon = 11;
        public const int NavIconTypeHelp = 12;
        //
        public const int QueryTypeSQL = 1;
        public const int QueryTypeOpenContent = 2;
        public const int QueryTypeUpdateContent = 3;
        public const int QueryTypeInsertContent = 4;
        //
        // Google Data Object construction in GetRemoteQuery
        //
        public class ColsType {
            public string Type;
            public string Id;
            public string Label;
            public string Pattern;
        }
        //
        public class CellType {
            public string v;
            public string f;
            public string p;
        }
        //
        public class RowsType {
            public List<CellType> Cell;
        }
        //
        public class GoogleDataType {
            public bool IsEmpty;
            public List<ColsType> col;
            public List<RowsType> row;
        }
        //
        public enum GoogleVisualizationStatusEnum {
            OK = 1,
            warning = 2,
            ErrorStatus = 3
        }
        //
        public class GoogleVisualizationType {
            public string version;
            public string reqid;
            public GoogleVisualizationStatusEnum status;
            public string[] warnings;
            public string[] errors;
            public string sig;
            public GoogleDataType table;
        }

        //Public Const ReturnFormatTypeGoogleTable = 1
        //Public Const ReturnFormatTypeNameValue = 2

        public enum RemoteFormatEnum {
            RemoteFormatJsonTable = 1,
            RemoteFormatJsonNameArray = 2,
            RemoteFormatJsonNameValue = 3
        }
        //Private Enum GoogleVisualizationStatusEnum
        //    OK = 1
        //    warning = 2
        //    ErrorStatus = 3
        //End Enum
        //
        //Private Structure GoogleVisualizationType
        //    Dim version As String
        //    Dim reqid As String
        //    Dim status As GoogleVisualizationStatusEnum
        //    Dim warnings() As String
        //    Dim errors() As String
        //    Dim sig As String
        //    Dim table As GoogleDataType
        //End Structure        '
        //
        //
        //[System.Runtime.InteropServices.DllImport("advapi32.dll", EntryPoint = "RegCloseKey&", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Ansi, SetLastError = true)]
        //public static extern object RegCloseKey&(long hKey);
        //[System.Runtime.InteropServices.DllImport("advapi32.dll", EntryPoint = "RegOpenKeyExA&", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Ansi, SetLastError = true)]
        //public static extern object RegOpenKeyExA&(long hKey, string lpszSubKey, long dwOptions, long samDesired, long lpHKey);
        //[System.Runtime.InteropServices.DllImport("advapi32.dll", EntryPoint = "RegQueryValueExA&", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Ansi, SetLastError = true)]
        //public static extern object RegQueryValueExA&(long hKey, string lpszValueName, long lpdwRes, long lpdwType, string lpDataBuff, long nSize);
        //[System.Runtime.InteropServices.DllImport("advapi32.dll", EntryPoint = "RegQueryValueExA", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Ansi, SetLastError = true)]
        //public static extern object RegQueryValueEx&(long hKey, string lpszValueName, long lpdwRes, long lpdwType, long lpDataBuff, long nSize);

        //public const uint HKEY_CLASSES_ROOT = unchecked((int)0x80000000);
        //public const uint HKEY_CURRENT_USER = unchecked((int)0x80000001);
        //public const uint HKEY_LOCAL_MACHINE = unchecked((int)0x80000002);
        //public const uint HKEY_USERS = unchecked((int)0x80000003);

        //public const int ERROR_SUCCESS = 0L;
        //public const int REG_SZ = 1L; // Unicode nul terminated string
        //public const int REG_DWORD = 4L; // 32-bit number

        //public const int KEY_QUERY_VALUE = 0x1L;
        //public const int KEY_SET_VALUE = 0x2L;
        //public const int KEY_CREATE_SUB_KEY = 0x4L;
        //public const int KEY_ENUMERATE_SUB_KEYS = 0x8L;
        //public const int KEY_NOTIFY = 0x10L;
        //public const int KEY_CREATE_LINK = 0x20L;
        //public const int READ_CONTROL = 0x20000;
        //public const int WRITE_DAC = 0x40000;
        //public const int WRITE_OWNER = 0x80000;
        //public const int SYNCHRONIZE = 0x100000;
        //public const int STANDARD_RIGHTS_REQUIRED = 0xF0000;
        //public const int STANDARD_RIGHTS_READ = READ_CONTROL;
        //public const int STANDARD_RIGHTS_WRITE = READ_CONTROL;
        //public const int STANDARD_RIGHTS_EXECUTE = READ_CONTROL;
        //public const int KEY_READ = STANDARD_RIGHTS_READ | KEY_QUERY_VALUE | KEY_ENUMERATE_SUB_KEYS | KEY_NOTIFY;
        //public const int KEY_WRITE = STANDARD_RIGHTS_WRITE | KEY_SET_VALUE | KEY_CREATE_SUB_KEY;
        //public const int KEY_EXECUTE = KEY_READ;
        public const int maxLongValue = 2147483647;
        //
        // Error Definitions
        //
        //public const int ERR_UNKNOWN = Microsoft.VisualBasic.Constants.vbObjectError + 101;
        //public const int ERR_FIELD_DOES_NOT_EXIST = Microsoft.VisualBasic.Constants.vbObjectError + 102;
        //public const int ERR_FILESIZE_NOT_ALLOWED = Microsoft.VisualBasic.Constants.vbObjectError + 103;
        //public const int ERR_FOLDER_DOES_NOT_EXIST = Microsoft.VisualBasic.Constants.vbObjectError + 104;
        //public const int ERR_FILE_ALREADY_EXISTS = Microsoft.VisualBasic.Constants.vbObjectError + 105;
        //public const int ERR_FILE_TYPE_NOT_ALLOWED = Microsoft.VisualBasic.Constants.vbObjectError + 106;

        //
        // addonIncludeRules cache
        //
        public const string cache_addonIncludeRules_cacheName = "cache_addonIncludeRules";
        public const string cache_addonIncludeRules_fieldList = "addonId,includedAddonId";
        public const int addonIncludeRulesCache_addonId = 0;
        public const int addonIncludeRulesCache_includedAddonId = 1;
        public const int addonIncludeRulesCacheColCnt = 2;
        //
        // addonIncludeRules cache
        //
        public const string cache_LibraryFiles_cacheName = "cache_LibraryFiles";
        public const string cache_LibraryFiles_fieldList = "id,ccguid,clicks,filename,width,height,AltText,altsizelist";
        public const int LibraryFilesCache_Id = 0;
        public const int LibraryFilesCache_ccGuid = 1;
        public const int LibraryFilesCache_clicks = 2;
        public const int LibraryFilesCache_filename = 3;
        public const int LibraryFilesCache_width = 4;
        public const int LibraryFilesCache_height = 5;
        public const int LibraryFilesCache_alttext = 6;
        public const int LibraryFilesCache_altsizelist = 7;
        public const int LibraryFilesCacheColCnt = 8;
        //
        // link forward cache
        //
        public const string cache_linkForward_cacheName = "cache_linkForward";
        //
        public const string main_cookieNameVisit = "visit";
        public const string main_cookieNameVisitor = "visitor";
        public const string html_quickEdit_fpo = "<quickeditor>";
        //
        //Public Const sqlAddonStyles  As String  = "select addonid,styleid from ccSharedStylesAddonRules where (active<>0) order by id"
        //
        public const string cacheNameAddonStyleRules = "addon styles";
        //
        public const bool ALLOWLEGACYAPI = false;
        public const bool ALLOWPROFILING = false;
        //
        public const string cacheNameAssemblySkipList = "assembly-skip-list";
        public const string cacheNameGlobalInvalidationDate = "global-invalidation-date";
        //
        // put content definitions here
        //
        //
        public struct NameValuePairType {
            public string Name;
            public string Value;
        }
        //
        //========================================================================
        //   defined errors (event log eventId)
        //       1000-1999 Contensive
        //       2000-2999 Datatree
        //
        //   see kmaErrorDescription() for transations
        //========================================================================
        //
        private const int Error_DataTree_RootNodeNext = 2000;
        private const int Error_DataTree_NoGoNext = 2001;
        //
        //========================================================================
        //
        //========================================================================
        //
        //Declare Function GetTickCount Lib "kernel32" () As Integer
        [System.Runtime.InteropServices.DllImport("kernel32", EntryPoint = "GetCurrentProcessId", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Ansi, SetLastError = true)]
        public static extern int GetCurrentProcessId();
        //Declare Sub Sleep Lib "kernel32" (ByVal dwMilliseconds As Integer)
        //
        //========================================================================
        //   Declarations for SetPiorityClass
        //========================================================================
        //
        //Const THREAD_BASE_PRIORITY_IDLE = -15
        //Const THREAD_BASE_PRIORITY_LOWRT = 15
        //Const THREAD_BASE_PRIORITY_MIN = -2
        //Const THREAD_BASE_PRIORITY_MAX = 2
        //Const THREAD_PRIORITY_LOWEST = THREAD_BASE_PRIORITY_MIN
        //Const THREAD_PRIORITY_HIGHEST = THREAD_BASE_PRIORITY_MAX
        //Const THREAD_PRIORITY_BELOW_NORMAL = (THREAD_PRIORITY_LOWEST + 1)
        //Const THREAD_PRIORITY_ABOVE_NORMAL = (THREAD_PRIORITY_HIGHEST - 1)
        //Const THREAD_PRIORITY_IDLE = THREAD_BASE_PRIORITY_IDLE
        //Const THREAD_PRIORITY_NORMAL = 0
        //Const THREAD_PRIORITY_TIME_CRITICAL = THREAD_BASE_PRIORITY_LOWRT
        //Const HIGH_PRIORITY_CLASS = &H80
        //Const IDLE_PRIORITY_CLASS = &H40
        //Const NORMAL_PRIORITY_CLASS = &H20
        //Const REALTIME_PRIORITY_CLASS = &H100
        //
        //Private Declare Function SetThreadPriority Lib "kernel32" (ByVal hThread As Integer, ByVal nPriority As Integer) As Integer
        //Private Declare Function SetPriorityClass Lib "kernel32" (ByVal hProcess As Integer, ByVal dwPriorityClass As Integer) As Integer
        //Private Declare Function GetThreadPriority Lib "kernel32" (ByVal hThread As Integer) As Integer
        //Private Declare Function GetPriorityClass Lib "kernel32" (ByVal hProcess As Integer) As Integer
        //Private Declare Function GetCurrentThread Lib "kernel32" () As Integer
        //Private Declare Function GetCurrentProcess Lib "kernel32" () As Integer
        //

        //
        //========================================================================
        //Converts unsafe characters,
        //such as spaces, into their
        //corresponding escape sequences.
        //========================================================================
        //
        //Declare Function UrlEscape Lib "shlwapi" _
        //   Alias "UrlEscapeA" _
        //  (ByVal pszURL As String, _
        //   ByVal pszEscaped As String, _
        //   ByVal pcchEscaped As Integer, _
        //   ByVal dwFlags As Integer) As Integer
        //
        //Converts escape sequences back into
        //ordinary characters.
        //
        //Declare Function UrlUnescape Lib "shlwapi" _
        //   Alias "UrlUnescapeA" _
        //  (ByVal pszURL As String, _
        //   ByVal pszUnescaped As String, _
        //   ByVal pcchUnescaped As Integer, _
        //   ByVal dwFlags As Integer) As Integer

        //
        //   Error reporting strategy
        //       Popups are pop-up boxes that tell the user what to do
        //       Logs are files with error details for developers to use debugging
        //
        //       Attended Programs
        //           - errors that do not effect the operation, resume next
        //           - all errors trickle up to the user interface level
        //           - User Errors, like file not found, return "UserError" code and a description
        //           - Internal Errors, like div-by-0, User should see no detail, log gets everything
        //           - Dependant Object Errors, codes that return from objects:
        //               - If UserError, translate ErrSource for raise, but log all original info
        //               - If InternalError, log info and raise InternalError
        //               - If you can not tell, call it InternalError
        //
        //       UnattendedMode
        //           The same, except each routine decides when
        //
        //       When an error happens in-line (bad condition without a raise)
        //           Log the error
        //           Raise the appropriate Code/Description in the current Source
        //
        //       When an ErrorTrap occurs
        //           If ErrSource is not AppTitle, it is a dependantObjectError, log and translate code
        //           If ErrNumber is not an ObjectError, call it internal error, log and translate code
        //           Error must be either "InternalError" or "UserError", just raise it again
        //
        // old - If an error is raised that is not a KmaCode, it is logged and translated
        // old - If an error is raised and the soure is not he current "dll", it is logged and translated
        //
        //Public Const ignoreInteger = vbObjectError                 ' Base on which Internal errors should start
        //
        //Public Const KmaError_UnderlyingObject = vbObjectError + 1     ' An error occurec in an underlying object
        //Public Const KmaccErrorServiceStopped = vbObjectError + 2       ' The service is not running
        //Public Const KmaError_BadObject = vbObjectError + 3            ' The Server Pointer is not valid
        //Public Const KmaError_UpgradeInProgress = vbObjectError + 4    ' page is blocked because an upgrade is in progress
        //Public Const KmaError_InvalidArgument = vbObjectError + 5      ' and input argument is not valid. Put details at end of description
        //
        //Public Const ignoreInteger = ignoreInteger + 16                   ' Generic Error code that passes the description back to the user
        //Public Const ignoreInteger = ignoreInteger + 17               ' Internal error which the user should not see
        //Public Const KmaErrorPage = ignoreInteger + 18                   ' Error from the page which called Contensive
        //
        //Public Const KmaObjectError = ignoreInteger + 256                ' Internal error which the user should not see
        //
        public const string SQLTrue = "1";
        public const string SQLFalse = "0";
        //
        public static readonly DateTime dateMinValue = new DateTime(1899, 12, 30);
        //
        //
        public const string kmaEndTable = "</table >";
        public const string tableCellEnd = "</td>";
        public const string kmaEndTableRow = "</tr>";
        //
        public enum contentTypeEnum {
            contentTypeWeb = 1,
            contentTypeEmail = 2,
            contentTypeWebTemplate = 3,
            contentTypeEmailTemplate = 4
        }
        //
        // ---------------------------------------------------------------------------------------------------
        // ----- CDefAdminColumnType
        // ---------------------------------------------------------------------------------------------------
        //
        //Public Structure cdefServices.CDefAdminColumnType
        //    Public Name As String
        //    Public FieldPointer As Integer
        //    Public Width As Integer
        //    Public SortPriority As Integer
        //    Public SortDirection As Integer
        //End Structure
        //
        // ---------------------------------------------------------------------------------------------------
        // ----- CDefFieldType
        //       class not structure because it has to marshall to vb6
        // ---------------------------------------------------------------------------------------------------
        //
        //Public Structure cdefServices.CDefFieldType
        //    Public Name As String                      ' The name of the field
        //    Public ValueVariant As Object             ' The value carried to and from the database
        //    Public Id As Integer                          ' the ID in the ccContentFields Table that this came from
        //    Public active As Boolean                   ' if the field is available in the admin area
        //    Public fieldType As Integer                   ' The type of data the field holds
        //    Public Caption As String                   ' The caption for displaying the field
        //    Public ReadOnlyField As Boolean            ' was ReadOnly -- If true, this field can not be written back to the database
        //    Public NotEditable As Boolean              ' if true, you can only edit new records
        //    Public LookupContentID As Integer             ' If TYPELOOKUP, (for Db controled sites) this is the content ID of the source table
        //    Public Required As Boolean                 ' if true, this field must be entered
        //    Public DefaultValueObject As Object      ' default value on a new record
        //    Public HelpMessage As String               ' explaination of this field
        //    Public UniqueName As Boolean               '
        //    Public TextBuffered As Boolean             ' if true, the input is run through RemoveControlCharacters()
        //    Public Password As Boolean                 ' for text boxes, sets the password attribute
        //    Public RedirectContentID As Integer           ' If TYPEREDIRECT, this is new contentID
        //    Public RedirectID As String                ' If TYPEREDIRECT, this is the field that must match ID of this record
        //    Public RedirectPath As String              ' New Field, If TYPEREDIRECT, this is the path to the next page (if blank, current page is used)
        //    Public IndexColumn As Integer                 ' the column desired in the admin index form
        //    Public IndexWidth As String                ' either number or percentage
        //    Public IndexSortOrder As String            ' alpha sort on index page
        //    Public IndexSortDirection As Integer          ' 1 sorts forward, -1 backward
        //    Public Changed As Boolean                  ' if true, field value needs to be saved to database
        //    Public AdminOnly As Boolean                ' This field is only available to administrators
        //    Public DeveloperOnly As Boolean            ' This field is only available to administrators
        //    Public BlockAccess As Boolean              ' ***** Field Reused to keep binary compatiblity - "IsBaseField" - if true this is a CDefBase field
        //    '   false - custom field, is not altered during upgrade, Help message comes from the local record
        //    '   true - upgrade modifies the field definition, help message comes from support.contensive.com
        //    Public htmlContent As Boolean              ' if true, the HTML editor (active edit) can be used
        //    Public Authorable As Boolean               ' true if it can be seen in the admin form
        //    Public Inherited As Boolean                ' if true, this field takes its values from a parent, see ContentID
        //    Public ContentID As Integer                   ' This is the ID of the Content Def that defines these properties
        //    Public EditSortPriority As Integer            ' The Admin Edit Sort Order
        //    Public ManyToManyContentID As Integer         ' Content containing Secondary Records
        //    Public ManyToManyRuleContentID As Integer     ' Content with rules between Primary and Secondary
        //    Public ManyToManyRulePrimaryField As String     ' Rule Field Name for Primary Table
        //    Public ManyToManyRuleSecondaryField As String   ' Rule Field Name for Secondary Table
        //    '
        //    ' - new
        //    '
        //    Public RSSTitleField As Boolean             ' When creating RSS fields from this content, this is the title
        //    Public RSSDescriptionField As Boolean       ' When creating RSS fields from this content, this is the description
        //    Public EditTab As String                   ' Editing group - used for the tabs
        //    Public Scramble As Boolean                 ' save the field scrambled in the Db
        //    Public MemberSelectGroupID As Integer         ' If the Type is TypeMemberSelect, this is the group that the member will be selected from
        //    Public LookupList As String                ' If TYPELOOKUP, and LookupContentID is null, this is a comma separated list of choices
        //End Structure
        //
        // ---------------------------------------------------------------------------------------------------
        // ----- CDefType
        //       class not structure because it has to marshall to vb6
        // ---------------------------------------------------------------------------------------------------
        //
        //Public Structure cdefServices.CDefType
        //    Public Name As String                       ' Name of Content
        //    Public Id As Integer                           ' index in content table
        //    Public ContentTableName As String           ' the name of the content table
        //    Public ContentDataSourceName As String      '
        //    Public AuthoringTableName As String         ' the name of the authoring table
        //    Public AuthoringDataSourceName As String    '
        //    Public AllowAdd As Boolean                  ' Allow adding records
        //    Public AllowDelete As Boolean               ' Allow deleting records
        //    Public WhereClause As String                ' Used to filter records in the admin area
        //    Public ParentID As Integer                     ' if not IgnoreContentControl, the ID of the parent content
        //    Public ChildIDList As String                ' Comma separated list of child content definition IDs
        //    Public ChildPointerList As String           ' Comma separated list of child content definition Pointers
        //    Public DefaultSortMethod As String          ' FieldName Direction, ....
        //    Public ActiveOnly As Boolean                ' When true
        //    Public AdminOnly As Boolean                 ' Only allow administrators to modify content
        //    Public DeveloperOnly As Boolean             ' Only allow developers to modify content
        //    Public AllowWorkflowAuthoring As Boolean    ' if true, treat this content with authoring proceses
        //    Public DropDownFieldList As String          ' String used to populate select boxes
        //    Public SelectFieldList As String            ' Field list used in OpenCSContent calls (all active field definitions)
        //    Public EditorGroupName As String            ' Group of members who administer Workflow Authoring
        //    '
        //    ' array of cdefFieldType throws a vb6 error, data or method problem
        //    ' public property does not work (msn article -- very slow because it marshals he entire array, not a pointer )
        //    ' public function works to read, but cannot write
        //    ' possible -- public fields() function for reads
        //    '   -- public setFields() function for writes
        //    '
        //    Public fields as appServices_CdefClass.CDefFieldType()
        //    Public FieldPointer As Integer                 ' Current Field for FirstField / NextField calls
        //    Public FieldCount As Integer                   ' The number of fields loaded (from ccFields, or Set calls)
        //    Public FieldSize As Integer                    ' The number of initialize Field()
        //    '
        //    ' same as fields
        //    '
        //    Public adminColumns as appServices_CdefClass.CDefAdminColumnType()
        //    'Public AdminColumnLocal As CDefAdminColumnType()  ' The Admins in this content
        //    Public AdminColumnPointer As Integer                 ' Current Admin for FirstAdminColumn / NextAdminColumn calls
        //    Public AdminColumnCount As Integer                   ' The number of AdminColumns loaded (from ccAdminColumns, or Set calls)
        //    Public AdminColumnSize As Integer                    ' The number of initialize Admin()s
        //    'AdminColumn As CDefAdminCollClass
        //    '
        //    Public ContentControlCriteria As String     ' String created from ParentIDs used to select records
        //    '
        //    ' ----- future
        //    '
        //    Public IgnoreContentControl As Boolean     ' if true, all records in the source are in this content
        //    Public AliasName As String                 ' Field Name of the required "name" field
        //    Public AliasID As String                   ' Field Name of the required "id" field
        //    '
        //    ' ----- removed
        //    '
        //    Public SingleRecord As Boolean             ' removeme
        //    '    Type as integer                        ' removeme
        //    Public TableName As String                 ' removeme
        //    Public DataSourceID As Integer                ' removeme
        //    '
        //    ' ----- new
        //    '
        //    Public AllowTopicRules As Boolean          ' For admin edit page
        //    Public AllowContentTracking As Boolean     ' For admin edit page
        //    Public AllowCalendarEvents As Boolean      ' For admin edit page
        //    Public AllowMetaContent As Boolean         ' For admin edit page - Adds the Meta Content Section
        //    Public TimeStamp As String                 ' string that changes if any record in Content Definition changes, in memory only
        //End Structure


        //Private Type AdminColumnType
        //    FieldPointer as integer
        //    FieldWidth as integer
        //    FieldSortDirection as integer
        //End Type
        //
        //Private Type AdminType
        //    ColumnCount as integer
        //    Columns() As AdminColumnType
        //End Type
        //
        //-----------------------------------------------------------------------
        //   Messages
        //-----------------------------------------------------------------------
        //
        public const string landingPageDefaultHtml = "<h1>Lorem Ipsum</h1><p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Fusce venenatis enim non magna porta, quis ultricies magna tincidunt. Nam vel lobortis quam. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Praesent accumsan lectus nec viverra condimentum. Morbi non ante vitae mauris mollis venenatis. Nulla tincidunt sapien in pulvinar sollicitudin. Mauris nec mattis sem. Nullam dapibus commodo nunc. Quisque sit amet massa vitae metus volutpat laoreet non sit amet tortor. Proin scelerisque justo eros, nec rhoncus magna pellentesque id. Nunc et aliquet est, ac cursus arcu.</p><p>Suspendisse potenti. Vivamus finibus libero et lobortis efficitur. Ut felis nisi, lobortis sed justo tempus, maximus placerat erat. Nunc elit lacus, condimentum ut malesuada ullamcorper, sodales sed nibh. Aliquam scelerisque lectus vitae mattis suscipit. Phasellus lobortis imperdiet nibh. Morbi ut est euismod, semper lectus nec, tempor quam. Pellentesque auctor bibendum nisl, in pulvinar elit scelerisque quis. Quisque ultrices nulla quis fringilla condimentum. Pellentesque venenatis quam non arcu venenatis, eget consectetur ante luctus. Sed non porta ante.</p><p>Aenean sagittis semper commodo. Suspendisse elementum dignissim sagittis. Etiam aliquet nisl vitae vestibulum sodales. Aenean tristique tristique quam ut faucibus. In hac habitasse platea dictumst. Fusce id est nisi. Nullam posuere ex nibh, id ornare ipsum pretium id. Maecenas ut nunc pellentesque mi tincidunt faucibus. Donec eget laoreet nisi. Nam vel tincidunt risus. Etiam faucibus tortor a sollicitudin accumsan.</p>";
        public const string Msg_AuthoringDeleted = "<b>Record Deleted</b><br>" + SpanClassAdminSmall + "This record was deleted and will be removed when publishing is complete.</SPAN>";
        public const string Msg_AuthoringInserted = "<b>Record Added</b><br>" + SpanClassAdminSmall + "This record was added and will display when publishing is complete.</span>";
        public const string Msg_EditLock = "<b>Edit Locked</b><br>" + SpanClassAdminSmall + "This record is currently being edited by <EDITNAME>.<br>"
                                                + "This lock will be released when the user releases the record, or at <EDITEXPIRES> (about <EDITEXPIRESMINUTES> minutes).</span>";
        public const string Msg_WorkflowDisabled = "<b>Immediate Authoring</b><br>" + SpanClassAdminSmall + "Changes made will be reflected on the web site immediately.</span>";
        public const string Msg_ContentWorkflowDisabled = "<b>Immediate Authoring Content Definition</b><br>" + SpanClassAdminSmall + "Changes made will be reflected on the web site immediately.</span>";
        public const string Msg_AuthoringRecordNotModifed = "" + SpanClassAdminSmall + "No changes have been saved to this record.</span>";
        public const string Msg_AuthoringRecordModifed = "<b>Edits Pending</b><br>" + SpanClassAdminSmall + "This record has been edited by <EDITNAME>.<br>"
                                                + "To publish these edits, submit for publishing, or have an administrator 'Publish Changes'.</span>";
        public const string Msg_AuthoringRecordModifedAdmin = "<b>Edits Pending</b><br>" + SpanClassAdminSmall + "This record has been edited by <EDITNAME>.<br>"
                                                + "To publish these edits immediately, hit 'Publish Changes'.<br>"
                                                + "To submit these changes for workflow publishing, hit 'Submit for Publishing'.</span>";
        public const string Msg_AuthoringSubmitted = "<b>Edits Submitted for Publishing</b><br>" + SpanClassAdminSmall + "This record has been edited and was submitted for publishing by <EDITNAME>.</span>";
        public const string Msg_AuthoringSubmittedAdmin = "<b>Edits Submitted for Publishing</b><br>" + SpanClassAdminSmall + "This record has been edited and was submitted for publishing by <EDITNAME>.<br>"
                                                + "As an administrator, you can make changes to this submitted record.<br>"
                                                + "To publish these edits immediately, hit 'Publish Changes'.<br>"
                                                + "To deny these edits, hit 'Abort Edits'.<br>"
                                                + "To approve these edits for workflow publishing, hit 'Approve for Publishing'."
                                                + "</span>";
        public const string Msg_AuthoringApproved = "<b>Edits Approved for Publishing</b><br>" + SpanClassAdminSmall + "This record has been edited and approved for publishing.<br>"
                                                + "No further changes can be made to this record until an administrator publishs, or aborts publishing."
                                                + "</span>";
        public const string Msg_AuthoringApprovedAdmin = "<b>Edits Approved for Publishing</b><br>" + SpanClassAdminSmall + "This record has been edited and approved for publishing.<br>"
                                                + "No further changes can be made to this record until an administrator publishs, or aborts publishing.<br>"
                                                + "To publish these edits immediately, hit 'Publish Changes'.<br>"
                                                + "To deny these edits, hit 'Abort Edits'."
                                                + "</span>";
        public const string Msg_AuthoringSubmittedNotification = "The following Content has been submitted for publication. Instructions on how to publish this content to web site are at the bottom of this message.<br>"
                                                + "<br>"
                                                + "website: <DOMAINNAME><br>"
                                                + "Name: <RECORDNAME><br>"
                                                + "<br>"
                                                + "Content: <CONTENTNAME><br>"
                                                + "Record Number: <RECORDID><br>"
                                                + "<br>"
                                                + "Submitted: <SUBMITTEDDATE><br>"
                                                + "Submitted By: <SUBMITTEDNAME><br>"
                                                + "<br>"
                                                + "<p>This content has been modified and was submitted for publication by the individual shown above. You were sent this notification because you are a member of the Editors Group for the content that has been changed.</p>"
                                                + "<p>To publish this content immediately, click on the website link above, check off this record in the box to the far left and click the \"Publish Selected Records\" button.</p>"
                                                + "<p>To edit the record, click the \"edit\" link for this record, make the desired changes and click the \"Publish\" button.</p>";
        //& "<p>This content has been modified and was submitted for publication by the individual shown above. You were sent this notification because your account on this system is a member of the Editors Group for the content that has been changed.</p>" _
        //& "<p>To publish this content immediately, click on the website link above and locate this record in the list of modified records presented. his content has been modified and was submitted for publication by the individual shown above. Click the 'Admin Site' link to edit the record, and hit the publish button.</p>" _
        //& "<p>To approved this record for workflow publishing, locate the record as described above, and hit the 'Approve for Publishing' button.</p>" _
        //& "<p>To publish all content records approved, go to the Workflow Publishing Screen (on the Administration Menu or the Administration Site) and hit the 'Publish Approved Records' button.</p>"
        public const string PageNotAvailable_Msg = "This page is not currently available. <br>"
                            + "Please use your back button to return to the previous page. <br>";
        public const string NewPage_Msg = "";
        //
        //Public Const htmlDoc_JavaStreamChunk = 100
        //Public Const htmlDoc_OutStreamStandard = 0
        //Public Const htmlDoc_OutStreamJavaScript = 1
        public const string main_BakeHeadDelimiter = "#####MultilineFlag#####";
        public const int navStruc_Descriptor = 1; // Descriptors:0 = RootPage, 1 = Parent Page, 2 = Current Page, 3 = Child Page
        public const int navStruc_Descriptor_CurrentPage = 2;
        public const int navStruc_Descriptor_ChildPage = 3;
        public const int navStruc_TemplateId = 7;
        public const string blockMessageDefault = "<p>The content on this page has restricted access. If you have a username and password for this system, <a href=\"?method=login\" rel=\"nofollow\">Click Here</a>. For more information, please contact the administrator.</p>";
        public const int main_BlockSourceDefaultMessage = 0;
        public const int main_BlockSourceCustomMessage = 1;
        public const int main_BlockSourceLogin = 2;
        public const int main_BlockSourceRegistration = 3;
        public const string main_FieldDelimiter = " , ";
        public const string main_LineDelimiter = " ,, ";
        public const int main_IPosType = 0;
        public const int main_IPosCaption = 1;
        public const int main_IPosRequired = 2;
        public const int main_IPosMax = 2; // value checked for the line before decoding
        public const int main_IPosPeopleField = 3;
        public const int main_IPosGroupName = 3;
        //
        public struct main_FormPagetype_InstType {
            public int Type;
            public string Caption;
            public bool REquired;
            public string PeopleField;
            public string GroupName;
        }
        //
        public struct main_FormPagetype {
            public string PreRepeat;
            public string PostRepeat;
            public string RepeatCell;
            public string AddGroupNameList;
            public bool AuthenticateOnFormProcess;
            public main_FormPagetype_InstType[] Inst;
        }
        //
        // Cache the input selects (admin uses the same ones over and over)
        //
        public class cacheInputSelectClass {
            public string SelectRaw;
            public string ContentName;
            public string Criteria;
            public string CurrentValue;
        }
        //
        // -- htmlAssetTypes
        public enum htmlAssetTypeEnum {
            script, // -- script at end of body (code or link)
            style, // -- css style at end of body (code or link)
            scriptOnLoad // -- special case, text is assumed to be script to run on load
        }
        //
        // -- assets to be added to the head section (and end-of-body) of html documents
        public class htmlAssetClass {
            public htmlAssetTypeEnum assetType; // the type of asset, css, js, etc
            public bool inHead; // if true, asset goes in head else it goes at end of body
            public bool isLink; // if true, the content property is a link to the asset, else use the content as the asset
            public string content; // either link or content depending on the isLink property
            public string addedByMessage; // message used during debug to show where the asset came from
            public int sourceAddonId; // if this asset was added from an addon, this is the addonId.
            public bool canBeMerged;
        }
        //
        // -- metaDescription
        public struct htmlMetaClass {
            public string content; // the description, title, etc.
            public string addedByMessage; // message used during debug to show where the asset came from
        }
        //
        // -- tasks
        public const string taskCommandBuildCsv = "buildcsv";
        public const string taskCommandBuildXml = "buildxml";
    }
    public static class taskQueueCommandEnumModule {
        public const string runAddon = "runaddon";
    }

    public class cmdDetailClass {
        public int addonId;
        public string addonName;
        public Dictionary<string, string> docProperties;
    }
    //
    //-----------------------------------------------------------------------
    //   legacy mainClass arguments
    //   REFACTOR - organize and rename
    //-----------------------------------------------------------------------
    //
    public struct NameValuePrivateType {
        public string Name;
        public string Value;
    }
    //
    //====================================================================================================
    //
    public class docPropertiesClass {
        public string Name;
        public string Value;
        public string NameValue;
        public bool IsForm;
        public bool IsFile;
        //Public FileContent() As Byte
        public string tempfilename;
        public int FileSize;
        public string fileType;
    }
    //
    // SF Resize Algorithms
    //
    public enum imageResizeAlgorithms {
        Box = 0,
        Triangle = 1,
        Hermite = 2,
        Bell = 3,
        BSpline = 4,
        Lanczos3 = 5,
        Mitchell = 6,
        Stretch = 7
    }

}
