Imports Contensive.BaseClasses
Imports System.Runtime.InteropServices

Namespace Contensive.Core
    '
    ' comVisible to be activeScript compatible
    '
    <ComVisible(True)> _
    <ComClass(CPRequestClass.ClassId, CPRequestClass.InterfaceId, CPRequestClass.EventsId)> _
    Public Class CPRequestClass
        Inherits BaseClasses.CPRequestBaseClass
        Implements IDisposable
        '
#Region "COM GUIDs"
        Public Const ClassId As String = "EF7782C1-76E4-45A7-BF30-E1CEBCBC56CF"
        Public Const InterfaceId As String = "39D6A73F-C11A-44F4-8405-A4CE3FB0A486"
        Public Const EventsId As String = "C8938AB2-26F0-41D2-A282-3313FD7BA490"
#End Region
        '
        Private cpCore As Contensive.Core.cpCoreClass
        Protected disposed As Boolean = False
        '
        ' Constructor
        '
        Public Sub New(ByVal cpCoreObj As Contensive.Core.cpCoreClass)
            MyBase.New()
            cpCore = cpCoreObj
        End Sub
        '
        ' dispose
        '
        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                Call appendDebugLog(".dispose, dereference main, csv")
                If disposing Then
                    '
                    ' call .dispose for managed objects
                    '
                    cpCore = Nothing
                End If
                '
                ' Add code here to release the unmanaged resource.
                '
            End If
            Me.disposed = True
        End Sub

        Public Overrides ReadOnly Property Browser() As String 'Inherits BaseClasses.CPRequestBaseClass.Browser
            Get
                If True Then
                    Return cpCore.requestBrowser
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property BrowserIsIE() As Boolean 'Inherits BaseClasses.CPRequestBaseClass.BrowserIsIE
            Get
                If True Then
                    Return cpCore.main_VisitBrowserIsIE
                Else
                    Return False
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property BrowserIsMac() As Boolean 'Inherits BaseClasses.CPRequestBaseClass.BrowserIsMac
            Get
                If True Then
                    Return cpCore.main_VisitBrowserIsMac
                Else
                    Return False
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property BrowserIsMobile() As Boolean 'Inherits BaseClasses.CPRequestBaseClass.BrowserIsMobile
            Get
                If True Then
                    Return cpCore.main_VisitBrowserIsMobile
                Else
                    Return False
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property BrowserIsWindows() As Boolean 'Inherits BaseClasses.CPRequestBaseClass.BrowserIsWindows
            Get
                If True Then
                    Return cpCore.main_VisitBrowserIsWindows
                Else
                    Return False
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property BrowserVersion() As String 'Inherits BaseClasses.CPRequestBaseClass.BrowserVersion
            Get
                If True Then
                    Return cpCore.main_VisitBrowserVersion
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides Function Cookie(ByVal CookieName As String) As String 'Inherits BaseClasses.CPRequestBaseClass.Cookie
            If True Then
                Return cpCore.main_GetStreamCookie(CookieName)
            Else
                Return ""
            End If
        End Function

        Public Overrides ReadOnly Property CookieString() As String 'Inherits BaseClasses.CPRequestBaseClass.CookieString
            Get
                If True Then
                    Return cpCore.requestCookies
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property Form() As String 'Inherits BaseClasses.CPRequestBaseClass.Form
            Get
                If True Then
                    Return cpCore.requestForm
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property FormAction() As String 'Inherits BaseClasses.CPRequestBaseClass.FormAction
            Get
                If True Then
                    Return cpCore.main_ServerFormActionURL
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides Function GetBoolean(ByVal RequestName As String) As Boolean 'Inherits BaseClasses.CPRequestBaseClass.GetBoolean
            If True Then
                Return cpCore.main_GetStreamBoolean2(RequestName)
            Else
                Return False
            End If
        End Function

        Public Overrides Function GetDate(ByVal RequestName As String) As Date 'Inherits BaseClasses.CPRequestBaseClass.GetDate
            If True Then
                Return cpCore.main_GetStreamDate(RequestName)
            Else
                Return New Date
            End If
        End Function

        Public Overrides Function GetInteger(ByVal RequestName As String) As Integer 'Inherits BaseClasses.CPRequestBaseClass.GetInteger
            If True Then
                Return cpCore.main_GetStreamInteger(RequestName)
            Else
                Return 0
            End If
        End Function

        Public Overrides Function GetNumber(ByVal RequestName As String) As Double 'Inherits BaseClasses.CPRequestBaseClass.GetNumber
            If True Then
                Return cpCore.main_GetStreamNumber2(RequestName)
            Else
                Return 0
            End If
        End Function

        Public Overrides Function GetText(ByVal RequestName As String) As String 'Inherits BaseClasses.CPRequestBaseClass.GetText
            If True Then
                Return cpCore.main_GetStreamText(RequestName)
            Else
                Return ""
            End If
        End Function

        Public Overrides ReadOnly Property Host() As String 'Inherits BaseClasses.CPRequestBaseClass.Host
            Get
                If True Then
                    Return cpCore.requestDomain
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property HTTPAccept() As String 'Inherits BaseClasses.CPRequestBaseClass.HTTPAccept
            Get
                If True Then
                    Return cpCore.requestHttpAccept
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property HTTPAcceptCharset() As String 'Inherits BaseClasses.CPRequestBaseClass.HTTPAcceptCharset
            Get
                If True Then
                    Return cpCore.requestHttpAcceptCharset
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property HTTPProfile() As String 'Inherits BaseClasses.CPRequestBaseClass.HTTPProfile
            Get
                If True Then
                    Return cpCore.requestHttpProfile
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property HTTPXWapProfile() As String 'Inherits BaseClasses.CPRequestBaseClass.HTTPXWapProfile
            Get
                If True Then
                    Return cpCore.requestxWapProfile
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property Language() As String 'Inherits BaseClasses.CPRequestBaseClass.Language
            Get
                If True Then
                    Return cpCore.userLanguage
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property Link() As String 'Inherits BaseClasses.CPRequestBaseClass.Link
            Get
                If True Then
                    Return cpCore.main_ServerLink
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property LinkForwardSource() As String 'Inherits BaseClasses.CPRequestBaseClass.LinkForwardSource
            Get
                If True Then
                    Return cpCore.main_LinkForwardSource
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property LinkSource() As String 'Inherits BaseClasses.CPRequestBaseClass.LinkSource
            Get
                If True Then
                    Return cpCore.main_ServerLinkSource
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property Page() As String 'Inherits BaseClasses.CPRequestBaseClass.Page
            Get
                If True Then
                    Return cpCore.main_ServerPage
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property Path() As String 'Inherits BaseClasses.CPRequestBaseClass.Path
            Get
                If True Then
                    Return cpCore.main_ServerPath
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property PathPage() As String 'Inherits BaseClasses.CPRequestBaseClass.PathPage
            Get
                If True Then
                    Return cpCore.requestPathPage
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property Protocol() As String 'Inherits BaseClasses.CPRequestBaseClass.Protocol
            Get
                If True Then
                    Return cpCore.main_ServerProtocol
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property QueryString() As String 'Inherits BaseClasses.CPRequestBaseClass.QueryString
            Get
                If True Then
                    Return cpCore.requestQueryString
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property Referer() As String 'Inherits BaseClasses.CPRequestBaseClass.Referer
            Get
                If True Then
                    Return cpCore.main_ServerReferer
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property RemoteIP() As String 'Inherits BaseClasses.CPRequestBaseClass.RemoteIP
            Get
                If True Then
                    Return cpCore.requestRemoteIP
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property Secure() As Boolean 'Inherits BaseClasses.CPRequestBaseClass.Secure
            Get
                If True Then
                    Return cpCore.requestSecure
                Else
                    Return False
                End If
            End Get
        End Property

        Public Overrides Function OK(ByVal RequestName As String) As Boolean 'Inherits BaseClasses.CPRequestBaseClass.OK
            If True Then
                Return cpCore.main_InStream(RequestName)
            Else
                Return False
            End If
        End Function
        '
        '
        '
        Private Sub appendDebugLog(ByVal copy As String)
            'My.Computer.FileSystem.WriteAllText("c:\clibCpDebug.log", Now & " - cp.request, " & copy & vbCrLf, True)
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
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
        Protected Overrides Sub Finalize()
            Dispose(False)
            MyBase.Finalize()
        End Sub
#End Region
    End Class
End Namespace