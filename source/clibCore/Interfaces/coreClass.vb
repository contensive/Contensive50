
Option Strict On
Option Explicit On

Imports System.Xml
Imports System.Reflection
Imports HttpMultipartParser
Imports Contensive.BaseClasses
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
Imports Contensive.Core.Models
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
        '   -- carries objects For services that need initialization (non Static)
        '   -- (no output buffer) html 'buffer' should be an object created, head added, body added then released
        '       -- methods like cp.html.addhead add things to this html object
        '
        ' -- objects passed by constructor - do not dispose
        ' -- yes, cp is needed to pass int addon execution and script execution - but DO NOT call if from anything else
        ' -- no, cpCore should never call up to cp. cp is the api that calls core.
        Friend cp_forAddonExecutionOnly As CPClass                                   ' constructor -- top-level cp
        '
        ' -- shared globals
        '
        Public serverConfig As Models.Entity.serverConfigModel
        '
        ' -- application storage
        '
        Friend deleteOnDisposeFileList As New List(Of String)               ' tmp file list of files that need to be deleted during dispose
        Friend errList As List(Of String)                                   ' exceptions collected during document construction
        Friend userErrorList As List(Of String)                           ' user messages
        '
        ' -- state, authentication, authorization
        ' -- these are set id=0 at construction, then initialize if authentication used
        '
        Public authContext As authContextModel
        '
        ' -- Debugging
        '
        Private appStopWatch As Stopwatch = Stopwatch.StartNew()
        Public Property app_startTime As Date                                        ' set in constructor
        Public Property app_startTickCount As Integer = 0
        Public Property debug_allowDebugLog As Boolean = False                       ' turn on in script -- use to write /debug.log in content files for whatever is needed
        Public Property blockExceptionReporting As Boolean = False                   ' used so error reporting can not call itself
        Public Property app_errorCount As Integer = 0
        Public Property debug_iUserError As String = ""                              ' User Error String
        Public Property html_PageErrorWithoutCsv As Boolean = False                  ' if true, the error occurred before Csv was available and main_TrapLogMessage needs to be saved and popedup
        Public Property main_TrapLogMessage As String = ""                           ' The content of the current traplog (keep for popups if no Csv)
        Public Property main_ClosePageCounter As Integer = 0
        Public Property html_BlockClosePageLink As Boolean = False                   ' if true,block the href to contensive
        Public Property main_testPointMessage As String = ""                         '
        Public Property testPointPrinting As Boolean = False                         ' if true, send main_TestPoint messages to the stream
        Public Property docOpen As Boolean = False                                   ' when false, routines should not add to the output and immediately exit
        'Public Const cache_linkAlias_cacheName = "cache_linkAlias"
        'Public Property cache_linkAlias As String(,)
        'Public Property cache_linkAliasCnt As Integer = 0
        'Public Property cache_linkAlias_NameIndex As keyPtrController
        'Public Property cache_linkAlias_PageIdQSSIndex As keyPtrController
        '
        '========================================================================================================================
        '   Internal cache (for content used to run the system)
        '
        Public Property upgradeInProgress() As Boolean
        Private Property main_PleaseWaitStarted As Boolean = False
        '
        '------------------------------------------------------------------------
        ' ----- Debugging
        '
        Public Property csv_ConnectionID As Integer = 0                     ' Random number (semi) unique to this hit
        Friend Property addonsRunOnThisPageIdList As New List(Of Integer)
        Friend Property addonsCurrentlyRunningIdList As New List(Of Integer)
        Public Structure csv_stylesheetCacheType
            Dim templateId As Integer
            Dim EmailID As Integer
            Dim StyleSheet As String
        End Structure
        Public Property csv_stylesheetCache As csv_stylesheetCacheType()
        Public Property csv_stylesheetCacheCnt As Integer
        Private Property web_EncodeContent_JavascriptOnLoad_Cnt As Integer
        Private Property web_EncodeContent_JavascriptOnLoad As String()
        Private Property web_EncodeContent_JSFilename_Cnt As Integer
        Private Property web_EncodeContent_JSFilename As String()
        Friend Property web_EncodeContent_JavascriptBodyEnd_cnt As Integer
        Friend Property web_EncodeContent_JavascriptBodyEnd As String()
        Friend Property web_EncodeContent_StyleFilenames_Cnt As Integer
        Friend Property web_EncodeContent_StyleFilenames As String()
        Friend Property web_EncodeContent_HeadTags As String = ""
        Public Property pageManager_PageAddonCnt As Integer = 0
        '
        '===================================================================================================
        Public ReadOnly Property addonStyleRulesIndex() As keyPtrIndexController
            Get
                If (_cache_addonStyleRules Is Nothing) Then
                    _cache_addonStyleRules = New keyPtrIndexController(Me, cacheNameAddonStyleRules, sqlAddonStyles, "shared style add-on rules,add-ons,shared styles")
                End If
                Return _cache_addonStyleRules
            End Get
        End Property
        Private _cache_addonStyleRules As keyPtrIndexController = Nothing
        '
        '===================================================================================================
        Public ReadOnly Property dataSourceDictionary() As Dictionary(Of String, Models.Entity.dataSourceModel)
            Get
                If (_dataSources Is Nothing) Then
                    _dataSources = Models.Entity.dataSourceModel.getNameDict(Me)
                End If
                Return _dataSources
            End Get
        End Property
        Private _dataSources As Dictionary(Of String, Models.Entity.dataSourceModel) = Nothing
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
        Public ReadOnly Property pages As pagesController
            Get
                If (_pages Is Nothing) Then
                    _pages = New pagesController(Me)
                End If
                Return _pages
            End Get
        End Property
        Private _pages As pagesController
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
        Public ReadOnly Property htmlDoc As Controllers.htmlDocController
            Get
                If (_htmlDoc Is Nothing) Then
                    _htmlDoc = New Controllers.htmlDocController(Me)
                End If
                Return _htmlDoc
            End Get
        End Property
        Private _htmlDoc As Controllers.htmlDocController
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
                If (_doc Is Nothing) Then
                    _doc = New docPropertyController(Me)
                End If
                Return _doc
            End Get
        End Property
        Private _doc As docPropertyController = Nothing
        '
        '===================================================================================================
        ''' <summary>
        ''' siteProperties object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property siteProperties() As Models.Context.siteContextModel
            Get
                If (_siteProperties Is Nothing) Then
                    _siteProperties = New Models.Context.siteContextModel(Me)
                End If
                Return _siteProperties
            End Get
        End Property
        Private _siteProperties As Models.Context.siteContextModel = Nothing
        '
        '===================================================================================================
        Public ReadOnly Property webServer As webServerController
            Get
                If (_webServer Is Nothing) Then
                    _webServer = New webServerController(Me)
                End If
                Return _webServer
            End Get
        End Property
        Private _webServer As webServerController
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
        Public ReadOnly Property serverFiles() As fileController
            Get
                If (_serverFiles Is Nothing) Then
                    If serverConfig.isLocalFileSystem Then
                        '
                        ' local server -- everything is ephemeral
                        _serverFiles = New fileController(Me, serverConfig.isLocalFileSystem, fileController.fileSyncModeEnum.noSync, "")
                    Else
                        '
                        ' cluster mode - each filesystem is configured accordingly
                        _serverFiles = New fileController(Me, serverConfig.isLocalFileSystem, fileController.fileSyncModeEnum.noSync, "")
                    End If
                End If
                Return _serverFiles
            End Get
        End Property
        Private _serverFiles As fileController = Nothing
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
        Public ReadOnly Property addonLegacyCache() As Models.Entity.addonLegacyModel
            Get
                If (_addonCache Is Nothing) Then
                    _addonCache = New Models.Entity.addonLegacyModel(Me)
                End If
                Return _addonCache
            End Get
        End Property
        Private _addonCache As Models.Entity.addonLegacyModel = Nothing
        '
        '===================================================================================================
        ''' <summary>
        ''' siteProperties object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property domainLegacyCache() As Models.Entity.domainLegacyModel
            Get
                If (_domains Is Nothing) Then
                    _domains = New Models.Entity.domainLegacyModel(Me)
                End If
                Return _domains
            End Get
        End Property
        Private _domains As Models.Entity.domainLegacyModel = Nothing
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
        Public ReadOnly Property metaData As metaDataController
            Get
                If _metaData Is Nothing Then
                    _metaData = New metaDataController(Me)
                End If
                Return _metaData
            End Get
        End Property
        Private _metaData As metaDataController = Nothing
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
            authContext = New authContextModel
            '
            serverConfig = Models.Entity.serverConfigModel.getObject(Me)
            Me.serverConfig.defaultDataSourceType = Models.Entity.dataSourceModel.dataSourceTypeEnum.sqlServerNative
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
        Public Sub New(cp As CPClass, serverConfig As Models.Entity.serverConfigModel)
            MyBase.New()
            Me.cp_forAddonExecutionOnly = cp
            '
            ' -- create default auth objects for non-user methods, or until auth is available
            authContext = New authContextModel
            '
            Me.serverConfig = serverConfig
            Me.serverConfig.defaultDataSourceType = Models.Entity.dataSourceModel.dataSourceTypeEnum.sqlServerNative
            Me.serverConfig.appConfig.appStatus = Models.Entity.serverConfigModel.applicationStatusEnum.ApplicationStatusReady
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
        Public Sub New(cp As CPClass, serverConfig As Models.Entity.serverConfigModel, httpContext As System.Web.HttpContext)
            MyBase.New()
            Me.cp_forAddonExecutionOnly = cp
            '
            ' -- create default auth objects for non-user methods, or until auth is available
            authContext = New authContextModel
            '
            Me.serverConfig = serverConfig
            Me.serverConfig.defaultDataSourceType = Models.Entity.dataSourceModel.dataSourceTypeEnum.sqlServerNative
            Me.serverConfig.appConfig.appStatus = Models.Entity.serverConfigModel.applicationStatusEnum.ApplicationStatusReady
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
            authContext = New authContextModel
            '
            serverConfig = Models.Entity.serverConfigModel.getObject(Me, applicationName)
            serverConfig.defaultDataSourceType = Models.Entity.dataSourceModel.dataSourceTypeEnum.sqlServerNative
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
            authContext = New authContextModel
            '
            serverConfig = Models.Entity.serverConfigModel.getObject(Me, applicationName)
            serverConfig.defaultDataSourceType = Models.Entity.dataSourceModel.dataSourceTypeEnum.sqlServerNative
            If (serverConfig.appConfig IsNot Nothing) Then
                Call webServer.initWebContext(httpContext)
                constructorInitialize()
            End If
        End Sub
        '=============================================================================
        ''' <summary>
        ''' Executes the current route (pathPage and/or querystring based). If not found, the default route (addon) is executed. Initially the default route is the pageManager.
        ''' </summary>
        ''' <returns>The doc created by the default addon. (html, json, etc)</returns>
        Public Function executeRoute(Optional route As String = "") As String
            Dim returnResult As String = ""
            Try
                If (serverConfig.appConfig IsNot Nothing) Then
                    '
                    ' -- if app is not configured, cannot execute route
                    Dim pairs() As String
                    Dim pairName As String
                    Dim pairValue As String
                    Dim addonRoute As String = ""
                    Dim routeTest As String
                    Dim workingRoute As String
                    Dim adminRoute As String = serverConfig.appConfig.adminRoute.ToLower
                    Dim AjaxFunction As String = docProperties.getText(RequestNameAjaxFunction)
                    Dim AjaxFastFunction As String = docProperties.getText(RequestNameAjaxFastFunction)
                    Dim RemoteMethodFromQueryString As String = docProperties.getText(RequestNameRemoteMethodAddon)
                    '
                    'debugLog("executeRoute, enter")
                    '
                    ' determine route from either url or querystring 
                    '
                    If (Not String.IsNullOrEmpty(route)) Then
                        '
                        ' route privided as argument
                        '
                        workingRoute = route
                    ElseIf (Not String.IsNullOrEmpty(RemoteMethodFromQueryString)) Then
                        '
                        ' route comes from a remoteMethod=route querystring argument
                        '
                        workingRoute = "/" & RemoteMethodFromQueryString.ToLower()
                    Else
                        '
                        ' routine comes from the url
                        '
                        workingRoute = webServer.requestPathPage.ToLower
                    End If
                    '
                    ' normalize route to /path/page or /path
                    '
                    workingRoute = genericController.normalizeRoute(workingRoute)
                    '
                    ' call with no addon route returns admin site
                    '
                    If True Then
                        '
                        '------------------------------------------------------------------------------------------
                        '   remote method
                        '       for hardcoded_addons (simple ajax functions) and addons with asajax and inframe
                        '       Eventually replace the hard-coded ajax hood with this process
                        '       so cj. methods can be consolidated into the cj.ajax.addon (can callback) calls
                        '------------------------------------------------------------------------------------------
                        '
                        ' if route is a remote method, use it
                        '
                        routeTest = workingRoute
                        Dim addonPtr As Integer = addonLegacyCache.getPtr(routeTest)
                        If addonPtr >= 0 Then
                            If addonLegacyCache.addonCache.addonList(addonPtr.ToString).remoteMethod Then
                                addonRoute = routeTest
                            End If
                        Else
                            If (InStr(routeTest, "/", CompareMethod.Text) = 1) Then
                                routeTest = routeTest.Substring(1)
                                addonPtr = addonLegacyCache.getPtr(routeTest)
                                If addonPtr >= 0 Then
                                    If addonLegacyCache.addonCache.addonList(addonPtr.ToString).remoteMethod Then
                                        addonRoute = routeTest
                                    End If
                                End If
                            End If
                        End If
                        If addonRoute = "" Then
                            '
                            ' if remote method is not in route, get nameGuid from querystring
                            '
                            addonRoute = docProperties.getText(RequestNameRemoteMethodAddon)
                        End If
                        If addonRoute <> "" Then
                            '
                            ' -- this section was added here. it came from an earlier processing section of initApp() but appears to apply only to remote method processing
                            '--------------------------------------------------------------------------
                            '   Verify Add-ons are run from Referrers on the Aggregate Access List
                            '--------------------------------------------------------------------------
                            '
                            If webServer.webServerIO_ReadStreamJSForm Then
                                If webServer.requestReferrer = "" Then
                                    '
                                    ' Allow it to be hand typed
                                    '
                                Else
                                    '
                                    ' Test source site
                                    '
                                    Dim refProtocol As String = ""
                                    Dim refHost As String = ""
                                    Dim refPath As String = ""
                                    Dim refPage As String = ""
                                    Dim refQueryString As String = ""
                                    Dim cs As Integer
                                    Call genericController.SeparateURL(webServer.requestReferrer, refProtocol, refHost, refPath, refPage, refQueryString)
                                    If genericController.vbUCase(refHost) <> genericController.vbUCase(webServer.requestDomain) Then
                                        '
                                        ' Not from this site
                                        '
                                        If siteProperties.getBoolean("AllowAggregateAccessBlocking") Then
                                            cs = db.cs_open("Aggregate Access", "Link=" & db.encodeSQLText(refHost), , False, , , , "active")
                                            If Not db.cs_ok(cs) Then
                                                '
                                                ' no record, add an inactive record and throw error
                                                '
                                                Call db.cs_Close(cs)
                                                cs = db.cs_insertRecord("Aggregate Access")
                                                If db.cs_ok(cs) Then
                                                    Call db.cs_set(cs, "Name", refHost)
                                                    Call db.cs_set(cs, "Link", refHost)
                                                    Call db.cs_set(cs, "active", False)
                                                End If
                                                Call db.cs_Close(cs)
                                                handleExceptionAndContinue(New ApplicationException("Add-on call from [" & refHost & "] was blocked because this domain is not in the Aggregate Access Content. An inactive record was added. To allow this domain access, mark the record active.")) ' handleLegacyError12("Init", "")
                                                docOpen = False '--- should be disposed by caller --- Call dispose
                                                Return htmlDoc.docBuffer
                                            ElseIf Not db.cs_getBoolean(cs, "active") Then
                                                '
                                                ' inactive record, throw error
                                                '
                                                Call db.cs_Close(cs)
                                                handleExceptionAndContinue(New ApplicationException("Add-on call from [" & refHost & "] was blocked because this domain is not active in the Aggregate Access Content. To allow this domain access, mark the record active.")) ' handleLegacyError12("Init", "")
                                                docOpen = False '--- should be disposed by caller --- Call dispose
                                                Return htmlDoc.docBuffer
                                            Else
                                                '
                                                ' Active record, allow hit
                                                '
                                                Call db.cs_Close(cs)
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                            '
                            'Call AppendLog("main_init(), 2710 - exit for remote method")
                            '
                            If True Then
                                Dim Option_String As String = ""
                                Dim pos As Integer
                                Dim HostContentName As String
                                Dim hostRecordId As Integer
                                If docProperties.containsKey("Option_String") Then
                                    Option_String = docProperties.getText("Option_String")
                                Else
                                    '
                                    ' convert Querystring encoding to (internal) NVA
                                    '
                                    If webServer.requestQueryString <> "" Then
                                        pairs = Split(webServer.requestQueryString, "&")
                                        For addonPtr = 0 To UBound(pairs)
                                            pairName = pairs(addonPtr)
                                            pairValue = ""
                                            pos = genericController.vbInstr(1, pairName, "=")
                                            If pos > 0 Then
                                                pairValue = genericController.DecodeResponseVariable(Mid(pairName, pos + 1))
                                                pairName = genericController.DecodeResponseVariable(Mid(pairName, 1, pos - 1))
                                            End If
                                            Option_String = Option_String & "&" & genericController.encodeNvaArgument(pairName) & "=" & genericController.encodeNvaArgument(pairValue)
                                        Next
                                        Option_String = Mid(Option_String, 2)
                                    End If
                                End If
                                HostContentName = docProperties.getText("hostcontentname")
                                hostRecordId = docProperties.getInteger("HostRecordID")
                                '
                                ' remote methods are add-ons
                                '
                                Dim AddonStatusOK As Boolean = True
                                '
                                ' REFACTOR -- must know if this is json or html remote before call because it is an argument -- assume this is a json for now -- must deal with it somehow
                                '
                                returnResult = addon.execute(0, addonRoute, Option_String, CPUtilsBaseClass.addonContext.ContextRemoteMethodJson, HostContentName, hostRecordId, "", "0", False, 0, "", AddonStatusOK, Nothing, "", Nothing, "", authContext.user.ID, authContext.isAuthenticated)
                            End If
                            '
                            ' deliver styles, javascript and other head tags as javascript appends
                            '
                            webServer.webServerIO_BlockClosePageCopyright = True
                            html_BlockClosePageLink = True
                            If (webServer.webServerIO_OutStreamDevice = htmlDocController.htmlDoc_OutStreamJavaScript) Then
                                If genericController.vbInstr(1, returnResult, "<form ", vbTextCompare) <> 0 Then
                                    Dim FormSplit As String() = Split(returnResult, "<form ", , vbTextCompare)
                                    returnResult = FormSplit(0)
                                    For addonPtr = 1 To UBound(FormSplit)
                                        Dim FormEndPos As Integer = genericController.vbInstr(1, FormSplit(addonPtr), ">")
                                        Dim FormInner As String = Mid(FormSplit(addonPtr), 1, FormEndPos)
                                        Dim FormSuffix As String = Mid(FormSplit(addonPtr), FormEndPos + 1)
                                        FormInner = genericController.vbReplace(FormInner, "method=""post""", "method=""main_Get""", 1, 99, vbTextCompare)
                                        FormInner = genericController.vbReplace(FormInner, "method=post", "method=""main_Get""", 1, 99, vbTextCompare)
                                        returnResult = returnResult & "<form " & FormInner & FormSuffix
                                    Next
                                End If
                                '
                                Call htmlDoc.writeAltBuffer(returnResult)
                                returnResult = ""
                            End If
                            Return returnResult
                        End If
                        If True Then
                            '
                            '------------------------------------------------------------------------------------------
                            '   These should all be converted to system add-ons
                            '
                            '   AJAX late functions (slower then the early functions, but they include visit state, etc.
                            '------------------------------------------------------------------------------------------
                            '
                            If AjaxFunction <> "" Then
                                returnResult = ""
                                Select Case AjaxFunction
                                    Case ajaxGetFieldEditorPreferenceForm
                                        '
                                        ' When editing in admin site, if a field has multiple editors (addons as editors), you main_Get an icon
                                        '   to click to select the editor. When clicked, a fancybox opens to display a form. The onStart of
                                        '   he fancybox calls this ajax call and puts the return in the div that is displayed. Return a list
                                        '   of addon editors compatible with the field type.
                                        '
                                        Dim addonDefaultEditorName As String = ""
                                        Dim addonDefaultEditorId As Integer = 0
                                        Dim fieldId As Integer = docProperties.getInteger("fieldid")
                                        '
                                        ' main_Get name of default editor
                                        '
                                        Dim Sql As String = "select top 1" _
                                        & " a.name,a.id" _
                                        & " from ccfields f left join ccAggregateFunctions a on a.id=f.editorAddonId" _
                                        & " where" _
                                        & " f.ID = " & fieldId _
                                        & ""
                                        Dim dt As DataTable
                                        dt = db.executeSql(Sql)
                                        If dt.Rows.Count > 0 Then
                                            For Each rsDr As DataRow In dt.Rows
                                                addonDefaultEditorName = "&nbsp;(" & genericController.encodeText(rsDr("name")) & ")"
                                                addonDefaultEditorId = genericController.EncodeInteger(rsDr("id"))
                                            Next
                                        End If
                                        '
                                        Dim radioGroupName As String = "setEditorPreference" & fieldId
                                        Dim currentEditorAddonId As Integer = docProperties.getInteger("currentEditorAddonId")
                                        Dim submitFormId As Integer = docProperties.getInteger("submitFormId")
                                        Sql = "select f.name,c.name,r.addonid,a.name as addonName" _
                                        & " from (((cccontent c" _
                                        & " left join ccfields f on f.contentid=c.id)" _
                                        & " left join ccAddonContentFieldTypeRules r on r.contentFieldTypeID=f.type)" _
                                        & " left join ccAggregateFunctions a on a.id=r.AddonId)" _
                                        & " where f.id=" & fieldId

                                        dt = db.executeSql(Sql)
                                        If dt.Rows.Count > 0 Then
                                            For Each rsDr As DataRow In dt.Rows
                                                Dim addonId As Integer = genericController.EncodeInteger(rsDr("addonid"))
                                                If (addonId <> 0) And (addonId <> addonDefaultEditorId) Then
                                                    returnResult = returnResult _
                                                    & vbCrLf & vbTab & "<div class=""radioCon"">" & htmlDoc.html_GetFormInputRadioBox(radioGroupName, genericController.encodeText(addonId), CStr(currentEditorAddonId)) & "&nbsp;Use " & genericController.encodeText(rsDr("addonName")) & "</div>" _
                                                    & ""
                                                End If

                                            Next
                                        End If

                                        Dim OnClick As String = "" _
                                        & "var a=document.getElementsByName('" & radioGroupName & "');" _
                                        & "for(i=0;i<a.length;i++) {" _
                                        & "if(a[i].checked){var v=a[i].value}" _
                                        & "}" _
                                        & "document.getElementById('fieldEditorPreference').value='" & fieldId & ":'+v;" _
                                        & "cj.admin.saveEmptyFieldList('" & "FormEmptyFieldList');" _
                                        & "document.getElementById('adminEditForm').submit();" _
                                        & ""

                                        returnResult = "" _
                                        & vbCrLf & vbTab & "<h1>Editor Preference</h1>" _
                                        & vbCrLf & vbTab & "<p>Select the editor you will use for this field. Select default if you want to use the current system default.</p>" _
                                        & vbCrLf & vbTab & "<div class=""radioCon"">" & htmlDoc.html_GetFormInputRadioBox("setEditorPreference" & fieldId, "0", "0") & "&nbsp;Use Default Editor" & addonDefaultEditorName & "</div>" _
                                        & vbCrLf & vbTab & returnResult _
                                        & vbCrLf & vbTab & "<div class=""buttonCon"">" _
                                        & vbCrLf & vbTab & "<button type=""button"" onclick=""" & OnClick & """>Select</button>" _
                                        & vbCrLf & vbTab & "</div>" _
                                        & ""
                                    Case AjaxGetDefaultAddonOptionString
                                        '
                                        ' return the addons defult AddonOption_String
                                        ' used in wysiwyg editor - addons in select list have no defaultOption_String
                                        ' because created it is expensive (lookuplists, etc). This is only called
                                        ' when the addon is double-clicked in the editor after being dropped
                                        '
                                        Dim AddonGuid As String = docProperties.getText("guid")
                                        '$$$$$ cache this
                                        Dim CS As Integer = db.cs_open(cnAddons, "ccguid=" & db.encodeSQLText(AddonGuid))
                                        Dim addonArgumentList As String = ""
                                        Dim addonIsInline As Boolean = False
                                        If db.cs_ok(CS) Then
                                            addonArgumentList = db.cs_getText(CS, "argumentlist")
                                            addonIsInline = db.cs_getBoolean(CS, "IsInline")
                                            returnResult = addonController.main_GetDefaultAddonOption_String(Me, addonArgumentList, AddonGuid, addonIsInline)
                                        End If
                                        Call db.cs_Close(CS)
                                    Case AjaxSetVisitProperty
                                        '
                                        ' 7/7/2009 - Moved from HardCodedPages - sets a visit property from the cj object
                                        '
                                        Dim ArgList As String = docProperties.getText("args")
                                        Dim Args As String() = Split(ArgList, "&")
                                        Dim gd As GoogleDataType = New GoogleDataType
                                        gd.IsEmpty = True
                                        For Ptr = 0 To UBound(Args)
                                            Dim ArgNameValue As String() = Split(Args(Ptr), "=")
                                            Dim PropertyName As String = ArgNameValue(0)
                                            Dim PropertyValue As String = ""
                                            If UBound(ArgNameValue) > 0 Then
                                                PropertyValue = ArgNameValue(1)
                                            End If
                                            Call visitProperty.setProperty(PropertyName, PropertyValue)
                                        Next
                                        returnResult = remoteQueryController.main_FormatRemoteQueryOutput(Me, gd, RemoteFormatEnum.RemoteFormatJsonNameValue)
                                        returnResult = htmlDoc.main_encodeHTML(returnResult)
                                        Call htmlDoc.writeAltBuffer(returnResult)
                                    Case AjaxGetVisitProperty
                                        '
                                        ' 7/7/2009 - Moved from HardCodedPages - sets a visit property from the cj object
                                        '
                                        Dim ArgList As String = docProperties.getText("args")
                                        Dim Args As String() = Split(ArgList, "&")
                                        Dim gd As GoogleDataType = New GoogleDataType
                                        gd.IsEmpty = False
                                        ReDim gd.row(0)

                                        For Ptr = 0 To UBound(Args)
                                            ReDim Preserve gd.col(Ptr)
                                            ReDim Preserve gd.row(0).Cell(Ptr)
                                            Dim ArgNameValue As String() = Split(Args(Ptr), "=")
                                            Dim PropertyName As String = ArgNameValue(0)
                                            gd.col(Ptr).Id = PropertyName
                                            gd.col(Ptr).Label = PropertyName
                                            gd.col(Ptr).Type = "string"
                                            Dim PropertyValue As String = ""
                                            If UBound(ArgNameValue) > 0 Then
                                                PropertyValue = ArgNameValue(1)
                                            End If
                                            gd.row(0).Cell(Ptr).v = visitProperty.getText(PropertyName, PropertyValue)
                                        Next
                                        returnResult = remoteQueryController.main_FormatRemoteQueryOutput(Me, gd, RemoteFormatEnum.RemoteFormatJsonNameValue)
                                        returnResult = htmlDoc.main_encodeHTML(returnResult)
                                        Call htmlDoc.writeAltBuffer(returnResult)
                                    Case AjaxData
                                        '
                                        ' 7/7/2009 - Moved from HardCodedPages - Run remote query from cj.remote object call, and return results html encoded in a <result></result> block
                                        ' 20050427 - not used
                                        Call htmlDoc.writeAltBuffer(executeRoute_ProcessAjaxData())
                                    Case AjaxPing
                                        '
                                        ' returns OK if the server is alive
                                        '
                                        returnResult = "ok"
                                    Case AjaxOpenIndexFilter
                                        Call visitProperty.setProperty("IndexFilterOpen", "1")
                                    Case AjaxOpenIndexFilterGetContent
                                        '
                                        ' should be converted to adminClass remoteMethod
                                        '
                                        Call visitProperty.setProperty("IndexFilterOpen", "1")
                                        Dim adminSite As New Contensive.Addons.addon_AdminSiteClass(cp_forAddonExecutionOnly)
                                        Dim ContentID As Integer = docProperties.getInteger("cid")
                                        If ContentID = 0 Then
                                            returnResult = "No filter is available"
                                        Else
                                            Dim cdef As cdefModel = metaData.getCdef(ContentID)
                                            returnResult = adminSite.GetForm_IndexFilterContent(cdef)
                                        End If
                                        adminSite = Nothing
                                    Case AjaxCloseIndexFilter
                                        Call visitProperty.setProperty("IndexFilterOpen", "0")
                                    Case AjaxOpenAdminNav
                                        Call visitProperty.setProperty("AdminNavOpen", "1")
                                    Case Else
                                End Select
                                '
                                'Call AppendLog("main_init(), 2810 - exit for ajax hook")
                                '
                                webServer.webServerIO_BlockClosePageCopyright = True
                                html_BlockClosePageLink = True
                                'Call AppendLog("call main_getEndOfBody, from main_initf")
                                returnResult = returnResult & htmlDoc.getBeforeEndOfBodyHtml(False, False, True, False)
                                Call htmlDoc.writeAltBuffer(returnResult)
                                docOpen = False '--- should be disposed by caller --- Call dispose
                                Return htmlDoc.docBuffer
                            End If
                        End If
                        '
                        '--------------------------------------------------------------------------
                        '   Process Email Open and Click Intercepts
                        '   works with DropID -> spacer.gif, or DropCssID -> styles.css
                        '--------------------------------------------------------------------------
                        '
                        If True Then
                            Dim recordid As Integer
                            Dim emailDropId As Integer
                            Dim RedirectLink As String
                            Dim EmailMemberID As Integer
                            Dim CSLog As Integer
                            Dim EmailSpamBlock As String
                            recordid = 0
                            emailDropId = docProperties.getInteger(RequestNameEmailOpenFlag)
                            If emailDropId <> 0 Then
                                recordid = emailDropId
                            End If
                            '    End If
                            If (recordid <> 0) Then
                                '
                                ' ----- Email open detected. Log it and redirect to a 1x1 spacer
                                '
                                EmailMemberID = docProperties.getInteger(RequestNameEmailMemberID)
                                CSLog = db.cs_insertRecord("Email Log")
                                If db.cs_ok(CSLog) Then
                                    Call db.cs_set(CSLog, "Name", "Opened " & CStr(app_startTime))
                                    Call db.cs_set(CSLog, "EmailDropID", recordid)
                                    Call db.cs_set(CSLog, "MemberID", EmailMemberID)
                                    Call db.cs_set(CSLog, "LogType", EmailLogTypeOpen)
                                End If
                                Call db.cs_Close(CSLog)
                                RedirectLink = webServer.webServerIO_requestProtocol & webServer.requestDomain & "/ccLib/images/spacer.gif"
                                Call webServer.redirect(RedirectLink, "Group Email Open hit, redirecting to a dummy image", False)
                            End If
                            '
                            emailDropId = docProperties.getInteger(RequestNameEmailClickFlag)
                            EmailSpamBlock = docProperties.getText(RequestNameEmailSpamFlag)
                            If (emailDropId <> 0) And (EmailSpamBlock = "") Then
                                '
                                ' ----- Email click detected. Log it.
                                '
                                EmailMemberID = docProperties.getInteger(RequestNameEmailMemberID)
                                CSLog = db.cs_insertRecord("Email Log")
                                If db.cs_ok(CSLog) Then
                                    Call db.cs_set(CSLog, "Name", "Clicked " & CStr(app_startTime))
                                    Call db.cs_set(CSLog, "EmailDropID", emailDropId)
                                    Call db.cs_set(CSLog, "MemberID", EmailMemberID)
                                    Call db.cs_set(CSLog, "VisitId", authContext.visit.ID)
                                    Call db.cs_set(CSLog, "LogType", EmailLogTypeClick)
                                End If
                                Call db.cs_Close(CSLog)
                            End If
                            If EmailSpamBlock <> "" Then
                                '
                                ' ----- Email spam footer was clicked, clear the AllowBulkEmail field
                                '
                                Call email.addToBlockList(EmailSpamBlock)
                                '
                                CSLog = db.cs_open("people", "email=" & db.encodeSQLText(EmailSpamBlock), , , , , , "AllowBulkEmail")
                                Do While db.cs_ok(CSLog)
                                    Call db.cs_set(CSLog, "AllowBulkEmail", False)
                                    Call db.cs_goNext(CSLog)
                                Loop
                                Call db.cs_Close(CSLog)
                                '
                                ' ----- Make a log entry to track the result of this email drop
                                '
                                emailDropId = docProperties.getInteger(RequestNameEmailBlockRequestDropID)
                                If emailDropId <> 0 Then
                                    '
                                    ' ----- Email click detected. Log it.
                                    '
                                    EmailMemberID = docProperties.getInteger(RequestNameEmailMemberID)
                                    CSLog = db.cs_insertRecord("Email Log")
                                    If db.cs_ok(CSLog) Then
                                        Call db.cs_set(CSLog, "Name", "Email Block Request " & CStr(app_startTime))
                                        Call db.cs_set(CSLog, "EmailDropID", emailDropId)
                                        Call db.cs_set(CSLog, "MemberID", EmailMemberID)
                                        Call db.cs_set(CSLog, "VisitId", authContext.visit.ID)
                                        Call db.cs_set(CSLog, "LogType", EmailLogTypeBlockRequest)
                                    End If
                                    Call db.cs_Close(CSLog)
                                End If
                                Call webServer.redirect(webServer.webServerIO_requestProtocol & webServer.requestDomain & "/ccLib/popup/EmailBlocked.htm", "Group Email Spam Block hit. Redirecting to EmailBlocked page.", False)
                            End If
                        End If
                        '
                        '--------------------------------------------------------------------------
                        '   Process Intercept Pages
                        '       must be before main_Get Intercept Pages
                        '       must be before path block, so a login will main_Get you through
                        '       must be before verbose check, so a change is reflected on this page
                        '--------------------------------------------------------------------------
                        '
                        If True Then
                            Dim formType As String
                            Dim StyleSN As Integer
                            formType = docProperties.getText("type")
                            If (formType <> "") Then
                                '
                                ' set the meta content flag to show it is not needed for the head tag
                                '
                                Call htmlDoc.main_SetMetaContent(0, 0)
                                Select Case formType
                                    Case FormTypeSiteStyleEditor
                                        If authContext.isAuthenticated() And authContext.isAuthenticatedAdmin(Me) Then
                                            '
                                            ' Save the site sites
                                            '
                                            Call appRootFiles.saveFile(DynamicStylesFilename, docProperties.getText("SiteStyles"))
                                            If docProperties.getBoolean(RequestNameInlineStyles) Then
                                                '
                                                ' Inline Styles
                                                '
                                                Call siteProperties.setProperty("StylesheetSerialNumber", "0")
                                            Else
                                                '
                                                ' Linked Styles
                                                ' Bump the Style Serial Number so next fetch is not cached
                                                '
                                                StyleSN = siteProperties.getinteger("StylesheetSerialNumber", 0)
                                                StyleSN = StyleSN + 1
                                                Call siteProperties.setProperty("StylesheetSerialNumber", genericController.encodeText(StyleSN))
                                                '
                                                ' Save new public stylesheet
                                                '
                                                Call appRootFiles.saveFile("templates\Public" & StyleSN & ".css", pages.pageManager_GetStyleSheet)
                                                Call appRootFiles.saveFile("templates\Admin" & StyleSN & ".css", pages.pageManager_GetStyleSheetDefault)
                                            End If
                                        End If
                                    Case FormTypeAddonStyleEditor
                                        '
                                        ' save custom styles
                                        '
                                        If authContext.isAuthenticated() And authContext.isAuthenticatedAdmin(Me) Then
                                            Dim addonId As Integer
                                            Dim contentName As String = ""
                                            Dim tableName As String
                                            Dim nothingObject As Object = Nothing
                                            Dim cs As Integer
                                            addonId = docProperties.getInteger("AddonID")
                                            cs = db.csOpen2(cnAddons, addonId)
                                            If db.cs_ok(cs) Then
                                                Call db.cs_set(cs, "CustomStylesFilename", docProperties.getText("CustomStyles"))
                                            End If
                                            Call db.cs_Close(cs)
                                            '
                                            ' Clear Caches
                                            '
                                            Call pages.cache_pageContent_clear()
                                            Call pages.pageManager_cache_pageTemplate_clear()
                                            Call pages.pageManager_cache_siteSection_clear()
                                            'Call cache.invalidateObjectList("")
                                            If contentName <> "" Then
                                                Call cache.invalidateContent(contentName)
                                                tableName = metaData.getContentTablename(contentName)
                                                If genericController.vbLCase(tableName) = "cctemplates" Then
                                                    Call cache.setObject(pagesController.cache_pageTemplate_cacheName, nothingObject)
                                                    Call pages.pageManager_cache_pageTemplate_load()
                                                End If
                                            End If
                                        End If
                                    Case FormTypeAddonSettingsEditor
                                        '
                                        '
                                        '
                                        Call htmlDoc.pageManager_ProcessAddonSettingsEditor()
                                    Case FormTypeHelpBubbleEditor
                                        '
                                        '
                                        '
                                        Call htmlDoc.main_ProcessHelpBubbleEditor()
                                    Case FormTypeJoin
                                        '
                                        '
                                        '
                                        Call htmlDoc.processFormJoin()
                                    Case FormTypeSendPassword
                                        '
                                        '
                                        '
                                        Call htmlDoc.processFormSendPassword()
                                    Case FormTypeLogin, "l09H58a195"
                                        '
                                        '
                                        '
                                        Call htmlDoc.processFormLoginDefault()
                                    Case FormTypeToolsPanel
                                        '
                                        ' ----- Administrator Tools Panel
                                        '
                                        Call htmlDoc.pageManager_ProcessFormToolsPanel()
                                    Case FormTypePageAuthoring
                                        '
                                        ' ----- Page Authoring Tools Panel
                                        '
                                        Call pages.pageManager_ProcessFormQuickEditing()
                                    Case FormTypeActiveEditor
                                        '
                                        ' ----- Active Editor
                                        '
                                        Call editorController.processActiveEditor(Me)
                                End Select
                            End If
                        End If
                        '
                        '--------------------------------------------------------------------------
                        ' Process HardCoded Methods
                        ' must go after form processing bc some of these pages have forms that are processed
                        '--------------------------------------------------------------------------
                        '
                        Dim HardCodedPage As String
                        HardCodedPage = docProperties.getText(RequestNameHardCodedPage)
                        If (HardCodedPage <> "") Then
                            '
                            'Call AppendLog("main_init(), 3110 - exit for hardcodedpage hook")
                            '
                            Dim ExitNow As Boolean = executeRoute_hardCodedPage(HardCodedPage)
                            If ExitNow Then
                                docOpen = False '--- should be disposed by caller --- Call dispose
                                Return htmlDoc.docBuffer
                            End If
                        End If
                        '
                        '--------------------------------------------------------------------------
                        ' normalize adminRoute and test for hit
                        '--------------------------------------------------------------------------
                        '
                        If (workingRoute = genericController.normalizeRoute(adminRoute)) Then
                            '
                            'debugLog("executeRoute, route is admin")
                            '
                            '--------------------------------------------------------------------------
                            ' route is admin
                            '   If the Then admin route Is taken -- the login panel processing Is bypassed. those methods need To be a different kind Of route, Or it should be an addon
                            '   runAtServerClass in the admin addon.
                            '--------------------------------------------------------------------------
                            '
                            Dim returnStatusOK As Boolean = False
                            '
                            ' REFACTOR -- when admin code is broken cleanly into an addon, run it through execute
                            '
                            'returnResult = executeAddon(0, adminSiteAddonGuid, "", CPUtilsBaseClass.addonContext.ContextAdmin, "", 0, "", "", False, 0, "", returnStatusOK, Nothing, "", Nothing, "", authcontext.user.userid, visit.visitAuthenticated)
                            '
                            ' until then, run it as an internal class
                            '
                            Dim admin As New Contensive.Addons.addon_AdminSiteClass()
                            returnResult = admin.execute(cp_forAddonExecutionOnly).ToString()
                        Else
                            '--------------------------------------------------------------------------
                            ' default routing addon takes what is left
                            '
                            ' Here was read a site property set to the default addon. Might be performanceCloud-type web application. Might be page-manager
                            '
                            '--------------------------------------------------------------------------
                            '
                            'debugLog("executeRoute, route is Default Route AddonId")
                            '
                            Dim defaultAddonId As Integer = siteProperties.getinteger("Default Route AddonId")
                            If (defaultAddonId = 0) Then
                                '
                                ' -- no default route set, assume html hit
                                returnResult = "<p>This site is not configured for website traffic. Please set the default route.</p>"
                            Else
                                Dim addonStatusOk As Boolean = False
                                returnResult = addon.execute(defaultAddonId, "", "", CPUtilsBaseClass.addonContext.ContextPage, "", 0, "", "", False, 0, "", addonStatusOk, Nothing, "", Nothing, "", authContext.user.ID, authContext.visit.VisitAuthenticated)
                                If (Not addonStatusOk) Then
                                    '
                                    ' -- there was an error in the default route addon
                                    returnResult = "<p>This site is temporarily unavailable.</p>"
                                Else
                                End If
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                Call handleExceptionAndContinue(ex)
            End Try
            Return returnResult
        End Function
        '
        '=================================================================================================
        '   Run and return results from a remotequery call from cj.ajax.data(handler,key,args,pagesize,pagenumber)
        '
        '   This routine builds an xml object inside a <result></result> node.
        '       Right now, the response is in JSON format, and conforms to the google data visualization spec 0.5
        '
        '
        '=================================================================================================
        '
        Private Function executeRoute_ProcessAjaxData() As String
            Dim result As String = ""
            Try
                Dim SetPairs() As String
                Dim Pos As Integer
                Dim FieldValue As String
                Dim SetPairString As String
                Dim ArgCnt As Integer
                Dim s As New stringBuilderLegacyController
                Dim FieldName As String
                Dim Copy As String
                Dim PageSize As Integer
                Dim ArgArray() As String
                Dim RemoteKey As String
                Dim EncodedArgs As String
                Dim Args As String
                Dim PageNumber As Integer
                Dim CS As Integer
                Dim SQLQuery As String
                Dim maxRows As Integer
                Dim ArgName() As String
                Dim ArgValue() As String
                Dim Ptr As Integer
                Dim QueryType As Integer
                Dim ContentName As String = ""
                Dim Criteria As String
                Dim SortFieldList As String = ""
                Dim AllowInactiveRecords2 As Boolean
                Dim SelectFieldList As String = ""
                Dim gd As New GoogleDataType
                Dim gv As New GoogleVisualizationType
                Dim RemoteFormat As RemoteFormatEnum
                'Dim DataSource As Models.Entity.dataSourceModel
                '
                gv.status = GoogleVisualizationStatusEnum.OK
                gd.IsEmpty = True
                '
                RemoteKey = docProperties.getText("key")
                EncodedArgs = docProperties.getText("args")

                PageSize = docProperties.getInteger("pagesize")
                PageNumber = docProperties.getInteger("pagenumber")
                Select Case genericController.vbLCase(docProperties.getText("responseformat"))
                    Case "jsonnamevalue"
                        RemoteFormat = RemoteFormatEnum.RemoteFormatJsonNameValue
                    Case "jsonnamearray"
                        RemoteFormat = RemoteFormatEnum.RemoteFormatJsonNameArray
                    Case Else 'jsontable
                        RemoteFormat = RemoteFormatEnum.RemoteFormatJsonTable
                End Select
                '
                ' Handle common work
                '
                If PageNumber = 0 Then
                    PageNumber = 1
                End If
                If PageSize = 0 Then
                    PageSize = 100
                End If
                If maxRows <> 0 And PageSize > maxRows Then
                    PageSize = maxRows
                End If
                '
                If EncodedArgs <> "" Then
                    Args = EncodedArgs
                    ArgArray = Split(Args, "&")
                    ArgCnt = UBound(ArgArray) + 1
                    ReDim ArgName(ArgCnt)
                    ReDim ArgValue(ArgCnt)
                    For Ptr = 0 To ArgCnt - 1
                        Pos = genericController.vbInstr(1, ArgArray(Ptr), "=")
                        If Pos > 0 Then
                            ArgName(Ptr) = genericController.DecodeResponseVariable(Mid(ArgArray(Ptr), 1, Pos - 1))
                            ArgValue(Ptr) = genericController.DecodeResponseVariable(Mid(ArgArray(Ptr), Pos + 1))
                        End If
                    Next
                End If
                '
                ' main_Get values out of the remote query record
                '
                If gv.status = GoogleVisualizationStatusEnum.OK Then
                    CS = db.cs_open("Remote Queries", "((VisitId=" & authContext.visit.ID & ")and(remotekey=" & db.encodeSQLText(RemoteKey) & "))")
                    If db.cs_ok(CS) Then
                        '
                        ' Use user definied query
                        '
                        SQLQuery = db.cs_getText(CS, "sqlquery")
                        'DataSource = Models.Entity.dataSourceModel.create(Me, db.cs_getInteger(CS, "datasourceid"), New List(Of String))
                        maxRows = db.cs_getInteger(CS, "maxrows")
                        QueryType = db.cs_getInteger(CS, "QueryTypeID")
                        ContentName = db.cs_get(CS, "ContentID")
                        Criteria = db.cs_getText(CS, "Criteria")
                        SortFieldList = db.cs_getText(CS, "SortFieldList")
                        AllowInactiveRecords2 = db.cs_getBoolean(CS, "AllowInactiveRecords")
                        SelectFieldList = db.cs_getText(CS, "SelectFieldList")
                        SetPairString = ""
                    Else
                        '
                        ' Try Hardcoded queries
                        '
                        Select Case genericController.vbLCase(RemoteKey)
                            Case "ccfieldhelpupdate"
                                '
                                ' developers editing field help
                                '
                                If Not authContext.user.Developer Then
                                    gv.status = GoogleVisualizationStatusEnum.ErrorStatus
                                    If IsArray(gv.errors) Then
                                        Ptr = 0
                                    Else
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
                    Call db.cs_Close(CS)
                    '
                    If gv.status = GoogleVisualizationStatusEnum.OK Then
                        Select Case QueryType
                        'Case QueryTypeSQL
                        '    '
                        '    ' ----- Run a SQL
                        '    '
                        '    If SQLQuery <> "" Then
                        '        For Ptr = 0 To ArgCnt - 1
                        '            SQLQuery = genericController.vbReplace(SQLQuery, ArgName(Ptr), ArgValue(Ptr), vbTextCompare)
                        '            'Criteria = genericController.vbReplace(Criteria, ArgName(Ptr), ArgValue(Ptr), vbTextCompare)
                        '        Next
                        '        On Error Resume Next
                        '        RS = main_ExecuteSQLCommand(DataSource, SQLQuery, 30, PageSize, PageNumber)
                        '        ErrorNumber = Err.Number
                        '        ErrorDescription = Err.Description
                        '        Err.Clear()
                        '        On Error GoTo ErrorTrap
                        '        If ErrorNumber <> 0 Then
                        '            '
                        '            ' ----- Error
                        '            '
                        '            gv.status = GoogleVisualizationStatusEnum.ErrorStatus
                        '            Ptr = UBound(gv.errors) + 1
                        '            ReDim gv.errors(Ptr)
                        '            gv.errors(Ptr) = "Error: " & Err.Description
                        '        ElseIf (Not isDataTableOk(rs)) Then
                        '            '
                        '            ' ----- no result
                        '            '
                        '        ElseIf (RS.State <> 1) Then
                        '            '
                        '            ' ----- no result
                        '            '
                        '        ElseIf (rs.rows.count = 0) Then
                        '            '
                        '            ' ----- no result
                        '            '
                        '        Else
                        '            PageSize = RS.PageSize
                        '            Cells = RS.GetRows(PageSize)
                        '            '
                        '            gd.IsEmpty = False
                        '            RowMax = UBound(Cells, 2)
                        '            ColMax = UBound(Cells, 1)
                        '            '
                        '            ' Build headers
                        '            '
                        '            ReDim gd.col(ColMax)
                        '            For ColPtr = 0 To ColMax
                        '                RecordField = RS.Fields.Item(ColPtr)
                        '                gd.col(ColPtr).Id = RecordField.Name
                        '                gd.col(ColPtr).Label = RecordField.Name
                        '                gd.col(ColPtr).Type = ConvertRSTypeToGoogleType(RecordField.Type)
                        '            Next
                        '            'RS.Close()
                        '            'RS = Nothing
                        '            '
                        '            ' Build output table
                        '            '
                        '            ReDim gd.row(RowMax)
                        '            For RowPtr = 0 To RowMax
                        '                With gd.row(RowPtr)
                        '                    ReDim .Cell(ColMax)
                        '                    For ColPtr = 0 To ColMax
                        '                        .Cell(ColPtr).v = genericController.encodeText(Cells(ColPtr, RowPtr))
                        '                    Next
                        '                End With
                        '            Next
                        '        End If
                        '        If (isDataTableOk(rs)) Then
                        '            If False Then
                        '                'RS.Close()
                        '            End If
                        '            'RS = Nothing
                        '        End If
                        '    End If
                        'Case QueryTypeOpenContent
                        '    '
                        '    ' Contensive Content Select, args are criteria replacements
                        '    '

                        '    CDef = app.getCdef(ContentName)
                        '    CS = app.csOpen(ContentName, Criteria, SortFieldList, AllowInactiveRecords, , , SelectFieldList)
                        '    Cells = app.csv_cs_getRows(CS)
                        '    FieldList = app.cs_getSelectFieldList(CS)
                        '    '
                        '    RowMax = UBound(Cells, 2)
                        '    ColMax = UBound(Cells, 1)
                        '    If RowMax = 0 And ColMax = 0 Then
                        '        '
                        '        ' Single result, display with no table
                        '        '
                        '        Copy = genericController.encodeText(Cells(0, 0))
                        '    Else
                        '        '
                        '        ' Build headers
                        '        '
                        '        gd.IsEmpty = False
                        '        RowMax = UBound(Cells, 2)
                        '        ColMax = UBound(Cells, 1)
                        '        '
                        '        ' Build headers
                        '        '
                        '        ReDim gd.col(ColMax)
                        '        For ColPtr = 0 To ColMax
                        '            RecordField = RS.Fields.Item(RowPtr)
                        '            gd.col(ColPtr).Id = RecordField.Name
                        '            gd.col(ColPtr).Label = RecordField.Name
                        '            gd.col(ColPtr).Type = ConvertRSTypeToGoogleType(RecordField.Type)
                        '        Next
                        '        '
                        '        ' Build output table
                        '        '
                        '        'RowStart = vbCrLf & "<Row>"
                        '        'Rowend = "</Row>"
                        '        For RowPtr = 0 To RowMax
                        '            With gd.row(RowPtr)
                        '                For ColPtr = 0 To ColMax
                        '                    .Cell(ColPtr).v = Cells(ColPtr, RowPtr)
                        '                Next
                        '            End With
                        '        Next
                        '    End If
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
                                For Ptr = 0 To ArgCnt - 1
                                    If genericController.vbLCase(ArgName(Ptr)) = "setpairs" Then
                                        SetPairString = ArgValue(Ptr)
                                    ElseIf genericController.vbLCase(ArgName(Ptr)) = "criteria" Then
                                        Criteria = ArgValue(Ptr)
                                    End If
                                Next
                                '
                                ' Open the content and cycle through each setPair
                                '
                                CS = db.cs_open(ContentName, Criteria, SortFieldList, AllowInactiveRecords2, , ,, SelectFieldList)
                                If db.cs_ok(CS) Then
                                    '
                                    ' update by looping through the args and setting name=values
                                    '
                                    SetPairs = Split(SetPairString, "&")
                                    For Ptr = 0 To UBound(SetPairs)
                                        If SetPairs(Ptr) <> "" Then
                                            Pos = genericController.vbInstr(1, SetPairs(Ptr), "=")
                                            If Pos > 0 Then
                                                FieldValue = genericController.DecodeResponseVariable(Mid(SetPairs(Ptr), Pos + 1))
                                                FieldName = genericController.DecodeResponseVariable(Mid(SetPairs(Ptr), 1, Pos - 1))
                                                If Not metaData.isContentFieldSupported(ContentName, FieldName) Then
                                                    Dim errorMessage As String = "result, QueryTypeUpdateContent, key [" & RemoteKey & "], bad field [" & FieldName & "] skipped"
                                                    Throw (New ApplicationException(errorMessage))
                                                Else
                                                    Call db.cs_set(CS, FieldName, FieldValue)
                                                End If
                                            End If
                                        End If
                                    Next
                                End If
                                Call db.cs_Close(CS)
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
                        Copy = remoteQueryController.main_FormatRemoteQueryOutput(Me, gd, RemoteFormat)
                        Copy = htmlDoc.html_EncodeHTML(Copy)
                        result = "<data>" & Copy & "</data>"
                    End If
                End If
            Catch ex As Exception
                Throw (ex)
            End Try
            Return result
        End Function




        '
        '
        '
        '=========================================================================================
        '   In Init(), Print Hard Coded Pages
        '       A Hard coded page replaces the entire output with an HTML compatible page
        '=========================================================================================
        '
        Private Function executeRoute_hardCodedPage(ByVal HardCodedPage As String) As Boolean
            Dim result As Boolean = False
            Try
                Dim allowPageWithoutSectionDisplay As Boolean
                Dim InsertTestOK As Boolean
                Dim ConfirmOrderID As Integer
                Dim PageSize As Integer
                Dim PageNumber As Integer
                Dim ContentName As String
                Dim MsgLabel As String
                Dim PageID As Integer
                Dim rootPageId As Integer
                Dim Pos As Integer
                Dim TrapID As Integer
                Dim CS As Integer
                Dim Name As String
                Dim Copy As String
                Dim Recipient As String
                Dim Sender As String
                Dim subject As String
                Dim Message As String
                Dim Emailtext As String
                Dim LinkObjectName As String
                Dim EditorObjectName As String
                '
                Select Case genericController.vbLCase(HardCodedPage)
                    Case HardCodedPageSendPassword
                        '
                        ' send password to the email address in the querystring
                        '
                        Emailtext = docProperties.getText("email")
                        If Emailtext <> "" Then
                            Call email.sendPassword(Emailtext)
                            Copy = "" _
                            & "<div style=""width:300px;margin:100px auto 0 auto;"">" _
                            & "<p>An attempt to send login information for email address '" & Emailtext & "' has been made.</p>" _
                            & "<p><a href=""?" & htmlDoc.refreshQueryString & """>Return to the Site.</a></p>" _
                            & "</div>"
                            Call htmlDoc.writeAltBuffer(Copy)
                            result = True
                        Else
                            result = False
                        End If
                'Case HardCodedPagePrinterVersion
                '    '
                '    ' ----- Page Content Printer main_version
                '    '
                '    Call htmlDoc.webServerIO_addRefreshQueryString(RequestNameHardCodedPage, HardCodedPagePrinterVersion)
                '    htmlDoc.pageManager_printVersion = True
                '    autoPrintText = docProperties.getText("AutoPrint")
                '    '
                '    If ContentName = "" Then
                '        ContentName = "Page Content"
                '    End If
                '    If autoPrintText = "" Then
                '        autoPrintText = siteProperties.getText("AllowAutoPrintDialog", "1")
                '    End If
                '    If RootPageName = "" Then
                '        blockSiteWithLogin = False
                '        PageCopy = pages.pageManager_GetHtmlBody_GetSection(AllowChildPage, False, False, blockSiteWithLogin)
                '        'PageCopy = main_GetSectionPage(AllowChildPage, False)
                '    Else
                '        OrderByClause = docProperties.getText(RequestNameOrderByClause)
                '        PageID = docProperties.getInteger("bid")
                '        '
                '        ' 5/12/2008 - converted to RootPageID call because we do not use RootPageName anymore
                '        '
                '        allowPageWithoutSectionDisplay = siteProperties.getBoolean(spAllowPageWithoutSectionDisplay, spAllowPageWithoutSectionDisplay_default)
                '        If Not allowPageWithoutSectionDisplay Then
                '            allowPageWithoutSectionDisplay = authContext.isAuthenticatedContentManager(Me, ContentName)
                '        End If
                '        PageCopy = pages.pageManager_GetHtmlBody_GetSection_GetContent(PageID, rootPageId, ContentName, OrderByClause, False, False, False, 0, siteProperties.useContentWatchLink, allowPageWithoutSectionDisplay)
                '        If pages.redirectLink <> "" Then
                '            Call webServer.webServerIO_Redirect2(pages.redirectLink, pages.pageManager_RedirectReason, False)
                '        End If
                '        'PageCopy = main_GetContentPage(RootPageName, ContentName, OrderByClause, AllowChildPage, False, PageID)
                '    End If
                '    '
                '    If genericController.EncodeBoolean(autoPrintText) Then
                '        Call htmlDoc.main_AddOnLoadJavascript2("window.print(); window.close()", "Print Page")
                '    End If
                '    BodyOpen = "<body class=""ccBodyPrint"">"

                '    'Call AppendLog("call main_getEndOfBody, from main_init_printhardcodedpage")
                '    Call htmlDoc.writeAltBuffer("" _
                '        & main_docType _
                '        & vbCrLf & "<html>" _
                '        & cr & "<head>" & htmlDoc.getHTMLInternalHead(False) _
                '        & cr & "</head>" _
                '        & vbCrLf & BodyOpen _
                '        & cr & "<div align=""left"">" _
                '        & cr2 & "<table border=""0"" cellpadding=""20"" cellspacing=""0"" width=""100%""><tr><td width=""100%"">" _
                '        & cr3 & "<p>" _
                '        & genericController.kmaIndent(PageCopy) _
                '        & cr3 & "</p>" _
                '        & cr2 & "</td></tr></table>" _
                '        & cr & "</div>" _
                '        & genericController.kmaIndent(htmlDoc.html_GetEndOfBody(False, False, False, False)) _
                '        & cr & "</body>" _
                '        & vbCrLf & "</html>" _
                '        & "")

                '    result = True
                ''Case HardCodedPageMyProfile
                ''    '
                ''    ' Print a User Profile page with the current member
                ''    '
                ''    Call web_addRefreshQueryString(RequestNameHardCodedPage, HardCodedPageMyProfile)
                ''    Call writeAltBuffer(main_GetMyProfilePage())
                ''    result = True
                    Case HardCodedPageResourceLibrary
                        '
                        ' main_Get FormIndex (the index to the InsertImage# function called on selection)
                        '
                        Call htmlDoc.webServerIO_addRefreshQueryString(RequestNameHardCodedPage, HardCodedPageResourceLibrary)
                        EditorObjectName = docProperties.getText("EditorObjectName")
                        LinkObjectName = docProperties.getText("LinkObjectName")
                        If EditorObjectName <> "" Then
                            '
                            ' Open a page compatible with a dialog
                            '
                            Call htmlDoc.webServerIO_addRefreshQueryString("EditorObjectName", EditorObjectName)
                            Call htmlDoc.main_AddHeadScriptLink("/ccLib/ClientSide/dialogs.js", "Resource Library")
                            'Call AddHeadScript("<script type=""text/javascript"" src=""/ccLib/ClientSide/dialogs.js""></script>")
                            Call htmlDoc.main_SetMetaContent(0, 0)
                            Call htmlDoc.main_AddOnLoadJavascript2("document.body.style.overflow='scroll';", "Resource Library")
                            Copy = htmlDoc.main_GetResourceLibrary2("", True, EditorObjectName, LinkObjectName, True)
                            'Call AppendLog("call main_getEndOfBody, from main_init_printhardcodedpage2b")
                            Copy = "" _
                            & siteProperties.docTypeDeclaration() _
                            & "<html>" _
                            & cr & "<head>" _
                            & genericController.kmaIndent(htmlDoc.getHTMLInternalHead(False)) _
                            & cr & "</head>" _
                            & cr & "<body class=""ccBodyAdmin ccCon"" style=""overflow:scroll"">" _
                            & genericController.kmaIndent(main_GetPanelHeader("Contensive Resource Library")) _
                            & cr & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%""><tr><td>" _
                            & cr2 & "<div style=""border-top:1px solid white;border-bottom:1px solid black;height:2px""><img alt=""spacer"" src=""/ccLib/images/spacer.gif"" width=1 height=1></div>" _
                            & genericController.kmaIndent(Copy) _
                            & cr & "</td></tr>" _
                            & cr & "<tr><td>" _
                            & genericController.kmaIndent(htmlDoc.getBeforeEndOfBodyHtml(False, False, False, False)) _
                            & cr & "</td></tr></table>" _
                            & cr & "<script language=javascript type=""text/javascript"">fixDialog();</script>" _
                            & cr & "</body>" _
                            & "</html>"
                            Call htmlDoc.writeAltBuffer(Copy)
                            result = True
                            'Call main_GetEndOfBody(False, False)
                            ''--- should be disposed by caller --- Call dispose
                            'Call main_CloseStream
                            'true = False
                            'Set main_cmc = Nothing
                            'Exit Sub
                            'Call main_CloseStream
                        ElseIf LinkObjectName <> "" Then
                            '
                            ' Open a page compatible with a dialog
                            '
                            Call htmlDoc.webServerIO_addRefreshQueryString("LinkObjectName", LinkObjectName)
                            Call htmlDoc.main_AddHeadScriptLink("/ccLib/ClientSide/dialogs.js", "Resource Library")
                            'Call AddHeadScript("<script type=""text/javascript"" src=""/ccLib/ClientSide/dialogs.js""></script>")
                            Call htmlDoc.main_SetMetaContent(0, 0)
                            Call htmlDoc.main_AddOnLoadJavascript2("document.body.style.overflow='scroll';", "Resource Library")
                            Copy = htmlDoc.main_GetResourceLibrary2("", True, EditorObjectName, LinkObjectName, True)
                            'Call AppendLog("call main_getEndOfBody, from main_init_printhardcodedpage2c")
                            Copy = "" _
                            & siteProperties.docTypeDeclaration() _
                            & cr & "<html>" _
                            & cr & "<head>" _
                            & genericController.kmaIndent(htmlDoc.getHTMLInternalHead(False)) _
                            & cr & "</head>" _
                            & cr & "<body class=""ccBodyAdmin ccCon"" style=""overflow:scroll"">" _
                            & main_GetPanelHeader("Contensive Resource Library") _
                            & cr & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%""><tr><td>" _
                            & Copy _
                            & cr & "</td></tr><tr><td>" & htmlDoc.getBeforeEndOfBodyHtml(False, False, False, False) & "</td></tr></table>" _
                            & cr & "<script language=javascript type=text/javascript>fixDialog();</script>" _
                            & cr & "</body>" _
                            & vbCrLf & "</html>"
                            Call htmlDoc.writeAltBuffer(Copy)
                            result = True
                        End If
                    Case HardCodedPageLoginDefault
                        '
                        ' 9/4/2012 added to prevent lockout if login addon fails
                        htmlDoc.refreshQueryString = webServer.requestQueryString
                        'Call main_AddRefreshQueryString("method", "")
                        Call htmlDoc.writeAltBuffer(htmlDoc.getLoginPage(True))
                        result = True
                    Case HardCodedPageLogin, HardCodedPageLogoutLogin
                        '
                        ' 7/8/9 - Moved from intercept pages
                        '
                        ' Print the Login form as an intercept page
                        ' Special case - set the current URL to the Refresh Query String
                        ' Because you want the form created to save the refresh values
                        '
                        If genericController.vbUCase(HardCodedPage) = "LOGOUTLOGIN" Then
                            Call authContext.logout(Me)
                        End If
                        htmlDoc.refreshQueryString = webServer.requestQueryString
                        'Call main_AddRefreshQueryString("method", "")
                        Call htmlDoc.writeAltBuffer(htmlDoc.getLoginPage(False))
                        'Call writeAltBuffer(main_GetLoginPage2(false) & main_GetEndOfBody(False, False, False))
                        result = True
                    Case HardCodedPageLogout
                        '
                        ' ----- logout the current member
                        '
                        Call authContext.logout(Me)
                        result = False
                    Case HardCodedPageSiteExplorer
                        '
                        ' 7/8/9 - Moved from intercept pages
                        '
                        Call htmlDoc.webServerIO_addRefreshQueryString(RequestNameHardCodedPage, HardCodedPageSiteExplorer)
                        LinkObjectName = docProperties.getText("LinkObjectName")
                        If LinkObjectName <> "" Then
                            '
                            ' Open a page compatible with a dialog
                            '
                            Call htmlDoc.webServerIO_addRefreshQueryString("LinkObjectName", LinkObjectName)
                            Call htmlDoc.main_AddPagetitle("Site Explorer")
                            Call htmlDoc.main_SetMetaContent(0, 0)
                            Copy = addon.execute_legacy5(0, "Site Explorer", "", CPUtilsBaseClass.addonContext.ContextPage, "", 0, "", 0)
                            Call htmlDoc.main_AddOnLoadJavascript2("document.body.style.overflow='scroll';", "Site Explorer")
                            'Call AppendLog("call main_getEndOfBody, from main_init_printhardcodedpage2d")
                            Copy = "" _
                            & siteProperties.docTypeDeclaration() _
                            & cr & "<html>" _
                            & cr & "<head>" _
                            & genericController.kmaIndent(htmlDoc.getHTMLInternalHead(False)) _
                            & cr & "</head>" _
                            & cr & "<body class=""ccBodyAdmin ccCon"" style=""overflow:scroll"">" _
                            & genericController.kmaIndent(main_GetPanelHeader("Contensive Site Explorer")) _
                            & cr & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%""><tr><td>" _
                            & genericController.kmaIndent(Copy) _
                            & cr & "</td></tr><tr><td>" & htmlDoc.getBeforeEndOfBodyHtml(False, False, False, False) & "</td></tr></table>" _
                            & cr & "</body>" _
                            & cr & "</html>"
                            'Set Obj = Nothing
                            Call htmlDoc.writeAltBuffer(Copy)
                            result = True
                        End If
                    Case HardCodedPageStatus
                        '
                        ' Status call
                        '
                        webServer.webServerIO_BlockClosePageCopyright = True
                        '
                        ' test default data connection
                        '
                        InsertTestOK = False
                        CS = db.cs_insertRecord("Trap Log")
                        If Not db.cs_ok(CS) Then
                            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError10(ignoreInteger, "dll", "Error during Status. Called InsertCSRecord to insert 'Trap Log' test, record set was not OK.", "Init", False, True)
                        Else
                            InsertTestOK = True
                            TrapID = db.cs_getInteger(CS, "ID")
                        End If
                        Call db.cs_Close(CS)
                        If InsertTestOK Then
                            If TrapID = 0 Then
                                Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError10(ignoreInteger, "dll", "Error during Status. Called InsertCSRecord to insert 'Trap Log' test, record set was OK, but ID=0.", "Init", False, True)
                            Else
                                Call db.deleteContentRecord("Trap Log", TrapID)
                            End If
                        End If
                        If Err.Number <> 0 Then
                            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError10(ignoreInteger, "dll", "Error during Status. After traplog insert, " & genericController.GetErrString(Err), "Init", False, True)
                            Err.Clear()
                        End If
                        '
                        ' Close page
                        '
                        Call main_ClearStream()
                        If app_errorCount = 0 Then
                            Call htmlDoc.writeAltBuffer("Contensive OK")
                        Else
                            Call htmlDoc.writeAltBuffer("Contensive Error Count = " & app_errorCount)
                        End If
                        webServer.webServerIO_BlockClosePageCopyright = True
                        html_BlockClosePageLink = True
                        'Call AppendLog("call main_getEndOfBody, from main_init_printhardcodedpage2f")
                        Call htmlDoc.getBeforeEndOfBodyHtml(False, False, False, False)
                        result = True
                    Case HardCodedPageGetJSPage
                        '
                        ' ----- Create a Javascript page that outputs a page content record
                        '
                        Name = docProperties.getText("name")
                        If Name <> "" Then
                            webServer.webServerIO_BlockClosePageCopyright = True
                            '
                            ' Determine bid (PageID) from referer querystring
                            '
                            Copy = webServer.requestReferrer
                            Pos = genericController.vbInstr(1, Copy, "bid=")
                            If Pos <> 0 Then
                                Copy = Trim(Mid(Copy, Pos + 4))
                                Pos = genericController.vbInstr(1, Copy, "&")
                                If Pos <> 0 Then
                                    Copy = Trim(Mid(Copy, 1, Pos))
                                End If
                                PageID = genericController.EncodeInteger(Copy)
                            End If
                            '
                            ' main_Get the page
                            '
                            rootPageId = db.getRecordID("Page Content", Name)
                            allowPageWithoutSectionDisplay = siteProperties.getBoolean(spAllowPageWithoutSectionDisplay, spAllowPageWithoutSectionDisplay_default)
                            If Not allowPageWithoutSectionDisplay Then
                                allowPageWithoutSectionDisplay = authContext.isAuthenticatedContentManager(Me, ContentName)
                            End If
                            Copy = pages.getContentBox(PageID, rootPageId, "Page Content", "", True, True, False, 0, siteProperties.useContentWatchLink, allowPageWithoutSectionDisplay)
                            'Call AppendLog("call main_getEndOfBody, from main_init_printhardcodedpage2g")
                            Copy = Copy & htmlDoc.getBeforeEndOfBodyHtml(False, True, False, False)
                            Copy = genericController.vbReplace(Copy, "'", "'+""'""+'")
                            Copy = genericController.vbReplace(Copy, vbCr, "\n")
                            Copy = genericController.vbReplace(Copy, vbLf, " ")
                            '
                            ' Write the page to the stream, with a javascript wrapper
                            '
                            MsgLabel = "Msg" & genericController.encodeText(genericController.GetRandomInteger)
                            Call webServer.setResponseContentType("text/plain")
                            Call htmlDoc.writeAltBuffer("var " & MsgLabel & " = '" & Copy & "'; " & vbCrLf)
                            Call htmlDoc.writeAltBuffer("document.write( " & MsgLabel & " ); " & vbCrLf)
                        End If
                        result = True
                    Case HardCodedPageGetJSLogin
                        '
                        ' ----- Create a Javascript login page
                        '
                        webServer.webServerIO_BlockClosePageCopyright = True
                        Copy = Copy & "<p align=""center""><CENTER>"
                        If Not authContext.isAuthenticated() Then
                            Copy = Copy & htmlDoc.getLoginPanel()
                        ElseIf authContext.isAuthenticatedContentManager(Me, "Page Content") Then
                            'Copy = Copy & main_GetToolsPanel
                        Else
                            Copy = Copy & "You are currently logged in as " & authContext.user.Name & ". To logout, click <a HREF=""" & webServer.webServerIO_ServerFormActionURL & "?Method=logout"" rel=""nofollow"">Here</A>."
                        End If
                        'Call AppendLog("call main_getEndOfBody, from main_init_printhardcodedpage2h")
                        Copy = Copy & htmlDoc.getBeforeEndOfBodyHtml(True, True, False, False)
                        Copy = Copy & "</CENTER></p>"
                        Copy = genericController.vbReplace(Copy, "'", "'+""'""+'")
                        Copy = genericController.vbReplace(Copy, vbCr, "")
                        Copy = genericController.vbReplace(Copy, vbLf, "")
                        'Copy = "<b>login Page</b>"
                        '
                        ' Write the page to the stream, with a javascript wrapper
                        '
                        MsgLabel = "Msg" & genericController.encodeText(genericController.GetRandomInteger)
                        Call webServer.setResponseContentType("text/plain")
                        Call htmlDoc.writeAltBuffer("var " & MsgLabel & " = '" & Copy & "'; " & vbCrLf)
                        Call htmlDoc.writeAltBuffer("document.write( " & MsgLabel & " ); " & vbCrLf)
                        result = True
                    Case HardCodedPageRedirect
                        '
                        ' ----- Redirect with RC and RI
                        '
                        htmlDoc.pageManager_RedirectContentID = docProperties.getInteger(rnRedirectContentId)
                        htmlDoc.pageManager_RedirectRecordID = docProperties.getInteger(rnRedirectRecordId)
                        If htmlDoc.pageManager_RedirectContentID <> 0 And htmlDoc.pageManager_RedirectRecordID <> 0 Then
                            ContentName = metaData.getContentNameByID(htmlDoc.pageManager_RedirectContentID)
                            If ContentName <> "" Then
                                Call main_RedirectByRecord_ReturnStatus(ContentName, htmlDoc.pageManager_RedirectRecordID)
                            End If
                        End If
                        webServer.webServerIO_BlockClosePageCopyright = True
                        html_BlockClosePageLink = True
                        result = False '--- should be disposed by caller --- Call dispose
                        result = True
                    Case HardCodedPageExportAscii
                        '
                        '----------------------------------------------------
                        '   Should be a remote method in commerce
                        '----------------------------------------------------
                        '
                        If Not authContext.isAuthenticatedAdmin(Me) Then
                            '
                            ' Administrator required
                            '
                            Call htmlDoc.writeAltBuffer("Error: You must be an administrator to use the ExportAscii method")
                        Else
                            webServer.webServerIO_BlockClosePageCopyright = True
                            ContentName = docProperties.getText("content")
                            PageSize = docProperties.getInteger("PageSize")
                            If PageSize = 0 Then
                                PageSize = 20
                            End If
                            PageNumber = docProperties.getInteger("PageNumber")
                            If PageNumber = 0 Then
                                PageNumber = 1
                            End If
                            If (ContentName = "") Then
                                Call htmlDoc.writeAltBuffer("Error: ExportAscii method requires ContentName")
                            Else
                                Call htmlDoc.writeAltBuffer(Controllers.exportAsciiController.exportAscii_GetAsciiExport(Me, ContentName, PageSize, PageNumber))
                            End If
                        End If
                        result = True
                        webServer.webServerIO_BlockClosePageCopyright = True
                        html_BlockClosePageLink = True
                        result = False '--- should be disposed by caller --- Call dispose
                        result = True
                    Case HardCodedPagePayPalConfirm
                        '
                        '
                        '----------------------------------------------------
                        '   Should be a remote method in commerce
                        '----------------------------------------------------
                        '
                        '
                        ConfirmOrderID = docProperties.getInteger("item_name")
                        If ConfirmOrderID <> 0 Then
                            '
                            ' Confirm the order
                            '
                            CS = db.cs_open("Orders", "(ID=" & ConfirmOrderID & ") and ((OrderCompleted=0)or(OrderCompleted is Null))")
                            If db.cs_ok(CS) Then
                                Call db.cs_set(CS, "OrderCompleted", True)
                                Call db.cs_set(CS, "DateCompleted", app_startTime)
                                Call db.cs_set(CS, "ccAuthCode", docProperties.getText("txn_id"))
                                Call db.cs_set(CS, "ccActionCode", docProperties.getText("payment_status"))
                                Call db.cs_set(CS, "ccRefCode", docProperties.getText("pending_reason"))
                                Call db.cs_set(CS, "PayMethod", "PayPal " & docProperties.getText("payment_type"))
                                Call db.cs_set(CS, "ShipName", docProperties.getText("first_name") & " " & docProperties.getText("last_name"))
                                Call db.cs_set(CS, "ShipAddress", docProperties.getText("address_street"))
                                Call db.cs_set(CS, "ShipCity", docProperties.getText("address_city"))
                                Call db.cs_set(CS, "ShipState", docProperties.getText("address_state"))
                                Call db.cs_set(CS, "ShipZip", docProperties.getText("address_zip"))
                                Call db.cs_set(CS, "BilleMail", docProperties.getText("payer_email"))
                                Call db.cs_set(CS, "ContentControlID", metaData.getContentId("Orders Completed"))
                                Call db.cs_save2(CS)
                            End If
                            Call db.cs_Close(CS)
                            '
                            ' Empty the cart
                            '
                            CS = db.cs_open("Visitors", "OrderID=" & ConfirmOrderID)
                            If db.cs_ok(CS) Then
                                Call db.cs_set(CS, "OrderID", 0)
                                Call db.cs_save2(CS)
                            End If
                            Call db.cs_Close(CS)
                            '
                            ' TEmp fix until HardCodedPage is complete
                            '
                            Recipient = siteProperties.getText("EmailOrderNotifyAddress", siteProperties.emailAdmin)
                            If genericController.vbInstr(genericController.encodeText(Recipient), "@") = 0 Then
                                Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError12("Init", "PayPal confirmation Order Process Notification email was not sent because EmailOrderNotifyAddress SiteProperty is not valid")
                            Else
                                Sender = siteProperties.getText("EmailOrderFromAddress")
                                subject = webServer.webServerIO_requestDomain & " Online Order Pending, #" & ConfirmOrderID
                                Message = "<p>An order confirmation has been recieved from PayPal for " & webServer.webServerIO_requestDomain & "</p>"
                                Call email.send_Legacy(Recipient, Sender, subject, Message, , False, True)
                            End If
                        End If
                        webServer.webServerIO_BlockClosePageCopyright = True
                        html_BlockClosePageLink = True
                        result = False '--- should be disposed by caller --- Call dispose
                        result = True
                End Select
            Catch ex As Exception
                handleExceptionAndContinue(ex)
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
                If errList Is Nothing Then
                    errList = New List(Of String)
                End If
                If errList.Count = 10 Then
                    errList.Add("Exception limit exceeded")
                ElseIf errList.Count < 10 Then
                    errList.Add(errMsg)
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
        Public Sub handleExceptionAndContinue(ByVal ex As Exception, ByVal cause As String)
            Call handleException(ex, cause, 2)
        End Sub
        '
        Public Sub handleExceptionAndContinue(ByVal ex As Exception)
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
                app_startTickCount = GetTickCount
                CPTickCountBase = GetTickCount
                main_ClosePageCounter = 0
                debug_allowDebugLog = True
                app_startTime = DateTime.Now()
                testPointPrinting = True
                '
                ' -- attempt auth load
                If (serverConfig.appConfig Is Nothing) Then
                    authContext = Models.Context.authContextModel.create(Me, False)
                Else
                    authContext = Models.Context.authContextModel.create(Me, siteProperties.allowVisitTracking)
                    '
                    ' debug printed defaults on, so if not on, set it off and clear what was collected
                    If Not visitProperty.getBoolean("AllowDebugging") Then
                        testPointPrinting = False
                        main_testPointMessage = ""
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
        '==========================================================================================
        ''' <summary>
        ''' return an html ul list of each eception produced during this document.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function getDocExceptionHtmlList() As String
            Dim returnHtmlList As String = ""
            Try
                If Not errList Is Nothing Then
                    If errList.Count > 0 Then
                        For Each exMsg As String In errList
                            returnHtmlList &= cr2 & "<li class=""ccExceptionListRow"">" & cr3 & htmlDoc.html_convertText2HTML(exMsg) & cr2 & "</li>"
                        Next
                        returnHtmlList = cr & "<ul class=""ccExceptionList"">" & returnHtmlList & cr & "</ul>"
                    End If
                End If
            Catch ex As Exception
                Throw (ex)
            End Try
            Return returnHtmlList
        End Function





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
            'Exit Sub

            Dim SQL As String
            Dim ViewingName As String
            Dim CSMax As Integer
            Dim PageID As Integer
            Dim FieldNames As String
            Dim Form As String
            '
            If Not Me.disposed Then
                Me.disposed = True
                If disposing Then
                    '
                    ' call .dispose for managed objects
                    ' delete tmp files
                    '
                    If deleteOnDisposeFileList.Count > 0 Then
                        For Each filename As String In deleteOnDisposeFileList
                            privateFiles.deleteFile(filename)
                        Next
                    End If
                    '
                    ' ----- Block all output from underlying routines
                    '
                    blockExceptionReporting = True
                    'docOpen = False
                    Call doc_close()
                    '
                    ' content server object is valid
                    '
                    If (serverConfig.appConfig IsNot Nothing) Then
                        If siteProperties.allowVisitTracking Then
                            '
                            ' If visit tracking, save the viewing record
                            '
                            ViewingName = Left(authContext.visit.ID & "." & authContext.visit.PageVisits, 10)
                            PageID = pages.currentPageID
                            FieldNames = "Name,VisitId,MemberID,Host,Path,Page,QueryString,Form,Referer,DateAdded,StateOK,ContentControlID,pagetime,Active,CreateKey,RecordID"
                            FieldNames = FieldNames & ",ExcludeFromAnalytics"
                            FieldNames = FieldNames & ",pagetitle"
                            SQL = "INSERT INTO ccViewings (" _
                                & FieldNames _
                                & ")VALUES(" _
                                & " " & db.encodeSQLText(ViewingName) _
                                & "," & db.encodeSQLNumber(authContext.visit.ID) _
                                & "," & db.encodeSQLNumber(authContext.user.ID) _
                                & "," & db.encodeSQLText(webServer.requestDomain) _
                                & "," & db.encodeSQLText(webServer.webServerIO_requestPath) _
                                & "," & db.encodeSQLText(webServer.webServerIO_requestPage) _
                                & "," & db.encodeSQLText(Left(webServer.requestQueryString, 255)) _
                                & "," & db.encodeSQLText(Left(Form, 255)) _
                                & "," & db.encodeSQLText(Left(webServer.requestReferrer, 255)) _
                                & "," & db.encodeSQLDate(app_startTime) _
                                & "," & db.encodeSQLBoolean(authContext.visit_stateOK) _
                                & "," & db.encodeSQLNumber(metaData.getContentId("Viewings")) _
                                & "," & db.encodeSQLNumber(appStopWatch.ElapsedMilliseconds) _
                                & ",1" _
                                & "," & db.encodeSQLNumber(CSMax) _
                                & "," & db.encodeSQLNumber(PageID)
                            SQL &= "," & db.encodeSQLBoolean(webServer.webServerIO_PageExcludeFromAnalytics)
                            SQL &= "," & db.encodeSQLText(htmlDoc.main_MetaContent_Title)
                            SQL &= ");"
                            Call db.executeSql(SQL)
                            'Call db.executeSqlAsync(SQL)
                        End If
                    End If
                    '
                    ' ----- dispose objects created here
                    '
                    If Not (_addonCache Is Nothing) Then
                        ' no dispose
                        'Call _addonCache.Dispose()
                        _addonCache = Nothing
                    End If
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
                    If Not (_metaData Is Nothing) Then
                        Call _metaData.Dispose()
                        _metaData = Nothing
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
                    If Not (_doc Is Nothing) Then
                        ' no dispose
                        'Call _doc.Dispose()
                        _doc = Nothing
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
                    '
                    If Not (_metaData Is Nothing) Then
                        _metaData.Dispose()
                        _metaData = Nothing
                    End If
                End If
                '
                ' cleanup non-managed objects
                '
            End If
        End Sub
#End Region        '















        '
        '========================================================================
        '   Open a content set with the current whats new list
        '========================================================================
        '
        Public Function csOpenWatchList(ByVal ListName As String, ByVal SortFieldList As String, ByVal ActiveOnly As Boolean, ByVal PageSize As Integer, ByVal PageNumber As Integer) As Integer
            On Error GoTo ErrorTrap 'Const Tn = "OpenCSContentWatchList" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim SQL As String
            'Dim SortFieldList As String
            'Dim iActiveOnly As Boolean
            Dim MethodName As String
            'Dim ListName As String
            Dim CS As Integer
            '
            'SortFieldList = Trim(encodeMissingText(SortFieldList, ""))
            'SortFieldList = encodeMissingText(SortFieldList, "DateAdded")
            If SortFieldList = "" Then
                SortFieldList = "DateAdded"
            End If
            'iActiveOnly = encodeMissingText(ActiveOnly, True)
            'ListName = Trim(genericController.encodeText(ListName))
            '
            MethodName = "csOpenWatchList( " & ListName & ", " & SortFieldList & ", " & ActiveOnly & " )"
            '
            ' ----- Add tablename to the front of SortFieldList fieldnames
            '
            SortFieldList = " " & genericController.vbReplace(SortFieldList, ",", " , ") & " "
            SortFieldList = genericController.vbReplace(SortFieldList, " ID ", " ccContentWatch.ID ")
            SortFieldList = genericController.vbReplace(SortFieldList, " Link ", " ccContentWatch.Link ")
            SortFieldList = genericController.vbReplace(SortFieldList, " LinkLabel ", " ccContentWatch.LinkLabel ")
            SortFieldList = genericController.vbReplace(SortFieldList, " SortOrder ", " ccContentWatch.SortOrder ")
            SortFieldList = genericController.vbReplace(SortFieldList, " DateAdded ", " ccContentWatch.DateAdded ")
            SortFieldList = genericController.vbReplace(SortFieldList, " ContentID ", " ccContentWatch.ContentID ")
            SortFieldList = genericController.vbReplace(SortFieldList, " RecordID ", " ccContentWatch.RecordID ")
            SortFieldList = genericController.vbReplace(SortFieldList, " ModifiedDate ", " ccContentWatch.ModifiedDate ")
            '
            SQL = "SELECT ccContentWatch.ID AS ID, ccContentWatch.Link as Link, ccContentWatch.LinkLabel as LinkLabel, ccContentWatch.SortOrder as SortOrder, ccContentWatch.DateAdded as DateAdded, ccContentWatch.ContentID as ContentID, ccContentWatch.RecordID as RecordID, ccContentWatch.ModifiedDate as ModifiedDate" _
                & " FROM (ccContentWatchLists LEFT JOIN ccContentWatchListRules ON ccContentWatchLists.ID = ccContentWatchListRules.ContentWatchListID) LEFT JOIN ccContentWatch ON ccContentWatchListRules.ContentWatchID = ccContentWatch.ID" _
                & " WHERE (((ccContentWatchLists.Name)=" & db.encodeSQLText(ListName) & ")" _
                    & "AND ((ccContentWatchLists.Active)<>0)" _
                    & "AND ((ccContentWatchListRules.Active)<>0)" _
                    & "AND ((ccContentWatch.Active)<>0)" _
                    & "AND (ccContentWatch.Link is not null)" _
                    & "AND (ccContentWatch.LinkLabel is not null)" _
                    & "AND ((ccContentWatch.WhatsNewDateExpires is null)or(ccContentWatch.WhatsNewDateExpires>" & db.encodeSQLDate(Now) & "))" _
                    & ")" _
                & " ORDER BY " & SortFieldList & ";"
            csOpenWatchList = db.cs_openCsSql_rev("Default", SQL)
            If Not db.cs_ok(csOpenWatchList) Then
                '
                ' Check if listname exists
                '
                CS = db.cs_open("Content Watch Lists", "name=" & db.encodeSQLText(ListName), "ID", , , , , "ID")
                If Not db.cs_ok(CS) Then
                    Call db.cs_Close(CS)
                    CS = db.cs_insertRecord("Content Watch Lists", 0)
                    If db.cs_ok(CS) Then
                        Call db.cs_set(CS, "name", ListName)
                    End If
                End If
                Call db.cs_Close(CS)
            End If
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError4(Err.Number, Err.Source, Err.Description, "csOpenWatchList", True)
        End Function
        '
        '=================================================================================================================
        '   csv_GetAddonOption
        '
        '   returns the value matching a given name in an AddonOptionConstructor
        '
        '   AddonOptionConstructor is a crlf delimited name=value[selector]descriptor list
        '
        '   See cpCoreClass.ExecuteAddon for a full description of:
        '       AddonOptionString
        '       AddonOptionConstructor
        '       AddonOptionNameValueList
        '       AddonOptionExpandedConstructor
        '=================================================================================================================
        '
        Public Function getAddonOption(OptionName As String, OptionString As String) As String
            On Error GoTo ErrorTrap 'Const Tn = "GetAddonOption": 'Dim th as integer: th = profileLogMethodEnter(Tn)
            '
            Dim WorkingString As String
            Dim iDefaultValue As String
            Dim NameLength As Integer
            Dim ValueStart As Integer
            Dim ValueEnd As Integer
            Dim IsQuoted As Boolean
            Dim Delimiter As String
            Dim Options() As String
            Dim Ptr As Integer
            Dim Pos As Integer
            Dim TestName As String
            Dim TargetName As String
            '
            WorkingString = OptionString
            getAddonOption = ""
            If WorkingString <> "" Then
                TargetName = genericController.vbLCase(OptionName)
                'targetName = genericController.vbLCase(encodeNvaArgument(OptionName))
                Options = Split(OptionString, "&")
                'Options = Split(OptionString, vbCrLf)
                For Ptr = 0 To UBound(Options)
                    Pos = genericController.vbInstr(1, Options(Ptr), "=")
                    If Pos > 0 Then
                        TestName = genericController.vbLCase(Trim(Left(Options(Ptr), Pos - 1)))
                        Do While (TestName <> "") And (Left(TestName, 1) = vbTab)
                            TestName = Trim(Mid(TestName, 2))
                        Loop
                        Do While (TestName <> "") And (Right(TestName, 1) = vbTab)
                            TestName = Trim(Mid(TestName, 1, Len(TestName) - 1))
                        Loop
                        If TestName = TargetName Then
                            getAddonOption = genericController.decodeNvaArgument(Trim(Mid(Options(Ptr), Pos + 1)))
                            'csv_GetAddonOption = Trim(Mid(Options(Ptr), Pos + 1))
                            Exit For
                        End If
                    End If

                Next
            End If
            '
            Exit Function
ErrorTrap:
            Throw (New Exception("Unexpected exception"))
        End Function
        '
        '   Returns the next entry in the array, empty when there are no more
        '
        Public Function csv_GetEncodeContent_JavascriptOnLoad() As String
            On Error GoTo ErrorTrap 'Const Tn = "csv_GetEncodeContent_JavascriptOnLoad": 'Dim th as integer: th = profileLogMethodEnter(Tn)
            '
            Dim Ptr As Integer
            '
            If web_EncodeContent_JavascriptOnLoad_Cnt >= 0 Then
                For Ptr = 0 To web_EncodeContent_JavascriptOnLoad_Cnt - 1
                    If web_EncodeContent_JavascriptOnLoad(Ptr) <> "" Then
                        csv_GetEncodeContent_JavascriptOnLoad = web_EncodeContent_JavascriptOnLoad(Ptr)
                        web_EncodeContent_JavascriptOnLoad(Ptr) = ""
                        Exit For
                    End If
                Next
            End If
            '
            Exit Function
ErrorTrap:
            Throw (New Exception("Unexpected exception"))
        End Function
        '
        '   Returns the next entry in the array, empty when there are no more
        '
        Public Function csv_GetEncodeContent_JavascriptBodyEnd() As String
            On Error GoTo ErrorTrap 'Const Tn = "csv_GetEncodeContent_JavascriptBodyEnd": 'Dim th as integer: th = profileLogMethodEnter(Tn)
            '
            Dim Ptr As Integer
            '
            If web_EncodeContent_JavascriptBodyEnd_cnt >= 0 Then
                For Ptr = 0 To web_EncodeContent_JavascriptBodyEnd_cnt - 1
                    If web_EncodeContent_JavascriptBodyEnd(Ptr) <> "" Then
                        csv_GetEncodeContent_JavascriptBodyEnd = web_EncodeContent_JavascriptBodyEnd(Ptr)
                        web_EncodeContent_JavascriptBodyEnd(Ptr) = ""
                        Exit For
                    End If
                Next
            End If
            '
            Exit Function
ErrorTrap:
            Throw (New Exception("Unexpected exception"))
        End Function
        '
        '   Returns the next entry in the array, empty when there are no more
        '
        Public Function csv_GetEncodeContent_JSFilename() As String
            On Error GoTo ErrorTrap 'Const Tn = "csv_GetEncodeContent_JSFilename": 'Dim th as integer: th = profileLogMethodEnter(Tn)
            '
            Dim Ptr As Integer
            '
            If web_EncodeContent_JSFilename_Cnt >= 0 Then
                For Ptr = 0 To web_EncodeContent_JSFilename_Cnt - 1
                    If web_EncodeContent_JSFilename(Ptr) <> "" Then
                        csv_GetEncodeContent_JSFilename = web_EncodeContent_JSFilename(Ptr)
                        web_EncodeContent_JSFilename(Ptr) = ""
                        Exit For
                    End If
                Next
            End If
            '
            Exit Function
ErrorTrap:
            Throw (New Exception("Unexpected exception"))
        End Function
        '
        '   Returns the next entry in the array, empty when there are no more
        '
        Public Function csv_GetEncodeContent_StyleFilenames() As String
            On Error GoTo ErrorTrap 'Const Tn = "csv_GetEncodeContent_StyleFilenames": 'Dim th as integer: th = profileLogMethodEnter(Tn)
            '
            Dim Ptr As Integer
            '
            If web_EncodeContent_StyleFilenames_Cnt >= 0 Then
                For Ptr = 0 To web_EncodeContent_StyleFilenames_Cnt - 1
                    If web_EncodeContent_StyleFilenames(Ptr) <> "" Then
                        csv_GetEncodeContent_StyleFilenames = web_EncodeContent_StyleFilenames(Ptr)
                        web_EncodeContent_StyleFilenames(Ptr) = ""
                        Exit For
                    End If
                Next
            End If
            '
            Exit Function
ErrorTrap:
            Throw (New Exception("Unexpected exception"))
        End Function
        '
        '========================================================================================================
        ' Returns any head tags picked up during csv_EncodeContent that must be delivered in teh page
        '========================================================================================================
        '
        Public Function web_GetEncodeContent_HeadTags() As String
            '
            web_GetEncodeContent_HeadTags = web_EncodeContent_HeadTags
            web_EncodeContent_HeadTags = ""
            '
        End Function


        '
        '=================================================================================================================
        '   csv_GetAddonOptionStringValue
        '
        '   gets the value from a list matching the name
        '
        '   InstanceOptionstring is an "AddonEncoded" name=AddonEncodedValue[selector]descriptor&name=value string
        '=================================================================================================================
        '
        Public Function csv_GetAddonOptionStringValue(OptionName As String, addonOptionString As String) As String
            On Error GoTo ErrorTrap
            '
            Dim Pos As Integer
            Dim s As String
            '
            s = genericController.getSimpleNameValue(OptionName, addonOptionString, "", "&")
            Pos = genericController.vbInstr(1, s, "[")
            If Pos > 0 Then
                s = Left(s, Pos - 1)
            End If
            s = genericController.decodeNvaArgument(s)
            '
            csv_GetAddonOptionStringValue = Trim(s)
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError3("", "", "dll", "ccCommonModule", "csv_GetAddonOptionStringValue", Err.Number, Err.Source, Err.Description, True, False, "")
        End Function
        '
        '================================================================================================
        '   see csv_reportWarning2
        '================================================================================================
        '
        Public Function csv_reportWarning(Name As String, Description As String, generalKey As String, specificKey As String) As String
            Call csv_reportWarning2(Name, Left(Description, 250), "", 0, Description, generalKey, specificKey)
        End Function
        '
        '================================================================================================
        '   Report Warning
        '       A warning is logged in the site warnings log
        '           name - a generic description of the warning
        '               "bad link found on page"
        '           short description - a <255 character cause
        '               "bad link http://thisisabadlink.com"
        '           location - the URL, service or process that caused the problem
        '               "http://goodpageThankHasBadLink.com"
        '           pageid - the record id of the bad page.
        '               "http://goodpageThankHasBadLink.com"
        '           description - a specific description
        '               "link to http://www.this.com/pagename was found on http://www.this.com/About-us"
        '           generalKey - a generic string that describes the warning. the warning report
        '               will display one line for each generalKey (name matches guid)
        '               like "bad link"
        '           specificKey - a string created by the addon logging so it does not continue to log exactly the
        '               same warning over and over. If there are 100 different link not found warnings,
        '               there should be 100 entires with the same guid and name, but 100 different keys. If the
        '               an identical key is found the count increments.
        '               specifickey is like "link to http://www.this.com/pagename was found on http://www.this.com/About-us"
        '           count - the number of times the key was attempted to add. "This error was reported 100 times"
        '================================================================================================
        '
        Public Function csv_reportWarning2(Name As String, shortDescription As String, location As String, PageID As Integer, Description As String, generalKey As String, specificKey As String) As String
            '
            Dim SQL As String
            'dim dt as datatable
            Dim warningId As Integer
            Dim CS As Integer
            '
            warningId = 0
            SQL = "select top 1 ID from ccSiteWarnings" _
                & " where (generalKey=" & db.encodeSQLText(generalKey) & ")" _
                & " and(specificKey=" & db.encodeSQLText(specificKey) & ")" _
                & ""
            Dim dt As DataTable
            dt = db.executeSql(SQL)
            If dt.Rows.Count > 0 Then
                warningId = genericController.EncodeInteger(dt.Rows(0).Item("id"))
            End If
            '
            If warningId <> 0 Then
                '
                ' increment count for matching warning
                '
                SQL = "update ccsitewarnings set count=count+1,DateLastReported=" & db.encodeSQLDate(Now()) & " where id=" & warningId
                Call db.executeSql(SQL)
            Else
                '
                ' insert new record
                '
                CS = db.cs_insertRecord("Site Warnings", 0)
                If db.cs_ok(CS) Then
                    Call db.cs_set(CS, "name", Name)
                    Call db.cs_set(CS, "description", Description)
                    Call db.cs_set(CS, "generalKey", generalKey)
                    Call db.cs_set(CS, "specificKey", specificKey)
                    Call db.cs_set(CS, "count", 1)
                    Call db.cs_set(CS, "DateLastReported", Now())
                    If True Then
                        Call db.cs_set(CS, "shortDescription", shortDescription)
                        Call db.cs_set(CS, "location", location)
                        Call db.cs_set(CS, "pageId", PageID)
                    End If
                End If
                Call db.cs_Close(CS)
            End If
            '
        End Function
        '
        '=================================================================================================================================================
        '   csv_addLinkAlias
        '
        '   Link Alias
        '       A LinkAlias name is a unique string that identifies a page on the site.
        '       A page on the site is generated from the PageID, and the QueryStringSuffix
        '       PageID - obviously, this is the ID of the page
        '       QueryStringSuffix - other things needed on the Query to display the correct content.
        '           The Suffix is needed in cases like when an Add-on is embedded in a page. The URL to that content becomes the pages
        '           Link, plus the suffix needed to find the content.
        '
        '       When you make the menus, look up the most recent Link Alias entry with the pageID, and a blank QueryStringSuffix
        '
        '   The Link Alias table no longer needs the Link field.
        '
        '=================================================================================================================================================
        '
        ' +++++ 9/8/2011 4.1.482, added csv_addLinkAlias to csv and changed main to call
        '
        Public Sub app_addLinkAlias(linkAlias As String, PageID As Integer, QueryStringSuffix As String)
            Dim return_ignoreError As String = ""
            Call app_addLinkAlias2(linkAlias, PageID, QueryStringSuffix, True, False, return_ignoreError)
        End Sub
        '
        ' +++++ 9/8/2011 4.1.482, added csv_addLinkAlias to csv and changed main to call
        '
        Public Sub app_addLinkAlias2(linkAlias As String, PageID As Integer, QueryStringSuffix As String, Optional OverRideDuplicate As Boolean = False, Optional DupCausesWarning As Boolean = False, Optional ByRef return_WarningMessage As String = "")
            On Error GoTo ErrorTrap
            '
            Const SafeString = "0123456789abcdefghijklmnopqrstuvwxyz-_/."
            '
            Dim Ptr As Integer
            Dim TestChr As String
            Dim Src As String
            Dim FieldList As String
            Dim LinkAliasPageID As Integer
            Dim PageContentCID As Integer
            Dim WorkingLinkAlias As String
            Dim CS As Integer
            Dim LoopCnt As Integer
            'Dim fs As New fileSystemClass
            Dim FolderCheck As String
            Dim SQL As String
            Dim AllowLinkAlias As Boolean
            'dim buildversion As String
            '
            If (True) Then
                AllowLinkAlias = siteProperties.getBoolean("allowLinkAlias", False)
                WorkingLinkAlias = linkAlias
                If (WorkingLinkAlias <> "") Then
                    '
                    ' remove nonsafe URL characters
                    '
                    Src = WorkingLinkAlias
                    Src = genericController.vbReplace(Src, "’", "'")
                    Src = genericController.vbReplace(Src, vbTab, " ")
                    WorkingLinkAlias = ""
                    For Ptr = 1 To Len(Src) + 1
                        TestChr = Mid(Src, Ptr, 1)
                        If genericController.vbInstr(1, SafeString, TestChr, vbTextCompare) <> 0 Then
                        Else
                            TestChr = vbTab
                        End If
                        WorkingLinkAlias = WorkingLinkAlias & TestChr
                    Next
                    Ptr = 0
                    Do While genericController.vbInstr(1, WorkingLinkAlias, vbTab & vbTab) <> 0 And (Ptr < 100)
                        WorkingLinkAlias = genericController.vbReplace(WorkingLinkAlias, vbTab & vbTab, vbTab)
                        Ptr = Ptr + 1
                    Loop
                    If Right(WorkingLinkAlias, 1) = vbTab Then
                        WorkingLinkAlias = Mid(WorkingLinkAlias, 1, Len(WorkingLinkAlias) - 1)
                    End If
                    If Left(WorkingLinkAlias, 1) = vbTab Then
                        WorkingLinkAlias = Mid(WorkingLinkAlias, 2)
                    End If
                    WorkingLinkAlias = genericController.vbReplace(WorkingLinkAlias, vbTab, "-")
                    If (WorkingLinkAlias <> "") Then
                        '
                        ' Make sure there is not a folder or page in the wwwroot that matches this Alias
                        '
                        If Left(WorkingLinkAlias, 1) <> "/" Then
                            WorkingLinkAlias = "/" & WorkingLinkAlias
                        End If
                        '
                        If genericController.vbLCase(WorkingLinkAlias) = genericController.vbLCase("/" & serverConfig.appConfig.name) Then
                            '
                            ' This alias points to the cclib folder
                            '
                            If AllowLinkAlias Then
                                return_WarningMessage = "" _
                                    & "The Link Alias being created (" & WorkingLinkAlias & ") can not be used because there is a virtual directory in your website directory that already uses this name." _
                                    & " Please change it to ensure the Link Alias is unique. To set or change the Link Alias, use the Link Alias tab and select a name not used by another page."
                            End If
                        ElseIf genericController.vbLCase(WorkingLinkAlias) = "/cclib" Then
                            '
                            ' This alias points to the cclib folder
                            '
                            If AllowLinkAlias Then
                                return_WarningMessage = "" _
                                    & "The Link Alias being created (" & WorkingLinkAlias & ") can not be used because there is a virtual directory in your website directory that already uses this name." _
                                    & " Please change it to ensure the Link Alias is unique. To set or change the Link Alias, use the Link Alias tab and select a name not used by another page."
                            End If
                        ElseIf appRootFiles.pathExists(serverConfig.appConfig.appRootFilesPath & "\" & Mid(WorkingLinkAlias, 2)) Then
                            'ElseIf appRootFiles.pathExists(serverConfig.clusterPath & serverconfig.appConfig.appRootFilesPath & "\" & Mid(WorkingLinkAlias, 2)) Then
                            '
                            ' This alias points to a different link, call it an error
                            '
                            If AllowLinkAlias Then
                                return_WarningMessage = "" _
                                    & "The Link Alias being created (" & WorkingLinkAlias & ") can not be used because there is a folder in your website directory that already uses this name." _
                                    & " Please change it to ensure the Link Alias is unique. To set or change the Link Alias, use the Link Alias tab and select a name not used by another page."
                            End If
                        Else
                            '
                            ' Make sure there is one here for this
                            '
                            If True Then
                                FieldList = "Name,PageID,QueryStringSuffix"
                            Else
                                '
                                ' must be > 33914 to run this routine
                                '
                                FieldList = "Name,PageID,'' as QueryStringSuffix"
                            End If
                            CS = db.cs_open("Link Aliases", "name=" & db.encodeSQLText(WorkingLinkAlias), , , , , , FieldList)
                            If Not db.cs_ok(CS) Then
                                '
                                ' Alias not found, create a Link Aliases
                                '
                                Call db.cs_Close(CS)
                                CS = db.cs_insertRecord("Link Aliases", 0)
                                If db.cs_ok(CS) Then
                                    Call db.cs_set(CS, "Name", WorkingLinkAlias)
                                    'Call app.csv_SetCS(CS, "Link", Link)
                                    Call db.cs_set(CS, "Pageid", PageID)
                                    If True Then
                                        Call db.cs_set(CS, "QueryStringSuffix", QueryStringSuffix)
                                    End If
                                End If
                            Else
                                '
                                ' Alias found, verify the pageid & QueryStringSuffix
                                '
                                Dim CurrentLinkAliasID As Integer
                                Dim resaveLinkAlias As Boolean
                                Dim CS2 As Integer
                                LinkAliasPageID = db.cs_getInteger(CS, "pageID")
                                If (db.cs_getText(CS, "QueryStringSuffix").ToLower = QueryStringSuffix.ToLower) And (PageID = LinkAliasPageID) Then
                                    '
                                    ' it maches a current entry for this link alias, if the current entry is not the highest number id,
                                    '   remove it and add this one
                                    '
                                    CurrentLinkAliasID = db.cs_getInteger(CS, "id")
                                    CS2 = db.cs_openCsSql_rev("default", "select top 1 id from ccLinkAliases where pageid=" & LinkAliasPageID & " order by id desc")
                                    If db.cs_ok(CS2) Then
                                        resaveLinkAlias = (CurrentLinkAliasID <> db.cs_getInteger(CS2, "id"))
                                    End If
                                    Call db.cs_Close(CS2)
                                    If resaveLinkAlias Then
                                        Call db.executeSql("delete from ccLinkAliases where id=" & CurrentLinkAliasID)
                                        Call db.cs_Close(CS)
                                        CS = db.cs_insertRecord("Link Aliases", 0)
                                        If db.cs_ok(CS) Then
                                            Call db.cs_set(CS, "Name", WorkingLinkAlias)
                                            Call db.cs_set(CS, "Pageid", PageID)
                                            If True Then
                                                Call db.cs_set(CS, "QueryStringSuffix", QueryStringSuffix)
                                            End If
                                        End If
                                    End If
                                Else
                                    '
                                    ' Does not match, this is either a change, or a duplicate that needs to be blocked
                                    '
                                    If OverRideDuplicate Then
                                        '
                                        ' change the Link Alias to the new link
                                        '
                                        'Call app.csv_SetCS(CS, "Link", Link)
                                        Call db.cs_set(CS, "Pageid", PageID)
                                        If True Then
                                            Call db.cs_set(CS, "QueryStringSuffix", QueryStringSuffix)
                                        End If
                                    ElseIf AllowLinkAlias Then
                                        '
                                        ' This alias points to a different link, and link aliasing is in use, call it an error (but save record anyway)
                                        '
                                        If DupCausesWarning Then
                                            If LinkAliasPageID = 0 Then '
                                                PageContentCID = metaData.getContentId("Page Content")
                                                return_WarningMessage = "" _
                                                    & "This page has been saved, but the Link Alias could not be created (" & WorkingLinkAlias & ") because it is already in use for another page." _
                                                    & " To use Link Aliasing (friendly page names) for this page, the Link Alias value must be unique on this site. To set or change the Link Alias, clicke the Link Alias tab and select a name not used by another page or a folder in your website."
                                            Else
                                                PageContentCID = metaData.getContentId("Page Content")
                                                return_WarningMessage = "" _
                                                    & "This page has been saved, but the Link Alias could not be created (" & WorkingLinkAlias & ") because it is already in use for another page (<a href=""?af=4&cid=" & PageContentCID & "&id=" & LinkAliasPageID & """>edit</a>)." _
                                                    & " To use Link Aliasing (friendly page names) for this page, the Link Alias value must be unique. To set or change the Link Alias, click the Link Alias tab and select a name not used by another page or a folder in your website."
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                            Call db.cs_Close(CS)
                            'Call cache_linkAlias_clear()
                        End If
                    End If
                End If
            End If
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError4(Err.Number, Err.Source, Err.Description, "csv_addLinkAlias", True)
        End Sub
        '
        '
        '
        Public Function csv_GetLinkedText(ByVal AnchorTag As String, ByVal AnchorText As String) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetLinkedText")
            '
            Dim UcaseAnchorText As String
            Dim LinkPosition As Integer
            Dim MethodName As String
            Dim iAnchorTag As String
            Dim iAnchorText As String
            '
            MethodName = "csv_GetLinkedText"
            '
            csv_GetLinkedText = ""
            iAnchorTag = genericController.encodeText(AnchorTag)
            iAnchorText = genericController.encodeText(AnchorText)
            UcaseAnchorText = genericController.vbUCase(iAnchorText)
            If (iAnchorTag <> "") And (iAnchorText <> "") Then
                LinkPosition = InStrRev(UcaseAnchorText, "<LINK>", -1)
                If LinkPosition = 0 Then
                    csv_GetLinkedText = iAnchorTag & iAnchorText & "</a>"
                Else
                    csv_GetLinkedText = iAnchorText
                    LinkPosition = InStrRev(UcaseAnchorText, "</LINK>", -1)
                    Do While LinkPosition > 1
                        csv_GetLinkedText = Mid(csv_GetLinkedText, 1, LinkPosition - 1) & "</a>" & Mid(csv_GetLinkedText, LinkPosition + 7)
                        LinkPosition = InStrRev(UcaseAnchorText, "<LINK>", LinkPosition - 1)
                        If LinkPosition <> 0 Then
                            csv_GetLinkedText = Mid(csv_GetLinkedText, 1, LinkPosition - 1) & iAnchorTag & Mid(csv_GetLinkedText, LinkPosition + 6)
                        End If
                        LinkPosition = InStrRev(UcaseAnchorText, "</LINK>", LinkPosition)
                    Loop
                End If
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError7(MethodName, "Unexpected Trap")
            '
        End Function
        '
        '========================================================================
        '   convert a virtual file into a Link usable on the website:
        '       convert all \ to /
        '       if it includes "://", leave it along
        '       if it starts with "/", it is already root relative, leave it alone
        '       else (if it start with a file or a path), add the serverFilePath
        '========================================================================
        '
        Public Function getCdnFileLink(ByVal virtualFile As String) As String
            Dim returnLink As String
            '
            returnLink = virtualFile
            returnLink = genericController.vbReplace(returnLink, "\", "/")
            If genericController.vbInstr(1, returnLink, "://") <> 0 Then
                '
                ' icon is an Absolute URL - leave it
                '
            ElseIf Left(returnLink, 1) = "/" Then
                '
                ' icon is Root Relative, leave it
                '
            Else
                '
                ' icon is a virtual file, add the serverfilepath
                '
                returnLink = serverConfig.appConfig.cdnFilesNetprefix & returnLink
            End If
            getCdnFileLink = returnLink
        End Function

        '
        '========================================================================
        '   42private
        '
        ' ----- Process Member Actions (called only from Init)
        '========================================================================
        '
        Private Sub main_ProcessFormMyProfile()
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("ProcessFormMyProfile")
            '
            'If Not (true) Then Exit Sub
            '
            Dim CSMember As Integer
            Dim CS As Integer
            Dim TopicCount As Integer
            Dim TopicPointer As Integer
            Dim TopicID As Integer
            Dim TopicRulesCID As Integer
            Dim Panel As String
            Dim MethodName As String
            Dim CreatePathBlock As Boolean
            Dim AllowChange As Boolean
            Dim PathID As Integer
            Dim Filename As String
            Dim Button As String
            Dim GroupCount As Integer
            Dim GroupPointer As Integer
            Dim CSPointer As Integer
            Dim CDef As cdefModel
            Dim ContentName As String
            ' converted array to dictionary - Dim FieldPointer As Integer
            Dim FieldName As String
            Dim FieldValue As String
            Dim CSTest As Integer
            Dim PeopleCID As Integer
            Dim ErrorMessage As String = ""
            Dim ErrorCode As Integer
            Dim FirstName As String
            Dim LastName As String
            Dim Newusername As String
            Dim NewPassword As String
            '
            MethodName = "main_ProcessFormMyProfile"
            '
            ' ----- Check if new username is allowed
            '
            Button = docProperties.getText("Button")
            If (Button = ButtonSave) Then
                AllowChange = True
                PeopleCID = metaData.getContentId("People")
                Newusername = docProperties.getText("username")
                NewPassword = docProperties.getText("password")
                If Newusername = "" Then
                    '
                    ' Sest to blank
                    '
                    AllowChange = True
                ElseIf genericController.vbUCase(Newusername) <> genericController.vbUCase(authContext.user.Username) Then
                    '
                    ' ----- username changed, check if change is allowed
                    '
                    If Not authContext.isNewLoginOK(Me, Newusername, NewPassword, ErrorMessage, ErrorCode) Then
                        errorController.error_AddUserError(Me, ErrorMessage)
                        AllowChange = False
                    End If
                End If
                If AllowChange Then
                    CSMember = db.cs_open("people", "id=" & db.encodeSQLNumber(authContext.user.ID))
                    If Not db.cs_ok(CSMember) Then
                        Call errorController.error_AddUserError(Me, "There was a problem locating your account record. No changes were saved.")
                        ' if user error, it goes back to the hardcodedpage
                        'LegacyInterceptPageSN = LegacyInterceptPageSNMyProfile
                    Else
                        '
                        ' Check for unique violations first
                        '
                        ContentName = metaData.getContentNameByID(db.cs_getInteger(CSMember, "ContentControlID"))
                        If ContentName = "" Then
                            Call errorController.error_AddUserError(Me, "There was a problem locating the information you requested.")
                        Else
                            CDef = metaData.getCdef(ContentName)
                            For Each keyValuePair As KeyValuePair(Of String, CDefFieldModel) In CDef.fields
                                Dim field As CDefFieldModel = keyValuePair.Value
                                If field.UniqueName Then
                                    FieldName = field.nameLc
                                    FieldValue = docProperties.getText(FieldName)
                                    If FieldValue <> "" Then
                                        CSTest = db.cs_open(ContentName, "(" & FieldName & "=" & db.encodeSQLText(FieldValue) & ")and(ID<>" & authContext.user.ID & ")")
                                        If db.cs_ok(CSTest) Then
                                            Call errorController.error_AddUserError(Me, "The field '" & FieldName & "' must be unique, and another account has already used '" & FieldValue & "'")
                                        End If
                                        Call db.cs_Close(CSTest)
                                    End If
                                End If
                            Next
                        End If
                        If (debug_iUserError <> "") Then
                            ' goes to hardcodedpage on user error
                            'LegacyInterceptPageSN = LegacyInterceptPageSNMyProfile
                        Else
                            '
                            ' Personal Info
                            '
                            FirstName = docProperties.getText("FirstName")
                            LastName = docProperties.getText("LastName")
                            '
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "firstname")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "LastName")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "Name")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "email")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "company")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "title")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "address")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "city")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "state")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "zip")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "country")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "phone")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "fax")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "ResumeFilename")
                            '
                            ' Billing Info
                            '
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "BillName")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "Billemail")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "Billcompany")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "Billaddress")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "Billcity")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "Billstate")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "Billzip")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "Billcountry")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "Billphone")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "Billfax")
                            '
                            ' Shiping Info
                            '
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "ShipName")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "Shipcompany")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "Shipaddress")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "Shipcity")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "Shipstate")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "Shipzip")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "Shipcountry")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "Shipphone")
                            '
                            ' Site preferences
                            '
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "username")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "password")
                            Call main_ProcessFormMyProfile_UpdateFieldBoolean(CSMember, "AllowBulkEmail")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "LanguageID")

                            If siteProperties.getBoolean("AllowAutoLogin", False) Then
                                Call main_ProcessFormMyProfile_UpdateFieldBoolean(CSMember, "AutoLogin")
                            End If
                            If authContext.isAuthenticatedContentManager(Me) Then
                                Call main_ProcessFormMyProfile_UpdateFieldBoolean(CSMember, "AllowToolsPanel")
                            End If
                            '
                            ' --- update Topic records
                            '
                            Call htmlDoc.main_ProcessCheckList(rnMyProfileTopics, "people", "memberid", "topics", "member topic rules", "memberid", "topicid")
                            '
                            ' --- Update Group Records
                            '
                            Call htmlDoc.main_ProcessCheckList("MemberRules", "Members", genericController.encodeText(authContext.user.ID), "Groups", "Member Rules", "MemberID", "GroupID")
                            '
                            '
                            '
                            If app_errorCount > 0 Then
                                Call errorController.error_AddUserError(Me, "An error occurred which prevented your information from being saved.")
                                'LegacyInterceptPageSN = LegacyInterceptPageSNMyProfile
                            Else
                                If app_errorCount > 0 Then
                                    Call errorController.error_AddUserError(Me, "An error occurred while saving your information.")
                                    'LegacyInterceptPageSN = LegacyInterceptPageSNMyProfile
                                End If
                            End If
                        End If
                        Call cache.invalidateContent("People")
                    End If
                    Call db.cs_Close(CSMember)
                End If
            End If
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError13(MethodName)
        End Sub
        '
        '========================================================================
        '   42private
        '========================================================================
        '
        Private Sub main_ProcessFormMyProfile_UpdateField(ByVal CSMember As Integer, ByVal FieldName As String)
            On Error GoTo ErrorTrap
            '
            Dim FieldValue As String
            '
            FieldValue = docProperties.getText(FieldName)
            If db.cs_getText(CSMember, FieldName) <> FieldValue Then
                Call logController.logActivity2(Me, "profile changed " & FieldName, authContext.user.ID, authContext.user.OrganizationID)
                Call db.cs_set(CSMember, FieldName, FieldValue)
            End If
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError13("main_ProcessFormMyProfile_UpdateField")
        End Sub
        '
        '========================================================================
        '   42private
        '========================================================================
        '
        Private Sub main_ProcessFormMyProfile_UpdateFieldBoolean(ByVal CSMember As Integer, ByVal FieldName As String)
            On Error GoTo ErrorTrap
            '
            Dim FieldValue As Boolean
            '
            FieldValue = docProperties.getBoolean(FieldName)
            If db.cs_getBoolean(CSMember, FieldName) <> FieldValue Then
                Call logController.logActivity2(Me, "profile changed " & FieldName, authContext.user.ID, authContext.user.OrganizationID)
                Call db.cs_set(CSMember, FieldName, FieldValue)
            End If
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError13("main_ProcessFormMyProfile_UpdateFieldBoolean")
        End Sub

        '
        '===========================================================================================
        '   ----- Redirect without reason - compatibility only
        '===========================================================================================
        '
        Public Sub main_Redirect(ByVal Link As Object)
            Call webServer.redirect(genericController.encodeText(Link), "No explaination provided", False)
        End Sub
        '
        '========================================================================
        ' Stop sending to the HTMLStream
        '========================================================================
        '
        Public Sub doc_close()
            '
            ' 2011/3/11 - just stop future Contensive output, do not end the parent's response object, developer may want to add more
            '
            docOpen = False
        End Sub
        '
        '=============================================================================
        ' Cleans a text file of control characters, allowing only vblf
        '=============================================================================
        '
        Public Function main_RemoveControlCharacters(ByVal DirtyText As Object) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("RemoveControlCharacters")
            '
            Dim Pointer As Integer
            Dim ChrTest As Integer
            Dim MethodName As String
            Dim iDirtyText As String
            '
            MethodName = "main_RemoveControlCharacters"
            '
            iDirtyText = genericController.encodeText(DirtyText)
            main_RemoveControlCharacters = ""
            If (iDirtyText <> "") Then
                main_RemoveControlCharacters = ""
                For Pointer = 1 To Len(iDirtyText)
                    ChrTest = Asc(Mid(iDirtyText, Pointer, 1))
                    If ChrTest >= 32 And ChrTest < 128 Then
                        main_RemoveControlCharacters = main_RemoveControlCharacters & Chr(ChrTest)
                    Else
                        Select Case ChrTest
                            Case 9
                                main_RemoveControlCharacters = main_RemoveControlCharacters & " "
                            Case 10
                                main_RemoveControlCharacters = main_RemoveControlCharacters & vbLf
                        End Select
                    End If
                Next
                '
                ' limit CRLF to 2
                '
                Do While genericController.vbInstr(main_RemoveControlCharacters, vbLf & vbLf & vbLf) <> 0
                    main_RemoveControlCharacters = genericController.vbReplace(main_RemoveControlCharacters, vbLf & vbLf & vbLf, vbLf & vbLf)
                Loop
                '
                ' limit spaces to 1
                '
                Do While genericController.vbInstr(main_RemoveControlCharacters, "  ") <> 0
                    main_RemoveControlCharacters = genericController.vbReplace(main_RemoveControlCharacters, "  ", " ")
                Loop
            End If
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError18(MethodName)
        End Function
        '
        '========================================================================
        '   Test Point
        '       If main_PageTestPointPrinting print a string, value paior
        '========================================================================
        '
        Public Sub debug_testPoint(Message As String)
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("TestPoint")
            '
            Dim ElapsedTime As Single
            Dim iMessage As String
            '
            '
            ' ----- If not main_PageTestPointPrinting, exit right away
            '
            If testPointPrinting Then
                '
                ' write to stream
                '
                ElapsedTime = CSng(GetTickCount - app_startTickCount) / 1000
                iMessage = genericController.encodeText(Message)
                iMessage = Format((ElapsedTime), "00.000") & " - " & iMessage
                main_testPointMessage = main_testPointMessage & "<nobr>" & iMessage & "</nobr><br >"
                'writeAltBuffer ("<nobr>" & iMessage & "</nobr><br >")
            End If
            If siteProperties.allowTestPointLogging Then
                '
                ' write to debug log in virtual files - to read from a test verbose viewer
                '
                iMessage = genericController.encodeText(Message)
                iMessage = genericController.vbReplace(iMessage, vbCrLf, " ")
                iMessage = genericController.vbReplace(iMessage, vbCr, " ")
                iMessage = genericController.vbReplace(iMessage, vbLf, " ")
                iMessage = FormatDateTime(Now, vbShortTime) & vbTab & Format((ElapsedTime), "00.000") & vbTab & authContext.visit.ID & vbTab & iMessage
                '
                logController.appendLog(Me, iMessage, "", "testPoints_" & serverConfig.appConfig.name)
            End If
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError18("main_TestPoint")
        End Sub
        ''
        ''========================================================================
        '' main_RedirectByRecord( iContentName, iRecordID )
        ''   looks up the record
        ''   increments the 'clicks' field and redirects to the 'link' field
        ''   if the record is not found or there is no link, it just returns
        ''   Note: also supports iContentName for pre-2.1 sites
        ''========================================================================
        ''
        'Public Sub main_RedirectByRecord(ByVal ContentName As String, ByVal RecordID As Integer, Optional ByVal FieldName As String = "")
        '    Call main_RedirectByRecord_ReturnStatus(ContentName, RecordID, FieldName)
        'End Sub
        '
        '========================================================================
        ' main_RedirectByRecord( iContentName, iRecordID )
        '   looks up the record
        '   increments the 'clicks' field and redirects to the 'link' field
        '   returns true if the redirect happened OK
        '========================================================================
        '
        Public Function main_RedirectByRecord_ReturnStatus(ByVal ContentName As String, ByVal RecordID As Integer, Optional ByVal FieldName As String = "") As Boolean
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("RedirectByRecord_ReturnStatus")
            '
            'If Not (true) Then Exit Function
            '
            Dim Link As String
            Dim CSPointer As Integer
            Dim MethodName As String
            Dim ContentID As Integer
            Dim CSHost As Integer
            Dim HostContentName As String
            Dim HostRecordID As Integer
            Dim BlockRedirect As Boolean
            Dim iContentName As String
            Dim iRecordID As Integer
            Dim iFieldName As String
            Dim LinkPrefix As String
            Dim EncodedLink As String
            Dim NonEncodedLink As String = ""
            Dim RecordActive As Boolean
            '
            iContentName = genericController.encodeText(ContentName)
            iRecordID = genericController.EncodeInteger(RecordID)
            iFieldName = genericController.encodeEmptyText(FieldName, "link")
            '
            MethodName = "main_RedirectByRecord_ReturnStatus( " & iContentName & ", " & iRecordID & ", " & genericController.encodeEmptyText(FieldName, "(fieldname empty)") & ")"
            '
            main_RedirectByRecord_ReturnStatus = False
            BlockRedirect = False
            CSPointer = db.cs_open(iContentName, "ID=" & iRecordID)
            If db.cs_ok(CSPointer) Then
                ' 2/18/2008 - EncodeLink change
                '
                ' Assume all Link fields are already encoded -- as this is how they would appear if the admin cut and pasted
                '
                EncodedLink = Trim(db.cs_getText(CSPointer, iFieldName))
                If EncodedLink = "" Then
                    BlockRedirect = True
                Else
                    '
                    ' ----- handle content special cases (prevent redirect to deleted records)
                    '
                    NonEncodedLink = htmlDoc.main_DecodeUrl(EncodedLink)
                    Select Case genericController.vbUCase(iContentName)
                        Case "CONTENT WATCH"
                            '
                            ' ----- special case
                            '       if this is a content watch record, check the underlying content for
                            '       inactive or expired before redirecting
                            '
                            LinkPrefix = webServer.webServerIO_requestContentWatchPrefix
                            ContentID = (db.cs_getInteger(CSPointer, "ContentID"))
                            HostContentName = metaData.getContentNameByID(ContentID)
                            If (HostContentName = "") Then
                                '
                                ' ----- Content Watch with a bad ContentID, mark inactive
                                '
                                BlockRedirect = True
                                Call db.cs_set(CSPointer, "active", 0)
                            Else
                                HostRecordID = (db.cs_getInteger(CSPointer, "RecordID"))
                                If HostRecordID = 0 Then
                                    '
                                    ' ----- Content Watch with a bad iRecordID, mark inactive
                                    '
                                    BlockRedirect = True
                                    Call db.cs_set(CSPointer, "active", 0)
                                Else
                                    CSHost = db.cs_open(HostContentName, "ID=" & HostRecordID)
                                    If Not db.cs_ok(CSHost) Then
                                        '
                                        ' ----- Content Watch host record not found, mark inactive
                                        '
                                        BlockRedirect = True
                                        Call db.cs_set(CSPointer, "active", 0)
                                    End If
                                End If
                                Call db.cs_Close(CSHost)
                            End If
                            If BlockRedirect Then
                                '
                                ' ----- if a content watch record is blocked, delete the content tracking
                                '
                                Call db.deleteContentRules(metaData.getContentId(HostContentName), HostRecordID)
                            End If
                    End Select
                End If
                If Not BlockRedirect Then
                    '
                    ' If link incorrectly includes the LinkPrefix, take it off first, then add it back
                    '
                    NonEncodedLink = genericController.ConvertShortLinkToLink(NonEncodedLink, LinkPrefix)
                    If db.cs_isFieldSupported(CSPointer, "Clicks") Then
                        Call db.cs_set(CSPointer, "Clicks", (db.cs_getNumber(CSPointer, "Clicks")) + 1)
                    End If
                    Call webServer.redirect(LinkPrefix & NonEncodedLink, "Call to " & MethodName & ", no reason given.", False)
                    main_RedirectByRecord_ReturnStatus = True
                End If
            End If
            Call db.cs_Close(CSPointer)
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError18(MethodName)
        End Function
        '
        '========================================================================
        ' main_IncrementTableField( TableName, RecordID, Fieldname )
        '========================================================================
        '
        Public Sub main_IncrementTableField(ByVal TableName As String, ByVal RecordID As Integer, ByVal FieldName As String, Optional ByVal DataSourceName As String = "")
            Call db.executeSql("update " & TableName & " set " & FieldName & "=" & FieldName & "+1 where id=" & RecordID, DataSourceName)

            '            On Error GoTo ErrorTrap : ''Dim th as integer : th = profileLogMethodEnter("IncrementTableField")
            '            '
            '            'If Not (true) Then Exit Sub
            '            '
            '            Dim SQL As String
            '            'dim dt as datatable
            '            Dim iDataSourceName As String
            '            Dim RecordValue As Integer
            '            Dim iFieldName As String
            '            Dim iTableName As String
            '            Dim iRecordID As Integer
            '            '
            '            iDataSourceName = genericController.encodeText(main_encodeMissingText(DataSourceName, "Default"))
            '            iFieldName = genericController.encodeText(FieldName)
            '            iTableName = genericController.encodeText(TableName)
            '            iRecordID = genericController.EncodeInteger(RecordID)
            '            '
            '            SQL = "Select " & iFieldName & " FROM " & iTableName & " where ID=" & iRecordID & ";"
            '            RS = main_OpenRSSQL(iDataSourceName, SQL)
            '            If (isDataTableOk(rs)) Then
            '                If Not rs.rows.count=0 Then
            '                    RecordValue = genericController.EncodeInteger(RS(iFieldName)) + 1
            '                    SQL = "Update " & iTableName & " set " & iFieldName & "=" & RecordValue & " where ID=" & iRecordID & ";"
            '                    Call main_ExecuteSQL(iDataSourceName, SQL)
            '                End If
            '                If false Then
            '                    'RS.Close()
            '                End If
            '                'RS = Nothing
            '            End If

            '            Exit Sub
            '            '
            'ErrorTrap:
            '            Call main_HandleClassErrorAndResume_TrapPatch1("main_IncrementTableField")
        End Sub
        '
        '=============================================================================
        '   See main_GetNameValue_Internal
        '=============================================================================
        '
        Public Function main_GetNameValue(Tag As String, Name As String) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetNameValue")
            '
            'If Not (true) Then Exit Function
            main_GetNameValue = main_GetNameValue_Internal(genericController.encodeText(Tag), genericController.encodeText(Name))
            Exit Function
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError18("main_GetNameValue")
        End Function
        '
        '=============================================================================
        ' ----- Return the value associated with the name given
        '   NameValueString is a string of Name=Value pairs, separated by spaces or "&"
        '   If Name is not given, returns ""
        '   If Name present but no value, returns true (as if Name=true)
        '   If Name = Value, it returns value
        '=============================================================================
        '
        Public Function main_GetNameValue_Internal(NameValueString As String, Name As String) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetNameValue_Internal")
            '
            'If Not (true) Then Exit Function
            '
            Dim NameValueStringWorking As String
            Dim UcaseNameValueStringWorking As String
            Dim Position As Integer
            Dim PositionEqual As Integer
            Dim PositionEnd As Integer
            Dim MethodName As String
            Dim pairs() As String
            Dim PairCount As Integer
            Dim PairPointer As Integer
            Dim PairSplit() As String
            '
            MethodName = "main_GetNameValue_Internal"
            '
            If ((NameValueString <> "") And (Name <> "")) Then
                Do While genericController.vbInstr(1, NameValueStringWorking, " =") <> 0
                    NameValueStringWorking = genericController.vbReplace(NameValueStringWorking, " =", "=")
                Loop
                Do While genericController.vbInstr(1, NameValueStringWorking, "= ") <> 0
                    NameValueStringWorking = genericController.vbReplace(NameValueStringWorking, "= ", "=")
                Loop
                Do While genericController.vbInstr(1, NameValueStringWorking, "& ") <> 0
                    NameValueStringWorking = genericController.vbReplace(NameValueStringWorking, "& ", "&")
                Loop
                Do While genericController.vbInstr(1, NameValueStringWorking, " &") <> 0
                    NameValueStringWorking = genericController.vbReplace(NameValueStringWorking, " &", "&")
                Loop
                NameValueStringWorking = NameValueString & "&"
                UcaseNameValueStringWorking = genericController.vbUCase(NameValueStringWorking)
                '
                main_GetNameValue_Internal = ""
                If NameValueStringWorking <> "" Then
                    pairs = Split(NameValueStringWorking, "&")
                    PairCount = UBound(pairs) + 1
                    For PairPointer = 0 To PairCount - 1
                        PairSplit = Split(pairs(PairPointer), "=")
                        If genericController.vbUCase(PairSplit(0)) = genericController.vbUCase(Name) Then
                            If UBound(PairSplit) > 0 Then
                                main_GetNameValue_Internal = PairSplit(1)
                            End If
                            Exit For
                        End If
                    Next
                End If
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError13(MethodName)
        End Function
        '
        '========================================================================
        '   main_GetPanel( Panel, Optional StylePanel, Optional StyleHilite, Optional StyleShadow, Optional Width, Optional Padding, Optional HeightMin) As String
        ' Return a panel with the input as center
        '========================================================================
        '
        Public Function main_GetPanel(ByVal Panel As String, Optional ByVal StylePanel As String = "", Optional ByVal StyleHilite As String = "ccPanelHilite", Optional ByVal StyleShadow As String = "ccPanelShadow", Optional ByVal Width As String = "100%", Optional ByVal Padding As Integer = 5, Optional ByVal HeightMin As Integer = 1) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetPanel")
            '
            'If Not (true) Then Exit Function
            '
            '
            Dim ContentPanelWidth As String
            Dim MethodName As String
            Dim MyStylePanel As String
            Dim MyStyleHilite As String
            Dim MyStyleShadow As String
            Dim MyWidth As String
            Dim MyPadding As String
            Dim MyHeightMin As String
            Dim s As String
            Dim s0 As String
            Dim s1 As String
            Dim s2 As String
            Dim s3 As String
            Dim s4 As String
            Dim contentPanelWidthStyle As String
            '
            MethodName = "main_GetPanelTop"
            '
            MyStylePanel = genericController.encodeEmptyText(StylePanel, "ccPanel")
            MyStyleHilite = genericController.encodeEmptyText(StyleHilite, "ccPanelHilite")
            MyStyleShadow = genericController.encodeEmptyText(StyleShadow, "ccPanelShadow")
            MyWidth = genericController.encodeEmptyText(Width, "100%")
            MyPadding = Padding.ToString
            MyHeightMin = HeightMin.ToString
            '
            If genericController.vbIsNumeric(MyWidth) Then
                ContentPanelWidth = (CInt(MyWidth) - 2).ToString
                contentPanelWidthStyle = ContentPanelWidth & "px"
            Else
                ContentPanelWidth = "100%"
                contentPanelWidthStyle = ContentPanelWidth
            End If
            '
            '
            '
            s0 = "" _
                & cr & "<td style=""padding:" & MyPadding & "px;vertical-align:top"" class=""" & MyStylePanel & """>" _
                & genericController.kmaIndent(genericController.encodeText(Panel)) _
                & cr & "</td>" _
                & ""
            '
            s1 = "" _
                & cr & "<tr>" _
                & genericController.kmaIndent(s0) _
                & cr & "</tr>" _
                & ""
            s2 = "" _
                & cr & "<table style=""width:" & contentPanelWidthStyle & ";border:0px;"" class=""" & MyStylePanel & """ cellspacing=""0"">" _
                & genericController.kmaIndent(s1) _
                & cr & "</table>" _
                & ""
            s3 = "" _
                & cr & "<td width=""1"" height=""" & MyHeightMin & """ class=""" & MyStyleHilite & """><img alt=""space"" src=""/ccLib/images/spacer.gif"" height=""" & MyHeightMin & """ width=""1"" ></td>" _
                & cr & "<td width=""" & ContentPanelWidth & """ valign=""top"" align=""left"" class=""" & MyStylePanel & """>" _
                & genericController.kmaIndent(s2) _
                & cr & "</td>" _
                & cr & "<td width=""1"" class=""" & MyStyleShadow & """><img alt=""space"" src=""/ccLib/images/spacer.gif"" height=""1"" width=""1"" ></td>" _
                & ""
            s4 = "" _
                & cr & "<tr>" _
                & cr2 & "<td colspan=""3"" class=""" & MyStyleHilite & """><img alt=""space"" src=""/ccLib/images/spacer.gif"" height=""1"" width=""" & MyWidth & """ ></td>" _
                & cr & "</tr>" _
                & cr & "<tr>" _
                & genericController.kmaIndent(s3) _
                & cr & "</tr>" _
                & cr & "<tr>" _
                & cr2 & "<td colspan=""3"" class=""" & MyStyleShadow & """><img alt=""space"" src=""/ccLib/images/spacer.gif"" height=""1"" width=""" & MyWidth & """ ></td>" _
                & cr & "</tr>" _
                & ""
            main_GetPanel = "" _
                & cr & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""" & MyWidth & """ class=""" & MyStylePanel & """>" _
                & genericController.kmaIndent(s4) _
                & cr & "</table>" _
                & ""

            '-------------------------------------------------------------------------
            '
            '    main_GetPanel = "" _
            '        & cr & main_GetPanelTop(StylePanel, StyleHilite, StyleShadow, Width, Padding, HeightMin) _
            '        & genericController.kmaIndent(genericController.encodeText(Panel)) _
            '        & cr & main_GetPanelBottom(StylePanel, StyleHilite, StyleShadow, Width, Padding)
            '
            Exit Function
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError18("main_GetPanel")
        End Function
        '
        '========================================================================
        '   main_GetPanel( Panel, Optional StylePanel, Optional StyleHilite, Optional StyleShadow, Optional Width, Optional Padding, Optional HeightMin) As String
        ' Return a panel with the input as center
        '========================================================================
        '
        Public Function main_GetReversePanel(ByVal Panel As String, Optional ByVal StylePanel As String = "", Optional ByVal StyleHilite As String = "ccPanelShadow", Optional ByVal StyleShadow As String = "ccPanelHilite", Optional ByVal Width As String = "", Optional ByVal Padding As String = "", Optional ByVal HeightMin As String = "") As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetReversePanel")
            '
            'If Not (true) Then Exit Function
            '
            Dim MyStyleHilite As String
            Dim MyStyleShadow As String
            '
            MyStyleHilite = genericController.encodeEmptyText(StyleHilite, "ccPanelShadow")
            MyStyleShadow = genericController.encodeEmptyText(StyleShadow, "ccPanelHilite")

            main_GetReversePanel = main_GetPanelTop(StylePanel, MyStyleHilite, MyStyleShadow, Width, Padding, HeightMin) _
                & genericController.encodeText(Panel) _
                & main_GetPanelBottom(StylePanel, MyStyleHilite, MyStyleShadow, Width, Padding)
            '
            Exit Function
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError18("main_GetReversePanel")
        End Function
        '
        '========================================================================
        ' Return a panel header with the header message reversed out of the left
        '========================================================================
        '
        Public Function main_GetPanelHeader(ByVal HeaderMessage As String, Optional ByVal RightSideMessage As String = "") As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetPanelHeader")
            '
            Dim iHeaderMessage As String
            Dim iRightSideMessage As String
            Dim Adminui As New adminUIController(Me)
            '
            'If Not (true) Then Exit Function
            '
            iHeaderMessage = genericController.encodeText(HeaderMessage)
            iRightSideMessage = genericController.encodeEmptyText(RightSideMessage, FormatDateTime(app_startTime))
            main_GetPanelHeader = Adminui.GetHeader(iHeaderMessage, iRightSideMessage)
            '
            Exit Function
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError18("main_GetPanelHeader")
        End Function

        '
        '========================================================================
        ' Prints the top of display panel
        '   Must be closed with PrintPanelBottom
        '========================================================================
        '
        Public Function main_GetPanelTop(Optional ByVal StylePanel As String = "", Optional ByVal StyleHilite As String = "", Optional ByVal StyleShadow As String = "", Optional ByVal Width As String = "", Optional ByVal Padding As String = "", Optional ByVal HeightMin As String = "") As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetPanelTop")
            '
            'If Not (true) Then Exit Function
            '
            Dim ContentPanelWidth As String
            Dim MethodName As String
            Dim MyStylePanel As String
            Dim MyStyleHilite As String
            Dim MyStyleShadow As String
            Dim MyWidth As String
            Dim MyPadding As String
            Dim MyHeightMin As String
            '
            main_GetPanelTop = ""
            MyStylePanel = genericController.encodeEmptyText(StylePanel, "ccPanel")
            MyStyleHilite = genericController.encodeEmptyText(StyleHilite, "ccPanelHilite")
            MyStyleShadow = genericController.encodeEmptyText(StyleShadow, "ccPanelShadow")
            MyWidth = genericController.encodeEmptyText(Width, "100%")
            MyPadding = genericController.encodeEmptyText(Padding, "5")
            MyHeightMin = genericController.encodeEmptyText(HeightMin, "1")
            MethodName = "main_GetPanelTop"
            If genericController.vbIsNumeric(MyWidth) Then
                ContentPanelWidth = (CInt(MyWidth) - 2).ToString
            Else
                ContentPanelWidth = "100%"
            End If
            main_GetPanelTop = main_GetPanelTop _
                & cr & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""" & MyWidth & """ class=""" & MyStylePanel & """>"
            '
            ' --- top hilite row
            '
            main_GetPanelTop = main_GetPanelTop _
                & cr2 & "<tr>" _
                & cr3 & "<td colspan=""3"" class=""" & MyStyleHilite & """><img alt=""space"" src=""/ccLib/images/spacer.gif"" height=""1"" width=""" & MyWidth & """ ></td>" _
                & cr2 & "</tr>"
            '
            ' --- center row with Panel
            '
            main_GetPanelTop = main_GetPanelTop _
                & cr2 & "<tr>" _
                & cr3 & "<td width=""1"" height=""" & MyHeightMin & """ class=""" & MyStyleHilite & """><img alt=""space"" src=""/ccLib/images/spacer.gif"" height=""" & MyHeightMin & """ width=""1"" ></td>" _
                & cr3 & "<td width=""" & ContentPanelWidth & """ valign=""top"" align=""left"" class=""" & MyStylePanel & """>" _
                & cr4 & "<table border=""0"" cellpadding=""" & MyPadding & """ cellspacing=""0"" width=""" & ContentPanelWidth & """ class=""" & MyStylePanel & """>" _
                & cr5 & "<tr>" _
                & cr6 & "<td valign=""top"" class=""" & MyStylePanel & """><Span class=""" & MyStylePanel & """>"
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError18(MethodName)
            '
        End Function
        '
        '========================================================================
        ' Return a panel with the input as center
        '========================================================================
        '
        Public Function main_GetPanelBottom(Optional ByVal StylePanel As String = "", Optional ByVal StyleHilite As String = "", Optional ByVal StyleShadow As String = "", Optional ByVal Width As String = "", Optional ByVal Padding As String = "") As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetPanelBottom")
            '
            'If Not (true) Then Exit Function
            '
            Dim MethodName As String
            Dim MyStylePanel As String
            Dim MyStyleHilite As String
            Dim MyStyleShadow As String
            Dim MyWidth As String
            Dim MyPadding As String
            '
            MyStylePanel = genericController.encodeEmptyText(StylePanel, "ccPanel")
            MyStyleHilite = genericController.encodeEmptyText(StyleHilite, "ccPanelHilite")
            MyStyleShadow = genericController.encodeEmptyText(StyleShadow, "ccPanelShadow")
            MyWidth = genericController.encodeEmptyText(Width, "100%")
            MyPadding = genericController.encodeEmptyText(Padding, "5")
            MethodName = "main_GetPanelBottom"
            '
            main_GetPanelBottom = main_GetPanelBottom _
                & cr6 & "</span></td>" _
                & cr5 & "</tr>" _
                & cr4 & "</table>" _
                & cr3 & "</td>" _
                & cr3 & "<td width=""1"" class=""" & MyStyleShadow & """><img alt=""space"" src=""/ccLib/images/spacer.gif"" height=""1"" width=""1"" ></td>" _
                & cr2 & "</tr>" _
                & cr2 & "<tr>" _
                & cr3 & "<td colspan=""3"" class=""" & MyStyleShadow & """><img alt=""space"" src=""/ccLib/images/spacer.gif"" height=""1"" width=""" & MyWidth & """ ></td>" _
                & cr2 & "</tr>" _
                & cr & "</table>"
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError18(MethodName)
            '
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Public Function main_GetPanelButtons(ByVal ButtonValueList As String, ByVal ButtonName As String, Optional ByVal PanelWidth As String = "", Optional ByVal PanelHeightMin As String = "") As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetPanelButtons")
            '
            'If Not (true) Then Exit Function
            '
            Dim iButtonValueList As String
            Dim iButtonName As String
            Dim MethodName As String
            Dim Adminui As New adminUIController(Me)
            '
            iButtonValueList = genericController.encodeText(ButtonValueList)
            iButtonName = genericController.encodeText(ButtonName)
            '
            MethodName = "main_GetPanelButtons()"
            '
            main_GetPanelButtons = Adminui.GetButtonBar(Adminui.GetButtonsFromList(iButtonValueList, True, True, iButtonName), "")
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError18(MethodName)
        End Function
        '
        '
        '
        Public Function main_GetPanelRev(ByVal PanelContent As String, Optional ByVal PanelWidth As String = "", Optional ByVal PanelHeightMin As String = "") As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetPanelRev")
            '
            'If Not (true) Then Exit Function
            '
            main_GetPanelRev = main_GetPanel(PanelContent, "ccPanel", "ccPanelShadow", "ccPanelHilite", PanelWidth, 2, genericController.EncodeInteger(PanelHeightMin))
            Exit Function
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError18("main_GetPanelRev")
        End Function
        '
        '
        '
        Public Function main_GetPanelInput(ByVal PanelContent As String, Optional ByVal PanelWidth As String = "", Optional ByVal PanelHeightMin As String = "1") As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetPanelInput")
            '
            'If Not (true) Then Exit Function
            '
            main_GetPanelInput = main_GetPanel(PanelContent, "ccPanelInput", "ccPanelShadow", "ccPanelHilite", PanelWidth, 2, genericController.EncodeInteger(PanelHeightMin))
            Exit Function
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError18("main_GetPanelInput")
        End Function
        '
        '========================================================================
        ' Print the tools panel at the bottom of the page
        '========================================================================
        '
        Public Function main_GetToolsPanel() As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetToolsPanel")
            '
            'If Not (true) Then Exit Function
            '
            Dim copyNameValue As String
            Dim CopyName As String
            Dim copyValue As String
            Dim copyNameValueSplit() As String
            Dim VisitMin As Integer
            Dim VisitHrs As Integer
            Dim VisitSec As Integer
            Dim DebugPanel As String
            Dim Copy As String
            Dim CopySplit() As String
            Dim Ptr As Integer
            Dim EditTagID As String
            Dim QuickEditTagID As String
            Dim AdvancedEditTagID As String
            Dim WorkflowTagID As String
            Dim Tag As String
            Dim PathID As Integer
            Dim CS As Integer
            Dim PathsContentID As Integer
            Dim MethodName As String
            Dim TagID As String
            Dim ButtonPanel As String
            Dim ToolsPanel As stringBuilderLegacyController
            Dim OptionsPanel As String
            Dim LinkPanel As stringBuilderLegacyController
            Dim LoginPanel As String
            Dim iValueBoolean As Boolean
            Dim WorkingQueryString As String
            Dim ActionURL As String
            Dim BubbleCopy As String
            Dim AnotherPanel As stringBuilderLegacyController
            Dim ClipBoard As String
            Dim RenderTimeString As String
            Dim Adminui As New adminUIController(Me)
            Dim ToolsPanelAddonID As Integer
            Dim ShowLegacyToolsPanel As Boolean
            Dim QS As String
            '
            MethodName = "main_GetToolsPanel"
            '
            If authContext.user.AllowToolsPanel Then
                ShowLegacyToolsPanel = siteProperties.getBoolean("AllowLegacyToolsPanel", True)
                '
                ' --- Link Panel - used for both Legacy Tools Panel, and without it
                '
                LinkPanel = New stringBuilderLegacyController
                LinkPanel.Add(SpanClassAdminSmall)
                LinkPanel.Add("Contensive " & codeVersion() & " | ")
                LinkPanel.Add(FormatDateTime(app_startTime) & " | ")
                LinkPanel.Add("<a class=""ccAdminLink"" target=""_blank"" href=""http://support.Contensive.com/"">Support</A> | ")
                LinkPanel.Add("<a class=""ccAdminLink"" href=""" & htmlDoc.html_EncodeHTML(siteProperties.adminURL) & """>Admin Home</A> | ")
                LinkPanel.Add("<a class=""ccAdminLink"" href=""" & htmlDoc.html_EncodeHTML("http://" & webServer.webServerIO_requestDomain) & """>Public Home</A> | ")
                LinkPanel.Add("<a class=""ccAdminLink"" target=""_blank"" href=""" & htmlDoc.html_EncodeHTML(siteProperties.adminURL & "?" & RequestNameHardCodedPage & "=" & HardCodedPageMyProfile) & """>My Profile</A> | ")
                If siteProperties.getBoolean("AllowMobileTemplates", False) Then
                    If authContext.visit.Mobile Then
                        QS = htmlDoc.refreshQueryString
                        QS = genericController.ModifyQueryString(QS, "method", "forcenonmobile")
                        LinkPanel.Add("<a class=""ccAdminLink"" href=""?" & QS & """>Non-Mobile Version</A> | ")
                    Else
                        QS = htmlDoc.refreshQueryString
                        QS = genericController.ModifyQueryString(QS, "method", "forcemobile")
                        LinkPanel.Add("<a class=""ccAdminLink"" href=""?" & QS & """>Mobile Version</A> | ")
                    End If
                End If
                LinkPanel.Add("</span>")
                '
                If ShowLegacyToolsPanel Then
                    ToolsPanel = New stringBuilderLegacyController
                    WorkingQueryString = genericController.ModifyQueryString(htmlDoc.refreshQueryString, "ma", "", False)
                    '
                    ' ----- Tools Panel Caption
                    '
                    Dim helpLink As String
                    helpLink = ""
                    'helpLink = main_GetHelpLink("2", "Contensive Tools Panel", BubbleCopy)
                    BubbleCopy = "Use the Tools Panel to enable features such as editing and debugging tools. It also includes links to the admin site, the support site and the My Profile page."
                    main_GetToolsPanel = main_GetToolsPanel & main_GetPanelHeader("Contensive Tools Panel" & helpLink)
                    '
                    ToolsPanel.Add(htmlDoc.html_GetFormStart(WorkingQueryString))
                    ToolsPanel.Add(htmlDoc.html_GetFormInputHidden("Type", FormTypeToolsPanel))
                    '
                    If True Then
                        '
                        ' ----- Create the Options Panel
                        '
                        'PathsContentID = main_GetContentID("Paths")
                        '                '
                        '                ' Allow Help Links
                        '                '
                        '                iValueBoolean = visitProperty.getboolean("AllowHelpIcon")
                        '                TagID =  "AllowHelpIcon"
                        '                OptionsPanel = OptionsPanel & "" _
                        '                    & CR & "<div class=""ccAdminSmall"">" _
                        '                    & cr2 & "<LABEL for=""" & TagID & """>" & main_GetFormInputCheckBox2(TagID, iValueBoolean, TagID) & "&nbsp;Help</LABEL>" _
                        '                    & CR & "</div>"
                        '
                        EditTagID = "AllowEditing"
                        QuickEditTagID = "AllowQuickEditor"
                        AdvancedEditTagID = "AllowAdvancedEditor"
                        WorkflowTagID = "AllowWorkflowRendering"
                        '
                        ' Edit
                        '
                        helpLink = ""
                        'helpLink = main_GetHelpLink(7, "Enable Editing", "Display the edit tools for basic content, such as pages, copy and sections. ")
                        iValueBoolean = visitProperty.getBoolean("AllowEditing")
                        Tag = htmlDoc.html_GetFormInputCheckBox2(EditTagID, iValueBoolean, EditTagID)
                        Tag = genericController.vbReplace(Tag, ">", " onClick=""document.getElementById('" & QuickEditTagID & "').checked=false;document.getElementById('" & AdvancedEditTagID & "').checked=false;"">")
                        OptionsPanel = OptionsPanel _
                            & cr & "<div class=""ccAdminSmall"">" _
                            & cr2 & "<LABEL for=""" & EditTagID & """>" & Tag & "&nbsp;Edit</LABEL>" & helpLink _
                            & cr & "</div>"
                        '
                        ' Quick Edit
                        '
                        helpLink = ""
                        'helpLink = main_GetHelpLink(8, "Enable Quick Edit", "Display the quick editor to edit the main page content.")
                        iValueBoolean = visitProperty.getBoolean("AllowQuickEditor")
                        Tag = htmlDoc.html_GetFormInputCheckBox2(QuickEditTagID, iValueBoolean, QuickEditTagID)
                        Tag = genericController.vbReplace(Tag, ">", " onClick=""document.getElementById('" & EditTagID & "').checked=false;document.getElementById('" & AdvancedEditTagID & "').checked=false;"">")
                        OptionsPanel = OptionsPanel _
                            & cr & "<div class=""ccAdminSmall"">" _
                            & cr2 & "<LABEL for=""" & QuickEditTagID & """>" & Tag & "&nbsp;Quick Edit</LABEL>" & helpLink _
                            & cr & "</div>"
                        '
                        ' Advanced Edit
                        '
                        helpLink = ""
                        'helpLink = main_GetHelpLink(0, "Enable Advanced Edit", "Display the edit tools for advanced content, such as templates and add-ons. Basic content edit tools are also displayed.")
                        iValueBoolean = visitProperty.getBoolean("AllowAdvancedEditor")
                        Tag = htmlDoc.html_GetFormInputCheckBox2(AdvancedEditTagID, iValueBoolean, AdvancedEditTagID)
                        Tag = genericController.vbReplace(Tag, ">", " onClick=""document.getElementById('" & QuickEditTagID & "').checked=false;document.getElementById('" & EditTagID & "').checked=false;"">")
                        OptionsPanel = OptionsPanel _
                            & cr & "<div class=""ccAdminSmall"">" _
                            & cr2 & "<LABEL for=""" & AdvancedEditTagID & """>" & Tag & "&nbsp;Advanced Edit</LABEL>" & helpLink _
                            & cr & "</div>"
                        '
                        ' Workflow Authoring Render Mode
                        '
                        helpLink = ""
                        'helpLink = main_GetHelpLink(9, "Enable Workflow Rendering", "Control the display of workflow rendering. With workflow rendering enabled, any changes saved to content records that have not been published will be visible for your review.")
                        If siteProperties.allowWorkflowAuthoring Then
                            iValueBoolean = visitProperty.getBoolean("AllowWorkflowRendering")
                            Tag = htmlDoc.html_GetFormInputCheckBox2(WorkflowTagID, iValueBoolean, WorkflowTagID)
                            OptionsPanel = OptionsPanel _
                                & cr & "<div class=""ccAdminSmall"">" _
                                & cr2 & "<LABEL for=""" & WorkflowTagID & """>" & Tag & "&nbsp;Render Workflow Authoring Changes</LABEL>" & helpLink _
                                & cr & "</div>"
                        End If
                        helpLink = ""
                        iValueBoolean = visitProperty.getBoolean("AllowDebugging")
                        TagID = "AllowDebugging"
                        Tag = htmlDoc.html_GetFormInputCheckBox2(TagID, iValueBoolean, TagID)
                        OptionsPanel = OptionsPanel _
                            & cr & "<div class=""ccAdminSmall"">" _
                            & cr2 & "<LABEL for=""" & TagID & """>" & Tag & "&nbsp;Debug</LABEL>" & helpLink _
                            & cr & "</div>"
                        '
                        ' Create Path Block Row
                        '
                        If authContext.isAuthenticatedDeveloper(Me) Then
                            TagID = "CreatePathBlock"
                            If siteProperties.allowPathBlocking Then
                                '
                                ' Path blocking allowed
                                '
                                'OptionsPanel = OptionsPanel & SpanClassAdminSmall & "<LABEL for=""" & TagID & """>"
                                CS = db.cs_open("Paths", "name=" & db.encodeSQLText(webServer.webServerIO_requestPath), , , , , , "ID")
                                If db.cs_ok(CS) Then
                                    PathID = (db.cs_getInteger(CS, "ID"))
                                End If
                                Call db.cs_Close(CS)
                                If PathID <> 0 Then
                                    '
                                    ' Path is blocked
                                    '
                                    Tag = htmlDoc.html_GetFormInputCheckBox2(TagID, True, TagID) & "&nbsp;Path is blocked [" & webServer.webServerIO_requestPath & "] [<a href=""" & htmlDoc.html_EncodeHTML(siteProperties.adminURL & "?af=" & AdminFormEdit & "&id=" & PathID & "&cid=" & metaData.getContentId("paths") & "&ad=1") & """ target=""_blank"">edit</a>]</LABEL>"
                                Else
                                    '
                                    ' Path is not blocked
                                    '
                                    Tag = htmlDoc.html_GetFormInputCheckBox2(TagID, False, TagID) & "&nbsp;Block this path [" & webServer.webServerIO_requestPath & "]</LABEL>"
                                End If
                                helpLink = ""
                                'helpLink = main_GetHelpLink(10, "Enable Debugging", "Debugging is a developer only debugging tool. With Debugging enabled, ccLib.TestPoints(...) will print, ErrorTrapping will be displayed, redirections are blocked, and more.")
                                OptionsPanel = OptionsPanel _
                                    & cr & "<div class=""ccAdminSmall"">" _
                                    & cr2 & "<LABEL for=""" & TagID & """>" & Tag & "</LABEL>" & helpLink _
                                    & cr & "</div>"
                            End If
                        End If
                        '
                        ' Buttons
                        '
                        OptionsPanel = OptionsPanel & "" _
                            & cr & "<div class=""ccButtonCon"">" _
                            & cr2 & "<input type=submit name=" & "mb value=""" & ButtonApply & """>" _
                            & cr & "</div>" _
                            & ""
                    End If
                    '
                    ' ----- Create the Login Panel
                    '
                    If Trim(authContext.user.Name) = "" Then
                        Copy = "You are logged in as member #" & authContext.user.ID & "."
                    Else
                        Copy = "You are logged in as " & authContext.user.Name & "."
                    End If
                    LoginPanel = LoginPanel & "" _
                        & cr & "<div class=""ccAdminSmall"">" _
                        & cr2 & Copy & "" _
                        & cr & "</div>"
                    '
                    ' Username
                    '
                    Dim Caption As String
                    If siteProperties.getBoolean("allowEmailLogin", False) Then
                        Caption = "Username&nbsp;or&nbsp;Email"
                    Else
                        Caption = "Username"
                    End If
                    TagID = "Username"
                    LoginPanel = LoginPanel & "" _
                        & cr & "<div class=""ccAdminSmall"">" _
                        & cr2 & "<LABEL for=""" & TagID & """>" & htmlDoc.html_GetFormInputText2(TagID, "", 1, 30, TagID, False) & "&nbsp;" & Caption & "</LABEL>" _
                        & cr & "</div>"
                    '
                    ' Username
                    '
                    If siteProperties.getBoolean("allownopasswordLogin", False) Then
                        Caption = "Password&nbsp;(optional)"
                    Else
                        Caption = "Password"
                    End If
                    TagID = "Password"
                    LoginPanel = LoginPanel & "" _
                        & cr & "<div class=""ccAdminSmall"">" _
                        & cr2 & "<LABEL for=""" & TagID & """>" & htmlDoc.html_GetFormInputText2(TagID, "", 1, 30, TagID, True) & "&nbsp;" & Caption & "</LABEL>" _
                        & cr & "</div>"
                    '
                    ' Autologin checkbox
                    '
                    If siteProperties.getBoolean("AllowAutoLogin", False) Then
                        If authContext.visit.CookieSupport Then
                            TagID = "autologin"
                            LoginPanel = LoginPanel & "" _
                                & cr & "<div class=""ccAdminSmall"">" _
                                & cr2 & "<LABEL for=""" & TagID & """>" & htmlDoc.html_GetFormInputCheckBox2(TagID, True, TagID) & "&nbsp;Login automatically from this computer</LABEL>" _
                                & cr & "</div>"
                        End If
                    End If
                    '
                    ' Buttons
                    '
                    LoginPanel = LoginPanel & Adminui.GetButtonBar(Adminui.GetButtonsFromList(ButtonLogin & "," & ButtonLogout, True, True, "mb"), "")
                    '
                    ' ----- assemble tools panel
                    '
                    Copy = "" _
                        & cr & "<td width=""50%"" class=""ccPanelInput"" style=""vertical-align:bottom;"">" _
                        & genericController.kmaIndent(LoginPanel) _
                        & cr & "</td>" _
                        & cr & "<td width=""50%"" class=""ccPanelInput"" style=""vertical-align:bottom;"">" _
                        & genericController.kmaIndent(OptionsPanel) _
                        & cr & "</td>"
                    Copy = "" _
                        & cr & "<tr>" _
                        & genericController.kmaIndent(Copy) _
                        & cr & "</tr>" _
                        & ""
                    Copy = "" _
                        & cr & "<table border=""0"" cellpadding=""3"" cellspacing=""0"" width=""100%"">" _
                        & genericController.kmaIndent(Copy) _
                        & cr & "</table>"
                    ToolsPanel.Add(main_GetPanelInput(Copy))
                    ToolsPanel.Add(htmlDoc.html_GetFormEnd)
                    main_GetToolsPanel = main_GetToolsPanel & main_GetPanel(ToolsPanel.Text, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 5)
                    '
                    main_GetToolsPanel = main_GetToolsPanel & main_GetPanel(LinkPanel.Text, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 5)
                    '
                    LinkPanel = Nothing
                    ToolsPanel = Nothing
                    AnotherPanel = Nothing
                End If
                '
                ' --- Developer Debug Panel
                '
                If visitProperty.getBoolean("AllowDebugging") Then
                    '
                    ' --- Debug Panel Header
                    '
                    LinkPanel = New stringBuilderLegacyController
                    LinkPanel.Add(SpanClassAdminSmall)
                    'LinkPanel.Add( "WebClient " & main_WebClientVersion & " | "
                    LinkPanel.Add("Contensive " & codeVersion() & " | ")
                    LinkPanel.Add(FormatDateTime(app_startTime) & " | ")
                    LinkPanel.Add("<a class=""ccAdminLink"" target=""_blank"" href=""http: //support.Contensive.com/"">Support</A> | ")
                    LinkPanel.Add("<a class=""ccAdminLink"" href=""" & htmlDoc.html_EncodeHTML(siteProperties.adminURL) & """>Admin Home</A> | ")
                    LinkPanel.Add("<a class=""ccAdminLink"" href=""" & htmlDoc.html_EncodeHTML("http://" & webServer.webServerIO_requestDomain) & """>Public Home</A> | ")
                    LinkPanel.Add("<a class=""ccAdminLink"" target=""_blank"" href=""" & htmlDoc.html_EncodeHTML(siteProperties.adminURL & "?" & RequestNameHardCodedPage & "=" & HardCodedPageMyProfile) & """>My Profile</A> | ")
                    LinkPanel.Add("</span>")
                    '
                    '
                    '
                    'DebugPanel = DebugPanel & main_GetPanel(LinkPanel.Text, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", "5")
                    '
                    DebugPanel = DebugPanel _
                        & cr & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">" _
                        & cr2 & "<tr>" _
                        & cr3 & "<td width=""100"" class=""ccPanel""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""100"" height=""1"" ></td>" _
                        & cr3 & "<td width=""100%"" class=""ccPanel""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""1"" height=""1"" ></td>" _
                        & cr2 & "</tr>"
                    '
                    DebugPanel = DebugPanel & main_DebugPanelRow("DOM", "<a class=""ccAdminLink"" href=""/ccLib/clientside/DOMViewer.htm"" target=""_blank"">Click</A>")
                    DebugPanel = DebugPanel & main_DebugPanelRow("Trap Errors", htmlDoc.html_EncodeHTML(siteProperties.trapErrors.ToString))
                    DebugPanel = DebugPanel & main_DebugPanelRow("Trap Email", htmlDoc.html_EncodeHTML(siteProperties.getText("TrapEmail")))
                    DebugPanel = DebugPanel & main_DebugPanelRow("main_ServerLink", htmlDoc.html_EncodeHTML(webServer.webServerIO_ServerLink))
                    DebugPanel = DebugPanel & main_DebugPanelRow("main_ServerDomain", htmlDoc.html_EncodeHTML(webServer.webServerIO_requestDomain))
                    DebugPanel = DebugPanel & main_DebugPanelRow("main_ServerProtocol", htmlDoc.html_EncodeHTML(webServer.webServerIO_requestProtocol))
                    DebugPanel = DebugPanel & main_DebugPanelRow("main_ServerHost", htmlDoc.html_EncodeHTML(webServer.requestDomain))
                    DebugPanel = DebugPanel & main_DebugPanelRow("main_ServerPath", htmlDoc.html_EncodeHTML(webServer.webServerIO_requestPath))
                    DebugPanel = DebugPanel & main_DebugPanelRow("main_ServerPage", htmlDoc.html_EncodeHTML(webServer.webServerIO_requestPage))
                    Copy = ""
                    If webServer.requestQueryString <> "" Then
                        CopySplit = Split(webServer.requestQueryString, "&")
                        For Ptr = 0 To UBound(CopySplit)
                            copyNameValue = CopySplit(Ptr)
                            If copyNameValue <> "" Then
                                copyNameValueSplit = Split(copyNameValue, "=")
                                CopyName = genericController.DecodeResponseVariable(copyNameValueSplit(0))
                                copyValue = ""
                                If UBound(copyNameValueSplit) > 0 Then
                                    copyValue = genericController.DecodeResponseVariable(copyNameValueSplit(1))
                                End If
                                Copy = Copy & cr & "<br>" & htmlDoc.html_EncodeHTML(CopyName & "=" & copyValue)
                            End If
                        Next
                        Copy = Mid(Copy, 8)
                    End If
                    DebugPanel = DebugPanel & main_DebugPanelRow("main_ServerQueryString", Copy)
                    Copy = ""
                    For Each key As String In docProperties.getKeyList()
                        Dim docProperty As docPropertiesClass = docProperties.getProperty(key)
                        If docProperty.IsForm Then
                            Copy = Copy & cr & "<br>" & htmlDoc.html_EncodeHTML(docProperty.NameValue)
                        End If
                    Next
                    'DebugPanel = DebugPanel & main_DebugPanelRow("ServerForm", Copy)
                    'DebugPanel = DebugPanel & main_DebugPanelRow("Request Path", html.html_EncodeHTML(web_requestPath))
                    'DebugPanel = DebugPanel & main_DebugPanelRow("CDN Files Path", html.html_EncodeHTML(serverconfig.appConfig.cdnFilesNetprefix))
                    'DebugPanel = DebugPanel & main_DebugPanelRow("Referrer", html.html_EncodeHTML(web.requestReferrer))
                    'DebugPanel = DebugPanel & main_DebugPanelRow("Cookies", html.html_EncodeHTML(web.requestCookieString))
                    'DebugPanel = DebugPanel & main_DebugPanelRow("Visit Id", "<a href=""" & siteProperties.adminURL & "?cid=" & main_GetContentID("visits") & "&af=4&id=" & main_VisitId & """>" & main_VisitId & "</a>")
                    'DebugPanel = DebugPanel & main_DebugPanelRow("Visit Start Date", genericController.encodeText(main_VisitStartDateValue))
                    'DebugPanel = DebugPanel & main_DebugPanelRow("Visit Start Time", genericController.encodeText(main_VisitStartTime))
                    'DebugPanel = DebugPanel & main_DebugPanelRow("Visit Last Time", genericController.encodeText(main_VisitLastTime))
                    'DebugPanel = DebugPanel & main_DebugPanelRow("Visit Cookies Supported", genericController.encodeText(main_VisitCookieSupport))
                    'DebugPanel = DebugPanel & main_DebugPanelRow("Visit Pages", genericController.encodeText(main_VisitPages))
                    'DebugPanel = DebugPanel & main_DebugPanelRow("Visitor ID", "<a href=""" & siteProperties.adminURL & "?cid=" & main_GetContentID("visitors") & "&af=4&id=" & main_VisitorID & """>" & main_VisitorID & "</a>")
                    'DebugPanel = DebugPanel & main_DebugPanelRow("Visitor New", genericController.encodeText(main_VisitorNew))
                    'DebugPanel = DebugPanel & main_DebugPanelRow("Member ID", "<a href=""" & siteProperties.adminURL & "?cid=" & main_GetContentID("people") & "&af=4&id=" & authcontext.user.userId & """>" & authcontext.user.userId & "</a>")
                    'DebugPanel = DebugPanel & main_DebugPanelRow("Member Name", html.html_EncodeHTML(authContext.user.userName))
                    'DebugPanel = DebugPanel & main_DebugPanelRow("Member New", genericController.encodeText(authContext.user.userIsNew))
                    'DebugPanel = DebugPanel & main_DebugPanelRow("Member Language", authcontext.user.userLanguage)
                    'DebugPanel = DebugPanel & main_DebugPanelRow("Page", "<a href=""" & siteProperties.adminURL & "?cid=" & main_GetContentID("page content") & "&af=4&id=" & currentPageID & """>" & currentPageID & ", " & currentPageName & "</a>")
                    'DebugPanel = DebugPanel & main_DebugPanelRow("Section", "<a href=""" & siteProperties.adminURL & "?cid=" & main_GetContentID("site sections") & "&af=4&id=" & currentSectionID & """>" & currentSectionID & ", " & currentSectionName & "</a>")
                    'DebugPanel = DebugPanel & main_DebugPanelRow("Template", "<a href=""" & siteProperties.adminURL & "?cid=" & main_GetContentID("page templates") & "&af=4&id=" & currentTemplateID & """>" & currentTemplateID & ", " & currentTemplateName & "</a>")
                    'DebugPanel = DebugPanel & main_DebugPanelRow("Domain", "<a href=""" & siteProperties.adminURL & "?cid=" & main_GetContentID("domains") & "&af=4&id=" & domains.domainDetails.id & """>" & domains.domainDetails.id & ", " & main_ServerDomain & "</a>")
                    'DebugPanel = DebugPanel & main_DebugPanelRow("Template Reason", pageManager_TemplateReason)
                    'DebugPanel = DebugPanel & main_DebugPanelRow("ProcessID", GetProcessID.ToString())
                    'DebugPanel = DebugPanel & main_DebugPanelRow("Krnl ProcessID", genericController.encodeText(csv_HostServiceProcessID))
                    DebugPanel = DebugPanel & main_DebugPanelRow("Render Time &gt;= ", Format((GetTickCount - app_startTickCount) / 1000, "0.000") & " sec")
                    If True Then
                        VisitHrs = CInt(authContext.visit.TimeToLastHit / 3600)
                        VisitMin = CInt(authContext.visit.TimeToLastHit / 60) - (60 * VisitHrs)
                        VisitSec = authContext.visit.TimeToLastHit Mod 60
                        DebugPanel = DebugPanel & main_DebugPanelRow("Visit Length", CStr(authContext.visit.TimeToLastHit) & " sec, (" & VisitHrs & " hrs " & VisitMin & " mins " & VisitSec & " secs)")
                        'DebugPanel = DebugPanel & main_DebugPanelRow("Visit Length", CStr(main_VisitTimeToLastHit) & " sec, (" & Int(main_VisitTimeToLastHit / 60) & " min " & (main_VisitTimeToLastHit Mod 60) & " sec)")
                    End If
                    DebugPanel = DebugPanel & main_DebugPanelRow("Addon Profile", "<hr><ul class=""ccPanel"">" & "<li>tbd</li>" & cr & "</ul>")
                    '
                    DebugPanel = DebugPanel & "</table>"
                    '
                    If ShowLegacyToolsPanel Then
                        '
                        ' Debug Panel as part of legacy tools panel
                        '
                        main_GetToolsPanel = main_GetToolsPanel _
                            & main_GetPanel(DebugPanel, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 5)
                    Else
                        '
                        ' Debug Panel without Legacy Tools panel
                        '
                        main_GetToolsPanel = main_GetToolsPanel _
                            & main_GetPanelHeader("Debug Panel") _
                            & main_GetPanel(LinkPanel.Text) _
                            & main_GetPanel(DebugPanel, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 5)
                    End If
                End If
                main_GetToolsPanel = cr & "<div class=""ccCon"">" & genericController.kmaIndent(main_GetToolsPanel) & cr & "</div>"
            End If
            '
            Exit Function
            '
ErrorTrap:
            LinkPanel = Nothing
            ToolsPanel = Nothing
            AnotherPanel = Nothing
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError18(MethodName)
        End Function
        '
        '
        '
        Private Function main_DebugPanelRow(Label As String, Value As String) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("DebugPanelRow")
            '
            'If Not (true) Then Exit Function
            '
            main_DebugPanelRow = cr2 & "<tr><td valign=""top"" class=""ccPanel ccAdminSmall"">" & Label & "</td><td valign=""top"" class=""ccPanel ccAdminSmall"">" & Value & "</td></tr>"
            '
            Exit Function
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError13("main_DebugPanelRow")
        End Function
        '        '
        '        '========================================================================
        '        '   Content Watch
        '        '
        '        '   Creates or updates a record in the content watch content. Content Watch
        '        '   contains a record that links
        '        '   Update link entry for content watch record for this content record
        '        '========================================================================
        '        '
        '        Public Sub main_TrackContent(ByVal ContentName As String, ByVal RecordID As Integer)
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("TrackContent")
        '            '
        '            'If Not (true) Then Exit Sub
        '            '
        '            Dim CSPointer As Integer
        '            Dim MethodName As String
        '            Dim iContentName As String
        '            Dim iRecordID As Integer
        '            '
        '            If Not main_GetStreamBoolean2(RequestNameBlockContentTracking) Then
        '                iContentName = genericController.encodeText(ContentName)
        '                iRecordID = genericController.EncodeInteger(RecordID)
        '                '
        '                MethodName = "main_TrackContent"
        '                '
        '                CSPointer = main_OpenCSContentRecord2(iContentName, iRecordID)
        '                If Not app.IsCSOK(CSPointer) Then
        '                    throw New ApplicationException("Unexpected exception") ' handleLegacyError14(MethodName, "main_TrackContent, Error opening ContentSet from Content/Record [" & iContentName & "/" & genericController.encodeText(iRecordID) & "].")
        '                Else
        '                    Call main_TrackContentSet(CSPointer)
        '                End If
        '            End If
        '            '
        '            Exit Sub
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            throw New ApplicationException("Unexpected exception") ' handleLegacyError18(MethodName)
        '            '
        '        End Sub
        '        '
        '        '========================================================================
        '        ' Print a content blocks headline
        '        '   note - this call includes encoding
        '        '========================================================================
        '        '
        '        Public Function main_GetTitle(ByVal Title As String) As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetTitle")
        '            '
        '            'If Not (true) Then Exit Function
        '            '
        '            Dim iTitle As String
        '            '
        '            iTitle = genericController.encodeText(Title)
        '            If iTitle <> "" Then
        '                main_GetTitle = "<p>" & AddSpan(iTitle, "ccHeadline") & "</p>"
        '            End If
        '            Exit Function
        '            '
        'ErrorTrap:
        '            throw New ApplicationException("Unexpected exception") ' handleLegacyError18("main_GetTitle")
        '        End Function
        ''
        ''========================================================================
        ''   Print the login form in an intercept page
        ''========================================================================
        ''
        'Public Function main_GetLoginPage() As String
        '    main_GetLoginPage = user_GetLoginPage2(False)
        'End Function
        '        '
        '        '========================================================================
        '        ' ----- main_GetJoinForm()
        '        '   Prints the Registration Form
        '        '   If you are already a member, it takes you to the member profile form
        '        '   If the site does not allow open joining, this is blocked.
        '        '========================================================================
        '        '
        '        Public Function main_GetJoinForm() As String
        '            Dim returnHtml As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetJoinForm")
        '            '
        '            'If Not (true) Then Exit Function
        '            '
        '            Dim CS As Integer
        '            Dim CSPeople As Integer
        '            Dim CSUsernameCheck As Integer
        '            Dim JoinButton As String
        '            Dim MethodName As String
        '            Dim Copy As String
        '            Dim GroupIDList As String
        '            Dim FirstName As String
        '            '
        '            MethodName = "main_GetJoinForm"
        '            '
        '            CSPeople = app.csOpen("people", "ID=" & authcontext.user.userid)
        '            If Not app.csOk(CSPeople) Then
        '                '
        '                ' ----- could not open people, can not continue
        '                '
        '                throw New ApplicationException("Unexpected exception") ' handleLegacyError14(MethodName, "main_GetJoinForm, could not open the guest identity")
        '            Else
        '                If True Then
        '                    If authcontext.user.user_isRecognized() And Not authcontext.user.user_isAuthenticated() Then
        '                        '
        '                        ' ----- Recognized but Not authenticated
        '                        '
        '                        returnHtml = returnHtml & main_GetLoginForm()
        '                    Else
        '                        '
        '                        ' ----- Not authenticated, Guest identity, ask for information.
        '                        '
        '                        Dim QS As String
        '                        FirstName = db.cs_getText(CSPeople, "firstName")
        '                        If genericController.vbLCase(FirstName) = "guest" Then
        '                            FirstName = ""
        '                        End If
        '                        QS = htmlDoc.refreshQueryString
        '                        QS = genericController.ModifyQueryString(QS, "S", "")
        '                        QS = genericController.ModifyQueryString(QS, "ccIPage", "")
        '                        returnHtml = returnHtml & main_GetFormStart(QS)
        '                        returnHtml = returnHtml & htmldoc.html_GetFormInputHidden("Type", FormTypeJoin)
        '                        returnHtml = returnHtml & "<table border=""0"" cellpadding=""5"" cellspacing=""0"" width=""100%"">"
        '                        returnHtml = returnHtml & "<tr>"
        '                        returnHtml = returnHtml & "<td align=""right"" width=""30%"">" & SpanClassAdminNormal & "First Name</span></td>"
        '                        returnHtml = returnHtml & "<td align=""left""  width=""70%""><input NAME=""" & "firstname"" VALUE=""" & html.html_EncodeHTML(FirstName) & """ SIZE=""20"" MAXLENGTH=""50""></td>"
        '                        returnHtml = returnHtml & "</tr>"
        '                        returnHtml = returnHtml & "<tr>"
        '                        returnHtml = returnHtml & "<td align=""right"" width=""30%"">" & SpanClassAdminNormal & "Last Name</span></td>"
        '                        returnHtml = returnHtml & "<td align=""left""  width=""70%""><input NAME=""" & "lastname"" VALUE=""" & html.html_EncodeHTML(db.cs_getText(CSPeople, "lastname")) & """ SIZE=""20"" MAXLENGTH=""50""></td>"
        '                        returnHtml = returnHtml & "</tr>"
        '                        returnHtml = returnHtml & "<tr>"
        '                        returnHtml = returnHtml & "<td align=""right"" width=""30%"">" & SpanClassAdminNormal & "Email</span></td>"
        '                        returnHtml = returnHtml & "<td align=""left""  width=""70%""><input NAME=""" & "email"" VALUE=""" & html.html_EncodeHTML(db.cs_getText(CSPeople, "email")) & """ SIZE=""20"" MAXLENGTH=""50""></td>"
        '                        returnHtml = returnHtml & "</tr>"
        '                        returnHtml = returnHtml & "<tr>"
        '                        returnHtml = returnHtml & "<td align=""right"" width=""30%"">" & SpanClassAdminNormal & "Username</span></td>"
        '                        returnHtml = returnHtml & "<td align=""left""  width=""70%""><input NAME=""" & "username"" VALUE=""" & html.html_EncodeHTML(db.cs_getText(CSPeople, "username")) & """ SIZE=""20"" MAXLENGTH=""50""></td>"
        '                        returnHtml = returnHtml & "</tr>"
        '                        returnHtml = returnHtml & "<tr>"
        '                        returnHtml = returnHtml & "<td align=""right"" width=""30%"">" & SpanClassAdminNormal & "Password</span></td>"
        '                        returnHtml = returnHtml & "<td align=""left""  width=""70%""><input type=password NAME=""" & "password"" SIZE=""20"" MAXLENGTH=""50""></td>"
        '                        returnHtml = returnHtml & "</tr>"
        '                        returnHtml = returnHtml & "<tr><td colspan=""2"">" & main_GetPanelButtons(ButtonRegister, "Button") & "</td></tr>"
        '                        returnHtml = returnHtml & "</table>"
        '                        returnHtml = returnHtml & "</form>"
        '                    End If
        '                End If
        '            End If
        '            '
        '            main_GetJoinForm = returnHtml
        '            '
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            throw New ApplicationException("Unexpected exception") ' handleLegacyError18(MethodName)
        '            '
        '        End Function
        '        '
        '        '========================================================================
        '        ' ----- main_GetMyProfileForm()
        '        '
        '        '   Anyone can have access to this form, if they are authenticated.
        '        '   To give a guest access, assign then a username and password and authenticated them.
        '        '========================================================================
        '        '
        '        Public Function main_GetMyProfileForm(PeopleID As Integer) As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetMyProfileForm")
        '            '
        '            'If Not (true) Then Exit Function
        '            '
        '            Const TableOpen = vbCrLf & "<table border=""0"" cellpadding=""3"" cellspacing=""0"" width=""100%""><tr><td width=""150""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""140"" height=""1""></td><td><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""100%"" height=""1""></td></tr>"
        '            Const TableClose = vbCrLf & "</table>"
        '            '
        '            Dim CSMember As Integer
        '            Dim MethodName As String
        '            Dim iPeopleID As Integer
        '            Dim CSLastVisit As Integer
        '            Dim RowDivider As String
        '            Dim RowCount As Integer
        '            Dim Stream As New coreFastStringClass
        '            Dim ButtonPanel As String
        '            Dim ButtonList As String
        '            Dim ContentName As String
        '            Dim ContentID As Integer
        '            Dim s As coreFastStringClass
        '            '
        '            iPeopleID = genericController.EncodeInteger(PeopleID)
        '            '
        '            MethodName = "main_GetMyProfileForm"
        '            '
        '            If Not authcontext.user.user_isAuthenticated() Then
        '                Call errorController.error_AddUserError(me,"You can not edit your MyAccount page until you have logged in.")
        '            Else
        '                CSMember = main_OpenContent("People", "ID=" & app.EncodeSQLNumber(iPeopleID))
        '                If app.csOk(CSMember) Then
        '                    '
        '                    ContentID = app.cs_getInteger(CSMember, "ContentControlID")
        '                    ContentName = metaData.getContentNameByID(ContentID)
        '                    '
        '                    ' ----- member personal information
        '                    '
        '                    s = New coreFastStringClass
        '                    s.Add(TableOpen)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "Name", "", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "FirstName", "", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "LastName", "", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "Email", "The internet email address where you can be contacted. This address is used to confirm your username and paStreamword.", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "Company", "Your employer", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "title", "Your job title", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "Address", "Your street address", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "Address2", "Your street address", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "City", "", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "State", "Your state or provence", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "Zip", "Your zipcode or postal code", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "Country", "", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "Phone", "", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "Fax", "", RowCount) & vbCrLf)
        '                    's.Add( main_GetMyProfileForm_RowCS(CSMember, "People", "imagefilename", "", RowCount) & vbcrlf )
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "ResumeFilename", "", RowCount) & vbCrLf)
        '                    s.Add(TableClose)
        '                    Call main_AddLiveTabEntry("Contact", s.Text)
        '                    '
        '                    ' ----- billing
        '                    '
        '                    s = New coreFastStringClass
        '                    s.Add(TableOpen)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "BillName", "", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "BillEmail", "", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "BillCompany", "Your employer", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "BillAddress", "Your street address", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "BillCity", "", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "BillState", "Your state or provence", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "BillZip", "Your zipcode or postal code", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "BillCountry", "", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "BillPhone", "", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "BillFax", "", RowCount) & vbCrLf)
        '                    s.Add(TableClose)
        '                    Call main_AddLiveTabEntry("Billing", s.Text)
        '                    '
        '                    ' ----- Shipping Information
        '                    '
        '                    s = New coreFastStringClass
        '                    s.Add(TableOpen)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "ShipName", "", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "ShipCompany", "", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "ShipAddress", "Your street address", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "ShipCity", "", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "ShipState", "Your state or provence", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "ShipZip", "Your zipcode or postal code", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "ShipCountry", "", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "ShipPhone", "", RowCount) & vbCrLf)
        '                    s.Add(TableClose)
        '                    Call main_AddLiveTabEntry("Shipping", s.Text)
        '                    '
        '                    ' ----- Site Preferences
        '                    '
        '                    s = New coreFastStringClass
        '                    s.Add(TableOpen)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "username", "Used with your password to gain access to the site.", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "Password", "Use with your username to gain access to the site.", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "allowbulkemail", "If checked, we may send you updates about our site from time to time.", RowCount) & vbCrLf)
        '                    ' 6/18/2009 - removed notes from base
        '                    '            s.Add( main_GetMyProfileForm_RowCS(CSMember, "People", "sendnotes", "If checked, notes sent to you as a site member will be emailed. Otherwise, they are available only when you have logged on.", RowCount) & vbCrLf
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "LanguageID", "select your prefered language. If content is aviable in your language, is will be displayed. Otherwise, the default language will be used.", RowCount) & vbCrLf)
        '                    If genericController.EncodeBoolean(app.siteProperty_getBoolean("AllowAutoLogin", False)) Then
        '                        s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "autologin", "This site allows automatic login. If this box is check, you will enable this function for your member account.", RowCount) & vbCrLf)
        '                    End If
        '                    If authcontext.user.main_IsContentManager() Then
        '                        s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "allowtoolspanel", "If checked, a tools panel appears at the bottom of every active page with acceStream to key administrative functions.", RowCount) & vbCrLf)
        '                    End If
        '                    s.Add(TableClose)
        '                    Call main_AddLiveTabEntry("Preferences", s.Text)
        '                    '
        '                    ' ----- Interest Topics
        '                    '
        '                    s = New coreFastStringClass
        '                    s.Add(TableOpen)
        '                    s.Add("<tr><td colspan=""2"">" & SpanClassAdminNormal & "<b>Selected Topics</b></span><br ><img src=""/ccLib/images/black.gif"" width=""100%"" height=""1"" ></td></tr>" & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_Topics(iPeopleID, ContentName) & vbCrLf)
        '                    s.Add("<tr><td colspan=""2"">" & SpanClassAdminNormal & "<b>Topics Habits</b></span><br ><img src=""/ccLib/images/black.gif"" width=""100%"" height=""1"" ></td></tr>" & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_TopicHabits)
        '                    s.Add(TableClose)
        '                    Call main_AddLiveTabEntry("Topics", s.Text)
        '                    '
        '                    ' ----- Group main_MemberShip
        '                    '
        '                    s = New coreFastStringClass
        '                    s.Add(TableOpen)
        '                    s.Add(main_GetMyProfileForm_Groups)
        '                    s.Add(TableClose)
        '                    Call main_AddLiveTabEntry("Groups", s.Text)
        '                    '
        '                    ' ----- Records
        '                    '
        '                    s = New coreFastStringClass
        '                    s.Add(TableOpen)
        '                    s.Add(main_GetMyProfileForm_Row("Member Number", genericController.encodeText(authContext.user.userid)) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_Row("Visitor Number", genericController.encodeText(main_VisitorID)) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_Row("Visit Number", genericController.encodeText(main_VisitId)) & vbCrLf)
        '                    CSLastVisit = app.csOpen("Visits", "MemberID=" & iPeopleID, "ID DESC")
        '                    If app.csOk(CSLastVisit) Then
        '                        s.Add(main_GetMyProfileForm_RowCS(CSLastVisit, "Visits", "StartTime", "", RowCount, "Start Time", True) & vbCrLf)
        '                        s.Add(main_GetMyProfileForm_RowCS(CSLastVisit, "Visits", "REMOTE_ADDR", "", RowCount, "IP Address", True) & vbCrLf)
        '                        s.Add(main_GetMyProfileForm_RowCS(CSLastVisit, "Visits", "Browser", "", RowCount, "", True) & vbCrLf)
        '                        s.Add(main_GetMyProfileForm_RowCS(CSLastVisit, "Visits", "HTTP_REFERER", "", RowCount, "Referer", True) & vbCrLf)
        '                        s.Add(main_GetMyProfileForm_RowCS(CSLastVisit, "Visits", "CookieSupport", "", RowCount, "Cookie Support", True) & vbCrLf)
        '                    End If
        '                    Call app.csClose(CSLastVisit)
        '                    s.Add(TableClose)
        '                    Call main_AddLiveTabEntry("Statistics", s.Text)
        '                    '
        '                    ' ----- save button
        '                    '
        '                    RowCount = 0
        '                    Stream.Add(main_GetFormStart)
        '                    ButtonList = ButtonSave & "," & ButtonCancel
        '                    ButtonPanel = main_GetPanelButtons(ButtonList, "Button")
        '                    Stream.Add(ButtonPanel)
        '                    Stream.Add(html_GetFormInputHidden("Type", FormTypeMyProfile))
        '                    Stream.Add(error_GetUserError())
        '                    Stream.Add("<div>&nbsp;</div>")
        '                    Stream.Add(main_GetLiveTabs())
        '                    Stream.Add(ButtonPanel)
        '                    '
        '                    main_GetMyProfileForm = Stream.Text & "</form>"
        '                End If
        '            End If
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            throw New ApplicationException("Unexpected exception") ' handleLegacyError18(MethodName)
        '            '
        '        End Function
        '        '
        '        '========================================================================
        '        ' ----- main_Get_old_MyProfileForm()
        '        '
        '        '   Anyone can have access to this form, if they are authenticated.
        '        '   To give a guest access, assign then a username and password and authenticated them.
        '        '========================================================================
        '        '
        '        Public Function main_Get_old_MyProfileForm(PeopleID As Integer) As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Get_old_MyProfileForm")
        '            '
        '            'If Not (true) Then Exit Function
        '            '
        '            Dim CSMember As Integer
        '            Dim MethodName As String
        '            Dim iPeopleID As Integer
        '            Dim CSLastVisit As Integer
        '            Dim RowDivider As String
        '            Dim RowCount As Integer
        '            Dim Stream As New coreFastStringClass
        '            Dim ButtonPanel As String
        '            Dim ButtonList As String
        '            Dim ContentName As String
        '            Dim ContentID As Integer
        '            Dim tabContent(5) As String
        '            '
        '            iPeopleID = genericController.EncodeInteger(PeopleID)
        '            '
        '            MethodName = "main_Get_old_MyProfileForm"
        '            '
        '            If Not authcontext.user.user_isAuthenticated() Then
        '                Call errorController.error_AddUserError(me,"You can not edit your MyAccount page until you have logged in.")
        '            Else
        '                CSMember = main_OpenContent("People", "ID=" & db.EncodeSQLNumber(iPeopleID))
        '                If db.csOk(CSMember) Then
        '                    ContentID = db.cs_getInteger(CSMember, "ContentControlID")
        '                    ContentName = metaData.getContentNameByID(ContentID)
        '                    '
        '                    RowDivider = "<tr><td width=""100%"" align=""left""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""1"" height=""1"" ></td></tr>" & vbCrLf
        '                    RowCount = 0
        '                    Stream.Add(main_GetFormStart)
        '                    ButtonList = ButtonSave & "," & ButtonCancel
        '                    ButtonPanel = main_GetPanelButtons(ButtonList, "Button")
        '                    Stream.Add(ButtonPanel)
        '                    Stream.Add(html_GetFormInputHidden("Type", FormTypeMyProfile))
        '                    Stream.Add(error_GetUserError())
        '                    Stream.Add("<table border=""0"" cellpadding=""3"" cellspacing=""0"" width=""100%"">")
        '                    Stream.Add("<tr><td width=""150""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""140"" height=""1"" ></td><td><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""100%"" height=""1"" ></td></tr>" & vbCrLf)
        '                    '
        '                    ' ----- member personal information
        '                    '
        '                    Stream.Add("<tr>" & vbCrLf)
        '                    Stream.Add("<td colspan=""2"">" & SpanClassAdminNormal & "<b>Your Information</b></span><br ><img src=""/ccLib/images/black.gif"" width=""100%"" height=""1"" ></td>" & vbCrLf)
        '                    Stream.Add("</tr>" & vbCrLf)
        '                    '
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "Name", "", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "FirstName", "", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "LastName", "", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "Email", "The internet email address where you can be contacted. This address is used to confirm your username and paStreamword.", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "Company", "Your employer", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "title", "Your job title", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "Address", "Your street address", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "City", "", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "State", "Your state or provence", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "Zip", "Your zipcode or postal code", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "Country", "", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "Phone", "", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "Fax", "", RowCount) & vbCrLf)
        '                    'Stream.Add( main_GetMyProfileForm_RowCS(CSMember, "People", "imagefilename", "", RowCount) & vbcrlf )
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "ResumeFilename", "", RowCount) & vbCrLf)
        '                    '
        '                    Stream.Add("<tr>" & vbCrLf)
        '                    Stream.Add("<td colspan=""2"">" & SpanClassAdminNormal & "<b>Billing Information (for online commerce only)</b></span><br ><img src=""/ccLib/images/black.gif"" width=""100%"" height=""1"" ></td>" & vbCrLf)
        '                    Stream.Add("</tr>" & vbCrLf)
        '                    '
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "BillName", "", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "BillEmail", "", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "BillCompany", "Your employer", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "BillAddress", "Your street address", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "BillCity", "", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "BillState", "Your state or provence", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "BillZip", "Your zipcode or postal code", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "BillCountry", "", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "BillPhone", "", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "BillFax", "", RowCount) & vbCrLf)
        '                    '
        '                    ' ----- Shipping Information
        '                    '
        '                    Stream.Add("<tr><td colspan=""2"">" & SpanClassAdminNormal & "<b>Shipping Information (for online commerce only)</b></span><br ><img src=""/ccLib/images/black.gif"" width=""100%"" height=""1"" ></td></tr>" & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "ShipName", "", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "ShipCompany", "", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "ShipAddress", "Your street address", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "ShipCity", "", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "ShipState", "Your state or provence", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "ShipZip", "Your zipcode or postal code", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "ShipCountry", "", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "ShipPhone", "", RowCount) & vbCrLf)
        '                    '
        '                    ' ----- Site Preferences
        '                    '
        '                    Stream.Add("<tr><td colspan=""2"">" & SpanClassAdminNormal & "<b>Site Preferences</b></span><br ><img src=""/ccLib/images/black.gif"" width=""100%"" height=""1"" ></td></tr>" & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "username", "Used with your password to gain access to the site.", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "Password", "Use with your username to gain access to the site.", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "allowbulkemail", "If checked, we may send you updates about our site from time to time.", RowCount) & vbCrLf)
        '                    ' 6/18/2009 - removed notes from base
        '                    '            Stream.Add( main_GetMyProfileForm_RowCS(CSMember, "People", "sendnotes", "If checked, notes sent to you as a site member will be emailed. Otherwise, they are available only when you have logged on.", RowCount) & vbcrlf )
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "LanguageID", "", RowCount) & vbCrLf)
        '                    If siteProperties.getBoolean("AllowAutoLogin", False) Then
        '                        Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "autologin", "This site allows automatic login. If this box is check, you will enable this function for your member account.", RowCount) & vbCrLf)
        '                    End If
        '                    If authcontext.user.main_IsContentManager() Then
        '                        Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "allowtoolspanel", "If checked, a tools panel appears at the bottom of every active page with acceStream to key administrative functions.", RowCount) & vbCrLf)
        '                    End If
        '                    '
        '                    ' ----- Interest Topics
        '                    '
        '                    Stream.Add("<tr><td colspan=""2"">" & SpanClassAdminNormal & "<b>Topics of Interest</b></span><br ><img src=""/ccLib/images/black.gif"" width=""100%"" height=""1"" ></td></tr>" & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_Topics(iPeopleID, ContentName) & vbCrLf)
        '                    '
        '                    ' ----- Topics Habits
        '                    '
        '                    Stream.Add("<tr><td colspan=""2"">" & SpanClassAdminNormal & "<b>Topic Habits</b></span><br ><img src=""/ccLib/images/black.gif"" width=""100%"" height=""1"" ></td></tr>" & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_TopicHabits)
        '                    '
        '                    ' ----- Group main_MemberShip
        '                    '
        '                    Stream.Add("<tr><td colspan=""2"">" & SpanClassAdminNormal & "<b>Group main_MemberShip</b></span><br ><img src=""/ccLib/images/black.gif"" width=""100%"" height=""1"" ></td></tr>" & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_Groups)
        '                    '
        '                    ' ----- Records
        '                    '
        '                    Stream.Add("<tr><td colspan=""2"">" & SpanClassAdminNormal & "<b>Statistics</b></span><br ><img src=""/ccLib/images/black.gif"" width=""100%"" height=""1"" ></td></tr>" & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_Row("Member Number", genericController.encodeText(authContext.user.userId)) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_Row("Visitor Number", genericController.encodeText(main_VisitorID)) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_Row("Visit Number", genericController.encodeText(main_VisitId)) & vbCrLf)
        '                    CSLastVisit = db.csOpen("Visits", "MemberID=" & iPeopleID, "ID DESC")
        '                    If db.csOk(CSLastVisit) Then
        '                        'Stream.Add( main_GetMyProfileForm_RowCS(CSLastVisit, "Visits", "main_VisitorID", "", RowCount) & vbcrlf )
        '                        'Stream.Add( main_GetMyProfileForm_RowCS(CSLastVisit, "Visits", "ID", "", RowCount) & vbcrlf )
        '                        Stream.Add(main_GetMyProfileForm_RowCS(CSLastVisit, "Visits", "StartTime", "", RowCount, "Start Time", True) & vbCrLf)
        '                        Stream.Add(main_GetMyProfileForm_RowCS(CSLastVisit, "Visits", "REMOTE_ADDR", "", RowCount, "IP Address", True) & vbCrLf)
        '                        Stream.Add(main_GetMyProfileForm_RowCS(CSLastVisit, "Visits", "Browser", "", RowCount, "", True) & vbCrLf)
        '                        Stream.Add(main_GetMyProfileForm_RowCS(CSLastVisit, "Visits", "HTTP_REFERER", "", RowCount, "Referer", True) & vbCrLf)
        '                        'Stream.Add( main_GetMyProfileForm_RowCS(CSLastVisit, "Visits", "RefererPathPage", "", RowCount) & vbcrlf )
        '                        Stream.Add(main_GetMyProfileForm_RowCS(CSLastVisit, "Visits", "CookieSupport", "", RowCount, "Cookie Support", True) & vbCrLf)
        '                        'Stream.Add( main_GetMyProfileForm_RowCS(CSLastVisit, "Visits", "main_VisitAuthenticated", "", RowCount) & vbcrlf )
        '                    End If
        '                    Call db.csClose(CSLastVisit)
        '                    '
        '                    Stream.Add("</table>")
        '                    '
        '                    ' ----- save button
        '                    '
        '                    Stream.Add(ButtonPanel)
        '                    '
        '                    main_Get_old_MyProfileForm = Stream.Text & "</form>"
        '                End If
        '            End If
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            throw New ApplicationException("Unexpected exception") ' handleLegacyError18(MethodName)
        '            '
        '        End Function
        '        '
        '        ' -----
        '        '
        '        Private Function main_GetMyProfileForm_RowCS(ByVal CSPointer As Integer, ByVal ContentName As String, ByVal FieldName As String, ByVal Explaination As String, ByVal RowCount As Integer, Optional ByVal Caption As String = "", Optional ByVal readOnlyField As Boolean = False) As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetMyProfileForm_RowCS")
        '            '
        '            'If Not (true) Then Exit Function
        '            '
        '            Dim Stream As String
        '            Dim MethodName As String
        '            Dim iCaption As String
        '            '
        '            MethodName = "main_GetMyProfileForm_RowCS"
        '            '
        '            iCaption = Caption
        '            If Caption = "" Then
        '                iCaption = db.cs_getFieldCaption(CSPointer, FieldName)
        '            End If
        '            '
        '            If Not db.csOk(CSPointer) Then
        '                throw (New Exception("ContentSet argument is not valid"))
        '            Else
        '                If readOnlyField Then
        '                    Stream = db.cs_getText(CSPointer, FieldName)
        '                Else
        '                    Stream = html_GetFormInputCS(CSPointer, ContentName, FieldName, , 60)
        '                End If
        '                main_GetMyProfileForm_RowCS = main_GetMyProfileForm_Row(iCaption, Stream)
        '            End If
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            throw New ApplicationException("Unexpected exception") ' handleLegacyError13(MethodName)
        '        End Function
        '        '
        '        ' ----- main_GetMyProfileForm_RowCS()
        '        '
        '        Private Function main_GetMyProfileForm_Row(LeftSide As String, RightSide As String) As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetMyProfileForm_Row")
        '            '
        '            'If Not (true) Then Exit Function
        '            '
        '            main_GetMyProfileForm_Row = "" _
        '                & "<tr>" _
        '                & "<td align=""right"" valign=""middle""><span class=""ccAdminSmall"">" & LeftSide & "</span></td>" _
        '                & "<td align=""left""  valign=""middle""><span class=""ccAdminSmall"">" & RightSide & "</span></td>" _
        '                & "</tr>"
        '            Exit Function
        'ErrorTrap:
        '            throw New ApplicationException("Unexpected exception") ' handleLegacyError13("main_GetMyProfileForm_Row")
        '        End Function
        '        '
        '        '========================================================================
        '        ' ----- main_Get a string with the topic groups and check the ones this member has selected
        '        '========================================================================
        '        '
        '        Private Function main_GetMyProfileForm_Topics(PeopleID As Integer, ContentName As String) As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetMyProfileForm_Topics")
        '            '
        '            Dim Stream As String
        '            '
        '            Stream = main_GetFormInputCheckList(rnMyProfileTopics, "people", PeopleID, "topics", "member topic rules", "memberid", "topicid")
        '            'Stream = main_GetFormInputTopics("Topic", "Topics", ContentName, PeopleID)
        '            '
        '            ' Empty case
        '            '
        '            If Stream = "" Then
        '                Stream = "There are currently no topics defined"
        '            End If
        '            '
        '            ' Set it in the output
        '            '
        '            main_GetMyProfileForm_Topics = "" _
        '                & "<tr>" _
        '                & "<td align=""right"" valign=""top"">" & SpanClassAdminSmall & "</span></td>" _
        '                & "<td valign=""top"">" & SpanClassAdminNormal & Stream & "</span></td>" _
        '                & "</tr>"
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            throw New ApplicationException("Unexpected exception") ' handleLegacyError13("main_GetMyProfileForm_Topics")
        '        End Function
        '        '
        '        '========================================================================
        '        ' ----- main_Get a string with the topic groups and check the ones this member has selected
        '        '========================================================================
        '        '
        '        Private Function main_GetMyProfileForm_TopicHabits() As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetMyProfileForm_TopicHabits")
        '            '
        '            'If Not (true) Then Exit Function
        '            '
        '            Dim SQL As String
        '            Dim CS As Integer
        '            Dim Stream As String
        '            Dim MethodName As String
        '            Dim TopicCount As Integer
        '            '
        '            MethodName = "main_GetMyProfileForm_TopicHabits"
        '            '
        '            ' ----- Gather all the topics to which this member belongs
        '            '
        '            SQL = "SELECT ccTopics.Name as Name, Sum( ccTopicHabits.Score ) as Score" _
        '                & " FROM ccTopics LEFT JOIN ccTopicHabits ON ccTopics.ID = ccTopicHabits.TopicID" _
        '                & " WHERE (((ccTopics.Active)<>0) AND ((ccTopicHabits.MemberID)=" & authcontext.user.userId & ")) OR (((ccTopics.Active)<>0) AND ((ccTopicHabits.MemberID) Is Null))" _
        '                & " Group By ccTopics.name" _
        '                & " Order by ccTopics.Name"
        '            CS = db.csOpenSql(SQL)
        '            Do While db.csOk(CS)
        '                Stream = Stream & SpanClassAdminNormal & db.cs_getText(CS, "name") & " = " & genericController.encodeText(db.cs_getInteger(CS, "score")) & "</span><BR >"
        '                Call db.csGoNext(CS)
        '                TopicCount = TopicCount + 1
        '            Loop
        '            Call db.csClose(CS)
        '            '
        '            '
        '            '
        '            If TopicCount = 0 Then
        '                Stream = "There are currently no topics defined"
        '            End If
        '            '
        '            ' ----- Set it in the output
        '            '
        '            main_GetMyProfileForm_TopicHabits = "" _
        '                & "<tr>" _
        '                & "<td align=""right"" valign=""top"">" & SpanClassAdminSmall & "</span></td>" _
        '                & "<td valign=""top"">" & SpanClassAdminNormal & Stream & "</span></td>" _
        '                & "</tr>"
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError13(MethodName)
        '        End Function
        '        '
        '        '========================================================================
        '        ' ----- main_Get a string with the topic groups and check the ones this member has selected
        '        '========================================================================
        '        '
        '        Private Function main_GetMyProfileForm_Groups() As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetMyProfileForm_Groups")
        '            '
        '            'If Not (true) Then Exit Function
        '            '
        '            Dim MethodName As String
        '            Dim PublicJoinCriteria As String
        '            Dim Stream As String
        '            Dim GroupList As String

        '            '
        '            MethodName = "main_GetMyProfileForm_Groups"
        '            '
        '            If Not authcontext.user.user_isAdmin() Then
        '                PublicJoinCriteria = "ccgroups.PublicJoin<>0"
        '            End If
        '            '
        '            GroupList = main_GetFormInputCheckList("MemberRules", "People", authcontext.user.userId, "Groups", "Member Rules", "MemberID", "GroupID", PublicJoinCriteria, "Caption")
        '            If GroupList = "" Then
        '                GroupList = "<div>There are no public groups</div>"
        '            End If
        '            main_GetMyProfileForm_Groups = "" _
        '                & "<tr>" _
        '                & "<td align=""right"" valign=""top"">" & SpanClassAdminSmall & "</span></td>" _
        '                & "<td valign=""top"">" & SpanClassAdminNormal & GroupList & "</span></td>" _
        '                & "</tr>"
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError13(MethodName)
        '        End Function
        '
        '=============================================================================
        ' main_Get the GroupID from iGroupName
        '=============================================================================
        '
        Public Function group_GetGroupID(ByVal GroupName As String) As Integer
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetGroupID")
            '
            'If Not (true) Then Exit Function
            '
            Dim dt As DataTable
            Dim MethodName As String
            Dim iGroupName As String
            '
            iGroupName = genericController.encodeText(GroupName)
            '
            MethodName = "main_GetGroupID"
            '
            group_GetGroupID = 0
            If (iGroupName <> "") Then
                '
                ' ----- main_Get the Group ID
                '
                dt = db.executeSql("select top 1 id from ccGroups where name=" & db.encodeSQLText(iGroupName))
                If dt.Rows.Count > 0 Then
                    group_GetGroupID = genericController.EncodeInteger(dt.Rows(0).Item(0))
                End If
            End If
            '
            Exit Function
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
        End Function
        '
        '=============================================================================
        ' main_Get the GroupName from iGroupID
        '=============================================================================
        '
        Public Function group_GetGroupName(GroupID As Integer) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetGroupByID")
            '
            'If Not (true) Then Exit Function
            '
            Dim CS As Integer
            Dim MethodName As String
            Dim iGroupID As Integer
            '
            iGroupID = genericController.EncodeInteger(GroupID)
            '
            MethodName = "main_GetGroupByID"
            '
            group_GetGroupName = ""
            If (iGroupID > 0) Then
                '
                ' ----- main_Get the Group name
                '
                CS = db.cs_open2("Groups", iGroupID)
                If db.cs_ok(CS) Then
                    group_GetGroupName = genericController.encodeText(db.cs_getField(CS, "Name"))
                End If
                Call db.cs_Close(CS)
            End If
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Function
        '
        '=============================================================================
        ' Add a new group, return its GroupID
        '=============================================================================
        '
        Public Function group_Add(ByVal GroupName As String, Optional ByVal GroupCaption As String = "") As Integer
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("AddGroup")
            '
            'If Not (true) Then Exit Function
            '
            'dim dt as datatable
            Dim CS As Integer
            Dim MethodName As String
            Dim iGroupName As String
            Dim iGroupCaption As String
            '
            MethodName = "main_AddGroup"
            '
            iGroupName = genericController.encodeText(GroupName)
            iGroupCaption = genericController.encodeEmptyText(GroupCaption, iGroupName)
            '
            group_Add = -1
            Dim dt As DataTable
            dt = db.executeSql("SELECT ID FROM ccgroups WHERE NAME=" & db.encodeSQLText(iGroupName))
            If dt.Rows.Count > 0 Then
                group_Add = genericController.EncodeInteger(dt.Rows(0).Item(0))
            Else
                CS = db.cs_insertRecord("Groups", SystemMemberID)
                If db.cs_ok(CS) Then
                    group_Add = genericController.EncodeInteger(db.cs_getField(CS, "ID"))
                    Call db.cs_set(CS, "name", iGroupName)
                    Call db.cs_set(CS, "caption", iGroupCaption)
                    Call db.cs_set(CS, "active", True)
                End If
                Call db.cs_Close(CS)
            End If
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
        End Function

        '
        '=============================================================================
        ' Add a new group, return its GroupID
        '=============================================================================
        '
        Public Sub group_DeleteGroup(ByVal GroupName As String)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("DeleteGroup")
            '
            'If Not (true) Then Exit Sub
            '
            Call db.deleteContentRecords("Groups", "name=" & db.encodeSQLText(GroupName))
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_DeleteGroup")
        End Sub
        '
        '=============================================================================
        ' Add a member to a group
        '=============================================================================
        '
        Public Sub group_AddGroupMember(ByVal GroupName As String, Optional ByVal NewMemberID As Integer = SystemMemberID, Optional ByVal DateExpires As Date = Nothing)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("AddGroupMember")
            '
            'If Not (true) Then Exit Sub
            '
            Dim CS As Integer
            Dim GroupID As Integer
            Dim MethodName As String
            Dim iGroupName As String
            Dim iDateExpires As Date
            '
            MethodName = "main_AddGroupMember"
            '
            iGroupName = genericController.encodeText(GroupName)
            iDateExpires = DateExpires 'encodeMissingDate(DateExpires, Date.MinValue)
            '
            If iGroupName <> "" Then
                GroupID = group_GetGroupID(iGroupName)
                If (GroupID < 1) Then
                    GroupID = group_Add(GroupName, GroupName)
                End If
                If (GroupID < 1) Then
                    Throw (New ApplicationException("main_AddGroupMember could not find or add Group [" & GroupName & "]")) ' handleLegacyError14(MethodName, "")
                Else
                    CS = db.cs_open("Member Rules", "(MemberID=" & db.encodeSQLNumber(NewMemberID) & ")and(GroupID=" & db.encodeSQLNumber(GroupID) & ")", , False)
                    If Not db.cs_ok(CS) Then
                        Call db.cs_Close(CS)
                        CS = db.cs_insertRecord("Member Rules")
                    End If
                    If Not db.cs_ok(CS) Then
                        Throw (New ApplicationException("main_AddGroupMember could not add this member to the Group [" & GroupName & "]")) ' handleLegacyError14(MethodName, "")
                    Else
                        Call db.cs_set(CS, "active", True)
                        Call db.cs_set(CS, "memberid", NewMemberID)
                        Call db.cs_set(CS, "groupid", GroupID)
                        If iDateExpires <> Date.MinValue Then
                            Call db.cs_set(CS, "DateExpires", iDateExpires)
                        Else
                            Call db.cs_set(CS, "DateExpires", "")
                        End If
                    End If
                    Call db.cs_Close(CS)
                End If
            End If
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Sub
        '
        '=============================================================================
        ' Delete a member from a group
        '=============================================================================
        '
        Public Sub group_DeleteGroupMember(ByVal GroupName As String, Optional ByVal NewMemberID As Integer = SystemMemberID)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("DeleteGroupMember")
            '
            'If Not (true) Then Exit Sub
            '
            Dim GroupID As Integer
            Dim MethodName As String
            Dim iGroupName As String
            '
            iGroupName = genericController.encodeText(GroupName)
            '
            MethodName = "main_DeleteGroupMember"
            '
            If iGroupName <> "" Then
                GroupID = group_GetGroupID(iGroupName)
                If (GroupID < 1) Then
                ElseIf (NewMemberID < 1) Then
                    Throw (New ApplicationException("Member ID is invalid")) ' handleLegacyError14(MethodName, "")
                Else
                    Call db.deleteContentRecords("Member Rules", "(MemberID=" & db.encodeSQLNumber(NewMemberID) & ")AND(groupid=" & db.encodeSQLNumber(GroupID) & ")")
                End If
            End If
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Sub
        '
        '=============================================================================
        ' Print the admin developer tools page
        '=============================================================================
        '
        Public Function tools_GetToolsForm() As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetToolsForm")
            '
            '
            Dim Tools As New coreToolsClass(Me)
            tools_GetToolsForm = Tools.GetForm()
            Tools = Nothing
            Exit Function
            '
ErrorTrap:
            Tools = Nothing
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("PrintToolsForm")
        End Function
        '
        '=============================================================================
        '   Return just the copy from a content page
        '=============================================================================
        '
        Public Sub content_SetContentCopy(ByVal CopyName As String, ByVal Content As String)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("SetContentCopy")
            '
            'If Not (true) Then Exit Sub
            '
            Dim CS As Integer
            Dim iCopyName As String
            Dim iContent As String
            'dim buildversion As String
            Const ContentName = "Copy Content"
            '
            '  BuildVersion = app.dataBuildVersion
            If False Then '.3.210" Then
                Throw (New Exception("Contensive database was created with version " & siteProperties.dataBuildVersion & ". main_SetContentCopy requires an builder."))
            Else
                iCopyName = genericController.encodeText(CopyName)
                iContent = genericController.encodeText(Content)
                CS = db.cs_open(ContentName, "name=" & db.encodeSQLText(iCopyName))
                If Not db.cs_ok(CS) Then
                    Call db.cs_Close(CS)
                    CS = db.cs_insertRecord(ContentName)
                End If
                If db.cs_ok(CS) Then
                    Call db.cs_set(CS, "name", iCopyName)
                    Call db.cs_set(CS, "Copy", iContent)
                End If
                Call db.cs_Close(CS)
            End If
            '
            Exit Sub
            '
ErrorTrap:
            '    PageList = Nothing
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetContentCopy")
        End Sub

        '        '
        '        '=============================================================================
        '        '   Print content in a page format
        '        '
        '        '   iRootPageName is the name of the position on the site, not the name of the
        '        '   content.
        '        '
        '        '   PageName is optional only if main_PreloadContentPage has been called, set to ""
        '        '=============================================================================
        '        '
        '        Public Function main_GetContentPage(ByVal RootPageName As String, Optional ByVal ContentName As String = "", Optional ByVal OrderByClause As String = "", Optional ByVal AllowChildPageList As Boolean = True, Optional ByVal AllowReturnLink As Boolean = True, Optional ByVal Bid As Integer = 0) As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetContentPage")
        '            '
        '            'If Not (true) Then Exit Function
        '            '
        '            Dim FieldRows As Integer
        '            Dim quickEditor As String
        '            Dim iRootPageName As String
        '            Dim rootPageId As Integer
        '            Dim pageId As Integer
        '            Dim iContentName As String
        '            Dim iOrderByClause As String
        '            Dim contentPage As String
        '            '
        '            ' ----- Type the input
        '            '
        '            '$$$$$ cache this - somewhere it opens cs with icontentname
        '            iContentName = genericController.encodeEmptyText(ContentName, "Page Content")
        '            iRootPageName = Trim(genericController.encodeText(RootPageName))
        '            If iRootPageName <> "" Then
        '                rootPageId = main_GetRecordID_Internal(iContentName, iRootPageName)
        '            End If
        '            iOrderByClause = genericController.encodeText(OrderByClause)
        '            If (Bid = 0) Then
        '                pageId = docProperties.getInteger("bid")
        '            Else
        '                pageId = Bid
        '            End If
        '            '
        '            ' ----- Test if the page has been preloaded
        '            '
        '            contentPage = pageManager_GetHtmlBody_GetSection_GetContent(pageId, rootPageId, iContentName, iOrderByClause, AllowChildPageList, AllowReturnLink, False, 0, siteProperties.useContentWatchLink, False)
        '            main_GetContentPage = main_GetEditWrapper(iContentName, contentPage)
        '            '
        '            ' ----- Redirect if required
        '            '       ##### to be moved directly into page list routines
        '            '
        '            If pageManager_RedirectLink <> "" Then
        '                '
        '                ' redirect
        '                '
        '                Call web_Redirect2(pageManager_RedirectLink, pageManager_RedirectReason, pageManager_RedirectBecausePageNotFound)
        '            ElseIf (InStr(1, main_GetContentPage, main_fpo_QuickEditing) <> 0) Then
        '                '
        '                ' quick editor
        '                '
        '                FieldRows = genericController.EncodeInteger(properties_user_getText(ContentName & ".copyFilename.PixelHeight", "500"))
        '                If FieldRows < 50 Then
        '                    FieldRows = 50
        '                    Call properties_SetMemberProperty(ContentName & ".copyFilename.PixelHeight", 50)
        '                End If
        '                quickEditor = html_GetFormInputHTML("copyFilename", main_QuickEditCopy)
        '                main_GetContentPage = genericController.vbReplace(main_GetContentPage, main_fpo_QuickEditing, quickEditor)
        '            End If
        '            Exit Function
        '            '
        'ErrorTrap:
        '            '    PageList = Nothing
        '            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetContentPage")
        '        End Function
        '        '
        '        '=============================================================================
        '        '   Print content in a page format
        '        '
        '        '   iRootPageName is the name of the position on the site, not the name of the
        '        '   content.
        '        '
        '        '   PageName is optional only if main_PreloadContentPage has been called, set to ""
        '        '=============================================================================
        '        '
        '        Public Function main_GetContentPageArchive(ByVal RootPageName As String, Optional ByVal ContentName As String = "", Optional ByVal OrderByClause As String = "") As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetContentPageArchive")
        '            '
        '            'If Not (true) Then Exit Function
        '            '
        '            Dim iRootPageName As String
        '            Dim rootPageId As Integer
        '            Dim PageRecordID As Integer
        '            Dim iContentName As String
        '            '
        '            iRootPageName = Trim(genericController.encodeText(RootPageName))
        '            iContentName = genericController.encodeEmptyText(ContentName, "Page Content")
        '            PageRecordID = docProperties.getInteger("bid")
        '            If iRootPageName <> "" Then
        '                rootPageId = main_GetRecordID_Internal(iContentName, iRootPageName)
        '            End If
        '            '
        '            main_GetContentPageArchive = pageManager_GetHtmlBody_GetSection_GetContent(PageRecordID, rootPageId, iContentName, genericController.encodeText(OrderByClause), True, True, True, 0, siteProperties.useContentWatchLink, False)
        '            '
        '            If pageManager_RedirectLink <> "" Then
        '                Call web_Redirect2(pageManager_RedirectLink, "Redirecting due to a main_GetContentPageArchive condition. (" & pageManager_RedirectReason & ")", pageManager_RedirectBecausePageNotFound)
        '            End If
        '            '
        '            main_GetContentPageArchive = main_GetEditWrapper(iContentName & " Archive", main_GetContentPageArchive)
        '            '
        '            Exit Function
        '            '
        'ErrorTrap:
        '            '   PageList = Nothing
        '            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetContentPageArchive")
        '        End Function
        '        '
        '        '=============================================================================
        '        '   Print content in a page format
        '        '
        '        '   RootPageNameLocal is the name of the position on the site, not the name of the
        '        '   content.
        '        '=============================================================================
        '        '
        '        Public Function main_GetContentPageMenu(ByVal RootPageName As String, Optional ByVal ContentName As String = "", Optional ByVal Link As String = "", Optional ByVal RootPageRecordID As Integer = 0, Optional ByVal DepthLimit As Integer = 0, Optional ByVal MenuStyle As String = "", Optional ByVal StyleSheetPrefix As String = "", Optional ByVal MenuImage As String = "") As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetContentPageMenu")
        '            '
        '            'If Not (true) Then Exit Function
        '            '
        '            'Dim PageList As PageListClass
        '            Dim RootPageNameLocal As String
        '            Dim ContentNameLocal As String
        '            Dim PageLink As String
        '            Dim ChildPageRecordID As Integer
        '            Dim MenuStyleLocal As Integer
        '            Dim StyleSheetPrefixLocal As String
        '            Dim IMenuImage As String
        '            Dim UseContentWatchLink As Boolean
        '            '
        '            RootPageNameLocal = Trim(genericController.encodeText(RootPageName))
        '            ContentNameLocal = genericController.encodeEmptyText(ContentName, "Page Content")
        '            PageLink = genericController.encodeEmptyText(Link, "")
        '            MenuStyleLocal = encodeEmptyInteger(MenuStyle, 1)
        '            StyleSheetPrefixLocal = genericController.encodeEmptyText(StyleSheetPrefix, "ccFlyout")
        '            IMenuImage = genericController.encodeEmptyText(MenuImage, "")
        '            UseContentWatchLink = siteProperties.useContentWatchLink
        '            '
        '            main_GetContentPageMenu = main_GetSectionMenu_NameMenu(RootPageNameLocal, ContentNameLocal, PageLink, RootPageRecordID, DepthLimit, MenuStyleLocal, StyleSheetPrefixLocal, IMenuImage, IMenuImage, "", 0, UseContentWatchLink)
        '            '
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            'Set PageList = Nothing
        '            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetContentPageMenu")
        '        End Function
        '        '
        '        '   2.0 compatibility
        '        '
        '        Public Function main_OpenContent(ByVal ContentName As String, Optional ByVal Criteria As String = "", Optional ByVal SortFieldList As String = "", Optional ByVal ActiveOnly As Boolean = True) As Integer
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("OpenContent")
        '            '
        '            'If Not (true) Then Exit Function
        '            main_OpenContent = db.csOpen(genericController.encodeText(ContentName), Criteria, SortFieldList, ActiveOnly)
        '            '
        '            Exit Function
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_OpenContent")
        '        End Function
        '
        '========================================================================
        ' main_Gets the field in the current CSRow according to its definition
        '========================================================================
        '
        Public Function cs_cs_getRecordEditLink(ByVal CSPointer As Integer, Optional ByVal AllowCut As Object = False) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("cs_getRecordEditLink")
            '
            'If Not (true) Then Exit Function
            '
            Dim RecordName As String
            Dim ContentName As String
            Dim RecordID As Integer
            Dim ContentControlID As Integer
            Dim MethodName As String
            Dim iCSPointer As Integer
            '
            iCSPointer = genericController.EncodeInteger(CSPointer)
            '
            MethodName = "main_cs_getRecordEditLink"
            '
            If iCSPointer = -1 Then
                Throw (New ApplicationException("main_cs_getRecordEditLink called with invalid iCSPointer")) ' handleLegacyError14(MethodName, "")
            Else
                If Not db.cs_ok(iCSPointer) Then
                    Throw (New ApplicationException("main_cs_getRecordEditLink called with Not main_CSOK")) ' handleLegacyError14(MethodName, "")
                Else
                    '
                    ' Print an edit link for the records Content (may not be iCSPointer content)
                    '
                    RecordID = (db.cs_getInteger(iCSPointer, "ID"))
                    RecordName = db.cs_getText(iCSPointer, "Name")
                    ContentControlID = (db.cs_getInteger(iCSPointer, "contentcontrolid"))
                    ContentName = metaData.getContentNameByID(ContentControlID)
                    If ContentName <> "" Then
                        cs_cs_getRecordEditLink = main_GetRecordEditLink2(ContentName, RecordID, genericController.EncodeBoolean(AllowCut), RecordName, authContext.isEditing(Me, ContentName))
                    End If
                End If
            End If
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Function
        '
        '
        '
        Public Sub main_ClearStream()
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("ClearStream")
            '
            htmlDoc.docBuffer = ""
            webServer.webServerIO_bufferRedirect = ""
            webServer.webServerIO_bufferResponseHeader = ""
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_ClearStream")
            '
        End Sub
        '        '
        '        '========================================================================
        '        '   Read in a file from the sites virtual file directory given filename
        '        '========================================================================
        '        '
        '        Public Function app.contentFiles.ReadFile(ByVal Filename As String) As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("ReadVirtualFile")
        '            '
        '            'If Not (true) Then Exit Function
        '            '
        '            main_ReadVirtualFile = app.contentFiles.ReadFile(Filename)
        '            '
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_ReadVirtualFile")
        '        End Function
        '            '
        '            '========================================================================
        '            '   Save data to a file in the sites virtual file directory
        '            '========================================================================
        '            '
        '            Public Sub app.publicFiles.SaveFile(ByVal Filename As Object, ByVal FileContent As Object)
        '            On Error GoTo ErrorTrap : ''Dim th as integer : th = profileLogMethodEnter("SaveVirtualFile")
        '            '
        '            'If Not (true) Then Exit Sub
        '            '
        '            Dim MethodName As String
        '            '
        '            MethodName = "main_SaveVirtualFile"
        '            '
        '            Call app.publicFiles.SaveFile(genericController.encodeText(Filename), genericController.encodeText(FileContent))
        '            '
        '            Exit Sub
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Call main_HandleClassErrorAndResume_TrapPatch1(MethodName)
        '        End Sub
        '        '
        '        '========================================================================
        '        ' Delete a file from the virtual director
        '        '========================================================================
        '        '
        '        Public Sub app.publicFiles.DeleteFile(ByVal Filename As Object)
        '            On Error GoTo ErrorTrap : ''Dim th as integer : th = profileLogMethodEnter("DeleteVirtualFile")
        '            '
        '            'If Not (true) Then Exit Sub
        '            Dim MethodName As String
        '            '
        '            MethodName = "main_DeleteVirtualFile"
        '            '
        '        Call app.publicFiles.DeleteFile(genericController.encodeText(Filename))
        '            Exit Sub
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Call main_HandleClassErrorAndResume_TrapPatch1(MethodName)
        '            '
        '        End Sub
        '        '
        '        '========================================================================
        '        ' Delete a file from the virtual director
        '        '========================================================================
        '        '
        '        Public Sub main_CopyVirtualFile(ByVal SourceFilename As String, ByVal DestinationFilename As String)
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("CopyVirtualFile")
        '            '
        '            'If Not (true) Then Exit Sub
        '            '
        '            Dim MethodName As String
        '            '
        '            MethodName = "main_CopyVirtualFilename"
        '            '
        '            Call app.contentFiles.copyFile(genericController.encodeText(SourceFilename), genericController.encodeText(DestinationFilename))
        '            Exit Sub
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
        '            '
        '        End Sub
        '        '
        '        '========================================================================
        '        '   append data to the end of a file in the sites virtual file directory
        '        '========================================================================
        '        '
        '        Public Sub main_AppendVirtualFile(ByVal Filename As Object, ByVal FileContent As Object)
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("AppendVirtualFile")
        '            '
        '            'If Not (true) Then Exit Sub
        '            Dim MethodName As String
        '            '
        '            MethodName = "main_AppendVirtualFile"
        '            '
        '            Call app.publicFiles.appendFile(genericController.encodeText(Filename), genericController.encodeText(FileContent))
        '            Exit Sub
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
        '        End Sub
        '        '
        '        '========================================================================
        '        '   Save data to a file
        '        '========================================================================
        '        '
        '        Public Sub main_SaveFile(ByVal Filename As Object, ByVal FileContent As Object)
        '            On Error GoTo ErrorTrap : ''Dim th as integer : th = profileLogMethodEnter("SaveFile")
        '            '
        '            'If Not (true) Then Exit Sub
        '            Dim MethodName As String
        '            '
        '            MethodName = "main_SaveFile"
        '            '
        '        Call app.publicFiles.SaveFile(genericController.encodeText(Filename), genericController.encodeText(FileContent))
        '            Exit Sub
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Call main_HandleClassErrorAndResume_TrapPatch1(MethodName)
        '            '
        '        End Sub
        '        '
        '        '========================================================================
        '        ' ----- Creates a file folder if it does not exist
        '        '========================================================================
        '        '
        '        Public Sub main_CreateFileFolder(ByVal FolderPath As Object)
        '            On Error GoTo ErrorTrap : ''Dim th as integer : th = profileLogMethodEnter("CreateFileFolder")
        '            '
        '            'If Not (true) Then Exit Sub
        '            Dim MethodName As String
        '            '
        '            MethodName = "main_CreateFileFolder"
        '            '
        '        Call app.publicFiles.createPath(genericController.encodeText(FolderPath))
        '            Exit Sub
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Call main_HandleClassErrorAndResume_TrapPatch1(MethodName)
        '            '
        '        End Sub
        '            '
        '            '========================================================================
        '            '   Deletes a file if it exists
        '            '========================================================================
        '            '
        '        Public Sub mainx_DeleteFile(ByVal Filename As Object)
        '            On Error GoTo ErrorTrap : ''Dim th as integer : th = profileLogMethodEnter("DeleteFile")
        '            '
        '            'If Not (true) Then Exit Sub
        '            Dim MethodName As String
        '            '
        '            MethodName = "main_DeleteFile"
        '            '
        '            Call app.csv_DeleteFile(genericController.encodeText(Filename))
        '            Exit Sub
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Call main_HandleClassErrorAndResume_TrapPatch1(MethodName)
        '            '
        '        End Sub
        '            '
        '            '========================================================================
        '            '   Copy a file
        '            '========================================================================
        '            '
        '        Public Sub main_xcopyFile(ByVal SourcePathFilename As Object, ByVal DestinationPathFilename As Object)
        '            On Error GoTo ErrorTrap : ''Dim th as integer : th = profileLogMethodEnter("copyFile")
        '            '
        '            'If Not (true) Then Exit Sub
        '            Dim MethodName As String
        '            '
        '            MethodName = "main_copyFile"
        '            '
        '            Call app.csv_CopyFile(genericController.encodeText(SourcePathFilename), genericController.encodeText(DestinationPathFilename))
        '            Exit Sub
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Call main_HandleClassErrorAndResume_TrapPatch1(MethodName)
        '            '
        '        End Sub
        '            '
        '            '========================================================================
        '            '   rename a file
        '            '========================================================================
        '            '
        '        Public Sub main_renamxeFile(ByVal SourcePathFilename As Object, ByVal DestinationFilename As Object)
        '            On Error GoTo ErrorTrap : ''Dim th as integer : th = profileLogMethodEnter("renameFile")
        '            '
        '            'If Not (true) Then Exit Sub
        '            Dim MethodName As String
        '            '
        '            MethodName = "main_renameFile"
        '            '
        '            Call app.csv_renameFile(genericController.encodeText(SourcePathFilename), genericController.encodeText(DestinationFilename))
        '            Exit Sub
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Call main_HandleClassErrorAndResume_TrapPatch1(MethodName)
        '            '
        '        End Sub
        '            '
        '            '========================================================================
        '            '   main_Get a list of files in a folder
        '            '========================================================================
        '            '
        '        Public Function main_GetFxileList(ByVal FolderPath As String, Optional ByVal PageSize As Integer = 1000, Optional ByVal PageNumber As Integer = 1) As IO.FileInfo()
        '            On Error GoTo ErrorTrap : ''Dim th as integer : th = profileLogMethodEnter("GetFileList")
        '            '
        '            'If Not (true) Then Exit Function
        '            '
        '            main_GetFileList = app.csv_GetFileList(genericController.encodeText(FolderPath), genericController.EncodeInteger(PageSize), genericController.EncodeInteger(PageNumber))
        '            '
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Call main_HandleClassErrorAndResume_TrapPatch1("main_GetFileList")
        '        End Function
        '        '
        '        '========================================================================
        '        '   main_Get a list of files in a folder
        '        '========================================================================
        '        '
        '        Public Function main_GetFileCount(ByVal FolderPath As Object) As Integer
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetFileCount")
        '            '
        '            'If Not (true) Then Exit Function
        '            '
        '            main_GetFileCount = app.getPublicFileCount(genericController.encodeText(FolderPath))
        '            '
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetFileCount")
        '        End Function
        '
        '========================================================================
        '   main_Get a list of files in a folder
        '========================================================================
        '
        Public Function getFolderNameList(ByVal FolderPath As Object) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetFolderList")
            '
            'If Not (true) Then Exit Function
            '
            getFolderNameList = appRootFiles.getFolderNameList(genericController.encodeText(FolderPath))
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetFolderList")
        End Function
        '            '
        '            '========================================================================
        '            '   main_Get a list of files in a folder in the Virtual Content path
        '            '========================================================================
        '            '
        '        Public Function main_GetVirtxualFileList(ByVal FolderPath As String, Optional ByVal PageSize As Integer = 1000, Optional ByVal PageNumber As Integer = 1) As IO.FileInfo()
        '            On Error GoTo ErrorTrap : ''Dim th as integer : th = profileLogMethodEnter("GetVirtualFileList")
        '            '
        '            'If Not (true) Then Exit Function
        '            '
        '            main_GetVirtualFileList = app.publicFiles.GetFolderFiles(genericController.encodeText(FolderPath))
        '            '
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Call main_HandleClassErrorAndResume_TrapPatch1("main_GetVirtualFileList")
        '        End Function
        '        '
        '        '========================================================================
        '        '   main_Get a list of files in a folder in the Virtual Content path
        '        '========================================================================
        '        '
        '        Public Function main_GetVirtualFileCount(ByVal FolderPath As Object) As Integer
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetVirtualFileCount")
        '            '
        '            'If Not (true) Then Exit Function
        '            '
        '            main_GetVirtualFileCount = app.csv_GetVirtualFileCount(genericController.encodeText(FolderPath))
        '            '
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetVirtualFileCount")
        '        End Function
        '
        '========================================================================
        '   main_Get a list of files in a folder in the Virtual Content path
        '========================================================================
        '
        Public Function main_GetVirtualFolderList(ByVal FolderPath As Object) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetVirtualFolderList")
            '
            'If Not (true) Then Exit Function
            '
            main_GetVirtualFolderList = cdnFiles.getFolderNameList(genericController.encodeText(FolderPath))
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetVirtualFolderList")
        End Function

        '
        '========================================================================
        ' main_DeleteCSRecord
        '========================================================================
        '
        Public Sub DeleteCSRecord(ByVal CSPointer As Integer)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("DeleteCSRecord")
            '
            'If Not (true) Then Exit Sub
            '
            Dim MethodName As String
            '
            MethodName = "main_DeleteCSRecord"
            '
            Call db.cs_deleteRecord(genericController.EncodeInteger(CSPointer))
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Sub
        '
        '========================================================================
        ' main_InsertContentRecordGetID
        '   Inserts a record into a content table.
        '   Returns the ID of the record, 0 if error
        '========================================================================
        '
        Public Function metaData_InsertContentRecordGetID(ByVal ContentName As String) As Integer
            metaData_InsertContentRecordGetID = db.metaData_InsertContentRecordGetID(genericController.encodeText(ContentName), authContext.user.ID)
        End Function
        '
        '=============================================================================
        ' Create a child content from a parent content
        '
        '   If child does not exist, copy everything from the parent
        '   If child already exists, add any missing fields from parent
        '=============================================================================
        '
        Public Sub metaData_CreateContentChild(ByVal ChildContentName As String, ByVal ParentContentName As String)
            Call metaData.createContentChild(genericController.encodeText(ChildContentName), genericController.encodeText(ParentContentName), authContext.user.ID)
        End Sub
        '
        ' ----- alternate name
        '
        Public Function InsertCSContent(ByVal ContentName As String) As Integer
            InsertCSContent = db.cs_insertRecord(genericController.encodeText(ContentName))
        End Function
        '
        '========================================================================
        '   Determine the current persons Language
        '
        '   Return the ID in the Languages content
        '========================================================================
        '
        Public Function web_GetBrowserLanguageID() As Integer
            Dim LanguageID As Integer = 0
            Dim LanguageName As String = ""
            Call web_GetBrowserLanguage(LanguageID, LanguageName)
            web_GetBrowserLanguageID = LanguageID
        End Function
        '
        '========================================================================
        '   Determine the current persons Language
        '
        '   Return the ID in the Languages content
        '========================================================================
        '
        Public Sub web_GetBrowserLanguage(ByRef LanguageID As Integer, ByRef LanguageName As String)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetBrowserLanguage")
            '
            'If Not (true) Then Exit Sub
            '
            Dim MethodName As String
            Dim CS As Integer
            Dim CommaPosition As Integer
            Dim DashPosition As Integer
            Dim AcceptLanguageString As String
            Dim AcceptLanguage As String
            '
            MethodName = "main_GetBrowserLanguage"
            LanguageID = 0
            LanguageName = ""
            '
            ' ----- Determine Language by browser
            '
            AcceptLanguageString = genericController.encodeText(webServer.RequestLanguage) & ","
            CommaPosition = genericController.vbInstr(1, AcceptLanguageString, ",")
            Do While CommaPosition <> 0 And LanguageID = 0
                AcceptLanguage = Trim(Mid(AcceptLanguageString, 1, CommaPosition - 1))
                AcceptLanguageString = Mid(AcceptLanguageString, CommaPosition + 1)
                If Len(AcceptLanguage) > 0 Then
                    DashPosition = genericController.vbInstr(1, AcceptLanguage, "-")
                    If DashPosition > 1 Then
                        AcceptLanguage = Mid(AcceptLanguage, 1, DashPosition - 1)
                    End If
                    DashPosition = genericController.vbInstr(1, AcceptLanguage, ";")
                    If DashPosition > 1 Then
                        AcceptLanguage = Mid(AcceptLanguage, 1, DashPosition - 1)
                    End If
                    If Len(AcceptLanguage) > 0 Then
                        CS = db.cs_open("languages", "HTTP_Accept_LANGUAGE=" & db.encodeSQLText(AcceptLanguage), , , , , , "ID", 1)
                        If db.cs_ok(CS) Then
                            LanguageID = db.cs_getInteger(CS, "ID")
                            LanguageName = db.cs_getText(CS, "Name")
                        End If
                        Call db.cs_Close(CS)
                    End If
                End If
                CommaPosition = genericController.vbInstr(1, AcceptLanguageString, ",")
            Loop
            '
            If LanguageID = 0 Then
                '
                ' ----- no matching browser language, use site default
                '
                CS = db.cs_open("languages", "name=" & db.encodeSQLText(siteProperties.language), , , , , , "ID", 1)
                If db.cs_ok(CS) Then
                    LanguageID = db.cs_getInteger(CS, "ID")
                    LanguageName = db.cs_getText(CS, "Name")
                End If
                Call db.cs_Close(CS)
            End If
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Sub
        '
        '========================================================================
        ' main_GetRecordEditLink( iContentName, iRecordID )
        '
        '   iContentName The content for this link
        '   iRecordID    The ID of the record in the Table
        '========================================================================
        '
        Public Function main_GetRecordEditLink(ByVal ContentName As String, ByVal RecordID As Integer, Optional ByVal AllowCut As Boolean = False) As String
            main_GetRecordEditLink = main_GetRecordEditLink2(ContentName, RecordID, genericController.EncodeBoolean(AllowCut), "", authContext.isEditing(Me, ContentName))
        End Function
        '
        '========================================================================
        ' main_GetRecordEditLink2( iContentName, iRecordID, AllowCut, RecordName )
        '
        '   ContentName The content for this link
        '   RecordID    The ID of the record in the Table
        '   AllowCut
        '   RecordName
        '   IsEditing
        '========================================================================
        '
        Public Function main_GetRecordEditLink2(ByVal ContentName As String, ByVal RecordID As Integer, ByVal AllowCut As Boolean, ByVal RecordName As String, ByVal IsEditing As Boolean) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetRecordEditLink2")
            '
            'If Not (true) Then Exit Function
            '
            Dim CS As Integer
            Dim SQL As String
            Dim ContentID As Integer
            Dim Link As String
            Dim MethodName As String
            Dim iContentName As String
            Dim iRecordID As Integer
            Dim RootEntryName As String
            Dim ClipBoard As String
            Dim WorkingLink As String
            Dim iAllowCut As Boolean
            Dim Icon As String
            Dim ContentCaption As String
            '
            iContentName = genericController.encodeText(ContentName)
            iRecordID = genericController.EncodeInteger(RecordID)
            iAllowCut = genericController.EncodeBoolean(AllowCut)
            ContentCaption = htmlDoc.html_EncodeHTML(iContentName)
            If genericController.vbLCase(ContentCaption) = "aggregate functions" Then
                ContentCaption = "Add-on"
            End If
            If genericController.vbLCase(ContentCaption) = "aggregate function objects" Then
                ContentCaption = "Add-on"
            End If
            ContentCaption = ContentCaption & " record"
            If RecordName <> "" Then
                ContentCaption = ContentCaption & ", named '" & RecordName & "'"
            End If
            '
            MethodName = "main_GetRecordEditLink2"
            '
            main_GetRecordEditLink2 = ""
            If (iContentName = "") Then
                Throw (New ApplicationException("ContentName [" & ContentName & "] is invalid")) ' handleLegacyError14(MethodName, "")
            Else
                If (iRecordID < 1) Then
                    Throw (New ApplicationException("RecordID [" & RecordID & "] is invalid")) ' handleLegacyError14(MethodName, "")
                Else
                    If IsEditing Then
                        '
                        ' Edit link, main_Get the CID
                        '
                        ContentID = metaData.getContentId(iContentName)
                        '
                        main_GetRecordEditLink2 = main_GetRecordEditLink2 _
                            & "<a" _
                            & " class=""ccRecordEditLink"" " _
                            & " TabIndex=-1" _
                            & " href=""" & htmlDoc.html_EncodeHTML(siteProperties.adminURL & "?cid=" & ContentID & "&id=" & iRecordID & "&af=4&aa=2&ad=1") & """"
                        If Not htmlDoc.main_ReturnAfterEdit Then
                            main_GetRecordEditLink2 = main_GetRecordEditLink2 & " target=""_blank"""
                        End If
                        main_GetRecordEditLink2 = main_GetRecordEditLink2 _
                            & "><img" _
                            & " src=""/ccLib/images/IconContentEdit.gif""" _
                            & " border=""0""" _
                            & " alt=""Edit this " & htmlDoc.html_EncodeHTML(ContentCaption) & """" _
                            & " title=""Edit this " & htmlDoc.html_EncodeHTML(ContentCaption) & """" _
                            & " align=""absmiddle""" _
                            & "></a>"
                        '
                        ' Cut Link if enabled
                        '
                        If iAllowCut Then
                            WorkingLink = genericController.modifyLinkQuery(webServer.webServerIO_requestPage & "?" & htmlDoc.refreshQueryString, RequestNameCut, genericController.encodeText(ContentID) & "." & genericController.encodeText(RecordID), True)
                            main_GetRecordEditLink2 = "" _
                                & main_GetRecordEditLink2 _
                                & "<a class=""ccRecordCutLink"" TabIndex=""-1"" href=""" & htmlDoc.html_EncodeHTML(WorkingLink) & """><img src=""/ccLib/images/Contentcut.gif"" border=""0"" alt=""Cut this " & ContentCaption & " to clipboard"" title=""Cut this " & ContentCaption & " to clipboard"" align=""absmiddle""></a>"
                        End If
                        '
                        ' Help link if enabled
                        '
                        Dim helpLink As String
                        helpLink = ""
                        'helpLink = main_GetHelpLink(5, "Editing " & ContentCaption, "Turn on Edit icons by checking 'Edit' in the tools panel, and click apply.<br><br><img src=""/ccLib/images/IconContentEdit.gif"" style=""vertical-align:middle""> Edit-Content icon<br><br>Edit-Content icons appear in your content. Click them to edit your content.")
                        main_GetRecordEditLink2 = "" _
                            & main_GetRecordEditLink2 _
                            & helpLink
                        '
                        main_GetRecordEditLink2 = "<span class=""ccRecordLinkCon"" style=""white-space:nowrap;"">" & main_GetRecordEditLink2 & "</span>"
                        ''
                        'main_GetRecordEditLink2 = "" _
                        '    & cr & "<div style=""position:absolute;"">" _
                        '    & genericController.kmaIndent(main_GetRecordEditLink2) _
                        '    & cr & "</div>"
                        '
                        'main_GetRecordEditLink2 = "" _
                        '    & cr & "<div style=""position:relative;display:inline;"">" _
                        '    & genericController.kmaIndent(main_GetRecordEditLink2) _
                        '    & cr & "</div>"
                    End If

                End If
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Function
        '
        '========================================================================
        ' Print an add link for the current ContentSet
        '   iCSPointer is the content set to be added to
        '   PresetNameValueList is a name=value pair to force in the added record
        '========================================================================
        '
        Public Function main_cs_getRecordAddLink(ByVal CSPointer As Integer, Optional ByVal PresetNameValueList As String = "", Optional ByVal AllowPaste As Boolean = False) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("cs_getRecordAddLink")
            '
            'If Not (true) Then Exit Function
            '
            Dim ContentName As String
            Dim iPresetNameValueList As String
            Dim MethodName As String
            Dim iCSPointer As Integer
            '
            iCSPointer = genericController.EncodeInteger(CSPointer)
            iPresetNameValueList = genericController.encodeEmptyText(PresetNameValueList, "")
            '
            MethodName = "main_cs_getRecordAddLink"
            '
            If iCSPointer < 0 Then
                Throw (New ApplicationException("invalid ContentSet pointer [" & iCSPointer & "]")) ' handleLegacyError14(MethodName, "main_cs_getRecordAddLink was called with ")
            Else
                '
                ' Print an add tag to the iCSPointers Content
                '
                ContentName = db.cs_getContentName(iCSPointer)
                If ContentName = "" Then
                    Throw (New ApplicationException("main_cs_getRecordAddLink was called with a ContentSet that was created with an SQL statement. The function requires a ContentSet opened with an OpenCSContent.")) ' handleLegacyError14(MethodName, "")
                Else
                    main_cs_getRecordAddLink = main_GetRecordAddLink(ContentName, iPresetNameValueList, AllowPaste)
                End If
            End If
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Function
        '
        '========================================================================
        ' main_GetRecordAddLink( iContentName, iPresetNameValueList )
        '
        '   Returns a string of add tags for the Content Definition included, and all
        '   child contents of that area.
        '
        '   iContentName The content for this link
        '   iPresetNameValueList The sql equivalent used to select the record.
        '           translates to name0=value0,name1=value1.. pairs separated by ,
        '
        '   LowestRootMenu - The Menu in the flyout structure that is the furthest down
        '   in the chain that the user has content access to. This is so a content manager
        '   does not have to navigate deep into a structure to main_Get to content he can
        '   edit.
        '   Basically, the entire menu is created down from the MenuName, and populated
        '   with all the entiries this user has access to. The LowestRequiredMenuName is
        '   is returned from the _branch routine, and that is to root on-which the
        '   main_GetMenu uses
        '========================================================================
        '
        Public Function main_GetRecordAddLink(ByVal ContentName As String, ByVal PresetNameValueList As String, Optional ByVal AllowPaste As Boolean = False) As String
            main_GetRecordAddLink = main_GetRecordAddLink2(genericController.encodeText(ContentName), genericController.encodeText(PresetNameValueList), AllowPaste, authContext.isEditing(Me, ContentName))
        End Function
        '
        '========================================================================
        ' main_GetRecordAddLink2
        '
        '   Returns a string of add tags for the Content Definition included, and all
        '   child contents of that area.
        '
        '   iContentName The content for this link
        '   iPresetNameValueList The sql equivalent used to select the record.
        '           translates to name0=value0,name1=value1.. pairs separated by ,
        '
        '   LowestRootMenu - The Menu in the flyout structure that is the furthest down
        '   in the chain that the user has content access to. This is so a content manager
        '   does not have to navigate deep into a structure to main_Get to content he can
        '   edit.
        '   Basically, the entire menu is created down from the MenuName, and populated
        '   with all the entiries this user has access to. The LowestRequiredMenuName is
        '   is returned from the _branch routine, and that is to root on-which the
        '   main_GetMenu uses
        '========================================================================
        '
        Public Function main_GetRecordAddLink2(ByVal ContentName As String, ByVal PresetNameValueList As String, ByVal AllowPaste As Boolean, ByVal IsEditing As Boolean) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetRecordAddLink")
            '
            'If Not (true) Then Exit Function
            '
            Dim ParentID As Integer
            Dim BufferString As String
            Dim MethodName As String
            Dim iContentName As String
            Dim iContentID As Integer
            Dim iPresetNameValueList As String
            Dim MenuName As String
            Dim MenuHasBranches As Boolean
            Dim LowestRequiredMenuName As String
            Dim ClipBoard As String
            Dim PasteLink As String
            Dim Position As Integer
            Dim ClipBoardArray As String()
            Dim ClipboardContentID As Integer
            Dim ClipChildRecordID As Integer
            Dim iAllowPaste As Boolean
            Dim useFlyout As Boolean
            Dim csChildContent As Integer
            Dim Link As String
            '
            MethodName = "main_GetRecordAddLink"
            '
            main_GetRecordAddLink2 = ""
            If IsEditing Then
                iContentName = genericController.encodeText(ContentName)
                iPresetNameValueList = genericController.encodeText(PresetNameValueList)
                iPresetNameValueList = genericController.vbReplace(iPresetNameValueList, "&", ",")
                iAllowPaste = genericController.EncodeBoolean(AllowPaste)

                If iContentName = "" Then
                    Throw (New ApplicationException("Method called with blank ContentName")) ' handleLegacyError14(MethodName, "")
                Else
                    iContentID = metaData.getContentId(iContentName)
                    csChildContent = db.cs_open("Content", "ParentID=" & iContentID, , , , , , "id")
                    useFlyout = db.cs_ok(csChildContent)
                    Call db.cs_Close(csChildContent)
                    '
                    If Not useFlyout Then
                        Link = siteProperties.adminURL & "?cid=" & iContentID & "&af=4&aa=2&ad=1"
                        If PresetNameValueList <> "" Then
                            Link = Link & "&wc=" & htmlDoc.main_EncodeRequestVariable(PresetNameValueList)
                        End If
                        main_GetRecordAddLink2 = main_GetRecordAddLink2 _
                            & "<a" _
                            & " TabIndex=-1" _
                            & " href=""" & htmlDoc.html_EncodeHTML(Link) & """"
                        If Not htmlDoc.main_ReturnAfterEdit Then
                            main_GetRecordAddLink2 = main_GetRecordAddLink2 & " target=""_blank"""
                        End If
                        main_GetRecordAddLink2 = main_GetRecordAddLink2 _
                            & "><img" _
                            & " src=""/ccLib/images/IconContentAdd.gif""" _
                            & " border=""0""" _
                            & " alt=""Add record""" _
                            & " title=""Add record""" _
                            & " align=""absmiddle""" _
                            & "></a>"
                    Else
                        '
                        MenuName = genericController.GetRandomInteger().ToString
                        Call htmlDoc.menu_AddEntry(MenuName, , "/ccLib/images/IconContentAdd.gif", , , , "stylesheet", "stylesheethover")
                        LowestRequiredMenuName = main_GetRecordAddLink_AddMenuEntry(iContentName, iPresetNameValueList, "", MenuName, MenuName)
                    End If
                    '
                    ' Add in the paste entry, if needed
                    '
                    If iAllowPaste Then
                        ClipBoard = visitProperty.getText("Clipboard", "")
                        If ClipBoard <> "" Then
                            Position = genericController.vbInstr(1, ClipBoard, ".")
                            If Position <> 0 Then
                                ClipBoardArray = Split(ClipBoard, ".")
                                If UBound(ClipBoardArray) > 0 Then
                                    ClipboardContentID = genericController.EncodeInteger(ClipBoardArray(0))
                                    ClipChildRecordID = genericController.EncodeInteger(ClipBoardArray(1))
                                    'iContentID = main_GetContentID(iContentName)
                                    If metaData.isWithinContent(ClipboardContentID, iContentID) Then
                                        If genericController.vbInstr(1, iPresetNameValueList, "PARENTID=", vbTextCompare) <> 0 Then
                                            '
                                            ' must test for main_IsChildRecord
                                            '
                                            BufferString = iPresetNameValueList
                                            BufferString = genericController.vbReplace(BufferString, "(", "")
                                            BufferString = genericController.vbReplace(BufferString, ")", "")
                                            BufferString = genericController.vbReplace(BufferString, ",", "&")
                                            ParentID = genericController.EncodeInteger(main_GetNameValue_Internal(BufferString, "Parentid"))
                                        End If


                                        If (ParentID <> 0) And (Not pages.main_IsChildRecord(iContentName, ParentID, ClipChildRecordID)) Then
                                            '
                                            ' Can not paste as child of itself
                                            '
                                            PasteLink = webServer.webServerIO_requestPage & "?" & htmlDoc.refreshQueryString
                                            PasteLink = genericController.modifyLinkQuery(PasteLink, RequestNamePaste, "1", True)
                                            PasteLink = genericController.modifyLinkQuery(PasteLink, RequestNamePasteParentContentID, CStr(iContentID), True)
                                            PasteLink = genericController.modifyLinkQuery(PasteLink, RequestNamePasteParentRecordID, CStr(ParentID), True)
                                            PasteLink = genericController.modifyLinkQuery(PasteLink, RequestNamePasteFieldList, iPresetNameValueList, True)
                                            main_GetRecordAddLink2 = main_GetRecordAddLink2 _
                                                & "<a class=""ccRecordCutLink"" TabIndex=""-1"" href=""" & htmlDoc.html_EncodeHTML(PasteLink) & """><img src=""/ccLib/images/ContentPaste.gif"" border=""0"" alt=""Paste record in clipboard here"" title=""Paste record in clipboard here"" align=""absmiddle""></a>"
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                    '
                    ' Add in the available flyout menu entries
                    '
                    If LowestRequiredMenuName <> "" Then
                        main_GetRecordAddLink2 = main_GetRecordAddLink2 & menuFlyout.getMenu(LowestRequiredMenuName, 0)
                        main_GetRecordAddLink2 = genericController.vbReplace(main_GetRecordAddLink2, "class=""ccFlyoutButton"" ", "", 1, 99, vbTextCompare)
                        If PasteLink <> "" Then
                            main_GetRecordAddLink2 = main_GetRecordAddLink2 & "<a TabIndex=-1 href=""" & htmlDoc.html_EncodeHTML(PasteLink) & """><img src=""/ccLib/images/ContentPaste.gif"" border=""0"" alt=""Paste content from clipboard"" align=""absmiddle""></a>"
                        End If
                    End If
                    '
                    ' Help link if enabled
                    '
                    Dim helpLink As String
                    helpLink = ""
                    'helpLink = main_GetHelpLink(6, "Adding " & iContentName, "Turn on Edit icons by checking 'Edit' in the tools panel, and click apply.<br><br><img src=""/ccLib/images/IconContentAdd.gif"" " & IconWidthHeight & " style=""vertical-align:middle""> Add-Content icon<br><br>Add-Content icons appear in your content. Click them to add content.")
                    main_GetRecordAddLink2 = main_GetRecordAddLink2 & helpLink                '
                    If main_GetRecordAddLink2 <> "" Then
                        main_GetRecordAddLink2 = "" _
                            & vbCrLf & vbTab & "<div style=""display:inline;"">" _
                            & genericController.kmaIndent(main_GetRecordAddLink2) _
                            & vbCrLf & vbTab & "</div>"
                    End If
                    '
                    ' ----- Add the flyout panels to the content to return
                    '       This must be here so if the call is made after main_ClosePage, the panels will still deliver
                    '
                    If LowestRequiredMenuName <> "" Then
                        main_GetRecordAddLink2 = main_GetRecordAddLink2 & htmlDoc.menu_GetClose()
                        If genericController.vbInstr(1, main_GetRecordAddLink2, "IconContentAdd.gif", vbTextCompare) <> 0 Then
                            main_GetRecordAddLink2 = genericController.vbReplace(main_GetRecordAddLink2, "IconContentAdd.gif"" ", "IconContentAdd.gif"" align=""absmiddle"" ")
                        End If
                    End If
                    If htmlDoc.main_ReturnAfterEdit Then
                        main_GetRecordAddLink2 = genericController.vbReplace(main_GetRecordAddLink2, "target=", "xtarget=", 1, 99, vbTextCompare)
                    End If
                    'End If
                End If
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Function
        '
        '========================================================================
        ' main_GetRecordAddLink_AddMenuEntry( ContentName, PresetNameValueList, ContentNameList, MenuName )
        '
        '   adds an add entry for the content name, and all the child content
        '   returns the MenuName of the lowest branch that has valid
        '   menu entries.
        '
        '   ContentName The content for this link
        '   PresetNameValueList The sql equivalent used to select the record.
        '           translates to (name0=value0)&(name1=value1).. pairs separated by &
        '   ContentNameList is a comma separated list of names of the content included so far
        '   MenuName is the name of the root branch, for flyout menu
        '
        '   IsMember(), main_IsAuthenticated() And Member_AllowLinkAuthoring must already be checked
        '========================================================================
        '
        Private Function main_GetRecordAddLink_AddMenuEntry(ByVal ContentName As String, ByVal PresetNameValueList As String, ByVal ContentNameList As String, ByVal MenuName As String, ByVal ParentMenuName As String) As String
            Dim result As String = ""
            Dim Copy As String
            Dim CS As Integer
            Dim SQL As String
            Dim csChildContent As Integer
            Dim ContentID As Integer
            Dim Link As String
            Dim MyContentNameList As String
            Dim ButtonCaption As String
            Dim ContentRecordFound As Boolean
            Dim ContentAllowAdd As Boolean
            Dim GroupRulesAllowAdd As Boolean
            Dim MemberRulesDateExpires As Date
            Dim MemberRulesAllow As Boolean
            Dim ChildMenuButtonCount As Integer
            Dim ChildMenuName As String
            Dim ChildContentName As String
            '
            Link = ""
            MyContentNameList = ContentNameList
            If (ContentName = "") Then
                Throw (New ApplicationException("main_GetRecordAddLink, ContentName is empty")) ' handleLegacyError14(MethodName, "")
            Else
                If (InStr(1, MyContentNameList, "," & genericController.vbUCase(ContentName) & ",") >= 0) Then
                    Throw (New ApplicationException("result , Content Child [" & ContentName & "] is one of its own parents")) ' handleLegacyError14(MethodName, "")
                Else
                    MyContentNameList = MyContentNameList & "," & genericController.vbUCase(ContentName) & ","
                    '
                    ' ----- Select the Content Record for the Menu Entry selected
                    '
                    ContentRecordFound = False
                    If authContext.isAuthenticatedAdmin(Me) Then
                        '
                        ' ----- admin member, they have access, main_Get ContentID and set markers true
                        '
                        SQL = "SELECT ID as ContentID, AllowAdd as ContentAllowAdd, 1 as GroupRulesAllowAdd, null as MemberRulesDateExpires" _
                            & " FROM ccContent" _
                            & " WHERE (" _
                            & " (ccContent.Name=" & db.encodeSQLText(ContentName) & ")" _
                            & " AND(ccContent.active<>0)" _
                            & " );"
                        CS = db.cs_openSql(SQL)
                        If db.cs_ok(CS) Then
                            '
                            ' Entry was found
                            '
                            ContentRecordFound = True
                            ContentID = db.cs_getInteger(CS, "ContentID")
                            ContentAllowAdd = db.cs_getBoolean(CS, "ContentAllowAdd")
                            GroupRulesAllowAdd = True
                            MemberRulesDateExpires = Date.MinValue
                            MemberRulesAllow = True
                        End If
                        Call db.cs_Close(CS)
                    Else
                        '
                        ' non-admin member, first check if they have access and main_Get true markers
                        '
                        SQL = "SELECT ccContent.ID as ContentID, ccContent.AllowAdd as ContentAllowAdd, ccGroupRules.AllowAdd as GroupRulesAllowAdd, ccMemberRules.DateExpires as MemberRulesDateExpires" _
                            & " FROM (((ccContent" _
                                & " LEFT JOIN ccGroupRules ON ccGroupRules.ContentID=ccContent.ID)" _
                                & " LEFT JOIN ccgroups ON ccGroupRules.GroupID=ccgroups.ID)" _
                                & " LEFT JOIN ccMemberRules ON ccgroups.ID=ccMemberRules.GroupID)" _
                                & " LEFT JOIN ccMembers ON ccMemberRules.MemberID=ccMembers.ID" _
                            & " WHERE (" _
                            & " (ccContent.Name=" & db.encodeSQLText(ContentName) & ")" _
                            & " AND(ccContent.active<>0)" _
                            & " AND(ccGroupRules.active<>0)" _
                            & " AND(ccMemberRules.active<>0)" _
                            & " AND((ccMemberRules.DateExpires is Null)or(ccMemberRules.DateExpires>" & db.encodeSQLDate(app_startTime) & "))" _
                            & " AND(ccgroups.active<>0)" _
                            & " AND(ccMembers.active<>0)" _
                            & " AND(ccMembers.ID=" & authContext.user.ID & ")" _
                            & " );"
                        CS = db.cs_openSql(SQL)
                        If db.cs_ok(CS) Then
                            '
                            ' ----- Entry was found, member has some kind of access
                            '
                            ContentRecordFound = True
                            ContentID = db.cs_getInteger(CS, "ContentID")
                            ContentAllowAdd = db.cs_getBoolean(CS, "ContentAllowAdd")
                            GroupRulesAllowAdd = db.cs_getBoolean(CS, "GroupRulesAllowAdd")
                            MemberRulesDateExpires = db.cs_getDate(CS, "MemberRulesDateExpires")
                            MemberRulesAllow = False
                            If MemberRulesDateExpires = Date.MinValue Then
                                MemberRulesAllow = True
                            ElseIf (MemberRulesDateExpires > app_startTime) Then
                                MemberRulesAllow = True
                            End If
                        Else
                            '
                            ' ----- No entry found, this member does not have access, just main_Get ContentID
                            '
                            ContentRecordFound = True
                            ContentID = metaData.getContentId(ContentName)
                            ContentAllowAdd = False
                            GroupRulesAllowAdd = False
                            MemberRulesAllow = False
                        End If
                        Call db.cs_Close(CS)
                    End If
                    If ContentRecordFound Then
                        '
                        ' Add the Menu Entry* to the current menu (MenuName)
                        '
                        Link = ""
                        ButtonCaption = ContentName
                        result = MenuName
                        If ContentAllowAdd And GroupRulesAllowAdd And MemberRulesAllow Then
                            Link = siteProperties.adminURL & "?cid=" & ContentID & "&af=4&aa=2&ad=1"
                            If PresetNameValueList <> "" Then
                                Dim NameValueList As String
                                NameValueList = PresetNameValueList
                                Link = Link & "&wc=" & htmlDoc.main_EncodeRequestVariable(PresetNameValueList)
                            End If
                        End If
                        Call htmlDoc.menu_AddEntry(MenuName & ":" & ContentName, ParentMenuName, , , Link, ButtonCaption, "", "", True)
                        '
                        ' Create child submenu if Child Entries found
                        '
                        csChildContent = db.cs_open("Content", "ParentID=" & ContentID, , , , , , "name")
                        If Not db.cs_ok(csChildContent) Then
                            '
                            ' No child menu
                            '
                        Else
                            '
                            ' Add the child menu
                            '
                            ChildMenuName = MenuName & ":" & ContentName
                            ChildMenuButtonCount = 0
                            '
                            ' ----- Create the ChildPanel with all Children found
                            '
                            Do While db.cs_ok(csChildContent)
                                ChildContentName = db.cs_getText(csChildContent, "name")
                                Copy = main_GetRecordAddLink_AddMenuEntry(ChildContentName, PresetNameValueList, MyContentNameList, MenuName, ParentMenuName)
                                If Copy <> "" Then
                                    ChildMenuButtonCount = ChildMenuButtonCount + 1
                                End If
                                If (result = "") And (Copy <> "") Then
                                    result = Copy
                                End If
                                db.cs_goNext(csChildContent)
                            Loop
                        End If
                    End If
                End If
                Call db.cs_Close(csChildContent)
            End If
            Return result
        End Function
        ''
        ''========================================================================
        '' Depricated - Use main_GetRecordEditLink and main_GetRecordAddLink
        ''========================================================================
        ''
        'Public Function main_GetRecordEditLinkByContent(ByVal ContentID As Integer, ByVal RecordIDVariant As Object, ByVal Criteria As String) As String
        '    Dim ContentName As String = metaData.getContentNameByID(ContentID)
        '    If ContentName <> "" Then
        '        If Not genericController.IsNull(RecordIDVariant) Then
        '            Return main_GetRecordEditLink2(ContentName, genericController.EncodeInteger(RecordIDVariant), False, "", authContext.isEditing(Me, ContentName))
        '        Else
        '            Return main_GetRecordAddLink(ContentName, Criteria)
        '        End If
        '    End If
        '    Return ""
        'End Function



    End Class
End Namespace
