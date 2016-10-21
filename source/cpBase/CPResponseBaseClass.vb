'
' documentation should be in a new project that inherits these classes. The class names should be the object names in the actual cp project
'
Namespace Contensive.BaseClasses
    Public MustInherit Class CPResponseBaseClass
        'Public Sub New(ByVal cmcObj As Contensive.Core.cpCoreClass, ByRef CPParent As CPBaseClass)
        Public MustOverride Property ContentType() As String 'Implements BaseClasses.CPResponseBaseClass.ContentType
        Public MustOverride ReadOnly Property Cookies() As String 'Implements BaseClasses.CPResponseBaseClass.Cookies
        Public MustOverride ReadOnly Property Header() As String 'Implements BaseClasses.CPResponseBaseClass.Header
        Public MustOverride Sub Clear() 'Implements BaseClasses.CPResponseBaseClass.Clear
        Public MustOverride Sub Close() 'Implements BaseClasses.CPResponseBaseClass.Close
        Public MustOverride Sub AddHeader(ByVal HeaderName As String, ByVal HeaderValue As String) 'Implements BaseClasses.CPResponseBaseClass.AddHeader
        Public MustOverride Sub Flush() 'Implements BaseClasses.CPResponseBaseClass.Flush
        Public MustOverride Sub Redirect(ByVal Link As String) 'Implements BaseClasses.CPResponseBaseClass.Redirect
        Public MustOverride Sub SetBuffer(ByVal BufferOn As Boolean) 'Implements BaseClasses.CPResponseBaseClass.SetBuffer
        Public MustOverride Sub SetStatus(ByVal status As String) 'Implements BaseClasses.CPResponseBaseClass.SetStatus
        Public MustOverride Sub SetTimeout(ByVal TimeoutSeconds As String) 'Implements BaseClasses.CPResponseBaseClass.SetTimeout
        Public MustOverride Sub SetType(ByVal ContentType As String) 'Implements BaseClasses.CPResponseBaseClass.SetType
        Public MustOverride Sub SetCookie(ByVal CookieName As String, ByVal CookieValue As String, Optional ByVal DateExpires As Date = #12:00:00 AM#, Optional ByVal Domain As String = "", Optional ByVal Path As String = "", Optional ByVal Secure As Boolean = False) 'Implements BaseClasses.CPResponseBaseClass.SetCookie
        Public MustOverride Sub Write(ByVal content As String)
        ''' <summary>
        ''' Is the response object available to write. False during background processes and after the html response has ended. For instance, when a remote method is returned the response is closed meaning no other data should be added to the output.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride ReadOnly Property isOpen() As Boolean
    End Class

End Namespace

