
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
        Private debugBlockCpCom As Boolean = False
        '
        Private AddonObj As CPAddonClass
        Private CacheObj As CPCacheClass
        Private ContentObj As CPContentClass
        Private ContextObj As CPContextClass
        Private DbObj As CPDbClass
        Private DocObj As CPDocClass
        Private EmailObj As CPEmailClass
        Private FileObj As CPFileClass
        Private GroupObj As CPGroupClass
        Private HtmlObj As CPHtmlClass
        Private MyAddonObj As CPAddonClass
        Private RequestObj As CPRequestClass
        Private ResponseObj As CPResponseClass
        Private SiteObj As CPSiteClass
        Private UserObj As CPUserClass
        Private UserErrorObj As CPUserErrorClass
        Private UtilsObj As CPUtilsClass
        Private VisitObj As CPVisitClass
        Private VisitorObj As CPVisitorClass
        ''
        '' 'Inherits the .thisaddon feature. before calling an addon that uses this BaseClass,
        '' the parent must 
        ''
        Private MyAddonID As Integer
        Private cpCredentialKey As String
        '
        '   cp creates and uses and instance of cpCore (all heavy lifting)
        '
        Public core As Contensive.Core.cpCoreClass
        '
        Protected disposed As Boolean = False
        '
        '=========================================================================================================
        ''' <summary>
        ''' cp constructor for cluster use. Provide the authentication token for cluster authorization.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            MyBase.New()
            core = New cpCoreClass(Me)
        End Sub
        '
        '=========================================================================================================
        ''' <summary>
        ''' cp constructor for app, non-Internet use. Provide the authentication token for cluster authorization.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New(appName As String)
            MyBase.New()
            core = New cpCoreClass(Me, appName)
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
            core = New cpCoreClass(Me, appName, httpContext)
        End Sub
        '
        ' dispose
        '
        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                Me.disposed = True
                If disposing Then
                    '
                    ' call .dispose for managed objects
                    '
                    'Call appendDebugLog("protected dispose, calling dispose on internal objects")
                    If Not (AddonObj Is Nothing) Then AddonObj.Dispose()
                    If Not (CacheObj Is Nothing) Then CacheObj.Dispose()
                    If Not (ContentObj Is Nothing) Then ContentObj.Dispose()
                    If Not (ContextObj Is Nothing) Then ContextObj.Dispose()
                    If Not (DbObj Is Nothing) Then DbObj.Dispose()
                    If Not (DocObj Is Nothing) Then DocObj.Dispose()
                    If Not (EmailObj Is Nothing) Then EmailObj.Dispose()
                    If Not (FileObj Is Nothing) Then FileObj.Dispose()
                    If Not (GroupObj Is Nothing) Then GroupObj.Dispose()
                    If Not (HtmlObj Is Nothing) Then HtmlObj.Dispose()
                    If Not (MyAddonObj Is Nothing) Then MyAddonObj.Dispose()
                    If Not (RequestObj Is Nothing) Then RequestObj.Dispose()
                    If Not (ResponseObj Is Nothing) Then ResponseObj.Dispose()
                    If Not (SiteObj Is Nothing) Then SiteObj.Dispose()
                    If Not (UserErrorObj Is Nothing) Then UserErrorObj.Dispose()
                    If Not (UserObj Is Nothing) Then UserObj.Dispose()
                    If Not (UtilsObj Is Nothing) Then UtilsObj.Dispose()
                    If Not (VisitObj Is Nothing) Then VisitObj.Dispose()
                    If Not (VisitorObj Is Nothing) Then VisitorObj.Dispose()
                    '
                    ' cp  creates and destroys cmc
                    '
                    If Not debugBlockCpCom Then
                        ' handle leak test
                        Call core.dispose()
                        core = Nothing
                    End If
                End If
                '
                GC.Collect()
                'appendDebugLog("CPCLASS.Dispose, exit")
            End If
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
        Public Function executeAddon(addonNameOrGuid As String, Optional addonContext As cpCoreClass.addonContextEnum = cpCoreClass.addonContextEnum.ContextSimple) As String
            Dim result As String = ""
            Try
                Dim MyDoc As CPDocClass = Doc
                Dim LegacyOptionString As String = MyDoc.getLegacyOptionStringFromVar()
                result = core.executeAddon_legacy4(addonNameOrGuid, LegacyOptionString, addonContext, Nothing)
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
        Public Function executeAddon(addonId As Integer, Optional addonContext As cpCoreClass.addonContextEnum = cpCoreClass.addonContextEnum.ContextSimple) As String
            Dim result As String = ""
            Try
                If Response.isOpen Then
                    If Response.isOpen Then
                        Dim MyDoc As CPDocClass = Doc
                        Dim LegacyOptionString As String = MyDoc.getLegacyOptionStringFromVar()
                        result = core.executeAddon_legacy4(addonId.ToString(), LegacyOptionString, addonContext, Nothing)
                    End If
                    result = core.executeAddon(addonId, "", "", addonContext, "", 0, "", "", False, 0, "", False, Nothing, "", Nothing, "", core.user.userId, core.visit_isAuthenticated)
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
                Return core.version()
            End Get
        End Property
        '
        ' Implement Cp.Group
        '
        Public Overrides ReadOnly Property Group() As CPGroupBaseClass 'Inherits BaseClasses.CPBaseClass.Group
            Get
                If GroupObj Is Nothing Then
                    GroupObj = New CPGroupClass(Me)
                End If
                Return GroupObj
            End Get
        End Property
        '
        ' Implement Cp.Request
        '
        Public Overrides ReadOnly Property Request() As CPRequestBaseClass 'Inherits BaseClasses.CPBaseClass.Request
            Get
                If RequestObj Is Nothing Then
                    RequestObj = New CPRequestClass(core)
                End If
                Return RequestObj
            End Get
        End Property
        '
        ' Implement Cp.Response
        '
        Public Overrides ReadOnly Property Response() As CPResponseBaseClass 'Inherits BaseClasses.CPBaseClass.Response
            Get
                If ResponseObj Is Nothing Then
                    ResponseObj = New CPResponseClass(core)
                End If
                Return ResponseObj
            End Get
        End Property
        '
        ' Implement Cp.UserError
        '
        Public Overrides ReadOnly Property UserError() As CPUserErrorBaseClass 'Inherits BaseClasses.CPBaseClass.UserError
            Get
                If UserErrorObj Is Nothing Then
                    UserErrorObj = New CPUserErrorClass(core)
                End If
                Return UserErrorObj
            End Get
        End Property
        '
        ' Implement Cp.Visit
        '
        Public Overrides ReadOnly Property Visit() As CPVisitBaseClass 'Inherits BaseClasses.CPBaseClass.Visit
            Get
                If VisitObj Is Nothing Then
                    VisitObj = New CPVisitClass(core, Me)
                End If
                Return VisitObj
            End Get
        End Property
        '
        ' Implement Cp.Visitor
        '
        Public Overrides ReadOnly Property Visitor() As CPVisitorBaseClass 'Inherits BaseClasses.CPBaseClass.Visitor
            Get
                If VisitorObj Is Nothing Then
                    VisitorObj = New CPVisitorClass(core, Me)
                End If
                Return VisitorObj
            End Get
        End Property
        '
        ' Implement Cp.Visitor
        '
        Public Overrides ReadOnly Property User() As CPUserBaseClass 'Inherits BaseClasses.CPBaseClass.User
            Get
                If UserObj Is Nothing Then
                    UserObj = New CPUserClass(core, Me)
                End If
                Return UserObj
            End Get
        End Property
        '
        ' Implement Cp.Html
        '
        Public Overrides ReadOnly Property Html() As CPHtmlBaseClass 'Inherits BaseClasses.CPBaseClass.Html
            Get
                If HtmlObj Is Nothing Then
                    HtmlObj = New CPHtmlClass(Me)
                End If
                Return HtmlObj
            End Get
        End Property
        '
        ' Implement Cp.Cache
        '
        Public Overrides ReadOnly Property Cache() As CPCacheBaseClass 'Inherits BaseClasses.CPBaseClass.Cache
            Get
                If CacheObj Is Nothing Then
                    CacheObj = New CPCacheClass(Me)
                End If
                Return CacheObj
            End Get
        End Property
        '
        ' Implement Cp.Db
        '
        Public Overrides ReadOnly Property Db() As CPDbBaseClass 'Inherits BaseClasses.CPBaseClass.Db
            Get
                If DbObj Is Nothing Then
                    DbObj = New CPDbClass(Me)
                End If
                Return DbObj
            End Get
        End Property
        '
        ' Implement Cp.Email
        '
        Public Overrides ReadOnly Property Email() As CPEmailBaseClass 'Inherits BaseClasses.CPBaseClass.Email
            Get
                If EmailObj Is Nothing Then
                    EmailObj = New CPEmailClass(core)
                End If
                Return EmailObj
            End Get
        End Property
        '
        ' Implement Cp.Content
        '
        Public Overrides ReadOnly Property Content() As CPContentBaseClass 'Inherits BaseClasses.CPBaseClass.Content
            Get
                If ContentObj Is Nothing Then
                    ContentObj = New CPContentClass(Me)
                End If
                Return ContentObj
            End Get
        End Property
        '
        ' Implement Cp.Context
        '
        Public ReadOnly Property Context() As CPContextClass
            Get
                If ContextObj Is Nothing Then
                    ContextObj = New CPContextClass(Me)
                End If
                Return ContextObj
            End Get
        End Property
        '
        ' Implement Cp.Addon
        '
        Public Overrides ReadOnly Property Addon() As CPAddonBaseClass 'Inherits BaseClasses.CPBaseClass.Addon
            Get
                If AddonObj Is Nothing Then
                    AddonObj = New CPAddonClass(Me)
                End If
                Return AddonObj
            End Get
        End Property
        '
        ' Implement Cp.Utils
        '
        Public Overrides ReadOnly Property Utils() As CPUtilsBaseClass 'Inherits BaseClasses.CPBaseClass.Utils
            Get
                If UtilsObj Is Nothing Then
                    UtilsObj = New CPUtilsClass(Me)
                End If
                Return UtilsObj
            End Get
        End Property
        '
        ' CP.Doc
        '
        Public Overrides ReadOnly Property Doc() As CPDocBaseClass  'Inherits BaseClasses.CPBaseClass.Doc
            Get
                If DocObj Is Nothing Then
                    DocObj = New CPDocClass(Me)
                End If
                Return DocObj
            End Get
        End Property
        '
        ' CP.Site
        '
        Public Overrides ReadOnly Property Site() As CPSiteBaseClass 'Inherits BaseClasses.CPBaseClass.Site
            Get
                If SiteObj Is Nothing Then
                    SiteObj = New CPSiteClass(core, Me)
                End If
                Return SiteObj
            End Get
        End Property

        Public Overrides ReadOnly Property MyAddon() As CPAddonBaseClass 'Inherits BaseClasses.CPBaseClass.MyAddon
            Get
                If MyAddonObj Is Nothing Then
                    MyAddonObj = New CPAddonClass(Me)
                    MyAddonObj.Open(MyAddonID)
                End If
                Return MyAddonObj
            End Get
        End Property

        Public Overrides ReadOnly Property File() As CPFileBaseClass
            Get
                If FileObj Is Nothing Then
                    FileObj = New CPFileClass(core)
                End If
                Return FileObj
            End Get
        End Property
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
        ' testpoint
        '
        Private Sub tp(ByVal msg As String)
            'Call appendDebugLog(msg)
        End Sub

#Region " IDisposable Support "
        ' Do not change or add Overridable to these methods.
        ' Put cleanup code in Dispose(ByVal disposing As Boolean).
        Public Overloads Sub Dispose() Implements IDisposable.Dispose
            'appendDebugLog("public dispose")
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
        Protected Overrides Sub Finalize()
            'appendDebugLog("finalize")
            Dispose(False)
            MyBase.Finalize()
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
            For ptr = 1 To 1000
                cpApp.AddVar("key" & ptr.ToString, "value" & ptr.ToString())
            Next
            ' assert
            Assert.Equal(cp.Doc.GetText("a"), "1")
            Assert.Equal(cp.Doc.GetText("b"), "3")
            Assert.Equal(cpApp.Doc.GetText("a"), "4")
            Assert.Equal(cpApp.Doc.GetText("b"), "5")
            For ptr = 1 To 1000
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