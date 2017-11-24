
Option Explicit On
Option Strict On

Imports Contensive.BaseClasses

Namespace Contensive.Core
    Public Module constants
        '
        ' code version for this build. This is saved in a site property and checked in the housekeeping event - checkDataVersion
        '
        Public Const codeVersion As Integer = 0
        '
        ' -- content names
        Public Const cnBlank As String = ""
        Public Const cnDataSources As String = "data sources"
        Public Const cnPeople As String = "people"
        Public Const cnAddons As String = "Add-ons"
        Public Const cnNavigatorEntries As String = "Navigator Entries"
        '
        Public Const requestAppRootPath As String = "/"
        '
        ' -- buttons
        Public Const ButtonCreateFields As String = " Create Fields "
        Public Const ButtonRun As String = "     Run     "
        Public Const ButtonSelect As String = "  Select "
        Public Const ButtonFindAndReplace As String = " Find and Replace "
        Public Const ButtonIISReset As String = " IIS Reset "
        Public Const ButtonCancel As String = " Cancel "
        '
        Public Const protectedContentSetControlFieldList As String = "ID,CREATEDBY,DATEADDED,MODIFIEDBY,MODIFIEDDATE,CONTENTCONTROLID"
        '
        Public Const HTMLEditorDefaultCopyStartMark As String = "<!-- cc -->"
        Public Const HTMLEditorDefaultCopyEndMark As String = "<!-- /cc -->"
        Public Const HTMLEditorDefaultCopyNoCr As String = HTMLEditorDefaultCopyStartMark & "<p><br></p>" & HTMLEditorDefaultCopyEndMark
        Public Const HTMLEditorDefaultCopyNoCr2 As String = "<p><br></p>"
        '
        Public Const IconWidthHeight As String = " width=21 height=22 "
        '
        Public Const baseCollectionGuid As String = "{7C6601A7-9D52-40A3-9570-774D0D43D758}"        ' part of software dist - base cdef plus addons with classes in in core library, plus depenancy on coreCollection
        Public Const CoreCollectionGuid As String = "{8DAABAE6-8E45-4CEE-A42C-B02D180E799B}"                  ' contains core Contensive objects, loaded from Library
        Public Const ApplicationCollectionGuid As String = "{C58A76E2-248B-4DE8-BF9C-849A960F79C6}"           ' exported from application during upgrade
        '
        ' -- navigator entries
        Public Const addonGuidManageAddon As String = "{DBA354AB-5D3E-4882-8718-CF23CAAB7927}"
        '
        ' -- addons
        Public Const addonGuidBaseStlyles As String = "{0dd7df28-4924-4881-a1d8-421824f5c2d1}"
        Public Const addonGuidAdminSite As String = "{c2de2acf-ca39-4668-b417-aa491e7d8460}"
        Public Const addonGuidDashboard As String = "{4BA7B4A2-ED6C-46C5-9C7B-8CE251FC8FF5}"
        Public Const addonGuidPersonalization As String = "{C82CB8A6-D7B9-4288-97FF-934080F5FC9C}"
        Public Const addonGuidTextBox As String = "{7010002E-5371-41F7-9C77-0BBFF1F8B728}"
        Public Const addonGuidContentBox As String = "{E341695F-C444-4E10-9295-9BEEC41874D8}"
        Public Const addonGuidDynamicMenu As String = "{DB1821B3-F6E4-4766-A46E-48CA6C9E4C6E}"
        Public Const addonGuidChildList As String = "{D291F133-AB50-4640-9A9A-18DB68FF363B}"
        Public Const addonGuidDynamicForm As String = "{8284FA0C-6C9D-43E1-9E57-8E9DD35D2DCC}"
        Public Const addonGuidAddonManager As String = "{1DC06F61-1837-419B-AF36-D5CC41E1C9FD}"
        Public Const addonGuidFormWizard As String = "{2B1384C4-FD0E-4893-B3EA-11C48429382F}"
        Public Const addonGuidImportWizard As String = "{37F66F90-C0E0-4EAF-84B1-53E90A5B3B3F}"
        Public Const addonGuidJQuery As String = "{9C882078-0DAC-48E3-AD4B-CF2AA230DF80}"
        Public Const addonGuidJQueryUI As String = "{840B9AEF-9470-4599-BD47-7EC0C9298614}"
        Public Const addonGuidImportProcess As String = "{5254FAC6-A7A6-4199-8599-0777CC014A13}"
        Public Const addonGuidStructuredDataProcessor As String = "{65D58FE9-8B76-4490-A2BE-C863B372A6A4}"
        Public Const addonGuidjQueryFancyBox As String = "{24C2DBCF-3D84-44B6-A5F7-C2DE7EFCCE3D}"
        Public Const addonGuidSiteStructureGuid As String = "{8CDD7960-0FCA-4042-B5D8-3A65BE487AC4}"
        'Public Const addonGuidLoginDefaultPage   As String  = "{288a7ee1-9d93-4058-bcd9-c9cd29d25ec8}"
        ' -- Login Page displays the currently selected login form addon
        Public Const addonGuidLoginPage As String = "{288a7ee1-9d93-4058-bcd9-c9cd29d25ec8}"
        ' -- Login Form, this is the addonGuid of the default login form. Login Page calls the addon
        Public Const addonGuidLoginForm As String = "{E23C5941-19C2-4164-BCFD-83D6DD42F651}"
        Public Const addonGuidPageManager As String = "{3a01572e-0f08-4feb-b189-18371752a3c3}"
        '
        ' -- content
        Public Const DefaultLandingPageGuid As String = "{925F4A57-32F7-44D9-9027-A91EF966FB0D}"
        Public Const DefaultLandingSectionGuid As String = "{D882ED77-DB8F-4183-B12C-F83BD616E2E1}"
        Public Const DefaultTemplateGuid As String = "{47BE95E4-5D21-42CC-9193-A343241E2513}"
        Public Const DefaultDynamicMenuGuid As String = "{E8D575B9-54AE-4BF9-93B7-C7E7FE6F2DB3}"
        '
        ' -- instance id used when running addons in the addon site 
        Public Const adminSiteInstanceId As String = "{E5418109-1206-43C5-A4F8-425E28BC629C}"
        '
        Public Const fpoContentBox As String = "{1571E62A-972A-4BFF-A161-5F6075720791}"
        '
        Public Const sfImageExtList As String = "jpg,jpeg,gif,png"
        '
        Public Const PageChildListInstanceID As String = "{ChildPageList}"
        '
        Public Const cr = vbCrLf & vbTab
        Public Const cr2 = cr & vbTab
        Public Const cr3 = cr2 & vbTab
        Public Const cr4 = cr3 & vbTab
        Public Const cr5 = cr4 & vbTab
        Public Const cr6 = cr5 & vbTab
        '
        Public Const AddonOptionConstructor_BlockNoAjax As String = "Wrapper=[Default:0|None:-1|ListID(Wrappers)]" & vbCrLf & "css Container id" & vbCrLf & "css Container class"
        Public Const AddonOptionConstructor_Block As String = "Wrapper=[Default:0|None:-1|ListID(Wrappers)]" & vbCrLf & "As Ajax=[If Add-on is Ajax:0|Yes:1]" & vbCrLf & "css Container id" & vbCrLf & "css Container class"
        Public Const AddonOptionConstructor_Inline As String = "As Ajax=[If Add-on is Ajax:0|Yes:1]" & vbCrLf & "css Container id" & vbCrLf & "css Container class"
        '
        ' Constants used as arguments to SiteBuilderClass.CreateNewSite
        '
        Public Const SiteTypeBaseAsp = 1
        Public Const sitetypebaseaspx = 2
        Public Const SiteTypeDemoAsp = 3
        Public Const SiteTypeBasePhp = 4
        '
        'Public Const AddonNewParse = True
        '
        Public Const AddonOptionConstructor_ForBlockText As String = "AllowGroups=[listid(groups)]checkbox"
        Public Const AddonOptionConstructor_ForBlockTextEnd As String = ""
        Public Const BlockTextStartMarker As String = "<!-- BLOCKTEXTSTART -->"
        Public Const BlockTextEndMarker As String = "<!-- BLOCKTEXTEND -->"
        '
        'Private Declare Sub Sleep Lib "kernel32" (ByVal dwMilliseconds As Integer)
        'Private Declare Function GetExitCodeProcess Lib "kernel32" (ByVal hProcess As Integer, ByVal lpExitCode As Integer) As Integer
        'Private Declare Function timeGetTime Lib "winmm.dll" () As Integer
        'Private Declare Function OpenProcess Lib "kernel32" (ByVal dwDesiredAccess As Integer, ByVal bInheritHandle As Integer, ByVal dwProcessId As Integer) As Integer
        'Private Declare Function CloseHandle Lib "kernel32" (ByVal hObject As Integer) As Integer
        '
        Public Const InstallFolderName As String = "Install"
        Public Const DownloadFileRootNode As String = "collectiondownload"
        Public Const CollectionFileRootNode As String = "collection"
        Public Const CollectionFileRootNodeOld As String = "addoncollection"
        Public Const CollectionListRootNode As String = "collectionlist"
        '
        Public Const LegacyLandingPageName As String = "Landing Page Content"
        Public Const DefaultNewLandingPageName As String = "Home"
        Public Const DefaultLandingSectionName As String = "Home"
        '
        Public Const defaultLandingPageHtml As String = ""
        Public Const defaultTemplateName As String = "Default"
        'Public Const defaultTemplateHtml As String = "{% {""addon"":{""addon"":""menu pages"",""name"":""Default""}} %}{% ""Content Box"" %}"
        Public Const defaultTemplateHomeFilename As String = "ContensiveBase\TemplateHomeDefault.html"
        '
        '
        '
        Public Const ignoreInteger As Integer = 0
        Public Const ignoreString As String = ""
        '
        ' ----- Errors Specific to the Contensive Objects
        '
        'Public Const ignoreInteger = KmaObjectError + 1
        'Public Const KmaccErrorServiceStopped = KmaObjectError + 2
        '
        Public Const UserErrorHeadline As String = "<p class=""ccError"">There was a problem with this page.</p>"
        ''
        '' ----- Errors connecting to server
        ''
        'Public Const ccError_InvalidAppName = 100
        'Public Const ccError_ErrorAddingApp = 101
        'Public Const ccError_ErrorDeletingApp = 102
        'Public Const ccError_InvalidFieldName = 103     ' Invalid parameter name
        'Public Const ignoreString = 104
        'Public Const ignoreString = 105
        'Public Const ccError_NotConnected = 106             ' Attempt to execute a command without a connection
        ''
        '
        '
        'Public Const ccStatusCode_Base = ignoreInteger
        'Public Const ccStatusCode_ControllerCreateFailed = ccStatusCode_Base + 1
        'Public Const ccStatusCode_ControllerInProcess = ccStatusCode_Base + 2
        'Public Const ccStatusCode_ControllerStartedWithoutService = ccStatusCode_Base + 3
        '
        ' ----- Previous errors, can be replaced
        '
        'Public Const KmaError_UnderlyingObject_Msg  As String  = "An error occurred in an underlying routine."
        'Public Const KmaccErrorServiceStopped_Msg  As String  = "The Contensive CSv Service is not running."
        'Public Const KmaError_BadObject_Msg  As String  = "Server Object is not valid."
        'Public Const ignoreString  As String  = "Server is busy with internal builder."
        '
        'Public Const KmaError_InvalidArgument_Msg  As String  = "Invalid Argument"
        'Public Const KmaError_UnderlyingObject_Msg  As String  = "An error occurred in an underlying routine."
        'Public Const KmaccErrorServiceStopped_Msg  As String  = "The Contensive CSv Service is not running."
        'Public Const KmaError_BadObject_Msg  As String  = "Server Object is not valid."
        'Public Const ignoreString  As String  = "Server is busy with internal builder."
        'Public Const KmaError_InvalidArgument_Msg  As String  = "Invalid Argument"
        '
        '-----------------------------------------------------------------------
        '   GetApplicationList indexes
        '-----------------------------------------------------------------------
        '
        Public Const AppList_Name = 0
        Public Const AppList_Status = 1
        Public Const AppList_ConnectionsActive = 2
        Public Const AppList_ConnectionString = 3
        Public Const AppList_DataBuildVersion = 4
        Public Const AppList_LicenseKey = 5
        Public Const AppList_RootPath = 6
        Public Const AppList_PhysicalFilePath = 7
        Public Const AppList_DomainName = 8
        Public Const AppList_DefaultPage = 9
        Public Const AppList_AllowSiteMonitor = 10
        Public Const AppList_HitCounter = 11
        Public Const AppList_ErrorCount = 12
        Public Const AppList_DateStarted = 13
        Public Const AppList_AutoStart = 14
        Public Const AppList_Progress = 15
        Public Const AppList_PhysicalWWWPath = 16
        Public Const AppListCount = 17
        '
        '-----------------------------------------------------------------------
        '   System MemberID - when the system does an update, it uses this member
        '-----------------------------------------------------------------------
        '
        Public Const SystemMemberID = 0
        '
        '-----------------------------------------------------------------------
        ' ----- old (OptionKeys for available Options)
        '-----------------------------------------------------------------------
        '
        Public Const OptionKeyProductionLicense = 0
        Public Const OptionKeyDeveloperLicense = 1
        '
        '-----------------------------------------------------------------------
        ' ----- LicenseTypes, replaced OptionKeys
        '-----------------------------------------------------------------------
        '
        Public Const LicenseTypeInvalid = -1
        Public Const LicenseTypeProduction = 0
        Public Const LicenseTypeTrial = 1
        '
        '-----------------------------------------------------------------------
        ' ----- Active Content Definitions
        '-----------------------------------------------------------------------
        '
        Public Const ACTypeDate As String = "DATE"
        Public Const ACTypeVisit As String = "VISIT"
        Public Const ACTypeVisitor As String = "VISITOR"
        Public Const ACTypeMember As String = "MEMBER"
        Public Const ACTypeOrganization As String = "ORGANIZATION"
        Public Const ACTypeChildList As String = "CHILDLIST"
        Public Const ACTypeContact As String = "CONTACT"
        Public Const ACTypeFeedback As String = "FEEDBACK"
        Public Const ACTypeLanguage As String = "LANGUAGE"
        Public Const ACTypeAggregateFunction As String = "AGGREGATEFUNCTION"
        Public Const ACTypeAddon As String = "ADDON"
        Public Const ACTypeImage As String = "IMAGE"
        Public Const ACTypeDownload As String = "DOWNLOAD"
        Public Const ACTypeEnd As String = "END"
        Public Const ACTypeTemplateContent As String = "CONTENT"
        Public Const ACTypeTemplateText As String = "TEXT"
        'Public Const ACTypeDynamicMenu  As String  = "DYNAMICMENU"
        Public Const ACTypeWatchList As String = "WATCHLIST"
        Public Const ACTypeRSSLink As String = "RSSLINK"
        Public Const ACTypePersonalization As String = "PERSONALIZATION"
        Public Const ACTypeDynamicForm As String = "DYNAMICFORM"
        '
        Public Const ACTagEnd As String = "<ac type=""" & ACTypeEnd & """>"
        '
        ' ----- PropertyType Definitions
        '
        Public Const PropertyTypeMember = 0
        Public Const PropertyTypeVisit = 1
        Public Const PropertyTypeVisitor = 2
        '
        '-----------------------------------------------------------------------
        ' ----- Port Assignments
        '-----------------------------------------------------------------------
        '
        Public Const WinsockPortWebOut = 4000
        Public Const WinsockPortServerFromWeb = 4001
        Public Const WinsockPortServerToClient = 4002
        '
        Public Const Port_ContentServerControlDefault = 4531
        Public Const Port_SiteMonitorDefault = 4532
        '
        Public Const RMBMethodHandShake = 1
        Public Const RMBMethodMessage = 3
        Public Const RMBMethodTestPoint = 4
        Public Const RMBMethodInit = 5
        Public Const RMBMethodClosePage = 6
        Public Const RMBMethodOpenCSContent = 7
        '
        ' ----- Position equates for the Remote Method Block
        '
        Public Const RMBPositionLength = 0             ' Length of the RMB
        Public Const RMBPositionSourceHandle = 4       ' Handle generated by the source of the command
        Public Const RMBPositionMethod = 8             ' Method in the method block
        Public Const RMBPositionArgumentCount = 12     ' The number of arguments in the Block
        Public Const RMBPositionFirstArgument = 16     ' The offset to the first argu
        '
        '-----------------------------------------------------------------------
        '   Remote Connections
        '   List of current remove connections for Remote Monitoring/administration
        '-----------------------------------------------------------------------
        '
        Public Class RemoteAdministratorType
            Dim RemoteIP As String
            Dim RemotePort As Integer
        End Class
        '
        ' -- Default username/password
        '
        Public Const DefaultServerUsername As String = "root"
        Public Const DefaultServerPassword As String = "contensive"
        '
        ' -- Request Names
        '
        Public Const rnRedirectContentId As String = "rc"
        Public Const rnRedirectRecordId As String = "ri"
        Public Const rnPageId As String = "bid"
        '
        '-----------------------------------------------------------------------
        '   Form Contension Strategy
        '        '       elements of the form are named  "ElementName"
        '
        '       This prevents form elements from different forms from interfearing
        '       with each other, and with developer generated forms.
        '
        '       All forms requires:
        '           a FormId (text), containing the formid string
        '           a [formid]Type (text), as defined in FormTypexxx in CommonModule
        '
        '       Forms have two primary sections: GetForm and ProcessForm
        '
        '       Any form that has a GetForm method, should have the process form
        '       in the cmc.main_init, selected with this [formid]type hidden (not the
        '       GetForm method). This is so the process can alter the stream
        '       output for areas before the GetForm call.
        '
        '       System forms, like tools panel, that may appear on any page, have
        '       their process call in the cmc.main_init.
        '
        '       Popup forms, like ImageSelector have their processform call in the
        '       cmc.main_init because no .asp page exists that might contain a call
        '       the process section.
        '
        '-----------------------------------------------------------------------
        '
        Public Const FormTypeToolsPanel As String = "do30a8vl29"
        Public Const FormTypeActiveEditor As String = "l1gk70al9n"
        Public Const FormTypeImageSelector As String = "ila9c5s01m"
        Public Const FormTypePageAuthoring As String = "2s09lmpalb"
        Public Const FormTypeMyProfile As String = "89aLi180j5"
        Public Const FormTypeLogin As String = "login"
        'Public Const FormTypeLogin As String = "l09H58a195"
        Public Const FormTypeSendPassword As String = "lk0q56am09"
        Public Const FormTypeJoin As String = "6df38abv00"
        Public Const FormTypeHelpBubbleEditor As String = "9df019d77sA"
        Public Const FormTypeAddonSettingsEditor As String = "4ed923aFGw9d"
        Public Const FormTypeAddonStyleEditor As String = "ar5028jklkfd0s"
        Public Const FormTypeSiteStyleEditor As String = "fjkq4w8794kdvse"
        'Public Const FormTypeAggregateFunctionProperties As String = "9wI751270"
        '
        '-----------------------------------------------------------------------
        '   Hardcoded profile form const
        '-----------------------------------------------------------------------
        '
        Public Const rnMyProfileTopics As String = "profileTopics"
        '
        '-----------------------------------------------------------------------
        ' Legacy - replaced with HardCodedPages
        '   Intercept Page Strategy
        '
        '       RequestnameInterceptpage = InterceptPage number from the input stream
        '       InterceptPage = Global variant with RequestnameInterceptpage value read during early Init
        '
        '       Intercept pages are complete pages that appear instead of what
        '       the physical page calls.
        '-----------------------------------------------------------------------
        '
        'Public Const RequestNameInterceptpage As String = "ccIPage"
        '
        Public Const LegacyInterceptPageSNResourceLibrary As String = "s033l8dm15"
        Public Const LegacyInterceptPageSNSiteExplorer As String = "kdif3318sd"
        Public Const LegacyInterceptPageSNImageUpload As String = "ka983lm039"
        Public Const LegacyInterceptPageSNMyProfile As String = "k09ddk9105"
        Public Const LegacyInterceptPageSNLogin As String = "6ge42an09a"
        Public Const LegacyInterceptPageSNPrinterVersion As String = "l6d09a10sP"
        Public Const LegacyInterceptPageSNUploadEditor As String = "k0hxp2aiOZ"
        '
        '-----------------------------------------------------------------------
        ' Ajax functions intercepted during init, answered and response closed
        ' todo - convert built-in request name functions to remoteMethods
        '   These are hard-coded internal Contensive functions
        '   These should eventually be replaced with (HardcodedAddons) remote methods
        '   They should all be prefixed "cc"
        '   They are called with cj.ajax.qs(), setting RequestNameAjaxFunction=name in the qs
        '   These name=value pairs go in the QueryString argument of the javascript cj.ajax.qs() function
        '-----------------------------------------------------------------------
        '
        'Public Const RequestNameOpenSettingPage As String = "settingpageid"
        Public Const RequestNameAjaxFunction As String = "ajaxfn"
        Public Const RequestNameAjaxFastFunction As String = "ajaxfastfn"
        '
        Public Const AjaxOpenAdminNav As String = "aps89102kd"
        Public Const AjaxOpenAdminNavGetContent As String = "d8475jkdmfj2"
        Public Const AjaxCloseAdminNav As String = "3857fdjdskf91"
        Public Const AjaxAdminNavOpenNode As String = "8395j2hf6jdjf"
        Public Const AjaxAdminNavOpenNodeGetContent As String = "eieofdwl34efvclaeoi234598"
        Public Const AjaxAdminNavCloseNode As String = "w325gfd73fhdf4rgcvjk2"
        '
        Public Const AjaxCloseIndexFilter As String = "k48smckdhorle0"
        Public Const AjaxOpenIndexFilter As String = "Ls8jCDt87kpU45YH"
        Public Const AjaxOpenIndexFilterGetContent As String = "llL98bbJQ38JC0KJm"
        Public Const AjaxStyleEditorAddStyle As String = "ajaxstyleeditoradd"
        Public Const AjaxPing As String = "ajaxalive"
        Public Const AjaxGetFormEditTabContent As String = "ajaxgetformedittabcontent"
        Public Const AjaxData As String = "data"
        Public Const AjaxGetVisitProperty As String = "getvisitproperty"
        Public Const AjaxSetVisitProperty As String = "setvisitproperty"
        Public Const AjaxGetDefaultAddonOptionString As String = "ccGetDefaultAddonOptionString"
        Public Const ajaxGetFieldEditorPreferenceForm As String = "ajaxgetfieldeditorpreference"
        '
        '-----------------------------------------------------------------------
        '
        ' no - for now just use ajaxfn in the cj.ajax.qs call
        '   this is more work, and I do not see why it buys anything new or better
        '
        '   Hard-coded addons
        '       these are internal Contensive functions
        '       can be called with just /addonname?querystring
        '       call them with cj.ajax.addon() or cj.ajax.addonCallback()
        '       are first in the list of checks when a URL rewrite is detected in Init()
        '       should all be prefixed with 'cc'
        '-----------------------------------------------------------------------
        '
        'Public Const HardcodedAddonGetDefaultAddonOptionString As String = "ccGetDefaultAddonOptionString"
        '
        '-----------------------------------------------------------------------
        '   Remote Methods
        '       ?RemoteMethodAddon=string
        '       calls an addon (if marked to run as a remote method)
        '       blocks all other Contensive output (tools panel, javascript, etc)
        '-----------------------------------------------------------------------
        '
        Public Const RequestNameRemoteMethodAddon As String = "remotemethodaddon"
        '
        '-----------------------------------------------------------------------
        '   Hard Coded Pages
        '       ?Method=string
        '       Querystring based so they can be added to URLs, preserving the current page for a return
        '       replaces output stream with html output
        '-----------------------------------------------------------------------
        '
        Public Const RequestNameHardCodedPage As String = "method"
        '
        Public Const HardCodedPageLogin As String = "login"
        Public Const HardCodedPageLoginDefault As String = "logindefault"
        Public Const HardCodedPageMyProfile As String = "myprofile"
        Public Const HardCodedPagePrinterVersion As String = "printerversion"
        Public Const HardCodedPageResourceLibrary As String = "resourcelibrary"
        Public Const HardCodedPageLogoutLogin As String = "logoutlogin"
        Public Const HardCodedPageLogout As String = "logout"
        Public Const HardCodedPageSiteExplorer As String = "siteexplorer"
        'Public Const HardCodedPageForceMobile As String = "forcemobile"
        'Public Const HardCodedPageForceNonMobile As String = "forcenonmobile"
        Public Const HardCodedPageNewOrder As String = "neworderpage"
        Public Const HardCodedPageStatus As String = "status"
        'Public Const HardCodedPageGetJSPage As String = "getjspage"
        'Public Const HardCodedPageGetJSLogin As String = "getjslogin"
        Public Const HardCodedPageRedirect As String = "redirect"
        Public Const HardCodedPageExportAscii As String = "exportascii"
        Public Const HardCodedPagePayPalConfirm As String = "paypalconfirm"
        Public Const HardCodedPageSendPassword As String = "sendpassword"
        '
        '-----------------------------------------------------------------------
        '   Option values
        '       does not effect output directly
        '-----------------------------------------------------------------------
        '
        Public Const RequestNamePageOptions As String = "ccoptions"
        '
        Public Const PageOptionForceMobile As String = "forcemobile"
        Public Const PageOptionForceNonMobile As String = "forcenonmobile"
        Public Const PageOptionLogout As String = "logout"
        Public Const PageOptionPrinterVersion As String = "printerversion"
        '
        ' convert to options later
        '
        Public Const RequestNameDashboardReset As String = "ResetDashboard"
        '
        '-----------------------------------------------------------------------
        '   DataSource constants
        '-----------------------------------------------------------------------
        '
        Public Const DefaultDataSourceID = -1
        '
        '-----------------------------------------------------------------------
        ' ----- Type compatibility between databases
        '       Boolean
        '           Access      YesNo       true=1, false=0
        '           SQL Server  bit         true=1, false=0
        '           MySQL       bit         true=1, false=0
        '           Oracle      integer(1)  true=1, false=0
        '           Note: false does not equal NOT true
        '       Integer (Number)
        '           Access      Long        8 bytes, about E308
        '           SQL Server  int
        '           MySQL       integer
        '           Oracle      integer(8)
        '       Float
        '           Access      Double      8 bytes, about E308
        '           SQL Server  Float
        '           MySQL
        '           Oracle
        '       Text
        '           Access
        '           SQL Server
        '           MySQL
        '           Oracle
        '-----------------------------------------------------------------------
        '
        'Public Const SQLFalse As String = "0"
        'Public Const SQLTrue As String = "1"
        '
        '-----------------------------------------------------------------------
        ' ----- Style sheet definitions
        '-----------------------------------------------------------------------
        '
        Public Const defaultStyleFilename As String = "ccDefault.r5.css"
        Public Const StyleSheetStart As String = "<STYLE TYPE=""text/css"">"
        Public Const StyleSheetEnd As String = "</STYLE>"
        '
        Public Const SpanClassAdminNormal As String = "<span class=""ccAdminNormal"">"
        Public Const SpanClassAdminSmall As String = "<span class=""ccAdminSmall"">"
        '
        ' remove these from ccWebx
        '
        Public Const SpanClassNormal As String = "<span class=""ccNormal"">"
        Public Const SpanClassSmall As String = "<span class=""ccSmall"">"
        Public Const SpanClassLarge As String = "<span class=""ccLarge"">"
        Public Const SpanClassHeadline As String = "<span class=""ccHeadline"">"
        Public Const SpanClassList As String = "<span class=""ccList"">"
        Public Const SpanClassListCopy As String = "<span class=""ccListCopy"">"
        Public Const SpanClassError As String = "<span class=""ccError"">"
        Public Const SpanClassSeeAlso As String = "<span class=""ccSeeAlso"">"
        Public Const SpanClassEnd As String = "</span>"
        '
        '-----------------------------------------------------------------------
        ' ----- XHTML definitions
        '-----------------------------------------------------------------------
        '
        Public Const DTDTransitional As String = "<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">"
        '
        Public Const BR As String = "<br>"
        '
        '-----------------------------------------------------------------------
        ' AuthoringControl Types
        '-----------------------------------------------------------------------
        '
        Public Const AuthoringControlsEditing = 1
        Public Const AuthoringControlsSubmitted = 2
        Public Const AuthoringControlsApproved = 3
        Public Const AuthoringControlsModified = 4
        '
        '-----------------------------------------------------------------------
        ' ----- Panel and header colors
        '-----------------------------------------------------------------------
        '
        'Public Const "ccPanel" As String = "#E0E0E0"    ' The background color of a panel (black copy visible on it)
        'Public Const "ccPanelHilite" As String = "#F8F8F8"  '
        'Public Const "ccPanelShadow" As String = "#808080"  '
        '
        'Public Const HeaderColorBase As String = "#0320B0"   ' The background color of a panel header (reverse copy visible)
        'Public Const "ccPanelHeaderHilite" As String = "#8080FF" '
        'Public Const "ccPanelHeaderShadow" As String = "#000000" '
        '
        '-----------------------------------------------------------------------
        ' ----- Field type Definitions
        '       Field Types are numeric values that describe how to treat values
        '       stored as ContentFieldDefinitionType (FieldType property of FieldType Type.. ;)
        '-----------------------------------------------------------------------
        '
        Public Const FieldTypeIdInteger = 1       ' An long number
        Public Const FieldTypeIdText = 2          ' A text field (up to 255 characters)
        Public Const FieldTypeIdLongText = 3      ' A memo field (up to 8000 characters)
        Public Const FieldTypeIdBoolean = 4       ' A yes/no field
        Public Const FieldTypeIdDate = 5          ' A date field
        Public Const FieldTypeIdFile = 6          ' A filename of a file in the files directory.
        Public Const FieldTypeIdLookup = 7        ' A lookup is a FieldTypeInteger that indexes into another table
        Public Const FieldTypeIdRedirect = 8      ' creates a link to another section
        Public Const FieldTypeIdCurrency = 9      ' A Float that prints in dollars
        Public Const FieldTypeIdFileText = 10     ' Text saved in a file in the files area.
        Public Const FieldTypeIdFileImage = 11        ' A filename of a file in the files directory.
        Public Const FieldTypeIdFloat = 12        ' A float number
        Public Const FieldTypeIdAutoIdIncrement = 13 'long that automatically increments with the new record
        Public Const FieldTypeIdManyToMany = 14    ' no database field - sets up a relationship through a Rule table to another table
        Public Const FieldTypeIdMemberSelect = 15 ' This ID is a ccMembers record in a group defined by the MemberSelectGroupID field
        Public Const FieldTypeIdFileCSS = 16      ' A filename of a CSS compatible file
        Public Const FieldTypeIdFileXML = 17      ' the filename of an XML compatible file
        Public Const FieldTypeIdFileJavascript = 18 ' the filename of a javascript compatible file
        Public Const FieldTypeIdLink = 19           ' Links used in href tags -- can go to pages or resources
        Public Const FieldTypeIdResourceLink = 20   ' Links used in resources, link <img or <object. Should not be pages
        Public Const FieldTypeIdHTML = 21           ' LongText field that expects HTML content
        Public Const FieldTypeIdFileHTML = 22       ' TextFile field that expects HTML content
        Public Const FieldTypeIdMax = 22
        '
        ' ----- Field Descriptors for these type
        '       These are what are publicly displayed for each type
        '       See GetFieldTypeNameByType and vise-versa to translater
        '
        Public Const FieldTypeNameInteger As String = "Integer"
        Public Const FieldTypeNameText As String = "Text"
        Public Const FieldTypeNameLongText As String = "LongText"
        Public Const FieldTypeNameBoolean As String = "Boolean"
        Public Const FieldTypeNameDate As String = "Date"
        Public Const FieldTypeNameFile As String = "File"
        Public Const FieldTypeNameLookup As String = "Lookup"
        Public Const FieldTypeNameRedirect As String = "Redirect"
        Public Const FieldTypeNameCurrency As String = "Currency"
        Public Const FieldTypeNameImage As String = "Image"
        Public Const FieldTypeNameFloat As String = "Float"
        Public Const FieldTypeNameManyToMany As String = "ManyToMany"
        Public Const FieldTypeNameTextFile As String = "TextFile"
        Public Const FieldTypeNameCSSFile As String = "CSSFile"
        Public Const FieldTypeNameXMLFile As String = "XMLFile"
        Public Const FieldTypeNameJavascriptFile As String = "JavascriptFile"
        Public Const FieldTypeNameLink As String = "Link"
        Public Const FieldTypeNameResourceLink As String = "ResourceLink"
        Public Const FieldTypeNameMemberSelect As String = "MemberSelect"
        Public Const FieldTypeNameHTML As String = "HTML"
        Public Const FieldTypeNameHTMLFile As String = "HTMLFile"
        '
        Public Const FieldTypeNameLcaseInteger As String = "integer"
        Public Const FieldTypeNameLcaseText As String = "text"
        Public Const FieldTypeNameLcaseLongText As String = "longtext"
        Public Const FieldTypeNameLcaseBoolean As String = "boolean"
        Public Const FieldTypeNameLcaseDate As String = "date"
        Public Const FieldTypeNameLcaseFile As String = "file"
        Public Const FieldTypeNameLcaseLookup As String = "lookup"
        Public Const FieldTypeNameLcaseRedirect As String = "redirect"
        Public Const FieldTypeNameLcaseCurrency As String = "currency"
        Public Const FieldTypeNameLcaseImage As String = "image"
        Public Const FieldTypeNameLcaseFloat As String = "float"
        Public Const FieldTypeNameLcaseManyToMany As String = "manytomany"
        Public Const FieldTypeNameLcaseTextFile As String = "textfile"
        Public Const FieldTypeNameLcaseCSSFile As String = "cssfile"
        Public Const FieldTypeNameLcaseXMLFile As String = "xmlfile"
        Public Const FieldTypeNameLcaseJavascriptFile As String = "javascriptfile"
        Public Const FieldTypeNameLcaseLink As String = "link"
        Public Const FieldTypeNameLcaseResourceLink As String = "resourcelink"
        Public Const FieldTypeNameLcaseMemberSelect As String = "memberselect"
        Public Const FieldTypeNameLcaseHTML As String = "html"
        Public Const FieldTypeNameLcaseHTMLFile As String = "htmlfile"
        '
        '------------------------------------------------------------------------
        ' ----- Payment Options
        '------------------------------------------------------------------------
        '
        Public Const PayTypeCreditCardOnline = 0   ' Pay by credit card online
        Public Const PayTypeCreditCardByPhone = 1  ' Phone in a credit card
        Public Const PayTypeCreditCardByFax = 9    ' Phone in a credit card
        Public Const PayTypeCHECK = 2              ' pay by check to be mailed
        Public Const PayTypeCREDIT = 3             ' pay on account
        Public Const PayTypeNONE = 4               ' order total is $0.00. Nothing due
        Public Const PayTypeCHECKCOMPANY = 5       ' pay by company check
        Public Const PayTypeNetTerms = 6
        Public Const PayTypeCODCompanyCheck = 7
        Public Const PayTypeCODCertifiedFunds = 8
        Public Const PayTypePAYPAL = 10
        Public Const PAYDEFAULT = 0
        '
        '------------------------------------------------------------------------
        ' ----- Credit card options
        '------------------------------------------------------------------------
        '
        Public Const CCTYPEVISA = 0                ' Visa
        Public Const CCTYPEMC = 1                  ' MasterCard
        Public Const CCTYPEAMEX = 2                ' American Express
        Public Const CCTYPEDISCOVER = 3            ' Discover
        Public Const CCTYPENOVUS = 4               ' Novus Card
        Public Const CCTYPEDEFAULT = 0
        '
        '------------------------------------------------------------------------
        ' ----- Shipping Options
        '------------------------------------------------------------------------
        '
        Public Const SHIPGROUND = 0                ' ground
        Public Const SHIPOVERNIGHT = 1             ' overnight
        Public Const SHIPSTANDARD = 2              ' standard, whatever that is
        Public Const SHIPOVERSEAS = 3              ' to overseas
        Public Const SHIPCANADA = 4                ' to Canada
        Public Const SHIPDEFAULT = 0
        '
        '------------------------------------------------------------------------
        ' Debugging info
        '------------------------------------------------------------------------
        '
        Public Const TestPointTab = 2
        Public Const debug_TestPointTabChr As Char = "-"c
        Public CPTickCountBase As Double
        '
        '------------------------------------------------------------------------
        '   project width button defintions
        '------------------------------------------------------------------------
        '
        Public Const ButtonApply As String = "  Apply "
        Public Const ButtonLogin As String = "  Login  "
        Public Const ButtonLogout As String = "  Logout  "
        Public Const ButtonSendPassword As String = "  Send Password  "
        Public Const ButtonJoin As String = "   Join   "
        Public Const ButtonSave As String = "  Save  "
        Public Const ButtonOK As String = "     OK     "
        Public Const ButtonReset As String = "  Reset  "
        Public Const ButtonSaveAddNew As String = " Save + Add "
        'Public Const ButtonSaveAddNew As String = " Save > Add "
        'Public Const ButtonCancel As String = " Cancel "
        Public Const ButtonRestartContensiveApplication As String = " Restart Contensive Application "
        Public Const ButtonCancelAll As String = "  Cancel  "
        Public Const ButtonFind As String = "   Find   "
        Public Const ButtonDelete As String = "  Delete  "
        Public Const ButtonDeletePerson As String = " Delete Person "
        Public Const ButtonDeleteRecord As String = " Delete Record "
        Public Const ButtonDeleteEmail As String = " Delete Email "
        Public Const ButtonDeletePage As String = " Delete Page "
        Public Const ButtonFileChange As String = "   Upload   "
        Public Const ButtonFileDelete As String = "    Delete    "
        Public Const ButtonClose As String = "  Close   "
        Public Const ButtonAdd As String = "   Add    "
        Public Const ButtonAddChildPage As String = " Add Child "
        Public Const ButtonAddSiblingPage As String = " Add Sibling "
        Public Const ButtonContinue As String = " Continue >> "
        Public Const ButtonBack As String = "  << Back  "
        Public Const ButtonNext As String = "   Next   "
        Public Const ButtonPrevious As String = " Previous "
        Public Const ButtonFirst As String = "  First   "
        Public Const ButtonSend As String = "  Send   "
        Public Const ButtonSendTest As String = "Send Test"
        Public Const ButtonCreateDuplicate As String = " Create Duplicate "
        Public Const ButtonActivate As String = "  Activate   "
        Public Const ButtonDeactivate As String = "  Deactivate   "
        Public Const ButtonOpenActiveEditor As String = "Active Edit"
        'Public Const ButtonPublish As String = " Publish Changes "
        'Public Const ButtonAbortEdit As String = " Abort Edits "
        'Public Const ButtonPublishSubmit As String = " Submit for Publishing "
        'Public Const ButtonPublishApprove As String = " Approve for Publishing "
        'Public Const ButtonPublishDeny As String = " Deny for Publishing "
        'Public Const ButtonWorkflowPublishApproved As String = " Publish Approved Records "
        'Public Const ButtonWorkflowPublishSelected As String = " Publish Selected Records "
        Public Const ButtonSetHTMLEdit As String = " Edit WYSIWYG "
        Public Const ButtonSetTextEdit As String = " Edit HTML "
        Public Const ButtonRefresh As String = " Refresh "
        Public Const ButtonOrder As String = " Order "
        Public Const ButtonSearch As String = " Search "
        Public Const ButtonSpellCheck As String = " Spell Check "
        Public Const ButtonLibraryUpload As String = " Upload "
        Public Const ButtonCreateReport As String = " Create Report "
        Public Const ButtonClearTrapLog As String = " Clear Trap Log "
        Public Const ButtonNewSearch As String = " New Search "
        Public Const ButtonSaveandInvalidateCache As String = " Save and Invalidate Cache "
        Public Const ButtonImportTemplates As String = " Import Templates "
        Public Const ButtonRSSRefresh As String = " Update RSS Feeds Now "
        Public Const ButtonRequestDownload As String = " Request Download "
        Public Const ButtonFinish As String = " Finish "
        Public Const ButtonRegister As String = " Register "
        Public Const ButtonBegin As String = "Begin"
        Public Const ButtonAbort As String = "Abort"
        Public Const ButtonCreateGUID As String = " Create GUID "
        Public Const ButtonEnable As String = " Enable "
        Public Const ButtonDisable As String = " Disable "
        Public Const ButtonMarkReviewed As String = " Mark Reviewed "
        '
        '------------------------------------------------------------------------
        '   member actions
        '------------------------------------------------------------------------
        '
        Public Const MemberActionNOP = 0
        Public Const MemberActionLogin = 1
        Public Const MemberActionLogout = 2
        Public Const MemberActionForceLogin = 3
        Public Const MemberActionSendPassword = 4
        Public Const MemberActionForceLogout = 5
        Public Const MemberActionToolsApply = 6
        Public Const MemberActionJoin = 7
        Public Const MemberActionSaveProfile = 8
        Public Const MemberActionEditProfile = 9
        '
        '-----------------------------------------------------------------------
        ' ----- note pad info
        '-----------------------------------------------------------------------
        '
        Public Const NoteFormList = 1
        Public Const NoteFormRead = 2
        '
        Public Const NoteButtonPrevious As String = " Previous "
        Public Const NoteButtonNext As String = "   Next   "
        Public Const NoteButtonDelete As String = "  Delete  "
        Public Const NoteButtonClose As String = "  Close   "
        '                       ' Submit button is created in CommonDim, so it is simple
        Public Const NoteButtonSubmit As String = "Submit"
        '
        '-----------------------------------------------------------------------
        ' ----- Admin site storage
        '-----------------------------------------------------------------------
        '
        Public Const AdminMenuModeHidden = 0       '   menu is hidden
        Public Const AdminMenuModeLeft = 1     '   menu on the left
        Public Const AdminMenuModeTop = 2      '   menu as dropdowns from the top
        '
        ' ----- AdminActions - describes the form processing to do
        '
        Public Const AdminActionNop = 0            ' do nothing
        Public Const AdminActionDelete = 4         ' delete record
        Public Const AdminActionFind = 5           '
        Public Const AdminActionDeleteFilex = 6        '
        Public Const AdminActionUpload = 7         '
        Public Const AdminActionSaveNormal = 3         ' save fields to database
        Public Const AdminActionSaveEmail = 8          ' save email record (and update EmailGroups) to database
        Public Const AdminActionSaveMember = 11        '
        Public Const AdminActionSaveSystem = 12
        Public Const AdminActionSavePaths = 13     ' Save a record that is in the BathBlocking Format
        Public Const AdminActionSendEmail = 9          '
        Public Const AdminActionSendEmailTest = 10     '
        Public Const AdminActionNext = 14               '
        Public Const AdminActionPrevious = 15           '
        Public Const AdminActionFirst = 16              '
        Public Const AdminActionSaveContent = 17        '
        Public Const AdminActionSaveField = 18          ' Save a single field, fieldname = fn input
        Public Const AdminActionPublish = 19            ' Publish record live
        Public Const AdminActionAbortEdit = 20          ' Publish record live
        Public Const AdminActionPublishSubmit = 21      ' Submit for Workflow Publishing
        Public Const AdminActionPublishApprove = 22     ' Approve for Workflow Publishing
        'Public Const AdminActionWorkflowPublishApproved = 23    ' Publish what was approved
        Public Const AdminActionSetHTMLEdit = 24        ' Set Member Property for this field to HTML Edit
        Public Const AdminActionSetTextEdit = 25        ' Set Member Property for this field to Text Edit
        Public Const AdminActionSave = 26               ' Save Record
        Public Const AdminActionActivateEmail = 27      ' Activate a Conditional Email
        Public Const AdminActionDeactivateEmail = 28    ' Deactivate a conditional email
        Public Const AdminActionDuplicate = 29          ' Duplicate the (sent email) record
        Public Const AdminActionDeleteRows = 30         ' Delete from rows of records, row0 is boolean, rowid0 is ID, rowcnt is count
        Public Const AdminActionSaveAddNew = 31         ' Save Record and add a new record
        Public Const AdminActionReloadCDef = 32         ' Load Content Definitions
        ' Public Const AdminActionWorkflowPublishSelected = 33 ' Publish what was selected
        Public Const AdminActionMarkReviewed = 34       ' Mark the record reviewed without making any changes
        Public Const AdminActionEditRefresh = 35        ' reload the page just like a save, but do not save
        '
        ' ----- Adminforms (0-99)
        '
        Public Const AdminFormRoot = 0             ' intro page
        Public Const AdminFormIndex = 1            ' record list page
        Public Const AdminFormHelp = 2             ' popup help window
        Public Const AdminFormUpload = 3           ' encoded file upload form
        Public Const AdminFormEdit = 4             ' Edit form for system format records
        Public Const AdminFormEditSystem = 5       ' Edit form for system format records
        Public Const AdminFormEditNormal = 6       ' record edit page
        Public Const AdminFormEditEmail = 7        ' Edit form for Email format records
        Public Const AdminFormEditMember = 8       ' Edit form for Member format records
        Public Const AdminFormEditPaths = 9        ' Edit form for Paths format records
        Public Const AdminFormClose = 10           ' Special Case - do a window close instead of displaying a form
        Public Const AdminFormReports = 12         ' Call Reports form (admin only)
        'Public Const AdminFormSpider = 13          ' Call Spider
        Public Const AdminFormEditContent = 14     ' Edit form for Content records
        Public Const AdminFormDHTMLEdit = 15       ' ActiveX DHTMLEdit form
        Public Const AdminFormEditPageContent = 16 '
        Public Const AdminFormPublishing = 17       ' Workflow Authoring Publish Control form
        Public Const AdminFormQuickStats = 18       ' Quick Stats (from Admin root)
        Public Const AdminFormResourceLibrary = 19  ' Resource Library without Selects
        Public Const AdminFormEDGControl = 20       ' Control Form for the EDG publishing controls
        Public Const AdminFormSpiderControl = 21    ' Control Form for the Content Spider
        Public Const AdminFormContentChildTool = 22 ' Admin Create Content Child tool
        Public Const AdminformPageContentMap = 23   ' Map all content to a single map
        Public Const AdminformHousekeepingControl = 24 ' Housekeeping control
        Public Const AdminFormCommerceControl = 25
        Public Const AdminFormContactManager = 26
        Public Const AdminFormStyleEditor = 27
        Public Const AdminFormEmailControl = 28
        Public Const AdminFormCommerceInterface = 29
        Public Const AdminFormDownloads = 30
        Public Const AdminformRSSControl = 31
        Public Const AdminFormMeetingSmart = 32
        Public Const AdminFormMemberSmart = 33
        Public Const AdminFormEmailWizard = 34
        Public Const AdminFormImportWizard = 35
        Public Const AdminFormCustomReports = 36
        Public Const AdminFormFormWizard = 37
        Public Const AdminFormLegacyAddonManager = 38
        Public Const AdminFormIndex_SubFormAdvancedSearch = 39
        Public Const AdminFormIndex_SubFormSetColumns = 40
        Public Const AdminFormPageControl = 41
        Public Const AdminFormSecurityControl = 42
        Public Const AdminFormEditorConfig = 43
        Public Const AdminFormBuilderCollection = 44
        Public Const AdminFormClearCache = 45
        Public Const AdminFormMobileBrowserControl = 46
        Public Const AdminFormMetaKeywordTool = 47
        Public Const AdminFormIndex_SubFormExport = 48
        '
        ' ----- AdminFormTools (11,100-199)
        '
        Public Const AdminFormTools = 11           ' Call Tools form (developer only)
        Public Const AdminFormToolRoot = 11         ' These should match for compatibility
        Public Const AdminFormToolCreateContentDefinition = 101
        Public Const AdminFormToolContentTest = 102
        Public Const AdminFormToolConfigureMenu = 103
        Public Const AdminFormToolConfigureListing = 104
        Public Const AdminFormToolConfigureEdit = 105
        Public Const AdminFormToolManualQuery = 106
        Public Const AdminFormToolWriteUpdateMacro = 107
        Public Const AdminFormToolDuplicateContent = 108
        Public Const AdminFormToolDuplicateDataSource = 109
        Public Const AdminFormToolDefineContentFieldsFromTable = 110
        Public Const AdminFormToolContentDiagnostic = 111
        Public Const AdminFormToolCreateChildContent = 112
        Public Const AdminFormToolClearContentWatchLink = 113
        Public Const AdminFormToolSyncTables = 114
        Public Const AdminFormToolBenchmark = 115
        Public Const AdminFormToolSchema = 116
        Public Const AdminFormToolContentFileView = 117
        Public Const AdminFormToolDbIndex = 118
        Public Const AdminFormToolContentDbSchema = 119
        Public Const AdminFormToolLogFileView = 120
        Public Const AdminFormToolLoadCDef = 121
        Public Const AdminFormToolLoadTemplates = 122
        Public Const AdminformToolFindAndReplace = 123
        Public Const AdminformToolCreateGUID = 124
        Public Const AdminformToolIISReset = 125
        Public Const AdminFormToolRestart = 126
        Public Const AdminFormToolWebsiteFileView = 127
        '
        ' ----- Define the index column structure
        '       IndexColumnVariant( 0, n ) is the first column on the left
        '       IndexColumnVariant( 0, IndexColumnField ) = the index into the fields array
        '
        Public Const IndexColumnField = 0          ' The field displayed in the column
        Public Const IndexColumnWIDTH = 1          ' The width of the column
        Public Const IndexColumnSORTPRIORITY = 2       ' lowest columns sorts first
        Public Const IndexColumnSORTDIRECTION = 3      ' direction of the sort on this column
        Public Const IndexColumnSATTRIBUTEMAX = 3      ' the number of attributes here
        Public Const IndexColumnsMax = 50
        '
        ' ----- ReportID Constants, moved to ccCommonModule
        '
        Public Const ReportFormRoot = 1
        Public Const ReportFormDailyVisits = 2
        Public Const ReportFormWeeklyVisits = 12
        Public Const ReportFormSitePath = 4
        Public Const ReportFormSearchKeywords = 5
        Public Const ReportFormReferers = 6
        Public Const ReportFormBrowserList = 8
        Public Const ReportFormAddressList = 9
        Public Const ReportFormContentProperties = 14
        Public Const ReportFormSurveyList = 15
        Public Const ReportFormOrdersList = 13
        Public Const ReportFormOrderDetails = 21
        Public Const ReportFormVisitorList = 11
        Public Const ReportFormMemberDetails = 16
        Public Const ReportFormPageList = 10
        Public Const ReportFormVisitList = 3
        Public Const ReportFormVisitDetails = 17
        Public Const ReportFormVisitorDetails = 20
        Public Const ReportFormSpiderDocList = 22
        Public Const ReportFormSpiderErrorList = 23
        Public Const ReportFormEDGDocErrors = 24
        Public Const ReportFormDownloadLog = 25
        Public Const ReportFormSpiderDocDetails = 26
        Public Const ReportFormSurveyDetails = 27
        Public Const ReportFormEmailDropList = 28
        Public Const ReportFormPageTraffic = 29
        Public Const ReportFormPagePerformance = 30
        Public Const ReportFormEmailDropDetails = 31
        Public Const ReportFormEmailOpenDetails = 32
        Public Const ReportFormEmailClickDetails = 33
        Public Const ReportFormGroupList = 34
        Public Const ReportFormGroupMemberList = 35
        Public Const ReportFormTrapList = 36
        Public Const ReportFormCount = 36
        '
        '=============================================================================
        ' Page Scope Meetings Related Storage
        '=============================================================================
        '
        Public Const MeetingFormIndex = 0
        Public Const MeetingFormAttendees = 1
        Public Const MeetingFormLinks = 2
        Public Const MeetingFormFacility = 3
        Public Const MeetingFormHotel = 4
        Public Const MeetingFormDetails = 5
        '
        '------------------------------------------------------------------------------
        ' Form actions
        '------------------------------------------------------------------------------
        '
        ' ----- DataSource Types
        '
        Public Const DataSourceTypeODBCSQL99 = 0
        Public Const DataSourceTypeODBCAccess = 1
        Public Const DataSourceTypeODBCSQLServer = 2
        Public Const DataSourceTypeODBCMySQL = 3
        Public Const DataSourceTypeXMLFile = 4      ' Use MSXML Interface to open a file

        '
        ' Document (HTML, graphic, etc) retrieved from site
        '
        Public Const ResponseHeaderCountMax = 20
        Public Const ResponseCookieCountMax = 20
        '
        ' ----- text delimiter that divides the text and html parts of an email message stored in the queue folder
        '
        Public Const EmailTextHTMLDelimiter = vbCrLf & " ----- End Text Begin HTML -----" & vbCrLf
        '
        '------------------------------------------------------------------------
        '   Common RequestName Variables
        '------------------------------------------------------------------------
        '
        Public Const RequestNameDynamicFormID As String = "dformid"
        '
        Public Const RequestNameRunAddon As String = "addonid"
        Public Const RequestNameEditReferer As String = "EditReferer"
        'Public Const RequestNameRefreshBlock As String = "ccFormRefreshBlockSN"
        Public Const RequestNameCatalogOrder As String = "CatalogOrderID"
        Public Const RequestNameCatalogCategoryID As String = "CatalogCatID"
        Public Const RequestNameCatalogForm As String = "CatalogFormID"
        Public Const RequestNameCatalogItemID As String = "CatalogItemID"
        Public Const RequestNameCatalogItemAge As String = "CatalogItemAge"
        Public Const RequestNameCatalogRecordTop As String = "CatalogTop"
        Public Const RequestNameCatalogFeatured As String = "CatalogFeatured"
        Public Const RequestNameCatalogSpan As String = "CatalogSpan"
        Public Const RequestNameCatalogKeywords As String = "CatalogKeywords"
        Public Const RequestNameCatalogSource As String = "CatalogSource"
        '
        Public Const RequestNameLibraryFileID As String = "fileEID"
        Public Const RequestNameDownloadID As String = "downloadid"
        Public Const RequestNameLibraryUpload As String = "LibraryUpload"
        Public Const RequestNameLibraryName As String = "LibraryName"
        Public Const RequestNameLibraryDescription As String = "LibraryDescription"

        Public Const RequestNameRootPage As String = "RootPageName"
        Public Const RequestNameRootPageID As String = "RootPageID"
        Public Const RequestNameContent As String = "ContentName"
        Public Const RequestNameOrderByClause As String = "OrderByClause"
        Public Const RequestNameAllowChildPageList As String = "AllowChildPageList"
        '
        Public Const RequestNameCRKey As String = "crkey"
        Public Const RequestNameAdminForm As String = "af"
        Public Const RequestNameAdminSubForm As String = "subform"
        Public Const RequestNameButton As String = "button"
        Public Const RequestNameAdminSourceForm As String = "asf"
        Public Const RequestNameAdminFormSpelling As String = "SpellingRequest"
        Public Const RequestNameInlineStyles As String = "InlineStyles"
        Public Const RequestNameAllowCSSReset As String = "AllowCSSReset"
        '
        Public Const RequestNameReportForm As String = "rid"
        '
        Public Const RequestNameToolContentID As String = "ContentID"
        '
        Public Const RequestNameCut As String = "a904o2pa0cut"
        Public Const RequestNamePaste As String = "dp29a7dsa6paste"
        Public Const RequestNamePasteParentContentID As String = "dp29a7dsa6cid"
        Public Const RequestNamePasteParentRecordID As String = "dp29a7dsa6rid"
        Public Const RequestNamePasteFieldList As String = "dp29a7dsa6key"
        Public Const RequestNameCutClear As String = "dp29a7dsa6clear"
        '
        Public Const RequestNameRequestBinary As String = "RequestBinary"
        ' removed -- this was an old method of blocking form input for file uploads
        'Public Const RequestNameFormBlock As String = "RB"
        Public Const RequestNameJSForm As String = "RequestJSForm"
        Public Const RequestNameJSProcess As String = "ProcessJSForm"
        '
        Public Const RequestNameFolderID As String = "FolderID"
        '
        Public Const rnEmailMemberID As String = "emi8s9Kj"
        Public Const rnEmailOpenFlag As String = "eof9as88"
        Public Const RequestNameEmailOpenCssFlag As String = "8aa41pM3"
        Public Const rnEmailClickFlag As String = "ecf34Msi"
        Public Const rnEmailBlockRecipientEmail As String = "9dq8Nh61"
        Public Const rnEmailBlockRequestDropID As String = "BlockEmailRequest"
        Public Const RequestNameVisitTracking As String = "s9lD1088"
        Public Const RequestNameBlockContentTracking As String = "BlockContentTracking"
        Public Const RequestNameCookieDetectVisitID As String = "f92vo2a8d"

        Public Const RequestNamePageNumber As String = "PageNumber"
        Public Const RequestNamePageSize As String = "PageSize"
        '
        Public Const RequestValueNull As String = "[NULL]"
        '
        Public Const SpellCheckUserDictionaryFilename As String = "SpellCheck\UserDictionary.txt"
        '
        Public Const RequestNameStateString As String = "vstate"
        '
        ' ----- Actions
        '
        Public Const ToolsActionMenuMove = 1
        Public Const ToolsActionAddField = 2            ' Add a field to the Index page
        Public Const ToolsActionRemoveField = 3
        Public Const ToolsActionMoveFieldRight = 4
        Public Const ToolsActionMoveFieldLeft = 5
        Public Const ToolsActionSetAZ = 6
        Public Const ToolsActionSetZA = 7
        Public Const ToolsActionExpand = 8
        Public Const ToolsActionContract = 9
        Public Const ToolsActionEditMove = 10
        Public Const ToolsActionRunQuery = 11
        Public Const ToolsActionDuplicateDataSource = 12
        Public Const ToolsActionDefineContentFieldFromTableFieldsFromTable = 13
        Public Const ToolsActionFindAndReplace = 14
        Public Const ToolsActionIISReset = 15
        '
        '=======================================================================
        '   sitepropertyNames
        '=======================================================================
        '
        Public Const siteproperty_serverPageDefault_name As String = "serverPageDefault"
        Public Const siteproperty_serverPageDefault_defaultValue As String = "default.aspx"
        Public Const spAllowPageWithoutSectionDisplay As String = "Allow Page Without Section Display"
        Public Const spAllowPageWithoutSectionDisplay_default As Boolean = True
        Public Const spDefaultRouteAddonId As String = "Default Route AddonId"
        '
        '=======================================================================
        '   content replacements
        '=======================================================================
        '
        Public Const contentReplaceEscapeStart As String = "{%"
        Public Const contentReplaceEscapeEnd As String = "%}"
        '
        Public Class fieldEditorType
            Public fieldId As Integer
            Public addonid As Integer
        End Class
        '
        ' ----- Dataset for graphing
        '
        Public Class ColumnDataType
            Dim Name As String
            Dim row() As Integer
        End Class
        '
        Public Class ChartDataType
            Dim Title As String
            Dim XLabel As String
            Dim YLabel As String
            Dim RowCount As Integer
            Dim RowLabel() As String
            Dim ColumnCount As Integer
            Dim Column() As ColumnDataType
        End Class
        ''
        ' PrivateStorage to hold the DebugTimer
        '
        Class TimerStackType
            Dim Label As String
            Dim StartTicks As Integer
        End Class
        Private Const TimerStackMax = 20
        Private TimerStack(TimerStackMax) As TimerStackType
        Private TimerStackCount As Integer
        '
        Public Const TextSearchStartTagDefault As String = "<!--TextSearchStart-->"
        Public Const TextSearchEndTagDefault As String = "<!--TextSearchEnd-->"
        '
        '-------------------------------------------------------------------------------------
        '   IPDaemon communication objects
        '-------------------------------------------------------------------------------------
        '
        Class IPDaemonConnectionType
            Dim ConnectionID As Integer
            Dim BytesToSend As Integer
            Dim HTTPVersion As String
            Dim HTTPMethod As String
            Dim Path As String
            Dim Query As String
            Dim Headers As String
            Dim PostData As String
            Dim SendData As Boolean
            Dim State As Integer
            Dim ContentLength As Integer
        End Class
        '
        '-------------------------------------------------------------------------------------
        '   Email
        '-------------------------------------------------------------------------------------
        '
        Public Const EmailLogTypeDrop = 1                   ' Email was dropped
        Public Const EmailLogTypeOpen = 2                   ' System detected the email was opened
        Public Const EmailLogTypeClick = 3                  ' System detected a click from a link on the email
        Public Const EmailLogTypeBounce = 4                 ' Email was processed by bounce processing
        Public Const EmailLogTypeBlockRequest = 5           ' recipient asked us to stop sending email
        Public Const EmailLogTypeImmediateSend = 6        ' Email was dropped
        '
        Public Const DefaultSpamFooter As String = "<p>To block future emails from this site, <link>click here</link></p>"
        '
        Public Const FeedbackFormNotSupportedComment As String = "<!--" & vbCrLf & "Feedback form is not supported in this context" & vbCrLf & "-->"
        '
        '-------------------------------------------------------------------------------------
        '   Page Content constants
        '-------------------------------------------------------------------------------------
        '
        Public Const ContentBlockCopyName As String = "Content Block Copy"
        '
        Public Const BubbleCopy_AdminAddPage As String = "Use the Add page to create new content records. The save button puts you in edit mode. The OK button creates the record and exits."
        Public Const BubbleCopy_AdminIndexPage As String = "Use the Admin Listing page to locate content records through the Admin Site."
        Public Const BubbleCopy_SpellCheckPage As String = "Use the Spell Check page to verify and correct spelling throught the content."
        Public Const BubbleCopy_AdminEditPage As String = "Use the Edit page to add and modify content."
        '
        '
        Public Const TemplateDefaultName As String = "Default"
        'Public Const TemplateDefaultBody As String = "<!--" & vbCrLf & "Default Template - edit this Page Template, or select a different template for your page or section" & vbCrLf & "-->{{DYNAMICMENU?MENU=}}<br>{{CONTENT}}"
        Public Const TemplateDefaultBody As String = "" _
            & vbCrLf & vbTab & "<!--" _
            & vbCrLf & vbTab & "Default Template - edit this Page Template, or select a different template for your page or section" _
            & vbCrLf & vbTab & "-->" _
            & vbCrLf & vbTab & "{% {""addon"":{""addon"":""menu"",""menu"":""Default""}} %}" _
            & vbCrLf & vbTab & "{% ""content box"" %}"
        Public Const TemplateDefaultBodyTag As String = "<body class=""ccBodyWeb"">"
        '
        '=======================================================================
        '   Internal Tab interface storage
        '=======================================================================
        '
        Private Class TabType
            Dim Caption As String
            Dim Link As String
            Dim StylePrefix As String
            Dim IsHit As Boolean
            Dim LiveBody As String
        End Class
        '
        Private Tabs() As TabType
        Private TabsCnt As Integer
        Private TabsSize As Integer
        '
        ' Admin Navigator Nodes
        '
        Public Const NavigatorNodeCollectionList = -1
        Public Const NavigatorNodeAddonList = -1
        ''
        '' Pointers into index of PCC (Page Content Cache) array
        ''
        'Public Const PCC_ID = 0
        'Public Const PCC_Active = 1
        'Public Const PCC_ParentID = 2
        'Public Const PCC_Name = 3
        'Public Const PCC_Headline = 4
        'Public Const PCC_MenuHeadline = 5
        'Public Const PCC_DateArchive = 6
        'Public Const PCC_DateExpires = 7
        'Public Const PCC_PubDate = 8
        'Public Const PCC_ChildListSortMethodID = 9
        'Public Const PCC_ContentControlID = 10
        'Public Const PCC_TemplateID = 11
        'Public Const PCC_BlockContent = 12
        'Public Const PCC_BlockPage = 13
        'Public Const PCC_Link = 14
        'Public Const PCC_RegistrationGroupID = 15
        'Public Const PCC_BlockSourceID = 16
        'Public Const PCC_CustomBlockMessageFilename = 17
        'Public Const PCC_JSOnLoad = 18
        'Public Const PCC_JSHead = 19
        'Public Const PCC_JSEndBody = 20
        'Public Const PCC_Viewings = 21
        'Public Const PCC_ContactMemberID = 22
        'Public Const PCC_AllowHitNotification = 23
        'Public Const PCC_TriggerSendSystemEmailID = 24
        'Public Const PCC_TriggerConditionID = 25
        'Public Const PCC_TriggerConditionGroupID = 26
        'Public Const PCC_TriggerAddGroupID = 27
        'Public Const PCC_TriggerRemoveGroupID = 28
        'Public Const PCC_AllowMetaContentNoFollow = 29
        'Public Const PCC_ParentListName = 30
        'Public Const PCC_CopyFilename = 31
        'Public Const PCC_BriefFilename = 32
        'Public Const PCC_AllowChildListDisplay = 33
        'Public Const PCC_SortOrder = 34
        'Public Const PCC_DateAdded = 35
        'Public Const PCC_ModifiedDate = 36
        'Public Const PCC_ChildPagesFound = 37
        'Public Const PCC_AllowInMenus = 38
        'Public Const PCC_AllowInChildLists = 39
        'Public Const PCC_JSFilename = 40
        'Public Const PCC_ChildListInstanceOptions = 41
        'Public Const PCC_IsSecure = 42
        'Public Const PCC_AllowBrief = 43
        'Public Const PCC_ColCnt = 44
        '
        ' Indexes into the SiteSectionCache
        ' Created from "ID, Name,TemplateID,ContentID,MenuImageFilename,Caption,MenuImageOverFilename,HideMenu,BlockSection,RootPageID,JSOnLoad,JSHead,JSEndBody"
        '
        'Public Const SSC_ID = 0
        'Public Const SSC_Name = 1
        'Public Const SSC_TemplateID = 2
        'Public Const SSC_ContentID = 3
        'Public Const SSC_MenuImageFilename = 4
        'Public Const SSC_Caption = 5
        'Public Const SSC_MenuImageOverFilename = 6
        'Public Const SSC_HideMenu = 7
        'Public Const SSC_BlockSection = 8
        'Public Const SSC_RootPageID = 9
        'Public Const SSC_JSOnLoad = 10
        'Public Const SSC_JSHead = 11
        'Public Const SSC_JSEndBody = 12
        'Public Const SSC_JSFilename = 13
        'Public Const SSC_cnt = 14
        '
        ' Indexes into the TemplateCache
        ' Created from "t.ID,t.Name,t.Link,t.BodyHTML,t.JSOnLoad,t.JSHead,t.JSEndBody,t.StylesFilename,r.StyleID"
        '
        'Public Const TC_ID = 0
        'Public Const TC_Name = 1
        'Public Const TC_Link = 2
        'Public Const TC_BodyHTML = 3
        'Public Const TC_JSOnLoad = 4
        'Public Const TC_JSInHeadLegacy = 5
        ''Public Const TC_JSHead = 5
        'Public Const TC_JSEndBody = 6
        'Public Const TC_StylesFilename = 7
        'Public Const TC_SharedStylesIDList = 8
        'Public Const TC_MobileBodyHTML = 9
        'Public Const TC_MobileStylesFilename = 10
        'Public Const TC_OtherHeadTags = 11
        'Public Const TC_BodyTag = 12
        'Public Const TC_JSInHeadFilename = 13
        ''Public Const TC_JSFilename = 13
        'Public Const TC_IsSecure = 14
        'Public Const TC_DomainIdList = 15
        '' for now, Mobile templates do not have shared styles
        ''Public Const TC_MobileSharedStylesIDList = 11
        'Public Const TC_cnt = 16
        '
        ' DTD
        '
        Public Const DTDDefault As String = "<!DOCTYPE html>"
        '
        ' innova Editor feature list
        '
        Public Const InnovaEditorFeaturefilename As String = "innova\EditorConfig.txt"
        Public Const InnovaEditorFeatureList As String = "FullScreen,Preview,Print,Search,Cut,Copy,Paste,PasteWord,PasteText,SpellCheck,Undo,Redo,Image,Flash,Media,CustomObject,CustomTag,Bookmark,Hyperlink,HTMLSource,XHTMLSource,Numbering,Bullets,Indent,Outdent,JustifyLeft,JustifyCenter,JustifyRight,JustifyFull,Table,Guidelines,Absolute,Characters,Line,Form,RemoveFormat,ClearAll,StyleAndFormatting,TextFormatting,ListFormatting,BoxFormatting,ParagraphFormatting,CssText,Styles,Paragraph,FontName,FontSize,Bold,Italic,Underline,Strikethrough,Superscript,Subscript,ForeColor,BackColor"
        Public Const InnovaEditorPublicFeatureList As String = "FullScreen,Preview,Print,Search,Cut,Copy,Paste,PasteWord,PasteText,SpellCheck,Undo,Redo,Bookmark,Hyperlink,HTMLSource,XHTMLSource,Numbering,Bullets,Indent,Outdent,JustifyLeft,JustifyCenter,JustifyRight,JustifyFull,Table,Guidelines,Absolute,Characters,Line,Form,RemoveFormat,ClearAll,StyleAndFormatting,TextFormatting,ListFormatting,BoxFormatting,ParagraphFormatting,CssText,Styles,Paragraph,FontName,FontSize,Bold,Italic,Underline,Strikethrough,Superscript,Subscript,ForeColor,BackColor"
        ''
        '' Content Type
        ''
        'Enum contentTypeEnum
        '    contentTypeWeb = 1
        '    ContentTypeEmail = 2
        '    contentTypeWebTemplate = 3
        '    contentTypeEmailTemplate = 4
        'End Enum
        'Public EditorContext As contentTypeEnum
        'Enum EditorContextEnum
        '    contentTypeWeb = 1
        '    contentTypeEmail = 2
        'End Enum
        'Public EditorContext As EditorContextEnum
        ''
        'Public Const EditorAddonMenuEmailTemplateFilename As String = "templates/EditorAddonMenuTemplateEmail.js"
        'Public Const EditorAddonMenuEmailContentFilename As String = "templates/EditorAddonMenuContentEmail.js"
        'Public Const EditorAddonMenuWebTemplateFilename As String = "templates/EditorAddonMenuTemplateWeb.js"
        'Public Const EditorAddonMenuWebContentFilename As String = "templates/EditorAddonMenuContentWeb.js"
        '
        Public Const DynamicStylesFilename As String = "templates/styles.css"
        Public Const AdminSiteStylesFilename As String = "templates/AdminSiteStyles.css"
        Public Const EditorStyleRulesFilenamePattern As String = "templates/EditorStyleRules$TemplateID$.js"
        ' deprecated 11/24/3009 - StyleRules destinction between web/email not needed b/c body background blocked
        'Public Const EditorStyleWebRulesFilename As String = "templates/EditorStyleWebRules.js"
        'Public Const EditorStyleEmailRulesFilename As String = "templates/EditorStyleEmailRules.js"
        '
        ' ----- ccGroupRules storage for list of Content that a group can author
        '
        Public Class ContentGroupRuleType
            Dim ContentID As Integer
            Dim GroupID As Integer
            Dim AllowAdd As Boolean
            Dim AllowDelete As Boolean
        End Class
        '
        ' ----- This should match the Lookup List in the NavIconType field in the Navigator Entry content definition
        '
        Public Const navTypeIDList As String = "Add-on,Report,Setting,Tool"
        Public Const NavTypeIDAddon = 1
        Public Const NavTypeIDReport = 2
        Public Const NavTypeIDSetting = 3
        Public Const NavTypeIDTool = 4
        '
        Public Const NavIconTypeList As String = "Custom,Advanced,Content,Folder,Email,User,Report,Setting,Tool,Record,Addon,help"
        Public Const NavIconTypeCustom = 1
        Public Const NavIconTypeAdvanced = 2
        Public Const NavIconTypeContent = 3
        Public Const NavIconTypeFolder = 4
        Public Const NavIconTypeEmail = 5
        Public Const NavIconTypeUser = 6
        Public Const NavIconTypeReport = 7
        Public Const NavIconTypeSetting = 8
        Public Const NavIconTypeTool = 9
        Public Const NavIconTypeRecord = 10
        Public Const NavIconTypeAddon = 11
        Public Const NavIconTypeHelp = 12
        '
        Public Const QueryTypeSQL = 1
        Public Const QueryTypeOpenContent = 2
        Public Const QueryTypeUpdateContent = 3
        Public Const QueryTypeInsertContent = 4
        '
        ' Google Data Object construction in GetRemoteQuery
        '
        Public Class ColsType
            Public Type As String
            Public Id As String
            Public Label As String
            Public Pattern As String
        End Class
        '
        Public Class CellType
            Public v As String
            Public f As String
            Public p As String
        End Class
        '
        Public Class RowsType
            Public Cell() As CellType
        End Class
        '
        Public Class GoogleDataType
            Public IsEmpty As Boolean
            Public col() As ColsType
            Public row() As RowsType
        End Class
        '
        Public Enum GoogleVisualizationStatusEnum
            OK = 1
            warning = 2
            ErrorStatus = 3
        End Enum
        '
        Public Class GoogleVisualizationType
            Public version As String
            Public reqid As String
            Public status As GoogleVisualizationStatusEnum
            Public warnings() As String
            Public errors() As String
            Public sig As String
            Public table As GoogleDataType
        End Class

        'Public Const ReturnFormatTypeGoogleTable = 1
        'Public Const ReturnFormatTypeNameValue = 2

        Public Enum RemoteFormatEnum
            RemoteFormatJsonTable = 1
            RemoteFormatJsonNameArray = 2
            RemoteFormatJsonNameValue = 3
        End Enum
        'Private Enum GoogleVisualizationStatusEnum
        '    OK = 1
        '    warning = 2
        '    ErrorStatus = 3
        'End Enum
        ''
        'Private Structure GoogleVisualizationType
        '    Dim version As String
        '    Dim reqid As String
        '    Dim status As GoogleVisualizationStatusEnum
        '    Dim warnings() As String
        '    Dim errors() As String
        '    Dim sig As String
        '    Dim table As GoogleDataType
        'End Structure        '
        '
        '
        Public Declare Function RegCloseKey& Lib "advapi32.dll" (ByVal hKey&)
        Public Declare Function RegOpenKeyExA& Lib "advapi32.dll" (ByVal hKey&, ByVal lpszSubKey$, ByVal dwOptions&, ByVal samDesired&, ByVal lpHKey&)
        Public Declare Function RegQueryValueExA& Lib "advapi32.dll" (ByVal hKey&, ByVal lpszValueName$, ByVal lpdwRes&, ByVal lpdwType&, ByVal lpDataBuff$, ByVal nSize&)
        Public Declare Function RegQueryValueEx& Lib "advapi32.dll" Alias "RegQueryValueExA" (ByVal hKey&, ByVal lpszValueName$, ByVal lpdwRes&, ByVal lpdwType&, ByVal lpDataBuff&, ByVal nSize&)

        Public Const HKEY_CLASSES_ROOT = &H80000000
        Public Const HKEY_CURRENT_USER = &H80000001
        Public Const HKEY_LOCAL_MACHINE = &H80000002
        Public Const HKEY_USERS = &H80000003

        Public Const ERROR_SUCCESS = 0&
        Public Const REG_SZ = 1&                          ' Unicode nul terminated string
        Public Const REG_DWORD = 4&                       ' 32-bit number

        Public Const KEY_QUERY_VALUE = &H1&
        Public Const KEY_SET_VALUE = &H2&
        Public Const KEY_CREATE_SUB_KEY = &H4&
        Public Const KEY_ENUMERATE_SUB_KEYS = &H8&
        Public Const KEY_NOTIFY = &H10&
        Public Const KEY_CREATE_LINK = &H20&
        Public Const READ_CONTROL = &H20000
        Public Const WRITE_DAC = &H40000
        Public Const WRITE_OWNER = &H80000
        Public Const SYNCHRONIZE = &H100000
        Public Const STANDARD_RIGHTS_REQUIRED = &HF0000
        Public Const STANDARD_RIGHTS_READ = READ_CONTROL
        Public Const STANDARD_RIGHTS_WRITE = READ_CONTROL
        Public Const STANDARD_RIGHTS_EXECUTE = READ_CONTROL
        Public Const KEY_READ = STANDARD_RIGHTS_READ Or KEY_QUERY_VALUE Or KEY_ENUMERATE_SUB_KEYS Or KEY_NOTIFY
        Public Const KEY_WRITE = STANDARD_RIGHTS_WRITE Or KEY_SET_VALUE Or KEY_CREATE_SUB_KEY
        Public Const KEY_EXECUTE = KEY_READ
        Public Const maxLongValue = 2147483647
        '
        ' Error Definitions
        '
        Public Const ERR_UNKNOWN As Integer = vbObjectError + 101
        Public Const ERR_FIELD_DOES_NOT_EXIST As Integer = vbObjectError + 102
        Public Const ERR_FILESIZE_NOT_ALLOWED As Integer = vbObjectError + 103
        Public Const ERR_FOLDER_DOES_NOT_EXIST As Integer = vbObjectError + 104
        Public Const ERR_FILE_ALREADY_EXISTS As Integer = vbObjectError + 105
        Public Const ERR_FILE_TYPE_NOT_ALLOWED As Integer = vbObjectError + 106

        '
        ' addonIncludeRules cache
        '
        Public Const cache_addonIncludeRules_cacheName As String = "cache_addonIncludeRules"
        Public Const cache_addonIncludeRules_fieldList As String = "addonId,includedAddonId"
        Public Const addonIncludeRulesCache_addonId = 0
        Public Const addonIncludeRulesCache_includedAddonId = 1
        Public Const addonIncludeRulesCacheColCnt = 2
        '
        ' addonIncludeRules cache
        '
        Public Const cache_LibraryFiles_cacheName As String = "cache_LibraryFiles"
        Public Const cache_LibraryFiles_fieldList As String = "id,ccguid,clicks,filename,width,height,AltText,altsizelist"
        Public Const LibraryFilesCache_Id = 0
        Public Const LibraryFilesCache_ccGuid = 1
        Public Const LibraryFilesCache_clicks = 2
        Public Const LibraryFilesCache_filename = 3
        Public Const LibraryFilesCache_width = 4
        Public Const LibraryFilesCache_height = 5
        Public Const LibraryFilesCache_alttext = 6
        Public Const LibraryFilesCache_altsizelist = 7
        Public Const LibraryFilesCacheColCnt = 8
        '
        ' link forward cache
        '
        Public Const cache_linkForward_cacheName As String = "cache_linkForward"
        '
        Public Const main_cookieNameVisit As String = "visit"
        Public Const main_cookieNameVisitor As String = "visitor"
        Public Const html_quickEdit_fpo As String = "<quickeditor>"
        '
        'Public Const sqlAddonStyles  As String  = "select addonid,styleid from ccSharedStylesAddonRules where (active<>0) order by id"
        '
        Public Const cacheNameAddonStyleRules As String = "addon styles"
        '
        Public Const ALLOWLEGACYAPI As Boolean = False
        Public Const ALLOWPROFILING As Boolean = False
        '
        Public Const cacheNameAssemblySkipList As String = "cacheNameAssemblySkipList"
        '
        ' put content definitions here
        '
        '
        Public Structure NameValuePairType
            Public Name As String
            Public Value As String
        End Structure
        '
        '========================================================================
        '   defined errors (event log eventId)
        '       1000-1999 Contensive
        '       2000-2999 Datatree
        '
        '   see kmaErrorDescription() for transations
        '========================================================================
        '
        Const Error_DataTree_RootNodeNext = 2000
        Const Error_DataTree_NoGoNext = 2001
        '
        '========================================================================
        '
        '========================================================================
        '
        'Declare Function GetTickCount Lib "kernel32" () As Integer
        Declare Function GetCurrentProcessId Lib "kernel32" () As Integer
        'Declare Sub Sleep Lib "kernel32" (ByVal dwMilliseconds As Integer)
        '
        '========================================================================
        '   Declarations for SetPiorityClass
        '========================================================================
        '
        'Const THREAD_BASE_PRIORITY_IDLE = -15
        'Const THREAD_BASE_PRIORITY_LOWRT = 15
        'Const THREAD_BASE_PRIORITY_MIN = -2
        'Const THREAD_BASE_PRIORITY_MAX = 2
        'Const THREAD_PRIORITY_LOWEST = THREAD_BASE_PRIORITY_MIN
        'Const THREAD_PRIORITY_HIGHEST = THREAD_BASE_PRIORITY_MAX
        'Const THREAD_PRIORITY_BELOW_NORMAL = (THREAD_PRIORITY_LOWEST + 1)
        'Const THREAD_PRIORITY_ABOVE_NORMAL = (THREAD_PRIORITY_HIGHEST - 1)
        'Const THREAD_PRIORITY_IDLE = THREAD_BASE_PRIORITY_IDLE
        'Const THREAD_PRIORITY_NORMAL = 0
        'Const THREAD_PRIORITY_TIME_CRITICAL = THREAD_BASE_PRIORITY_LOWRT
        'Const HIGH_PRIORITY_CLASS = &H80
        'Const IDLE_PRIORITY_CLASS = &H40
        'Const NORMAL_PRIORITY_CLASS = &H20
        'Const REALTIME_PRIORITY_CLASS = &H100
        '
        'Private Declare Function SetThreadPriority Lib "kernel32" (ByVal hThread As Integer, ByVal nPriority As Integer) As Integer
        'Private Declare Function SetPriorityClass Lib "kernel32" (ByVal hProcess As Integer, ByVal dwPriorityClass As Integer) As Integer
        'Private Declare Function GetThreadPriority Lib "kernel32" (ByVal hThread As Integer) As Integer
        'Private Declare Function GetPriorityClass Lib "kernel32" (ByVal hProcess As Integer) As Integer
        'Private Declare Function GetCurrentThread Lib "kernel32" () As Integer
        'Private Declare Function GetCurrentProcess Lib "kernel32" () As Integer
        '

        '
        '========================================================================
        'Converts unsafe characters,
        'such as spaces, into their
        'corresponding escape sequences.
        '========================================================================
        '
        'Declare Function UrlEscape Lib "shlwapi" _
        '   Alias "UrlEscapeA" _
        '  (ByVal pszURL As String, _
        '   ByVal pszEscaped As String, _
        '   ByVal pcchEscaped As Integer, _
        '   ByVal dwFlags As Integer) As Integer
        ''
        ''Converts escape sequences back into
        ''ordinary characters.
        ''
        'Declare Function UrlUnescape Lib "shlwapi" _
        '   Alias "UrlUnescapeA" _
        '  (ByVal pszURL As String, _
        '   ByVal pszUnescaped As String, _
        '   ByVal pcchUnescaped As Integer, _
        '   ByVal dwFlags As Integer) As Integer

        '
        '   Error reporting strategy
        '       Popups are pop-up boxes that tell the user what to do
        '       Logs are files with error details for developers to use debugging
        '
        '       Attended Programs
        '           - errors that do not effect the operation, resume next
        '           - all errors trickle up to the user interface level
        '           - User Errors, like file not found, return "UserError" code and a description
        '           - Internal Errors, like div-by-0, User should see no detail, log gets everything
        '           - Dependant Object Errors, codes that return from objects:
        '               - If UserError, translate ErrSource for raise, but log all original info
        '               - If InternalError, log info and raise InternalError
        '               - If you can not tell, call it InternalError
        '
        '       UnattendedMode
        '           The same, except each routine decides when
        '
        '       When an error happens in-line (bad condition without a raise)
        '           Log the error
        '           Raise the appropriate Code/Description in the current Source
        '
        '       When an ErrorTrap occurs
        '           If ErrSource is not AppTitle, it is a dependantObjectError, log and translate code
        '           If ErrNumber is not an ObjectError, call it internal error, log and translate code
        '           Error must be either "InternalError" or "UserError", just raise it again
        '
        ' old - If an error is raised that is not a KmaCode, it is logged and translated
        ' old - If an error is raised and the soure is not he current "dll", it is logged and translated
        '
        'Public Const ignoreInteger = vbObjectError                 ' Base on which Internal errors should start
        ''
        ''Public Const KmaError_UnderlyingObject = vbObjectError + 1     ' An error occurec in an underlying object
        ''Public Const KmaccErrorServiceStopped = vbObjectError + 2       ' The service is not running
        ''Public Const KmaError_BadObject = vbObjectError + 3            ' The Server Pointer is not valid
        ''Public Const KmaError_UpgradeInProgress = vbObjectError + 4    ' page is blocked because an upgrade is in progress
        ''Public Const KmaError_InvalidArgument = vbObjectError + 5      ' and input argument is not valid. Put details at end of description
        ''
        'Public Const ignoreInteger = ignoreInteger + 16                   ' Generic Error code that passes the description back to the user
        'Public Const ignoreInteger = ignoreInteger + 17               ' Internal error which the user should not see
        'Public Const KmaErrorPage = ignoreInteger + 18                   ' Error from the page which called Contensive
        ''
        'Public Const KmaObjectError = ignoreInteger + 256                ' Internal error which the user should not see
        '
        Public Const SQLTrue As String = "1"
        Public Const SQLFalse As String = "0"
        '
        Public Const dateMinValue As Date = #12/30/1899#
        '
        '
        Public Const kmaEndTable As String = "</table >"
        Public Const kmaEndTableCell As String = "</td>"
        Public Const kmaEndTableRow As String = "</tr>"
        '
        Public Enum csv_contentTypeEnum
            contentTypeWeb = 1
            contentTypeEmail = 2
            contentTypeWebTemplate = 3
            contentTypeEmailTemplate = 4
        End Enum
        ''
        '' ---------------------------------------------------------------------------------------------------
        '' ----- CDefAdminColumnType
        '' ---------------------------------------------------------------------------------------------------
        ''
        'Public Structure cdefServices.CDefAdminColumnType
        '    Public Name As String
        '    Public FieldPointer As Integer
        '    Public Width As Integer
        '    Public SortPriority As Integer
        '    Public SortDirection As Integer
        'End Structure
        ''
        '' ---------------------------------------------------------------------------------------------------
        '' ----- CDefFieldType
        ''       class not structure because it has to marshall to vb6
        '' ---------------------------------------------------------------------------------------------------
        ''
        'Public Structure cdefServices.CDefFieldType
        '    Public Name As String                      ' The name of the field
        '    Public ValueVariant As Object             ' The value carried to and from the database
        '    Public Id As Integer                          ' the ID in the ccContentFields Table that this came from
        '    Public active As Boolean                   ' if the field is available in the admin area
        '    Public fieldType As Integer                   ' The type of data the field holds
        '    Public Caption As String                   ' The caption for displaying the field
        '    Public ReadOnlyField As Boolean            ' was ReadOnly -- If true, this field can not be written back to the database
        '    Public NotEditable As Boolean              ' if true, you can only edit new records
        '    Public LookupContentID As Integer             ' If TYPELOOKUP, (for Db controled sites) this is the content ID of the source table
        '    Public Required As Boolean                 ' if true, this field must be entered
        '    Public DefaultValueObject As Object      ' default value on a new record
        '    Public HelpMessage As String               ' explaination of this field
        '    Public UniqueName As Boolean               '
        '    Public TextBuffered As Boolean             ' if true, the input is run through RemoveControlCharacters()
        '    Public Password As Boolean                 ' for text boxes, sets the password attribute
        '    Public RedirectContentID As Integer           ' If TYPEREDIRECT, this is new contentID
        '    Public RedirectID As String                ' If TYPEREDIRECT, this is the field that must match ID of this record
        '    Public RedirectPath As String              ' New Field, If TYPEREDIRECT, this is the path to the next page (if blank, current page is used)
        '    Public IndexColumn As Integer                 ' the column desired in the admin index form
        '    Public IndexWidth As String                ' either number or percentage
        '    Public IndexSortOrder As String            ' alpha sort on index page
        '    Public IndexSortDirection As Integer          ' 1 sorts forward, -1 backward
        '    Public Changed As Boolean                  ' if true, field value needs to be saved to database
        '    Public AdminOnly As Boolean                ' This field is only available to administrators
        '    Public DeveloperOnly As Boolean            ' This field is only available to administrators
        '    Public BlockAccess As Boolean              ' ***** Field Reused to keep binary compatiblity - "IsBaseField" - if true this is a CDefBase field
        '    '   false - custom field, is not altered during upgrade, Help message comes from the local record
        '    '   true - upgrade modifies the field definition, help message comes from support.contensive.com
        '    Public htmlContent As Boolean              ' if true, the HTML editor (active edit) can be used
        '    Public Authorable As Boolean               ' true if it can be seen in the admin form
        '    Public Inherited As Boolean                ' if true, this field takes its values from a parent, see ContentID
        '    Public ContentID As Integer                   ' This is the ID of the Content Def that defines these properties
        '    Public EditSortPriority As Integer            ' The Admin Edit Sort Order
        '    Public ManyToManyContentID As Integer         ' Content containing Secondary Records
        '    Public ManyToManyRuleContentID As Integer     ' Content with rules between Primary and Secondary
        '    Public ManyToManyRulePrimaryField As String     ' Rule Field Name for Primary Table
        '    Public ManyToManyRuleSecondaryField As String   ' Rule Field Name for Secondary Table
        '    '
        '    ' - new
        '    '
        '    Public RSSTitleField As Boolean             ' When creating RSS fields from this content, this is the title
        '    Public RSSDescriptionField As Boolean       ' When creating RSS fields from this content, this is the description
        '    Public EditTab As String                   ' Editing group - used for the tabs
        '    Public Scramble As Boolean                 ' save the field scrambled in the Db
        '    Public MemberSelectGroupID As Integer         ' If the Type is TypeMemberSelect, this is the group that the member will be selected from
        '    Public LookupList As String                ' If TYPELOOKUP, and LookupContentID is null, this is a comma separated list of choices
        'End Structure
        ''
        '' ---------------------------------------------------------------------------------------------------
        '' ----- CDefType
        ''       class not structure because it has to marshall to vb6
        '' ---------------------------------------------------------------------------------------------------
        ''
        'Public Structure cdefServices.CDefType
        '    Public Name As String                       ' Name of Content
        '    Public Id As Integer                           ' index in content table
        '    Public ContentTableName As String           ' the name of the content table
        '    Public ContentDataSourceName As String      '
        '    Public AuthoringTableName As String         ' the name of the authoring table
        '    Public AuthoringDataSourceName As String    '
        '    Public AllowAdd As Boolean                  ' Allow adding records
        '    Public AllowDelete As Boolean               ' Allow deleting records
        '    Public WhereClause As String                ' Used to filter records in the admin area
        '    Public ParentID As Integer                     ' if not IgnoreContentControl, the ID of the parent content
        '    Public ChildIDList As String                ' Comma separated list of child content definition IDs
        '    Public ChildPointerList As String           ' Comma separated list of child content definition Pointers
        '    Public DefaultSortMethod As String          ' FieldName Direction, ....
        '    Public ActiveOnly As Boolean                ' When true
        '    Public AdminOnly As Boolean                 ' Only allow administrators to modify content
        '    Public DeveloperOnly As Boolean             ' Only allow developers to modify content
        '    Public AllowWorkflowAuthoring As Boolean    ' if true, treat this content with authoring proceses
        '    Public DropDownFieldList As String          ' String used to populate select boxes
        '    Public SelectFieldList As String            ' Field list used in OpenCSContent calls (all active field definitions)
        '    Public EditorGroupName As String            ' Group of members who administer Workflow Authoring
        '    '
        '    ' array of cdefFieldType throws a vb6 error, data or method problem
        '    ' public property does not work (msn article -- very slow because it marshals he entire array, not a pointer )
        '    ' public function works to read, but cannot write
        '    ' possible -- public fields() function for reads
        '    '   -- public setFields() function for writes
        '    '
        '    Public fields as appServices_CdefClass.CDefFieldType()
        '    Public FieldPointer As Integer                 ' Current Field for FirstField / NextField calls
        '    Public FieldCount As Integer                   ' The number of fields loaded (from ccFields, or Set calls)
        '    Public FieldSize As Integer                    ' The number of initialize Field()
        '    '
        '    ' same as fields
        '    '
        '    Public adminColumns as appServices_CdefClass.CDefAdminColumnType()
        '    'Public AdminColumnLocal As CDefAdminColumnType()  ' The Admins in this content
        '    Public AdminColumnPointer As Integer                 ' Current Admin for FirstAdminColumn / NextAdminColumn calls
        '    Public AdminColumnCount As Integer                   ' The number of AdminColumns loaded (from ccAdminColumns, or Set calls)
        '    Public AdminColumnSize As Integer                    ' The number of initialize Admin()s
        '    'AdminColumn As CDefAdminCollClass
        '    '
        '    Public ContentControlCriteria As String     ' String created from ParentIDs used to select records
        '    '
        '    ' ----- future
        '    '
        '    Public IgnoreContentControl As Boolean     ' if true, all records in the source are in this content
        '    Public AliasName As String                 ' Field Name of the required "name" field
        '    Public AliasID As String                   ' Field Name of the required "id" field
        '    '
        '    ' ----- removed
        '    '
        '    Public SingleRecord As Boolean             ' removeme
        '    '    Type as integer                        ' removeme
        '    Public TableName As String                 ' removeme
        '    Public DataSourceID As Integer                ' removeme
        '    '
        '    ' ----- new
        '    '
        '    Public AllowTopicRules As Boolean          ' For admin edit page
        '    Public AllowContentTracking As Boolean     ' For admin edit page
        '    Public AllowCalendarEvents As Boolean      ' For admin edit page
        '    Public AllowMetaContent As Boolean         ' For admin edit page - Adds the Meta Content Section
        '    Public TimeStamp As String                 ' string that changes if any record in Content Definition changes, in memory only
        'End Structure


        'Private Type AdminColumnType
        '    FieldPointer as integer
        '    FieldWidth as integer
        '    FieldSortDirection as integer
        'End Type
        '
        'Private Type AdminType
        '    ColumnCount as integer
        '    Columns() As AdminColumnType
        'End Type
        '
        '-----------------------------------------------------------------------
        '   Messages
        '-----------------------------------------------------------------------
        '
        Public Const Msg_AuthoringDeleted As String = "<b>Record Deleted</b><br>" & SpanClassAdminSmall _
                                                & "This record was deleted and will be removed when publishing is complete.</SPAN>"
        Public Const Msg_AuthoringInserted As String = "<b>Record Added</b><br>" & SpanClassAdminSmall _
                                                & "This record was added and will display when publishing is complete.</span>"
        Public Const Msg_EditLock As String = "<b>Edit Locked</b><br>" & SpanClassAdminSmall _
                                                & "This record is currently being edited by <EDITNAME>.<br>" _
                                                & "This lock will be released when the user releases the record, or at <EDITEXPIRES> (about <EDITEXPIRESMINUTES> minutes).</span>"
        Public Const Msg_WorkflowDisabled As String = "<b>Immediate Authoring</b><br>" & SpanClassAdminSmall _
                                                & "Changes made will be reflected on the web site immediately.</span>"
        Public Const Msg_ContentWorkflowDisabled As String = "<b>Immediate Authoring Content Definition</b><br>" & SpanClassAdminSmall _
                                                & "Changes made will be reflected on the web site immediately.</span>"
        Public Const Msg_AuthoringRecordNotModifed As String = "" & SpanClassAdminSmall _
                                                & "No changes have been saved to this record.</span>"
        Public Const Msg_AuthoringRecordModifed As String = "<b>Edits Pending</b><br>" & SpanClassAdminSmall _
                                                & "This record has been edited by <EDITNAME>.<br>" _
                                                & "To publish these edits, submit for publishing, or have an administrator 'Publish Changes'.</span>"
        Public Const Msg_AuthoringRecordModifedAdmin As String = "<b>Edits Pending</b><br>" & SpanClassAdminSmall _
                                                & "This record has been edited by <EDITNAME>.<br>" _
                                                & "To publish these edits immediately, hit 'Publish Changes'.<br>" _
                                                & "To submit these changes for workflow publishing, hit 'Submit for Publishing'.</span>"
        Public Const Msg_AuthoringSubmitted As String = "<b>Edits Submitted for Publishing</b><br>" & SpanClassAdminSmall _
                                                & "This record has been edited and was submitted for publishing by <EDITNAME>.</span>"
        Public Const Msg_AuthoringSubmittedAdmin As String = "<b>Edits Submitted for Publishing</b><br>" & SpanClassAdminSmall _
                                                & "This record has been edited and was submitted for publishing by <EDITNAME>.<br>" _
                                                & "As an administrator, you can make changes to this submitted record.<br>" _
                                                & "To publish these edits immediately, hit 'Publish Changes'.<br>" _
                                                & "To deny these edits, hit 'Abort Edits'.<br>" _
                                                & "To approve these edits for workflow publishing, hit 'Approve for Publishing'." _
                                                & "</span>"
        Public Const Msg_AuthoringApproved As String = "<b>Edits Approved for Publishing</b><br>" & SpanClassAdminSmall _
                                                & "This record has been edited and approved for publishing.<br>" _
                                                & "No further changes can be made to this record until an administrator publishs, or aborts publishing." _
                                                & "</span>"
        Public Const Msg_AuthoringApprovedAdmin As String = "<b>Edits Approved for Publishing</b><br>" & SpanClassAdminSmall _
                                                & "This record has been edited and approved for publishing.<br>" _
                                                & "No further changes can be made to this record until an administrator publishs, or aborts publishing.<br>" _
                                                & "To publish these edits immediately, hit 'Publish Changes'.<br>" _
                                                & "To deny these edits, hit 'Abort Edits'." _
                                                & "</span>"
        Public Const Msg_AuthoringSubmittedNotification As String = "The following Content has been submitted for publication. Instructions on how to publish this content to web site are at the bottom of this message.<br>" _
                                                & "<br>" _
                                                & "website: <DOMAINNAME><br>" _
                                                & "Name: <RECORDNAME><br>" _
                                                & "<br>" _
                                                & "Content: <CONTENTNAME><br>" _
                                                & "Record Number: <RECORDID><br>" _
                                                & "<br>" _
                                                & "Submitted: <SUBMITTEDDATE><br>" _
                                                & "Submitted By: <SUBMITTEDNAME><br>" _
                                                & "<br>" _
                                                & "<p>This content has been modified and was submitted for publication by the individual shown above. You were sent this notification because you are a member of the Editors Group for the content that has been changed.</p>" _
                                                & "<p>To publish this content immediately, click on the website link above, check off this record in the box to the far left and click the ""Publish Selected Records"" button.</p>" _
                                                & "<p>To edit the record, click the ""edit"" link for this record, make the desired changes and click the ""Publish"" button.</p>"
        '& "<p>This content has been modified and was submitted for publication by the individual shown above. You were sent this notification because your account on this system is a member of the Editors Group for the content that has been changed.</p>" _
        '& "<p>To publish this content immediately, click on the website link above and locate this record in the list of modified records presented. his content has been modified and was submitted for publication by the individual shown above. Click the 'Admin Site' link to edit the record, and hit the publish button.</p>" _
        '& "<p>To approved this record for workflow publishing, locate the record as described above, and hit the 'Approve for Publishing' button.</p>" _
        '& "<p>To publish all content records approved, go to the Workflow Publishing Screen (on the Administration Menu or the Administration Site) and hit the 'Publish Approved Records' button.</p>"
        Public Const PageNotAvailable_Msg As String = "This page is not currently available. <br>" _
                            & "Please use your back button to return to the previous page. <br>"
        Public Const NewPage_Msg As String = ""
        '
        'Public Const htmlDoc_JavaStreamChunk = 100
        'Public Const htmlDoc_OutStreamStandard = 0
        'Public Const htmlDoc_OutStreamJavaScript = 1
        Public Const main_BakeHeadDelimiter As String = "#####MultilineFlag#####"
        Public Const navStruc_Descriptor = 1           ' Descriptors:0 = RootPage, 1 = Parent Page, 2 = Current Page, 3 = Child Page
        Public Const navStruc_Descriptor_CurrentPage = 2
        Public Const navStruc_Descriptor_ChildPage = 3
        Public Const navStruc_TemplateId = 7
        Public Const blockMessageDefault As String = "<p>The content on this page has restricted access. If you have a username and password for this system, <a href=""?method=login"" rel=""nofollow"">Click Here</a>. For more information, please contact the administrator.</p>"
        Public Const main_BlockSourceDefaultMessage = 0
        Public Const main_BlockSourceCustomMessage = 1
        Public Const main_BlockSourceLogin = 2
        Public Const main_BlockSourceRegistration = 3
        Public Const main_FieldDelimiter As String = " , "
        Public Const main_LineDelimiter As String = " ,, "
        Public Const main_IPosType = 0
        Public Const main_IPosCaption = 1
        Public Const main_IPosRequired = 2
        Public Const main_IPosMax = 2       ' value checked for the line before decoding
        Public Const main_IPosPeopleField = 3
        Public Const main_IPosGroupName = 3
        '
        Public Structure main_FormPagetype_InstType
            Dim Type As Integer
            Dim Caption As String
            Dim REquired As Boolean
            Dim PeopleField As String
            Dim GroupName As String
        End Structure
        '
        Public Structure main_FormPagetype
            Dim PreRepeat As String
            Dim PostRepeat As String
            Dim RepeatCell As String
            Dim AddGroupNameList As String
            Dim AuthenticateOnFormProcess As Boolean
            Dim Inst() As main_FormPagetype_InstType
        End Structure
        '
        ' Cache the input selects (admin uses the same ones over and over)
        '
        Public Structure main_InputSelectCacheType
            Dim SelectRaw As String
            Dim ContentName As String
            Dim Criteria As String
            Dim CurrentValue As String
        End Structure
        '
        ' -- htmlAssetTypes
        Public Enum htmlAssetTypeEnum
            script          ' -- script at end of body (code or link)
            style           ' -- css style at end of body (code or link)
            OnLoadScript    ' -- special case, text is assumed to be script to run on load
        End Enum
        '
        ' -- assets to be added to the head section (and end-of-body) of html documents
        Public Structure htmlAssetClass
            Dim assetType As htmlAssetTypeEnum  ' the type of asset
            Dim inHead As Boolean               ' if true, asset goes in head else it goes at end of body
            Dim isLink As Boolean               ' asset is a link, else it is content
            Dim content As String               ' either link or content
            Dim addedByMessage As String        ' message used during debug to show where the asset came from 
        End Structure
        '
        ' -- metaDescription
        Public Structure htmlMetaClass
            Dim content As String               ' the description, title, etc.
            Dim addedByMessage As String        ' message used during debug to show where the asset came from 
        End Structure

    End Module
    Public Module taskQueueCommandEnumModule
        Public Const runAddon As String = "runaddon"
    End Module

    Public Class cmdDetailClass
        Public addonId As Integer
        Public addonName As String
        Public docProperties As Dictionary(Of String, String)
    End Class
    '
    '-----------------------------------------------------------------------
    '   legacy mainClass arguments
    '   REFACTOR - organize and rename
    '-----------------------------------------------------------------------
    '
    Public Structure NameValuePrivateType
        Dim Name As String
        Dim Value As String
    End Structure
    '
    '====================================================================================================
    '
    Public Class docPropertiesClass
        Public Name As String
        Public Value As String
        Public NameValue As String
        Public IsForm As Boolean
        Public IsFile As Boolean
        'Public FileContent() As Byte
        Public tempfilename As String
        Public FileSize As Integer
        Public fileType As String
    End Class
    '
    ' SF Resize Algorithms
    '
    Public Enum imageResizeAlgorithms
        Box = 0
        Triangle = 1
        Hermite = 2
        Bell = 3
        BSpline = 4
        Lanczos3 = 5
        Mitchell = 6
        Stretch = 7
    End Enum

End Namespace
