
Option Explicit On
Option Strict On

Imports Contensive.BaseClasses

Namespace Contensive.Core
    Public Module constantsModule
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
    End Module
    '
End Namespace
