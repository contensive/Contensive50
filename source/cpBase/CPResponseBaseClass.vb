'
' documentation should be in a new project that inherits these classes. The class names should be the object names in the actual cp project
'
Namespace Contensive.BaseClasses
    Public MustInherit Class CPResponseBaseClass
        'Public Sub New(ByVal cmcObj As Contensive.Core.cpCoreClass, ByRef CPParent As CPBaseClass)
        Public MustOverride Property ContentType() As String
        Public MustOverride ReadOnly Property Cookies() As String
        Public MustOverride ReadOnly Property Header() As String
        Public MustOverride Sub Clear()
        Public MustOverride Sub Close()
        Public MustOverride Sub AddHeader(ByVal HeaderName As String, ByVal HeaderValue As String)
        Public MustOverride Sub Flush()
        Public MustOverride Sub Redirect(ByVal Link As String)
        Public MustOverride Sub SetBuffer(ByVal BufferOn As Boolean)
        Public MustOverride Sub SetStatus(ByVal status As String)
        Public MustOverride Sub SetTimeout(ByVal TimeoutSeconds As String)
        Public MustOverride Sub SetType(ByVal ContentType As String)
        Public MustOverride Sub SetCookie(ByVal CookieName As String, ByVal CookieValue As String)
        Public MustOverride Sub SetCookie(ByVal CookieName As String, ByVal CookieValue As String, ByVal DateExpires As Date)
        Public MustOverride Sub SetCookie(ByVal CookieName As String, ByVal CookieValue As String, ByVal DateExpires As Date, ByVal Domain As String)
        Public MustOverride Sub SetCookie(ByVal CookieName As String, ByVal CookieValue As String, ByVal DateExpires As Date, ByVal Domain As String, ByVal Path As String)
        Public MustOverride Sub SetCookie(ByVal CookieName As String, ByVal CookieValue As String, ByVal DateExpires As Date, ByVal Domain As String, ByVal Path As String, ByVal Secure As Boolean)
        Public MustOverride Sub Write(ByVal content As String)
        Public MustOverride ReadOnly Property isOpen() As Boolean
    End Class

End Namespace

