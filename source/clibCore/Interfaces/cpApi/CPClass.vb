
Option Explicit On
Option Strict On

Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
Imports System.Runtime.InteropServices
Imports Contensive.BaseClasses


Namespace Contensive.Core
    '
    ' comVisible to be activeScript compatible
    '
    <ComVisible(True)>
    <ComClass(CPClass.ClassId, CPClass.InterfaceId, CPClass.EventsId)>
    Public Class CPClass
        Inherits CPBaseClass
        Implements IDisposable
#Region "COM GUIDs"
        Public Const ClassId As String = "2EF01C6F-5288-411D-A5DE-76C8923CE1D3"
        Public Const InterfaceId As String = "58E04B36-2C75-4D11-9A8D-22A52E8417EB"
        Public Const EventsId As String = "4FADD1C2-6A89-4A8E-ADD0-9850D3EB6DBC"
#End Region
        '
        Public Property core As Contensive.Core.coreClass
        Private Property MyAddonID As Integer
        '
        '=========================================================================================================
        ''' <summary>
        ''' constructor for server use. No application context will be available. Use to create new apps.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            MyBase.New()
            core = New coreClass(Me)
        End Sub
        '
        '=========================================================================================================
        ''' <summary>
        ''' constructor for non-Internet app use. Configuration read from programdata json
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New(appName As String)
            MyBase.New()
            core = New coreClass(Me, appName)
        End Sub
        '
        '=========================================================================================================
        ''' <summary>
        ''' constructor for non-Internet app use. Configuration provided manually
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New(serverConfig As Models.Entity.serverConfigModel)
            MyBase.New()
            core = New coreClass(Me, serverConfig)
        End Sub
        '
        '=========================================================================================================
        ''' <summary>
        ''' constructor for iis site use. Configuration provided manually (maybe from webconfig)
        ''' </summary>
        ''' <param name="httpContext"></param>
        ''' <remarks></remarks>
        Public Sub New(serverConfig As Models.Entity.serverConfigModel, httpContext As System.Web.HttpContext)
            MyBase.New()
            core = New coreClass(Me, serverConfig, httpContext)
        End Sub
        '
        '=========================================================================================================
        ''' <summary>
        ''' constructor for iis site use. Configuration read from programdata json
        ''' </summary>
        ''' <param name="httpContext"></param>
        ''' <remarks></remarks>
        Public Sub New(appName As String, httpContext As System.Web.HttpContext)
            MyBase.New()
            core = New coreClass(Me, appName, httpContext)
        End Sub
        '
        '=========================================================================================================
        '
        Public ReadOnly Property status As Models.Entity.serverConfigModel.appStatusEnum
            Get
                Return core.serverConfig.appConfig.appStatus
            End Get
        End Property
        '
        '=========================================================================================================
        '
        Public ReadOnly Property statusMessage As String
            Get
                Return GetApplicationStatusMessage(core.serverConfig.appConfig.appStatus)
            End Get
        End Property
        '
        '====================================================================================================
        ''' <summary>
        ''' returns true if the server config file is valid (currently only requires a valid db)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property serverOk As Boolean
            Get
                Dim result As Boolean = False
                If (core Is Nothing) Then
                    '
                ElseIf (core.serverConfig Is Nothing) Then
                    '
                Else
                    result = Not String.IsNullOrEmpty(core.serverConfig.defaultDataSourceAddress)
                End If
                Return result
            End Get
        End Property
        '
        '=========================================================================================================
        '
        Public ReadOnly Property appOk As Boolean
            Get
                If (core Is Nothing) Then
                    Return False
                Else
                    Return (core.serverConfig.appConfig.appStatus = Models.Entity.serverConfigModel.appStatusEnum.ready)
                End If
            End Get
        End Property
        '
        '==========================================================================================
        ''' <summary>
        ''' Executes a specific route. The route can be a remote method, link alias, admin route, etc. If the route is not provided, the default route set in the admin settings is used.
        ''' </summary>
        ''' <param name="route"></param>
        ''' <returns></returns>
        Public Function executeRoute(Optional route As String = "") As String
            Dim result As String = ""
            Try
                result = core.executeRoute(route)
            Catch ex As Exception
                Site.ErrorReport(ex)
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' executes an addon with the name or guid provided, in the context specified.
        ''' </summary>
        ''' <param name="addonNameOrGuid"></param>
        ''' <param name="addonContext"></param>
        ''' <returns></returns>
        Public Function executeAddon(addonNameOrGuid As String, Optional addonContext As Contensive.BaseClasses.CPUtilsBaseClass.addonContext = Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextSimple) As String
            Dim result As String = ""
            Try
                If genericController.isGuid(addonNameOrGuid) Then
                    core.addon.execute(Models.Entity.addonModel.create(core, addonNameOrGuid), New CPUtilsBaseClass.addonExecuteContext With {.addonType = addonContext, .errorCaption = addonNameOrGuid})
                Else
                    core.addon.execute(Models.Entity.addonModel.createByName(core, addonNameOrGuid), New CPUtilsBaseClass.addonExecuteContext With {.addonType = addonContext, .errorCaption = addonNameOrGuid})
                End If
                'result = core.addon.execute_legacy4(addonNameOrGuid, core.docProperties.getLegacyOptionStringFromVar(), addonContext, Nothing)
            Catch ex As Exception
                Site.ErrorReport(ex)
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' executes an addon with the id provided, in the context specified.
        ''' </summary>
        ''' <param name="addonId"></param>
        ''' <param name="addonContext"></param>
        ''' <returns></returns>
        Public Function executeAddon(addonId As Integer, Optional addonContext As Contensive.BaseClasses.CPUtilsBaseClass.addonContext = Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextSimple) As String
            Dim result As String = ""
            Try
                core.addon.execute(Models.Entity.addonModel.create(core, addonId), New CPUtilsBaseClass.addonExecuteContext With {.addonType = addonContext, .errorCaption = "id:" & addonId.ToString()})
                'If Response.isOpen Then
                '    result = core.addon.execute_legacy6(addonId, "", "", addonContext, "", 0, "", "", False, 0, "", False, Nothing, "", Nothing, "", core.authContext.user.id, core.authContext.visit.VisitAuthenticated)
                'End If
                '
            Catch ex As Exception
                Site.ErrorReport(ex)
            End Try
            Return result
        End Function
        '
        '=========================================================================================================
        '
        Public Sub AddVar(ByVal OptionName As String, ByVal OptionValue As String)
            Try
                If OptionName <> "" Then
                    Me.Doc.Var(OptionName) = OptionValue
                End If
            Catch ex As Exception
                Site.ErrorReport(ex)
            End Try
        End Sub
        '
        '=========================================================================================================
        '
        Public Overrides Function BlockNew() As CPBlockBaseClass
            BlockNew = New CPBlockClass(Me)
        End Function
        '
        '=========================================================================================================
        '
        Public Overrides Function CSNew() As CPCSBaseClass
            CSNew = New CPCSClass(Me)
        End Function
        '
        '=========================================================================================================
        '
        Public Overrides ReadOnly Property Version() As String
            Get
                Return core.codeVersion()
            End Get
        End Property
        '
        '=========================================================================================================
        '
        Public Overrides ReadOnly Property UserError() As CPUserErrorBaseClass
            Get
                If _userErrorObj Is Nothing Then
                    _userErrorObj = New CPUserErrorClass(core)
                End If
                Return _userErrorObj
            End Get
        End Property
        Private _userErrorObj As CPUserErrorClass
        '
        '=========================================================================================================
        '
        Public Overrides ReadOnly Property User() As CPUserBaseClass
            Get
                If _userObj Is Nothing Then
                    _userObj = New CPUserClass(core, Me)
                End If
                Return _userObj
            End Get
        End Property
        Private _userObj As CPUserClass
        '
        '=========================================================================================================
        '
        Public Overrides ReadOnly Property Addon() As CPAddonBaseClass
            Get
                If _addonObj Is Nothing Then
                    _addonObj = New CPAddonClass(Me)
                End If
                Return _addonObj
            End Get
        End Property
        Private _addonObj As CPAddonClass
        '
        '=========================================================================================================
        '
        Public Overrides ReadOnly Property cdnFiles() As CPFileSystemBaseClass
            Get
                If (_cdnFiles Is Nothing) Then
                    _cdnFiles = New CPFileSystemClass(core, core.cdnFiles)
                End If
                Return _cdnFiles
            End Get
        End Property
        Private _cdnFiles As CPFileSystemClass
        '
        '=========================================================================================================
        '
        Public Overrides ReadOnly Property Cache() As CPCacheBaseClass
            Get
                If _cacheObj Is Nothing Then
                    _cacheObj = New CPCacheClass(Me)
                End If
                Return _cacheObj
            End Get
        End Property
        Private _cacheObj As CPCacheClass
        '
        '=========================================================================================================
        '
        Public Overrides ReadOnly Property Content() As CPContentBaseClass
            Get
                If _contentObj Is Nothing Then
                    _contentObj = New CPContentClass(Me)
                End If
                Return _contentObj
            End Get
        End Property
        Private _contentObj As CPContentClass
        '
        '=========================================================================================================
        '
        Public ReadOnly Property Context() As CPContextClass
            Get
                If _contextObj Is Nothing Then
                    _contextObj = New CPContextClass(Me)
                End If
                Return _contextObj
            End Get
        End Property
        Private _contextObj As CPContextClass
        '
        '=========================================================================================================
        '
        Public Overrides ReadOnly Property Db() As CPDbBaseClass
            Get
                If _dbObj Is Nothing Then
                    _dbObj = New CPDbClass(Me)
                End If
                Return _dbObj
            End Get
        End Property
        Private _dbObj As CPDbClass
        '
        '=========================================================================================================
        '
        Public Overrides ReadOnly Property Doc() As CPDocBaseClass
            Get
                If _docObj Is Nothing Then
                    _docObj = New CPDocClass(Me)
                End If
                Return _docObj
            End Get
        End Property
        Private _docObj As CPDocClass
        '
        '====================================================================================================
        '
        Public Overrides ReadOnly Property Email() As CPEmailBaseClass
            Get
                If _emailObj Is Nothing Then
                    _emailObj = New CPEmailClass(core)
                End If
                Return _emailObj
            End Get
        End Property
        Private _emailObj As CPEmailClass
        '
        '====================================================================================================
        '
        Public Overrides ReadOnly Property File() As CPFileBaseClass
            Get
                If _fileObj Is Nothing Then
                    _fileObj = New CPFileClass(core)
                End If
                Return _fileObj
            End Get
        End Property
        Private _fileObj As CPFileClass
        '
        '====================================================================================================
        '
        Public Overrides ReadOnly Property Group() As CPGroupBaseClass
            Get
                If _groupObj Is Nothing Then
                    _groupObj = New CPGroupClass(Me)
                End If
                Return _groupObj
            End Get
        End Property
        Private _groupObj As CPGroupClass
        '
        '====================================================================================================
        '
        Public Overrides ReadOnly Property Html() As CPHtmlBaseClass
            Get
                If _htmlObj Is Nothing Then
                    _htmlObj = New CPHtmlClass(Me)
                End If
                Return _htmlObj
            End Get
        End Property
        Private _htmlObj As CPHtmlClass
        '
        '====================================================================================================
        '
        Public Overrides ReadOnly Property MyAddon() As CPAddonBaseClass
            Get
                If _myAddonObj Is Nothing Then
                    _myAddonObj = New CPAddonClass(Me)
                    _myAddonObj.Open(MyAddonID)
                End If
                Return _myAddonObj
            End Get
        End Property
        Private _myAddonObj As CPAddonClass
        '
        '====================================================================================================
        '
        Public Overrides ReadOnly Property privateFiles() As CPFileSystemBaseClass
            Get
                If (_privateFiles Is Nothing) Then
                    _privateFiles = New CPFileSystemClass(core, core.privateFiles)
                End If
                Return _privateFiles
            End Get
        End Property
        Private _privateFiles As CPFileSystemClass
        '
        '====================================================================================================
        '
        Public Overrides ReadOnly Property Request() As CPRequestBaseClass
            Get
                If _requestObj Is Nothing Then
                    _requestObj = New CPRequestClass(core)
                End If
                Return _requestObj
            End Get
        End Property
        Private _requestObj As CPRequestClass
        '
        '====================================================================================================
        '
        Public Overrides ReadOnly Property Response() As CPResponseBaseClass
            Get
                If _responseObj Is Nothing Then
                    _responseObj = New CPResponseClass(core)
                End If
                Return _responseObj
            End Get
        End Property
        Private _responseObj As CPResponseClass
        '
        '====================================================================================================
        '
        Public Overrides ReadOnly Property Site() As CPSiteBaseClass
            Get
                If _siteObj Is Nothing Then
                    _siteObj = New CPSiteClass(core, Me)
                End If
                Return _siteObj
            End Get
        End Property
        Private _siteObj As CPSiteClass
        '
        '====================================================================================================
        '
        Public Overrides ReadOnly Property Utils() As CPUtilsBaseClass
            Get
                If _utilsObj Is Nothing Then
                    _utilsObj = New CPUtilsClass(Me)
                End If
                Return _utilsObj
            End Get
        End Property
        Private _utilsObj As CPUtilsClass
        '
        '====================================================================================================
        '
        Public Overrides ReadOnly Property Visit() As CPVisitBaseClass
            Get
                If _visitObj Is Nothing Then
                    _visitObj = New CPVisitClass(core, Me)
                End If
                Return _visitObj
            End Get
        End Property
        Private _visitObj As CPVisitClass
        '
        '====================================================================================================
        '
        Public Overrides ReadOnly Property Visitor() As CPVisitorBaseClass
            Get
                If _visitorObj Is Nothing Then
                    _visitorObj = New CPVisitorClass(core, Me)
                End If
                Return _visitorObj
            End Get
        End Property
        Private _visitorObj As CPVisitorClass
        '
        '====================================================================================================
        '
        Public Overrides ReadOnly Property wwwFiles() As CPFileSystemBaseClass
            Get
                If (_appRootFiles Is Nothing) Then
                    _appRootFiles = New CPFileSystemClass(core, core.appRootFiles)
                End If
                Return _appRootFiles
            End Get
        End Property
        Private _appRootFiles As CPFileSystemClass
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
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
        '
        Protected Overrides Sub Finalize()
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
                    If Not (_addonObj Is Nothing) Then _addonObj.Dispose()
                    If Not (_cacheObj Is Nothing) Then _cacheObj.Dispose()
                    If Not (_contentObj Is Nothing) Then _contentObj.Dispose()
                    If Not (_contextObj Is Nothing) Then _contextObj.Dispose()
                    If Not (_dbObj Is Nothing) Then _dbObj.Dispose()
                    If Not (_docObj Is Nothing) Then _docObj.Dispose()
                    If Not (_emailObj Is Nothing) Then _emailObj.Dispose()
                    If Not (_fileObj Is Nothing) Then _fileObj.Dispose()
                    If Not (_groupObj Is Nothing) Then _groupObj.Dispose()
                    If Not (_htmlObj Is Nothing) Then _htmlObj.Dispose()
                    If Not (_myAddonObj Is Nothing) Then _myAddonObj.Dispose()
                    If Not (_requestObj Is Nothing) Then _requestObj.Dispose()
                    If Not (_responseObj Is Nothing) Then _responseObj.Dispose()
                    If Not (_siteObj Is Nothing) Then _siteObj.Dispose()
                    If Not (_userErrorObj Is Nothing) Then _userErrorObj.Dispose()
                    If Not (_userObj Is Nothing) Then _userObj.Dispose()
                    If Not (_utilsObj Is Nothing) Then _utilsObj.Dispose()
                    If Not (_visitObj Is Nothing) Then _visitObj.Dispose()
                    If Not (_visitorObj Is Nothing) Then _visitorObj.Dispose()
                    If Not (_cdnFiles Is Nothing) Then _cdnFiles.Dispose()
                    If Not (_appRootFiles Is Nothing) Then _appRootFiles.Dispose()
                    If Not (_privateFiles Is Nothing) Then _privateFiles.Dispose()
                    If Not (core Is Nothing) Then core.Dispose()
                End If
                '
                ' cleanup non-managed objects
                '
            End If
        End Sub
#End Region
    End Class

End Namespace