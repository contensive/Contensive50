
Option Explicit On
Option Strict On

Imports System.Runtime.InteropServices
Imports Contensive.BaseClasses
Imports Xunit

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
        '   cp creates and uses and instance of cpCore (all heavy lifting)
        '
        Public core As Contensive.Core.coreClass
        '
        Private MyAddonID As Integer
        '
        '=========================================================================================================
        ''' <summary>
        ''' cp constructor for cluster use. Provide the authentication token for cluster authorization.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            MyBase.New()
            core = New coreClass(Me)
        End Sub
        '
        '=========================================================================================================
        ''' <summary>
        ''' cp constructor for app, non-Internet use. Provide the authentication token for cluster authorization.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New(appName As String)
            MyBase.New()
            core = New coreClass(Me, appName)
        End Sub
        '
        '=========================================================================================================
        ''' <summary>
        ''' cp constructor for application use associated to a web request/response. Provide the authentication token which authorizes access to this application.
        ''' </summary>
        ''' <param name="httpContext"></param>
        ''' <remarks></remarks>
        Public Sub New(appName As String, httpContext As System.Web.HttpContext)
            MyBase.New()
            core = New coreClass(Me, appName, httpContext)
        End Sub
        '
        '
        '
        Public ReadOnly Property status As applicationStatusEnum
            Get
                Return core.appStatus
            End Get
        End Property
        '
        '
        '
        Public ReadOnly Property statusMessage As String
            Get
                Return GetApplicationStatusMessage(core.appStatus)
            End Get
        End Property
        '
        '
        '
        Public ReadOnly Property clusterOk As Boolean
            Get
                If (core Is Nothing) Then
                    Return False
                Else
                    Return core.cluster.ok
                End If
            End Get
        End Property
        '
        '
        '
        Public ReadOnly Property appOk As Boolean
            Get
                If (core Is Nothing) Then
                    Return False
                Else
                    Return (core.appStatus = applicationStatusEnum.ApplicationStatusReady)
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
                Site.ErrorReport(ex, "Unexpected error in cp.executeRoute()")
            End Try
            Return result
        End Function
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
                result = core.addon.execute_legacy4(addonNameOrGuid, core.getLegacyOptionStringFromVar(), addonContext, Nothing)
            Catch ex As Exception
                Site.ErrorReport(ex, "Unexpected error in cp.executeRoute()")
            End Try
            Return result
        End Function
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
                If Response.isOpen Then
                    'If Response.isOpen Then
                    '    result = core.addon.addon_execute_legacy4(addonId.ToString(), core.getLegacyOptionStringFromVar(), addonContext, Nothing)
                    'End If
                    result = core.addon.execute(addonId, "", "", addonContext, "", 0, "", "", False, 0, "", False, Nothing, "", Nothing, "", core.user.id, core.visit_isAuthenticated)
                End If
                '
            Catch ex As Exception
                Site.ErrorReport(ex, "Unexpected error in cp.executeRoute()")
            End Try
            Return result
        End Function
        '
        '
        '
        Public Sub AddVar(ByVal OptionName As String, ByVal OptionValue As String)
            Try
                If OptionName <> "" Then
                    'Call appendDebugLog("addVar, calling doc.var to save [" & OptionName & "] as [" & OptionValue & "]")
                    Me.Doc.Var(OptionName) = OptionValue
                End If
            Catch ex As Exception
                Site.ErrorReport(ex, "Unexpected error in AddVar()")
            End Try
        End Sub
        '
        '
        '
        Public Overrides Function BlockNew() As CPBlockBaseClass
            BlockNew = New CPBlockClass(Me)
        End Function
        '
        '
        '
        Public Overrides Function CSNew() As CPCSBaseClass
            CSNew = New CPCSClass(Me)
        End Function
        '
        '
        '
        Public Overrides ReadOnly Property Version() As String
            Get
                Return core.common_version()
            End Get
        End Property
        '
        ' Implement Cp.UserError
        '
        Public Overrides ReadOnly Property UserError() As CPUserErrorBaseClass 'Inherits BaseClasses.CPBaseClass.UserError
            Get
                If _userErrorObj Is Nothing Then
                    _userErrorObj = New CPUserErrorClass(core)
                End If
                Return _userErrorObj
            End Get
        End Property
        Private _userErrorObj As CPUserErrorClass
        '
        ' Implement Cp.Visitor
        '
        Public Overrides ReadOnly Property User() As CPUserBaseClass 'Inherits BaseClasses.CPBaseClass.User
            Get
                If _userObj Is Nothing Then
                    _userObj = New CPUserClass(core, Me)
                End If
                Return _userObj
            End Get
        End Property
        Private _userObj As CPUserClass
        '
        '====================================================================================================
        ''' <summary>
        ''' Returns the cluster services object, used for roles like housekeeping, server. etc. May require a permission key, to-be-implemented
        ''' </summary>
        ''' <param name="permissionKey"></param>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property cluster() As coreClusterClass
            Get
                '
                ' core blocks this property if not authCluster
                '
                Return core.cluster
            End Get
        End Property
        '
        ' append to logfile
        '
        Private Sub appendDebugLog(ByVal copy As String)
            Console.WriteLine("CPClass-" & copy)
            'My.Computer.FileSystem.WriteAllText("c:\clibCpDebug.log", Now & " - cp, " & copy & vbCrLf, True)
            ' 'My.Computer.FileSystem.WriteAllText(System.AppDocmc.main_CurrentDocmc.main_BaseDirectory() & "cpLog.txt", Now & " - " & copy & vbCrLf, True)
        End Sub
        '
        ' Implement Cp.Addon
        '
        Public Overrides ReadOnly Property Addon() As CPAddonBaseClass 'Inherits BaseClasses.CPBaseClass.Addon
            Get
                If _addonObj Is Nothing Then
                    _addonObj = New CPAddonClass(Me)
                End If
                Return _addonObj
            End Get
        End Property
        Private _addonObj As CPAddonClass
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
        ' Implement Cp.Cache
        '
        Public Overrides ReadOnly Property Cache() As CPCacheBaseClass 'Inherits BaseClasses.CPBaseClass.Cache
            Get
                If _cacheObj Is Nothing Then
                    _cacheObj = New CPCacheClass(Me)
                End If
                Return _cacheObj
            End Get
        End Property
        Private _cacheObj As CPCacheClass
        '
        ' Implement Cp.Content
        '
        Public Overrides ReadOnly Property Content() As CPContentBaseClass 'Inherits BaseClasses.CPBaseClass.Content
            Get
                If _contentObj Is Nothing Then
                    _contentObj = New CPContentClass(Me)
                End If
                Return _contentObj
            End Get
        End Property
        Private _contentObj As CPContentClass
        '
        ' Implement Cp.Context
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
        ' Implement Cp.Db
        '
        Public Overrides ReadOnly Property Db() As CPDbBaseClass 'Inherits BaseClasses.CPBaseClass.Db
            Get
                If _dbObj Is Nothing Then
                    _dbObj = New CPDbClass(Me)
                End If
                Return _dbObj
            End Get
        End Property
        Private _dbObj As CPDbClass
        '
        ' CP.Doc
        '
        Public Overrides ReadOnly Property Doc() As CPDocBaseClass  'Inherits BaseClasses.CPBaseClass.Doc
            Get
                If _docObj Is Nothing Then
                    _docObj = New CPDocClass(Me)
                End If
                Return _docObj
            End Get
        End Property
        Private _docObj As CPDocClass
        '
        ' Implement Cp.Email
        '
        Public Overrides ReadOnly Property Email() As CPEmailBaseClass 'Inherits BaseClasses.CPBaseClass.Email
            Get
                If _emailObj Is Nothing Then
                    _emailObj = New CPEmailClass(core)
                End If
                Return _emailObj
            End Get
        End Property
        Private _emailObj As CPEmailClass

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
        ' Implement Cp.Group
        '
        Public Overrides ReadOnly Property Group() As CPGroupBaseClass 'Inherits BaseClasses.CPBaseClass.Group
            Get
                If _groupObj Is Nothing Then
                    _groupObj = New CPGroupClass(Me)
                End If
                Return _groupObj
            End Get
        End Property
        Private _groupObj As CPGroupClass
        '
        ' Implement Cp.Html
        '
        Public Overrides ReadOnly Property Html() As CPHtmlBaseClass 'Inherits BaseClasses.CPBaseClass.Html
            Get
                If _htmlObj Is Nothing Then
                    _htmlObj = New CPHtmlClass(Me)
                End If
                Return _htmlObj
            End Get
        End Property
        '
        Private _htmlObj As CPHtmlClass
        Public Overrides ReadOnly Property MyAddon() As CPAddonBaseClass 'Inherits BaseClasses.CPBaseClass.MyAddon
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
        ' Implement Cp.Request
        '
        Public Overrides ReadOnly Property Request() As CPRequestBaseClass 'Inherits BaseClasses.CPBaseClass.Request
            Get
                If _requestObj Is Nothing Then
                    _requestObj = New CPRequestClass(core)
                End If
                Return _requestObj
            End Get
        End Property
        Private _requestObj As CPRequestClass
        '
        ' Implement Cp.Response
        '
        Public Overrides ReadOnly Property Response() As CPResponseBaseClass 'Inherits BaseClasses.CPBaseClass.Response
            Get
                If _responseObj Is Nothing Then
                    _responseObj = New CPResponseClass(core)
                End If
                Return _responseObj
            End Get
        End Property
        Private _responseObj As CPResponseClass
        '
        ' CP.Site
        '
        Public Overrides ReadOnly Property Site() As CPSiteBaseClass 'Inherits BaseClasses.CPBaseClass.Site
            Get
                If _siteObj Is Nothing Then
                    _siteObj = New CPSiteClass(core, Me)
                End If
                Return _siteObj
            End Get
        End Property
        Private _siteObj As CPSiteClass
        '
        ' Implement Cp.Utils
        '
        Public Overrides ReadOnly Property Utils() As CPUtilsBaseClass 'Inherits BaseClasses.CPBaseClass.Utils
            Get
                If _utilsObj Is Nothing Then
                    _utilsObj = New CPUtilsClass(Me)
                End If
                Return _utilsObj
            End Get
        End Property
        Private _utilsObj As CPUtilsClass
        '
        ' Implement Cp.Visit
        '
        Public Overrides ReadOnly Property Visit() As CPVisitBaseClass 'Inherits BaseClasses.CPBaseClass.Visit
            Get
                If _visitObj Is Nothing Then
                    _visitObj = New CPVisitClass(core, Me)
                End If
                Return _visitObj
            End Get
        End Property
        Private _visitObj As CPVisitClass
        '
        ' Implement Cp.Visitor
        '
        Public Overrides ReadOnly Property Visitor() As CPVisitorBaseClass 'Inherits BaseClasses.CPBaseClass.Visitor
            Get
                If _visitorObj Is Nothing Then
                    _visitorObj = New CPVisitorClass(core, Me)
                End If
                Return _visitorObj
            End Get
        End Property
        Private _visitorObj As CPVisitorClass
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
                End If
                '
                ' cleanup non-managed objects
                '
            End If
        End Sub
#End Region
    End Class
    '
    '====================================================================================================
    ' unit tests
    '
    Public Class CPClassUnitTests
        '
        '====================================================================================================
        ' unit test - cp.addVar
        '
        <Fact> Public Sub cp_AddVar_unit()
            ' arrange
            Dim cp As New CPClass()
            Dim cpApp As New CPClass("testapp")
            ' act
            cp.AddVar("a", "1")
            cp.AddVar("b", "2")
            cp.AddVar("b", "3")
            cpApp.AddVar("a", "4")
            cpApp.AddVar("b", "5")
            For ptr = 1 To 10
                cpApp.AddVar("key" & ptr.ToString, "value" & ptr.ToString())
            Next
            ' assert
            Assert.Equal(cp.Doc.GetText("a"), "1")
            Assert.Equal(cp.Doc.GetText("b"), "3")
            Assert.Equal(cpApp.Doc.GetText("a"), "4")
            Assert.Equal(cpApp.Doc.GetText("b"), "5")
            For ptr = 1 To 10
                Assert.Equal(cpApp.Doc.GetText("key" & ptr.ToString), "value" & ptr.ToString())
            Next
            ' dispose
            cp.Dispose()
            cpApp.Dispose()
        End Sub
        '
        '====================================================================================================
        ' unit test - cp.appOk
        '
        <Fact> Public Sub cp_AppOk_unit()
            ' arrange
            Dim cp As New CPClass()
            Dim cpApp As New CPClass("testapp")
            ' act
            ' assert
            Assert.Equal(cp.appOk, False)
            Assert.Equal(cpApp.appOk, True)
            ' dispose
            cp.Dispose()
            cpApp.Dispose()
        End Sub

        '
        '====================================================================================================
        ' unit test - sample
        '
        <Fact> Public Sub cp_sample_unit()
            ' arrange
            Dim cp As New CPClass()
            ' act
            '
            ' assert
            Assert.Equal(cp.appOk, False)
            ' dispose
            cp.Dispose()
        End Sub
    End Class
End Namespace