'
' documentation should be in a new project that inherits these classes. The class names should be the object names in the actual cp project
'
Namespace Contensive.BaseClasses
    Public MustInherit Class CPRequestBaseClass
        'Public Sub New(ByVal cmcObj As Contensive.Core.cpCoreClass, ByRef CPParent As CPBaseClass)
        Public MustOverride ReadOnly Property Browser() As String 'Implements BaseClasses.CPRequestBaseClass.Browser
        Public MustOverride ReadOnly Property BrowserIsIE() As Boolean 'Implements BaseClasses.CPRequestBaseClass.BrowserIsIE
        Public MustOverride ReadOnly Property BrowserIsMac() As Boolean 'Implements BaseClasses.CPRequestBaseClass.BrowserIsMac
        Public MustOverride ReadOnly Property BrowserIsMobile() As Boolean 'Implements BaseClasses.CPRequestBaseClass.BrowserIsMobile
        Public MustOverride ReadOnly Property BrowserIsWindows() As Boolean 'Implements BaseClasses.CPRequestBaseClass.BrowserIsWindows
        Public MustOverride ReadOnly Property BrowserVersion() As String 'Implements BaseClasses.CPRequestBaseClass.BrowserVersion
        Public MustOverride Function Cookie(ByVal CookieName As String) As String 'Implements BaseClasses.CPRequestBaseClass.Cookie
        Public MustOverride ReadOnly Property CookieString() As String 'Implements BaseClasses.CPRequestBaseClass.CookieString
        Public MustOverride ReadOnly Property Form() As String 'Implements BaseClasses.CPRequestBaseClass.Form
        Public MustOverride ReadOnly Property FormAction() As String 'Implements BaseClasses.CPRequestBaseClass.FormAction
        Public MustOverride Function GetBoolean(ByVal RequestName As String) As Boolean 'Implements BaseClasses.CPRequestBaseClass.GetBoolean
        Public MustOverride Function GetDate(ByVal RequestName As String) As Date 'Implements BaseClasses.CPRequestBaseClass.GetDate
        Public MustOverride Function GetInteger(ByVal RequestName As String) As Integer 'Implements BaseClasses.CPRequestBaseClass.GetInteger
        Public MustOverride Function GetNumber(ByVal RequestName As String) As Double 'Implements BaseClasses.CPRequestBaseClass.GetNumber
        Public MustOverride Function GetText(ByVal RequestName As String) As String 'Implements BaseClasses.CPRequestBaseClass.GetText
        Public MustOverride ReadOnly Property Host() As String 'Implements BaseClasses.CPRequestBaseClass.Host
        Public MustOverride ReadOnly Property HTTPAccept() As String 'Implements BaseClasses.CPRequestBaseClass.HTTPAccept
        Public MustOverride ReadOnly Property HTTPAcceptCharset() As String 'Implements BaseClasses.CPRequestBaseClass.HTTPAcceptCharset
        Public MustOverride ReadOnly Property HTTPProfile() As String 'Implements BaseClasses.CPRequestBaseClass.HTTPProfile
        Public MustOverride ReadOnly Property HTTPXWapProfile() As String 'Implements BaseClasses.CPRequestBaseClass.HTTPXWapProfile
        Public MustOverride ReadOnly Property Language() As String 'Implements BaseClasses.CPRequestBaseClass.Language
        Public MustOverride ReadOnly Property Link() As String 'Implements BaseClasses.CPRequestBaseClass.Link
        Public MustOverride ReadOnly Property LinkForwardSource() As String 'Implements BaseClasses.CPRequestBaseClass.LinkForwardSource
        Public MustOverride ReadOnly Property LinkSource() As String 'Implements BaseClasses.CPRequestBaseClass.LinkSource
        Public MustOverride ReadOnly Property Page() As String 'Implements BaseClasses.CPRequestBaseClass.Page
        Public MustOverride ReadOnly Property Path() As String 'Implements BaseClasses.CPRequestBaseClass.Path
        Public MustOverride ReadOnly Property PathPage() As String 'Implements BaseClasses.CPRequestBaseClass.PathPage
        Public MustOverride ReadOnly Property Protocol() As String 'Implements BaseClasses.CPRequestBaseClass.Protocol
        Public MustOverride ReadOnly Property QueryString() As String 'Implements BaseClasses.CPRequestBaseClass.QueryString
        Public MustOverride ReadOnly Property Referer() As String 'Implements BaseClasses.CPRequestBaseClass.Referer
        Public MustOverride ReadOnly Property RemoteIP() As String 'Implements BaseClasses.CPRequestBaseClass.RemoteIP
        Public MustOverride ReadOnly Property Secure() As Boolean 'Implements BaseClasses.CPRequestBaseClass.Secure
        Public MustOverride Function OK(ByVal RequestName As String) As Boolean 'Implements BaseClasses.CPRequestBaseClass.OK
    End Class

End Namespace

