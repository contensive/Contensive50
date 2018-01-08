
using System;
using System.Reflection;
using System.Xml;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Contensive.Core;
using Contensive.Core.Models.Entity;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
using System.Data;
using System.Linq;
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
        //                If Not cpCore.app.publicFiles.checkPath(DotNetInstallPath) Then
        //                    DotNetInstallPath = "C:\Windows\Microsoft.NET\Framework\"
        //                    If Not cpCore.app.publicFiles.checkPath(DotNetInstallPath) Then
        //                        DotNetInstallPath = ""
        //                    End If
        //                End If
        //            End If
        //            If DotNetInstallPath = "" Then
        //                Call handleLegacyClassError2("", "RegisterDotNet", "Could not detect dotnet installation path. Dot Net may not be installed.")
        //            Else
        //                RegAsmFound = True
        //                RegAsmFilename = DotNetInstallPath & "v2.0.50727\regasm.exe"
        //                If Not cpCore.app.publicFiles.checkFile(RegAsmFilename) Then
        //                    RegAsmFilename = DotNetInstallPath & "v3.0\regasm.exe"
        //                    If Not cpCore.app.publicFiles.checkFile(RegAsmFilename) Then
        //                        RegAsmFilename = DotNetInstallPath & "v3.5\regasm.exe"
        //                        If Not cpCore.app.publicFiles.checkFile(RegAsmFilename) Then
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
        //                        Call runProcess(cpCore, Cmd, , True)
        //                    End If
        //                End If
        //            End If

        //            'cpCore.AppendLog("dll" & ".SiteBuilderClass.RegisterDotnet called Regsvr32, called but the output could not be captured")
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
        //        Select Case cpCore.cluster.config.appPattern.ToLower
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
        //        Call builder.upgrade(cpcore,cpcore,True)
        //        Call cpNewApp.core.app.siteProperty_set(siteproperty_serverPageDefault_name, iisDefaultDoc)
        //        cpNewApp.Dispose()
        //    Catch ex As Exception
        //        cpCore.handleException(ex);
        //    End Try
        //    Return returnOk
        //End Function
        //
        //=============================================================================================================
        //   Main
        //       Returns nothing if all OK, else returns an error message
        //=============================================================================================================
        //
        //Public Function importApp(cpCore As coreClass, ByVal siteName As String, ByVal IPAddress As String, ByVal DomainName As String, ByVal ODBCConnectionString As String, ByVal ContentFilesPath As String, ByVal WWWRootPath As String, ByVal defaultDoc As String, ByVal SMTPServer As String, ByVal AdminEmail As String) As String
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
        //            Call iisController.verifySite(cpCore, siteName, DomainName, "\", defaultDoc)
        //            '
        //            ' Now wait here for site to start with upgrade
        //            '
        //            Call upgrade(cpCore, False)
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
        //Public shared Sub Init(appservicesObj As cpCoreClass)
        //    appservices = appservicesObj
        //    On Error GoTo ErrorTrap
        //    '
        //    Dim MethodName As String
        //    '
        //    MethodName = "Init"
        //    '
        //    ApplicationNameLocal = cpcore.app.appEnvironment.name
        //    ClassInitialized = True
        //    '
        //    Exit Sub
        //    '
        ////ErrorTrap:
        //    dim ex as new exception("todo"): Call HandleClassError(ex,cpcore.app.appEnvironment.name,Err.Number, Err.Source, Err.Description, "Init", True, False)
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
        //    If Not cpcore.app.config.enabled Then
        //        Call Upgrade2( isNewSite)
        //    Else
        //        cpCore.AppendLog("Cannot upgrade until the site is disabled")
        //    End If
        //End Sub
        //
        //=========================================================================
        // upgrade
        //=========================================================================
        //
        public static void upgrade(coreClass cpcore, bool isNewBuild) {
            try {
                if (cpcore.doc.upgradeInProgress) {
                    // leftover from 4.1
                } else {
                    cpcore.doc.upgradeInProgress = true;
                    string DataBuildVersion = cpcore.siteProperties.dataBuildVersion;
                    List<string> nonCriticalErrorList = new List<string>();
                    //
                    // -- determine primary domain
                    string primaryDomain = cpcore.serverConfig.appConfig.name;
                    if (cpcore.serverConfig.appConfig.domainList.Count > 0) {
                        primaryDomain = cpcore.serverConfig.appConfig.domainList[0];
                    }
                    //
                    // -- Verify core table fields (DataSources, Content Tables, Content, Content Fields, Setup, Sort Methods), then other basic system ops work, like site properties
                    VerifyBasicTables(cpcore);
                    //
                    // -- verify base collection
                    logController.appendLogInstall(cpcore, "Install base collection");
                    collectionController.installBaseCollection(cpcore, isNewBuild,ref  nonCriticalErrorList);
                    //
                    // -- Update server config file
                    logController.appendLogInstall(cpcore, "Update configuration file");
                    if (!cpcore.serverConfig.appConfig.appStatus.Equals(Models.Context.serverConfigModel.appStatusEnum.OK)) {
                        cpcore.serverConfig.appConfig.appStatus = Models.Context.serverConfigModel.appStatusEnum.OK;
                        cpcore.serverConfig.saveObject(cpcore);
                    }
                    //
                    // -- verify iis configuration
                    logController.appendLogInstall(cpcore, "Verify iis configuration");
                    Controllers.iisController.verifySite(cpcore, cpcore.serverConfig.appConfig.name, primaryDomain, cpcore.serverConfig.appConfig.appRootFilesPath, "default.aspx");
                    //
                    // -- verify root developer
                    logController.appendLogInstall(cpcore, "verify developer user");
                    var rootList = personModel.createList(cpcore, "(Developer<>0)");
                    if ( rootList.Count==0 ) {
                        logController.appendLogInstall(cpcore, "verify root user, no developers found, adding root/contensive");
                        var root = personModel.add(cpcore);
                        root.name = "root";
                        root.FirstName = "root";
                        root.Username = "root";
                        root.Password = "contensive";
                        root.Developer = true;
                        root.save(cpcore);
                    }
                    //
                    //---------------------------------------------------------------------
                    // ----- Convert Database fields for new Db
                    //---------------------------------------------------------------------
                    //
                    if (isNewBuild) {
                        //
                        // -- Copy default styles into Template Styles
                        //logController.appendLogInstall(cpcore, "New build, verify legacy styles");
                        //cpcore.appRootFiles.copyFile("ccLib\\Config\\Styles.css", "Templates\\Styles.css", cpcore.cdnFiles);
                        //
                        // -- set build version so a scratch build will not go through data conversion
                        DataBuildVersion = cpcore.codeVersion();
                        cpcore.siteProperties.dataBuildVersion = cpcore.codeVersion();
                    }
                    //
                    //---------------------------------------------------------------------
                    // ----- Upgrade Database fields if not new
                    //---------------------------------------------------------------------
                    //
                    if (string.CompareOrdinal(DataBuildVersion, cpcore.codeVersion()) < 0) {
                        //
                        // -- data updates
                        logController.appendLogInstall(cpcore, "Run database conversions, DataBuildVersion [" + DataBuildVersion + "], software version [" + cpcore.codeVersion() + "]");
                        Upgrade_Conversion(cpcore, DataBuildVersion);
                    }
                    //
                    //---------------------------------------------------------------------
                    // ----- Verify content
                    //---------------------------------------------------------------------
                    //
                    logController.appendLogInstall(cpcore, "Verify records required");
                    //
                    // ##### menus are created in ccBase.xml, this just checks for dups
                    VerifyAdminMenus(cpcore, DataBuildVersion);
                    VerifyLanguageRecords(cpcore);
                    VerifyCountries(cpcore);
                    VerifyStates(cpcore);
                    VerifyLibraryFolders(cpcore);
                    VerifyLibraryFileTypes(cpcore);
                    VerifyDefaultGroups(cpcore);
                    VerifyScriptingRecords(cpcore);
                    //
                    //---------------------------------------------------------------------
                    // ----- Set Default SitePropertyDefaults
                    //       must be after upgrade_conversion
                    //---------------------------------------------------------------------
                    //
                    logController.appendLogInstall(cpcore, "Verify Site Properties");
                    //
                    cpcore.siteProperties.getText("AllowAutoHomeSectionOnce", genericController.encodeText(isNewBuild));
                    cpcore.siteProperties.getText("AllowAutoLogin", "False");
                    cpcore.siteProperties.getText("AllowBake", "True");
                    cpcore.siteProperties.getText("AllowChildMenuHeadline", "True");
                    cpcore.siteProperties.getText("AllowContentAutoLoad", "True");
                    cpcore.siteProperties.getText("AllowContentSpider", "False");
                    cpcore.siteProperties.getText("AllowContentWatchLinkUpdate", "True");
                    cpcore.siteProperties.getText("AllowDuplicateUsernames", "False");
                    cpcore.siteProperties.getText("ConvertContentText2HTML", "False");
                    cpcore.siteProperties.getText("AllowMemberJoin", "False");
                    cpcore.siteProperties.getText("AllowPasswordEmail", "True");
                    cpcore.siteProperties.getText("AllowPathBlocking", "True");
                    cpcore.siteProperties.getText("AllowPopupErrors", "True");
                    cpcore.siteProperties.getText("AllowTestPointLogging", "False");
                    cpcore.siteProperties.getText("AllowTestPointPrinting", "False");
                    cpcore.siteProperties.getText("AllowTransactionLog", "False");
                    cpcore.siteProperties.getText("AllowTrapEmail", "True");
                    cpcore.siteProperties.getText("AllowTrapLog", "True");
                    cpcore.siteProperties.getText("AllowWorkflowAuthoring", "False");
                    cpcore.siteProperties.getText("ArchiveAllowFileClean", "False");
                    cpcore.siteProperties.getText("ArchiveRecordAgeDays", "90");
                    cpcore.siteProperties.getText("ArchiveTimeOfDay", "2:00:00 AM");
                    cpcore.siteProperties.getText("BreadCrumbDelimiter", "&nbsp;&gt;&nbsp;");
                    cpcore.siteProperties.getText("CalendarYearLimit", "1");
                    cpcore.siteProperties.getText("ContentPageCompatibility21", "false");
                    cpcore.siteProperties.getText("DefaultFormInputHTMLHeight", "500");
                    cpcore.siteProperties.getText("DefaultFormInputTextHeight", "1");
                    cpcore.siteProperties.getText("DefaultFormInputWidth", "60");
                    cpcore.siteProperties.getText("EditLockTimeout", "5");
                    cpcore.siteProperties.getText("EmailAdmin", "webmaster@" + cpcore.serverConfig.appConfig.domainList[0]);
                    cpcore.siteProperties.getText("EmailFromAddress", "webmaster@" + cpcore.serverConfig.appConfig.domainList[0]);
                    cpcore.siteProperties.getText("EmailPublishSubmitFrom", "webmaster@" + cpcore.serverConfig.appConfig.domainList[0]);
                    cpcore.siteProperties.getText("Language", "English");
                    cpcore.siteProperties.getText("PageContentMessageFooter", "Copyright " + cpcore.serverConfig.appConfig.domainList[0]);
                    cpcore.siteProperties.getText("SelectFieldLimit", "4000");
                    cpcore.siteProperties.getText("SelectFieldWidthLimit", "100");
                    cpcore.siteProperties.getText("SMTPServer", "127.0.0.1");
                    cpcore.siteProperties.getText("TextSearchEndTag", "<!-- TextSearchEnd -->");
                    cpcore.siteProperties.getText("TextSearchStartTag", "<!-- TextSearchStart -->");
                    cpcore.siteProperties.getText("TrapEmail", "");
                    cpcore.siteProperties.getText("TrapErrors", "0");
                    addonModel defaultRouteAddon = addonModel.create(cpcore, cpcore.siteProperties.defaultRouteId);
                    if (defaultRouteAddon == null) {
                        defaultRouteAddon = addonModel.create(cpcore, addonGuidPageManager);
                        if (defaultRouteAddon != null) {
                            cpcore.siteProperties.defaultRouteId = defaultRouteAddon.id;
                        }
                    }
                    //
                    //---------------------------------------------------------------------
                    // ----- Changes that effect the web server or content files, not the Database
                    //---------------------------------------------------------------------
                    //
                    int StyleSN = (cpcore.siteProperties.getInteger("StylesheetSerialNumber"));
                    if (StyleSN > 0) {
                        StyleSN += 1;
                        cpcore.siteProperties.setProperty("StylesheetSerialNumber", StyleSN.ToString());
                        // too lazy
                        //Call cpcore.app.publicFiles.SaveFile(cpcore.app.genericController.convertCdnUrlToCdnPathFilename("templates\Public" & StyleSN & ".css"), cpcore.app.csv_getStyleSheetProcessed)
                        //Call cpcore.app.publicFiles.SaveFile(cpcore.app.genericController.convertCdnUrlToCdnPathFilename("templates\Admin" & StyleSN & ".css", cpcore.app.csv_getStyleSheetDefault)
                    }
                    //
                    // clear all cache
                    //
                    cpcore.cache.invalidateAll();
                    //
                    if (isNewBuild) {
                        //
                        // -- primary domain
                        domainModel domain = domainModel.createByName(cpcore, primaryDomain);
                        if (domain == null) {
                            domain = domainModel.add(cpcore);
                            domain.name = primaryDomain;
                        }
                        //
                        // -- Landing Page
                        pageContentModel landingPage = pageContentModel.create(cpcore, DefaultLandingPageGuid);
                        if (landingPage == null) {
                            landingPage = pageContentModel.add(cpcore);
                            landingPage.ccguid = DefaultLandingPageGuid;
                        }
                        //
                        // -- default template
                        pageTemplateModel defaultTemplate = pageTemplateModel.createByName(cpcore, "Default");
                        if (defaultTemplate == null) {
                            defaultTemplate = pageTemplateModel.add(cpcore);
                            defaultTemplate.name = "Default";
                        }
                        domain.defaultTemplateId = defaultTemplate.id;
                        domain.name = primaryDomain;
                        domain.pageNotFoundPageId = landingPage.id;
                        domain.rootPageId = landingPage.id;
                        domain.typeId = (int) domainModel.domainTypeEnum.Normal;
                        domain.visited = false;
                        domain.save(cpcore);
                        //
                        landingPage.TemplateID = defaultTemplate.id;
                        landingPage.Copyfilename.content = constants.defaultLandingPageHtml;
                        landingPage.save(cpcore);
                        //
                        defaultTemplate.bodyHTML = cpcore.appRootFiles.readFile(defaultTemplateHomeFilename);
                        defaultTemplate.save(cpcore);
                        //
                        if (cpcore.siteProperties.getInteger("LandingPageID", landingPage.id) == 0) {
                            cpcore.siteProperties.setProperty("LandingPageID", landingPage.id);
                        }
                    }
                    //
                    //---------------------------------------------------------------------
                    // ----- internal upgrade complete
                    //---------------------------------------------------------------------
                    //
                    if (true) {
                        logController.appendLogInstall(cpcore, "Internal upgrade complete, set Buildversion to " + cpcore.codeVersion());
                        cpcore.siteProperties.setProperty("BuildVersion", cpcore.codeVersion());
                        //
                        //---------------------------------------------------------------------
                        // ----- Upgrade local collections
                        //       This would happen if this is the first site to upgrade after a new build is installed
                        //       (can not be in startup because new addons might fail with DbVersions)
                        //       This means a dataupgrade is required with a new build - You can expect errors
                        //---------------------------------------------------------------------
                        //
                        if (true) {
                            //If Not UpgradeDbOnly Then
                            //
                            // 4.1.575 - 8/28 - put this code behind the DbOnly check, makes DbOnly beuild MUCH faster
                            //
                            string ErrorMessage = "";
                            bool IISResetRequired = false;
                            //RegisterList = ""
                            logController.appendLogInstall(cpcore, "Upgrading All Local Collections to new server build.");
                            string tmpString = "";
                            bool UpgradeOK = collectionController.UpgradeLocalCollectionRepoFromRemoteCollectionRepo(cpcore, ref ErrorMessage, ref tmpString, ref  IISResetRequired, isNewBuild, ref  nonCriticalErrorList);
                            if (!string.IsNullOrEmpty(ErrorMessage)) {
                                throw (new ApplicationException("Unexpected exception")); //cpCore.handleLegacyError3(cpcore.serverConfig.appConfig.name, "During UpgradeAllLocalCollectionsFromLib3 call, " & ErrorMessage, "dll", "builderClass", "Upgrade2", 0, "", "", False, True, "")
                            } else if (!UpgradeOK) {
                                throw (new ApplicationException("Unexpected exception")); //cpCore.handleLegacyError3(cpcore.serverConfig.appConfig.name, "During UpgradeAllLocalCollectionsFromLib3 call, NotOK was returned without an error message", "dll", "builderClass", "Upgrade2", 0, "", "", False, True, "")
                            }
                            //
                            //---------------------------------------------------------------------
                            // ----- Upgrade collections added during upgrade process
                            //---------------------------------------------------------------------
                            //
                            //Call appendUpgradeLog(cpcore, "Installing Add-on Collections gathered during upgrade")
                            //If InstallCollectionList = "" Then
                            //    Call appendUpgradeLog(cpCore.app.config.name, MethodName, "No Add-on collections added during upgrade")
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
                            //                Call addonInstall.installCollectionFromLocalRepo(Me, IISResetRequired, Guid, cpCore.version, ErrorMessage, "", "", isNewBuild)
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
                            //                   throw (New ApplicationException("Unexpected exception"))'cpCore.handleLegacyError3(cpCore.app.config.name, "Error upgrading Addon Collection [" & Guid & "], " & ErrorMessage, "dll", "builderClass", "Upgrade2", 0, "", "", False, True, "")
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
                            logController.appendLogInstall(cpcore, "Checking all installed collections for upgrades from Collection Library");
                            logController.appendLogInstall(cpcore, "...Open collectons.xml");
                            try {
                                XmlDocument Doc = new XmlDocument();
                                Doc.LoadXml(collectionController.getCollectionListFile(cpcore));
                                if (true) {
                                    if (genericController.vbLCase(Doc.DocumentElement.Name) != genericController.vbLCase(CollectionListRootNode)) {
                                        throw (new ApplicationException("Unexpected exception")); //cpCore.handleLegacyError3(cpcore.serverConfig.appConfig.name, "Error loading Collection config file. The Collections.xml file has an invalid root node, [" & Doc.DocumentElement.Name & "] was received and [" & CollectionListRootNode & "] was expected.", "dll", "builderClass", "Upgrade", 0, "", "", False, True, "")
                                    } else {
                                        if (genericController.vbLCase(Doc.DocumentElement.Name) == "collectionlist") {
                                            //
                                            // now go through each collection in this app and check the last updated agains the one here
                                            //
                                            logController.appendLogInstall(cpcore, "...Open site collectons, iterate through all collections");
                                            //Dim dt As DataTable
                                            DataTable dt = cpcore.db.executeQuery("select * from ccaddoncollections where (ccguid is not null)and(updatable<>0)");
                                            if (dt.Rows.Count > 0) {
                                                int rowptr = 0;
                                                for (rowptr = 0; rowptr < dt.Rows.Count; rowptr++) {

                                                    ErrorMessage = "";
                                                    CollectionGuid = genericController.vbLCase(dt.Rows[rowptr]["ccguid"].ToString());
                                                    Collectionname = dt.Rows[rowptr]["name"].ToString();
                                                    logController.appendLogInstall(cpcore, "...checking collection [" + Collectionname + "], guid [" + CollectionGuid + "]");
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
                                                            appendUpgradeLog(cpcore, cpcore.serverConfig.appConfig.name, "upgrade", "Upgrading collection " + dt.Rows[rowptr]["name"].ToString() + " because the collection installed in the application has no LastChangeDate. It may have been installed manually.");
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
                                                                    logController.appendLogInstall(cpcore, "...local collection found");
                                                                    if (LocalLastChangeDate != DateTime.MinValue) {
                                                                        if (LocalLastChangeDate > LastChangeDate) {
                                                                            appendUpgradeLog(cpcore, cpcore.serverConfig.appConfig.name, "upgrade", "Upgrading collection " + dt.Rows[rowptr]["name"].ToString() + " because the collection in the local server store has a newer LastChangeDate than the collection installed on this application.");
                                                                            upgradeCollection = true;
                                                                        }
                                                                    }
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                        ErrorMessage = "";
                                                        if (!localCollectionFound) {
                                                            logController.appendLogInstall(cpcore, "...site collection [" + Collectionname + "] not found in local collection, call UpgradeAllAppsFromLibCollection2 to install it.");
                                                            bool addonInstallOk = collectionController.installCollectionFromRemoteRepo(cpcore, CollectionGuid, ref  ErrorMessage, "", isNewBuild, ref nonCriticalErrorList);
                                                            if (!addonInstallOk) {
                                                                //
                                                                // this may be OK so log, but do not call it an error
                                                                //
                                                                logController.appendLogInstall(cpcore, "...site collection [" + Collectionname + "] not found in collection Library. It may be a custom collection just for this site. Collection guid [" + CollectionGuid + "]");
                                                            }
                                                        } else {
                                                            if (upgradeCollection) {
                                                                logController.appendLogInstall(cpcore, "...upgrading collection");
                                                                collectionController.installCollectionFromLocalRepo(cpcore, CollectionGuid, cpcore.codeVersion(), ref ErrorMessage, "", isNewBuild, ref nonCriticalErrorList);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                            } catch (Exception ex9) {
                                cpcore.handleException(ex9);
                            }
                        }
                    }
                    //
                    //---------------------------------------------------------------------
                    // ----- Explain, put up a link and exit without continuing
                    //---------------------------------------------------------------------
                    //
                    cpcore.cache.invalidateAll();
                    logController.appendLogInstall(cpcore, "Upgrade Complete");
                    cpcore.doc.upgradeInProgress = false;
                }
            } catch (Exception ex) {
                cpcore.handleException(ex);
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
        //            Call cpCore.app.executeSql(SQL)
        //            '
        //            Exit Sub
        //            '
        //            ' ----- Error Trap
        //            '
        ////ErrorTrap:
        //            Dim ex As New Exception("todo") : Call handleClassException(ex, cpCore.app.config.name, "methodNameFPO") ' Err.Number, Err.Source, Err.Description, "RenameContentDefinition", True, False)
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
        //            Call cpcore.app.csv_CreateSQLTableField(DataSourceName, TableName, "TempField", FieldTypeText)
        //            CSPointer = cpcore.app.csv_OpenCSSQL(DataSourceName, "SELECT ID, Sortorder from " & TableName & " Order By Sortorder;")
        //            If Not cpcore.app.csv_IsCSOK(CSPointer) Then
        //                Dim ex2 As New Exception("todo") : Call HandleClassError(ex2, cpcore.app.config.name, methodName) ' ignoreInteger, "dll", "Could not upgrade SortOrder", "UpgradeSortOrder", False, True)
        //            Else
        //                Do While cpcore.app.csv_IsCSOK(CSPointer)
        //                    Call cpcore.app.ExecuteSQL(DataSourceName, "UPDATE " & TableName & " SET TempField=" & encodeSQLText(Format(cpcore.app.csv_cs_getInteger(CSPointer, "sortorder"), "00000000")) & " WHERE ID=" & encodeSQLNumber(cpcore.app.csv_cs_getInteger(CSPointer, "ID")) & ";")
        //                    cpcore.app.csv_NextCSRecord(CSPointer)
        //                Loop
        //                Call cpcore.app.csv_CloseCS(CSPointer)
        //                Call cpcore.app.csv_DeleteTableIndex(DataSourceName, TableName, "SORTORDER")
        //                Call cpcore.app.csv_DeleteTableIndex(DataSourceName, TableName, TableName & "SORTORDER")
        //                Call cpcore.app.csv_DeleteTableField(DataSourceName, TableName, "SortOrder")
        //                Call cpcore.app.csv_CreateSQLTableField(DataSourceName, TableName, "SortOrder", FieldTypeText)
        //                Call cpcore.app.ExecuteSQL(DataSourceName, "UPDATE " & TableName & " SET SortOrder=TempField;")
        //                Call cpcore.app.csv_CreateSQLIndex(DataSourceName, TableName, TableName & "SORTORDER", "SortOrder")
        //            End If
        //            Call cpcore.app.csv_DeleteTableField(DataSourceName, TableName, "TempField")
        //            '
        //            Exit Sub
        //            '
        //            ' ----- Error Trap
        //            '
        ////ErrorTrap:
        //            Dim ex As New exception("todo") : Call HandleClassError(ex, cpcore.app.config.name, "methodNameFPO") ' Err.Number, Err.Source, Err.Description, "UpgradeSortOrder", True, False)
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
        //            dt = cpCore.app.executeSql("SELECT * FROM " & TableName & ";")
        //            If dt.Rows.Count = 0 Then
        //                Call cpCore.app.executeSql("INSERT INTO " & TableName & " (Name)VALUES('no name');")
        //                dt = cpCore.app.executeSql("SELECT * FROM " & TableName & ";")
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
        //            Dim ex As New Exception("todo") : Call handleClassException(ex, cpCore.app.config.name, "methodNameFPO") ' Err.Number, Err.Source, Err.Description, "ExistsSQLTableField", True, False)
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
        //            CSPointer = cpcore.app.csv_InsertCSRecord(ContentName)
        //            If cpcore.app.csv_IsCSOK(CSPointer) Then
        //                RecordID = (cpcore.app.csv_cs_getInteger(CSPointer, "ID"))
        //                Filename = cpcore.app.csv_cs_getFilename(CSPointer, "Name", "")
        //                'Filename = cpcore.app.csv_GetVirtualFilename(ContentName, "Name", RecordID)
        //                Call cpcore.app.csv_SetCSField(CSPointer, "name", PageName)
        //                Call cpcore.app.csv_SetCSField(CSPointer, "copyfilename", Filename)
        //                Call cpcore.app.publicFiles.SaveFile(Filename, PageCopy)
        //            End If
        //            cpcore.app.csv_CloseCS(CSPointer)
        //            Exit Sub
        //            '
        //            ' ----- Error Trap
        //            '
        ////ErrorTrap:
        //            Dim ex As New Exception("todo") : Call HandleClassError(ex, cpcore.app.config.name, "createPage") ' Err.Number, Err.Source, Err.Description, "CreatePage", True, False)
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
        //            Call cpcore.app.csv_CreateContentFromSQLTable("Default", "ccTables", "Tables")
        //            TablesCID = cpcore.app.csv_GetContentID("Tables")
        //            If True Then
        //                '
        //                ' ----- Create the ccTables TableName entry in the ccContent Table
        //                '
        //                SQL = "Update ccContent set sqlTable='ccTables' where name='tables';"
        //                Call cpcore.app.executeSql(SQL)
        //                '
        //                ' ----- Append tables from ccContent
        //                '
        //                SQL = "Select ID, sqlTable,DataSourceID From ccContent where active<>0;"
        //                RS = cpcore.app.executeSql(SQL)
        //                If (isDataTableOk(rs)) Then
        //                    '
        //                    ' if no error, field exists, and it is OK to continue
        //                    '
        //                    Do While Not rs.rows.count=0
        //                        TableName = genericController.encodeText(cpcore.app.getDataRowColumnName(RS.rows(0), "sqlTable"))
        //                        TableID = 0
        //                        DataSourceID = genericController.EncodeInteger(cpcore.app.getDataRowColumnName(RS.rows(0), "DataSourceID"))
        //                        ContentID = genericController.EncodeInteger(cpcore.app.getDataRowColumnName(RS.rows(0), "ID"))
        //                        If TableName <> "" Then
        //                            '
        //                            ' ----- Get TableID from TableName
        //                            '
        //                            SQL = "SELECT ID FROM ccTables where name=" & EncodeSQLText(TableName) & ";"
        //                            RSTables = cpcore.app.executeSql(SQL)
        //                            If Not RSTables.EOF Then
        //                                '
        //                                ' ----- Table entry found
        //                                '
        //                                TableID = cpcore.app.getDataRowColumnName(RSTables, "ID")
        //                            Else
        //                                '
        //                                ' ----- Table entry not found in ccTables, Create it
        //                                '
        //                                RSNewTable = cpcore.app.csv_InsertTableRecordGetDataTable("Default", "ccTables")
        //                                If Not (RSNewTable Is Nothing) Then
        //                                    TableID = cpcore.app.getDataRowColumnName(RSNewTable, "ID")
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
        //                                    Call cpcore.app.executeSql(SQL)
        //                                End If
        //                                RSNewTable = Nothing
        //                            End If
        //                            RSTables = Nothing
        //                        End If
        //                        If TableID <> 0 Then
        //                            SQL = "Update ccContent set ContentTableID=" & EncodeSQLNumber(TableID) & ", AuthoringTableID=" & EncodeSQLNumber(TableID) & " where ID=" & ContentID & ";"
        //                            Call cpcore.app.executeSql(SQL)
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
        //            Dim ex As New exception("todo") : Call HandleClassError(ex, cpcore.app.config.name, "methodNameFPO") ' Err.Number, Err.Source, Err.Description, "PopulateTableTable (error #" & UpgradeErrorCount & ")", True, True)
        //            If UpgradeErrorCount >= UpgradeErrorTheshold Then
        //                Dim ex3 As New Exception("todo") : Call HandleClassError(ex3, cpcore.app.config.name, MethodName) ' Err.Number, Err.Source, Err.Description, "PopulateTableTable (error #" & UpgradeErrorCount & ")", True, False)
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
        //            'TimeoutSave = cpcore.app.csv_SQLCommandTimeout
        //            'cpcore.app.csv_SQLCommandTimeout = 1800
        //            '
        //            If False Then
        //                Exit Sub
        //            Else
        //                '
        //                MethodName = "Upgrade_Conversion_to_41"
        //                '
        //                If false Then
        //                    Call appendUpgradeLogAddStep(cpcore.app.config.name,MethodName, "4.1.279 upgrade")
        //                    '
        //                    ' added ccguid to all cdefs, but non-base did not upgrade automatically
        //                    '
        //                    CS = cpcore.app.csOpen("content")
        //                    Do While cpcore.app.csv_IsCSOK(CS)
        //                        Call metaData_VerifyCDefField_ReturnID(True, cpcore.app.csv_cs_getText(CS, "name"), "ccguid", FieldTypeText, , False, "Guid", , , , , , , , , , , , , , , , , True)
        //                        Call cpcore.app.csv_NextCSRecord(CS)
        //                    Loop
        //                    Call cpcore.app.csv_CloseCS(CS)
        //                End If
        //                If false Then
        //                    Call appendUpgradeLogAddStep(cpcore.app.config.name,MethodName, "4.1.288 upgrade")
        //                    '
        //                    ' added updatable again (was originally added in 275)
        //                    '
        //                    Call cpcore.app.ExecuteSQL("", "update ccAddonCollections set updatable=1")
        //                End If
        //                If false Then
        //                    Call appendUpgradeLogAddStep(cpcore.app.config.name,MethodName, "4.1.290 upgrade")
        //                    '
        //                    ' delete blank field help records, new method creates dups of inheritance cdef parent fields
        //                    '
        //                    Call cpcore.app.ExecuteSQL("", "delete from ccfieldhelp where (HelpDefault is null)and(HelpCustom is null)")
        //                End If
        //                If false Then
        //                    Call appendUpgradeLogAddStep(cpcore.app.config.name,MethodName, "4.1.294 upgrade")
        //                    '
        //                    ' convert fieldtypelongtext + htmlcontent to fieldtypehtml
        //                    '
        //                    Call cpcore.app.ExecuteSQL("", "update ccfields set type=" & FieldTypeHTML & " where type=" & FieldTypeLongText & " and (htmlcontent<>0)")
        //                    '
        //                    ' convert fieldtypetextfile + htmlcontent to fieldtypehtmlfile
        //                    '
        //                    Call cpcore.app.ExecuteSQL("", "update ccfields set type=" & FieldTypeHTMLFile & " where type=" & FieldTypeTextFile & " and (htmlcontent<>0)")
        //                End If
        //                If false Then
        //                    Call appendUpgradeLogAddStep(cpcore.app.config.name,MethodName, "4.1.352 upgrade")
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
        //                    Call appendUpgradeLogAddStep(cpcore.app.config.name,MethodName, "4.1.374 upgrade")
        //                    '
        //                    ' repair the country table (abbreviation was set to an integer a long time ago)
        //                    '
        //                    SQL = "insert into cccountries (abbreviation)values('US')"
        //                    //On Error //Resume Next
        //                    Call cpcore.app.ExecuteSQL("", CStr(SQL))
        //                    ErrMessage = Err.Description
        //                    Err.Clear()
        //                    On Error GoTo ErrorTrap
        //                    SQL = "delete from cccountries where (name is null) or (name='')"
        //                    Call cpcore.app.ExecuteSQL("", CStr(SQL))
        //                    If ErrMessage <> "" Then
        //                        '
        //                        ' needs to be fixed
        //                        '
        //                        typeId = cpcore.app.csv_GetDataSourceType("default")
        //                        If typeId = DataSourceTypeODBCAccess Then
        //                            '
        //                            ' MS Access
        //                            '
        //                            SQL = "alter table cccountries add column abbr VarChar(255) NULL"
        //                            Call cpcore.app.ExecuteSQL("", CStr(SQL))
        //                            '
        //                            SQL = "update cccountries set abbr=abbreviation"
        //                            Call cpcore.app.ExecuteSQL("", CStr(SQL))
        //                            '
        //                            SQL = "alter table cccountries drop abbreviation"
        //                            Call cpcore.app.ExecuteSQL("", CStr(SQL))
        //                            '
        //                            SQL = "alter table cccountries add column abbreviation VarChar(255) NULL"
        //                            Call cpcore.app.ExecuteSQL("", CStr(SQL))
        //                            '
        //                            SQL = "update cccountries set abbreviation=abbr"
        //                            Call cpcore.app.ExecuteSQL("", CStr(SQL))
        //                            '
        //                            SQL = "alter table cccountries drop abbr"
        //                            Call cpcore.app.ExecuteSQL("", CStr(SQL))
        //                        ElseIf typeId = DataSourceTypeODBCSQLServer Then
        //                            '
        //                            ' MS SQL Server
        //                            '
        //                            SQL = "alter table cccountries add abbr VarChar(255) NULL"
        //                            Call cpcore.app.ExecuteSQL("", CStr(SQL))
        //                            '
        //                            SQL = "update cccountries set abbr=abbreviation"
        //                            Call cpcore.app.ExecuteSQL("", CStr(SQL))
        //                            '
        //                            SQL = "alter table cccountries drop column abbreviation"
        //                            Call cpcore.app.ExecuteSQL("", CStr(SQL))
        //                            '
        //                            SQL = "alter table cccountries add abbreviation VarChar(255) NULL"
        //                            Call cpcore.app.ExecuteSQL("", CStr(SQL))
        //                            '
        //                            SQL = "update cccountries set abbreviation=abbr"
        //                            Call cpcore.app.ExecuteSQL("", CStr(SQL))
        //                            '
        //                            SQL = "alter table cccountries drop column abbr"
        //                            Call cpcore.app.ExecuteSQL("", CStr(SQL))
        //                        End If
        //                    End If
        //                    '
        //                    ' remove all ccwebx3 and ccwebx4 addons
        //                    '
        //                    SQL = "delete from ccaggregatefunctions where ObjectProgramID like '%ccwebx3%'"
        //                    Call cpcore.app.ExecuteSQL("", CStr(SQL))
        //                End If
        //                If false Then
        //                    Call appendUpgradeLogAddStep(cpcore.app.config.name,MethodName, "4.1.508 upgrade")
        //                    '
        //                    ' repair the country table (abbreviation was set to an integer a long time ago)
        //                    '
        //                    Call cpcore.app.setSiteProperty("DefaultFormInputHTMLHeight", "500", 0)
        //                End If
        //                If false Then
        //                    Call appendUpgradeLogAddStep(cpcore.app.config.name,MethodName, "4.1.517 upgrade")
        //                    '
        //                    ' prepopulate docpcore.app.main_allowCrossLogin
        //                    '
        //                    SQL = "update ccDomains set allowCrossLogin=1 where typeid=1"
        //                    Call cpcore.app.ExecuteSQL("", CStr(SQL))
        //                End If
        //                If false Then
        //                    Call appendUpgradeLogAddStep(cpcore.app.config.name,MethodName, "4.1.588 upgrade")
        //                    '
        //                    ' ccfields.htmlContent redefined
        //                    '   means nothing for fields not set to html or htmlFile
        //                    '   for these, it means the initial editor should show the HTML (not wysiwyg) -- "real html"
        //                    '
        //                    SQL = "update ccFields set htmlcontent=null where (name<>'layout')or(contentid<>" & cpcore.app.csv_GetContentID("layouts") & ")"
        //                    Call cpcore.app.ExecuteSQL("", CStr(SQL))
        //                End If
        //                '
        //                ' return the normal timeout
        //                '
        //                cpcore.app.csv_SQLCommandTimeout = TimeoutSave
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
        //            Dim ex As New exception("todo") : Call HandleClassError(ex, cpcore.app.config.name, "methodNameFPO") ' Err.Number, Err.Source, Err.Description, "Upgrade_Conversion_to_41 (error #" & UpgradeErrorCount & ")", True, True)
        //            If UpgradeErrorCount >= UpgradeErrorTheshold Then
        //                Dim ex4 As New Exception("todo") : Call HandleClassError(ex4, cpcore.app.config.name, MethodName) ' Err.Number, Err.Source, Err.Description, "Upgrade_Conversion_to_41 (error #" & UpgradeErrorCount & ")", True, False)
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
        //            Call cpcore.app.csv_DeleteTableIndex(DataSourceName, TableName, TableName & FieldName)
        //            'Call cpcore.app.csv_DeleteTableIndex(DataSourceName, TableName, TableName & FieldName)
        //            '
        //            ' Delete all Content Definition Fields associated with this field
        //            '
        //            RSTables = cpcore.app.ExecuteSQL(DataSourceName, "SELECT ID from ccTables where name='" & TableName & "';")
        //            If Not (RSTables Is Nothing) Then
        //                Do While Not RSTables.EOF
        //                    TableID = genericController.EncodeInteger(RSTables("ID"))
        //                    RSContent = cpcore.app.ExecuteSQL(DataSourceName, "Select ID from ccContent where (ContentTableID=" & TableID & ")or(AuthoringTableID=" & TableID & ");")
        //                    If Not (RSContent Is Nothing) Then
        //                        Do While Not RSContent.EOF
        //                            ContentID = genericController.EncodeInteger(RSContent("ID"))
        //                            Call cpcore.app.ExecuteSQL(DataSourceName, "Delete From ccFields where (ContentID=" & ContentID & ")and(name=" & encodeSQLText(FieldName) & ");")
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
        //            Call cpcore.app.csv_DeleteTableField(DataSourceName, TableName, FieldName)
        //            '
        //            Exit Sub
        //            '
        ////ErrorTrap:
        //            Dim ex As New Exception("todo") : Call HandleClassError(ex, cpcore.app.config.name, "deleteField") ' Err.Number, Err.Source, Err.Description, "DeleteField", True, False)
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
        //            RS = cpcore.app.ExecuteSQL( "Select ID from ccTables where name=" & encodeSQLText(TableName) & ";")
        //            If (isDataTableOk(rs)) Then
        //                If Not rs.rows.count=0 Then
        //                    GetTableID = cpcore.app.getDataRowColumnName(RS.rows(0), "ID")
        //                End If
        //            End If
        //            '
        //            Exit Function
        //            '
        ////ErrorTrap:
        //            Dim ex As New Exception("todo") : Call HandleClassError(ex, cpcore.app.config.name, "methodNameFPO") ' Err.Number, Err.Source, Err.Description, "GetTableID", True, False)
        //        End Function
        //        '
        //        '
        //        '
        //        Private shared Function core_group_add(ByVal GroupName As String) As Integer
        //            On Error GoTo ErrorTrap
        //            '
        //            Dim dt As DataTable
        //            Dim sql As String
        //            Dim createkey As Integer = genericController.GetRandomInteger(cpCore)
        //            Dim cid As Integer
        //            '
        //            core_group_add = 0
        //            dt = cpCore.app.executeSql("SELECT ID FROM CCGROUPS WHERE NAME='" & GroupName & "';")
        //            If dt.Rows.Count > 0 Then
        //                core_group_add = genericController.EncodeInteger(dt.Rows[0]["ID"))
        //            Else
        //                cid = GetContentID("groups")
        //                sql = "insert into ccgroups (contentcontrolid,active,createkey,name,caption) values (" & cid & ",1," & createkey & "," & EncodeSQLText(GroupName) & "," & EncodeSQLText(GroupName) & ")"
        //                Call cpCore.app.executeSql(sql)
        //                sql = "select id from ccgroups where createkey=" & createkey & " order by id desc"
        //                dt = cpCore.app.executeSql(sql)
        //                If dt.Rows.Count > 0 Then
        //                    core_group_add = genericController.EncodeInteger(dt.Rows[0]["id"))
        //                End If

        //            End If
        //            '
        //            Exit Function
        //            '
        ////ErrorTrap:
        //            Dim ex As New Exception("todo") : Call handleClassException(ex, cpCore.app.config.name, "methodNameFPO") ' Err.Number, Err.Source, Err.Description, "AddGroup", True, False)
        //        End Function
        //
        //
        //
        private static void VerifyAdminMenus(coreClass cpCore, string DataBuildVersion) {
            try {
                DataTable dt = null;
                //
                // ----- remove duplicate menus that may have been added during faulty upgrades
                //
                string FieldNew = null;
                string FieldLast = null;
                int FieldRecordID = 0;
                //Dim dt As DataTable
                dt = cpCore.db.executeQuery("Select ID,Name,ParentID from ccMenuEntries where (active<>0) Order By ParentID,Name");
                if (dt.Rows.Count > 0) {
                    FieldLast = "";
                    for (var rowptr = 0; rowptr < dt.Rows.Count; rowptr++) {
                        FieldNew = genericController.encodeText(dt.Rows[rowptr]["name"]) + "." + genericController.encodeText(dt.Rows[rowptr]["parentid"]);
                        if (FieldNew == FieldLast) {
                            FieldRecordID = genericController.encodeInteger(dt.Rows[rowptr]["ID"]);
                            cpCore.db.executeQuery("Update ccMenuEntries set active=0 where ID=" + FieldRecordID + ";");
                        }
                        FieldLast = FieldNew;
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        // Get the Menu for FormInputHTML
        //
        private static void VerifyRecord(coreClass cpcore, string ContentName, string Name, string CodeFieldName = "", string Code = "", bool InActive = false) {
            try {
                bool Active = false;
                DataTable dt = null;
                string sql1 = null;
                string sql2 = null;
                string sql3 = null;
                //
                Active = !InActive;
                Models.Complex.cdefModel cdef = Models.Complex.cdefModel.getCdef(cpcore, ContentName);
                string tableName = cdef.ContentTableName;
                int cid = cdef.Id;
                //
                dt = cpcore.db.executeQuery("SELECT ID FROM " + tableName + " WHERE NAME=" + cpcore.db.encodeSQLText(Name) + ";");
                if (dt.Rows.Count == 0) {
                    sql1 = "insert into " + tableName + " (contentcontrolid,createkey,active,name";
                    sql2 = ") values (" + cid + ",0," + cpcore.db.encodeSQLBoolean(Active) + "," + cpcore.db.encodeSQLText(Name);
                    sql3 = ")";
                    if (!string.IsNullOrEmpty(CodeFieldName)) {
                        sql1 += "," + CodeFieldName;
                        sql2 += "," + Code;
                    }
                    cpcore.db.executeQuery(sql1 + sql2 + sql3);
                }
            } catch (Exception ex) {
                cpcore.handleException(ex);
                throw;
            }
        }
        //
        //========================================================================
        // ----- Upgrade Conversion
        //========================================================================
        //
        private static void Upgrade_Conversion(coreClass cpCore, string DataBuildVersion) {
            try {
                //
                // -- Roll the style sheet cache if it is setup
                cpCore.siteProperties.setProperty("StylesheetSerialNumber", (-1).ToString());
                //
                // -- Reload
                cpCore.cache.invalidateAll();
                cpCore.doc.clearMetaData();
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //=========================================================================================
        //
        //=========================================================================================
        //
        public static void VerifyTableCoreFields(coreClass cpCore) {
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
                appendUpgradeLogAddStep(cpCore, cpCore.serverConfig.appConfig.name, MethodName, "Verify core fields in all tables registered in [Tables] content.");
                //
                SQL = "SELECT ccDataSources.Name as DataSourceName, ccDataSources.ID as DataSourceID, ccDataSources.Active as DataSourceActive, ccTables.Name as TableName"
                + " FROM ccTables LEFT JOIN ccDataSources ON ccTables.DataSourceID = ccDataSources.ID"
                + " Where (((ccTables.active) <> 0))"
                + " ORDER BY ccDataSources.Name, ccTables.Name;";
                dt = cpCore.db.executeQuery(SQL);
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
                        cpCore.db.createSQLTable(DataSourceName, genericController.encodeText(dt.Rows[ptr]["Tablename"]));
                    }
                    ptr += 1;
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //=========================================================================================
        //
        //=========================================================================================
        //
        public static void VerifyScriptingRecords(coreClass cpCore) {
            try {
                //
                appendUpgradeLogAddStep(cpCore, cpCore.serverConfig.appConfig.name, "VerifyScriptingRecords", "Verify Scripting Records.");
                //
                VerifyRecord(cpCore, "Scripting Languages", "VBScript", "", "");
                VerifyRecord(cpCore, "Scripting Languages", "JScript", "", "");
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //=========================================================================================
        //
        //=========================================================================================
        //
        public static void VerifyLanguageRecords(coreClass cpCore) {
            try {
                //
                appendUpgradeLogAddStep(cpCore, cpCore.serverConfig.appConfig.name, "VerifyLanguageRecords", "Verify Language Records.");
                //
                VerifyRecord(cpCore, "Languages", "English", "HTTP_Accept_Language", "'en'");
                VerifyRecord(cpCore, "Languages", "Spanish", "HTTP_Accept_Language", "'es'");
                VerifyRecord(cpCore, "Languages", "French", "HTTP_Accept_Language", "'fr'");
                VerifyRecord(cpCore, "Languages", "Any", "HTTP_Accept_Language", "'any'");
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //   Verify Library Folder records
        //
        private static void VerifyLibraryFolders(coreClass cpCore) {
            try {
                DataTable dt = null;
                //
                appendUpgradeLogAddStep(cpCore, cpCore.serverConfig.appConfig.name, "VerifyLibraryFolders", "Verify Library Folders: Images and Downloads");
                //
                dt = cpCore.db.executeQuery("select id from cclibraryfiles");
                if (dt.Rows.Count == 0) {
                    VerifyRecord(cpCore, "Library Folders", "Images");
                    VerifyRecord(cpCore, "Library Folders", "Downloads");
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
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
        //            Call appendUpgradeLogAddStep(cpcore.app.config.name,"VerifyContentWatchLists", "Verify Content Watch Lists: What's New and What's Related")
        //            '
        //            If Not (False) Then
        //                CS = cpcore.app.csOpen("Content Watch Lists", , "name", , , , , "ID")
        //                If Not cpcore.app.csv_IsCSOK(CS) Then
        //                    Call VerifyRecord( "Content Watch Lists", "What's New", "Active", "1", True)
        //                    Call VerifyRecord( "Content Watch Lists", "What's Related", "Active", "1", True)
        //                End If
        //                Call cpcore.app.csv_CloseCS(CS)
        //            End If
        //            '
        //            Exit Sub
        //            '
        //            ' ----- Error Trap
        //            '
        ////ErrorTrap:
        //            Dim ex As New Exception("todo") : Call HandleClassError(ex, cpcore.app.config.name, "methodNameFPO") ' Err.Number, Err.Source, Err.Description, "VerifyContentWatchLists", True, True)
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
        //        Call appendUpgradeLogAddStep(cpCore.app.config.name, "VerifySurveyQuestionTypes", "Verify Survey Question Types")
        //        '
        //        ' ----- make sure there are enough records
        //        '
        //        TableBad = False
        //        RowsFound = 0
        //        rs = cpCore.app.executeSql("Select ID from ccSurveyQuestionTypes order by id")
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
        //            Call cpCore.app.csv_DeleteTable("Default", "ccSurveyQuestionTypes")
        //            'Call cpcore.app.ExecuteSQL( "Drop table ccSurveyQuestionTypes")
        //            Call cpCore.app.CreateSQLTable("Default", "ccSurveyQuestionTypes")
        //            RowsFound = 0
        //        End If
        //        '
        //        ' ----- Add the number of rows needed
        //        '
        //        RowsNeeded = 3 - RowsFound
        //        If RowsNeeded > 0 Then
        //            CID = cpCore.app.csv_GetContentID("Survey Question Types")
        //            If CID <= 0 Then
        //                '
        //                ' Problem
        //                '
        //                fixme-- cpCore.handleException(New ApplicationException("")) ' -----ignoreInteger, "dll", "Survey Question Types content definition was not found")
        //            Else
        //                Do While RowsNeeded > 0
        //                    Call cpCore.app.executeSql("Insert into ccSurveyQuestionTypes (active,contentcontrolid)values(1," & CID & ")")
        //                    RowsNeeded = RowsNeeded - 1
        //                Loop
        //            End If
        //        End If
        //        '
        //        ' ----- Update the Names of each row
        //        '
        //        Call cpCore.app.executeSql("Update ccSurveyQuestionTypes Set Name='Text Field' where ID=1;")
        //        Call cpCore.app.executeSql("Update ccSurveyQuestionTypes Set Name='Select Dropdown' where ID=2;")
        //        Call cpCore.app.executeSql("Update ccSurveyQuestionTypes Set Name='Radio Buttons' where ID=3;")
        //    Catch ex As Exception
        //        cpCore.handleException(ex);
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
        //                cpCore.handleException(ex);
        //            End Try
        //            Return returnAttr

        //            On Error GoTo ErrorTrap
        //            '
        //            Dim ParentID As Integer
        //            '
        //            If ParentMenuName <> "" Then
        //                ParentID = GetIDBYName("ccMenuEntries", ParentMenuName)
        //                Call cpCore.app.executeSql("Delete from ccMenuEntries where name=" & EncodeSQLText(MenuName) & " and ParentID=" & ParentID)
        //            Else
        //                Call cpCore.app.executeSql("Delete from ccMenuEntries where name=" & EncodeSQLText(MenuName))
        //            End If
        //            '
        //            Exit Function
        //            '
        //            ' ----- Error Trap
        //            '
        ////ErrorTrap:
        //            Dim ex As New Exception("todo") : Call handleClassException(ex, cpCore.app.config.name, "methodNameFPO") ' Err.Number, Err.Source, Err.Description, "DeleteAdminMenu", True, False)
        //        End Function
        //
        //=============================================================================
        //
        //=============================================================================
        //
        private static int GetIDBYName(coreClass cpCore, string TableName, string RecordName) {
            int tempGetIDBYName = 0;
            int returnid = 0;
            try {
                //
                DataTable rs;
                //
                rs = cpCore.db.executeQuery("Select ID from " + TableName + " where name=" + cpCore.db.encodeSQLText(RecordName));
                if (isDataTableOk(rs)) {
                    tempGetIDBYName = genericController.encodeInteger(rs.Rows[0]["ID"]);
                }
                rs.Dispose();
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnid;
        }
        //
        //   Verify Library Folder records
        //
        private static void VerifyLibraryFileTypes(coreClass cpCore) {
            try {
                //
                // Load basic records -- default images are handled in the REsource Library through the /ccLib/config/DefaultValues.txt GetDefaultValue(key) mechanism
                //
                if (cpCore.db.getRecordID("Library File Types", "Image") == 0) {
                    VerifyRecord(cpCore, "Library File Types", "Image", "ExtensionList", "'GIF,JPG,JPE,JPEG,BMP,PNG'", false);
                    VerifyRecord(cpCore, "Library File Types", "Image", "IsImage", "1", false);
                    VerifyRecord(cpCore, "Library File Types", "Image", "IsVideo", "0", false);
                    VerifyRecord(cpCore, "Library File Types", "Image", "IsDownload", "1", false);
                    VerifyRecord(cpCore, "Library File Types", "Image", "IsFlash", "0", false);
                }
                //
                if (cpCore.db.getRecordID("Library File Types", "Video") == 0) {
                    VerifyRecord(cpCore, "Library File Types", "Video", "ExtensionList", "'ASX,AVI,WMV,MOV,MPG,MPEG,MP4,QT,RM'", false);
                    VerifyRecord(cpCore, "Library File Types", "Video", "IsImage", "0", false);
                    VerifyRecord(cpCore, "Library File Types", "Video", "IsVideo", "1", false);
                    VerifyRecord(cpCore, "Library File Types", "Video", "IsDownload", "1", false);
                    VerifyRecord(cpCore, "Library File Types", "Video", "IsFlash", "0", false);
                }
                //
                if (cpCore.db.getRecordID("Library File Types", "Audio") == 0) {
                    VerifyRecord(cpCore, "Library File Types", "Audio", "ExtensionList", "'AIF,AIFF,ASF,CDA,M4A,M4P,MP2,MP3,MPA,WAV,WMA'", false);
                    VerifyRecord(cpCore, "Library File Types", "Audio", "IsImage", "0", false);
                    VerifyRecord(cpCore, "Library File Types", "Audio", "IsVideo", "0", false);
                    VerifyRecord(cpCore, "Library File Types", "Audio", "IsDownload", "1", false);
                    VerifyRecord(cpCore, "Library File Types", "Audio", "IsFlash", "0", false);
                }
                //
                if (cpCore.db.getRecordID("Library File Types", "Word") == 0) {
                    VerifyRecord(cpCore, "Library File Types", "Word", "ExtensionList", "'DOC'", false);
                    VerifyRecord(cpCore, "Library File Types", "Word", "IsImage", "0", false);
                    VerifyRecord(cpCore, "Library File Types", "Word", "IsVideo", "0", false);
                    VerifyRecord(cpCore, "Library File Types", "Word", "IsDownload", "1", false);
                    VerifyRecord(cpCore, "Library File Types", "Word", "IsFlash", "0", false);
                }
                //
                if (cpCore.db.getRecordID("Library File Types", "Flash") == 0) {
                    VerifyRecord(cpCore, "Library File Types", "Flash", "ExtensionList", "'SWF'", false);
                    VerifyRecord(cpCore, "Library File Types", "Flash", "IsImage", "0", false);
                    VerifyRecord(cpCore, "Library File Types", "Flash", "IsVideo", "0", false);
                    VerifyRecord(cpCore, "Library File Types", "Flash", "IsDownload", "1", false);
                    VerifyRecord(cpCore, "Library File Types", "Flash", "IsFlash", "1", false);
                }
                //
                if (cpCore.db.getRecordID("Library File Types", "PDF") == 0) {
                    VerifyRecord(cpCore, "Library File Types", "PDF", "ExtensionList", "'PDF'", false);
                    VerifyRecord(cpCore, "Library File Types", "PDF", "IsImage", "0", false);
                    VerifyRecord(cpCore, "Library File Types", "PDF", "IsVideo", "0", false);
                    VerifyRecord(cpCore, "Library File Types", "PDF", "IsDownload", "1", false);
                    VerifyRecord(cpCore, "Library File Types", "PDF", "IsFlash", "0", false);
                }
                //
                if (cpCore.db.getRecordID("Library File Types", "XLS") == 0) {
                    VerifyRecord(cpCore, "Library File Types", "Excel", "ExtensionList", "'XLS'", false);
                    VerifyRecord(cpCore, "Library File Types", "Excel", "IsImage", "0", false);
                    VerifyRecord(cpCore, "Library File Types", "Excel", "IsVideo", "0", false);
                    VerifyRecord(cpCore, "Library File Types", "Excel", "IsDownload", "1", false);
                    VerifyRecord(cpCore, "Library File Types", "Excel", "IsFlash", "0", false);
                }
                //
                if (cpCore.db.getRecordID("Library File Types", "PPT") == 0) {
                    VerifyRecord(cpCore, "Library File Types", "Power Point", "ExtensionList", "'PPT,PPS'", false);
                    VerifyRecord(cpCore, "Library File Types", "Power Point", "IsImage", "0", false);
                    VerifyRecord(cpCore, "Library File Types", "Power Point", "IsVideo", "0", false);
                    VerifyRecord(cpCore, "Library File Types", "Power Point", "IsDownload", "1", false);
                    VerifyRecord(cpCore, "Library File Types", "Power Point", "IsFlash", "0", false);
                }
                //
                if (cpCore.db.getRecordID("Library File Types", "Default") == 0) {
                    VerifyRecord(cpCore, "Library File Types", "Default", "ExtensionList", "''", false);
                    VerifyRecord(cpCore, "Library File Types", "Default", "IsImage", "0", false);
                    VerifyRecord(cpCore, "Library File Types", "Default", "IsVideo", "0", false);
                    VerifyRecord(cpCore, "Library File Types", "Default", "IsDownload", "1", false);
                    VerifyRecord(cpCore, "Library File Types", "Default", "IsFlash", "0", false);
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //=========================================================================================
        //
        //=========================================================================================
        //
        private static void VerifyState(coreClass cpcore, string Name, string Abbreviation, double SaleTax, int CountryID, string FIPSState) {
            try {
                //
                int CS = 0;
                const string ContentName = "States";
                //
                CS = cpcore.db.csOpen(ContentName, "name=" + cpcore.db.encodeSQLText(Name),"", false);
                if (!cpcore.db.csOk(CS)) {
                    //
                    // create new record
                    //
                    cpcore.db.csClose(ref CS);
                    CS = cpcore.db.csInsertRecord(ContentName, SystemMemberID);
                    cpcore.db.csSet(CS, "NAME", Name);
                    cpcore.db.csSet(CS, "ACTIVE", true);
                    cpcore.db.csSet(CS, "Abbreviation", Abbreviation);
                    cpcore.db.csSet(CS, "CountryID", CountryID);
                    cpcore.db.csSet(CS, "FIPSState", FIPSState);
                } else {
                    //
                    // verify only fields needed for contensive
                    //
                    cpcore.db.csSet(CS, "CountryID", CountryID);
                    cpcore.db.csSet(CS, "Abbreviation", Abbreviation);
                }
                cpcore.db.csClose(ref CS);
            } catch (Exception ex) {
                cpcore.handleException(ex);
                throw;
            }
        }
        //
        //=========================================================================================
        //
        //=========================================================================================
        //
        public static void VerifyStates(coreClass cpCore) {
            try {
                //
                appendUpgradeLogAddStep(cpCore, cpCore.serverConfig.appConfig.name, "VerifyStates", "Verify States");
                //
                int CountryID = 0;
                //
                VerifyCountry(cpCore, "United States", "US");
                CountryID = cpCore.db.getRecordID("Countries", "United States");
                //
                VerifyState(cpCore, "Alaska", "AK", 0.0D, CountryID, "");
                VerifyState(cpCore, "Alabama", "AL", 0.0D, CountryID, "");
                VerifyState(cpCore, "Arizona", "AZ", 0.0D, CountryID, "");
                VerifyState(cpCore, "Arkansas", "AR", 0.0D, CountryID, "");
                VerifyState(cpCore, "California", "CA", 0.0D, CountryID, "");
                VerifyState(cpCore, "Connecticut", "CT", 0.0D, CountryID, "");
                VerifyState(cpCore, "Colorado", "CO", 0.0D, CountryID, "");
                VerifyState(cpCore, "Delaware", "DE", 0.0D, CountryID, "");
                VerifyState(cpCore, "District of Columbia", "DC", 0.0D, CountryID, "");
                VerifyState(cpCore, "Florida", "FL", 0.0D, CountryID, "");
                VerifyState(cpCore, "Georgia", "GA", 0.0D, CountryID, "");

                VerifyState(cpCore, "Hawaii", "HI", 0.0D, CountryID, "");
                VerifyState(cpCore, "Idaho", "ID", 0.0D, CountryID, "");
                VerifyState(cpCore, "Illinois", "IL", 0.0D, CountryID, "");
                VerifyState(cpCore, "Indiana", "IN", 0.0D, CountryID, "");
                VerifyState(cpCore, "Iowa", "IA", 0.0D, CountryID, "");
                VerifyState(cpCore, "Kansas", "KS", 0.0D, CountryID, "");
                VerifyState(cpCore, "Kentucky", "KY", 0.0D, CountryID, "");
                VerifyState(cpCore, "Louisiana", "LA", 0.0D, CountryID, "");
                VerifyState(cpCore, "Massachusetts", "MA", 0.0D, CountryID, "");
                VerifyState(cpCore, "Maine", "ME", 0.0D, CountryID, "");

                VerifyState(cpCore, "Maryland", "MD", 0.0D, CountryID, "");
                VerifyState(cpCore, "Michigan", "MI", 0.0D, CountryID, "");
                VerifyState(cpCore, "Minnesota", "MN", 0.0D, CountryID, "");
                VerifyState(cpCore, "Missouri", "MO", 0.0D, CountryID, "");
                VerifyState(cpCore, "Mississippi", "MS", 0.0D, CountryID, "");
                VerifyState(cpCore, "Montana", "MT", 0.0D, CountryID, "");
                VerifyState(cpCore, "North Carolina", "NC", 0.0D, CountryID, "");
                VerifyState(cpCore, "Nebraska", "NE", 0.0D, CountryID, "");
                VerifyState(cpCore, "New Hampshire", "NH", 0.0D, CountryID, "");
                VerifyState(cpCore, "New Mexico", "NM", 0.0D, CountryID, "");

                VerifyState(cpCore, "New Jersey", "NJ", 0.0D, CountryID, "");
                VerifyState(cpCore, "New York", "NY", 0.0D, CountryID, "");
                VerifyState(cpCore, "Nevada", "NV", 0.0D, CountryID, "");
                VerifyState(cpCore, "North Dakota", "ND", 0.0D, CountryID, "");
                VerifyState(cpCore, "Ohio", "OH", 0.0D, CountryID, "");
                VerifyState(cpCore, "Oklahoma", "OK", 0.0D, CountryID, "");
                VerifyState(cpCore, "Oregon", "OR", 0.0D, CountryID, "");
                VerifyState(cpCore, "Pennsylvania", "PA", 0.0D, CountryID, "");
                VerifyState(cpCore, "Rhode Island", "RI", 0.0D, CountryID, "");
                VerifyState(cpCore, "South Carolina", "SC", 0.0D, CountryID, "");

                VerifyState(cpCore, "South Dakota", "SD", 0.0D, CountryID, "");
                VerifyState(cpCore, "Tennessee", "TN", 0.0D, CountryID, "");
                VerifyState(cpCore, "Texas", "TX", 0.0D, CountryID, "");
                VerifyState(cpCore, "Utah", "UT", 0.0D, CountryID, "");
                VerifyState(cpCore, "Vermont", "VT", 0.0D, CountryID, "");
                VerifyState(cpCore, "Virginia", "VA", 0.045, CountryID, "");
                VerifyState(cpCore, "Washington", "WA", 0.0D, CountryID, "");
                VerifyState(cpCore, "Wisconsin", "WI", 0.0D, CountryID, "");
                VerifyState(cpCore, "West Virginia", "WV", 0.0D, CountryID, "");
                VerifyState(cpCore, "Wyoming", "WY", 0.0D, CountryID, "");
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        // Get the Menu for FormInputHTML
        //
        private static void VerifyCountry(coreClass cpCore, string Name, string Abbreviation) {
            try {
                int CS;
                //
                CS = cpCore.db.csOpen("Countries", "name=" + cpCore.db.encodeSQLText(Name));
                if (!cpCore.db.csOk(CS)) {
                    cpCore.db.csClose(ref CS);
                    CS = cpCore.db.csInsertRecord("Countries", SystemMemberID);
                    if (cpCore.db.csOk(CS)) {
                        cpCore.db.csSet(CS, "ACTIVE", true);
                    }
                }
                if (cpCore.db.csOk(CS)) {
                    cpCore.db.csSet(CS, "NAME", Name);
                    cpCore.db.csSet(CS, "Abbreviation", Abbreviation);
                    if (genericController.vbLCase(Name) == "united states") {
                        cpCore.db.csSet(CS, "DomesticShipping", "1");
                    }
                }
                cpCore.db.csClose(ref CS);
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //=========================================================================================
        //
        //=========================================================================================
        //
        public static void VerifyCountries(coreClass cpCore) {
            try {
                //
                appendUpgradeLogAddStep(cpCore, cpCore.serverConfig.appConfig.name, "VerifyCountries", "Verify Countries");
                //
                string list = cpCore.appRootFiles.readFile("cclib\\config\\isoCountryList.txt");
                string[] rows  = genericController.stringSplit(list, "\r\n");
                foreach( var row in rows) {
                    if (!string.IsNullOrEmpty(row)) {
                        string[] attrs = row.Split(';');
                        foreach (var attr in attrs) {
                            VerifyCountry(cpCore, EncodeInitialCaps(attr), attrs[1]);
                        }
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //=========================================================================================
        //
        //=========================================================================================
        //
        public static void VerifyDefaultGroups(coreClass cpCore) {
            try {
                //
                int GroupID = 0;
                string SQL = null;
                //
                appendUpgradeLogAddStep(cpCore, cpCore.serverConfig.appConfig.name, "VerifyDefaultGroups", "Verify Default Groups");
                //
                GroupID = groupController.group_add(cpCore, "Site Managers");
                SQL = "Update ccContent Set EditorGroupID=" + cpCore.db.encodeSQLNumber(GroupID) + " where EditorGroupID is null;";
                cpCore.db.executeQuery(SQL);
            } catch (Exception ex) {
                cpCore.handleException(ex);
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
        //        Call UpgradeCDef_BuildDbFromCollection(CollectionWorking, IISResetRequired, cpCore.app.DataBuildVersion_DontUseThis)
        //        '
        //        ' IISReset if needed
        //        '
        //        If IISResetRequired Then
        //            '
        //            ' Restart IIS if stopped
        //            '
        //            runAtServer = New runAtServerClass(cpCore)
        //            Call runAtServer.executeCmd("IISReset", "")
        //        End If
        //        '
        //        ' Clear the CDef folder
        //        '
        //        Call cpCore.app.publicFiles.DeleteFileFolder(FilePath)
        //        Call cpCore.app.publicFiles.createPath(FilePath)
        //    Catch ex As Exception
        //        cpCore.handleException(ex);
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
        //        CS = cpcore.app.csOpen(cnNavigatorEntries, "name=" & encodeSQLText(ParentName), "ID", , , , , "ID")
        //        If Not cpcore.app.csv_IsCSOK(CS) Then
        //            Call cpcore.app.csv_CloseCS(CS)
        //            CS = cpcore.app.csv_InsertCSRecord(cnNavigatorEntries)
        //        End If
        //        If cpcore.app.csv_IsCSOK(CS) Then
        //            ParentID = cpcore.app.csv_cs_getInteger(CS, "ID")
        //        End If
        //    End If
        //    CS = cpcore.app.csOpen(cnNavigatorEntries, "name=" & encodeSQLText(EntryName), "ID", , , , , "ID,Name,ParentID,AddonID")
        //    If Not cpcore.app.csv_IsCSOK(CS) Then
        //        Call cpcore.app.csv_CloseCS(CS)
        //        CS = cpcore.app.csv_InsertCSRecord(cnNavigatorEntries)
        //    End If
        //    If cpcore.app.csv_IsCSOK(CS) Then
        //        Call cpcore.app.csv_SetCS(CS, "Name", EntryName)
        //        Call cpcore.app.csv_SetCS(CS, "ParentID", ParentID)
        //        Call cpcore.app.csv_SetCS(CS, "AddonID", AddonID)
        //    End If
        //    Call cpcore.app.csv_CloseCS(CS)
        //    '
        //    Exit Sub
        ////ErrorTrap:
        //    dim ex as new exception("todo"): Call HandleClassError(ex,cpcore.app.appEnvironment.name,Err.Number, Err.Source, Err.Description, "SetNavigatorEntry", True, False)
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
        //        CS = cpcore.app.csOpen(cnNavigatorEntries, "NavGuid=" & encodeSQLText(ParentGuid), "ID", , , , , "ID")
        //        If cpcore.app.csv_IsCSOK(CS) Then
        //            ParentID = cpcore.app.csv_cs_getInteger(CS, "ID")
        //        End If
        //        Call cpcore.app.csv_CloseCS(CS)
        //    End If
        //    If ParentID > 0 Then
        //        CS = cpcore.app.csOpen(cnNavigatorEntries, "(parentid=" & ParentID & ")and(name=" & encodeSQLText(EntryName) & ")", "ID", , , , , "ID,Name,ParentID,AddonID")
        //        If Not cpcore.app.csv_IsCSOK(CS) Then
        //            Call cpcore.app.csv_CloseCS(CS)
        //            CS = cpcore.app.csv_InsertCSRecord(cnNavigatorEntries)
        //        End If
        //        If cpcore.app.csv_IsCSOK(CS) Then
        //            Call cpcore.app.csv_SetCS(CS, "Name", EntryName)
        //            Call cpcore.app.csv_SetCS(CS, "ParentID", ParentID)
        //            Call cpcore.app.csv_SetCS(CS, "AddonID", AddonID)
        //            If true Then
        //                Call cpcore.app.csv_SetCS(CS, "NavIconTypeID", NavIconTypeID)
        //                Call cpcore.app.csv_SetCS(CS, "NavIconTitle", NavIconTitle)
        //            End If
        //        End If
        //        Call cpcore.app.csv_CloseCS(CS)
        //    End If
        //    '
        //    Exit Sub
        ////ErrorTrap:
        //    dim ex as new exception("todo"): Call HandleClassError(ex,cpcore.app.appEnvironment.name,Err.Number, Err.Source, Err.Description, "SetNavigatorEntry", True, False)
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
        //        Dim addonInstall As New addonInstallClass(cpCore)
        //        Dim saveLogFolder As String
        //        '
        //        InstallAddons = False
        //        '
        //        saveLogFolder = classLogFolder
        //        InstallAddons = addonInstall.InstallCollectionFromPrivateFolder(Me, buildVersion, "Install\", IISResetRequired, cpCore.app.config.name, "", "", IsNewBuild)
        //        classLogFolder = saveLogFolder
        //        '
        //        ' IISReset if needed
        //        '
        //        If IISResetRequired Then
        //            runAtServer = New runAtServerClass(cpCore)
        //            Call runAtServer.executeCmd("IISReset", "")
        //        End If
        //    Catch ex As Exception
        //        cpCore.handleException(ex);
        //    End Try
        //    Return returnOk
        //End Function
        //
        //
        ///
        //Public Shared Sub ReplaceAddonWithCollection(ByVal AddonProgramID As String, ByVal CollectionGuid As String, ByRef return_IISResetRequired As Boolean, ByRef return_RegisterList As String)
        //    Dim ex As New Exception("todo") : Call handleClassException(cpCore, ex, cpCore.serverConfig.appConfig.name, "methodNameFPO") ' ignoreInteger, "dll", "builderClass.ReplaceAddonWithCollection is deprecated", "ReplaceAddonWithCollection", True, True)
        //End Sub
        //    On Error GoTo ErrorTrap
        //    '
        //    Dim CS As Integer
        //    Dim ErrorMessage As String
        //    Dim addonInstall As addonInstallClass
        //    '
        //    CS = cpcore.app.csOpen(cnAddons, "objectProgramID=" & encodeSQLText(AddonProgramID))
        //    If cpcore.app.csv_IsCSOK(CS) Then
        //        Call cpcore.app.csv_DeleteCSRecord(CS)
        //        InstallCollectionList = InstallCollectionList & "," & CollectionGuid
        //        'Set AddonInstall = New AddonInstallClass
        //        'If Not AddonInstall.UpgradeAllAppsFromLibCollection2(CollectionGuid, cpcore.app.appEnvironment.name, Return_IISResetRequired, Return_RegisterList, ErrorMessage) Then
        //        'End If
        //    End If
        //    Call cpcore.app.csv_CloseCS(CS)
        //    '
        //    Exit Sub
        //    '
        //    ' ----- Error Trap
        //    '
        ////ErrorTrap:
        //    dim ex as new exception("todo"): Call HandleClassError(ex,cpcore.app.appEnvironment.name, Err.Number, Err.Source, Err.Description, "ReplaceAddonWithCollection", True, True)
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
        internal static void VerifyBasicTables(coreClass cpCore) {
            try {
                //
                if (!false) {
                    appendUpgradeLogAddStep(cpCore, cpCore.serverConfig.appConfig.name, "VerifyCoreTables", "Verify Core SQL Tables");
                    //
                    cpCore.db.createSQLTable("Default", "ccDataSources");
                    cpCore.db.createSQLTableField("Default", "ccDataSources", "typeId", FieldTypeIdInteger);
                    cpCore.db.createSQLTableField("Default", "ccDataSources", "address", FieldTypeIdText);
                    cpCore.db.createSQLTableField("Default", "ccDataSources", "username", FieldTypeIdText);
                    cpCore.db.createSQLTableField("Default", "ccDataSources", "password", FieldTypeIdText);
                    cpCore.db.createSQLTableField("Default", "ccDataSources", "ConnString", FieldTypeIdText);
                    cpCore.db.createSQLTableField("Default", "ccDataSources", "endpoint", FieldTypeIdText);
                    cpCore.db.createSQLTableField("Default", "ccDataSources", "dbtypeid", FieldTypeIdLookup);
                    //
                    cpCore.db.createSQLTable("Default", "ccTables");
                    cpCore.db.createSQLTableField("Default", "ccTables", "DataSourceID", FieldTypeIdLookup);
                    //
                    cpCore.db.createSQLTable("Default", "ccContent");
                    cpCore.db.createSQLTableField("Default", "ccContent", "ContentTableID", FieldTypeIdInteger);
                    cpCore.db.createSQLTableField("Default", "ccContent", "AuthoringTableID", FieldTypeIdInteger);
                    cpCore.db.createSQLTableField("Default", "ccContent", "AllowAdd", FieldTypeIdBoolean);
                    cpCore.db.createSQLTableField("Default", "ccContent", "AllowDelete", FieldTypeIdBoolean);
                    cpCore.db.createSQLTableField("Default", "ccContent", "AllowWorkflowAuthoring", FieldTypeIdBoolean);
                    cpCore.db.createSQLTableField("Default", "ccContent", "DeveloperOnly", FieldTypeIdBoolean);
                    cpCore.db.createSQLTableField("Default", "ccContent", "AdminOnly", FieldTypeIdBoolean);
                    cpCore.db.createSQLTableField("Default", "ccContent", "ParentID", FieldTypeIdInteger);
                    cpCore.db.createSQLTableField("Default", "ccContent", "DefaultSortMethodID", FieldTypeIdInteger);
                    cpCore.db.createSQLTableField("Default", "ccContent", "DropDownFieldList", FieldTypeIdText);
                    cpCore.db.createSQLTableField("Default", "ccContent", "EditorGroupID", FieldTypeIdInteger);
                    cpCore.db.createSQLTableField("Default", "ccContent", "AllowCalendarEvents", FieldTypeIdBoolean);
                    cpCore.db.createSQLTableField("Default", "ccContent", "AllowContentTracking", FieldTypeIdBoolean);
                    cpCore.db.createSQLTableField("Default", "ccContent", "AllowTopicRules", FieldTypeIdBoolean);
                    cpCore.db.createSQLTableField("Default", "ccContent", "AllowContentChildTool", FieldTypeIdBoolean);
                    //Call cpCore.db.createSQLTableField("Default", "ccContent", "AllowMetaContent", FieldTypeIdBoolean)
                    cpCore.db.createSQLTableField("Default", "ccContent", "IconLink", FieldTypeIdLink);
                    cpCore.db.createSQLTableField("Default", "ccContent", "IconHeight", FieldTypeIdInteger);
                    cpCore.db.createSQLTableField("Default", "ccContent", "IconWidth", FieldTypeIdInteger);
                    cpCore.db.createSQLTableField("Default", "ccContent", "IconSprites", FieldTypeIdInteger);
                    cpCore.db.createSQLTableField("Default", "ccContent", "installedByCollectionId", FieldTypeIdInteger);
                    //Call cpCore.app.csv_CreateSQLTableField("Default", "ccContent", "ccGuid", FieldTypeText)
                    cpCore.db.createSQLTableField("Default", "ccContent", "IsBaseContent", FieldTypeIdBoolean);
                    //Call cpcore.app.csv_CreateSQLTableField("Default", "ccContent", "WhereClause", FieldTypeText)
                    //
                    cpCore.db.createSQLTable("Default", "ccFields");
                    cpCore.db.createSQLTableField("Default", "ccFields", "ContentID", FieldTypeIdInteger);
                    cpCore.db.createSQLTableField("Default", "ccFields", "Type", FieldTypeIdInteger);
                    cpCore.db.createSQLTableField("Default", "ccFields", "Caption", FieldTypeIdText);
                    cpCore.db.createSQLTableField("Default", "ccFields", "ReadOnly", FieldTypeIdBoolean);
                    cpCore.db.createSQLTableField("Default", "ccFields", "NotEditable", FieldTypeIdBoolean);
                    cpCore.db.createSQLTableField("Default", "ccFields", "LookupContentID", FieldTypeIdInteger);
                    cpCore.db.createSQLTableField("Default", "ccFields", "RedirectContentID", FieldTypeIdInteger);
                    cpCore.db.createSQLTableField("Default", "ccFields", "RedirectPath", FieldTypeIdText);
                    cpCore.db.createSQLTableField("Default", "ccFields", "RedirectID", FieldTypeIdText);
                    cpCore.db.createSQLTableField("Default", "ccFields", "HelpMessage", FieldTypeIdLongText); // deprecated but Im chicken to remove this
                    cpCore.db.createSQLTableField("Default", "ccFields", "UniqueName", FieldTypeIdBoolean);
                    cpCore.db.createSQLTableField("Default", "ccFields", "TextBuffered", FieldTypeIdBoolean);
                    cpCore.db.createSQLTableField("Default", "ccFields", "Password", FieldTypeIdBoolean);
                    cpCore.db.createSQLTableField("Default", "ccFields", "IndexColumn", FieldTypeIdInteger);
                    cpCore.db.createSQLTableField("Default", "ccFields", "IndexWidth", FieldTypeIdText);
                    cpCore.db.createSQLTableField("Default", "ccFields", "IndexSortPriority", FieldTypeIdInteger);
                    cpCore.db.createSQLTableField("Default", "ccFields", "IndexSortDirection", FieldTypeIdInteger);
                    cpCore.db.createSQLTableField("Default", "ccFields", "EditSortPriority", FieldTypeIdInteger);
                    cpCore.db.createSQLTableField("Default", "ccFields", "AdminOnly", FieldTypeIdBoolean);
                    cpCore.db.createSQLTableField("Default", "ccFields", "DeveloperOnly", FieldTypeIdBoolean);
                    cpCore.db.createSQLTableField("Default", "ccFields", "DefaultValue", FieldTypeIdText);
                    cpCore.db.createSQLTableField("Default", "ccFields", "Required", FieldTypeIdBoolean);
                    cpCore.db.createSQLTableField("Default", "ccFields", "HTMLContent", FieldTypeIdBoolean);
                    cpCore.db.createSQLTableField("Default", "ccFields", "Authorable", FieldTypeIdBoolean);
                    cpCore.db.createSQLTableField("Default", "ccFields", "ManyToManyContentID", FieldTypeIdInteger);
                    cpCore.db.createSQLTableField("Default", "ccFields", "ManyToManyRuleContentID", FieldTypeIdInteger);
                    cpCore.db.createSQLTableField("Default", "ccFields", "ManyToManyRulePrimaryField", FieldTypeIdText);
                    cpCore.db.createSQLTableField("Default", "ccFields", "ManyToManyRuleSecondaryField", FieldTypeIdText);
                    cpCore.db.createSQLTableField("Default", "ccFields", "RSSTitleField", FieldTypeIdBoolean);
                    cpCore.db.createSQLTableField("Default", "ccFields", "RSSDescriptionField", FieldTypeIdBoolean);
                    cpCore.db.createSQLTableField("Default", "ccFields", "MemberSelectGroupID", FieldTypeIdInteger);
                    cpCore.db.createSQLTableField("Default", "ccFields", "EditTab", FieldTypeIdText);
                    cpCore.db.createSQLTableField("Default", "ccFields", "Scramble", FieldTypeIdBoolean);
                    cpCore.db.createSQLTableField("Default", "ccFields", "LookupList", FieldTypeIdText);
                    cpCore.db.createSQLTableField("Default", "ccFields", "IsBaseField", FieldTypeIdBoolean);
                    cpCore.db.createSQLTableField("Default", "ccFields", "installedByCollectionId", FieldTypeIdInteger);
                    //
                    cpCore.db.createSQLTable("Default", "ccFieldHelp");
                    cpCore.db.createSQLTableField("Default", "ccFieldHelp", "FieldID", FieldTypeIdLookup);
                    cpCore.db.createSQLTableField("Default", "ccFieldHelp", "HelpDefault", FieldTypeIdLongText);
                    cpCore.db.createSQLTableField("Default", "ccFieldHelp", "HelpCustom", FieldTypeIdLongText);
                    //
                    cpCore.db.createSQLTable("Default", "ccSetup");
                    cpCore.db.createSQLTableField("Default", "ccSetup", "FieldValue", FieldTypeIdText);
                    cpCore.db.createSQLTableField("Default", "ccSetup", "DeveloperOnly", FieldTypeIdBoolean);
                    //
                    cpCore.db.createSQLTable("Default", "ccSortMethods");
                    cpCore.db.createSQLTableField("Default", "ccSortMethods", "OrderByClause", FieldTypeIdText);
                    //
                    cpCore.db.createSQLTable("Default", "ccFieldTypes");
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }

        //
        //===========================================================================
        //   Error handler
        //===========================================================================
        //
        private static void handleClassException(coreClass cpCore, Exception ex, string ApplicationName, string MethodName) {
            logController.appendLog(cpCore, "exception in builderClass." + MethodName + ", application [" + ApplicationName + "], ex [" + ex.ToString() + "]");
        }
        //
        //===========================================================================
        //   Append Log File
        //===========================================================================
        //
        private static void appendUpgradeLog(coreClass cpCore, string appName, string Method, string Message) {
            logController.appendLogInstall(cpCore, "app [" + appName + "], Method [" + Method + "], Message [" + Message + "]");
        }
        //
        //=============================================================================
        //   Get a ContentID from the ContentName using just the tables
        //=============================================================================
        //
        private static void appendUpgradeLogAddStep(coreClass cpCore, string appName, string Method, string Message) {
            appendUpgradeLog(cpCore, appName, Method, Message);
        }
        //
        //=====================================================================================================
        //   a value in a name/value pair
        //=====================================================================================================
        //
        public static void SetNameValueArrays(coreClass cpCore, string InputName, string InputValue, ref string[] SQLName, ref string[] SQLValue, ref int Index) {
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
        //        cpCore.handleException(ex);
        //    End Try
        //End Sub '

        // -- deprecated
        //Public Shared Sub admin_VerifyMenuEntry(cpCore As coreClass, ByVal ParentName As String, ByVal EntryName As String, ByVal ContentName As String, ByVal LinkPage As String, ByVal SortOrder As String, ByVal AdminOnly As Boolean, ByVal DeveloperOnly As Boolean, ByVal NewWindow As Boolean, ByVal Active As Boolean, ByVal MenuContentName As String, ByVal AddonName As String)
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
        //        SupportAddonID = Models.Complex.cdefModel.isContentFieldSupported(cpcore,MenuContentName, "AddonID")
        //        '
        //        ' Get AddonID from AddonName
        //        '
        //        addonId = 0
        //        If Not SupportAddonID Then
        //            SelectList = SelectList & ",0 as AddonID"
        //        Else
        //            SelectList = SelectList & ",AddonID"
        //            If AddonName <> "" Then
        //                CS = cpCore.db.cs_open(AddonContentName, "name=" & cpCore.db.encodeSQLText(AddonName), "ID", False, , , , "ID", 1)
        //                If cpCore.db.cs_ok(CS) Then
        //                    addonId = (cpCore.db.cs_getInteger(CS, "ID"))
        //                End If
        //                Call cpCore.db.cs_Close(CS)
        //            End If
        //        End If
        //        '
        //        ' Get ParentID from ParentName
        //        '
        //        ParentID = 0
        //        If ParentName <> "" Then
        //            CS = cpCore.db.cs_open(MenuContentName, "name=" & cpCore.db.encodeSQLText(ParentName), "ID", False, , , , "ID", 1)
        //            If cpCore.db.cs_ok(CS) Then
        //                ParentID = (cpCore.db.cs_getInteger(CS, "ID"))
        //            End If
        //            Call cpCore.db.cs_Close(CS)
        //        End If
        //        '
        //        ' Set ContentID from ContentName
        //        '
        //        ContentID = -1
        //        If ContentName <> "" Then
        //            ContentID = Models.Complex.cdefModel.getContentId(cpcore,ContentName)
        //        End If
        //        '
        //        ' Locate current entry
        //        '
        //        CSEntry = cpCore.db.cs_open(MenuContentName, "(name=" & cpCore.db.encodeSQLText(EntryName) & ")", "ID", False, , , , SelectList)
        //        '
        //        ' If no current entry, create one
        //        '
        //        If Not cpCore.db.cs_ok(CSEntry) Then
        //            cpCore.db.cs_Close(CSEntry)
        //            CSEntry = cpCore.db.cs_insertRecord(MenuContentName, SystemMemberID)
        //            If cpCore.db.cs_ok(CSEntry) Then
        //                Call cpCore.db.cs_set(CSEntry, "name", EntryName)
        //            End If
        //        End If
        //        If cpCore.db.cs_ok(CSEntry) Then
        //            If ParentID = 0 Then
        //                Call cpCore.db.cs_set(CSEntry, "ParentID", 0)
        //            Else
        //                Call cpCore.db.cs_set(CSEntry, "ParentID", ParentID)
        //            End If
        //            If (ContentID = -1) Then
        //                Call cpCore.db.cs_set(CSEntry, "ContentID", 0)
        //            Else
        //                Call cpCore.db.cs_set(CSEntry, "ContentID", ContentID)
        //            End If
        //            Call cpCore.db.cs_set(CSEntry, "LinkPage", LinkPage)
        //            Call cpCore.db.cs_set(CSEntry, "SortOrder", SortOrder)
        //            Call cpCore.db.cs_set(CSEntry, "AdminOnly", AdminOnly)
        //            Call cpCore.db.cs_set(CSEntry, "DeveloperOnly", DeveloperOnly)
        //            Call cpCore.db.cs_set(CSEntry, "NewWindow", NewWindow)
        //            Call cpCore.db.cs_set(CSEntry, "Active", Active)
        //            If SupportAddonID Then
        //                Call cpCore.db.cs_set(CSEntry, "AddonID", addonId)
        //            End If
        //        End If
        //        Call cpCore.db.cs_Close(CSEntry)
        //    Catch ex As Exception
        //        cpCore.handleExceptionAndContinue(ex) : Throw
        //    End Try
        //End Sub
        //
        //=============================================================================
        //   Verify an Admin Menu Entry
        //       Entries are unique by their name
        //=============================================================================
        //
        //Public Shared Sub admin_VerifyAdminMenu(cpCore As coreClass, ByVal ParentName As String, ByVal EntryName As String, ByVal ContentName As String, ByVal LinkPage As String, ByVal SortOrder As String, Optional ByVal AdminOnly As Boolean = False, Optional ByVal DeveloperOnly As Boolean = False, Optional ByVal NewWindow As Boolean = False, Optional ByVal Active As Boolean = True)

        //    'Call admin_VerifyMenuEntry(cpCore, ParentName, EntryName, ContentName, LinkPage, SortOrder, AdminOnly, DeveloperOnly, NewWindow, Active, cnNavigatorEntries, "")
        //End Sub
        //
        //=============================================================================
        //   Verify an Admin Navigator Entry
        //       Entries are unique by their ccGuid
        //       Includes InstalledByCollectionID
        //       returns the entry id
        //=============================================================================
        //
        public static int verifyNavigatorEntry(coreClass cpCore, string ccGuid, string menuNameSpace, string EntryName, string ContentName, string LinkPage, string SortOrder, bool AdminOnly, bool DeveloperOnly, bool NewWindow, bool Active, string AddonName, string NavIconType, string NavIconTitle, int InstalledByCollectionID) {
            int returnEntry = 0;
            try {
                if (!string.IsNullOrEmpty(EntryName.Trim())) {
                    int addonId = cpCore.db.getRecordID(cnAddons, AddonName);
                    int parentId = verifyNavigatorEntry_getParentIdFromNameSpace(cpCore, menuNameSpace);
                    int contentId = Models.Complex.cdefModel.getContentId(cpCore, ContentName);
                    string listCriteria = "(name=" + cpCore.db.encodeSQLText(EntryName) + ")and(Parentid=" + parentId + ")";
                    List<Models.Entity.NavigatorEntryModel> entryList = NavigatorEntryModel.createList(cpCore, listCriteria, "id");
                    NavigatorEntryModel entry = null;
                    if (entryList.Count == 0) {
                        entry = NavigatorEntryModel.add(cpCore);
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
                    entry.save(cpCore);
                    returnEntry = entry.id;
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnEntry;
        }
        //Public Shared Function verifyNavigatorEntry(cpCore As coreClass, ByVal ccGuid As String, ByVal menuNameSpace As String, ByVal EntryName As String, ByVal ContentName As String, ByVal LinkPage As String, ByVal SortOrder As String, ByVal AdminOnly As Boolean, ByVal DeveloperOnly As Boolean, ByVal NewWindow As Boolean, ByVal Active As Boolean, ByVal AddonName As String, ByVal NavIconType As String, ByVal NavIconTitle As String, ByVal InstalledByCollectionID As Integer) As Integer
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
        //            SupportAddonID = Models.Complex.cdefModel.isContentFieldSupported(cpcore,cnNavigatorEntries, "AddonID")
        //            SupportInstalledByCollectionID = Models.Complex.cdefModel.isContentFieldSupported(cpcore,cnNavigatorEntries, "InstalledByCollectionID")
        //            If SupportAddonID Then
        //                SelectList = SelectList & ",AddonID"
        //            Else
        //                SelectList = SelectList & ",0 as AddonID"
        //            End If
        //            If SupportInstalledByCollectionID Then
        //                SelectList = SelectList & ",InstalledByCollectionID"
        //            End If
        //            If Models.Complex.cdefModel.isContentFieldSupported(cpcore,cnNavigatorEntries, "ccGuid") Then
        //                SupportGuid = True
        //                SupportccGuid = True
        //                GuidFieldName = "ccguid"
        //                SelectList = SelectList & ",ccGuid"
        //            ElseIf Models.Complex.cdefModel.isContentFieldSupported(cpcore,cnNavigatorEntries, "NavGuid") Then
        //                SupportGuid = True
        //                SupportNavGuid = True
        //                GuidFieldName = "navguid"
        //                SelectList = SelectList & ",NavGuid"
        //            Else
        //                SelectList = SelectList & ",'' as ccGuid"
        //            End If
        //            SupportNavIcon = Models.Complex.cdefModel.isContentFieldSupported(cpcore,cnNavigatorEntries, "NavIconType")
        //            addonId = 0
        //            If SupportAddonID And (AddonName <> "") Then
        //                CS = cpCore.db.cs_open(AddonContentName, "name=" & cpCore.db.encodeSQLText(AddonName), "ID", False, , , , "ID", 1)
        //                If cpCore.db.cs_ok(CS) Then
        //                    addonId = cpCore.db.cs_getInteger(CS, "ID")
        //                End If
        //                Call cpCore.db.cs_Close(CS)
        //            End If
        //            ParentID = getNavigatorEntryParentIDFromNameSpace(cpCore, cnNavigatorEntries, menuNameSpace)
        //            ContentID = -1
        //            If ContentName <> "" Then
        //                ContentID = Models.Complex.cdefModel.getContentId(cpcore,ContentName)
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
        //                    CSEntry = cpCore.db.cs_open(cnNavigatorEntries, "(name=" & cpCore.db.encodeSQLText(EntryName) & ")and(Parentid=" & ParentID & ")and((" & GuidFieldName & " is null)or(" & GuidFieldName & "=''))", "ID", True, , , , SelectList)
        //                Else
        //                    '
        //                    ' ----- Find match by guid
        //                    '
        //                    CSEntry = cpCore.db.cs_open(cnNavigatorEntries, "(" & GuidFieldName & "=" & cpCore.db.encodeSQLText(ccGuid) & ")", "ID", True, , , , SelectList)
        //                End If
        //                If Not cpCore.db.cs_ok(CSEntry) Then
        //                    '
        //                    ' ----- if not found by guid, look for a name/parent match with a blank guid
        //                    '
        //                    Call cpCore.db.cs_Close(CSEntry)
        //                    Criteria = "AND((" & GuidFieldName & " is null)or(" & GuidFieldName & "=''))"
        //                End If
        //            End If
        //            If Not cpCore.db.cs_ok(CSEntry) Then
        //                If ParentID = 0 Then
        //                    ' 12/19/2008 change to ActiveOnly - because if there is a non-guid entry, it is marked inactive. We only want to update the active entries
        //                    Criteria = Criteria & "And(name=" & cpCore.db.encodeSQLText(EntryName) & ")and(ParentID is null)"
        //                Else
        //                    ' 12/19/2008 change to ActiveOnly - because if there is a non-guid entry, it is marked inactive. We only want to update the active entries
        //                    Criteria = Criteria & "And(name=" & cpCore.db.encodeSQLText(EntryName) & ")and(ParentID=" & ParentID & ")"
        //                End If
        //                CSEntry = cpCore.db.cs_open(cnNavigatorEntries, Mid(Criteria, 4), "ID", True, , , , SelectList)
        //            End If
        //            '
        //            ' If no current entry, create one
        //            '
        //            If Not cpCore.db.cs_ok(CSEntry) Then
        //                cpCore.db.cs_Close(CSEntry)
        //                '
        //                ' This entry was not found - insert a new record if there is no other name/menuNameSpace match
        //                '
        //                If False Then
        //                    '
        //                    ' OK - the first entry search was name/menuNameSpace
        //                    '
        //                    DupFound = False
        //                ElseIf ParentID = 0 Then
        //                    CSEntry = cpCore.db.cs_open(cnNavigatorEntries, "(name=" & cpCore.db.encodeSQLText(EntryName) & ")and(ParentID is null)", "ID", False, , , , SelectList)
        //                    DupFound = cpCore.db.cs_ok(CSEntry)
        //                    cpCore.db.cs_Close(CSEntry)
        //                Else
        //                    CSEntry = cpCore.db.cs_open(cnNavigatorEntries, "(name=" & cpCore.db.encodeSQLText(EntryName) & ")and(ParentID=" & ParentID & ")", "ID", False, , , , SelectList)
        //                    DupFound = cpCore.db.cs_ok(CSEntry)
        //                    cpCore.db.cs_Close(CSEntry)
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
        //                    CSEntry = cpCore.db.cs_insertRecord(cnNavigatorEntries, SystemMemberID)
        //                End If
        //            End If
        //            If cpCore.db.cs_ok(CSEntry) Then
        //                EntryID = cpCore.db.cs_getInteger(CSEntry, "ID")
        //                If EntryID = 265 Then
        //                    EntryID = EntryID
        //                End If
        //                Call cpCore.db.cs_set(CSEntry, "name", EntryName)
        //                If ParentID = 0 Then
        //                    Call cpCore.db.cs_set(CSEntry, "ParentID", 0)
        //                Else
        //                    Call cpCore.db.cs_set(CSEntry, "ParentID", ParentID)
        //                End If
        //                If (ContentID = -1) Then
        //                    Call cpCore.db.cs_set(CSEntry, "ContentID", 0)
        //                Else
        //                    Call cpCore.db.cs_set(CSEntry, "ContentID", ContentID)
        //                End If
        //                Call cpCore.db.cs_set(CSEntry, "LinkPage", LinkPage)
        //                Call cpCore.db.cs_set(CSEntry, "SortOrder", SortOrder)
        //                Call cpCore.db.cs_set(CSEntry, "AdminOnly", AdminOnly)
        //                Call cpCore.db.cs_set(CSEntry, "DeveloperOnly", DeveloperOnly)
        //                Call cpCore.db.cs_set(CSEntry, "NewWindow", NewWindow)
        //                Call cpCore.db.cs_set(CSEntry, "Active", Active)
        //                If SupportAddonID Then
        //                    Call cpCore.db.cs_set(CSEntry, "AddonID", addonId)
        //                End If
        //                If SupportGuid Then
        //                    Call cpCore.db.cs_set(CSEntry, GuidFieldName, ccGuid)
        //                End If
        //                If SupportNavIcon Then
        //                    Call cpCore.db.cs_set(CSEntry, "NavIconTitle", NavIconTitle)
        //                    Dim NavIconID As Integer
        //                    NavIconID = GetListIndex(NavIconType, NavIconTypeList)
        //                    Call cpCore.db.cs_set(CSEntry, "NavIconType", NavIconID)
        //                End If
        //                If SupportInstalledByCollectionID Then
        //                    Call cpCore.db.cs_set(CSEntry, "InstalledByCollectionID", InstalledByCollectionID)
        //                End If
        //                '
        //                ' merge any duplicate guid matches
        //                '
        //                Call cpCore.db.cs_goNext(CSEntry)
        //                Do While cpCore.db.cs_ok(CSEntry)
        //                    DuplicateID = cpCore.db.cs_getInteger(CSEntry, "ID")
        //                    Call cpCore.db.executeSql("update ccMenuEntries set ParentID=" & EntryID & " where ParentID=" & DuplicateID)
        //                    Call cpCore.db.deleteContentRecord(cnNavigatorEntries, DuplicateID)
        //                    Call cpCore.db.cs_goNext(CSEntry)
        //                Loop
        //            End If
        //            Call cpCore.db.cs_Close(CSEntry)
        //            '
        //            ' Merge duplicates with menuNameSpace.Name match
        //            '
        //            If EntryID <> 0 Then
        //                If ParentID = 0 Then
        //                    CSEntry = cpCore.db.cs_openCsSql_rev("default", "select * from ccMenuEntries where (parentid is null)and(name=" & cpCore.db.encodeSQLText(EntryName) & ")and(id<>" & EntryID & ")")
        //                Else
        //                    CSEntry = cpCore.db.cs_openCsSql_rev("default", "select * from ccMenuEntries where (parentid=" & ParentID & ")and(name=" & cpCore.db.encodeSQLText(EntryName) & ")and(id<>" & EntryID & ")")
        //                End If
        //                Do While cpCore.db.cs_ok(CSEntry)
        //                    DuplicateID = cpCore.db.cs_getInteger(CSEntry, "ID")
        //                    Call cpCore.db.executeSql("update ccMenuEntries set ParentID=" & EntryID & " where ParentID=" & DuplicateID)
        //                    Call cpCore.db.deleteContentRecord(cnNavigatorEntries, DuplicateID)
        //                    Call cpCore.db.cs_goNext(CSEntry)
        //                Loop
        //                Call cpCore.db.cs_Close(CSEntry)
        //            End If
        //        End If
        //        '
        //        returnEntry = EntryID
        //    Catch ex As Exception
        //        cpCore.handleExceptionAndContinue(ex) : Throw
        //    End Try
        //    Return returnEntry
        //End Function
        //
        //
        //
        public static int verifyNavigatorEntry_getParentIdFromNameSpace(coreClass cpCore, string menuNameSpace) {
            int parentRecordId = 0;
            try {
                if (!string.IsNullOrEmpty(menuNameSpace.Trim())) {
                    string[] parents = menuNameSpace.Trim().Split('.');
                    foreach( var parent in parents ) {
                        string recordName = parent.Trim();
                        if (!string.IsNullOrEmpty(recordName)) {
                            string Criteria = "(name=" + cpCore.db.encodeSQLText(recordName) + ")";
                            if (parentRecordId == 0) {
                                Criteria += "and((Parentid is null)or(Parentid=0))";
                            } else {
                                Criteria += "and(Parentid=" + parentRecordId + ")";
                            }
                            int RecordID = 0;
                            int CS = cpCore.db.csOpen(cnNavigatorEntries, Criteria, "ID", true, 0, false, false, "ID", 1);
                            if (cpCore.db.csOk(CS)) {
                                RecordID = (cpCore.db.csGetInteger(CS, "ID"));
                            }
                            cpCore.db.csClose(ref CS);
                            if (RecordID == 0) {
                                CS = cpCore.db.csInsertRecord(cnNavigatorEntries, SystemMemberID);
                                if (cpCore.db.csOk(CS)) {
                                    RecordID = cpCore.db.csGetInteger(CS, "ID");
                                    cpCore.db.csSet(CS, "name", recordName);
                                    cpCore.db.csSet(CS, "parentID", parentRecordId);
                                }
                                cpCore.db.csClose(ref CS);
                            }
                            parentRecordId = RecordID;
                        }
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return parentRecordId;
        }
    }
}
