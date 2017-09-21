
Option Strict On
Option Explicit On

Imports System.Reflection
Imports Contensive.BaseClasses
Imports Contensive.Core.Controllers
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
        Friend Property cp_forAddonExecutionOnly As CPClass                                   ' constructor -- top-level cp
        Public Property serverConfig As Models.Entity.serverConfigModel
        Friend Property errList As List(Of String)                                   ' exceptions collected during document construction
        Public Property errorCount As Integer = 0
        Friend Property userErrorList As List(Of String)                           ' user messages
        Public Property debug_iUserError As String = ""                              ' User Error String
        Public Property trapLogMessage As String = ""                           ' The content of the current traplog (keep for popups if no Csv)
        Public Property testPointMessage As String = ""                         '
        Public Property testPointPrinting As Boolean = False                         ' if true, send main_TestPoint messages to the stream
        Public Property authContext As authContextModel
        Private Property appStopWatch As Stopwatch = Stopwatch.StartNew()
        Public Property profileStartTime As Date                                        ' set in constructor
        Public Property profileStartTickCount As Integer = 0
        Public Property allowDebugLog As Boolean = False                       ' turn on in script -- use to write /debug.log in content files for whatever is needed
        Public Property blockExceptionReporting As Boolean = False                   ' used so error reporting can not call itself
        'Public Property pageErrorWithoutCsv As Boolean = False                  ' if true, the error occurred before Csv was available and main_TrapLogMessage needs to be saved and popedup
        'Public Property closePageCounter As Integer = 0
        Public Property blockClosePageLink As Boolean = False                   ' if true,block the href to contensive
        Public Property continueProcessing As Boolean = False                                   ' when false, routines should not add to the output and immediately exit
        Public Property upgradeInProgress() As Boolean
        Public Property docGuid As String                        ' Random number (semi) unique to this hit
        Friend Property addonsRunOnThisPageIdList As New List(Of Integer)
        Friend Property addonsCurrentlyRunningIdList As New List(Of Integer)
        Public Property pageAddonCnt As Integer = 0
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
                            _addonCache.add(addon)
                        Next
                        Call cache.setObject("addonCache", _addonCache)
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
            Me.serverConfig.appConfig.appStatus = Models.Entity.serverConfigModel.appStatusEnum.ready
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
            Me.serverConfig.appConfig.appStatus = Models.Entity.serverConfigModel.appStatusEnum.ready
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
        ''' Executes the current route (pathPage and/or querystring based). If not found, the default route (addon) is executed. Initially the default route is the page Manager.
        ''' </summary>
        ''' <returns>The doc created by the default addon. (html, json, etc)</returns>
        Public Function executeRoute(Optional route As String = "") As String
            Dim result As String = ""
            Try
                If (serverConfig.appConfig IsNot Nothing) Then
                    '
                    ' determine route from either url or querystring 
                    '
                    Dim normalRoute As String = webServer.requestPathPage.ToLower
                    Dim RemoteMethodFromQueryString As String = docProperties.getText(RequestNameRemoteMethodAddon)
                    If (Not String.IsNullOrEmpty(route)) Then
                        '
                        ' route privided as argument
                        '
                        normalRoute = route
                    ElseIf (Not String.IsNullOrEmpty(RemoteMethodFromQueryString)) Then
                        '
                        ' route comes from a remoteMethod=route querystring argument
                        '
                        normalRoute = "/" & RemoteMethodFromQueryString.ToLower()
                    End If
                    '
                    ' normalize route to /path/page or /path
                    '
                    normalRoute = genericController.normalizeRoute(normalRoute)
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
                        Dim addonRoute As String = normalRoute
                        Dim addon As addonModel = addonCache.getAddonByName(addonRoute)
                        If (addon Is Nothing) Then
                            '
                            ' -- try testRoute2
                            addonRoute = normalRoute & "/"
                            addon = addonCache.getAddonByName(addonRoute)
                            If (addon Is Nothing) Then
                                '
                                ' -- try testRoute3
                                addonRoute = normalRoute.Substring(1)
                                addon = addonCache.getAddonByName(addonRoute)
                                If (addon Is Nothing) Then
                                    '
                                    ' -- try testRoute4
                                    addonRoute &= "/"
                                    addon = addonCache.getAddonByName(addonRoute)
                                    If (addon Is Nothing) Then
                                        '
                                        ' -- not found
                                        addonRoute = ""
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
                            If webServer.readStreamJSForm Then
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
                                                handleException(New ApplicationException("Add-on call from [" & refHost & "] was blocked because this domain is not in the Aggregate Access Content. An inactive record was added. To allow this domain access, mark the record active.")) ' handleLegacyError12("Init", "")
                                                continueProcessing = False '--- should be disposed by caller --- Call dispose
                                                Return doc.docBuffer
                                            ElseIf Not db.cs_getBoolean(cs, "active") Then
                                                '
                                                ' inactive record, throw error
                                                '
                                                Call db.cs_Close(cs)
                                                handleException(New ApplicationException("Add-on call from [" & refHost & "] was blocked because this domain is not active in the Aggregate Access Content. To allow this domain access, mark the record active.")) ' handleLegacyError12("Init", "")
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
                                        Dim pairs() As String = Split(webServer.requestQueryString, "&")
                                        For addonPtr = 0 To UBound(pairs)
                                            Dim pairName As String = pairs(addonPtr)
                                            Dim pairValue As String = ""
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
                                Dim executeContext As New CPUtilsBaseClass.addonExecuteContext() With {
                                    .addonType = CPUtilsBaseClass.addonContext.ContextRemoteMethodJson,
                                    .cssContainerClass = "",
                                    .cssContainerId = "",
                                    .hostRecord = New CPUtilsBaseClass.addonExecuteHostRecordContext() With {
                                        .contentName = HostContentName,
                                        .fieldName = "",
                                        .recordId = hostRecordId
                                    },
                                    .personalizationAuthenticated = authContext.isAuthenticated,
                                    .personalizationPeopleId = authContext.user.id
                                }
                                result = Me.addon.execute(Models.Entity.addonModel.createByName(Me, addonRoute), executeContext)
                                'result = Me.addon.execute_legacy6(0, addonRoute, Option_String, CPUtilsBaseClass.addonContext.ContextRemoteMethodJson, HostContentName, hostRecordId, "", "0", False, 0, "", False, Nothing, "", Nothing, "", authContext.user.id, authContext.isAuthenticated)
                            End If
                            '
                            ' deliver styles, javascript and other head tags as javascript appends
                            '
                            webServer.blockClosePageCopyright = True
                            blockClosePageLink = True
                            If (webServer.outStreamDevice = htmlDoc_OutStreamJavaScript) Then
                                If genericController.vbInstr(1, result, "<form ", vbTextCompare) <> 0 Then
                                    Dim FormSplit As String() = Split(result, "<form ", , vbTextCompare)
                                    result = FormSplit(0)
                                    For addonPtr = 1 To UBound(FormSplit)
                                        Dim FormEndPos As Integer = genericController.vbInstr(1, FormSplit(addonPtr), ">")
                                        Dim FormInner As String = Mid(FormSplit(addonPtr), 1, FormEndPos)
                                        Dim FormSuffix As String = Mid(FormSplit(addonPtr), FormEndPos + 1)
                                        FormInner = genericController.vbReplace(FormInner, "method=""post""", "method=""main_Get""", 1, 99, vbTextCompare)
                                        FormInner = genericController.vbReplace(FormInner, "method=post", "method=""main_Get""", 1, 99, vbTextCompare)
                                        result = result & "<form " & FormInner & FormSuffix
                                    Next
                                End If
                                '
                                Call html.writeAltBuffer(result)
                                result = ""
                            End If
                            Return result
                        End If
                        If True Then
                            '
                            '------------------------------------------------------------------------------------------
                            '   These should all be converted to system add-ons
                            '
                            '   AJAX late functions (slower then the early functions, but they include visit state, etc.
                            '------------------------------------------------------------------------------------------
                            '
                            Dim AjaxFunction As String = docProperties.getText(RequestNameAjaxFunction)
                            If AjaxFunction <> "" Then
                                result = ""
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
                                        dt = db.executeQuery(Sql)
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

                                        dt = db.executeQuery(Sql)
                                        If dt.Rows.Count > 0 Then
                                            For Each rsDr As DataRow In dt.Rows
                                                Dim addonId As Integer = genericController.EncodeInteger(rsDr("addonid"))
                                                If (addonId <> 0) And (addonId <> addonDefaultEditorId) Then
                                                    result = result _
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

                                        result = "" _
                                        & vbCrLf & vbTab & "<h1>Editor Preference</h1>" _
                                        & vbCrLf & vbTab & "<p>Select the editor you will use for this field. Select default if you want to use the current system default.</p>" _
                                        & vbCrLf & vbTab & "<div class=""radioCon"">" & html.html_GetFormInputRadioBox("setEditorPreference" & fieldId, "0", "0") & "&nbsp;Use Default Editor" & addonDefaultEditorName & "</div>" _
                                        & vbCrLf & vbTab & result _
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
                                            result = addonController.main_GetDefaultAddonOption_String(Me, addonArgumentList, AddonGuid, addonIsInline)
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
                                        result = remoteQueryController.main_FormatRemoteQueryOutput(Me, gd, RemoteFormatEnum.RemoteFormatJsonNameValue)
                                        result = html.main_encodeHTML(result)
                                        Call html.writeAltBuffer(result)
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
                                        result = remoteQueryController.main_FormatRemoteQueryOutput(Me, gd, RemoteFormatEnum.RemoteFormatJsonNameValue)
                                        result = html.main_encodeHTML(result)
                                        Call html.writeAltBuffer(result)
                                    Case AjaxData
                                        '
                                        ' 7/7/2009 - Moved from HardCodedPages - Run remote query from cj.remote object call, and return results html encoded in a <result></result> block
                                        ' 20050427 - not used
                                        Call html.writeAltBuffer(executeRoute_ProcessAjaxData())
                                    Case AjaxPing
                                        '
                                        ' returns OK if the server is alive
                                        '
                                        result = "ok"
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
                                            result = "No filter is available"
                                        Else
                                            Dim cdef As cdefModel = metaData.getCdef(ContentID)
                                            result = adminSite.GetForm_IndexFilterContent(cdef)
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
                                webServer.blockClosePageCopyright = True
                                blockClosePageLink = True
                                'Call AppendLog("call main_getEndOfBody, from main_initf")
                                ' -- removed, not sure what it did but this should just be part of getHtmlDoc() 
                                'returnResult = returnResult & html.getHtmlDoc_beforeEndOfBodyHtml(False, False, True, False)
                                Call html.writeAltBuffer(result)
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
                                    Call db.cs_set(CSLog, "Name", "Opened " & CStr(profileStartTime))
                                    Call db.cs_set(CSLog, "EmailDropID", recordid)
                                    Call db.cs_set(CSLog, "MemberID", EmailMemberID)
                                    Call db.cs_set(CSLog, "LogType", EmailLogTypeOpen)
                                End If
                                Call db.cs_Close(CSLog)
                                RedirectLink = webServer.requestProtocol & webServer.requestDomain & "/ccLib/images/spacer.gif"
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
                                    Call db.cs_set(CSLog, "Name", "Clicked " & CStr(profileStartTime))
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
                                        Call db.cs_set(CSLog, "Name", "Email Block Request " & CStr(profileStartTime))
                                        Call db.cs_set(CSLog, "EmailDropID", emailDropId)
                                        Call db.cs_set(CSLog, "MemberID", EmailMemberID)
                                        Call db.cs_set(CSLog, "VisitId", authContext.visit.id)
                                        Call db.cs_set(CSLog, "LogType", EmailLogTypeBlockRequest)
                                    End If
                                    Call db.cs_Close(CSLog)
                                End If
                                Call webServer.redirect(webServer.requestProtocol & webServer.requestDomain & "/ccLib/popup/EmailBlocked.htm", "Group Email Spam Block hit. Redirecting to EmailBlocked page.", False)
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
                            '
                            ' -- must support this for tool panel addon, until it is fixed
                            Dim legacyFormSn As String = docProperties.getText("ccformsn")
                            formType = docProperties.getText(legacyFormSn & "type")
                            If (formType <> "") Then
                                '
                                ' set the meta content flag to show it is not needed for the head tag
                                '
                                Call doc.setMetaContent(0, 0)
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
                                                'Call appRootFiles.saveFile("templates\Public" & StyleSN & ".css", html.html_getStyleSheet2(0, 0))
                                                'Call appRootFiles.saveFile("templates\Admin" & StyleSN & ".css", html.getStyleSheetDefault())
                                            End If
                                        End If
                                    Case FormTypeAddonStyleEditor
                                        '
                                        ' save custom styles
                                        '
                                        If authContext.isAuthenticated() And authContext.isAuthenticatedAdmin(Me) Then
                                            Dim addonId As Integer
                                            addonId = docProperties.getInteger("AddonID")
                                            If (addonId > 0) Then
                                                Dim styleAddon As Models.Entity.addonModel = Models.Entity.addonModel.create(Me, addonId)
                                                If (styleAddon.StylesFilename.content <> docProperties.getText("CustomStyles")) Then
                                                    styleAddon.StylesFilename.content = docProperties.getText("CustomStyles")
                                                    styleAddon.save(Me)
                                                    '
                                                    ' Clear Caches
                                                    '
                                                    Call cache.invalidateContent(addonModel.contentName)
                                                End If
                                            End If
                                        End If
                                    Case FormTypeAddonSettingsEditor
                                        '
                                        '
                                        '
                                        Call html.processAddonSettingsEditor()
                                    Case FormTypeHelpBubbleEditor
                                        '
                                        '
                                        '
                                        Call html.processHelpBubbleEditor()
                                    Case FormTypeJoin
                                        '
                                        '
                                        '
                                        Call Controllers.loginController.processFormJoin(Me)
                                    Case FormTypeSendPassword
                                        '
                                        '
                                        '
                                        Call Controllers.loginController.processFormSendPassword(Me)
                                    Case FormTypeLogin, "l09H58a195"
                                        '
                                        '
                                        '
                                        Call Controllers.loginController.processFormLoginDefault(Me)
                                    Case FormTypeToolsPanel
                                        '
                                        ' ----- Administrator Tools Panel
                                        '
                                        Call html.processFormToolsPanel(legacyFormSn)
                                    Case FormTypePageAuthoring
                                        '
                                        ' ----- Page Authoring Tools Panel
                                        '
                                        Call doc.processFormQuickEditing()
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
                            Dim exitNow As Boolean = False
                            result &= executeRoute_hardCodedPage(HardCodedPage, exitNow)
                            If exitNow Then
                                continueProcessing = False '--- should be disposed by caller --- Call dispose
                                Return result
                            End If
                        End If
                        '
                        '--------------------------------------------------------------------------
                        ' normalize adminRoute and test for hit
                        '--------------------------------------------------------------------------
                        '
                        If (normalRoute = genericController.normalizeRoute(serverConfig.appConfig.adminRoute.ToLower)) Then
                            '
                            ' route is admin
                            '   If the Then admin route Is taken -- the login panel processing Is bypassed. those methods need To be a different kind Of route, Or it should be an addon
                            '   runAtServerClass in the admin addon.
                            result = Me.addon.execute(addonModel.create(Me, addonGuidAdminSite), New CPUtilsBaseClass.addonExecuteContext() With {.addonType = CPUtilsBaseClass.addonContext.ContextAdmin})
                            'result = Me.addon.execute_legacy4(addonGuidAdminSite, docProperties.getLegacyOptionStringFromVar(), CPUtilsBaseClass.addonContext.ContextAdmin, Nothing)
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
                            Dim defaultAddonId As Integer = siteProperties.getinteger(spDefaultRouteAddonId)
                            If (defaultAddonId = 0) Then
                                '
                                ' -- no default route set, assume html hit
                                result = "<p>This site is not configured for website traffic. Please set the default route.</p>"
                            Else
                                Dim executeContext As New CPUtilsBaseClass.addonExecuteContext() With {
                                    .addonType = CPUtilsBaseClass.addonContext.ContextPage,
                                    .cssContainerClass = "",
                                    .cssContainerId = "",
                                    .hostRecord = New CPUtilsBaseClass.addonExecuteHostRecordContext() With {
                                        .contentName = "",
                                        .fieldName = "",
                                        .recordId = 0
                                    },
                                    .personalizationAuthenticated = authContext.visit.VisitAuthenticated,
                                    .personalizationPeopleId = authContext.user.id
                                }
                                result = Me.addon.execute(Models.Entity.addonModel.create(Me, defaultAddonId), executeContext)
                                'result = Me.addon.execute_legacy6(defaultAddonId, "", "", CPUtilsBaseClass.addonContext.ContextPage, "", 0, "", "", False, 0, "", False, Nothing, "", Nothing, "", authContext.user.id, authContext.visit.VisitAuthenticated)
                            End If
                        End If
                    End If
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
        Private Function executeRoute_ProcessAjaxData() As String
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
                    Dim CS As Integer = db.cs_open("Remote Queries", "((VisitId=" & authContext.visit.id & ")and(remotekey=" & db.encodeSQLText(RemoteKey) & "))")
                    If db.cs_ok(CS) Then
                        '
                        ' Use user definied query
                        '
                        Dim SQLQuery As String = db.cs_getText(CS, "sqlquery")
                        'DataSource = Models.Entity.dataSourceModel.create(Me, db.cs_getInteger(CS, "datasourceid"), New List(Of String))
                        maxRows = db.cs_getInteger(CS, "maxrows")
                        QueryType = db.cs_getInteger(CS, "QueryTypeID")
                        ContentName = db.cs_get(CS, "ContentID")
                        Criteria = db.cs_getText(CS, "Criteria")
                        SortFieldList = db.cs_getText(CS, "SortFieldList")
                        AllowInactiveRecords2 = db.cs_getBoolean(CS, "AllowInactiveRecords")
                        SelectFieldList = db.cs_getText(CS, "SelectFieldList")
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
                    Call db.cs_Close(CS)
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
                                CS = db.cs_open(ContentName, Criteria, SortFieldList, AllowInactiveRecords2, , ,, SelectFieldList)
                                If db.cs_ok(CS) Then
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
        '=========================================================================================
        ''' <summary>
        ''' In Init(), Print Hard Coded Pages, A Hard coded page replaces the entire output with an HTML compatible page
        ''' </summary>
        ''' <param name="HardCodedPage"></param>
        ''' <returns></returns>
        Private Function executeRoute_hardCodedPage(ByVal HardCodedPage As String, ByRef exitNow As Boolean) As String
            Dim result As String = ""
            Try
                exitNow = False
                Select Case genericController.vbLCase(HardCodedPage)
                    Case HardCodedPageSendPassword
                        '
                        ' -- send password
                        Dim Emailtext As String = docProperties.getText("email")
                        If Emailtext <> "" Then
                            Call email.sendPassword(Emailtext)
                            result &= "" _
                                & "<div style=""width:300px;margin:100px auto 0 auto;"">" _
                                & "<p>An attempt to send login information for email address '" & Emailtext & "' has been made.</p>" _
                                & "<p><a href=""?" & doc.refreshQueryString & """>Return to the Site.</a></p>" _
                                & "</div>"
                            exitNow = True
                        End If
                    Case HardCodedPageResourceLibrary
                        '
                        ' -- resource library
                        Call doc.addRefreshQueryString(RequestNameHardCodedPage, HardCodedPageResourceLibrary)
                        Dim EditorObjectName As String = docProperties.getText("EditorObjectName")
                        Dim LinkObjectName As String = docProperties.getText("LinkObjectName")
                        If EditorObjectName <> "" Then
                            '
                            ' Open a page compatible with a dialog
                            '
                            Call doc.addRefreshQueryString("EditorObjectName", EditorObjectName)
                            Call html.addJavaScriptLinkHead("/ccLib/ClientSide/dialogs.js", "Resource Library")
                            'Call AddHeadScript("<script type=""text/javascript"" src=""/ccLib/ClientSide/dialogs.js""></script>")
                            Call doc.setMetaContent(0, 0)
                            Call html.addOnLoadJavascript("document.body.style.overflow='scroll';", "Resource Library")
                            Dim Copy As String = html.main_GetResourceLibrary2("", True, EditorObjectName, LinkObjectName, True)
                            Dim htmlBody As String = "" _
                                & genericController.htmlIndent(html.main_GetPanelHeader("Contensive Resource Library")) _
                                & cr & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%""><tr><td>" _
                                & cr2 & "<div style=""border-top:1px solid white;border-bottom:1px solid black;height:2px""><img alt=""spacer"" src=""/ccLib/images/spacer.gif"" width=1 height=1></div>" _
                                & genericController.htmlIndent(Copy) _
                                & cr & "</td></tr>" _
                                & cr & "<tr><td>" _
                                & genericController.htmlIndent(html.getHtmlDoc_beforeEndOfBodyHtml(False, False, False, False)) _
                                & cr & "</td></tr></table>" _
                                & cr & "<script language=javascript type=""text/javascript"">fixDialog();</script>" _
                                & ""
                            Dim htmlBodyTag As String = "<body class=""ccBodyAdmin ccCon"" style=""overflow:scroll"">"
                            result = html.getHtmlDoc(htmlBody, htmlBodyTag, False, False, False, False)
                            exitNow = True
                        ElseIf LinkObjectName <> "" Then
                            '
                            ' Open a page compatible with a dialog
                            Call doc.addRefreshQueryString("LinkObjectName", LinkObjectName)
                            Call html.addJavaScriptLinkHead("/ccLib/ClientSide/dialogs.js", "Resource Library")
                            Call doc.setMetaContent(0, 0)
                            Call html.addOnLoadJavascript("document.body.style.overflow='scroll';", "Resource Library")
                            Dim htmlBody As String = "" _
                                & html.main_GetPanelHeader("Contensive Resource Library") _
                                & cr & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%""><tr><td>" _
                                & html.main_GetResourceLibrary2("", True, EditorObjectName, LinkObjectName, True) _
                                & cr & "</td></tr></table>" _
                                & cr & "<script language=javascript type=text/javascript>fixDialog();</script>" _
                                & ""
                            Dim htmlBodyTag As String = "<body class=""ccBodyAdmin ccCon"" style=""overflow:scroll"">"
                            result = html.getHtmlDoc(htmlBody, htmlBodyTag, False, False, False, False)
                            exitNow = True
                        End If
                    Case HardCodedPageLoginDefault
                        '
                        ' -- default login page
                        exitNow = True
                        Dim addonArguments As New Dictionary(Of String, String)
                        addonArguments.Add("Force Default Login", "true")
                        Return addon.execute(
                            addonModel.create(Me, addonGuidLoginPage),
                                New CPUtilsBaseClass.addonExecuteContext() With {
                                    .addonType = CPUtilsBaseClass.addonContext.ContextPage,
                                    .instanceArguments = addonArguments
                                }
                            )
                    Case HardCodedPageLogin, HardCodedPageLogoutLogin
                        '
                        ' -- login
                        If genericController.vbUCase(HardCodedPage) = "LOGOUTLOGIN" Then
                            '
                            ' -- must logout first
                            Call authContext.logout(Me)
                        End If
                        exitNow = True
                        Dim addonArguments As New Dictionary(Of String, String)
                        addonArguments.Add("Force Default Login", "false")
                        Return addon.execute(
                            addonModel.create(Me, addonGuidLoginPage),
                                New CPUtilsBaseClass.addonExecuteContext() With {
                                    .addonType = CPUtilsBaseClass.addonContext.ContextPage
                                }
                            )
                    Case HardCodedPageLogout
                        '
                        ' -- logout the current member
                        Call authContext.logout(Me)
                        Return String.Empty
                    Case HardCodedPageSiteExplorer
                        '
                        ' 7/8/9 - Moved from intercept pages
                        '
                        Call doc.addRefreshQueryString(RequestNameHardCodedPage, HardCodedPageSiteExplorer)
                        Dim LinkObjectName As String = docProperties.getText("LinkObjectName")
                        If LinkObjectName <> "" Then
                            '
                            ' Open a page compatible with a dialog
                            '
                            Call doc.addRefreshQueryString("LinkObjectName", LinkObjectName)
                            Call html.main_AddPagetitle("Site Explorer")
                            Call doc.setMetaContent(0, 0)
                            Dim copy As String = addon.execute(addonModel.createByName(Me, "Site Explorer"), New CPUtilsBaseClass.addonExecuteContext() With {.addonType = CPUtilsBaseClass.addonContext.ContextPage})
                            'Dim Copy As String = addon.execute_legacy5(0, "Site Explorer", "", CPUtilsBaseClass.addonContext.ContextPage, "", 0, "", 0)
                            Call html.addOnLoadJavascript("document.body.style.overflow='scroll';", "Site Explorer")
                            Dim htmlBodyTag As String = "<body class=""ccBodyAdmin ccCon"" style=""overflow:scroll"">"
                            Dim htmlBody As String = "" _
                                & genericController.htmlIndent(html.main_GetPanelHeader("Contensive Site Explorer")) _
                                & cr & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%""><tr><td>" _
                                & genericController.htmlIndent(copy) _
                                & cr & "</td></tr></table>" _
                                & ""
                            result = html.getHtmlDoc(htmlBody, htmlBodyTag, False, False, False, False)
                            exitNow = True
                        End If
                    Case HardCodedPageStatus
                        '
                        ' Status call
                        '
                        webServer.blockClosePageCopyright = True
                        '
                        ' test default data connection
                        '
                        Dim InsertTestOK As Boolean = False
                        Dim TrapID As Integer = 0
                        Dim CS As Integer = db.cs_insertRecord("Trap Log")
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
                        If errorCount = 0 Then
                            Call html.writeAltBuffer("Contensive OK")
                        Else
                            Call html.writeAltBuffer("Contensive Error Count = " & errorCount)
                        End If
                        webServer.blockClosePageCopyright = True
                        blockClosePageLink = True
                        result = html.getHtmlDoc_beforeEndOfBodyHtml(False, False, False, False)
                        exitNow = True
                    Case HardCodedPageRedirect
                        '
                        ' ----- Redirect with RC and RI
                        '
                        doc.redirectContentID = docProperties.getInteger(rnRedirectContentId)
                        doc.redirectRecordID = docProperties.getInteger(rnRedirectRecordId)
                        If doc.redirectContentID <> 0 And doc.redirectRecordID <> 0 Then
                            Dim ContentName As String = metaData.getContentNameByID(doc.redirectContentID)
                            If ContentName <> "" Then
                                Call iisController.main_RedirectByRecord_ReturnStatus(Me, ContentName, doc.redirectRecordID)
                                result = ""
                                blockClosePageLink = True
                                exitNow = True
                            End If
                        End If
                    Case HardCodedPageExportAscii
                        '
                        ' -- Should be a remote method in commerce
                        If Not authContext.isAuthenticatedAdmin(Me) Then
                            '
                            ' Administrator required
                            '
                            userErrorList.Add("Error: You must be an administrator to use the ExportAscii method")
                        Else
                            webServer.blockClosePageCopyright = True
                            Dim ContentName As String = docProperties.getText("content")
                            Dim PageSize As Integer = docProperties.getInteger("PageSize")
                            If PageSize = 0 Then
                                PageSize = 20
                            End If
                            Dim PageNumber As Integer = docProperties.getInteger("PageNumber")
                            If PageNumber = 0 Then
                                PageNumber = 1
                            End If
                            If (ContentName = "") Then
                                userErrorList.Add("Error: ExportAscii method requires ContentName")
                            Else
                                result = Controllers.exportAsciiController.exportAscii_GetAsciiExport(Me, ContentName, PageSize, PageNumber)
                                webServer.blockClosePageCopyright = True
                                blockClosePageLink = True
                                exitNow = True
                            End If
                        End If
                    Case HardCodedPagePayPalConfirm
                        '
                        ' -- Should be a remote method in commerce
                        Dim ConfirmOrderID As Integer = docProperties.getInteger("item_name")
                        If ConfirmOrderID <> 0 Then
                            '
                            ' Confirm the order
                            '
                            Dim CS As Integer = db.cs_open("Orders", "(ID=" & ConfirmOrderID & ") and ((OrderCompleted=0)or(OrderCompleted is Null))")
                            If db.cs_ok(CS) Then
                                Call db.cs_set(CS, "OrderCompleted", True)
                                Call db.cs_set(CS, "DateCompleted", profileStartTime)
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
                            Dim Recipient As String = siteProperties.getText("EmailOrderNotifyAddress", siteProperties.emailAdmin)
                            If genericController.vbInstr(genericController.encodeText(Recipient), "@") = 0 Then
                                Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError12("Init", "PayPal confirmation Order Process Notification email was not sent because EmailOrderNotifyAddress SiteProperty is not valid")
                            Else
                                Dim Sender As String = siteProperties.getText("EmailOrderFromAddress")
                                Dim subject As String = webServer.requestDomain & " Online Order Pending, #" & ConfirmOrderID
                                Dim Message As String = "<p>An order confirmation has been recieved from PayPal for " & webServer.requestDomain & "</p>"
                                Call email.send_Legacy(Recipient, Sender, subject, Message, , False, True)
                            End If
                        End If
                        webServer.blockClosePageCopyright = True
                        blockClosePageLink = True
                        exitNow = True
                End Select
            Catch ex As Exception
                handleException(ex)
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
                docGuid = genericController.createGuid()
                profileStartTickCount = GetTickCount
                CPTickCountBase = GetTickCount
                'closePageCounter = 0
                allowDebugLog = True
                profileStartTime = DateTime.Now()
                testPointPrinting = True
                '
                ' -- attempt auth load
                If (serverConfig.appConfig Is Nothing) Then
                    '
                    ' -- server mode, there is no application
                    authContext = Models.Context.authContextModel.create(Me, False)
                ElseIf ((serverConfig.appConfig.appMode <> Models.Entity.serverConfigModel.appModeEnum.normal) Or (serverConfig.appConfig.appStatus <> Models.Entity.serverConfigModel.appStatusEnum.ready)) Then
                    '
                    ' -- application is not ready, might be error, or in maintainence mode
                    authContext = Models.Context.authContextModel.create(Me, False)
                Else
                    authContext = Models.Context.authContextModel.create(Me, siteProperties.allowVisitTracking)
                    '
                    ' debug printed defaults on, so if not on, set it off and clear what was collected
                    If Not visitProperty.getBoolean("AllowDebugging") Then
                        testPointPrinting = False
                        testPointMessage = ""
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
            If Not Me.disposed Then
                Me.disposed = True
                If disposing Then
                    '
                    ' call .dispose for managed objects
                    '
                    ' ----- Block all output from underlying routines
                    '
                    blockExceptionReporting = True
                    continueProcessing = False
                    '
                    ' -- save addoncache
                    If (_assemblySkipList IsNot Nothing) Then
                        If (_assemblySkipList.Count > _assemblySkipList_CountWhenLoaded) Then
                            cache.setObject(cacheNameAssemblySkipList, _assemblySkipList)
                        End If
                    End If
                    '
                    ' content server object is valid
                    '
                    If (serverConfig IsNot Nothing) Then
                        If (serverConfig.appConfig IsNot Nothing) Then
                            If (serverConfig.appConfig.appMode = serverConfigModel.appModeEnum.normal) And (serverConfig.appConfig.appStatus = serverConfigModel.appStatusEnum.ready) Then
                                If siteProperties.allowVisitTracking Then
                                    '
                                    ' If visit tracking, save the viewing record
                                    '
                                    Dim ViewingName As String = Left(authContext.visit.id & "." & authContext.visit.PageVisits, 10)
                                    Dim PageID As Integer = 0
                                    If (_doc IsNot Nothing) Then
                                        If (doc.page IsNot Nothing) Then
                                            PageID = doc.page.id
                                        End If
                                    End If
                                    '
                                    ' -- convert requestFormDict to a name=value string for Db storage
                                    Dim requestFormSerialized As String = genericController.convertNameValueDictToREquestString(webServer.requestFormDict)
                                    Dim SQL As String = "INSERT INTO ccViewings (" _
                                        & "Name,VisitId,MemberID,Host,Path,Page,QueryString,Form,Referer,DateAdded,StateOK,ContentControlID,pagetime,Active,CreateKey,RecordID,ExcludeFromAnalytics,pagetitle" _
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
                                        & "," & db.encodeSQLDate(profileStartTime) _
                                        & "," & db.encodeSQLBoolean(authContext.visit_stateOK) _
                                        & "," & db.encodeSQLNumber(metaData.getContentId("Viewings")) _
                                        & "," & db.encodeSQLNumber(appStopWatch.ElapsedMilliseconds) _
                                        & ",1" _
                                        & "," & db.encodeSQLNumber(0) _
                                        & "," & db.encodeSQLNumber(PageID)
                                    SQL &= "," & db.encodeSQLBoolean(webServer.pageExcludeFromAnalytics)
                                    SQL &= "," & db.encodeSQLText(doc.metaContent_Title)
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
