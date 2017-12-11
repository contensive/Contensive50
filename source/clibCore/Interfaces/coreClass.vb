
Option Strict On
Option Explicit On

Imports System.Reflection
Imports Contensive.BaseClasses
Imports Contensive.Core.Controllers
Imports Contensive.Core.Models.Complex
Imports Contensive.Core.Models.Context
Imports Contensive.Core.Models.Entity
'
Namespace Contensive.Core
    Public Class coreClass
        Implements IDisposable
        '
        '======================================================================
        '
        ' core 
        '   -- provides object dependancy injection
        '
        Friend Property cp_forAddonExecutionOnly As CPClass
        Public Property serverConfig As serverConfigModel
        '
        '===================================================================================================
        ''' <summary>
        ''' list of DLLs in the addon assembly path that are not adds. As they are discovered, they are added to this list
        ''' and not loaded in the future. The me.dispose compares the list count to the loaded count and caches if different.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property assemblySkipList As List(Of String)
            Get
                If (_assemblySkipList Is Nothing) Then
                    _assemblySkipList = cache.getObject(Of List(Of String))(cacheNameAssemblySkipList)
                    If (_assemblySkipList Is Nothing) Then
                        _assemblySkipList = New List(Of String)
                    End If
                    _assemblySkipList_CountWhenLoaded = _assemblySkipList.Count()
                End If
                Return _assemblySkipList
            End Get
        End Property
        Private _assemblySkipList As List(Of String)
        Private _assemblySkipList_CountWhenLoaded As Integer
        '
        '===================================================================================================
        Public ReadOnly Property dataSourceDictionary() As Dictionary(Of String, dataSourceModel)
            Get
                If (_dataSources Is Nothing) Then
                    _dataSources = dataSourceModel.getNameDict(Me)
                End If
                Return _dataSources
            End Get
        End Property
        Private _dataSources As Dictionary(Of String, dataSourceModel) = Nothing
        '
        '===================================================================================================
        Public ReadOnly Property email As emailController
            Get
                If (_email Is Nothing) Then
                    _email = New emailController(Me)
                End If
                Return _email
            End Get
        End Property
        Private _email As emailController
        '
        '===================================================================================================
        Public ReadOnly Property doc As docController
            Get
                If (_doc Is Nothing) Then
                    _doc = New docController(Me)
                End If
                Return _doc
            End Get
        End Property
        Private _doc As docController
        '
        '===================================================================================================
        Public ReadOnly Property menuTab As menuTabController
            Get
                If (_menuTab Is Nothing) Then
                    _menuTab = New menuTabController(Me)
                End If
                Return _menuTab
            End Get
        End Property
        Private _menuTab As menuTabController
        '
        '===================================================================================================
        Public ReadOnly Property html As Controllers.htmlController
            Get
                If (_html Is Nothing) Then
                    _html = New Controllers.htmlController(Me)
                End If
                Return _html
            End Get
        End Property
        Private _html As Controllers.htmlController
        '
        '===================================================================================================
        Public ReadOnly Property addon As Controllers.addonController
            Get
                If (_addon Is Nothing) Then
                    _addon = New Controllers.addonController(Me)
                End If
                Return _addon
            End Get
        End Property
        Private _addon As Controllers.addonController
        '
        '===================================================================================================
        Public ReadOnly Property menuFlyout As menuFlyoutController
            Get
                If (_menuFlyout Is Nothing) Then
                    _menuFlyout = New menuFlyoutController(Me)
                End If
                Return _menuFlyout
            End Get
        End Property
        Private _menuFlyout As menuFlyoutController
        '
        '===================================================================================================
        Public ReadOnly Property userProperty As propertyModelClass
            Get
                If (_userProperty Is Nothing) Then
                    _userProperty = New propertyModelClass(Me, PropertyTypeMember)
                End If
                Return _userProperty
            End Get
        End Property
        Private _userProperty As propertyModelClass
        '
        '===================================================================================================
        Public ReadOnly Property visitorProperty As propertyModelClass
            Get
                If (_visitorProperty Is Nothing) Then
                    _visitorProperty = New propertyModelClass(Me, PropertyTypeVisitor)
                End If
                Return _visitorProperty
            End Get
        End Property
        Private _visitorProperty As propertyModelClass
        '
        '===================================================================================================
        Public ReadOnly Property visitProperty As propertyModelClass
            Get
                If (_visitProperty Is Nothing) Then
                    _visitProperty = New propertyModelClass(Me, PropertyTypeVisit)
                End If
                Return _visitProperty
            End Get
        End Property
        Private _visitProperty As propertyModelClass
        '
        '===================================================================================================
        Public ReadOnly Property docProperties() As docPropertyController
            Get
                If (_docProperties Is Nothing) Then
                    _docProperties = New docPropertyController(Me)
                End If
                Return _docProperties
            End Get
        End Property
        Private _docProperties As docPropertyController = Nothing
        '
        '===================================================================================================
        ''' <summary>
        ''' siteProperties object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property siteProperties() As sitePropertiesController
            Get
                If (_siteProperties Is Nothing) Then
                    _siteProperties = New sitePropertiesController(Me)
                End If
                Return _siteProperties
            End Get
        End Property
        Private _siteProperties As sitePropertiesController = Nothing
        '
        '===================================================================================================
        Public ReadOnly Property webServer As iisController
            Get
                If (_webServer Is Nothing) Then
                    _webServer = New iisController(Me)
                End If
                Return _webServer
            End Get
        End Property
        Private _webServer As iisController
        '
        '===================================================================================================
        Public ReadOnly Property security() As securityController
            Get
                If (_security Is Nothing) Then
                    _security = New securityController(Me, serverConfig.appConfig.privateKey)
                End If
                Return _security
            End Get
        End Property
        Private _security As securityController = Nothing
        '
        '===================================================================================================
        Public ReadOnly Property appRootFiles() As fileController
            Get
                If (_appRootFiles Is Nothing) Then
                    If (serverConfig.appConfig IsNot Nothing) Then
                        If (serverConfig.appConfig.enabled) Then
                            If serverConfig.isLocalFileSystem Then
                                '
                                ' local server -- everything is ephemeral
                                _appRootFiles = New fileController(Me, serverConfig.isLocalFileSystem, fileController.fileSyncModeEnum.noSync, fileController.normalizePath(serverConfig.appConfig.appRootFilesPath))
                            Else
                                '
                                ' cluster mode - each filesystem is configured accordingly
                                _appRootFiles = New fileController(Me, serverConfig.isLocalFileSystem, fileController.fileSyncModeEnum.activeSync, fileController.normalizePath(serverConfig.appConfig.appRootFilesPath))
                            End If
                        End If
                    End If
                End If
                Return _appRootFiles
            End Get
        End Property
        Private _appRootFiles As fileController = Nothing
        '
        '===================================================================================================
        Public ReadOnly Property tempFiles() As fileController
            Get
                If (_tmpFiles Is Nothing) Then
                    '
                    ' local server -- everything is ephemeral
                    _tmpFiles = New fileController(Me, serverConfig.isLocalFileSystem, fileController.fileSyncModeEnum.noSync, fileController.normalizePath(serverConfig.appConfig.tempFilesPath))
                End If
                Return _tmpFiles
            End Get
        End Property
        Private _tmpFiles As fileController = Nothing
        '
        '===================================================================================================
        Public ReadOnly Property privateFiles() As fileController
            Get
                If (_privateFiles Is Nothing) Then
                    If (serverConfig.appConfig IsNot Nothing) Then
                        If (serverConfig.appConfig.enabled) Then
                            If serverConfig.isLocalFileSystem Then
                                '
                                ' local server -- everything is ephemeral
                                _privateFiles = New fileController(Me, serverConfig.isLocalFileSystem, fileController.fileSyncModeEnum.noSync, fileController.normalizePath(serverConfig.appConfig.privateFilesPath))
                            Else
                                '
                                ' cluster mode - each filesystem is configured accordingly
                                _privateFiles = New fileController(Me, serverConfig.isLocalFileSystem, fileController.fileSyncModeEnum.passiveSync, fileController.normalizePath(serverConfig.appConfig.privateFilesPath))
                            End If
                        End If
                    End If
                End If
                Return _privateFiles
            End Get
        End Property
        Private _privateFiles As fileController = Nothing
        '
        '===================================================================================================
        Public ReadOnly Property programDataFiles() As fileController
            Get
                If (_programDataFiles Is Nothing) Then
                    '
                    ' -- always local -- must be because this object is used to read serverConfig, before the object is valid
                    Dim programDataPath As String = fileController.normalizePath(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)) & "Contensive\"
                    _programDataFiles = New fileController(Me, True, fileController.fileSyncModeEnum.noSync, fileController.normalizePath(programDataPath))
                End If
                Return _programDataFiles
            End Get
        End Property
        Private _programDataFiles As fileController = Nothing
        '
        '===================================================================================================
        Public ReadOnly Property programFiles() As fileController
            Get
                If (_programFiles Is Nothing) Then
                    '
                    ' -- always local
                    _programFiles = New fileController(Me, True, fileController.fileSyncModeEnum.noSync, fileController.normalizePath(serverConfig.programFilesPath))
                End If
                Return _programFiles
            End Get
        End Property
        Private _programFiles As fileController = Nothing
        '
        '===================================================================================================
        Public ReadOnly Property cdnFiles() As fileController
            Get
                If (_cdnFiles Is Nothing) Then
                    If (serverConfig.appConfig IsNot Nothing) Then
                        If (serverConfig.appConfig.enabled) Then
                            If serverConfig.isLocalFileSystem Then
                                '
                                ' local server -- everything is ephemeral
                                _cdnFiles = New fileController(Me, serverConfig.isLocalFileSystem, fileController.fileSyncModeEnum.noSync, fileController.normalizePath(serverConfig.appConfig.cdnFilesPath))
                            Else
                                '
                                ' cluster mode - each filesystem is configured accordingly
                                _cdnFiles = New fileController(Me, serverConfig.isLocalFileSystem, fileController.fileSyncModeEnum.passiveSync, fileController.normalizePath(serverConfig.appConfig.cdnFilesPath))
                            End If
                        End If
                    End If
                End If
                Return _cdnFiles
            End Get
        End Property
        Private _cdnFiles As fileController = Nothing
        '
        '===================================================================================================
        Public ReadOnly Property addonCache As addonModel.addonCacheClass
            Get
                If (_addonCache Is Nothing) Then
                    _addonCache = cache.getObject(Of addonModel.addonCacheClass)("addonCache")
                    If (_addonCache Is Nothing) Then
                        _addonCache = New addonModel.addonCacheClass
                        For Each addon As addonModel In addonModel.createList(Me, "")
                            _addonCache.add(Me, addon)
                        Next
                        Call cache.setContent("addonCache", _addonCache)
                    End If
                End If
                Return _addonCache
            End Get
        End Property
        Private _addonCache As addonModel.addonCacheClass = Nothing
        '
        '===================================================================================================
        ''' <summary>
        ''' siteProperties object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property domainLegacyCache() As domainLegacyModel
            Get
                If (_domains Is Nothing) Then
                    _domains = New domainLegacyModel(Me)
                End If
                Return _domains
            End Get
        End Property
        Private _domains As domainLegacyModel = Nothing
        '
        '===================================================================================================
        Public ReadOnly Property json() As System.Web.Script.Serialization.JavaScriptSerializer
            Get
                If (_json Is Nothing) Then
                    _json = New System.Web.Script.Serialization.JavaScriptSerializer
                End If
                Return _json
            End Get
        End Property
        Private _json As System.Web.Script.Serialization.JavaScriptSerializer
        '
        '===================================================================================================
        Public ReadOnly Property workflow() As workflowController
            Get
                If (_workflow Is Nothing) Then
                    _workflow = New workflowController(Me)
                End If
                Return _workflow
            End Get
        End Property
        Private _workflow As workflowController = Nothing
        '
        '===================================================================================================
        Public ReadOnly Property cache() As Controllers.cacheController
            Get
                If (_cache Is Nothing) Then
                    _cache = New Controllers.cacheController(Me)
                End If
                Return _cache
            End Get
        End Property
        Private _cache As Controllers.cacheController = Nothing
        '
        '===================================================================================================
        Public ReadOnly Property db As dbController
            Get
                If (_db Is Nothing) Then
                    _db = New dbController(Me)
                End If
                Return _db
            End Get
        End Property
        Private _db As dbController
        '
        '===================================================================================================
        Public ReadOnly Property dbServer As dbEngineController
            Get
                If (_dbEngine Is Nothing) Then
                    _dbEngine = New dbEngineController(Me)
                End If
                Return _dbEngine
            End Get
        End Property
        Private _dbEngine As dbEngineController
        '
        '====================================================================================================
        ''' <summary>
        ''' cpCoreClass constructor for cluster use.
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <remarks></remarks>
        Public Sub New(cp As CPClass)
            MyBase.New()
            cp_forAddonExecutionOnly = cp
            '
            ' -- create default auth objects for non-user methods, or until auth is available
            doc.authContext = New authContextModel(Me)
            '
            serverConfig = serverConfigModel.getObject(Me)
            Me.serverConfig.defaultDataSourceType = dataSourceModel.dataSourceTypeEnum.sqlServerNative
            webServer.iisContext = Nothing
            constructorInitialize()
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' cpCoreClass constructor for app, non-Internet use. cpCoreClass is the primary object internally, created by cp.
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <remarks></remarks>
        Public Sub New(cp As CPClass, serverConfig As serverConfigModel)
            MyBase.New()
            Me.cp_forAddonExecutionOnly = cp
            '
            ' -- create default auth objects for non-user methods, or until auth is available
            doc.authContext = New authContextModel(Me)
            '
            Me.serverConfig = serverConfig
            Me.serverConfig.defaultDataSourceType = dataSourceModel.dataSourceTypeEnum.sqlServerNative
            Me.serverConfig.appConfig.appStatus = serverConfigModel.appStatusEnum.OK
            webServer.iisContext = Nothing
            constructorInitialize()
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' cpCoreClass constructor for app, non-Internet use. cpCoreClass is the primary object internally, created by cp.
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <remarks></remarks>
        Public Sub New(cp As CPClass, serverConfig As serverConfigModel, httpContext As System.Web.HttpContext)
            MyBase.New()
            Me.cp_forAddonExecutionOnly = cp
            '
            ' -- create default auth objects for non-user methods, or until auth is available
            doc.authContext = New authContextModel(Me)
            '
            Me.serverConfig = serverConfig
            Me.serverConfig.defaultDataSourceType = dataSourceModel.dataSourceTypeEnum.sqlServerNative
            Me.serverConfig.appConfig.appStatus = serverConfigModel.appStatusEnum.OK
            webServer.initWebContext(httpContext)
            constructorInitialize()
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' cpCoreClass constructor for app, non-Internet use. cpCoreClass is the primary object internally, created by cp.
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <remarks></remarks>
        Public Sub New(cp As CPClass, applicationName As String)
            MyBase.New()
            Me.cp_forAddonExecutionOnly = cp
            '
            ' -- create default auth objects for non-user methods, or until auth is available
            doc.authContext = New authContextModel(Me)
            '
            serverConfig = serverConfigModel.getObject(Me, applicationName)
            serverConfig.defaultDataSourceType = dataSourceModel.dataSourceTypeEnum.sqlServerNative
            If (serverConfig.appConfig IsNot Nothing) Then
                webServer.iisContext = Nothing
                constructorInitialize()
            End If
        End Sub
        '====================================================================================================
        ''' <summary>
        ''' cpCoreClass constructor for a web request/response environment. cpCoreClass is the primary object internally, created by cp.
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <remarks>
        ''' All iis httpContext is loaded here and the context should not be used after this method.
        ''' </remarks>
        Public Sub New(cp As CPClass, applicationName As String, httpContext As System.Web.HttpContext)
            MyBase.New()
            Me.cp_forAddonExecutionOnly = cp
            '
            ' -- create default auth objects for non-user methods, or until auth is available
            doc.authContext = New authContextModel(Me)
            '
            serverConfig = serverConfigModel.getObject(Me, applicationName)
            serverConfig.defaultDataSourceType = dataSourceModel.dataSourceTypeEnum.sqlServerNative
            If (serverConfig.appConfig IsNot Nothing) Then
                Call webServer.initWebContext(httpContext)
                constructorInitialize()
            End If
        End Sub
        '=============================================================================
        ''' <summary>
        ''' Executes the current route. To determine the route:
        ''' route can be from URL, or from routeOverride
        ''' how to process route
        ''' -- urlParameters - /urlParameter(0)/urlParameter(1)/etc.
        ''' -- first try full url, then remove them from the left and test until last, try just urlParameter(0)
        ''' ---- so url /a/b/c, with addon /a and addon /a/b -> would run addon /a/b
        ''' 
        ''' </summary>
        ''' <returns>The doc created by the default addon. (html, json, etc)</returns>
        Public Function executeRoute(Optional routeOverride As String = "") As String
            Dim result As String = ""
            Try
                If (serverConfig.appConfig IsNot Nothing) Then
                    '
                    ' -- execute intercept methods first, like login, that run before the route that returns the page
                    ' -- intercept routes should be addons alos
                    '
                    ' -- determine the route: try routeOverride
                    Dim normalizedRoute As String = genericController.normalizeRoute(routeOverride)
                    If (String.IsNullOrEmpty(normalizedRoute)) Then
                        '
                        ' -- no override, try argument route (remoteMethodAddon=)
                        normalizedRoute = genericController.normalizeRoute(docProperties.getText(RequestNameRemoteMethodAddon))
                        If String.IsNullOrEmpty(normalizedRoute) Then
                            '
                            ' -- no override or argument, use the url as the route
                            normalizedRoute = genericController.normalizeRoute(webServer.requestPathPage.ToLower)
                        End If
                    End If
                    '
                    ' -- legacy ajaxfn methods
                    Dim AjaxFunction As String = docProperties.getText(RequestNameAjaxFunction)
                    If AjaxFunction <> "" Then
                        '
                        ' -- Need to be converted to Url parameter addons
                        result = ""
                        Select Case AjaxFunction
                            Case ajaxGetFieldEditorPreferenceForm
                                '
                                ' moved to Addons.AdminSite
                                doc.continueProcessing = False
                                Return (New Addons.AdminSite.getFieldEditorPreference).execute(cp_forAddonExecutionOnly).ToString()
                            Case AjaxGetDefaultAddonOptionString
                                '
                                ' moved to Addons.AdminSite
                                doc.continueProcessing = False
                                Return (New Addons.AdminSite.getAjaxDefaultAddonOptionStringClass).execute(cp_forAddonExecutionOnly).ToString()
                            Case AjaxSetVisitProperty
                                '
                                ' moved to Addons.AdminSite
                                doc.continueProcessing = False
                                Return (New Addons.AdminSite.setAjaxVisitPropertyClass).execute(cp_forAddonExecutionOnly).ToString()
                            Case AjaxGetVisitProperty
                                '
                                ' moved to Addons.AdminSite
                                doc.continueProcessing = False
                                Return (New Addons.AdminSite.getAjaxVisitPropertyClass).execute(cp_forAddonExecutionOnly).ToString()
                            Case AjaxData
                                '
                                ' moved to Addons.AdminSite
                                doc.continueProcessing = False
                                Return (New Addons.AdminSite.processAjaxDataClass).execute(cp_forAddonExecutionOnly).ToString()
                            Case AjaxPing
                                '
                                ' moved to Addons.AdminSite
                                doc.continueProcessing = False
                                Return (New Addons.AdminSite.getOKClass).execute(cp_forAddonExecutionOnly).ToString()
                            Case AjaxOpenIndexFilter
                                '
                                ' moved to Addons.AdminSite
                                doc.continueProcessing = False
                                Return (New Addons.AdminSite.openAjaxIndexFilterClass).execute(cp_forAddonExecutionOnly).ToString()
                            Case AjaxOpenIndexFilterGetContent
                                '
                                ' moved to Addons.AdminSite
                                doc.continueProcessing = False
                                Return (New Addons.AdminSite.openAjaxIndexFilterGetContentClass).execute(cp_forAddonExecutionOnly).ToString()
                            Case AjaxCloseIndexFilter
                                '
                                ' moved to Addons.AdminSite
                                doc.continueProcessing = False
                                Return (New Addons.AdminSite.closeAjaxIndexFilterClass).execute(cp_forAddonExecutionOnly).ToString()
                            Case AjaxOpenAdminNav
                                '
                                ' moved to Addons.AdminSite
                                doc.continueProcessing = False
                                Return (New Addons.AdminSite.openAjaxAdminNavClass).execute(cp_forAddonExecutionOnly).ToString()
                            Case Else
                                '
                                ' -- unknown method, log warning
                                doc.continueProcessing = False
                                Return String.Empty
                        End Select
                    End If
                    '
                    ' -- legacy email intercept methods
                    If docProperties.getInteger(rnEmailOpenFlag) > 0 Then
                        '
                        ' -- Process Email Open
                        doc.continueProcessing = False
                        Return (New Addons.Core.openEmailClass).execute(cp_forAddonExecutionOnly).ToString()
                    End If
                    If docProperties.getInteger(rnEmailClickFlag) > 0 Then
                        '
                        ' -- Process Email click
                        doc.continueProcessing = False
                        Return (New Addons.Core.clickEmailClass).execute(cp_forAddonExecutionOnly).ToString()
                    End If
                    If docProperties.getInteger(rnEmailBlockRecipientEmail) > 0 Then
                        '
                        ' -- Process Email block
                        doc.continueProcessing = False
                        Return (New Addons.Core.blockEmailClass).execute(cp_forAddonExecutionOnly).ToString()
                    End If
                    '
                    ' -- legacy form process methods 
                    Dim formType As String = docProperties.getText(docProperties.getText("ccformsn") & "type")
                    If (Not String.IsNullOrEmpty(formType)) Then
                        '
                        ' set the meta content flag to show it is not needed for the head tag
                        Select Case formType
                            Case FormTypeAddonStyleEditor
                                '
                                result = (New Addons.Core.processAddonStyleEditorClass).execute(cp_forAddonExecutionOnly).ToString()
                            Case FormTypeAddonSettingsEditor
                                '
                                result = (New Addons.Core.processAddonSettingsEditorClass).execute(cp_forAddonExecutionOnly).ToString()
                            Case FormTypeSendPassword
                                '
                                result = (New Addons.Core.processSendPasswordFormClass).execute(cp_forAddonExecutionOnly).ToString()
                            Case FormTypeLogin, "l09H58a195"
                                '
                                result = (New Addons.Core.processFormLoginDefaultClass).execute(cp_forAddonExecutionOnly).ToString()
                            Case FormTypeToolsPanel
                                '
                                result = (New Addons.Core.processFormToolsPanelClass).execute(cp_forAddonExecutionOnly).ToString()
                            Case FormTypePageAuthoring
                                '
                                result = (New Addons.Core.processFormQuickEditingClass).execute(cp_forAddonExecutionOnly).ToString()
                            Case FormTypeActiveEditor
                                '
                                result = (New Addons.Core.processActiveEditorClass).execute(cp_forAddonExecutionOnly).ToString()
                            Case FormTypeSiteStyleEditor
                                '
                                result = (New Addons.Core.processSiteStyleEditorClass).execute(cp_forAddonExecutionOnly).ToString()
                            Case FormTypeHelpBubbleEditor
                                '
                                result = (New Addons.Core.processHelpBubbleEditorClass).execute(cp_forAddonExecutionOnly).ToString()
                            Case FormTypeJoin
                                '
                                result = (New Addons.Core.processJoinFormClass).execute(cp_forAddonExecutionOnly).ToString()
                        End Select
                    End If
                    '
                    ' -- legacy methods=
                    Dim HardCodedPage As String = docProperties.getText(RequestNameHardCodedPage)
                    If (HardCodedPage <> "") Then
                        Select Case genericController.vbLCase(HardCodedPage)
                            Case HardCodedPageSendPassword
                                '
                                Return (New Addons.Core.processSendPasswordMethodClass).execute(cp_forAddonExecutionOnly).ToString()
                            Case HardCodedPageResourceLibrary
                                '
                                Return (New Addons.Core.processResourceLibraryMethodClass).execute(cp_forAddonExecutionOnly).ToString()
                            Case HardCodedPageLoginDefault
                                '
                                Return (New Addons.Core.processLoginDefaultMethodClass).execute(cp_forAddonExecutionOnly).ToString()
                            Case HardCodedPageLogin
                                '
                                Return (New Addons.Core.processLoginMethodClass).execute(cp_forAddonExecutionOnly).ToString()
                            Case HardCodedPageLogoutLogin
                                '
                                Return (New Addons.Core.processLogoutLoginMethodClass).execute(cp_forAddonExecutionOnly).ToString()
                            Case HardCodedPageLogout
                                '
                                Return (New Addons.Core.processLogoutMethodClass).execute(cp_forAddonExecutionOnly).ToString()
                            Case HardCodedPageSiteExplorer
                                '
                                Return (New Addons.Core.processSiteExplorerMethodClass).execute(cp_forAddonExecutionOnly).ToString()
                            Case HardCodedPageStatus
                                '
                                Return (New Addons.Core.processStatusMethodClass).execute(cp_forAddonExecutionOnly).ToString()
                            Case HardCodedPageRedirect
                                '
                                Return (New Addons.Core.processRedirectMethodClass).execute(cp_forAddonExecutionOnly).ToString()
                            Case HardCodedPageExportAscii
                                '
                                Return (New Addons.Core.processExportAsciiMethodClass).execute(cp_forAddonExecutionOnly).ToString()
                            Case HardCodedPagePayPalConfirm
                                '
                                Return (New Addons.Core.processPayPalConformMethodClass).execute(cp_forAddonExecutionOnly).ToString()
                        End Select
                    End If
                    '
                    ' -- try route Dictionary (addons, admin, link forwards, link alias), from full route to first segment one at a time
                    ' -- so route /this/and/that would first test /this/and/that, then test /this/and, then test /this
                    Dim routeTest As String = normalizedRoute
                    Dim routeFound As Boolean = False
                    Dim routeCnt As Integer = 100
                    Do
                        routeFound = routeDictionary.ContainsKey(routeTest)
                        If (routeFound) Then Exit Do
                        If (routeTest.IndexOf("/") < 0) Then Exit Do
                        routeTest = routeTest.Substring(0, routeTest.LastIndexOf("/"))
                        routeCnt -= 1
                    Loop While ((routeCnt > 0) And (Not routeFound))
                    '
                    ' -- execute route
                    If (routeFound) Then
                        Dim route As CPSiteBaseClass.routeClass = routeDictionary(routeTest)
                        Select Case route.routeType
                            Case CPSiteBaseClass.routeTypeEnum.admin
                                '
                                ' -- admin site
                                '
                                Return Me.addon.execute(addonModel.create(Me, addonGuidAdminSite), New CPUtilsBaseClass.addonExecuteContext() With {.addonType = CPUtilsBaseClass.addonContext.ContextAdmin})
                            Case CPSiteBaseClass.routeTypeEnum.remoteMethod
                                '
                                ' -- remote method
                                Dim addon As addonModel = addonCache.getAddonById(route.remoteMethodAddonId)
                                If (addon IsNot Nothing) Then
                                    Dim executeContext As New CPUtilsBaseClass.addonExecuteContext() With {
                                        .addonType = CPUtilsBaseClass.addonContext.ContextRemoteMethodJson,
                                        .cssContainerClass = "",
                                        .cssContainerId = "",
                                        .hostRecord = New CPUtilsBaseClass.addonExecuteHostRecordContext() With {
                                            .contentName = docProperties.getText("hostcontentname"),
                                            .fieldName = "",
                                            .recordId = docProperties.getInteger("HostRecordID")
                                        },
                                        .personalizationAuthenticated = doc.authContext.isAuthenticated,
                                        .personalizationPeopleId = doc.authContext.user.id
                                    }
                                    Return Me.addon.execute(addon, executeContext)
                                End If
                            Case CPSiteBaseClass.routeTypeEnum.linkAlias
                                '
                                ' - link alias
                                Dim linkAlias As linkAliasModel = linkAliasModel.create(Me, route.linkAliasId)
                                If (linkAlias IsNot Nothing) Then
                                    docProperties.setProperty("bid", linkAlias.PageID)
                                    If (Not String.IsNullOrEmpty(linkAlias.QueryStringSuffix)) Then
                                        Dim nvp As String() = linkAlias.QueryStringSuffix.Split("&"c)
                                        For Each nv In nvp
                                            Dim keyValue As String() = nv.Split("="c)
                                            If (Not String.IsNullOrEmpty(keyValue(0))) Then
                                                If (keyValue.Length > 1) Then
                                                    siteProperties.setProperty(keyValue(0), keyValue(1))
                                                Else
                                                    siteProperties.setProperty(keyValue(0), String.Empty)
                                                End If
                                            End If
                                        Next
                                    End If

                                End If
                            Case CPSiteBaseClass.routeTypeEnum.linkForward
                                '
                                ' -- link forward
                                Dim linkForward As linkForwardModel = linkForwardModel.create(Me, route.linkForwardId)
                                Return webServer.redirect(linkForward.DestinationLink, "Link Forward #" & linkForward.id & ", " & linkForward.name)
                        End Select
                    End If
                    If (normalizedRoute.Equals("favicon.ico")) Then
                        '
                        ' -- Favicon.ico
                        doc.continueProcessing = False
                        Return (New Addons.Core.faviconIcoClass).execute(cp_forAddonExecutionOnly).ToString()
                    End If
                    If (normalizedRoute.Equals("robots.txt")) Then
                        '
                        ' -- Favicon.ico
                        doc.continueProcessing = False
                        Return (New Addons.Core.robotsTxtClass).execute(cp_forAddonExecutionOnly).ToString()
                    End If
                    '
                    ' -- default route
                    Dim defaultAddonId As Integer = siteProperties.getinteger(spDefaultRouteAddonId)
                    If (defaultAddonId > 0) Then
                        '
                        ' -- default route is run if no other route is found, which includes the route=defaultPage (default.aspx)
                        Dim executeContext As New CPUtilsBaseClass.addonExecuteContext() With {
                            .addonType = CPUtilsBaseClass.addonContext.ContextPage,
                            .cssContainerClass = "",
                            .cssContainerId = "",
                            .hostRecord = New CPUtilsBaseClass.addonExecuteHostRecordContext() With {
                                .contentName = "",
                                .fieldName = "",
                                .recordId = 0
                            },
                            .personalizationAuthenticated = doc.authContext.visit.VisitAuthenticated,
                            .personalizationPeopleId = doc.authContext.user.id
                        }
                        Return Me.addon.execute(addonModel.create(Me, defaultAddonId), executeContext)
                    End If
                    '
                    ' -- no route
                    result = "<p>This site is not configured for website traffic. Please set the default route.</p>"
                End If
            Catch ex As Exception
                Call handleException(ex)
            End Try
            Return result
        End Function
        '
        '=================================================================================================
        ''' <summary>
        ''' Run and return results from a remotequery call from cj.ajax.data(handler,key,args,pagesize,pagenumber)
        ''' This routine builds an xml object inside a <result></result> node. 
        ''' Right now, the response is in JSON format, and conforms to the google data visualization spec 0.5
        ''' </summary>
        ''' <returns></returns>
        Public Function executeRoute_ProcessAjaxData() As String
            Dim result As String = ""
            Try
                Dim RemoteKey As String = docProperties.getText("key")
                Dim EncodedArgs As String = docProperties.getText("args")
                Dim PageSize As Integer = docProperties.getInteger("pagesize")
                Dim PageNumber As Integer = docProperties.getInteger("pagenumber")
                Dim RemoteFormat As RemoteFormatEnum
                Select Case genericController.vbLCase(docProperties.getText("responseformat"))
                    Case "jsonnamevalue"
                        RemoteFormat = RemoteFormatEnum.RemoteFormatJsonNameValue
                    Case "jsonnamearray"
                        RemoteFormat = RemoteFormatEnum.RemoteFormatJsonNameArray
                    Case Else 'jsontable
                        RemoteFormat = RemoteFormatEnum.RemoteFormatJsonTable
                End Select
                '
                handleException(New ApplicationException("executeRoute_ProcessAjaxData deprecated, remoteKey [" & RemoteKey & "], responseformat [" & docProperties.getText("responseformat") & "], PageNumber [" & PageNumber & "], PageSize [" & PageSize & "], EncodedArgs [" & EncodedArgs & "]"))
                Return ""
                '
                '
                ' Handle common work
                '
                If PageNumber = 0 Then
                    PageNumber = 1
                End If
                If PageSize = 0 Then
                    PageSize = 100
                End If
                Dim maxRows As Integer = 0
                If maxRows <> 0 And PageSize > maxRows Then
                    PageSize = maxRows
                End If
                '
                Dim ArgName As String() = {}
                Dim ArgValue As String() = {}
                If EncodedArgs <> "" Then
                    Dim Args As String = EncodedArgs
                    Dim ArgArray() As String = Split(Args, "&")
                    Dim ArgCnt As Integer = UBound(ArgArray) + 1
                    ReDim ArgName(ArgCnt)
                    ReDim ArgValue(ArgCnt)
                    For Ptr = 0 To ArgCnt - 1
                        Dim Pos As Integer = genericController.vbInstr(1, ArgArray(Ptr), "=")
                        If Pos > 0 Then
                            ArgName(Ptr) = genericController.DecodeResponseVariable(Mid(ArgArray(Ptr), 1, Pos - 1))
                            ArgValue(Ptr) = genericController.DecodeResponseVariable(Mid(ArgArray(Ptr), Pos + 1))
                        End If
                    Next
                End If
                '
                ' main_Get values out of the remote query record
                '
                Dim gv As New GoogleVisualizationType
                gv.status = GoogleVisualizationStatusEnum.OK
                '
                If gv.status = GoogleVisualizationStatusEnum.OK Then
                    Dim SetPairString As String = ""
                    Dim QueryType As Integer = 0
                    Dim ContentName As String = ""
                    Dim Criteria As String = ""
                    Dim SortFieldList As String = ""
                    Dim AllowInactiveRecords2 As Boolean = False
                    Dim SelectFieldList As String = ""
                    Dim CS As Integer = db.csOpen("Remote Queries", "((VisitId=" & doc.authContext.visit.id & ")and(remotekey=" & db.encodeSQLText(RemoteKey) & "))")
                    If db.csOk(CS) Then
                        '
                        ' Use user definied query
                        '
                        Dim SQLQuery As String = db.csGetText(CS, "sqlquery")
                        'DataSource = dataSourceModel.create(Me, db.cs_getInteger(CS, "datasourceid"), New List(Of String))
                        maxRows = db.csGetInteger(CS, "maxrows")
                        QueryType = db.csGetInteger(CS, "QueryTypeID")
                        ContentName = db.csGet(CS, "ContentID")
                        Criteria = db.csGetText(CS, "Criteria")
                        SortFieldList = db.csGetText(CS, "SortFieldList")
                        AllowInactiveRecords2 = db.csGetBoolean(CS, "AllowInactiveRecords")
                        SelectFieldList = db.csGetText(CS, "SelectFieldList")
                    Else
                        '
                        ' Try Hardcoded queries
                        '
                        Select Case genericController.vbLCase(RemoteKey)
                            Case "ccfieldhelpupdate"
                                '
                                ' developers editing field help
                                '
                                If Not doc.authContext.user.Developer Then
                                    gv.status = GoogleVisualizationStatusEnum.ErrorStatus
                                    Dim Ptr As Integer = 0
                                    If IsArray(gv.errors) Then
                                        Ptr = UBound(gv.errors) + 1
                                    End If
                                    ReDim gv.errors(Ptr)
                                    gv.errors(Ptr) = "permission error"
                                Else
                                    QueryType = QueryTypeUpdateContent
                                    ContentName = "Content Field Help"
                                    Criteria = ""
                                    AllowInactiveRecords2 = False
                                End If
                                'Case Else
                                '    '
                                '    ' query not found
                                '    '
                                '    gv.status = GoogleVisualizationStatusEnum.ErrorStatus
                                '    If IsArray(gv.errors) Then
                                '        Ptr = 0
                                '    Else
                                '        Ptr = UBound(gv.errors) + 1
                                '    End If
                                '    ReDim gv.errors(Ptr)
                                '    gv.errors(Ptr) = "query not found"
                        End Select
                    End If
                    Call db.csClose(CS)
                    '
                    If gv.status = GoogleVisualizationStatusEnum.OK Then
                        Select Case QueryType
                            Case QueryTypeUpdateContent
                                '
                                ' Contensive Content Update, args are field=value updates
                                ' !!!! only allow inbound hits with a referrer from this site - later use the aggregate access table
                                '
                                '
                                ' Go though args and main_Get Set and Criteria
                                '
                                SetPairString = ""
                                Criteria = ""
                                For Ptr = 0 To ArgName.Count - 1
                                    If genericController.vbLCase(ArgName(Ptr)) = "setpairs" Then
                                        SetPairString = ArgValue(Ptr)
                                    ElseIf genericController.vbLCase(ArgName(Ptr)) = "criteria" Then
                                        Criteria = ArgValue(Ptr)
                                    End If
                                Next
                                '
                                ' Open the content and cycle through each setPair
                                '
                                CS = db.csOpen(ContentName, Criteria, SortFieldList, AllowInactiveRecords2, , ,, SelectFieldList)
                                If db.csOk(CS) Then
                                    '
                                    ' update by looping through the args and setting name=values
                                    '
                                    Dim SetPairs() As String = Split(SetPairString, "&")
                                    For Ptr = 0 To UBound(SetPairs)
                                        If SetPairs(Ptr) <> "" Then
                                            Dim Pos As Integer = genericController.vbInstr(1, SetPairs(Ptr), "=")
                                            If Pos > 0 Then
                                                Dim FieldValue As String = genericController.DecodeResponseVariable(Mid(SetPairs(Ptr), Pos + 1))
                                                Dim FieldName As String = genericController.DecodeResponseVariable(Mid(SetPairs(Ptr), 1, Pos - 1))
                                                If Not Models.Complex.cdefModel.isContentFieldSupported(Me, ContentName, FieldName) Then
                                                    Dim errorMessage As String = "result, QueryTypeUpdateContent, key [" & RemoteKey & "], bad field [" & FieldName & "] skipped"
                                                    Throw (New ApplicationException(errorMessage))
                                                Else
                                                    Call db.csSet(CS, FieldName, FieldValue)
                                                End If
                                            End If
                                        End If
                                    Next
                                End If
                                Call db.csClose(CS)
                                'Case QueryTypeInsertContent
                                '    '
                                '    ' !!!! only allow inbound hits with a referrer from this site - later use the aggregate access table
                                '    '
                                '    '
                                '    ' Contensive Content Insert, args are field=value
                                '    '
                                '    'CS = main_InsertCSContent(ContentName)
                            Case Else
                        End Select
                        '
                        ' output
                        '
                        Dim gd As New GoogleDataType
                        gd.IsEmpty = True
                        '
                        Dim Copy As String = remoteQueryController.main_FormatRemoteQueryOutput(Me, gd, RemoteFormat)
                        Copy = genericController.encodeHTML(Copy)
                        result = "<data>" & Copy & "</data>"
                    End If
                End If
            Catch ex As Exception
                Throw (ex)
            End Try
            Return result
        End Function
        '
        '==========================================================================================
        ''' <summary>
        ''' Generic handle exception. Determines method name and class of caller from stack. 
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="ex"></param>
        ''' <param name="cause"></param>
        ''' <param name="stackPtr">How far down in the stack to look for the method error. Pass 1 if the method calling has the error, 2 if there is an intermediate routine.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Sub handleException(ByVal ex As Exception, ByVal cause As String, stackPtr As Integer)
            If (Not _handlingExceptionRecursionBlock) Then
                _handlingExceptionRecursionBlock = True
                Dim frame As StackFrame = New StackFrame(stackPtr)
                Dim method As System.Reflection.MethodBase = frame.GetMethod()
                Dim type As System.Type = method.DeclaringType()
                Dim methodName As String = method.Name
                Dim errMsg As String = type.Name & "." & methodName & ", cause=[" & cause & "], ex=[" & ex.ToString & "]"
                '
                ' append to application event log
                '
                Dim sSource As String = "Contensive"
                Dim sLog As String = "Application"
                Dim eventId As Integer = 1001
                Try
                    '
                    ' if command line has been run on this server, this will work. Otherwise skip
                    '
                    EventLog.WriteEntry(sSource, errMsg, EventLogEntryType.Error, eventId)
                Catch exEvent As Exception
                    ' ignore error. Can be caused if source has not been created. It is created automatically in command line installation util.
                End Try
                '
                ' append to daily trace log
                '
                logController.appendLog(Me, errMsg)
                '
                ' add to doc exception list to display at top of webpage
                '
                If doc.errList Is Nothing Then
                    doc.errList = New List(Of String)
                End If
                If doc.errList.Count = 10 Then
                    doc.errList.Add("Exception limit exceeded")
                ElseIf doc.errList.Count < 10 Then
                    doc.errList.Add(errMsg)
                End If
                '
                ' write consol for debugging
                '
                Console.WriteLine(errMsg)
                '
                _handlingExceptionRecursionBlock = False
            End If
        End Sub
        Private _handlingExceptionRecursionBlock As Boolean = False
        '
        '====================================================================================================
        '
        Public Sub handleException(ByVal ex As Exception, ByVal cause As String)
            Call handleException(ex, cause, 2)
        End Sub
        '
        '====================================================================================================
        '
        Public Sub handleException(ByVal ex As Exception)
            Call handleException(ex, "n/a", 2)
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' cpCoreClass constructor common tasks.
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <remarks></remarks>
        Private Sub constructorInitialize()
            Try
                '
                doc.docGuid = genericController.createGuid()
                doc.allowDebugLog = True
                doc.profileStartTime = DateTime.Now()
                doc.visitPropertyAllowDebugging = True
                '
                ' -- attempt auth load
                If (serverConfig.appConfig Is Nothing) Then
                    '
                    ' -- server mode, there is no application
                    doc.authContext = Models.Context.authContextModel.create(Me, False)
                ElseIf ((serverConfig.appConfig.appMode <> serverConfigModel.appModeEnum.normal) Or (serverConfig.appConfig.appStatus <> serverConfigModel.appStatusEnum.OK)) Then
                    '
                    ' -- application is not ready, might be error, or in maintainence mode
                    doc.authContext = Models.Context.authContextModel.create(Me, False)
                Else
                    doc.authContext = Models.Context.authContextModel.create(Me, siteProperties.allowVisitTracking)
                    '
                    ' -- debug printed defaults on, so if not on, set it off and clear what was collected
                    doc.visitPropertyAllowDebugging = visitProperty.getBoolean("AllowDebugging")
                    If Not doc.visitPropertyAllowDebugging Then
                        doc.testPointMessage = ""
                    End If
                End If
            Catch ex As Exception
                Throw (ex)
            End Try
        End Sub
        '
        '
        '====================================================================================================
        ''' <summary>
        ''' version for cpCore assembly
        ''' </summary>
        ''' <remarks></remarks>
        Public Function codeVersion() As String
            Dim myType As Type = GetType(coreClass)
            Dim myAssembly As Assembly = Assembly.GetAssembly(myType)
            Dim myAssemblyname As AssemblyName = myAssembly.GetName()
            Dim myVersion As Version = myAssemblyname.Version
            Return Format(myVersion.Major, "0") & "." & Format(myVersion.Minor, "00") & "." & Format(myVersion.Build, "00000000")
        End Function
        '
        '===================================================================================================
        Public ReadOnly Property routeDictionary As Dictionary(Of String, CPSiteBaseClass.routeClass)
            Get
                ' -- when an addon changes, the route map has to reload on page exit so it is ready on the next hit. lazy cache here clears on page load, so this does work
                Return Models.Complex.routeDictionaryModel.create(Me)
                'If (_routeDictionary Is Nothing) Then
                '    _routeDictionary = Models.Complex.routeDictionaryModel.create(Me)
                'End If
                'Return _routeDictionary
            End Get
        End Property
        'Private _routeDictionary As Dictionary(Of String, CPSiteBaseClass.routeClass) = Nothing
        '
        '====================================================================================================
#Region " IDisposable Support "
        '
        ' this class must implement System.IDisposable
        ' never throw an exception in dispose
        ' Do not change or add Overridable to these methods.
        ' Put cleanup code in Dispose(ByVal disposing As Boolean).
        '====================================================================================================
        '
        Protected disposed As Boolean = False
        '
        Public Overloads Sub Dispose() Implements IDisposable.Dispose
            ' do not add code here. Use the Dispose(disposing) overload
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
        '
        Protected Overrides Sub Finalize()
            ' do not add code here. Use the Dispose(disposing) overload
            Dispose(False)
            MyBase.Finalize()
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' dispose.
        ''' </summary>
        ''' <param name="disposing"></param>
        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                Me.disposed = True
                If disposing Then
                    '
                    ' call .dispose for managed objects
                    '
                    ' ----- Block all output from underlying routines
                    '
                    doc.blockExceptionReporting = True
                    doc.continueProcessing = False
                    '
                    ' -- save addoncache
                    If (_assemblySkipList IsNot Nothing) Then
                        If (_assemblySkipList.Count > _assemblySkipList_CountWhenLoaded) Then
                            cache.setContent(cacheNameAssemblySkipList, _assemblySkipList)
                        End If
                    End If
                    '
                    ' content server object is valid
                    '
                    If (serverConfig IsNot Nothing) Then
                        If (serverConfig.appConfig IsNot Nothing) Then
                            If (serverConfig.appConfig.appMode = serverConfigModel.appModeEnum.normal) And (serverConfig.appConfig.appStatus = serverConfigModel.appStatusEnum.OK) Then
                                If siteProperties.allowVisitTracking Then
                                    '
                                    ' If visit tracking, save the viewing record
                                    '
                                    Dim ViewingName As String = Left(doc.authContext.visit.id & "." & doc.authContext.visit.PageVisits, 10)
                                    Dim PageID As Integer = 0
                                    If (_doc IsNot Nothing) Then
                                        If (doc.page IsNot Nothing) Then
                                            PageID = doc.page.id
                                        End If
                                    End If
                                    '
                                    ' -- convert requestFormDict to a name=value string for Db storage
                                    Dim requestFormSerialized As String = genericController.convertNameValueDictToREquestString(webServer.requestFormDict)
                                    Dim pagetitle As String = ""
                                    If (Not doc.htmlMetaContent_TitleList.Count.Equals(0)) Then
                                        pagetitle = doc.htmlMetaContent_TitleList.First.content
                                    End If
                                    Dim SQL As String = "insert into ccviewings (" _
                                        & "Name,VisitId,MemberID,Host,Path,Page,QueryString,Form,Referer,DateAdded,StateOK,ContentControlID,pagetime,Active,CreateKey,RecordID,ExcludeFromAnalytics,pagetitle" _
                                        & ")values(" _
                                        & " " & db.encodeSQLText(ViewingName) _
                                        & "," & db.encodeSQLNumber(doc.authContext.visit.id) _
                                        & "," & db.encodeSQLNumber(doc.authContext.user.id) _
                                        & "," & db.encodeSQLText(webServer.requestDomain) _
                                        & "," & db.encodeSQLText(webServer.requestPath) _
                                        & "," & db.encodeSQLText(webServer.requestPage) _
                                        & "," & db.encodeSQLText(Left(webServer.requestQueryString, 255)) _
                                        & "," & db.encodeSQLText(Left(requestFormSerialized, 255)) _
                                        & "," & db.encodeSQLText(Left(webServer.requestReferrer, 255)) _
                                        & "," & db.encodeSQLDate(doc.profileStartTime) _
                                        & "," & db.encodeSQLBoolean(doc.authContext.visit_stateOK) _
                                        & "," & db.encodeSQLNumber(Models.Complex.cdefModel.getContentId(Me, "Viewings")) _
                                        & "," & db.encodeSQLNumber(doc.appStopWatch.ElapsedMilliseconds) _
                                        & ",1" _
                                        & "," & db.encodeSQLNumber(0) _
                                        & "," & db.encodeSQLNumber(PageID)
                                    SQL &= "," & db.encodeSQLBoolean(webServer.pageExcludeFromAnalytics)
                                    SQL &= "," & db.encodeSQLText(pagetitle)
                                    SQL &= ");"
                                    Call db.executeQuery(SQL)
                                End If
                            End If
                        End If
                    End If
                    '
                    ' ----- dispose objects created here
                    '
                    If Not (_addon Is Nothing) Then
                        Call _addon.Dispose()
                        _addon = Nothing
                    End If
                    '
                    If Not (_db Is Nothing) Then
                        Call _db.Dispose()
                        _db = Nothing
                    End If
                    '
                    If Not (_cache Is Nothing) Then
                        Call _cache.Dispose()
                        _cache = Nothing
                    End If
                    '
                    If Not (_workflow Is Nothing) Then
                        Call _workflow.Dispose()
                        _workflow = Nothing
                    End If
                    '
                    If Not (_siteProperties Is Nothing) Then
                        ' no dispose
                        'Call _siteProperties.Dispose()
                        _siteProperties = Nothing
                    End If
                    '
                    If Not (_json Is Nothing) Then
                        ' no dispose
                        'Call _json.Dispose()
                        _json = Nothing
                    End If
                    ''
                    'If Not (_user Is Nothing) Then
                    '    ' no dispose
                    '    'Call _user.Dispose()
                    '    _user = Nothing
                    'End If
                    '
                    If Not (_domains Is Nothing) Then
                        ' no dispose
                        'Call _domains.Dispose()
                        _domains = Nothing
                    End If
                    '
                    If Not (_docProperties Is Nothing) Then
                        ' no dispose
                        'Call _doc.Dispose()
                        _docProperties = Nothing
                    End If
                    '
                    If Not (_security Is Nothing) Then
                        ' no dispose
                        'Call _security.Dispose()
                        _security = Nothing
                    End If
                    '
                    If Not (_webServer Is Nothing) Then
                        ' no dispose
                        'Call _webServer.Dispose()
                        _webServer = Nothing
                    End If
                    '
                    If Not (_menuFlyout Is Nothing) Then
                        ' no dispose
                        'Call _menuFlyout.Dispose()
                        _menuFlyout = Nothing
                    End If
                    '
                    If Not (_visitProperty Is Nothing) Then
                        ' no dispose
                        'Call _visitProperty.Dispose()
                        _visitProperty = Nothing
                    End If
                    '
                    If Not (_visitorProperty Is Nothing) Then
                        ' no dispose
                        'Call _visitorProperty.Dispose()
                        _visitorProperty = Nothing
                    End If
                    '
                    If Not (_userProperty Is Nothing) Then
                        ' no dispose
                        'Call _userProperty.Dispose()
                        _userProperty = Nothing
                    End If
                    '
                    If Not (_db Is Nothing) Then
                        Call _db.Dispose()
                        _db = Nothing
                    End If
                End If
                '
                ' cleanup non-managed objects
                '
            End If
        End Sub
#End Region        '
    End Class
End Namespace
