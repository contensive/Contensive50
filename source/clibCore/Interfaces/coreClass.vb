
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
        Public Property continueProcessing As Boolean = False                                   ' when false, routines should not add to the output and immediately exit
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
            authContext = New authContextModel(Me)
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
            authContext = New authContextModel(Me)
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
            authContext = New authContextModel(Me)
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
            authContext = New authContextModel(Me)
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
            authContext = New authContextModel(Me)
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
                                                continueProcessing = False '--- should be disposed by caller --- Call dispose
                                                Return doc.docBuffer
                                            ElseIf Not db.cs_getBoolean(cs, "active") Then
                                                '
                                                ' inactive record, throw error
                                                '
                                                Call db.cs_Close(cs)
                                                handleExceptionAndContinue(New ApplicationException("Add-on call from [" & refHost & "] was blocked because this domain is not active in the Aggregate Access Content. To allow this domain access, mark the record active.")) ' handleLegacyError12("Init", "")
                                                continueProcessing = False '--- should be disposed by caller --- Call dispose
                                                Return doc.docBuffer
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
                                returnResult = addon.execute(0, addonRoute, Option_String, CPUtilsBaseClass.addonContext.ContextRemoteMethodJson, HostContentName, hostRecordId, "", "0", False, 0, "", AddonStatusOK, Nothing, "", Nothing, "", authContext.user.id, authContext.isAuthenticated)
                            End If
                            '
                            ' deliver styles, javascript and other head tags as javascript appends
                            '
                            webServer.webServerIO_BlockClosePageCopyright = True
                            html_BlockClosePageLink = True
                            If (webServer.webServerIO_OutStreamDevice = docController.htmlDoc_OutStreamJavaScript) Then
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
                                Call html.writeAltBuffer(returnResult)
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
                                                    & vbCrLf & vbTab & "<div class=""radioCon"">" & html.html_GetFormInputRadioBox(radioGroupName, genericController.encodeText(addonId), CStr(currentEditorAddonId)) & "&nbsp;Use " & genericController.encodeText(rsDr("addonName")) & "</div>" _
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
                                        & vbCrLf & vbTab & "<div class=""radioCon"">" & html.html_GetFormInputRadioBox("setEditorPreference" & fieldId, "0", "0") & "&nbsp;Use Default Editor" & addonDefaultEditorName & "</div>" _
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
                                        returnResult = html.main_encodeHTML(returnResult)
                                        Call html.writeAltBuffer(returnResult)
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
                                        returnResult = html.main_encodeHTML(returnResult)
                                        Call html.writeAltBuffer(returnResult)
                                    Case AjaxData
                                        '
                                        ' 7/7/2009 - Moved from HardCodedPages - Run remote query from cj.remote object call, and return results html encoded in a <result></result> block
                                        ' 20050427 - not used
                                        Call html.writeAltBuffer(executeRoute_ProcessAjaxData())
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
                                returnResult = returnResult & html.getHtmlDoc_beforeEndOfBodyHtml(False, False, True, False)
                                Call html.writeAltBuffer(returnResult)
                                continueProcessing = False '--- should be disposed by caller --- Call dispose
                                Return doc.docBuffer
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
                                    Call db.cs_set(CSLog, "VisitId", authContext.visit.id)
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
                                        Call db.cs_set(CSLog, "VisitId", authContext.visit.id)
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
                                Call html.main_SetMetaContent(0, 0)
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
                                                Call appRootFiles.saveFile("templates\Public" & StyleSN & ".css", doc.pageManager_GetStyleSheet)
                                                Call appRootFiles.saveFile("templates\Admin" & StyleSN & ".css", doc.pageManager_GetStyleSheetDefault)
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
                                            cs = db.csOpenRecord(cnAddons, addonId)
                                            If db.cs_ok(cs) Then
                                                Call db.cs_set(cs, "CustomStylesFilename", docProperties.getText("CustomStyles"))
                                            End If
                                            Call db.cs_Close(cs)
                                            '
                                            ' Clear Caches
                                            '
                                            'Call pages.cache_pageContent_clear()
                                            'Call pages.pageManager_cache_pageTemplate_clear()
                                            'Call pages.pageManager_cache_siteSection_clear()
                                            'Call cache.invalidateObjectList("")
                                            If contentName <> "" Then
                                                Call cache.invalidateContent(contentName)
                                                tableName = metaData.getContentTablename(contentName)
                                                If genericController.vbLCase(tableName) = "cctemplates" Then
                                                    'Call cache.setObject(pagesController.cache_pageTemplate_cacheName, nothingObject)
                                                    'Call pages.pageManager_cache_pageTemplate_load()
                                                End If
                                            End If
                                        End If
                                    Case FormTypeAddonSettingsEditor
                                        '
                                        '
                                        '
                                        Call html.pageManager_ProcessAddonSettingsEditor()
                                    Case FormTypeHelpBubbleEditor
                                        '
                                        '
                                        '
                                        Call html.main_ProcessHelpBubbleEditor()
                                    Case FormTypeJoin
                                        '
                                        '
                                        '
                                        Call html.processFormJoin()
                                    Case FormTypeSendPassword
                                        '
                                        '
                                        '
                                        Call html.processFormSendPassword()
                                    Case FormTypeLogin, "l09H58a195"
                                        '
                                        '
                                        '
                                        Call html.processFormLoginDefault()
                                    Case FormTypeToolsPanel
                                        '
                                        ' ----- Administrator Tools Panel
                                        '
                                        Call html.pageManager_ProcessFormToolsPanel()
                                    Case FormTypePageAuthoring
                                        '
                                        ' ----- Page Authoring Tools Panel
                                        '
                                        Call doc.pageManager_ProcessFormQuickEditing()
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
                                continueProcessing = False '--- should be disposed by caller --- Call dispose
                                Return doc.docBuffer
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
                                returnResult = addon.execute(defaultAddonId, "", "", CPUtilsBaseClass.addonContext.ContextPage, "", 0, "", "", False, 0, "", addonStatusOk, Nothing, "", Nothing, "", authContext.user.id, authContext.visit.VisitAuthenticated)
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
                    CS = db.cs_open("Remote Queries", "((VisitId=" & authContext.visit.id & ")and(remotekey=" & db.encodeSQLText(RemoteKey) & "))")
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
                        Copy = html.html_EncodeHTML(Copy)
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
                            & "<p><a href=""?" & doc.refreshQueryString & """>Return to the Site.</a></p>" _
                            & "</div>"
                            Call html.writeAltBuffer(Copy)
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
                '        PageID = docProperties.getInteger(rnPageId)
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
                        Call doc.addRefreshQueryString(RequestNameHardCodedPage, HardCodedPageResourceLibrary)
                        EditorObjectName = docProperties.getText("EditorObjectName")
                        LinkObjectName = docProperties.getText("LinkObjectName")
                        If EditorObjectName <> "" Then
                            '
                            ' Open a page compatible with a dialog
                            '
                            Call doc.addRefreshQueryString("EditorObjectName", EditorObjectName)
                            Call html.main_AddHeadScriptLink("/ccLib/ClientSide/dialogs.js", "Resource Library")
                            'Call AddHeadScript("<script type=""text/javascript"" src=""/ccLib/ClientSide/dialogs.js""></script>")
                            Call html.main_SetMetaContent(0, 0)
                            Call html.main_AddOnLoadJavascript2("document.body.style.overflow='scroll';", "Resource Library")
                            Copy = html.main_GetResourceLibrary2("", True, EditorObjectName, LinkObjectName, True)
                            'Call AppendLog("call main_getEndOfBody, from main_init_printhardcodedpage2b")
                            Copy = "" _
                            & siteProperties.docTypeDeclaration() _
                            & "<html>" _
                            & cr & "<head>" _
                            & genericController.htmlIndent(html.getHtmlDocHead(False)) _
                            & cr & "</head>" _
                            & cr & "<body class=""ccBodyAdmin ccCon"" style=""overflow:scroll"">" _
                            & genericController.htmlIndent(html.main_GetPanelHeader("Contensive Resource Library")) _
                            & cr & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%""><tr><td>" _
                            & cr2 & "<div style=""border-top:1px solid white;border-bottom:1px solid black;height:2px""><img alt=""spacer"" src=""/ccLib/images/spacer.gif"" width=1 height=1></div>" _
                            & genericController.htmlIndent(Copy) _
                            & cr & "</td></tr>" _
                            & cr & "<tr><td>" _
                            & genericController.htmlIndent(html.getHtmlDoc_beforeEndOfBodyHtml(False, False, False, False)) _
                            & cr & "</td></tr></table>" _
                            & cr & "<script language=javascript type=""text/javascript"">fixDialog();</script>" _
                            & cr & "</body>" _
                            & "</html>"
                            Call html.writeAltBuffer(Copy)
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
                            Call doc.addRefreshQueryString("LinkObjectName", LinkObjectName)
                            Call html.main_AddHeadScriptLink("/ccLib/ClientSide/dialogs.js", "Resource Library")
                            'Call AddHeadScript("<script type=""text/javascript"" src=""/ccLib/ClientSide/dialogs.js""></script>")
                            Call html.main_SetMetaContent(0, 0)
                            Call html.main_AddOnLoadJavascript2("document.body.style.overflow='scroll';", "Resource Library")
                            Copy = html.main_GetResourceLibrary2("", True, EditorObjectName, LinkObjectName, True)
                            'Call AppendLog("call main_getEndOfBody, from main_init_printhardcodedpage2c")
                            Copy = "" _
                            & siteProperties.docTypeDeclaration() _
                            & cr & "<html>" _
                            & cr & "<head>" _
                            & genericController.htmlIndent(html.getHtmlDocHead(False)) _
                            & cr & "</head>" _
                            & cr & "<body class=""ccBodyAdmin ccCon"" style=""overflow:scroll"">" _
                            & html.main_GetPanelHeader("Contensive Resource Library") _
                            & cr & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%""><tr><td>" _
                            & Copy _
                            & cr & "</td></tr><tr><td>" & html.getHtmlDoc_beforeEndOfBodyHtml(False, False, False, False) & "</td></tr></table>" _
                            & cr & "<script language=javascript type=text/javascript>fixDialog();</script>" _
                            & cr & "</body>" _
                            & vbCrLf & "</html>"
                            Call html.writeAltBuffer(Copy)
                            result = True
                        End If
                    Case HardCodedPageLoginDefault
                        '
                        ' 9/4/2012 added to prevent lockout if login addon fails
                        doc.refreshQueryString = webServer.requestQueryString
                        'Call main_AddRefreshQueryString("method", "")
                        Call html.writeAltBuffer(html.getLoginPage(True))
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
                        doc.refreshQueryString = webServer.requestQueryString
                        'Call main_AddRefreshQueryString("method", "")
                        Call html.writeAltBuffer(html.getLoginPage(False))
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
                        Call doc.addRefreshQueryString(RequestNameHardCodedPage, HardCodedPageSiteExplorer)
                        LinkObjectName = docProperties.getText("LinkObjectName")
                        If LinkObjectName <> "" Then
                            '
                            ' Open a page compatible with a dialog
                            '
                            Call doc.addRefreshQueryString("LinkObjectName", LinkObjectName)
                            Call html.main_AddPagetitle("Site Explorer")
                            Call html.main_SetMetaContent(0, 0)
                            Copy = addon.execute_legacy5(0, "Site Explorer", "", CPUtilsBaseClass.addonContext.ContextPage, "", 0, "", 0)
                            Call html.main_AddOnLoadJavascript2("document.body.style.overflow='scroll';", "Site Explorer")
                            'Call AppendLog("call main_getEndOfBody, from main_init_printhardcodedpage2d")
                            Copy = "" _
                            & siteProperties.docTypeDeclaration() _
                            & cr & "<html>" _
                            & cr & "<head>" _
                            & genericController.htmlIndent(html.getHtmlDocHead(False)) _
                            & cr & "</head>" _
                            & cr & "<body class=""ccBodyAdmin ccCon"" style=""overflow:scroll"">" _
                            & genericController.htmlIndent(html.main_GetPanelHeader("Contensive Site Explorer")) _
                            & cr & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%""><tr><td>" _
                            & genericController.htmlIndent(Copy) _
                            & cr & "</td></tr><tr><td>" & html.getHtmlDoc_beforeEndOfBodyHtml(False, False, False, False) & "</td></tr></table>" _
                            & cr & "</body>" _
                            & cr & "</html>"
                            'Set Obj = Nothing
                            Call html.writeAltBuffer(Copy)
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
                        Call html.main_ClearStream()
                        If app_errorCount = 0 Then
                            Call html.writeAltBuffer("Contensive OK")
                        Else
                            Call html.writeAltBuffer("Contensive Error Count = " & app_errorCount)
                        End If
                        webServer.webServerIO_BlockClosePageCopyright = True
                        html_BlockClosePageLink = True
                        'Call AppendLog("call main_getEndOfBody, from main_init_printhardcodedpage2f")
                        Call html.getHtmlDoc_beforeEndOfBodyHtml(False, False, False, False)
                        result = True
                    'Case HardCodedPageGetJSPage
                    '    '
                    '    ' ----- Create a Javascript page that outputs a page content record
                    '    '
                    '    Name = docProperties.getText("name")
                    '    If Name <> "" Then
                    '        webServer.webServerIO_BlockClosePageCopyright = True
                    '        '
                    '        ' Determine bid (PageID) from referer querystring
                    '        '
                    '        Copy = webServer.requestReferrer
                    '        Pos = genericController.vbInstr(1, Copy, rnPageId & "=")
                    '        If Pos <> 0 Then
                    '            Copy = Trim(Mid(Copy, Pos + 4))
                    '            Pos = genericController.vbInstr(1, Copy, "&")
                    '            If Pos <> 0 Then
                    '                Copy = Trim(Mid(Copy, 1, Pos))
                    '            End If
                    '            PageID = genericController.EncodeInteger(Copy)
                    '        End If
                    '        '
                    '        ' main_Get the page
                    '        '
                    '        rootPageId = db.getRecordID("Page Content", Name)
                    '        allowPageWithoutSectionDisplay = siteProperties.getBoolean(spAllowPageWithoutSectionDisplay, spAllowPageWithoutSectionDisplay_default)
                    '        If Not allowPageWithoutSectionDisplay Then
                    '            allowPageWithoutSectionDisplay = authContext.isAuthenticatedContentManager(Me, ContentName)
                    '        End If
                    '        Copy = pages.getContentBox(PageID, rootPageId, "Page Content", "", True, True, False, 0, siteProperties.useContentWatchLink, allowPageWithoutSectionDisplay)
                    '        'Call AppendLog("call main_getEndOfBody, from main_init_printhardcodedpage2g")
                    '        Copy = Copy & htmlDoc.getHtmlDoc_beforeEndOfBodyHtml(False, True, False, False)
                    '        Copy = genericController.vbReplace(Copy, "'", "'+""'""+'")
                    '        Copy = genericController.vbReplace(Copy, vbCr, "\n")
                    '        Copy = genericController.vbReplace(Copy, vbLf, " ")
                    '        '
                    '        ' Write the page to the stream, with a javascript wrapper
                    '        '
                    '        MsgLabel = "Msg" & genericController.encodeText(genericController.GetRandomInteger)
                    '        Call webServer.setResponseContentType("text/plain")
                    '        Call htmlDoc.writeAltBuffer("var " & MsgLabel & " = '" & Copy & "'; " & vbCrLf)
                    '        Call htmlDoc.writeAltBuffer("document.write( " & MsgLabel & " ); " & vbCrLf)
                    '    End If
                    '    result = True
                    Case HardCodedPageGetJSLogin
                        '
                        ' ----- Create a Javascript login page
                        '
                        webServer.webServerIO_BlockClosePageCopyright = True
                        Copy = Copy & "<p align=""center""><CENTER>"
                        If Not authContext.isAuthenticated() Then
                            Copy = Copy & html.getLoginPanel()
                        ElseIf authContext.isAuthenticatedContentManager(Me, "Page Content") Then
                            'Copy = Copy & main_GetToolsPanel
                        Else
                            Copy = Copy & "You are currently logged in as " & authContext.user.Name & ". To logout, click <a HREF=""" & webServer.webServerIO_ServerFormActionURL & "?Method=logout"" rel=""nofollow"">Here</A>."
                        End If
                        'Call AppendLog("call main_getEndOfBody, from main_init_printhardcodedpage2h")
                        Copy = Copy & html.getHtmlDoc_beforeEndOfBodyHtml(True, True, False, False)
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
                        Call html.writeAltBuffer("var " & MsgLabel & " = '" & Copy & "'; " & vbCrLf)
                        Call html.writeAltBuffer("document.write( " & MsgLabel & " ); " & vbCrLf)
                        result = True
                    Case HardCodedPageRedirect
                        '
                        ' ----- Redirect with RC and RI
                        '
                        doc.pageManager_RedirectContentID = docProperties.getInteger(rnRedirectContentId)
                        doc.pageManager_RedirectRecordID = docProperties.getInteger(rnRedirectRecordId)
                        If doc.pageManager_RedirectContentID <> 0 And doc.pageManager_RedirectRecordID <> 0 Then
                            ContentName = metaData.getContentNameByID(doc.pageManager_RedirectContentID)
                            If ContentName <> "" Then
                                Call iisController.main_RedirectByRecord_ReturnStatus(Me, ContentName, doc.pageManager_RedirectRecordID)
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
                            Call html.writeAltBuffer("Error: You must be an administrator to use the ExportAscii method")
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
                                Call html.writeAltBuffer("Error: ExportAscii method requires ContentName")
                            Else
                                Call html.writeAltBuffer(Controllers.exportAsciiController.exportAscii_GetAsciiExport(Me, ContentName, PageSize, PageNumber))
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
            Dim requestForm As String
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
                    continueProcessing = False
                    '
                    ' content server object is valid
                    '
                    If (serverConfig.appConfig IsNot Nothing) Then
                        If siteProperties.allowVisitTracking Then
                            '
                            ' If visit tracking, save the viewing record
                            '
                            ViewingName = Left(authContext.visit.id & "." & authContext.visit.PageVisits, 10)
                            PageID = 0
                            If (_doc IsNot Nothing) Then
                                If (doc.page IsNot Nothing) Then
                                    PageID = doc.page.id
                                End If
                            End If
                            '
                            ' -- convert requestFormDict to a name=value string for Db storage
                            Dim requestFormSerialized As String = genericController.convertNameValueDictToREquestString(webServer.requestFormDict)
                            FieldNames = "Name,VisitId,MemberID,Host,Path,Page,QueryString,Form,Referer,DateAdded,StateOK,ContentControlID,pagetime,Active,CreateKey,RecordID"
                            FieldNames = FieldNames & ",ExcludeFromAnalytics"
                            FieldNames = FieldNames & ",pagetitle"
                            SQL = "INSERT INTO ccViewings (" _
                                & FieldNames _
                                & ")VALUES(" _
                                & " " & db.encodeSQLText(ViewingName) _
                                & "," & db.encodeSQLNumber(authContext.visit.id) _
                                & "," & db.encodeSQLNumber(authContext.user.id) _
                                & "," & db.encodeSQLText(webServer.requestDomain) _
                                & "," & db.encodeSQLText(webServer.requestPath) _
                                & "," & db.encodeSQLText(webServer.requestPage) _
                                & "," & db.encodeSQLText(Left(webServer.requestQueryString, 255)) _
                                & "," & db.encodeSQLText(Left(requestFormSerialized, 255)) _
                                & "," & db.encodeSQLText(Left(webServer.requestReferrer, 255)) _
                                & "," & db.encodeSQLDate(app_startTime) _
                                & "," & db.encodeSQLBoolean(authContext.visit_stateOK) _
                                & "," & db.encodeSQLNumber(metaData.getContentId("Viewings")) _
                                & "," & db.encodeSQLNumber(appStopWatch.ElapsedMilliseconds) _
                                & ",1" _
                                & "," & db.encodeSQLNumber(CSMax) _
                                & "," & db.encodeSQLNumber(PageID)
                            SQL &= "," & db.encodeSQLBoolean(webServer.webServerIO_PageExcludeFromAnalytics)
                            SQL &= "," & db.encodeSQLText(doc.main_MetaContent_Title)
                            SQL &= ");"
                            Call db.executeSql(SQL)
                        End If
                    End If
                    '
                    ' ----- dispose objects created here
                    '
                    If Not (_addonCache Is Nothing) Then
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










    End Class
End Namespace
