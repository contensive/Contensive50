
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
        '
        Public Const requestAppRootPath As String = "/"
        '
        ' -- buttons
        Public Const ButtonCreateFields = " Create Fields "
        Public Const ButtonRun = "     Run     "
        Public Const ButtonSelect = "  Select "
        Public Const ButtonFindAndReplace = " Find and Replace "
        Public Const ButtonIISReset = " IIS Reset "
        Public Const ButtonCancel = " Cancel "
        '
        Public Const protectedContentSetControlFieldList = "ID,CREATEDBY,DATEADDED,MODIFIEDBY,MODIFIEDDATE,EDITSOURCEID,EDITARCHIVE,EDITBLANK,CONTENTCONTROLID"
        '
        Public Const HTMLEditorDefaultCopyStartMark = "<!-- cc -->"
        Public Const HTMLEditorDefaultCopyEndMark = "<!-- /cc -->"
        Public Const HTMLEditorDefaultCopyNoCr = HTMLEditorDefaultCopyStartMark & "<p><br></p>" & HTMLEditorDefaultCopyEndMark
        Public Const HTMLEditorDefaultCopyNoCr2 = "<p><br></p>"
        '
        Public Const IconWidthHeight = " width=21 height=22 "
        '
        Public Const baseCollectionGuid As String = "{7C6601A7-9D52-40A3-9570-774D0D43D758}"        ' part of software dist - base cdef plus addons with classes in in core library, plus depenancy on coreCollection
        Public Const CoreCollectionGuid = "{8DAABAE6-8E45-4CEE-A42C-B02D180E799B}"                  ' contains core Contensive objects, loaded from Library
        Public Const ApplicationCollectionGuid = "{C58A76E2-248B-4DE8-BF9C-849A960F79C6}"           ' exported from application during upgrade
        '
        Public Const adminSiteAddonGuid As String = "{c2de2acf-ca39-4668-b417-aa491e7d8460}"
        Public Const adminCommonAddonGuid = "{76E7F79E-489F-4B0F-8EE5-0BAC3E4CD782}"
        Public Const DashboardAddonGuid = "{4BA7B4A2-ED6C-46C5-9C7B-8CE251FC8FF5}"
        Public Const PersonalizationGuid = "{C82CB8A6-D7B9-4288-97FF-934080F5FC9C}"
        Public Const TextBoxGuid = "{7010002E-5371-41F7-9C77-0BBFF1F8B728}"
        Public Const ContentBoxGuid = "{E341695F-C444-4E10-9295-9BEEC41874D8}"
        Public Const DynamicMenuGuid = "{DB1821B3-F6E4-4766-A46E-48CA6C9E4C6E}"
        Public Const ChildListGuid = "{D291F133-AB50-4640-9A9A-18DB68FF363B}"
        Public Const DynamicFormGuid = "{8284FA0C-6C9D-43E1-9E57-8E9DD35D2DCC}"
        Public Const AddonManagerGuid = "{1DC06F61-1837-419B-AF36-D5CC41E1C9FD}"
        Public Const FormWizardGuid = "{2B1384C4-FD0E-4893-B3EA-11C48429382F}"
        Public Const ImportWizardGuid = "{37F66F90-C0E0-4EAF-84B1-53E90A5B3B3F}"
        Public Const JQueryGuid = "{9C882078-0DAC-48E3-AD4B-CF2AA230DF80}"
        Public Const JQueryUIGuid = "{840B9AEF-9470-4599-BD47-7EC0C9298614}"
        Public Const ImportProcessAddonGuid = "{5254FAC6-A7A6-4199-8599-0777CC014A13}"
        Public Const StructuredDataProcessorGuid = "{65D58FE9-8B76-4490-A2BE-C863B372A6A4}"
        Public Const jQueryFancyBoxGuid = "{24C2DBCF-3D84-44B6-A5F7-C2DE7EFCCE3D}"
        Public Const addonSiteStructureGuid = "{8CDD7960-0FCA-4042-B5D8-3A65BE487AC4}"
        '
        Public Const DefaultLandingPageGuid = "{925F4A57-32F7-44D9-9027-A91EF966FB0D}"
        Public Const DefaultLandingSectionGuid = "{D882ED77-DB8F-4183-B12C-F83BD616E2E1}"
        Public Const DefaultTemplateGuid = "{47BE95E4-5D21-42CC-9193-A343241E2513}"
        Public Const DefaultDynamicMenuGuid = "{E8D575B9-54AE-4BF9-93B7-C7E7FE6F2DB3}"
        '
        Public Const fpoContentBox = "{1571E62A-972A-4BFF-A161-5F6075720791}"
        '
        Public Const sfImageExtList = "jpg,jpeg,gif,png"
        '
        Public Const PageChildListInstanceID = "{ChildPageList}"
        '
        Public Const cr = vbCrLf & vbTab
        Public Const cr2 = cr & vbTab
        Public Const cr3 = cr2 & vbTab
        Public Const cr4 = cr3 & vbTab
        Public Const cr5 = cr4 & vbTab
        Public Const cr6 = cr5 & vbTab
        '
        Public Const AddonOptionConstructor_BlockNoAjax = "Wrapper=[Default:0|None:-1|ListID(Wrappers)]" & vbCrLf & "css Container id" & vbCrLf & "css Container class"
        Public Const AddonOptionConstructor_Block = "Wrapper=[Default:0|None:-1|ListID(Wrappers)]" & vbCrLf & "As Ajax=[If Add-on is Ajax:0|Yes:1]" & vbCrLf & "css Container id" & vbCrLf & "css Container class"
        Public Const AddonOptionConstructor_Inline = "As Ajax=[If Add-on is Ajax:0|Yes:1]" & vbCrLf & "css Container id" & vbCrLf & "css Container class"
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
        Public Const AddonOptionConstructor_ForBlockText = "AllowGroups=[listid(groups)]checkbox"
        Public Const AddonOptionConstructor_ForBlockTextEnd = ""
        Public Const BlockTextStartMarker = "<!-- BLOCKTEXTSTART -->"
        Public Const BlockTextEndMarker = "<!-- BLOCKTEXTEND -->"
        '
        'Private Declare Sub Sleep Lib "kernel32" (ByVal dwMilliseconds As Integer)
        'Private Declare Function GetExitCodeProcess Lib "kernel32" (ByVal hProcess As Integer, ByVal lpExitCode As Integer) As Integer
        'Private Declare Function timeGetTime Lib "winmm.dll" () As Integer
        'Private Declare Function OpenProcess Lib "kernel32" (ByVal dwDesiredAccess As Integer, ByVal bInheritHandle As Integer, ByVal dwProcessId As Integer) As Integer
        'Private Declare Function CloseHandle Lib "kernel32" (ByVal hObject As Integer) As Integer
        '
        Public Const InstallFolderName = "Install"
        Public Const DownloadFileRootNode = "collectiondownload"
        Public Const CollectionFileRootNode = "collection"
        Public Const CollectionFileRootNodeOld = "addoncollection"
        Public Const CollectionListRootNode = "collectionlist"
        '
        Public Const LegacyLandingPageName = "Landing Page Content"
        Public Const DefaultNewLandingPageName = "Home"
        Public Const DefaultLandingSectionName = "Home"
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
        Public Const UserErrorHeadline = "<p class=""ccError"">There was a problem with this page.</p>"
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
        'Public Const KmaError_UnderlyingObject_Msg = "An error occurred in an underlying routine."
        'Public Const KmaccErrorServiceStopped_Msg = "The Contensive CSv Service is not running."
        'Public Const KmaError_BadObject_Msg = "Server Object is not valid."
        'Public Const ignoreString = "Server is busy with internal builder."
        '
        'Public Const KmaError_InvalidArgument_Msg = "Invalid Argument"
        'Public Const KmaError_UnderlyingObject_Msg = "An error occurred in an underlying routine."
        'Public Const KmaccErrorServiceStopped_Msg = "The Contensive CSv Service is not running."
        'Public Const KmaError_BadObject_Msg = "Server Object is not valid."
        'Public Const ignoreString = "Server is busy with internal builder."
        'Public Const KmaError_InvalidArgument_Msg = "Invalid Argument"
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
        Public Const ACTypeDate = "DATE"
        Public Const ACTypeVisit = "VISIT"
        Public Const ACTypeVisitor = "VISITOR"
        Public Const ACTypeMember = "MEMBER"
        Public Const ACTypeOrganization = "ORGANIZATION"
        Public Const ACTypeChildList = "CHILDLIST"
        Public Const ACTypeContact = "CONTACT"
        Public Const ACTypeFeedback = "FEEDBACK"
        Public Const ACTypeLanguage = "LANGUAGE"
        Public Const ACTypeAggregateFunction = "AGGREGATEFUNCTION"
        Public Const ACTypeAddon = "ADDON"
        Public Const ACTypeImage = "IMAGE"
        Public Const ACTypeDownload = "DOWNLOAD"
        Public Const ACTypeEnd = "END"
        Public Const ACTypeTemplateContent = "CONTENT"
        Public Const ACTypeTemplateText = "TEXT"
        Public Const ACTypeDynamicMenu = "DYNAMICMENU"
        Public Const ACTypeWatchList = "WATCHLIST"
        Public Const ACTypeRSSLink = "RSSLINK"
        Public Const ACTypePersonalization = "PERSONALIZATION"
        Public Const ACTypeDynamicForm = "DYNAMICFORM"
        '
        Public Const ACTagEnd = "<ac type=""" & ACTypeEnd & """>"
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
        ' Default username/password
        '
        Public Const DefaultServerUsername = "root"
        Public Const DefaultServerPassword = "contensive"
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
        Public Const FormTypeToolsPanel = "do30a8vl29"
        Public Const FormTypeActiveEditor = "l1gk70al9n"
        Public Const FormTypeImageSelector = "ila9c5s01m"
        Public Const FormTypePageAuthoring = "2s09lmpalb"
        Public Const FormTypeMyProfile = "89aLi180j5"
        Public Const FormTypeLogin = "login"
        'Public Const FormTypeLogin = "l09H58a195"
        Public Const FormTypeSendPassword = "lk0q56am09"
        Public Const FormTypeJoin = "6df38abv00"
        Public Const FormTypeHelpBubbleEditor = "9df019d77sA"
        Public Const FormTypeAddonSettingsEditor = "4ed923aFGw9d"
        Public Const FormTypeAddonStyleEditor = "ar5028jklkfd0s"
        Public Const FormTypeSiteStyleEditor = "fjkq4w8794kdvse"
        'Public Const FormTypeAggregateFunctionProperties = "9wI751270"
        '
        '-----------------------------------------------------------------------
        '   Hardcoded profile form const
        '-----------------------------------------------------------------------
        '
        Public Const rnMyProfileTopics = "profileTopics"
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
        'Public Const RequestNameInterceptpage = "ccIPage"
        '
        Public Const LegacyInterceptPageSNResourceLibrary = "s033l8dm15"
        Public Const LegacyInterceptPageSNSiteExplorer = "kdif3318sd"
        Public Const LegacyInterceptPageSNImageUpload = "ka983lm039"
        Public Const LegacyInterceptPageSNMyProfile = "k09ddk9105"
        Public Const LegacyInterceptPageSNLogin = "6ge42an09a"
        Public Const LegacyInterceptPageSNPrinterVersion = "l6d09a10sP"
        Public Const LegacyInterceptPageSNUploadEditor = "k0hxp2aiOZ"
        '
        '-----------------------------------------------------------------------
        ' Ajax functions intercepted during init, answered and response closed
        '   These are hard-coded internal Contensive functions
        '   These should eventually be replaced with (HardcodedAddons) remote methods
        '   They should all be prefixed "cc"
        '   They are called with cj.ajax.qs(), setting RequestNameAjaxFunction=name in the qs
        '   These name=value pairs go in the QueryString argument of the javascript cj.ajax.qs() function
        '-----------------------------------------------------------------------
        '
        'Public Const RequestNameOpenSettingPage = "settingpageid"
        Public Const RequestNameAjaxFunction = "ajaxfn"
        Public Const RequestNameAjaxFastFunction = "ajaxfastfn"
        '
        Public Const AjaxOpenAdminNav = "aps89102kd"
        Public Const AjaxOpenAdminNavGetContent = "d8475jkdmfj2"
        Public Const AjaxCloseAdminNav = "3857fdjdskf91"
        Public Const AjaxAdminNavOpenNode = "8395j2hf6jdjf"
        Public Const AjaxAdminNavOpenNodeGetContent = "eieofdwl34efvclaeoi234598"
        Public Const AjaxAdminNavCloseNode = "w325gfd73fhdf4rgcvjk2"
        '
        Public Const AjaxCloseIndexFilter = "k48smckdhorle0"
        Public Const AjaxOpenIndexFilter = "Ls8jCDt87kpU45YH"
        Public Const AjaxOpenIndexFilterGetContent = "llL98bbJQ38JC0KJm"
        Public Const AjaxStyleEditorAddStyle = "ajaxstyleeditoradd"
        Public Const AjaxPing = "ajaxalive"
        Public Const AjaxGetFormEditTabContent = "ajaxgetformedittabcontent"
        Public Const AjaxData = "data"
        Public Const AjaxGetVisitProperty = "getvisitproperty"
        Public Const AjaxSetVisitProperty = "setvisitproperty"
        Public Const AjaxGetDefaultAddonOptionString = "ccGetDefaultAddonOptionString"
        Public Const ajaxGetFieldEditorPreferenceForm = "ajaxgetfieldeditorpreference"
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
        'Public Const HardcodedAddonGetDefaultAddonOptionString = "ccGetDefaultAddonOptionString"
        '
        '-----------------------------------------------------------------------
        '   Remote Methods
        '       ?RemoteMethodAddon=string
        '       calls an addon (if marked to run as a remote method)
        '       blocks all other Contensive output (tools panel, javascript, etc)
        '-----------------------------------------------------------------------
        '
        Public Const RequestNameRemoteMethodAddon = "remotemethodaddon"
        '
        '-----------------------------------------------------------------------
        '   Hard Coded Pages
        '       ?Method=string
        '       Querystring based so they can be added to URLs, preserving the current page for a return
        '       replaces output stream with html output
        '-----------------------------------------------------------------------
        '
        Public Const RequestNameHardCodedPage = "method"
        '
        Public Const HardCodedPageLogin = "login"
        Public Const HardCodedPageLoginDefault = "logindefault"
        Public Const HardCodedPageMyProfile = "myprofile"
        Public Const HardCodedPagePrinterVersion = "printerversion"
        Public Const HardCodedPageResourceLibrary = "resourcelibrary"
        Public Const HardCodedPageLogoutLogin = "logoutlogin"
        Public Const HardCodedPageLogout = "logout"
        Public Const HardCodedPageSiteExplorer = "siteexplorer"
        'Public Const HardCodedPageForceMobile = "forcemobile"
        'Public Const HardCodedPageForceNonMobile = "forcenonmobile"
        Public Const HardCodedPageNewOrder = "neworderpage"
        Public Const HardCodedPageStatus = "status"
        Public Const HardCodedPageGetJSPage = "getjspage"
        Public Const HardCodedPageGetJSLogin = "getjslogin"
        Public Const HardCodedPageRedirect = "redirect"
        Public Const HardCodedPageExportAscii = "exportascii"
        Public Const HardCodedPagePayPalConfirm = "paypalconfirm"
        Public Const HardCodedPageSendPassword = "sendpassword"
        '
        '-----------------------------------------------------------------------
        '   Option values
        '       does not effect output directly
        '-----------------------------------------------------------------------
        '
        Public Const RequestNamePageOptions = "ccoptions"
        '
        Public Const PageOptionForceMobile = "forcemobile"
        Public Const PageOptionForceNonMobile = "forcenonmobile"
        Public Const PageOptionLogout = "logout"
        Public Const PageOptionPrinterVersion = "printerversion"
        '
        ' convert to options later
        '
        Public Const RequestNameDashboardReset = "ResetDashboard"
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
        'Public Const SQLFalse = "0"
        'Public Const SQLTrue = "1"
        '
        '-----------------------------------------------------------------------
        ' ----- Style sheet definitions
        '-----------------------------------------------------------------------
        '
        Public Const defaultStyleFilename = "ccDefault.r5.css"
        Public Const StyleSheetStart = "<STYLE TYPE=""text/css"">"
        Public Const StyleSheetEnd = "</STYLE>"
        '
        Public Const SpanClassAdminNormal = "<span class=""ccAdminNormal"">"
        Public Const SpanClassAdminSmall = "<span class=""ccAdminSmall"">"
        '
        ' remove these from ccWebx
        '
        Public Const SpanClassNormal = "<span class=""ccNormal"">"
        Public Const SpanClassSmall = "<span class=""ccSmall"">"
        Public Const SpanClassLarge = "<span class=""ccLarge"">"
        Public Const SpanClassHeadline = "<span class=""ccHeadline"">"
        Public Const SpanClassList = "<span class=""ccList"">"
        Public Const SpanClassListCopy = "<span class=""ccListCopy"">"
        Public Const SpanClassError = "<span class=""ccError"">"
        Public Const SpanClassSeeAlso = "<span class=""ccSeeAlso"">"
        Public Const SpanClassEnd = "</span>"
        '
        '-----------------------------------------------------------------------
        ' ----- XHTML definitions
        '-----------------------------------------------------------------------
        '
        Public Const DTDTransitional = "<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">"
        '
        Public Const BR = "<br>"
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
        'Public Const "ccPanel" = "#E0E0E0"    ' The background color of a panel (black copy visible on it)
        'Public Const "ccPanelHilite" = "#F8F8F8"  '
        'Public Const "ccPanelShadow" = "#808080"  '
        '
        'Public Const HeaderColorBase = "#0320B0"   ' The background color of a panel header (reverse copy visible)
        'Public Const "ccPanelHeaderHilite" = "#8080FF" '
        'Public Const "ccPanelHeaderShadow" = "#000000" '
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
        Public Const FieldTypeIdFileTextPrivate = 10     ' Text saved in a file in the files area.
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
        Public Const FieldTypeIdFileHTMLPrivate = 22       ' TextFile field that expects HTML content
        Public Const FieldTypeIdMax = 22
        '
        ' ----- Field Descriptors for these type
        '       These are what are publicly displayed for each type
        '       See GetFieldTypeNameByType and vise-versa to translater
        '
        Public Const FieldTypeNameInteger = "Integer"
        Public Const FieldTypeNameText = "Text"
        Public Const FieldTypeNameLongText = "LongText"
        Public Const FieldTypeNameBoolean = "Boolean"
        Public Const FieldTypeNameDate = "Date"
        Public Const FieldTypeNameFile = "File"
        Public Const FieldTypeNameLookup = "Lookup"
        Public Const FieldTypeNameRedirect = "Redirect"
        Public Const FieldTypeNameCurrency = "Currency"
        Public Const FieldTypeNameImage = "Image"
        Public Const FieldTypeNameFloat = "Float"
        Public Const FieldTypeNameManyToMany = "ManyToMany"
        Public Const FieldTypeNameTextFile = "TextFile"
        Public Const FieldTypeNameCSSFile = "CSSFile"
        Public Const FieldTypeNameXMLFile = "XMLFile"
        Public Const FieldTypeNameJavascriptFile = "JavascriptFile"
        Public Const FieldTypeNameLink = "Link"
        Public Const FieldTypeNameResourceLink = "ResourceLink"
        Public Const FieldTypeNameMemberSelect = "MemberSelect"
        Public Const FieldTypeNameHTML = "HTML"
        Public Const FieldTypeNameHTMLFile = "HTMLFile"
        '
        Public Const FieldTypeNameLcaseInteger = "integer"
        Public Const FieldTypeNameLcaseText = "text"
        Public Const FieldTypeNameLcaseLongText = "longtext"
        Public Const FieldTypeNameLcaseBoolean = "boolean"
        Public Const FieldTypeNameLcaseDate = "date"
        Public Const FieldTypeNameLcaseFile = "file"
        Public Const FieldTypeNameLcaseLookup = "lookup"
        Public Const FieldTypeNameLcaseRedirect = "redirect"
        Public Const FieldTypeNameLcaseCurrency = "currency"
        Public Const FieldTypeNameLcaseImage = "image"
        Public Const FieldTypeNameLcaseFloat = "float"
        Public Const FieldTypeNameLcaseManyToMany = "manytomany"
        Public Const FieldTypeNameLcaseTextFile = "textfile"
        Public Const FieldTypeNameLcaseCSSFile = "cssfile"
        Public Const FieldTypeNameLcaseXMLFile = "xmlfile"
        Public Const FieldTypeNameLcaseJavascriptFile = "javascriptfile"
        Public Const FieldTypeNameLcaseLink = "link"
        Public Const FieldTypeNameLcaseResourceLink = "resourcelink"
        Public Const FieldTypeNameLcaseMemberSelect = "memberselect"
        Public Const FieldTypeNameLcaseHTML = "html"
        Public Const FieldTypeNameLcaseHTMLFile = "htmlfile"
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
        Public Const ButtonApply = "  Apply "
        Public Const ButtonLogin = "  Login  "
        Public Const ButtonLogout = "  Logout  "
        Public Const ButtonSendPassword = "  Send Password  "
        Public Const ButtonJoin = "   Join   "
        Public Const ButtonSave = "  Save  "
        Public Const ButtonOK = "     OK     "
        Public Const ButtonReset = "  Reset  "
        Public Const ButtonSaveAddNew = " Save + Add "
        'Public Const ButtonSaveAddNew = " Save > Add "
        'Public Const ButtonCancel = " Cancel "
        Public Const ButtonRestartContensiveApplication = " Restart Contensive Application "
        Public Const ButtonCancelAll = "  Cancel  "
        Public Const ButtonFind = "   Find   "
        Public Const ButtonDelete = "  Delete  "
        Public Const ButtonDeletePerson = " Delete Person "
        Public Const ButtonDeleteRecord = " Delete Record "
        Public Const ButtonDeleteEmail = " Delete Email "
        Public Const ButtonDeletePage = " Delete Page "
        Public Const ButtonFileChange = "   Upload   "
        Public Const ButtonFileDelete = "    Delete    "
        Public Const ButtonClose = "  Close   "
        Public Const ButtonAdd = "   Add    "
        Public Const ButtonAddChildPage = " Add Child "
        Public Const ButtonAddSiblingPage = " Add Sibling "
        Public Const ButtonContinue = " Continue >> "
        Public Const ButtonBack = "  << Back  "
        Public Const ButtonNext = "   Next   "
        Public Const ButtonPrevious = " Previous "
        Public Const ButtonFirst = "  First   "
        Public Const ButtonSend = "  Send   "
        Public Const ButtonSendTest = "Send Test"
        Public Const ButtonCreateDuplicate = " Create Duplicate "
        Public Const ButtonActivate = "  Activate   "
        Public Const ButtonDeactivate = "  Deactivate   "
        Public Const ButtonOpenActiveEditor = "Active Edit"
        Public Const ButtonPublish = " Publish Changes "
        Public Const ButtonAbortEdit = " Abort Edits "
        Public Const ButtonPublishSubmit = " Submit for Publishing "
        Public Const ButtonPublishApprove = " Approve for Publishing "
        Public Const ButtonPublishDeny = " Deny for Publishing "
        Public Const ButtonWorkflowPublishApproved = " Publish Approved Records "
        Public Const ButtonWorkflowPublishSelected = " Publish Selected Records "
        Public Const ButtonSetHTMLEdit = " Edit WYSIWYG "
        Public Const ButtonSetTextEdit = " Edit HTML "
        Public Const ButtonRefresh = " Refresh "
        Public Const ButtonOrder = " Order "
        Public Const ButtonSearch = " Search "
        Public Const ButtonSpellCheck = " Spell Check "
        Public Const ButtonLibraryUpload = " Upload "
        Public Const ButtonCreateReport = " Create Report "
        Public Const ButtonClearTrapLog = " Clear Trap Log "
        Public Const ButtonNewSearch = " New Search "
        Public Const ButtonSaveandInvalidateCache = " Save and Invalidate Cache "
        Public Const ButtonImportTemplates = " Import Templates "
        Public Const ButtonRSSRefresh = " Update RSS Feeds Now "
        Public Const ButtonRequestDownload = " Request Download "
        Public Const ButtonFinish = " Finish "
        Public Const ButtonRegister = " Register "
        Public Const ButtonBegin = "Begin"
        Public Const ButtonAbort = "Abort"
        Public Const ButtonCreateGUID = " Create GUID "
        Public Const ButtonEnable = " Enable "
        Public Const ButtonDisable = " Disable "
        Public Const ButtonMarkReviewed = " Mark Reviewed "
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
        Public Const NoteButtonPrevious = " Previous "
        Public Const NoteButtonNext = "   Next   "
        Public Const NoteButtonDelete = "  Delete  "
        Public Const NoteButtonClose = "  Close   "
        '                       ' Submit button is created in CommonDim, so it is simple
        Public Const NoteButtonSubmit = "Submit"
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
        Public Const AdminActionWorkflowPublishApproved = 23    ' Publish what was approved
        Public Const AdminActionSetHTMLEdit = 24        ' Set Member Property for this field to HTML Edit
        Public Const AdminActionSetTextEdit = 25        ' Set Member Property for this field to Text Edit
        Public Const AdminActionSave = 26               ' Save Record
        Public Const AdminActionActivateEmail = 27      ' Activate a Conditional Email
        Public Const AdminActionDeactivateEmail = 28    ' Deactivate a conditional email
        Public Const AdminActionDuplicate = 29          ' Duplicate the (sent email) record
        Public Const AdminActionDeleteRows = 30         ' Delete from rows of records, row0 is boolean, rowid0 is ID, rowcnt is count
        Public Const AdminActionSaveAddNew = 31         ' Save Record and add a new record
        Public Const AdminActionReloadCDef = 32         ' Load Content Definitions
        Public Const AdminActionWorkflowPublishSelected = 33 ' Publish what was selected
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
        Public Const RequestNameDynamicFormID = "dformid"
        '
        Public Const RequestNameRunAddon = "addonid"
        Public Const RequestNameEditReferer = "EditReferer"
        'Public Const RequestNameRefreshBlock = "ccFormRefreshBlockSN"
        Public Const RequestNameCatalogOrder = "CatalogOrderID"
        Public Const RequestNameCatalogCategoryID = "CatalogCatID"
        Public Const RequestNameCatalogForm = "CatalogFormID"
        Public Const RequestNameCatalogItemID = "CatalogItemID"
        Public Const RequestNameCatalogItemAge = "CatalogItemAge"
        Public Const RequestNameCatalogRecordTop = "CatalogTop"
        Public Const RequestNameCatalogFeatured = "CatalogFeatured"
        Public Const RequestNameCatalogSpan = "CatalogSpan"
        Public Const RequestNameCatalogKeywords = "CatalogKeywords"
        Public Const RequestNameCatalogSource = "CatalogSource"
        '
        Public Const RequestNameLibraryFileID = "fileEID"
        Public Const RequestNameDownloadID = "downloadid"
        Public Const RequestNameLibraryUpload = "LibraryUpload"
        Public Const RequestNameLibraryName = "LibraryName"
        Public Const RequestNameLibraryDescription = "LibraryDescription"

        Public Const RequestNameRootPage = "RootPageName"
        Public Const RequestNameRootPageID = "RootPageID"
        Public Const RequestNameContent = "ContentName"
        Public Const RequestNameOrderByClause = "OrderByClause"
        Public Const RequestNameAllowChildPageList = "AllowChildPageList"
        '
        Public Const RequestNameCRKey = "crkey"
        Public Const RequestNameAdminForm = "af"
        Public Const RequestNameAdminSubForm = "subform"
        Public Const RequestNameButton = "button"
        Public Const RequestNameAdminSourceForm = "asf"
        Public Const RequestNameAdminFormSpelling = "SpellingRequest"
        Public Const RequestNameInlineStyles = "InlineStyles"
        Public Const RequestNameAllowCSSReset = "AllowCSSReset"
        '
        Public Const RequestNameReportForm = "rid"
        '
        Public Const RequestNameToolContentID = "ContentID"
        '
        Public Const RequestNameCut = "a904o2pa0cut"
        Public Const RequestNamePaste = "dp29a7dsa6paste"
        Public Const RequestNamePasteParentContentID = "dp29a7dsa6cid"
        Public Const RequestNamePasteParentRecordID = "dp29a7dsa6rid"
        Public Const RequestNamePasteFieldList = "dp29a7dsa6key"
        Public Const RequestNameCutClear = "dp29a7dsa6clear"
        '
        Public Const RequestNameRequestBinary = "RequestBinary"
        ' removed -- this was an old method of blocking form input for file uploads
        'Public Const RequestNameFormBlock = "RB"
        Public Const RequestNameJSForm = "RequestJSForm"
        Public Const RequestNameJSProcess = "ProcessJSForm"
        '
        Public Const RequestNameFolderID = "FolderID"
        '
        Public Const RequestNameEmailMemberID = "emi8s9Kj"
        Public Const RequestNameEmailOpenFlag = "eof9as88"
        Public Const RequestNameEmailOpenCssFlag = "8aa41pM3"
        Public Const RequestNameEmailClickFlag = "ecf34Msi"
        Public Const RequestNameEmailSpamFlag = "9dq8Nh61"
        Public Const RequestNameEmailBlockRequestDropID = "BlockEmailRequest"
        Public Const RequestNameVisitTracking = "s9lD1088"
        Public Const RequestNameBlockContentTracking = "BlockContentTracking"
        Public Const RequestNameCookieDetectVisitID = "f92vo2a8d"

        Public Const RequestNamePageNumber = "PageNumber"
        Public Const RequestNamePageSize = "PageSize"
        '
        Public Const RequestValueNull = "[NULL]"
        '
        Public Const SpellCheckUserDictionaryFilename = "SpellCheck\UserDictionary.txt"
        '
        Public Const RequestNameStateString = "vstate"
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
        Public Const siteproperty_serverPageDefault_name = "serverPageDefault"
        Public Const siteproperty_serverPageDefault_defaultValue = "default.aspx"
        Public Const spAllowPageWithoutSectionDisplay As String = "Allow Page Without Section Display"
        Public Const spAllowPageWithoutSectionDisplay_default As Boolean = True
        '
        '=======================================================================
        '   content replacements
        '=======================================================================
        '
        Public Const contentReplaceEscapeStart = "{%"
        Public Const contentReplaceEscapeEnd = "%}"
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
        Public Const TextSearchStartTagDefault = "<!--TextSearchStart-->"
        Public Const TextSearchEndTagDefault = "<!--TextSearchEnd-->"
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
        Public Const DefaultSpamFooter = "<p>To block future emails from this site, <link>click here</link></p>"
        '
        Public Const FeedbackFormNotSupportedComment = "<!--" & vbCrLf & "Feedback form is not supported in this context" & vbCrLf & "-->"
        '
        '-------------------------------------------------------------------------------------
        '   Page Content constants
        '-------------------------------------------------------------------------------------
        '
        Public Const ContentBlockCopyName = "Content Block Copy"
        '
        Public Const BubbleCopy_AdminAddPage = "Use the Add page to create new content records. The save button puts you in edit mode. The OK button creates the record and exits."
        Public Const BubbleCopy_AdminIndexPage = "Use the Admin Listing page to locate content records through the Admin Site."
        Public Const BubbleCopy_SpellCheckPage = "Use the Spell Check page to verify and correct spelling throught the content."
        Public Const BubbleCopy_AdminEditPage = "Use the Edit page to add and modify content."
        '
        '
        Public Const TemplateDefaultName = "Default"
        'Public Const TemplateDefaultBody = "<!--" & vbCrLf & "Default Template - edit this Page Template, or select a different template for your page or section" & vbCrLf & "-->{{DYNAMICMENU?MENU=}}<br>{{CONTENT}}"
        Public Const TemplateDefaultBody = "" _
            & vbCrLf & vbTab & "<!--" _
            & vbCrLf & vbTab & "Default Template - edit this Page Template, or select a different template for your page or section" _
            & vbCrLf & vbTab & "-->" _
            & vbCrLf & vbTab & "<ac type=""AGGREGATEFUNCTION"" name=""Dynamic Menu"" querystring=""Menu Name=Default"" acinstanceid=""{6CBADABB-5B0D-43E1-B3CA-46A3D60DA3E1}"" >" _
            & vbCrLf & vbTab & "<ac type=""AGGREGATEFUNCTION"" name=""Content Box"" acinstanceid=""{49E0D0C0-D323-49B6-B211-B9599673A265}"" >"
        Public Const TemplateDefaultBodyTag = "<body class=""ccBodyWeb"">"
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
        Public Const SSC_ID = 0
        Public Const SSC_Name = 1
        Public Const SSC_TemplateID = 2
        Public Const SSC_ContentID = 3
        Public Const SSC_MenuImageFilename = 4
        Public Const SSC_Caption = 5
        Public Const SSC_MenuImageOverFilename = 6
        Public Const SSC_HideMenu = 7
        Public Const SSC_BlockSection = 8
        Public Const SSC_RootPageID = 9
        Public Const SSC_JSOnLoad = 10
        Public Const SSC_JSHead = 11
        Public Const SSC_JSEndBody = 12
        Public Const SSC_JSFilename = 13
        Public Const SSC_cnt = 14
        '
        ' Indexes into the TemplateCache
        ' Created from "t.ID,t.Name,t.Link,t.BodyHTML,t.JSOnLoad,t.JSHead,t.JSEndBody,t.StylesFilename,r.StyleID"
        '
        Public Const TC_ID = 0
        Public Const TC_Name = 1
        Public Const TC_Link = 2
        Public Const TC_BodyHTML = 3
        Public Const TC_JSOnLoad = 4
        Public Const TC_JSInHeadLegacy = 5
        'Public Const TC_JSHead = 5
        Public Const TC_JSEndBody = 6
        Public Const TC_StylesFilename = 7
        Public Const TC_SharedStylesIDList = 8
        Public Const TC_MobileBodyHTML = 9
        Public Const TC_MobileStylesFilename = 10
        Public Const TC_OtherHeadTags = 11
        Public Const TC_BodyTag = 12
        Public Const TC_JSInHeadFilename = 13
        'Public Const TC_JSFilename = 13
        Public Const TC_IsSecure = 14
        Public Const TC_DomainIdList = 15
        ' for now, Mobile templates do not have shared styles
        'Public Const TC_MobileSharedStylesIDList = 11
        Public Const TC_cnt = 16
        '
        ' DTD
        '
        Public Const DTDDefault = "<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01 Transitional//EN"" ""http://www.w3.org/TR/html4/loose.dtd"">"
        Public Const DTDDefaultAdmin = "<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01 Transitional//EN"" ""http://www.w3.org/TR/html4/loose.dtd"">"
        '
        ' innova Editor feature list
        '
        Public Const InnovaEditorFeaturefilename = "innova\EditorConfig.txt"
        Public Const InnovaEditorFeatureList = "FullScreen,Preview,Print,Search,Cut,Copy,Paste,PasteWord,PasteText,SpellCheck,Undo,Redo,Image,Flash,Media,CustomObject,CustomTag,Bookmark,Hyperlink,HTMLSource,XHTMLSource,Numbering,Bullets,Indent,Outdent,JustifyLeft,JustifyCenter,JustifyRight,JustifyFull,Table,Guidelines,Absolute,Characters,Line,Form,RemoveFormat,ClearAll,StyleAndFormatting,TextFormatting,ListFormatting,BoxFormatting,ParagraphFormatting,CssText,Styles,Paragraph,FontName,FontSize,Bold,Italic,Underline,Strikethrough,Superscript,Subscript,ForeColor,BackColor"
        Public Const InnovaEditorPublicFeatureList = "FullScreen,Preview,Print,Search,Cut,Copy,Paste,PasteWord,PasteText,SpellCheck,Undo,Redo,Bookmark,Hyperlink,HTMLSource,XHTMLSource,Numbering,Bullets,Indent,Outdent,JustifyLeft,JustifyCenter,JustifyRight,JustifyFull,Table,Guidelines,Absolute,Characters,Line,Form,RemoveFormat,ClearAll,StyleAndFormatting,TextFormatting,ListFormatting,BoxFormatting,ParagraphFormatting,CssText,Styles,Paragraph,FontName,FontSize,Bold,Italic,Underline,Strikethrough,Superscript,Subscript,ForeColor,BackColor"
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
        'Public Const EditorAddonMenuEmailTemplateFilename = "templates/EditorAddonMenuTemplateEmail.js"
        'Public Const EditorAddonMenuEmailContentFilename = "templates/EditorAddonMenuContentEmail.js"
        'Public Const EditorAddonMenuWebTemplateFilename = "templates/EditorAddonMenuTemplateWeb.js"
        'Public Const EditorAddonMenuWebContentFilename = "templates/EditorAddonMenuContentWeb.js"
        '
        Public Const DynamicStylesFilename = "templates/styles.css"
        Public Const AdminSiteStylesFilename = "templates/AdminSiteStyles.css"
        Public Const EditorStyleRulesFilenamePattern = "templates/EditorStyleRules$TemplateID$.js"
        ' deprecated 11/24/3009 - StyleRules destinction between web/email not needed b/c body background blocked
        'Public Const EditorStyleWebRulesFilename = "templates/EditorStyleWebRules.js"
        'Public Const EditorStyleEmailRulesFilename = "templates/EditorStyleEmailRules.js"
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
        Public Const navTypeIDList = "Add-on,Report,Setting,Tool"
        Public Const NavTypeIDAddon = 1
        Public Const NavTypeIDReport = 2
        Public Const NavTypeIDSetting = 3
        Public Const NavTypeIDTool = 4
        '
        Public Const NavIconTypeList = "Custom,Advanced,Content,Folder,Email,User,Report,Setting,Tool,Record,Addon,help"
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
        ' page content cache
        '
        Public Const pageManager_cache_pageContent_cacheName = "cache_pageContent"
        Public Const pageManager_cache_pageContent_fieldList = "ID,Active,ParentID,Name,Headline,MenuHeadline,DateArchive,DateExpires,PubDate,ChildListSortMethodID,ContentControlID,TemplateID" _
                & ",BlockContent,BlockPage,Link,RegistrationGroupID,BlockSourceID,CustomBlockMessage,JSOnLoad,JSHead,JSEndBody,Viewings,ContactMemberID" _
                & ",AllowHitNotification,TriggerSendSystemEmailID,TriggerConditionID,TriggerConditionGroupID,TriggerAddGroupID,TriggerRemoveGroupID,AllowMetaContentNoFollow" _
                & ",ParentListName,copyFilename,BriefFilename,AllowChildListDisplay,SortOrder,DateAdded,ModifiedDate,ChildPagesFound,AllowInMenus,AllowInChildLists" _
                & ",JSFilename,ChildListInstanceOptions,IsSecure,AllowBrief,allowReturnLinkDisplay,AllowPrinterVersion,allowEmailPage,allowSeeAlso,allowMoreInfo" _
                & ",allowFeedback,allowLastModifiedFooter,modifiedBy,DateReviewed,ReviewedBy,allowReviewedFooter,allowMessageFooter,contentpadding"
        Public Const PCC_ID = 0
        Public Const PCC_Active = 1
        Public Const PCC_ParentID = 2
        Public Const PCC_Name = 3
        Public Const PCC_Headline = 4
        Public Const PCC_MenuHeadline = 5
        Public Const PCC_DateArchive = 6
        Public Const PCC_DateExpires = 7
        Public Const PCC_PubDate = 8
        Public Const PCC_ChildListSortMethodID = 9
        Public Const PCC_ContentControlID = 10
        Public Const PCC_TemplateID = 11
        Public Const PCC_BlockContent = 12
        Public Const PCC_BlockPage = 13
        Public Const PCC_Link = 14
        Public Const PCC_RegistrationGroupID = 15
        Public Const PCC_BlockSourceID = 16
        Public Const PCC_CustomBlockMessageFilename = 17
        Public Const PCC_JSOnLoad = 18
        Public Const PCC_JSHead = 19
        Public Const PCC_JSEndBody = 20
        Public Const PCC_Viewings = 21
        Public Const PCC_ContactMemberID = 22
        Public Const PCC_AllowHitNotification = 23
        Public Const PCC_TriggerSendSystemEmailID = 24
        Public Const PCC_TriggerConditionID = 25
        Public Const PCC_TriggerConditionGroupID = 26
        Public Const PCC_TriggerAddGroupID = 27
        Public Const PCC_TriggerRemoveGroupID = 28
        Public Const PCC_AllowMetaContentNoFollow = 29
        Public Const PCC_ParentListName = 30
        Public Const PCC_CopyFilename = 31
        Public Const PCC_BriefFilename = 32
        Public Const PCC_AllowChildListDisplay = 33
        Public Const PCC_SortOrder = 34
        Public Const PCC_DateAdded = 35
        Public Const PCC_ModifiedDate = 36
        Public Const PCC_ChildPagesFound = 37
        Public Const PCC_AllowInMenus = 38
        Public Const PCC_AllowInChildLists = 39
        Public Const PCC_JSFilename = 40
        Public Const PCC_ChildListInstanceOptions = 41
        Public Const PCC_IsSecure = 42
        Public Const PCC_AllowBrief = 43
        Public Const PCC_allowReturnLinkDisplay = 44
        Public Const pcc_allowPrinterVersion = 45
        Public Const pcc_allowEmailPage = 46
        Public Const pcc_allowSeeAlso = 47
        Public Const pcc_allowMoreInfo = 48
        Public Const pcc_allowFeedback = 49
        Public Const pcc_allowLastModifiedFooter = 50
        Public Const PCC_ModifiedBy = 51
        Public Const PCC_DateReviewed = 52
        Public Const PCC_ReviewedBy = 53
        Public Const PCC_allowReviewedFooter = 54
        Public Const PCC_allowMessageFooter = 55
        Public Const PCC_ContentPadding = 56
        Public Const PCC_ColCnt = 57
        '
        ' addonIncludeRules cache
        '
        Public Const cache_sharedStylesAddonRules_cacheName = "cache_sharedStylesAddonRules"
        Public Const cache_sharedStylesAddonRules_fieldList = "addonId,styleId"
        Public Const sharedStylesAddonRulesCache_addonId = 0
        Public Const sharedStylesAddonRulesCache_styleId = 1
        Public Const sharedStylesAddonRulesCacheColCnt = 2
        '
        ' linkalias cache
        '
        Public Const cache_linkAlias_fieldList = "ID,Name,Link,PageID,QueryStringSuffix"
        Public Const linkAliasCache_id = 0
        Public Const linkAliasCache_name = 1
        Public Const linkAliasCache_link = 2
        Public Const linkAliasCache_pageId = 3
        Public Const linkAliasCache_queryStringSuffix = 4
        Public Const linkAliasCacheColCnt = 5
        '
        '   addonCache
        '
        'Public Const cache_addon_cacheName = "cache_addon"
        'Public Const cache_addon_fieldList = "id,active,name,ccguid,collectionid,Copy,ccguid,Link,ObjectProgramID,DotNetClass,ArgumentList,CopyText,IsInline,BlockDefaultStyles,StylesFilename,CustomStylesFilename,formxml,RemoteAssetLink,AsAjax,InFrame,ScriptingEntryPoint,ScriptingLanguageID,ScriptingCode,BlockEditTools,ScriptingTimeout,inlineScript,help,helplink,JavaScriptOnLoad,JavaScriptBodyEnd,PageTitle,MetaDescription,MetaKeywordList,OtherHeadTags,JSFilename,remoteMethod,onBodyStart,onBodyEnd,OnPageStartEvent,OnPageEndEvent,robotsTxt"
        'Public Const addonCache_Id = 0
        'Public Const addonCache_active = 1
        'Public Const addonCache_name = 2
        'Public Const addonCache_guid = 3
        'Public Const addonCache_collectionid = 4
        'Public Const addonCache_Copy = 5
        'Public Const addonCache_ccguid = 6
        'Public Const addonCache_Link = 7
        'Public Const addonCache_ObjectProgramID = 8
        'Public Const addonCache_DotNetClass = 9
        'Public Const addonCache_ArgumentList = 10
        'Public Const addonCache_CopyText = 11
        'Public Const addonCache_IsInline = 12
        'Public Const addonCache_BlockDefaultStyles = 13
        'Public Const addonCache_StylesFilename = 14
        'Public Const addonCache_CustomStylesFilename = 15
        'Public Const addonCache_formxml = 16
        'Public Const addonCache_RemoteAssetLink = 17
        'Public Const addonCache_AsAjax = 18
        'Public Const addonCache_InFrame = 19
        'Public Const addonCache_ScriptingEntryPoint = 20
        'Public Const addonCache_ScriptingLanguageID = 21
        'Public Const addonCache_ScriptingCode = 22
        'Public Const addonCache_BlockEditTools = 23
        'Public Const addonCache_ScriptingTimeout = 24
        'Public Const addonCache_inlineScript = 25
        'Public Const addonCache_help = 26
        'Public Const addonCache_helpLink = 27
        'Public Const addonCache_JavaScriptOnLoad = 28
        'Public Const addonCache_JavaScriptBodyEnd = 29
        'Public Const addonCache_PageTitle = 30
        'Public Const addonCache_MetaDescription = 31
        'Public Const addonCache_MetaKeywordList = 32
        'Public Const addonCache_OtherHeadTags = 33
        'Public Const addonCache_JSFilename = 34
        'Public Const addonCache_remoteMethod = 35
        'Public Const addoncache_OnBodyStart = 36
        'Public Const addoncache_OnBodyEnd = 37
        'Public Const addoncache_OnPageStart = 38
        'Public Const addoncache_OnPageEnd = 39
        'Public Const addoncache_robotsTxt = 40
        'Public Const addonCacheColCnt = 41
        '
        ' addonIncludeRules cache
        '
        Public Const cache_addonIncludeRules_cacheName = "cache_addonIncludeRules"
        Public Const cache_addonIncludeRules_fieldList = "addonId,includedAddonId"
        Public Const addonIncludeRulesCache_addonId = 0
        Public Const addonIncludeRulesCache_includedAddonId = 1
        Public Const addonIncludeRulesCacheColCnt = 2
        '
        ' addonIncludeRules cache
        '
        Public Const cache_LibraryFiles_cacheName = "cache_LibraryFiles"
        Public Const cache_LibraryFiles_fieldList = "id,ccguid,clicks,filename,width,height,AltText,altsizelist"
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
        Public Const cache_linkForward_cacheName = "cache_linkForward"
        '
        Public Const main_cookieNameVisit = "visit"
        Public Const main_cookieNameVisitor = "visitor"
        Public Const html_quickEdit_fpo = "<quickeditor>"
    End Module
End Namespace
