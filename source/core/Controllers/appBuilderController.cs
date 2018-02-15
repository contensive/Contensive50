
using System;
using System.Reflection;
using System.Xml;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Contensive.Core;
using Contensive.Core.Models.DbModels;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
using System.Data;
using System.Linq;
using Contensive.Core.Models.Context;
//
namespace Contensive.Core.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// code to built and upgrade apps
    /// not IDisposable - not contained classes that need to be disposed
    /// </summary>
    public class appBuilderController {
        //
        //====================================================================================================
        //
        //private struct fieldTypePrivate {
        //    public string Name;
        //    //todo  NOTE: This member was renamed since members cannot have the same name as their enclosing type in C#:
        //    public int fieldTypePrivate_Renamed;
        //}
        //        '
        //        '=======================================================================================
        //        '   Register a dotnet assembly (only with interop perhaps)
        //        '
        //        '   Must be called from a process running as admin
        //        '   This can be done using the command queue, which kicks off the ccCmd process from the Server
        //        '=======================================================================================
        //        '
        //        Public shared Sub RegisterDotNet(ByVal FilePathFileName As String)
        //            On Error GoTo ErrorTrap
        //            '
        //            Dim RegAsmFound As Boolean
        //            Dim Cmd As String
        //            Dim Filename As String
        //            Dim FilePathFileNameNoExt As String
        //            Dim Ptr As Integer
        //            Dim LogFilename As String
        //            Dim MonthNumber As Integer
        //            Dim DayNumber As Integer
        //            Dim Filenamec As String
        //            Dim DotNetInstallPath As String
        //            'Dim fs As New fileSystemClass
        //            Dim RegAsmFilename As String

        //            Const userRoot As String = "HKEY_LOCAL_MACHINE"
        //            Const subkey As String = "\SOFTWARE\Microsoft\.NETFramework"
        //            Const keyName As String = userRoot & "\" & subkey
        //            '
        //            DotNetInstallPath = Microsoft.Win32.Registry.GetValue(keyName, "InstallRoot", "")
        //            If DotNetInstallPath = "" Then
        //                DotNetInstallPath = "C:\WINNT\Microsoft.NET\Framework\"
        //                If Not core.app.publicFiles.checkPath(DotNetInstallPath) Then
        //                    DotNetInstallPath = "C:\Windows\Microsoft.NET\Framework\"
        //                    If Not core.app.publicFiles.checkPath(DotNetInstallPath) Then
        //                        DotNetInstallPath = ""
        //                    End If
        //                End If
        //            End If
        //            If DotNetInstallPath = "" Then
        //                Call handleLegacyClassError2("", "RegisterDotNet", "Could not detect dotnet installation path. Dot Net may not be installed.")
        //            Else
        //                RegAsmFound = True
        //                RegAsmFilename = DotNetInstallPath & "v2.0.50727\regasm.exe"
        //                If Not core.app.publicFiles.checkFile(RegAsmFilename) Then
        //                    RegAsmFilename = DotNetInstallPath & "v3.0\regasm.exe"
        //                    If Not core.app.publicFiles.checkFile(RegAsmFilename) Then
        //                        RegAsmFilename = DotNetInstallPath & "v3.5\regasm.exe"
        //                        If Not core.app.publicFiles.checkFile(RegAsmFilename) Then
        //                            RegAsmFound = False
        //                        End If
        //                    End If
        //                End If
        //                If Not RegAsmFound Then
        //                    Call handleLegacyClassError2("", "RegisterDotNet", "Could not find regasm.exe in subfolders v2.0.50727, v3.0, or v3.5 of dotnet installation path [" & RegAsmFilename & "]. Dot Net may not be installed.")
        //                Else
        //                    Ptr = InStrRev(FilePathFileName, ".")
        //                    If Ptr > 0 Then
        //                        FilePathFileNameNoExt = Mid(FilePathFileName, 1, Ptr - 1)
        //                        DayNumber = Day(Now)
        //                        MonthNumber = Month(Now)
        //                        LogFilename = Year(Now)
        //                        If MonthNumber < 10 Then
        //                            LogFilename = LogFilename & "0"
        //                        End If
        //                        LogFilename = LogFilename & MonthNumber
        //                        If DayNumber < 10 Then
        //                            LogFilename = LogFilename & "0"
        //                        End If
        //                        LogFilename = getProgramFilesPath() & "\Logs\addoninstall\" & LogFilename & DayNumber & ".log"
        //                        Cmd = """" & RegAsmFilename & """ """ & FilePathFileName & """ /codebase"
        //                        Call runProcess(core, Cmd, , True)
        //                    End If
        //                End If
        //            End If

        //            'core.AppendLog("dll" & ".SiteBuilderClass.RegisterDotnet called Regsvr32, called but the output could not be captured")
        //            '
        //            Exit Sub
        ////ErrorTrap:
        //            Call handleLegacyClassError1("unknown", "RegisterDotnet")
        //        End Sub
        //
        //=============================================================================================================
        //   Main
        //       Returns nothing if all OK, else returns an error message
        //=============================================================================================================
        //

        //Public Function createApp(ByVal appName As String, ByVal domainName As String) As Boolean
        //    Dim returnOk As Boolean = False
        //    Try
        //        Dim builder As builderClass
        //        Dim useIIS As Boolean
        //        Dim iisDefaultDoc As String
        //        Dim cpNewApp As CPClass
        //        '
        //        useIIS = False
        //        iisDefaultDoc = ""
        //        Select Case core.cluster.config.appPattern.ToLower
        //            Case "php"
        //                useIIS = True
        //                iisDefaultDoc = "index.php"
        //            Case "aspnet"
        //                useIIS = True
        //                iisDefaultDoc = "default.aspx"
        //            Case "iismodule"
        //                useIIS = True
        //                iisDefaultDoc = "default.aspx"
        //            Case "nodejs"
        //        End Select
        //        '
        //        ' setup iis
        //        '
        //        If useIIS Then
        //            Call web_addSite(appName, domainName, "\", iisDefaultDoc)
        //        End If
        //        '
        //        ' initialize the new app, use the save authentication that was used to authorize this object
        //        '
        //        cpNewApp = New CPClass(appName)
        //        builder = New builderClass(cpNewApp.core)
        //        Call builder.upgrade(core,core,True)
        //        Call cpNewApp.core.app.siteProperty_set(siteproperty_serverPageDefault_name, iisDefaultDoc)
        //        cpNewApp.Dispose()
        //    Catch ex As Exception
        //        core.handleException(ex);
        //    End Try
        //    Return returnOk
        //End Function
        //
        //=============================================================================================================
        //   Main
        //       Returns nothing if all OK, else returns an error message
        //=============================================================================================================
        //
        //Public Function importApp(core As coreClass, ByVal siteName As String, ByVal IPAddress As String, ByVal DomainName As String, ByVal ODBCConnectionString As String, ByVal ContentFilesPath As String, ByVal WWWRootPath As String, ByVal defaultDoc As String, ByVal SMTPServer As String, ByVal AdminEmail As String) As String
        //    Dim returnMessage As String = ""
        //    Try
        //        If siteName = "" Then
        //            returnMessage = "The application name was blank. It is required."
        //        Else
        //            '
        //            If defaultDoc = "" Then
        //                '
        //                ' it was required, this is the best guess
        //                '
        //                defaultDoc = siteproperty_serverPageDefault_defaultValue
        //            End If
        //            '
        //            If IPAddress = "" Then
        //                IPAddress = "127.0.0.1"
        //            End If
        //            '
        //            If DomainName = "" Then
        //                DomainName = IPAddress
        //            End If
        //            '
        //            If ContentFilesPath = "" Then
        //                ContentFilesPath = "c:\inetpub\apps\" & siteName & "\cdnFiles"
        //            End If
        //            If Right(ContentFilesPath, 1) <> "\" Then
        //                ContentFilesPath = ContentFilesPath & "\"
        //            End If
        //            '
        //            If ODBCConnectionString = "" Then
        //                ODBCConnectionString = siteName
        //            End If
        //            '
        //            If WWWRootPath = "" Then
        //                WWWRootPath = "c:\inetpub\apps\" & siteName & "\appRoot"
        //            End If
        //            If Right(WWWRootPath, 1) <> "\" Then
        //                WWWRootPath = WWWRootPath & "\"
        //            End If
        //            '
        //            If SMTPServer = "" Then
        //                SMTPServer = "127.0.0.1"
        //            End If
        //            '
        //            If AdminEmail = "" Then
        //                AdminEmail = "admin@" & DomainName
        //            End If
        //            '
        //            ' Configure Contensive
        //            '
        //            'Call VerifyApp2(siteName, DomainName, ODBCConnectionString, ContentFilesPath, "/", WWWRootPath)
        //            '
        //            ' Rebuild IIS Server
        //            '
        //            Call iisController.verifySite(core, siteName, DomainName, "\", defaultDoc)
        //            '
        //            ' Now wait here for site to start with upgrade
        //            '
        //            Call upgrade(core, False)
        //            returnMessage = ""
        //        End If
        //    Catch ex As Exception

        //    End Try
        //    Return returnMessage
        //End Function

        //
        //========================================================================
        //   Init()
        //========================================================================
        //
        //Public shared Sub Init(appservicesObj As coreClass)
        //    appservices = appservicesObj
        //    On Error GoTo ErrorTrap
        //    '
        //    Dim MethodName As String
        //    '
        //    MethodName = "Init"
        //    '
        //    ApplicationNameLocal = core.app.appEnvironment.name
        //    ClassInitialized = True
        //    '
        //    Exit Sub
        //    '
        ////ErrorTrap:
        //    dim ex as new exception("todo"): Call HandleClassError(ex,core.app.appEnvironment.name,Err.Number, Err.Source, Err.Description, "Init", True, False)
        //End Sub
        //
        //=========================================================================
        //
        //=========================================================================
        //
        //Public shared Sub Upgrade()
        //    '
        //    ' deprecated call
        //    '
        //    Call Upgrade2( "index.php")
        //End Sub
        //Public shared Sub upgrade(ByVal appName As String, ByVal clusterServices As clusterServicesClass, isNewSite As Boolean)
        //    Dim appservices As New appServicesClass(appName, clusterServices)
        //    If Not core.app.config.enabled Then
        //        Call Upgrade2( isNewSite)
        //    Else
        //        core.AppendLog("Cannot upgrade until the site is disabled")
        //    End If
        //End Sub
        //
        //=========================================================================
        // upgrade
        //=========================================================================
        //
        public static void upgrade(coreController core, bool isNewBuild) {
            try {
                if (core.doc.upgradeInProgress) {
                    // leftover from 4.1
                } else {
                    core.doc.upgradeInProgress = true;
                    string DataBuildVersion = core.siteProperties.dataBuildVersion;
                    List<string> nonCriticalErrorList = new List<string>();
                    //
                    // -- determine primary domain
                    string primaryDomain = core.appConfig.name;
                    if (core.appConfig.domainList.Count > 0) {
                        primaryDomain = core.appConfig.domainList[0];
                    }
                    //
                    // -- Verify core table fields (DataSources, Content Tables, Content, Content Fields, Setup, Sort Methods), then other basic system ops work, like site properties
                    VerifyBasicTables(core);
                    //
                    // -- verify base collection
                    logController.appendLogInstall(core, "Install base collection");
                    collectionController.installBaseCollection(core, isNewBuild,ref  nonCriticalErrorList);
                    //
                    // -- Update server config file
                    logController.appendLogInstall(core, "Update configuration file");
                    if (!core.appConfig.appStatus.Equals(appConfigModel.appStatusEnum.OK)) {
                        core.appConfig.appStatus = appConfigModel.appStatusEnum.OK;
                        core.serverConfig.saveObject(core);
                    }
                    //
                    // -- verify iis configuration
                    logController.appendLogInstall(core, "Verify iis configuration");
                    Controllers.iisController.verifySite(core, core.appConfig.name, primaryDomain, core.appConfig.appRootFilesPath, "default.aspx");
                    //
                    // -- verify root developer
                    logController.appendLogInstall(core, "verify developer user");
                    var rootList = personModel.createList(core, "(Developer<>0)");
                    if ( rootList.Count==0 ) {
                        logController.appendLogInstall(core, "verify root user, no developers found, adding root/contensive");
                        var root = personModel.add(core);
                        root.name = "root";
                        root.FirstName = "root";
                        root.Username = "root";
                        root.Password = "contensive";
                        root.Developer = true;
                        root.save(core);
                    }
                    //
                    //---------------------------------------------------------------------
                    // ----- Convert Database fields for new Db
                    //---------------------------------------------------------------------
                    //
                    if (isNewBuild) {
                        //
                        // -- set build version so a scratch build will not go through data conversion
                        DataBuildVersion = core.codeVersion();
                        core.siteProperties.dataBuildVersion = core.codeVersion();
                    }
                    //
                    //---------------------------------------------------------------------
                    // ----- Upgrade Database fields if not new
                    //---------------------------------------------------------------------
                    //
                    if (string.CompareOrdinal(DataBuildVersion, core.codeVersion()) < 0) {
                        //
                        // -- data updates
                        logController.appendLogInstall(core, "Run database conversions, DataBuildVersion [" + DataBuildVersion + "], software version [" + core.codeVersion() + "]");
                        Upgrade_Conversion(core, DataBuildVersion);
                    }
                    //
                    //---------------------------------------------------------------------
                    // ----- Verify content needed internally
                    //---------------------------------------------------------------------
                    //
                    logController.appendLogInstall(core, "Verify records required");
                    //
                    //  menus are created in ccBase.xml, this just checks for dups
                    VerifyAdminMenus(core, DataBuildVersion);
                    VerifyLanguageRecords(core);
                    VerifyCountries(core);
                    VerifyStates(core);
                    VerifyLibraryFolders(core);
                    VerifyLibraryFileTypes(core);
                    VerifyDefaultGroups(core);
                    VerifyScriptingRecords(core);
                    //
                    //---------------------------------------------------------------------
                    // ----- Set Default SitePropertyDefaults
                    //       must be after upgrade_conversion
                    //---------------------------------------------------------------------
                    //
                    logController.appendLogInstall(core, "Verify Site Properties");
                    // todo remove site properties not used, put all in preferences
                    core.siteProperties.getText("AllowAutoHomeSectionOnce", genericController.encodeText(isNewBuild));
                    core.siteProperties.getText("AllowAutoLogin", "False");
                    core.siteProperties.getText("AllowBake", "True");
                    core.siteProperties.getText("AllowChildMenuHeadline", "True");
                    core.siteProperties.getText("AllowContentAutoLoad", "True");
                    core.siteProperties.getText("AllowContentSpider", "False");
                    core.siteProperties.getText("AllowContentWatchLinkUpdate", "True");
                    core.siteProperties.getText("AllowDuplicateUsernames", "False");
                    core.siteProperties.getText("ConvertContentText2HTML", "False");
                    core.siteProperties.getText("AllowMemberJoin", "False");
                    core.siteProperties.getText("AllowPasswordEmail", "True");
                    core.siteProperties.getText("AllowPathBlocking", "True");
                    core.siteProperties.getText("AllowPopupErrors", "True");
                    core.siteProperties.getText("AllowTestPointLogging", "False");
                    core.siteProperties.getText("AllowTestPointPrinting", "False");
                    core.siteProperties.getText("AllowTransactionLog", "False");
                    core.siteProperties.getText("AllowTrapEmail", "True");
                    core.siteProperties.getText("AllowTrapLog", "True");
                    core.siteProperties.getText("AllowWorkflowAuthoring", "False");
                    core.siteProperties.getText("ArchiveAllowFileClean", "False");
                    core.siteProperties.getText("ArchiveRecordAgeDays", "90");
                    core.siteProperties.getText("ArchiveTimeOfDay", "2:00:00 AM");
                    core.siteProperties.getText("BreadCrumbDelimiter", "&nbsp;&gt;&nbsp;");
                    core.siteProperties.getText("CalendarYearLimit", "1");
                    core.siteProperties.getText("ContentPageCompatibility21", "false");
                    core.siteProperties.getText("DefaultFormInputHTMLHeight", "500");
                    core.siteProperties.getText("DefaultFormInputTextHeight", "1");
                    core.siteProperties.getText("DefaultFormInputWidth", "60");
                    core.siteProperties.getText("EditLockTimeout", "5");
                    core.siteProperties.getText("EmailAdmin", "webmaster@" + core.appConfig.domainList[0]);
                    core.siteProperties.getText("EmailFromAddress", "webmaster@" + core.appConfig.domainList[0]);
                    core.siteProperties.getText("EmailPublishSubmitFrom", "webmaster@" + core.appConfig.domainList[0]);
                    core.siteProperties.getText("Language", "English");
                    core.siteProperties.getText("PageContentMessageFooter", "Copyright " + core.appConfig.domainList[0]);
                    core.siteProperties.getText("SelectFieldLimit", "4000");
                    core.siteProperties.getText("SelectFieldWidthLimit", "100");
                    core.siteProperties.getText("SMTPServer", "127.0.0.1");
                    core.siteProperties.getText("TextSearchEndTag", "<!-- TextSearchEnd -->");
                    core.siteProperties.getText("TextSearchStartTag", "<!-- TextSearchStart -->");
                    core.siteProperties.getText("TrapEmail", "");
                    core.siteProperties.getText("TrapErrors", "0");
                    addonModel defaultRouteAddon = addonModel.create(core, core.siteProperties.defaultRouteId);
                    if (defaultRouteAddon == null) {
                        defaultRouteAddon = addonModel.create(core, addonGuidPageManager);
                        if (defaultRouteAddon != null) {
                            core.siteProperties.defaultRouteId = defaultRouteAddon.id;
                        }
                    }
                    //
                    //---------------------------------------------------------------------
                    // ----- Changes that effect the web server or content files, not the Database
                    //---------------------------------------------------------------------
                    //
                    int StyleSN = (core.siteProperties.getInteger("StylesheetSerialNumber"));
                    if (StyleSN > 0) {
                        StyleSN += 1;
                        core.siteProperties.setProperty("StylesheetSerialNumber", StyleSN.ToString());
                        // too lazy
                        //Call core.app.publicFiles.SaveFile(core.app.genericController.convertCdnUrlToCdnPathFilename("templates\Public" & StyleSN & ".css"), core.app.csv_getStyleSheetProcessed)
                        //Call core.app.publicFiles.SaveFile(core.app.genericController.convertCdnUrlToCdnPathFilename("templates\Admin" & StyleSN & ".css", core.app.csv_getStyleSheetDefault)
                    }
                    //
                    // clear all cache
                    //
                    core.cache.invalidateAll();
                    //
                    if (isNewBuild) {
                        //
                        // -- primary domain
                        domainModel domain = domainModel.createByName(core, primaryDomain);
                        if (domain == null) {
                            domain = domainModel.add(core);
                            domain.name = primaryDomain;
                        }
                        //
                        // -- Landing Page
                        pageContentModel landingPage = pageContentModel.create(core, DefaultLandingPageGuid);
                        if (landingPage == null) {
                            landingPage = pageContentModel.add(core);
                            landingPage.ccguid = DefaultLandingPageGuid;
                        }
                        //
                        // -- default template
                        pageTemplateModel defaultTemplate = pageTemplateModel.createByName(core, "Default");
                        if (defaultTemplate == null) {
                            defaultTemplate = pageTemplateModel.add(core);
                            defaultTemplate.name = "Default";
                        }
                        domain.defaultTemplateId = defaultTemplate.id;
                        domain.name = primaryDomain;
                        domain.pageNotFoundPageId = landingPage.id;
                        domain.rootPageId = landingPage.id;
                        domain.typeId = (int) domainModel.domainTypeEnum.Normal;
                        domain.visited = false;
                        domain.save(core);
                        //
                        landingPage.TemplateID = defaultTemplate.id;
                        landingPage.Copyfilename.content = constants.defaultLandingPageHtml;
                        landingPage.save(core);
                        //
                        defaultTemplate.bodyHTML = core.appRootFiles.readFileText(defaultTemplateHomeFilename);
                        defaultTemplate.save(core);
                        //
                        if (core.siteProperties.getInteger("LandingPageID", landingPage.id) == 0) {
                            core.siteProperties.setProperty("LandingPageID", landingPage.id);
                        }
                    }
                    //
                    //---------------------------------------------------------------------
                    // ----- internal upgrade complete
                    //---------------------------------------------------------------------
                    //
                    {
                        logController.appendLogInstall(core, "Internal upgrade complete, set Buildversion to " + core.codeVersion());
                        core.siteProperties.setProperty("BuildVersion", core.codeVersion());
                        //
                        //---------------------------------------------------------------------
                        // ----- Upgrade local collections
                        //       This would happen if this is the first site to upgrade after a new build is installed
                        //       (can not be in startup because new addons might fail with DbVersions)
                        //       This means a dataupgrade is required with a new build - You can expect errors
                        //---------------------------------------------------------------------
                        //
                        {
                            //
                            // 4.1.575 - 8/28 - put this code behind the DbOnly check, makes DbOnly beuild MUCH faster
                            //
                            string ErrorMessage = "";
                            bool IISResetRequired = false;
                            //RegisterList = ""
                            logController.appendLogInstall(core, "Upgrading All Local Collections to new server build.");
                            string tmpString = "";
                            bool UpgradeOK = collectionController.UpgradeLocalCollectionRepoFromRemoteCollectionRepo(core, ref ErrorMessage, ref tmpString, ref  IISResetRequired, isNewBuild, ref  nonCriticalErrorList);
                            if (!string.IsNullOrEmpty(ErrorMessage)) {
                                throw (new ApplicationException("Unexpected exception")); //core.handleLegacyError3(core.appConfig.name, "During UpgradeAllLocalCollectionsFromLib3 call, " & ErrorMessage, "dll", "builderClass", "Upgrade2", 0, "", "", False, True, "")
                            } else if (!UpgradeOK) {
                                throw (new ApplicationException("Unexpected exception")); //core.handleLegacyError3(core.appConfig.name, "During UpgradeAllLocalCollectionsFromLib3 call, NotOK was returned without an error message", "dll", "builderClass", "Upgrade2", 0, "", "", False, True, "")
                            }
                            //
                            //---------------------------------------------------------------------
                            // ----- Upgrade collections added during upgrade process
                            //---------------------------------------------------------------------
                            //
                            //Call appendUpgradeLog(core, "Installing Add-on Collections gathered during upgrade")
                            //If InstallCollectionList = "" Then
                            //    Call appendUpgradeLog(core.app.config.name, MethodName, "No Add-on collections added during upgrade")
                            //Else
                            //    ErrorMessage = ""
                            //    Guids = Split(InstallCollectionList, ",")
                            //    For Ptr = 0 To UBound(Guids)
                            //        ErrorMessage = ""
                            //        Guid = Guids[Ptr]
                            //        If Guid <> "" Then
                            //            saveLogFolder = classLogFolder
                            //            Call addonInstall.GetCollectionConfig(Guid, CollectionPath, LastChangeDate, "")
                            //            classLogFolder = saveLogFolder
                            //            If CollectionPath <> "" Then
                            //                '
                            //                ' This collection is installed locally, install from local collections
                            //                '
                            //                saveLogFolder = classLogFolder
                            //                Call addonInstall.installCollectionFromLocalRepo(Me, IISResetRequired, Guid, core.version, ErrorMessage, "", "", isNewBuild)
                            //                classLogFolder = saveLogFolder
                            //                'Call AddonInstall.UpgradeAppFromLocalCollection( Me, ParentNavigatorID, IISResetRequired, Guid, getcodeversion, ErrorMessage, RegisterList, "")
                            //            Else
                            //                '
                            //                ' This is a new collection, install to the server and force it on this site
                            //                '
                            //                saveLogFolder = classLogFolder
                            //                addonInstallOk = addonInstall.installCollectionFromRemoteRepo(Guid, DataBuildVersion, IISResetRequired, "", ErrorMessage, "", isNewBuild)
                            //                classLogFolder = saveLogFolder
                            //                If Not addonInstallOk Then
                            //                   throw (New ApplicationException("Unexpected exception"))'core.handleLegacyError3(core.app.config.name, "Error upgrading Addon Collection [" & Guid & "], " & ErrorMessage, "dll", "builderClass", "Upgrade2", 0, "", "", False, True, "")
                            //                End If
                            //            End If
                            //        End If
                            //    Next
                            //End If
                            //
                            //---------------------------------------------------------------------
                            // ----- Upgrade all collection for this app (in case collections were installed before the upgrade
                            //---------------------------------------------------------------------
                            //
                            string Collectionname = null;
                            string CollectionGuid = null;
                            bool localCollectionFound = false;
                            logController.appendLogInstall(core, "Checking all installed collections for upgrades from Collection Library");
                            logController.appendLogInstall(core, "...Open collectons.xml");
                            try {
                                XmlDocument Doc = new XmlDocument();
                                Doc.LoadXml(collectionController.getLocalCollectionStoreListXml(core));
                                if (true) {
                                    if (genericController.vbLCase(Doc.DocumentElement.Name) != genericController.vbLCase(CollectionListRootNode)) {
                                        throw (new ApplicationException("Unexpected exception")); //core.handleLegacyError3(core.appConfig.name, "Error loading Collection config file. The Collections.xml file has an invalid root node, [" & Doc.DocumentElement.Name & "] was received and [" & CollectionListRootNode & "] was expected.", "dll", "builderClass", "Upgrade", 0, "", "", False, True, "")
                                    } else {
                                        if (genericController.vbLCase(Doc.DocumentElement.Name) == "collectionlist") {
                                            //
                                            // now go through each collection in this app and check the last updated agains the one here
                                            //
                                            logController.appendLogInstall(core, "...Open site collectons, iterate through all collections");
                                            //Dim dt As DataTable
                                            DataTable dt = core.db.executeQuery("select * from ccaddoncollections where (ccguid is not null)and(updatable<>0)");
                                            if (dt.Rows.Count > 0) {
                                                int rowptr = 0;
                                                for (rowptr = 0; rowptr < dt.Rows.Count; rowptr++) {

                                                    ErrorMessage = "";
                                                    CollectionGuid = genericController.vbLCase(dt.Rows[rowptr]["ccguid"].ToString());
                                                    Collectionname = dt.Rows[rowptr]["name"].ToString();
                                                    logController.appendLogInstall(core, "...checking collection [" + Collectionname + "], guid [" + CollectionGuid + "]");
                                                    if (CollectionGuid != "{7c6601a7-9d52-40a3-9570-774d0d43d758}") {
                                                        //
                                                        // upgrade all except base collection from the local collections
                                                        //
                                                        localCollectionFound = false;
                                                        bool upgradeCollection = false;
                                                        DateTime LastChangeDate = genericController.encodeDate(dt.Rows[rowptr]["LastChangeDate"]);
                                                        if (LastChangeDate == DateTime.MinValue) {
                                                            //
                                                            // app version has no lastchangedate
                                                            //
                                                            upgradeCollection = true;
                                                            appendUpgradeLog(core, core.appConfig.name, "upgrade", "Upgrading collection " + dt.Rows[rowptr]["name"].ToString() + " because the collection installed in the application has no LastChangeDate. It may have been installed manually.");
                                                        } else {
                                                            //
                                                            // compare to last change date in collection config file
                                                            //
                                                            string LocalGuid = "";
                                                            DateTime LocalLastChangeDate = DateTime.MinValue;
                                                            foreach (XmlNode LocalListNode in Doc.DocumentElement.ChildNodes) {
                                                                switch (genericController.vbLCase(LocalListNode.Name)) {
                                                                    case "collection":
                                                                        foreach (XmlNode CollectionNode in LocalListNode.ChildNodes) {
                                                                            switch (genericController.vbLCase(CollectionNode.Name)) {
                                                                                case "guid":
                                                                                    //
                                                                                    LocalGuid = genericController.vbLCase(CollectionNode.InnerText);
                                                                                    break;
                                                                                case "lastchangedate":
                                                                                    //
                                                                                    LocalLastChangeDate = genericController.encodeDate(CollectionNode.InnerText);
                                                                                    break;
                                                                            }
                                                                        }
                                                                        break;
                                                                }
                                                                if (CollectionGuid == genericController.vbLCase(LocalGuid)) {
                                                                    localCollectionFound = true;
                                                                    logController.appendLogInstall(core, "...local collection found");
                                                                    if (LocalLastChangeDate != DateTime.MinValue) {
                                                                        if (LocalLastChangeDate > LastChangeDate) {
                                                                            appendUpgradeLog(core, core.appConfig.name, "upgrade", "Upgrading collection " + dt.Rows[rowptr]["name"].ToString() + " because the collection in the local server store has a newer LastChangeDate than the collection installed on this application.");
                                                                            upgradeCollection = true;
                                                                        }
                                                                    }
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                        ErrorMessage = "";
                                                        if (!localCollectionFound) {
                                                            logController.appendLogInstall(core, "...site collection [" + Collectionname + "] not found in local collection, call UpgradeAllAppsFromLibCollection2 to install it.");
                                                            bool addonInstallOk = collectionController.installCollectionFromRemoteRepo(core, CollectionGuid, ref  ErrorMessage, "", isNewBuild, ref nonCriticalErrorList);
                                                            if (!addonInstallOk) {
                                                                //
                                                                // this may be OK so log, but do not call it an error
                                                                //
                                                                logController.appendLogInstall(core, "...site collection [" + Collectionname + "] not found in collection Library. It may be a custom collection just for this site. Collection guid [" + CollectionGuid + "]");
                                                            }
                                                        } else {
                                                            if (upgradeCollection) {
                                                                logController.appendLogInstall(core, "...upgrading collection");
                                                                collectionController.installCollectionFromLocalRepo(core, CollectionGuid, core.codeVersion(), ref ErrorMessage, "", isNewBuild, ref nonCriticalErrorList);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                            } catch (Exception ex9) {
                                core.handleException(ex9);
                            }
                        }
                    }
                    //
                    //---------------------------------------------------------------------
                    // ----- Explain, put up a link and exit without continuing
                    //---------------------------------------------------------------------
                    //
                    core.cache.invalidateAll();
                    logController.appendLogInstall(core, "Upgrade Complete");
                    core.doc.upgradeInProgress = false;
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }
        //        '
        //        ' ----- Rename a content definition to a new name
        //        '
        //        private shared sub RenameContentDefinition(ByVal OldName As String, ByVal NewName As String)

        //            On Error GoTo ErrorTrap
        //            '
        //            Dim ErrorDescription As String
        //            Dim MethodName As String
        //            Dim SQL As String
        //            '
        //            MethodName = "RenameContentDefinition"
        //            '
        //            SQL = "UPDATE ccContent SET ccContent.name = '" & NewName & "' WHERE (((ccContent.name)='" & OldName & "'));"
        //            Call core.app.executeSql(SQL)
        //            '
        //            Exit Sub
        //            '
        //            ' ----- Error Trap
        //            '
        ////ErrorTrap:
        //            Dim ex As New Exception("todo") : Call handleClassException(ex, core.app.config.name, "methodNameFPO") ' Err.Number, Err.Source, Err.Description, "RenameContentDefinition", True, False)
        //        End Sub

        //        '
        //        '
        //        '
        //        private shared sub UpgradeSortOrder( ByVal DataSourceName As String, ByVal TableName As String)
        //            On Error GoTo ErrorTrap
        //            '
        //            Dim ErrorDescription As String
        //            Dim CSPointer As Integer
        //            Dim methodName As String = "UpgradeSortOrder"
        //            '
        //            Call core.app.csv_CreateSQLTableField(DataSourceName, TableName, "TempField", FieldTypeText)
        //            CSPointer = core.app.csv_OpenCSSQL(DataSourceName, "SELECT ID, Sortorder from " & TableName & " Order By Sortorder;")
        //            If Not core.app.csv_IsCSOK(CSPointer) Then
        //                Dim ex2 As New Exception("todo") : Call HandleClassError(ex2, core.app.config.name, methodName) ' ignoreInteger, "dll", "Could not upgrade SortOrder", "UpgradeSortOrder", False, True)
        //            Else
        //                Do While core.app.csv_IsCSOK(CSPointer)
        //                    Call core.app.ExecuteSQL(DataSourceName, "UPDATE " & TableName & " SET TempField=" & encodeSQLText(Format(core.app.csv_cs_getInteger(CSPointer, "sortorder"), "00000000")) & " WHERE ID=" & encodeSQLNumber(core.app.csv_cs_getInteger(CSPointer, "ID")) & ";")
        //                    core.app.csv_NextCSRecord(CSPointer)
        //                Loop
        //                Call core.app.csv_CloseCS(CSPointer)
        //                Call core.app.csv_DeleteTableIndex(DataSourceName, TableName, "SORTORDER")
        //                Call core.app.csv_DeleteTableIndex(DataSourceName, TableName, TableName & "SORTORDER")
        //                Call core.app.csv_DeleteTableField(DataSourceName, TableName, "SortOrder")
        //                Call core.app.csv_CreateSQLTableField(DataSourceName, TableName, "SortOrder", FieldTypeText)
        //                Call core.app.ExecuteSQL(DataSourceName, "UPDATE " & TableName & " SET SortOrder=TempField;")
        //                Call core.app.csv_CreateSQLIndex(DataSourceName, TableName, TableName & "SORTORDER", "SortOrder")
        //            End If
        //            Call core.app.csv_DeleteTableField(DataSourceName, TableName, "TempField")
        //            '
        //            Exit Sub
        //            '
        //            ' ----- Error Trap
        //            '
        ////ErrorTrap:
        //            Dim ex As New exception("todo") : Call HandleClassError(ex, core.app.config.name, "methodNameFPO") ' Err.Number, Err.Source, Err.Description, "UpgradeSortOrder", True, False)
        //        End Sub
        //        '
        //        '=============================================================================
        //        ' ----- Returns true if the content field exists
        //        '=============================================================================
        //        '
        //        Private shared Function ExistsSQLTableField(ByVal TableName As String, ByVal FieldName As String) As Boolean
        //            On Error GoTo ErrorTrap
        //            '
        //            Dim ErrorDescription As String
        //            ' converted array to dictionary - Dim FieldPointer As Integer
        //            Dim dt As DataTable
        //            Dim UcaseTableFieldName
        //            Dim UcaseFieldName As String
        //            Dim MethodName As String
        //            '
        //            MethodName = "ExistsSQLTableField"
        //            '
        //            ExistsSQLTableField = False
        //            '
        //            dt = core.app.executeSql("SELECT * FROM " & TableName & ";")
        //            If dt.Rows.Count = 0 Then
        //                Call core.app.executeSql("INSERT INTO " & TableName & " (Name)VALUES('no name');")
        //                dt = core.app.executeSql("SELECT * FROM " & TableName & ";")
        //            End If
        //            If dt.Rows.Count > 0 Then

        //                For FieldPointer = 1 To dt.Rows.Count - 1
        //                    If UcaseFieldName = genericController.vbUCase(dt.Rows(FieldPointer)["name")) Then
        //                        ExistsSQLTableField = True
        //                        Exit For
        //                    End If
        //                Next
        //            End If
        //            '
        //            Exit Function
        //            '
        //            ' ----- Error Trap
        //            '
        ////ErrorTrap:
        //            Dim ex As New Exception("todo") : Call handleClassException(ex, core.app.config.name, "methodNameFPO") ' Err.Number, Err.Source, Err.Description, "ExistsSQLTableField", True, False)
        //        End Function
        //        '
        //        '
        //        '
        //        private shared sub CreatePage( ByVal ContentName As String, ByVal PageName As String, ByVal PageCopy As String)
        //            On Error GoTo ErrorTrap
        //            '
        //            Dim ErrorDescription As String
        //            Dim CSPointer As Integer
        //            Dim RecordID As Integer
        //            Dim Filename As String
        //            '
        //            CSPointer = core.app.csv_InsertCSRecord(ContentName)
        //            If core.app.csv_IsCSOK(CSPointer) Then
        //                RecordID = (core.app.csv_cs_getInteger(CSPointer, "ID"))
        //                Filename = core.app.csv_cs_getFilename(CSPointer, "Name", "")
        //                'Filename = core.app.csv_GetVirtualFilename(ContentName, "Name", RecordID)
        //                Call core.app.csv_SetCSField(CSPointer, "name", PageName)
        //                Call core.app.csv_SetCSField(CSPointer, "copyfilename", Filename)
        //                Call core.app.publicFiles.SaveFile(Filename, PageCopy)
        //            End If
        //            core.app.csv_CloseCS(CSPointer)
        //            Exit Sub
        //            '
        //            ' ----- Error Trap
        //            '
        ////ErrorTrap:
        //            Dim ex As New Exception("todo") : Call HandleClassError(ex, core.app.config.name, "createPage") ' Err.Number, Err.Source, Err.Description, "CreatePage", True, False)
        //        End Sub
        //        '
        //        '==========================================================================================
        //        '   Add the ccTable for version 3.0.300
        //        '==========================================================================================
        //        '
        //        private shared sub PopulateTableTable()
        //            On Error GoTo ErrorTrap
        //            '
        //            Dim ErrorDescription As String
        //            Dim SQL As String
        //            dim dt as datatable
        //            Dim RSTables as datatable
        //            Dim RSNewTable as datatable
        //            Dim DataSourceID As Integer
        //            Dim ContentCID As Integer
        //            Dim TablesCID As Integer
        //            Dim MethodName As String
        //            Dim RunUpgrade As Boolean
        //            Dim SQLNow As String
        //            Dim TableName As String
        //            Dim TableID As Integer
        //            Dim ContentID As Integer
        //            '
        //            MethodName = "PopulateTableTable"
        //            '
        //            If False Then
        //                Exit Sub
        //            End If
        //            '
        //            ' ----- Make sure content definition exists
        //            '
        //            Call core.app.csv_CreateContentFromSQLTable("Default", "ccTables", "Tables")
        //            TablesCID = core.app.csv_GetContentID("Tables")
        //            If True Then
        //                '
        //                ' ----- Create the ccTables TableName entry in the ccContent Table
        //                '
        //                SQL = "Update ccContent set sqlTable='ccTables' where name='tables';"
        //                Call core.app.executeSql(SQL)
        //                '
        //                ' ----- Append tables from ccContent
        //                '
        //                SQL = "Select ID, sqlTable,DataSourceID From ccContent where active<>0;"
        //                RS = core.app.executeSql(SQL)
        //                If (isDataTableOk(rs)) Then
        //                    '
        //                    ' if no error, field exists, and it is OK to continue
        //                    '
        //                    Do While Not rs.rows.count=0
        //                        TableName = genericController.encodeText(core.app.getDataRowColumnName(RS.rows(0), "sqlTable"))
        //                        TableID = 0
        //                        DataSourceID = genericController.EncodeInteger(core.app.getDataRowColumnName(RS.rows(0), "DataSourceID"))
        //                        ContentID = genericController.EncodeInteger(core.app.getDataRowColumnName(RS.rows(0), "ID"))
        //                        If TableName <> "" Then
        //                            '
        //                            ' ----- Get TableID from TableName
        //                            '
        //                            SQL = "SELECT ID FROM ccTables where name=" & EncodeSQLText(TableName) & ";"
        //                            RSTables = core.app.executeSql(SQL)
        //                            If Not RSTables.EOF Then
        //                                '
        //                                ' ----- Table entry found
        //                                '
        //                                TableID = core.app.getDataRowColumnName(RSTables, "ID")
        //                            Else
        //                                '
        //                                ' ----- Table entry not found in ccTables, Create it
        //                                '
        //                                RSNewTable = core.app.csv_InsertTableRecordGetDataTable("Default", "ccTables")
        //                                If Not (RSNewTable Is Nothing) Then
        //                                    TableID = core.app.getDataRowColumnName(RSNewTable, "ID")
        //                                    Call RSNewTable.Close()
        //                                    If DataSourceID = 0 Then
        //                                        '
        //                                        ' ----- New entry has null datasource
        //                                        '
        //                                        SQL = "Update ccTables set active=" & SQLTrue & ", ContentControlID=" & EncodeSQLNumber(TablesCID) & ", Name=" & EncodeSQLText(TableName) & " where ID=" & TableID & ";"
        //                                    Else
        //                                        '
        //                                        ' ----- New entry has datasource
        //                                        '
        //                                        SQL = "Update ccTables set active=" & SQLTrue & ", ContentControlID=" & EncodeSQLNumber(TablesCID) & ", Name=" & EncodeSQLText(TableName) & ", DataSourceId=" & EncodeSQLNumber(DataSourceID) & " where ID=" & TableID & ";"
        //                                    End If
        //                                    Call core.app.executeSql(SQL)
        //                                End If
        //                                RSNewTable = Nothing
        //                            End If
        //                            RSTables = Nothing
        //                        End If
        //                        If TableID <> 0 Then
        //                            SQL = "Update ccContent set ContentTableID=" & EncodeSQLNumber(TableID) & ", AuthoringTableID=" & EncodeSQLNumber(TableID) & " where ID=" & ContentID & ";"
        //                            Call core.app.executeSql(SQL)
        //                        End If
        //                        Call RS.MoveNext()
        //                    Loop
        //                    If (isDataTableOk(rs)) Then
        //                        If false Then
        //                            'RS.Close()
        //                        End If
        //                        'RS = Nothing
        //                    End If
        //                End If
        //            End If
        //            '
        //            Exit Sub
        //            '
        //            ' ----- Error Trap
        //            '
        ////ErrorTrap:
        //            UpgradeErrorCount = UpgradeErrorCount + 1
        //            Dim ex As New exception("todo") : Call HandleClassError(ex, core.app.config.name, "methodNameFPO") ' Err.Number, Err.Source, Err.Description, "PopulateTableTable (error #" & UpgradeErrorCount & ")", True, True)
        //            If UpgradeErrorCount >= UpgradeErrorTheshold Then
        //                Dim ex3 As New Exception("todo") : Call HandleClassError(ex3, core.app.config.name, MethodName) ' Err.Number, Err.Source, Err.Description, "PopulateTableTable (error #" & UpgradeErrorCount & ")", True, False)
        //            End If
        //            //Resume Next
        //        End Sub

        //        '
        //        ' ----- Upgrade to version 4.1.xxx
        //        '
        //        private shared sub Upgrade_Conversion_to_41( ByVal DataBuildVersion As String)
        //            On Error GoTo ErrorTrap
        //            '
        //            Dim ErrMessage As String
        //            Dim typeId As Integer
        //            Dim sf As Object
        //            Dim FilenameNoExt As String
        //            Dim Filename As String
        //            Dim Pos As Integer
        //            Dim FilenameExt As String
        //            Dim AltSizeList As String
        //            Dim Height As Integer
        //            Dim Width As Integer
        //            Dim SQL As String
        //            Dim RecordID As Integer
        //            Dim CS As Integer
        //            Dim CSSrc As Integer
        //            Dim CSDst As Integer
        //            Dim Guid As String
        //            Dim Name As String
        //            Dim addonId As Integer
        //            Dim runAtServer As runAtServerClass
        //            Dim addonInstall As New addonInstallClass
        //            Dim IISResetRequired As Boolean
        //            Dim RegisterList As String
        //            Dim MethodName As String
        //            Dim ContentID As Integer
        //            Dim TimeoutSave As Integer
        //            Dim CID As Integer
        //            ''
        //            'TimeoutSave = core.app.csv_SQLCommandTimeout
        //            'core.app.csv_SQLCommandTimeout = 1800
        //            '
        //            If False Then
        //                Exit Sub
        //            Else
        //                '
        //                MethodName = "Upgrade_Conversion_to_41"
        //                '
        //                If false Then
        //                    Call appendUpgradeLogAddStep(core.app.config.name,MethodName, "4.1.279 upgrade")
        //                    '
        //                    ' added ccguid to all cdefs, but non-base did not upgrade automatically
        //                    '
        //                    CS = core.app.csOpen("content")
        //                    Do While core.app.csv_IsCSOK(CS)
        //                        Call metaData_VerifyCDefField_ReturnID(True, core.app.csv_cs_getText(CS, "name"), "ccguid", FieldTypeText, , False, "Guid", , , , , , , , , , , , , , , , , True)
        //                        Call core.app.csv_NextCSRecord(CS)
        //                    Loop
        //                    Call core.app.csv_CloseCS(CS)
        //                End If
        //                If false Then
        //                    Call appendUpgradeLogAddStep(core.app.config.name,MethodName, "4.1.288 upgrade")
        //                    '
        //                    ' added updatable again (was originally added in 275)
        //                    '
        //                    Call core.app.ExecuteSQL("", "update ccAddonCollections set updatable=1")
        //                End If
        //                If false Then
        //                    Call appendUpgradeLogAddStep(core.app.config.name,MethodName, "4.1.290 upgrade")
        //                    '
        //                    ' delete blank field help records, new method creates dups of inheritance cdef parent fields
        //                    '
        //                    Call core.app.ExecuteSQL("", "delete from ccfieldhelp where (HelpDefault is null)and(HelpCustom is null)")
        //                End If
        //                If false Then
        //                    Call appendUpgradeLogAddStep(core.app.config.name,MethodName, "4.1.294 upgrade")
        //                    '
        //                    ' convert fieldtypelongtext + htmlcontent to fieldtypehtml
        //                    '
        //                    Call core.app.ExecuteSQL("", "update ccfields set type=" & FieldTypeHTML & " where type=" & FieldTypeLongText & " and (htmlcontent<>0)")
        //                    '
        //                    ' convert fieldtypetextfile + htmlcontent to fieldtypehtmlfile
        //                    '
        //                    Call core.app.ExecuteSQL("", "update ccfields set type=" & FieldTypeHTMLFile & " where type=" & FieldTypeTextFile & " and (htmlcontent<>0)")
        //                End If
        //                If false Then
        //                    Call appendUpgradeLogAddStep(core.app.config.name,MethodName, "4.1.352 upgrade")
        //                    '
        //                    ' try again - from 4.1.195
        //                    ' Some content used to be base, but now they are in add-ons
        //                    ' the isbasefield and isbasecontent needs to be cleard
        //                    '
        //                    Call RemoveIsBase( "items")
        //                    Call RemoveIsBase( "orders")
        //                    Call RemoveIsBase( "order details")
        //                    Call RemoveIsBase( "event images")
        //                    Call RemoveIsBase( "Promotions")
        //                    Call RemoveIsBase( "Page Types")
        //                    Call RemoveIsBase( "Forums")
        //                    Call RemoveIsBase( "Forum Messages")
        //                    Call RemoveIsBase( "Item Categories")
        //                    Call RemoveIsBase( "Survey Question Types")
        //                    Call RemoveIsBase( "Survey Questions")
        //                    Call RemoveIsBase( "Surveys")
        //                    Call RemoveIsBase( "Meeting Links")
        //                    Call RemoveIsBase( "news articles")
        //                    Call RemoveIsBase( "news issues")
        //                    Call RemoveIsBase( "Order Ship Methods")
        //                    Call RemoveIsBase( "Promotion Uses")
        //                    Call RemoveIsBase( "Spider Tasks")
        //                    Call RemoveIsBase( "Spider Cookies")
        //                    Call RemoveIsBase( "Spider Docs")
        //                    Call RemoveIsBase( "Spider Word Hits")
        //                    Call RemoveIsBase( "Spider Errors")
        //                    Call RemoveIsBase( "Calendar Events")
        //                    Call RemoveIsBase( "Calendars")
        //                    Call RemoveIsBase( "WhitePapers")
        //                    Call RemoveIsBase( "Survey Results")
        //                    Call RemoveIsBase( "Orders Completed")
        //                    Call RemoveIsBase( "Meetings")
        //                    Call RemoveIsBase( "Meeting Sessions")
        //                    Call RemoveIsBase( "Meeting Attendee Types")
        //                    Call RemoveIsBase( "Meeting Session Fees")
        //                    Call RemoveIsBase( "Meeting Attendees")
        //                    Call RemoveIsBase( "Meeting Attendee Sessions")
        //                    Call RemoveIsBase( "PAY METHODS")
        //                    Call RemoveIsBase( "Meeting Pay Methods")
        //                    Call RemoveIsBase( "Memberships")
        //                    Call RemoveIsBase( "Organization Address Types")
        //                End If
        //                If false Then
        //                    Call appendUpgradeLogAddStep(core.app.config.name,MethodName, "4.1.374 upgrade")
        //                    '
        //                    ' repair the country table (abbreviation was set to an integer a long time ago)
        //                    '
        //                    SQL = "insert into cccountries (abbreviation)values('US')"
        //                    //On Error //Resume Next
        //                    Call core.app.ExecuteSQL("", CStr(SQL))
        //                    ErrMessage = Err.Description
        //                    Err.Clear()
        //                    On Error GoTo ErrorTrap
        //                    SQL = "delete from cccountries where (name is null) or (name='')"
        //                    Call core.app.ExecuteSQL("", CStr(SQL))
        //                    If ErrMessage <> "" Then
        //                        '
        //                        ' needs to be fixed
        //                        '
        //                        typeId = core.app.csv_GetDataSourceType("default")
        //                        If typeId = DataSourceTypeODBCAccess Then
        //                            '
        //                            ' MS Access
        //                            '
        //                            SQL = "alter table cccountries add column abbr VarChar(255) NULL"
        //                            Call core.app.ExecuteSQL("", CStr(SQL))
        //                            '
        //                            SQL = "update cccountries set abbr=abbreviation"
        //                            Call core.app.ExecuteSQL("", CStr(SQL))
        //                            '
        //                            SQL = "alter table cccountries drop abbreviation"
        //                            Call core.app.ExecuteSQL("", CStr(SQL))
        //                            '
        //                            SQL = "alter table cccountries add column abbreviation VarChar(255) NULL"
        //                            Call core.app.ExecuteSQL("", CStr(SQL))
        //                            '
        //                            SQL = "update cccountries set abbreviation=abbr"
        //                            Call core.app.ExecuteSQL("", CStr(SQL))
        //                            '
        //                            SQL = "alter table cccountries drop abbr"
        //                            Call core.app.ExecuteSQL("", CStr(SQL))
        //                        ElseIf typeId = DataSourceTypeODBCSQLServer Then
        //                            '
        //                            ' MS SQL Server
        //                            '
        //                            SQL = "alter table cccountries add abbr VarChar(255) NULL"
        //                            Call core.app.ExecuteSQL("", CStr(SQL))
        //                            '
        //                            SQL = "update cccountries set abbr=abbreviation"
        //                            Call core.app.ExecuteSQL("", CStr(SQL))
        //                            '
        //                            SQL = "alter table cccountries drop column abbreviation"
        //                            Call core.app.ExecuteSQL("", CStr(SQL))
        //                            '
        //                            SQL = "alter table cccountries add abbreviation VarChar(255) NULL"
        //                            Call core.app.ExecuteSQL("", CStr(SQL))
        //                            '
        //                            SQL = "update cccountries set abbreviation=abbr"
        //                            Call core.app.ExecuteSQL("", CStr(SQL))
        //                            '
        //                            SQL = "alter table cccountries drop column abbr"
        //                            Call core.app.ExecuteSQL("", CStr(SQL))
        //                        End If
        //                    End If
        //                    '
        //                    ' remove all ccwebx3 and ccwebx4 addons
        //                    '
        //                    SQL = "delete from ccaggregatefunctions where ObjectProgramID like '%ccwebx3%'"
        //                    Call core.app.ExecuteSQL("", CStr(SQL))
        //                End If
        //                If false Then
        //                    Call appendUpgradeLogAddStep(core.app.config.name,MethodName, "4.1.508 upgrade")
        //                    '
        //                    ' repair the country table (abbreviation was set to an integer a long time ago)
        //                    '
        //                    Call core.app.setSiteProperty("DefaultFormInputHTMLHeight", "500", 0)
        //                End If
        //                If false Then
        //                    Call appendUpgradeLogAddStep(core.app.config.name,MethodName, "4.1.517 upgrade")
        //                    '
        //                    ' prepopulate docore.app.main_allowCrossLogin
        //                    '
        //                    SQL = "update ccDomains set allowCrossLogin=1 where typeid=1"
        //                    Call core.app.ExecuteSQL("", CStr(SQL))
        //                End If
        //                If false Then
        //                    Call appendUpgradeLogAddStep(core.app.config.name,MethodName, "4.1.588 upgrade")
        //                    '
        //                    ' ccfields.htmlContent redefined
        //                    '   means nothing for fields not set to html or htmlFile
        //                    '   for these, it means the initial editor should show the HTML (not wysiwyg) -- "real html"
        //                    '
        //                    SQL = "update ccFields set htmlcontent=null where (name<>'layout')or(contentid<>" & core.app.csv_GetContentID("layouts") & ")"
        //                    Call core.app.ExecuteSQL("", CStr(SQL))
        //                End If
        //                '
        //                ' return the normal timeout
        //                '
        //                core.app.csv_SQLCommandTimeout = TimeoutSave
        //                '
        //                ' Regsiter and IISReset if needed
        //                '
        //                If RegisterList <> "" Then
        //                    Call addonInstall.RegisterActiveXFiles(RegisterList)
        //                    Call addonInstall.RegisterDotNet(RegisterList)
        //                    RegisterList = ""
        //                End If
        //                '
        //                ' IISReset if needed
        //                '
        //                If IISResetRequired Then
        //                    '
        //                    ' Restart IIS if stopped
        //                    '
        //                    runAtServer = New runAtServerClass
        //                    Call runAtServer.executeCmd("IISReset", "")
        //                End If
        //            End If

        //            Exit Sub
        //            '
        //            ' ----- Error Trap
        //            '
        ////ErrorTrap:
        //            UpgradeErrorCount = UpgradeErrorCount + 1
        //            Dim ex As New exception("todo") : Call HandleClassError(ex, core.app.config.name, "methodNameFPO") ' Err.Number, Err.Source, Err.Description, "Upgrade_Conversion_to_41 (error #" & UpgradeErrorCount & ")", True, True)
        //            If UpgradeErrorCount >= UpgradeErrorTheshold Then
        //                Dim ex4 As New Exception("todo") : Call HandleClassError(ex4, core.app.config.name, MethodName) ' Err.Number, Err.Source, Err.Description, "Upgrade_Conversion_to_41 (error #" & UpgradeErrorCount & ")", True, False)
        //            End If
        //            //Resume Next
        //        End Sub
        //        '
        //        ' ----- Delete unused fields from both the Content Definition and the Table
        //        '
        //        private shared sub DeleteField( ByVal DataSourceName As String, ByVal TableName As String, ByVal FieldName As String)
        //            On Error GoTo ErrorTrap
        //            '
        //            Dim RSTables as datatable
        //            'Dim RSContent as datatable
        //            Dim TableID As Integer
        //            Dim ContentID As Integer
        //            '
        //            ' Delete any indexes found for this field
        //            '
        //            Call core.app.csv_DeleteTableIndex(DataSourceName, TableName, TableName & FieldName)
        //            'Call core.app.csv_DeleteTableIndex(DataSourceName, TableName, TableName & FieldName)
        //            '
        //            ' Delete all Content Definition Fields associated with this field
        //            '
        //            RSTables = core.app.ExecuteSQL(DataSourceName, "SELECT ID from ccTables where name='" & TableName & "';")
        //            If Not (RSTables Is Nothing) Then
        //                Do While Not RSTables.EOF
        //                    TableID = genericController.EncodeInteger(RSTables("ID"))
        //                    RSContent = core.app.ExecuteSQL(DataSourceName, "Select ID from ccContent where (ContentTableID=" & TableID & ")or(AuthoringTableID=" & TableID & ");")
        //                    If Not (RSContent Is Nothing) Then
        //                        Do While Not RSContent.EOF
        //                            ContentID = genericController.EncodeInteger(RSContent("ID"))
        //                            Call core.app.ExecuteSQL(DataSourceName, "Delete From ccFields where (ContentID=" & ContentID & ")and(name=" & encodeSQLText(FieldName) & ");")
        //                            RSContent.MoveNext()
        //                        Loop
        //                        Call RSContent.Close()
        //                    End If
        //                    RSContent = Nothing
        //                    RSTables.MoveNext()
        //                Loop
        //                Call RSTables.Close()
        //            End If
        //            RSTables = Nothing
        //            '
        //            ' Drop the field from the table
        //            '
        //            Call core.app.csv_DeleteTableField(DataSourceName, TableName, FieldName)
        //            '
        //            Exit Sub
        //            '
        ////ErrorTrap:
        //            Dim ex As New Exception("todo") : Call HandleClassError(ex, core.app.config.name, "deleteField") ' Err.Number, Err.Source, Err.Description, "DeleteField", True, False)
        //        End Sub
        //        '
        //        '   Returns TableID
        //        '       -1 if table not found
        //        '
        //        Private shared Function GetTableID( ByVal TableName As String) As Integer
        //            On Error GoTo ErrorTrap
        //            '
        //            Dim SQL As String
        //            dim dt as datatable
        //            '
        //            GetTableID = -1
        //            RS = core.app.ExecuteSQL( "Select ID from ccTables where name=" & encodeSQLText(TableName) & ";")
        //            If (isDataTableOk(rs)) Then
        //                If Not rs.rows.count=0 Then
        //                    GetTableID = core.app.getDataRowColumnName(RS.rows(0), "ID")
        //                End If
        //            End If
        //            '
        //            Exit Function
        //            '
        ////ErrorTrap:
        //            Dim ex As New Exception("todo") : Call HandleClassError(ex, core.app.config.name, "methodNameFPO") ' Err.Number, Err.Source, Err.Description, "GetTableID", True, False)
        //        End Function
        //        '
        //        '
        //        '
        //        Private shared Function core_group_add(ByVal GroupName As String) As Integer
        //            On Error GoTo ErrorTrap
        //            '
        //            Dim dt As DataTable
        //            Dim sql As String
        //            Dim createkey As Integer = genericController.GetRandomInteger(core)
        //            Dim cid As Integer
        //            '
        //            core_group_add = 0
        //            dt = core.app.executeSql("SELECT ID FROM CCGROUPS WHERE NAME='" & GroupName & "';")
        //            If dt.Rows.Count > 0 Then
        //                core_group_add = genericController.EncodeInteger(dt.Rows[0]["ID"))
        //            Else
        //                cid = GetContentID("groups")
        //                sql = "insert into ccgroups (contentcontrolid,active,createkey,name,caption) values (" & cid & ",1," & createkey & "," & EncodeSQLText(GroupName) & "," & EncodeSQLText(GroupName) & ")"
        //                Call core.app.executeSql(sql)
        //                sql = "select id from ccgroups where createkey=" & createkey & " order by id desc"
        //                dt = core.app.executeSql(sql)
        //                If dt.Rows.Count > 0 Then
        //                    core_group_add = genericController.EncodeInteger(dt.Rows[0]["id"))
        //                End If

        //            End If
        //            '
        //            Exit Function
        //            '
        ////ErrorTrap:
        //            Dim ex As New Exception("todo") : Call handleClassException(ex, core.app.config.name, "methodNameFPO") ' Err.Number, Err.Source, Err.Description, "AddGroup", True, False)
        //        End Function
        //
        //
        //
        private static void VerifyAdminMenus(coreController core, string DataBuildVersion) {
            try {
                DataTable dt = null;
                //
                // ----- remove duplicate menus that may have been added during faulty upgrades
                //
                string FieldNew = null;
                string FieldLast = null;
                int FieldRecordID = 0;
                //Dim dt As DataTable
                dt = core.db.executeQuery("Select ID,Name,ParentID from ccMenuEntries where (active<>0) Order By ParentID,Name");
                if (dt.Rows.Count > 0) {
                    FieldLast = "";
                    for (var rowptr = 0; rowptr < dt.Rows.Count; rowptr++) {
                        FieldNew = genericController.encodeText(dt.Rows[rowptr]["name"]) + "." + genericController.encodeText(dt.Rows[rowptr]["parentid"]);
                        if (FieldNew == FieldLast) {
                            FieldRecordID = genericController.encodeInteger(dt.Rows[rowptr]["ID"]);
                            core.db.executeQuery("Update ccMenuEntries set active=0 where ID=" + FieldRecordID + ";");
                        }
                        FieldLast = FieldNew;
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }
        //
        // Get the Menu for FormInputHTML
        //
        private static void VerifyRecord(coreController core, string ContentName, string Name, string CodeFieldName = "", string Code = "", bool InActive = false) {
            try {
                bool Active = false;
                DataTable dt = null;
                string sql1 = null;
                string sql2 = null;
                string sql3 = null;
                //
                Active = !InActive;
                Models.Complex.cdefModel cdef = Models.Complex.cdefModel.getCdef(core, ContentName);
                string tableName = cdef.contentTableName;
                int cid = cdef.id;
                //
                dt = core.db.executeQuery("SELECT ID FROM " + tableName + " WHERE NAME=" + core.db.encodeSQLText(Name) + ";");
                if (dt.Rows.Count == 0) {
                    sql1 = "insert into " + tableName + " (contentcontrolid,createkey,active,name";
                    sql2 = ") values (" + cid + ",0," + core.db.encodeSQLBoolean(Active) + "," + core.db.encodeSQLText(Name);
                    sql3 = ")";
                    if (!string.IsNullOrEmpty(CodeFieldName)) {
                        sql1 += "," + CodeFieldName;
                        sql2 += "," + Code;
                    }
                    core.db.executeQuery(sql1 + sql2 + sql3);
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }
        //
        //========================================================================
        // ----- Upgrade Conversion
        //========================================================================
        //
        private static void Upgrade_Conversion(coreController core, string DataBuildVersion) {
            try {
                //
                // -- Roll the style sheet cache if it is setup
                core.siteProperties.setProperty("StylesheetSerialNumber", (-1).ToString());
                //
                // -- Reload
                core.cache.invalidateAll();
                core.doc.clearMetaData();
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }
        //
        //=========================================================================================
        //
        //=========================================================================================
        //
        public static void VerifyTableCoreFields(coreController core) {
            try {
                //
                int IDVariant = 0;
                bool Active = false;
                string DataSourceName = null;
                string MethodName = null;
                string SQL = null;
                DataTable dt = null;
                int ptr = 0;
                //
                MethodName = "VerifyTableCoreFields";
                //
                appendUpgradeLogAddStep(core, core.appConfig.name, MethodName, "Verify core fields in all tables registered in [Tables] content.");
                //
                SQL = "SELECT ccDataSources.Name as DataSourceName, ccDataSources.ID as DataSourceID, ccDataSources.Active as DataSourceActive, ccTables.Name as TableName"
                + " FROM ccTables LEFT JOIN ccDataSources ON ccTables.DataSourceID = ccDataSources.ID"
                + " Where (((ccTables.active) <> 0))"
                + " ORDER BY ccDataSources.Name, ccTables.Name;";
                dt = core.db.executeQuery(SQL);
                ptr = 0;
                while (ptr < dt.Rows.Count) {
                    IDVariant = genericController.encodeInteger(dt.Rows[ptr]["DataSourceID"]);
                    if (IDVariant == 0) {
                        Active = true;
                        DataSourceName = "Default";
                    } else {
                        Active = genericController.encodeBoolean(dt.Rows[ptr]["DataSourceActive"]);
                        DataSourceName = genericController.encodeText(dt.Rows[ptr]["DataSourcename"]);
                    }
                    if (Active) {
                        core.db.createSQLTable(DataSourceName, genericController.encodeText(dt.Rows[ptr]["Tablename"]));
                    }
                    ptr += 1;
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }
        //
        //=========================================================================================
        //
        //=========================================================================================
        //
        public static void VerifyScriptingRecords(coreController core) {
            try {
                //
                appendUpgradeLogAddStep(core, core.appConfig.name, "VerifyScriptingRecords", "Verify Scripting Records.");
                //
                VerifyRecord(core, "Scripting Languages", "VBScript", "", "");
                VerifyRecord(core, "Scripting Languages", "JScript", "", "");
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }
        //
        //=========================================================================================
        //
        //=========================================================================================
        //
        public static void VerifyLanguageRecords(coreController core) {
            try {
                //
                appendUpgradeLogAddStep(core, core.appConfig.name, "VerifyLanguageRecords", "Verify Language Records.");
                //
                VerifyRecord(core, "Languages", "English", "HTTP_Accept_Language", "'en'");
                VerifyRecord(core, "Languages", "Spanish", "HTTP_Accept_Language", "'es'");
                VerifyRecord(core, "Languages", "French", "HTTP_Accept_Language", "'fr'");
                VerifyRecord(core, "Languages", "Any", "HTTP_Accept_Language", "'any'");
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }
        //
        //   Verify Library Folder records
        //
        private static void VerifyLibraryFolders(coreController core) {
            try {
                DataTable dt = null;
                //
                appendUpgradeLogAddStep(core, core.appConfig.name, "VerifyLibraryFolders", "Verify Library Folders: Images and Downloads");
                //
                dt = core.db.executeQuery("select id from cclibraryfiles");
                if (dt.Rows.Count == 0) {
                    VerifyRecord(core, "Library Folders", "Images");
                    VerifyRecord(core, "Library Folders", "Downloads");
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }
        //        '
        //        '   Verify ContentWatchLists
        //        '
        //        private shared sub VerifyContentWatchLists()
        //            On Error GoTo ErrorTrap
        //            '
        //            Dim CS As Integer
        //            Dim FieldName As String
        //            '
        //            Call appendUpgradeLogAddStep(core.app.config.name,"VerifyContentWatchLists", "Verify Content Watch Lists: What's New and What's Related")
        //            '
        //            If Not (False) Then
        //                CS = core.app.csOpen("Content Watch Lists", , "name", , , , , "ID")
        //                If Not core.app.csv_IsCSOK(CS) Then
        //                    Call VerifyRecord( "Content Watch Lists", "What's New", "Active", "1", True)
        //                    Call VerifyRecord( "Content Watch Lists", "What's Related", "Active", "1", True)
        //                End If
        //                Call core.app.csv_CloseCS(CS)
        //            End If
        //            '
        //            Exit Sub
        //            '
        //            ' ----- Error Trap
        //            '
        ////ErrorTrap:
        //            Dim ex As New Exception("todo") : Call HandleClassError(ex, core.app.config.name, "methodNameFPO") ' Err.Number, Err.Source, Err.Description, "VerifyContentWatchLists", True, True)
        //        End Sub
        //
        //=============================================================================
        //
        //=============================================================================
        //
        //Private shared Function VerifySurveyQuestionTypes() As Integer
        //    Dim returnType As Integer = 0
        //    Try
        //        '
        //        Dim rs As DataTable
        //        Dim RowsFound As Integer
        //        Dim CID As Integer
        //        Dim TableBad As Boolean
        //        Dim RowsNeeded As Integer
        //        '
        //        Call appendUpgradeLogAddStep(core.app.config.name, "VerifySurveyQuestionTypes", "Verify Survey Question Types")
        //        '
        //        ' ----- make sure there are enough records
        //        '
        //        TableBad = False
        //        RowsFound = 0
        //        rs = core.app.executeSql("Select ID from ccSurveyQuestionTypes order by id")
        //        If (Not isDataTableOk(rs)) Then
        //            '
        //            ' problem
        //            '
        //            TableBad = True
        //        Else
        //            '
        //            ' Verify the records that are there
        //            '
        //            RowsFound = 0
        //            For Each rsDr As DataRow In rs.Rows
        //                RowsFound = RowsFound + 1
        //                If RowsFound <> genericController.EncodeInteger(rsDr("ID")) Then
        //                    '
        //                    ' Bad Table
        //                    '
        //                    TableBad = True
        //                    Exit For
        //                End If
        //            Next
        //        End If
        //        rs.Dispose()
        //        '
        //        ' ----- Replace table if needed
        //        '
        //        If TableBad Then
        //            Call core.app.csv_DeleteTable("Default", "ccSurveyQuestionTypes")
        //            'Call core.app.ExecuteSQL( "Drop table ccSurveyQuestionTypes")
        //            Call core.app.CreateSQLTable("Default", "ccSurveyQuestionTypes")
        //            RowsFound = 0
        //        End If
        //        '
        //        ' ----- Add the number of rows needed
        //        '
        //        RowsNeeded = 3 - RowsFound
        //        If RowsNeeded > 0 Then
        //            CID = core.app.csv_GetContentID("Survey Question Types")
        //            If CID <= 0 Then
        //                '
        //                ' Problem
        //                '
        //                fixme-- core.handleException(New ApplicationException("")) ' -----ignoreInteger, "dll", "Survey Question Types content definition was not found")
        //            Else
        //                Do While RowsNeeded > 0
        //                    Call core.app.executeSql("Insert into ccSurveyQuestionTypes (active,contentcontrolid)values(1," & CID & ")")
        //                    RowsNeeded = RowsNeeded - 1
        //                Loop
        //            End If
        //        End If
        //        '
        //        ' ----- Update the Names of each row
        //        '
        //        Call core.app.executeSql("Update ccSurveyQuestionTypes Set Name='Text Field' where ID=1;")
        //        Call core.app.executeSql("Update ccSurveyQuestionTypes Set Name='Select Dropdown' where ID=2;")
        //        Call core.app.executeSql("Update ccSurveyQuestionTypes Set Name='Radio Buttons' where ID=3;")
        //    Catch ex As Exception
        //        core.handleException(ex);
        //    End Try
        //    Return returnType
        //End Function
        //        '
        //        '=============================================================================
        //        '
        //        '=============================================================================
        //        '
        //        Private shared Function DeleteAdminMenu(ByVal MenuName As String, ByVal ParentMenuName As String) As Integer
        //            Dim returnAttr As String = ""
        //            Try

        //            Catch ex As Exception
        //                core.handleException(ex);
        //            End Try
        //            Return returnAttr

        //            On Error GoTo ErrorTrap
        //            '
        //            Dim ParentID As Integer
        //            '
        //            If ParentMenuName <> "" Then
        //                ParentID = GetIDBYName("ccMenuEntries", ParentMenuName)
        //                Call core.app.executeSql("Delete from ccMenuEntries where name=" & EncodeSQLText(MenuName) & " and ParentID=" & ParentID)
        //            Else
        //                Call core.app.executeSql("Delete from ccMenuEntries where name=" & EncodeSQLText(MenuName))
        //            End If
        //            '
        //            Exit Function
        //            '
        //            ' ----- Error Trap
        //            '
        ////ErrorTrap:
        //            Dim ex As New Exception("todo") : Call handleClassException(ex, core.app.config.name, "methodNameFPO") ' Err.Number, Err.Source, Err.Description, "DeleteAdminMenu", True, False)
        //        End Function
        //
        //=============================================================================
        //
        //=============================================================================
        //
        private static int GetIDBYName(coreController core, string TableName, string RecordName) {
            int tempGetIDBYName = 0;
            int returnid = 0;
            try {
                //
                DataTable rs;
                //
                rs = core.db.executeQuery("Select ID from " + TableName + " where name=" + core.db.encodeSQLText(RecordName));
                if (isDataTableOk(rs)) {
                    tempGetIDBYName = genericController.encodeInteger(rs.Rows[0]["ID"]);
                }
                rs.Dispose();
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return returnid;
        }
        //
        //   Verify Library Folder records
        //
        private static void VerifyLibraryFileTypes(coreController core) {
            try {
                //
                // Load basic records -- default images are handled in the REsource Library through the /ccLib/config/DefaultValues.txt GetDefaultValue(key) mechanism
                //
                if (core.db.getRecordID("Library File Types", "Image") == 0) {
                    VerifyRecord(core, "Library File Types", "Image", "ExtensionList", "'GIF,JPG,JPE,JPEG,BMP,PNG'", false);
                    VerifyRecord(core, "Library File Types", "Image", "IsImage", "1", false);
                    VerifyRecord(core, "Library File Types", "Image", "IsVideo", "0", false);
                    VerifyRecord(core, "Library File Types", "Image", "IsDownload", "1", false);
                    VerifyRecord(core, "Library File Types", "Image", "IsFlash", "0", false);
                }
                //
                if (core.db.getRecordID("Library File Types", "Video") == 0) {
                    VerifyRecord(core, "Library File Types", "Video", "ExtensionList", "'ASX,AVI,WMV,MOV,MPG,MPEG,MP4,QT,RM'", false);
                    VerifyRecord(core, "Library File Types", "Video", "IsImage", "0", false);
                    VerifyRecord(core, "Library File Types", "Video", "IsVideo", "1", false);
                    VerifyRecord(core, "Library File Types", "Video", "IsDownload", "1", false);
                    VerifyRecord(core, "Library File Types", "Video", "IsFlash", "0", false);
                }
                //
                if (core.db.getRecordID("Library File Types", "Audio") == 0) {
                    VerifyRecord(core, "Library File Types", "Audio", "ExtensionList", "'AIF,AIFF,ASF,CDA,M4A,M4P,MP2,MP3,MPA,WAV,WMA'", false);
                    VerifyRecord(core, "Library File Types", "Audio", "IsImage", "0", false);
                    VerifyRecord(core, "Library File Types", "Audio", "IsVideo", "0", false);
                    VerifyRecord(core, "Library File Types", "Audio", "IsDownload", "1", false);
                    VerifyRecord(core, "Library File Types", "Audio", "IsFlash", "0", false);
                }
                //
                if (core.db.getRecordID("Library File Types", "Word") == 0) {
                    VerifyRecord(core, "Library File Types", "Word", "ExtensionList", "'DOC'", false);
                    VerifyRecord(core, "Library File Types", "Word", "IsImage", "0", false);
                    VerifyRecord(core, "Library File Types", "Word", "IsVideo", "0", false);
                    VerifyRecord(core, "Library File Types", "Word", "IsDownload", "1", false);
                    VerifyRecord(core, "Library File Types", "Word", "IsFlash", "0", false);
                }
                //
                if (core.db.getRecordID("Library File Types", "Flash") == 0) {
                    VerifyRecord(core, "Library File Types", "Flash", "ExtensionList", "'SWF'", false);
                    VerifyRecord(core, "Library File Types", "Flash", "IsImage", "0", false);
                    VerifyRecord(core, "Library File Types", "Flash", "IsVideo", "0", false);
                    VerifyRecord(core, "Library File Types", "Flash", "IsDownload", "1", false);
                    VerifyRecord(core, "Library File Types", "Flash", "IsFlash", "1", false);
                }
                //
                if (core.db.getRecordID("Library File Types", "PDF") == 0) {
                    VerifyRecord(core, "Library File Types", "PDF", "ExtensionList", "'PDF'", false);
                    VerifyRecord(core, "Library File Types", "PDF", "IsImage", "0", false);
                    VerifyRecord(core, "Library File Types", "PDF", "IsVideo", "0", false);
                    VerifyRecord(core, "Library File Types", "PDF", "IsDownload", "1", false);
                    VerifyRecord(core, "Library File Types", "PDF", "IsFlash", "0", false);
                }
                //
                if (core.db.getRecordID("Library File Types", "XLS") == 0) {
                    VerifyRecord(core, "Library File Types", "Excel", "ExtensionList", "'XLS'", false);
                    VerifyRecord(core, "Library File Types", "Excel", "IsImage", "0", false);
                    VerifyRecord(core, "Library File Types", "Excel", "IsVideo", "0", false);
                    VerifyRecord(core, "Library File Types", "Excel", "IsDownload", "1", false);
                    VerifyRecord(core, "Library File Types", "Excel", "IsFlash", "0", false);
                }
                //
                if (core.db.getRecordID("Library File Types", "PPT") == 0) {
                    VerifyRecord(core, "Library File Types", "Power Point", "ExtensionList", "'PPT,PPS'", false);
                    VerifyRecord(core, "Library File Types", "Power Point", "IsImage", "0", false);
                    VerifyRecord(core, "Library File Types", "Power Point", "IsVideo", "0", false);
                    VerifyRecord(core, "Library File Types", "Power Point", "IsDownload", "1", false);
                    VerifyRecord(core, "Library File Types", "Power Point", "IsFlash", "0", false);
                }
                //
                if (core.db.getRecordID("Library File Types", "Default") == 0) {
                    VerifyRecord(core, "Library File Types", "Default", "ExtensionList", "''", false);
                    VerifyRecord(core, "Library File Types", "Default", "IsImage", "0", false);
                    VerifyRecord(core, "Library File Types", "Default", "IsVideo", "0", false);
                    VerifyRecord(core, "Library File Types", "Default", "IsDownload", "1", false);
                    VerifyRecord(core, "Library File Types", "Default", "IsFlash", "0", false);
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }
        //
        //=========================================================================================
        //
        //=========================================================================================
        //
        private static void VerifyState(coreController core, string Name, string Abbreviation, double SaleTax, int CountryID, string FIPSState) {
            try {
                //
                int CS = 0;
                const string ContentName = "States";
                //
                CS = core.db.csOpen(ContentName, "name=" + core.db.encodeSQLText(Name),"", false);
                if (!core.db.csOk(CS)) {
                    //
                    // create new record
                    //
                    core.db.csClose(ref CS);
                    CS = core.db.csInsertRecord(ContentName, SystemMemberID);
                    core.db.csSet(CS, "NAME", Name);
                    core.db.csSet(CS, "ACTIVE", true);
                    core.db.csSet(CS, "Abbreviation", Abbreviation);
                    core.db.csSet(CS, "CountryID", CountryID);
                    core.db.csSet(CS, "FIPSState", FIPSState);
                } else {
                    //
                    // verify only fields needed for contensive
                    //
                    core.db.csSet(CS, "CountryID", CountryID);
                    core.db.csSet(CS, "Abbreviation", Abbreviation);
                }
                core.db.csClose(ref CS);
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }
        //
        //=========================================================================================
        //
        //=========================================================================================
        //
        public static void VerifyStates(coreController core) {
            try {
                //
                appendUpgradeLogAddStep(core, core.appConfig.name, "VerifyStates", "Verify States");
                //
                int CountryID = 0;
                //
                VerifyCountry(core, "United States", "US");
                CountryID = core.db.getRecordID("Countries", "United States");
                //
                VerifyState(core, "Alaska", "AK", 0.0D, CountryID, "");
                VerifyState(core, "Alabama", "AL", 0.0D, CountryID, "");
                VerifyState(core, "Arizona", "AZ", 0.0D, CountryID, "");
                VerifyState(core, "Arkansas", "AR", 0.0D, CountryID, "");
                VerifyState(core, "California", "CA", 0.0D, CountryID, "");
                VerifyState(core, "Connecticut", "CT", 0.0D, CountryID, "");
                VerifyState(core, "Colorado", "CO", 0.0D, CountryID, "");
                VerifyState(core, "Delaware", "DE", 0.0D, CountryID, "");
                VerifyState(core, "District of Columbia", "DC", 0.0D, CountryID, "");
                VerifyState(core, "Florida", "FL", 0.0D, CountryID, "");
                VerifyState(core, "Georgia", "GA", 0.0D, CountryID, "");

                VerifyState(core, "Hawaii", "HI", 0.0D, CountryID, "");
                VerifyState(core, "Idaho", "ID", 0.0D, CountryID, "");
                VerifyState(core, "Illinois", "IL", 0.0D, CountryID, "");
                VerifyState(core, "Indiana", "IN", 0.0D, CountryID, "");
                VerifyState(core, "Iowa", "IA", 0.0D, CountryID, "");
                VerifyState(core, "Kansas", "KS", 0.0D, CountryID, "");
                VerifyState(core, "Kentucky", "KY", 0.0D, CountryID, "");
                VerifyState(core, "Louisiana", "LA", 0.0D, CountryID, "");
                VerifyState(core, "Massachusetts", "MA", 0.0D, CountryID, "");
                VerifyState(core, "Maine", "ME", 0.0D, CountryID, "");

                VerifyState(core, "Maryland", "MD", 0.0D, CountryID, "");
                VerifyState(core, "Michigan", "MI", 0.0D, CountryID, "");
                VerifyState(core, "Minnesota", "MN", 0.0D, CountryID, "");
                VerifyState(core, "Missouri", "MO", 0.0D, CountryID, "");
                VerifyState(core, "Mississippi", "MS", 0.0D, CountryID, "");
                VerifyState(core, "Montana", "MT", 0.0D, CountryID, "");
                VerifyState(core, "North Carolina", "NC", 0.0D, CountryID, "");
                VerifyState(core, "Nebraska", "NE", 0.0D, CountryID, "");
                VerifyState(core, "New Hampshire", "NH", 0.0D, CountryID, "");
                VerifyState(core, "New Mexico", "NM", 0.0D, CountryID, "");

                VerifyState(core, "New Jersey", "NJ", 0.0D, CountryID, "");
                VerifyState(core, "New York", "NY", 0.0D, CountryID, "");
                VerifyState(core, "Nevada", "NV", 0.0D, CountryID, "");
                VerifyState(core, "North Dakota", "ND", 0.0D, CountryID, "");
                VerifyState(core, "Ohio", "OH", 0.0D, CountryID, "");
                VerifyState(core, "Oklahoma", "OK", 0.0D, CountryID, "");
                VerifyState(core, "Oregon", "OR", 0.0D, CountryID, "");
                VerifyState(core, "Pennsylvania", "PA", 0.0D, CountryID, "");
                VerifyState(core, "Rhode Island", "RI", 0.0D, CountryID, "");
                VerifyState(core, "South Carolina", "SC", 0.0D, CountryID, "");

                VerifyState(core, "South Dakota", "SD", 0.0D, CountryID, "");
                VerifyState(core, "Tennessee", "TN", 0.0D, CountryID, "");
                VerifyState(core, "Texas", "TX", 0.0D, CountryID, "");
                VerifyState(core, "Utah", "UT", 0.0D, CountryID, "");
                VerifyState(core, "Vermont", "VT", 0.0D, CountryID, "");
                VerifyState(core, "Virginia", "VA", 0.045, CountryID, "");
                VerifyState(core, "Washington", "WA", 0.0D, CountryID, "");
                VerifyState(core, "Wisconsin", "WI", 0.0D, CountryID, "");
                VerifyState(core, "West Virginia", "WV", 0.0D, CountryID, "");
                VerifyState(core, "Wyoming", "WY", 0.0D, CountryID, "");
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }
        //
        // Get the Menu for FormInputHTML
        //
        private static void VerifyCountry(coreController core, string Name, string Abbreviation) {
            try {
                int CS;
                //
                CS = core.db.csOpen("Countries", "name=" + core.db.encodeSQLText(Name));
                if (!core.db.csOk(CS)) {
                    core.db.csClose(ref CS);
                    CS = core.db.csInsertRecord("Countries", SystemMemberID);
                    if (core.db.csOk(CS)) {
                        core.db.csSet(CS, "ACTIVE", true);
                    }
                }
                if (core.db.csOk(CS)) {
                    core.db.csSet(CS, "NAME", Name);
                    core.db.csSet(CS, "Abbreviation", Abbreviation);
                    if (genericController.vbLCase(Name) == "united states") {
                        core.db.csSet(CS, "DomesticShipping", "1");
                    }
                }
                core.db.csClose(ref CS);
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }
        //
        //=========================================================================================
        //
        //=========================================================================================
        //
        public static void VerifyCountries(coreController core) {
            try {
                //
                appendUpgradeLogAddStep(core, core.appConfig.name, "VerifyCountries", "Verify Countries");
                //
                string list = core.appRootFiles.readFileText("cclib\\config\\isoCountryList.txt");
                string[] rows  = genericController.stringSplit(list, "\r\n");
                foreach( var row in rows) {
                    if (!string.IsNullOrEmpty(row)) {
                        string[] attrs = row.Split(';');
                        foreach (var attr in attrs) {
                            VerifyCountry(core, EncodeInitialCaps(attr), attrs[1]);
                        }
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }
        //
        //=========================================================================================
        //
        //=========================================================================================
        //
        public static void VerifyDefaultGroups(coreController core) {
            try {
                //
                int GroupID = 0;
                string SQL = null;
                //
                appendUpgradeLogAddStep(core, core.appConfig.name, "VerifyDefaultGroups", "Verify Default Groups");
                //
                GroupID = groupController.group_add(core, "Site Managers");
                SQL = "Update ccContent Set EditorGroupID=" + core.db.encodeSQLNumber(GroupID) + " where EditorGroupID is null;";
                core.db.executeQuery(SQL);
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }
        //
        //=========================================================================================
        //
        //=========================================================================================
        //
        //Public shared Sub ImportCDefFolder()
        //    Try
        //        '
        //        Dim runAtServer As runAtServerClass
        //        Dim FilePath As String
        //        Dim CollectionWorking As CollectionClass
        //        Dim IISResetRequired As Boolean
        //        '
        //        ' Now Create / Modify Db based on all CDef records that are 'CDefChanged'
        //        '
        //        Call UpgradeCDef_BuildDbFromCollection(CollectionWorking, IISResetRequired, core.app.DataBuildVersion_DontUseThis)
        //        '
        //        ' IISReset if needed
        //        '
        //        If IISResetRequired Then
        //            '
        //            ' Restart IIS if stopped
        //            '
        //            runAtServer = New runAtServerClass(core)
        //            Call runAtServer.executeCmd("IISReset", "")
        //        End If
        //        '
        //        ' Clear the CDef folder
        //        '
        //        Call core.app.publicFiles.DeleteFileFolder(FilePath)
        //        Call core.app.publicFiles.createPath(FilePath)
        //    Catch ex As Exception
        //        core.handleException(ex);
        //    End Try
        //End Sub
        //
        //
        //
        //Public shared Sub SetNavigatorEntry(EntryName As String, ParentName As String, AddonID As Integer)
        //    On Error GoTo ErrorTrap
        //    '
        //    Dim CS As Integer
        //    Dim ParentID As Integer
        //    '
        //    If ParentName <> "" Then
        //        CS = core.app.csOpen(cnNavigatorEntries, "name=" & encodeSQLText(ParentName), "ID", , , , , "ID")
        //        If Not core.app.csv_IsCSOK(CS) Then
        //            Call core.app.csv_CloseCS(CS)
        //            CS = core.app.csv_InsertCSRecord(cnNavigatorEntries)
        //        End If
        //        If core.app.csv_IsCSOK(CS) Then
        //            ParentID = core.app.csv_cs_getInteger(CS, "ID")
        //        End If
        //    End If
        //    CS = core.app.csOpen(cnNavigatorEntries, "name=" & encodeSQLText(EntryName), "ID", , , , , "ID,Name,ParentID,AddonID")
        //    If Not core.app.csv_IsCSOK(CS) Then
        //        Call core.app.csv_CloseCS(CS)
        //        CS = core.app.csv_InsertCSRecord(cnNavigatorEntries)
        //    End If
        //    If core.app.csv_IsCSOK(CS) Then
        //        Call core.app.csv_SetCS(CS, "Name", EntryName)
        //        Call core.app.csv_SetCS(CS, "ParentID", ParentID)
        //        Call core.app.csv_SetCS(CS, "AddonID", AddonID)
        //    End If
        //    Call core.app.csv_CloseCS(CS)
        //    '
        //    Exit Sub
        ////ErrorTrap:
        //    dim ex as new exception("todo"): Call HandleClassError(ex,core.app.appEnvironment.name,Err.Number, Err.Source, Err.Description, "SetNavigatorEntry", True, False)
        //End Sub
        //
        //
        //
        //Public shared Sub SetNavigatorEntry2(EntryName As String, ParentGuid As String, AddonID As Integer, NavIconTypeID As Integer, NavIconTitle As String, DataBuildVersion As String)
        //    On Error GoTo ErrorTrap
        //    '
        //    Dim CS As Integer
        //    Dim ParentID As Integer
        //    '
        //    If ParentGuid <> "" Then
        //        CS = core.app.csOpen(cnNavigatorEntries, "NavGuid=" & encodeSQLText(ParentGuid), "ID", , , , , "ID")
        //        If core.app.csv_IsCSOK(CS) Then
        //            ParentID = core.app.csv_cs_getInteger(CS, "ID")
        //        End If
        //        Call core.app.csv_CloseCS(CS)
        //    End If
        //    If ParentID > 0 Then
        //        CS = core.app.csOpen(cnNavigatorEntries, "(parentid=" & ParentID & ")and(name=" & encodeSQLText(EntryName) & ")", "ID", , , , , "ID,Name,ParentID,AddonID")
        //        If Not core.app.csv_IsCSOK(CS) Then
        //            Call core.app.csv_CloseCS(CS)
        //            CS = core.app.csv_InsertCSRecord(cnNavigatorEntries)
        //        End If
        //        If core.app.csv_IsCSOK(CS) Then
        //            Call core.app.csv_SetCS(CS, "Name", EntryName)
        //            Call core.app.csv_SetCS(CS, "ParentID", ParentID)
        //            Call core.app.csv_SetCS(CS, "AddonID", AddonID)
        //            If true Then
        //                Call core.app.csv_SetCS(CS, "NavIconTypeID", NavIconTypeID)
        //                Call core.app.csv_SetCS(CS, "NavIconTitle", NavIconTitle)
        //            End If
        //        End If
        //        Call core.app.csv_CloseCS(CS)
        //    End If
        //    '
        //    Exit Sub
        ////ErrorTrap:
        //    dim ex as new exception("todo"): Call HandleClassError(ex,core.app.appEnvironment.name,Err.Number, Err.Source, Err.Description, "SetNavigatorEntry", True, False)
        //End Sub
        //
        //======================================================================================================
        //   Installs Addons in the content folder install subfolder
        //======================================================================================================
        // REFACTOR - \INSTALL FOLDER IS NOT THE CURRENT PATTERN
        //Public Function InstallAddons(IsNewBuild As Boolean, buildVersion As String) As Boolean
        //    Dim returnOk As Boolean = True
        //    Try
        //        '
        //        Dim IISResetRequired As Boolean
        //        Dim runAtServer As runAtServerClass
        //        Dim addonInstall As New addonInstallClass(core)
        //        Dim saveLogFolder As String
        //        '
        //        InstallAddons = False
        //        '
        //        saveLogFolder = classLogFolder
        //        InstallAddons = addonInstall.InstallCollectionFromPrivateFolder(Me, buildVersion, "Install\", IISResetRequired, core.app.config.name, "", "", IsNewBuild)
        //        classLogFolder = saveLogFolder
        //        '
        //        ' IISReset if needed
        //        '
        //        If IISResetRequired Then
        //            runAtServer = New runAtServerClass(core)
        //            Call runAtServer.executeCmd("IISReset", "")
        //        End If
        //    Catch ex As Exception
        //        core.handleException(ex);
        //    End Try
        //    Return returnOk
        //End Function
        //
        //
        ///
        //Public Shared Sub ReplaceAddonWithCollection(ByVal AddonProgramID As String, ByVal CollectionGuid As String, ByRef return_IISResetRequired As Boolean, ByRef return_RegisterList As String)
        //    Dim ex As New Exception("todo") : Call handleClassException(core, ex, core.appConfig.name, "methodNameFPO") ' ignoreInteger, "dll", "builderClass.ReplaceAddonWithCollection is deprecated", "ReplaceAddonWithCollection", True, True)
        //End Sub
        //    On Error GoTo ErrorTrap
        //    '
        //    Dim CS As Integer
        //    Dim ErrorMessage As String
        //    Dim addonInstall As addonInstallClass
        //    '
        //    CS = core.app.csOpen(cnAddons, "objectProgramID=" & encodeSQLText(AddonProgramID))
        //    If core.app.csv_IsCSOK(CS) Then
        //        Call core.app.csv_DeleteCSRecord(CS)
        //        InstallCollectionList = InstallCollectionList & "," & CollectionGuid
        //        'Set AddonInstall = New AddonInstallClass
        //        'If Not AddonInstall.UpgradeAllAppsFromLibCollection2(CollectionGuid, core.app.appEnvironment.name, Return_IISResetRequired, Return_RegisterList, ErrorMessage) Then
        //        'End If
        //    End If
        //    Call core.app.csv_CloseCS(CS)
        //    '
        //    Exit Sub
        //    '
        //    ' ----- Error Trap
        //    '
        ////ErrorTrap:
        //    dim ex as new exception("todo"): Call HandleClassError(ex,core.app.appEnvironment.name, Err.Number, Err.Source, Err.Description, "ReplaceAddonWithCollection", True, True)
        //    Err.Clear
        //End Sub
        //
        //===================================================================================================================
        //   Verify all the core tables
        //       Moved to Sub because if an Addon upgrade from another site on the server distributes the
        //       CDef changes, this site could have to update it's ccContent and ccField records, and
        //       it will fail if they are not up to date.
        //===================================================================================================================
        //
        internal static void VerifyBasicTables(coreController core) {
            try {
                //
                if (!false) {
                    appendUpgradeLogAddStep(core, core.appConfig.name, "VerifyCoreTables", "Verify Core SQL Tables");
                    //
                    core.db.createSQLTable("Default", "ccDataSources");
                    core.db.createSQLTableField("Default", "ccDataSources", "typeId", FieldTypeIdInteger);
                    core.db.createSQLTableField("Default", "ccDataSources", "address", FieldTypeIdText);
                    core.db.createSQLTableField("Default", "ccDataSources", "username", FieldTypeIdText);
                    core.db.createSQLTableField("Default", "ccDataSources", "password", FieldTypeIdText);
                    core.db.createSQLTableField("Default", "ccDataSources", "ConnString", FieldTypeIdText);
                    core.db.createSQLTableField("Default", "ccDataSources", "endpoint", FieldTypeIdText);
                    core.db.createSQLTableField("Default", "ccDataSources", "dbtypeid", FieldTypeIdLookup);
                    //
                    core.db.createSQLTable("Default", "ccTables");
                    core.db.createSQLTableField("Default", "ccTables", "DataSourceID", FieldTypeIdLookup);
                    //
                    core.db.createSQLTable("Default", "ccContent");
                    core.db.createSQLTableField("Default", "ccContent", "ContentTableID", FieldTypeIdInteger);
                    core.db.createSQLTableField("Default", "ccContent", "AuthoringTableID", FieldTypeIdInteger);
                    core.db.createSQLTableField("Default", "ccContent", "AllowAdd", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccContent", "AllowDelete", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccContent", "AllowWorkflowAuthoring", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccContent", "DeveloperOnly", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccContent", "AdminOnly", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccContent", "ParentID", FieldTypeIdInteger);
                    core.db.createSQLTableField("Default", "ccContent", "DefaultSortMethodID", FieldTypeIdInteger);
                    core.db.createSQLTableField("Default", "ccContent", "DropDownFieldList", FieldTypeIdText);
                    core.db.createSQLTableField("Default", "ccContent", "EditorGroupID", FieldTypeIdInteger);
                    core.db.createSQLTableField("Default", "ccContent", "AllowCalendarEvents", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccContent", "AllowContentTracking", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccContent", "AllowTopicRules", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccContent", "AllowContentChildTool", FieldTypeIdBoolean);
                    //Call core.db.createSQLTableField("Default", "ccContent", "AllowMetaContent", FieldTypeIdBoolean)
                    core.db.createSQLTableField("Default", "ccContent", "IconLink", FieldTypeIdLink);
                    core.db.createSQLTableField("Default", "ccContent", "IconHeight", FieldTypeIdInteger);
                    core.db.createSQLTableField("Default", "ccContent", "IconWidth", FieldTypeIdInteger);
                    core.db.createSQLTableField("Default", "ccContent", "IconSprites", FieldTypeIdInteger);
                    core.db.createSQLTableField("Default", "ccContent", "installedByCollectionId", FieldTypeIdInteger);
                    //Call core.app.csv_CreateSQLTableField("Default", "ccContent", "ccGuid", FieldTypeText)
                    core.db.createSQLTableField("Default", "ccContent", "IsBaseContent", FieldTypeIdBoolean);
                    //Call core.app.csv_CreateSQLTableField("Default", "ccContent", "WhereClause", FieldTypeText)
                    //
                    core.db.createSQLTable("Default", "ccFields");
                    core.db.createSQLTableField("Default", "ccFields", "ContentID", FieldTypeIdInteger);
                    core.db.createSQLTableField("Default", "ccFields", "Type", FieldTypeIdInteger);
                    core.db.createSQLTableField("Default", "ccFields", "Caption", FieldTypeIdText);
                    core.db.createSQLTableField("Default", "ccFields", "ReadOnly", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccFields", "NotEditable", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccFields", "LookupContentID", FieldTypeIdInteger);
                    core.db.createSQLTableField("Default", "ccFields", "RedirectContentID", FieldTypeIdInteger);
                    core.db.createSQLTableField("Default", "ccFields", "RedirectPath", FieldTypeIdText);
                    core.db.createSQLTableField("Default", "ccFields", "RedirectID", FieldTypeIdText);
                    core.db.createSQLTableField("Default", "ccFields", "HelpMessage", FieldTypeIdLongText); // deprecated but Im chicken to remove this
                    core.db.createSQLTableField("Default", "ccFields", "UniqueName", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccFields", "TextBuffered", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccFields", "Password", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccFields", "IndexColumn", FieldTypeIdInteger);
                    core.db.createSQLTableField("Default", "ccFields", "IndexWidth", FieldTypeIdText);
                    core.db.createSQLTableField("Default", "ccFields", "IndexSortPriority", FieldTypeIdInteger);
                    core.db.createSQLTableField("Default", "ccFields", "IndexSortDirection", FieldTypeIdInteger);
                    core.db.createSQLTableField("Default", "ccFields", "EditSortPriority", FieldTypeIdInteger);
                    core.db.createSQLTableField("Default", "ccFields", "AdminOnly", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccFields", "DeveloperOnly", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccFields", "DefaultValue", FieldTypeIdText);
                    core.db.createSQLTableField("Default", "ccFields", "Required", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccFields", "HTMLContent", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccFields", "Authorable", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccFields", "ManyToManyContentID", FieldTypeIdInteger);
                    core.db.createSQLTableField("Default", "ccFields", "ManyToManyRuleContentID", FieldTypeIdInteger);
                    core.db.createSQLTableField("Default", "ccFields", "ManyToManyRulePrimaryField", FieldTypeIdText);
                    core.db.createSQLTableField("Default", "ccFields", "ManyToManyRuleSecondaryField", FieldTypeIdText);
                    core.db.createSQLTableField("Default", "ccFields", "RSSTitleField", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccFields", "RSSDescriptionField", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccFields", "MemberSelectGroupID", FieldTypeIdInteger);
                    core.db.createSQLTableField("Default", "ccFields", "EditTab", FieldTypeIdText);
                    core.db.createSQLTableField("Default", "ccFields", "Scramble", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccFields", "LookupList", FieldTypeIdText);
                    core.db.createSQLTableField("Default", "ccFields", "IsBaseField", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccFields", "installedByCollectionId", FieldTypeIdInteger);
                    //
                    core.db.createSQLTable("Default", "ccFieldHelp");
                    core.db.createSQLTableField("Default", "ccFieldHelp", "FieldID", FieldTypeIdLookup);
                    core.db.createSQLTableField("Default", "ccFieldHelp", "HelpDefault", FieldTypeIdLongText);
                    core.db.createSQLTableField("Default", "ccFieldHelp", "HelpCustom", FieldTypeIdLongText);
                    //
                    core.db.createSQLTable("Default", "ccSetup");
                    core.db.createSQLTableField("Default", "ccSetup", "FieldValue", FieldTypeIdText);
                    core.db.createSQLTableField("Default", "ccSetup", "DeveloperOnly", FieldTypeIdBoolean);
                    //
                    core.db.createSQLTable("Default", "ccSortMethods");
                    core.db.createSQLTableField("Default", "ccSortMethods", "OrderByClause", FieldTypeIdText);
                    //
                    core.db.createSQLTable("Default", "ccFieldTypes");
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }

        //
        //===========================================================================
        //   Error handler
        //===========================================================================
        //
        private static void handleClassException(coreController core, Exception ex, string ApplicationName, string MethodName) {
            logController.appendLog(core, "exception in builderClass." + MethodName + ", application [" + ApplicationName + "], ex [" + ex.ToString() + "]");
        }
        //
        //===========================================================================
        //   Append Log File
        //===========================================================================
        //
        private static void appendUpgradeLog(coreController core, string appName, string Method, string Message) {
            logController.appendLogInstall(core, "app [" + appName + "], Method [" + Method + "], Message [" + Message + "]");
        }
        //
        //=============================================================================
        //   Get a ContentID from the ContentName using just the tables
        //=============================================================================
        //
        private static void appendUpgradeLogAddStep(coreController core, string appName, string Method, string Message) {
            appendUpgradeLog(core, appName, Method, Message);
        }
        //
        //=====================================================================================================
        //   a value in a name/value pair
        //=====================================================================================================
        //
        public static void SetNameValueArrays(coreController core, string InputName, string InputValue, ref string[] SQLName, ref string[] SQLValue, ref int Index) {
            // ##### removed to catch err<>0 problem //On Error //Resume Next
            //
            SQLName[Index] = InputName;
            SQLValue[Index] = InputValue;
            Index = Index + 1;
            //
        }
        //
        //=============================================================================
        //
        //=============================================================================
        //
        //Public shared Sub csv_VerifyAggregateFunction(ByVal Name As String, ByVal Link As String, ByVal ObjectProgramID As String, ByVal ArgumentList As String, ByVal SortOrder As String)
        //    Try
        //        '
        //        ' Determine Function or Object based on Link
        //        '
        //        If Link <> "" Then
        //            Call csv_VerifyAggregateScript(Name, Link, ArgumentList, SortOrder)
        //        ElseIf ObjectProgramID <> "" Then
        //            Call csv_VerifyAggregateObject(Name, ObjectProgramID, ArgumentList, SortOrder)
        //        Else
        //            Call csv_VerifyAggregateReplacement2(Name, "", ArgumentList, SortOrder)
        //        End If
        //    Catch ex As Exception
        //        core.handleException(ex);
        //    End Try
        //End Sub '

        // -- deprecated
        //Public Shared Sub admin_VerifyMenuEntry(core As coreClass, ByVal ParentName As String, ByVal EntryName As String, ByVal ContentName As String, ByVal LinkPage As String, ByVal SortOrder As String, ByVal AdminOnly As Boolean, ByVal DeveloperOnly As Boolean, ByVal NewWindow As Boolean, ByVal Active As Boolean, ByVal MenuContentName As String, ByVal AddonName As String)
        //    Try
        //        '
        //        Const AddonContentName = "Aggregate Functions"
        //        '
        //        Dim SelectList As String
        //        Dim CSEntry As Integer
        //        Dim ContentID As Integer
        //        Dim ParentID As Integer
        //        Dim addonId As Integer
        //        Dim CS As Integer
        //        Dim SupportAddonID As Boolean
        //        '
        //        SelectList = "Name,ContentID,ParentID,LinkPage,SortOrder,AdminOnly,DeveloperOnly,NewWindow,Active"
        //        SupportAddonID = Models.Complex.cdefModel.isContentFieldSupported(core,MenuContentName, "AddonID")
        //        '
        //        ' Get AddonID from AddonName
        //        '
        //        addonId = 0
        //        If Not SupportAddonID Then
        //            SelectList = SelectList & ",0 as AddonID"
        //        Else
        //            SelectList = SelectList & ",AddonID"
        //            If AddonName <> "" Then
        //                CS = core.db.cs_open(AddonContentName, "name=" & core.db.encodeSQLText(AddonName), "ID", False, , , , "ID", 1)
        //                If core.db.cs_ok(CS) Then
        //                    addonId = (core.db.cs_getInteger(CS, "ID"))
        //                End If
        //                Call core.db.cs_Close(CS)
        //            End If
        //        End If
        //        '
        //        ' Get ParentID from ParentName
        //        '
        //        ParentID = 0
        //        If ParentName <> "" Then
        //            CS = core.db.cs_open(MenuContentName, "name=" & core.db.encodeSQLText(ParentName), "ID", False, , , , "ID", 1)
        //            If core.db.cs_ok(CS) Then
        //                ParentID = (core.db.cs_getInteger(CS, "ID"))
        //            End If
        //            Call core.db.cs_Close(CS)
        //        End If
        //        '
        //        ' Set ContentID from ContentName
        //        '
        //        ContentID = -1
        //        If ContentName <> "" Then
        //            ContentID = Models.Complex.cdefModel.getContentId(core,ContentName)
        //        End If
        //        '
        //        ' Locate current entry
        //        '
        //        CSEntry = core.db.cs_open(MenuContentName, "(name=" & core.db.encodeSQLText(EntryName) & ")", "ID", False, , , , SelectList)
        //        '
        //        ' If no current entry, create one
        //        '
        //        If Not core.db.cs_ok(CSEntry) Then
        //            core.db.cs_Close(CSEntry)
        //            CSEntry = core.db.cs_insertRecord(MenuContentName, SystemMemberID)
        //            If core.db.cs_ok(CSEntry) Then
        //                Call core.db.cs_set(CSEntry, "name", EntryName)
        //            End If
        //        End If
        //        If core.db.cs_ok(CSEntry) Then
        //            If ParentID = 0 Then
        //                Call core.db.cs_set(CSEntry, "ParentID", 0)
        //            Else
        //                Call core.db.cs_set(CSEntry, "ParentID", ParentID)
        //            End If
        //            If (ContentID = -1) Then
        //                Call core.db.cs_set(CSEntry, "ContentID", 0)
        //            Else
        //                Call core.db.cs_set(CSEntry, "ContentID", ContentID)
        //            End If
        //            Call core.db.cs_set(CSEntry, "LinkPage", LinkPage)
        //            Call core.db.cs_set(CSEntry, "SortOrder", SortOrder)
        //            Call core.db.cs_set(CSEntry, "AdminOnly", AdminOnly)
        //            Call core.db.cs_set(CSEntry, "DeveloperOnly", DeveloperOnly)
        //            Call core.db.cs_set(CSEntry, "NewWindow", NewWindow)
        //            Call core.db.cs_set(CSEntry, "Active", Active)
        //            If SupportAddonID Then
        //                Call core.db.cs_set(CSEntry, "AddonID", addonId)
        //            End If
        //        End If
        //        Call core.db.cs_Close(CSEntry)
        //    Catch ex As Exception
        //        core.handleExceptionAndContinue(ex) : Throw
        //    End Try
        //End Sub
        //
        //=============================================================================
        //   Verify an Admin Menu Entry
        //       Entries are unique by their name
        //=============================================================================
        //
        //Public Shared Sub admin_VerifyAdminMenu(core As coreClass, ByVal ParentName As String, ByVal EntryName As String, ByVal ContentName As String, ByVal LinkPage As String, ByVal SortOrder As String, Optional ByVal AdminOnly As Boolean = False, Optional ByVal DeveloperOnly As Boolean = False, Optional ByVal NewWindow As Boolean = False, Optional ByVal Active As Boolean = True)

        //    'Call admin_VerifyMenuEntry(core, ParentName, EntryName, ContentName, LinkPage, SortOrder, AdminOnly, DeveloperOnly, NewWindow, Active, cnNavigatorEntries, "")
        //End Sub
        //
        //=============================================================================
        //   Verify an Admin Navigator Entry
        //       Entries are unique by their ccGuid
        //       Includes InstalledByCollectionID
        //       returns the entry id
        //=============================================================================
        //
        public static int verifyNavigatorEntry(coreController core, string ccGuid, string menuNameSpace, string EntryName, string ContentName, string LinkPage, string SortOrder, bool AdminOnly, bool DeveloperOnly, bool NewWindow, bool Active, string AddonName, string NavIconType, string NavIconTitle, int InstalledByCollectionID) {
            int returnEntry = 0;
            try {
                if (!string.IsNullOrEmpty(EntryName.Trim())) {
                    int addonId = core.db.getRecordID(cnAddons, AddonName);
                    int parentId = verifyNavigatorEntry_getParentIdFromNameSpace(core, menuNameSpace);
                    int contentId = Models.Complex.cdefModel.getContentId(core, ContentName);
                    string listCriteria = "(name=" + core.db.encodeSQLText(EntryName) + ")and(Parentid=" + parentId + ")";
                    List<Models.DbModels.NavigatorEntryModel> entryList = NavigatorEntryModel.createList(core, listCriteria, "id");
                    NavigatorEntryModel entry = null;
                    if (entryList.Count == 0) {
                        entry = NavigatorEntryModel.add(core);
                        entry.name = EntryName.Trim();
                        entry.ParentID = parentId;
                    } else {
                        entry = entryList.First();
                    }
                    if (contentId <= 0) {
                        entry.ContentID = 0;
                    } else {
                        entry.ContentID = contentId;
                    }
                    entry.LinkPage = LinkPage;
                    entry.sortOrder = SortOrder;
                    entry.AdminOnly = AdminOnly;
                    entry.DeveloperOnly = DeveloperOnly;
                    entry.NewWindow = NewWindow;
                    entry.active = Active;
                    entry.AddonID = addonId;
                    entry.ccguid = ccGuid;
                    entry.NavIconTitle = NavIconTitle;
                    entry.NavIconType = GetListIndex(NavIconType, NavIconTypeList);
                    entry.InstalledByCollectionID = InstalledByCollectionID;
                    entry.save(core);
                    returnEntry = entry.id;
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return returnEntry;
        }
        //Public Shared Function verifyNavigatorEntry(core As coreClass, ByVal ccGuid As String, ByVal menuNameSpace As String, ByVal EntryName As String, ByVal ContentName As String, ByVal LinkPage As String, ByVal SortOrder As String, ByVal AdminOnly As Boolean, ByVal DeveloperOnly As Boolean, ByVal NewWindow As Boolean, ByVal Active As Boolean, ByVal AddonName As String, ByVal NavIconType As String, ByVal NavIconTitle As String, ByVal InstalledByCollectionID As Integer) As Integer
        //    Dim returnEntry As Integer = 0
        //    Try
        //        '
        //        Const AddonContentName = cnAddons
        //        '
        //        Dim DupFound As Boolean
        //        Dim EntryID As Integer
        //        Dim DuplicateID As Integer
        //        Dim Criteria As String
        //        Dim SelectList As String
        //        Dim CSEntry As Integer
        //        Dim ContentID As Integer
        //        Dim ParentID As Integer
        //        Dim addonId As Integer
        //        Dim CS As Integer
        //        Dim SupportAddonID As Boolean
        //        Dim SupportGuid As Boolean
        //        Dim SupportNavGuid As Boolean
        //        Dim SupportccGuid As Boolean
        //        Dim SupportNavIcon As Boolean
        //        Dim GuidFieldName As String
        //        Dim SupportInstalledByCollectionID As Boolean
        //        '
        //        If Trim(EntryName) <> "" Then
        //            If genericController.vbLCase(EntryName) = "manage add-ons" Then
        //                EntryName = EntryName
        //            End If
        //            '
        //            ' Setup misc arguments
        //            '
        //            SelectList = "Name,ContentID,ParentID,LinkPage,SortOrder,AdminOnly,DeveloperOnly,NewWindow,Active,NavIconType,NavIconTitle"
        //            SupportAddonID = Models.Complex.cdefModel.isContentFieldSupported(core,cnNavigatorEntries, "AddonID")
        //            SupportInstalledByCollectionID = Models.Complex.cdefModel.isContentFieldSupported(core,cnNavigatorEntries, "InstalledByCollectionID")
        //            If SupportAddonID Then
        //                SelectList = SelectList & ",AddonID"
        //            Else
        //                SelectList = SelectList & ",0 as AddonID"
        //            End If
        //            If SupportInstalledByCollectionID Then
        //                SelectList = SelectList & ",InstalledByCollectionID"
        //            End If
        //            If Models.Complex.cdefModel.isContentFieldSupported(core,cnNavigatorEntries, "ccGuid") Then
        //                SupportGuid = True
        //                SupportccGuid = True
        //                GuidFieldName = "ccguid"
        //                SelectList = SelectList & ",ccGuid"
        //            ElseIf Models.Complex.cdefModel.isContentFieldSupported(core,cnNavigatorEntries, "NavGuid") Then
        //                SupportGuid = True
        //                SupportNavGuid = True
        //                GuidFieldName = "navguid"
        //                SelectList = SelectList & ",NavGuid"
        //            Else
        //                SelectList = SelectList & ",'' as ccGuid"
        //            End If
        //            SupportNavIcon = Models.Complex.cdefModel.isContentFieldSupported(core,cnNavigatorEntries, "NavIconType")
        //            addonId = 0
        //            If SupportAddonID And (AddonName <> "") Then
        //                CS = core.db.cs_open(AddonContentName, "name=" & core.db.encodeSQLText(AddonName), "ID", False, , , , "ID", 1)
        //                If core.db.cs_ok(CS) Then
        //                    addonId = core.db.cs_getInteger(CS, "ID")
        //                End If
        //                Call core.db.cs_Close(CS)
        //            End If
        //            ParentID = getNavigatorEntryParentIDFromNameSpace(core, cnNavigatorEntries, menuNameSpace)
        //            ContentID = -1
        //            If ContentName <> "" Then
        //                ContentID = Models.Complex.cdefModel.getContentId(core,ContentName)
        //            End If
        //            '
        //            ' Locate current entry(s)
        //            '
        //            CSEntry = -1
        //            Criteria = ""
        //            If True Then
        //                ' 12/19/2008 change to ActiveOnly - because if there is a non-guid entry, it is marked inactive. We only want to update the active entries
        //                If ccGuid = "" Then
        //                    '
        //                    ' ----- Find match by menuNameSpace
        //                    '
        //                    CSEntry = core.db.cs_open(cnNavigatorEntries, "(name=" & core.db.encodeSQLText(EntryName) & ")and(Parentid=" & ParentID & ")and((" & GuidFieldName & " is null)or(" & GuidFieldName & "=''))", "ID", True, , , , SelectList)
        //                Else
        //                    '
        //                    ' ----- Find match by guid
        //                    '
        //                    CSEntry = core.db.cs_open(cnNavigatorEntries, "(" & GuidFieldName & "=" & core.db.encodeSQLText(ccGuid) & ")", "ID", True, , , , SelectList)
        //                End If
        //                If Not core.db.cs_ok(CSEntry) Then
        //                    '
        //                    ' ----- if not found by guid, look for a name/parent match with a blank guid
        //                    '
        //                    Call core.db.cs_Close(CSEntry)
        //                    Criteria = "AND((" & GuidFieldName & " is null)or(" & GuidFieldName & "=''))"
        //                End If
        //            End If
        //            If Not core.db.cs_ok(CSEntry) Then
        //                If ParentID = 0 Then
        //                    ' 12/19/2008 change to ActiveOnly - because if there is a non-guid entry, it is marked inactive. We only want to update the active entries
        //                    Criteria = Criteria & "And(name=" & core.db.encodeSQLText(EntryName) & ")and(ParentID is null)"
        //                Else
        //                    ' 12/19/2008 change to ActiveOnly - because if there is a non-guid entry, it is marked inactive. We only want to update the active entries
        //                    Criteria = Criteria & "And(name=" & core.db.encodeSQLText(EntryName) & ")and(ParentID=" & ParentID & ")"
        //                End If
        //                CSEntry = core.db.cs_open(cnNavigatorEntries, Mid(Criteria, 4), "ID", True, , , , SelectList)
        //            End If
        //            '
        //            ' If no current entry, create one
        //            '
        //            If Not core.db.cs_ok(CSEntry) Then
        //                core.db.cs_Close(CSEntry)
        //                '
        //                ' This entry was not found - insert a new record if there is no other name/menuNameSpace match
        //                '
        //                If False Then
        //                    '
        //                    ' OK - the first entry search was name/menuNameSpace
        //                    '
        //                    DupFound = False
        //                ElseIf ParentID = 0 Then
        //                    CSEntry = core.db.cs_open(cnNavigatorEntries, "(name=" & core.db.encodeSQLText(EntryName) & ")and(ParentID is null)", "ID", False, , , , SelectList)
        //                    DupFound = core.db.cs_ok(CSEntry)
        //                    core.db.cs_Close(CSEntry)
        //                Else
        //                    CSEntry = core.db.cs_open(cnNavigatorEntries, "(name=" & core.db.encodeSQLText(EntryName) & ")and(ParentID=" & ParentID & ")", "ID", False, , , , SelectList)
        //                    DupFound = core.db.cs_ok(CSEntry)
        //                    core.db.cs_Close(CSEntry)
        //                End If
        //                If DupFound Then
        //                    '
        //                    ' Must block this entry because a menuNameSpace duplicate exists
        //                    '
        //                    CSEntry = -1
        //                Else
        //                    '
        //                    ' Create new entry
        //                    '
        //                    CSEntry = core.db.cs_insertRecord(cnNavigatorEntries, SystemMemberID)
        //                End If
        //            End If
        //            If core.db.cs_ok(CSEntry) Then
        //                EntryID = core.db.cs_getInteger(CSEntry, "ID")
        //                If EntryID = 265 Then
        //                    EntryID = EntryID
        //                End If
        //                Call core.db.cs_set(CSEntry, "name", EntryName)
        //                If ParentID = 0 Then
        //                    Call core.db.cs_set(CSEntry, "ParentID", 0)
        //                Else
        //                    Call core.db.cs_set(CSEntry, "ParentID", ParentID)
        //                End If
        //                If (ContentID = -1) Then
        //                    Call core.db.cs_set(CSEntry, "ContentID", 0)
        //                Else
        //                    Call core.db.cs_set(CSEntry, "ContentID", ContentID)
        //                End If
        //                Call core.db.cs_set(CSEntry, "LinkPage", LinkPage)
        //                Call core.db.cs_set(CSEntry, "SortOrder", SortOrder)
        //                Call core.db.cs_set(CSEntry, "AdminOnly", AdminOnly)
        //                Call core.db.cs_set(CSEntry, "DeveloperOnly", DeveloperOnly)
        //                Call core.db.cs_set(CSEntry, "NewWindow", NewWindow)
        //                Call core.db.cs_set(CSEntry, "Active", Active)
        //                If SupportAddonID Then
        //                    Call core.db.cs_set(CSEntry, "AddonID", addonId)
        //                End If
        //                If SupportGuid Then
        //                    Call core.db.cs_set(CSEntry, GuidFieldName, ccGuid)
        //                End If
        //                If SupportNavIcon Then
        //                    Call core.db.cs_set(CSEntry, "NavIconTitle", NavIconTitle)
        //                    Dim NavIconID As Integer
        //                    NavIconID = GetListIndex(NavIconType, NavIconTypeList)
        //                    Call core.db.cs_set(CSEntry, "NavIconType", NavIconID)
        //                End If
        //                If SupportInstalledByCollectionID Then
        //                    Call core.db.cs_set(CSEntry, "InstalledByCollectionID", InstalledByCollectionID)
        //                End If
        //                '
        //                ' merge any duplicate guid matches
        //                '
        //                Call core.db.cs_goNext(CSEntry)
        //                Do While core.db.cs_ok(CSEntry)
        //                    DuplicateID = core.db.cs_getInteger(CSEntry, "ID")
        //                    Call core.db.executeSql("update ccMenuEntries set ParentID=" & EntryID & " where ParentID=" & DuplicateID)
        //                    Call core.db.deleteContentRecord(cnNavigatorEntries, DuplicateID)
        //                    Call core.db.cs_goNext(CSEntry)
        //                Loop
        //            End If
        //            Call core.db.cs_Close(CSEntry)
        //            '
        //            ' Merge duplicates with menuNameSpace.Name match
        //            '
        //            If EntryID <> 0 Then
        //                If ParentID = 0 Then
        //                    CSEntry = core.db.cs_openCsSql_rev("default", "select * from ccMenuEntries where (parentid is null)and(name=" & core.db.encodeSQLText(EntryName) & ")and(id<>" & EntryID & ")")
        //                Else
        //                    CSEntry = core.db.cs_openCsSql_rev("default", "select * from ccMenuEntries where (parentid=" & ParentID & ")and(name=" & core.db.encodeSQLText(EntryName) & ")and(id<>" & EntryID & ")")
        //                End If
        //                Do While core.db.cs_ok(CSEntry)
        //                    DuplicateID = core.db.cs_getInteger(CSEntry, "ID")
        //                    Call core.db.executeSql("update ccMenuEntries set ParentID=" & EntryID & " where ParentID=" & DuplicateID)
        //                    Call core.db.deleteContentRecord(cnNavigatorEntries, DuplicateID)
        //                    Call core.db.cs_goNext(CSEntry)
        //                Loop
        //                Call core.db.cs_Close(CSEntry)
        //            End If
        //        End If
        //        '
        //        returnEntry = EntryID
        //    Catch ex As Exception
        //        core.handleExceptionAndContinue(ex) : Throw
        //    End Try
        //    Return returnEntry
        //End Function
        //
        //
        //
        public static int verifyNavigatorEntry_getParentIdFromNameSpace(coreController core, string menuNameSpace) {
            int parentRecordId = 0;
            try {
                if (!string.IsNullOrEmpty(menuNameSpace.Trim())) {
                    string[] parents = menuNameSpace.Trim().Split('.');
                    foreach( var parent in parents ) {
                        string recordName = parent.Trim();
                        if (!string.IsNullOrEmpty(recordName)) {
                            string Criteria = "(name=" + core.db.encodeSQLText(recordName) + ")";
                            if (parentRecordId == 0) {
                                Criteria += "and((Parentid is null)or(Parentid=0))";
                            } else {
                                Criteria += "and(Parentid=" + parentRecordId + ")";
                            }
                            int RecordID = 0;
                            int CS = core.db.csOpen(cnNavigatorEntries, Criteria, "ID", true, 0, false, false, "ID", 1);
                            if (core.db.csOk(CS)) {
                                RecordID = (core.db.csGetInteger(CS, "ID"));
                            }
                            core.db.csClose(ref CS);
                            if (RecordID == 0) {
                                CS = core.db.csInsertRecord(cnNavigatorEntries, SystemMemberID);
                                if (core.db.csOk(CS)) {
                                    RecordID = core.db.csGetInteger(CS, "ID");
                                    core.db.csSet(CS, "name", recordName);
                                    core.db.csSet(CS, "parentID", parentRecordId);
                                }
                                core.db.csClose(ref CS);
                            }
                            parentRecordId = RecordID;
                        }
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return parentRecordId;
        }
    }
}
