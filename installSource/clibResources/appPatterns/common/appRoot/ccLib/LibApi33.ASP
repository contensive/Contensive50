
<%
'
'==============================================================================
'
'	This header file contains site wide definitions and is required
'	by all asp documents in the site.
'
'	This page and its contents are copyright by Kidwell McGowan Associates.
'
'=============================================================================
'
Dim ccLib
'
'========================================================================
' ----- Initialize the API
'========================================================================
'
Sub InitLibAPI
	on error goto 0
    '
    Dim NewKey
    Dim TotalBytes
    '
    ' setup IIS Response
    '
    Response.CacheControl = "no-cache"
    Response.Expires = -1
    Response.Buffer = True
    '
    ' Create Contensive object
    '
    Set ccLib = Server.CreateObject( "ccWeb3.MainClass" )
    '
    ' Setup Contensive from IIS Request
    '
    ccLib.ServerPathPage = CStr( Request.ServerVariables("SCRIPT_NAME"))
    ccLib.ServerReferrer = Request.ServerVariables("HTTP_REFERER")
    ccLib.ServerHost = Request.ServerVariables("SERVER_NAME")
    ccLib.BrowserLanguage = CStr( Request.ServerVariables("HTTP_ACCEPT_LANGUAGE"))
    ccLib.ServerPageSecure = cBool( Request.ServerVariables("SERVER_PORT_SECURE"))
    ccLib.PhysicalWWWPath = Server.MapPath("/")
    ccLib.PhysicalccLibPath = Server.MapPath( "/ccLib" )
    ccLib.VisitRemoteIP = CStr( Request.ServerVariables("REMOTE_ADDR"))
    ccLib.VisitBrowser = CStr( Request.ServerVariables("HTTP_USER_AGENT"))
    ccLib.HTTP_Accept = CStr( Request.ServerVariables("HTTP_ACCEPT"))
    ccLib.HTTP_Accept_charset = CStr( Request.ServerVariables("HTTP_ACCEPT_CHARSET"))
    ccLib.HTTP_Profile = CStr( Request.ServerVariables("HTTP_PROFILE"))
    ccLib.HTTP_X_Wap_Profile = CStr( Request.ServerVariables("HTTP_X_WAP_PROFILE"))
    '
    ' Create ServerQueryString
    '
    For Each NewKey In Request.QueryString
        ccLib.ServerQueryString = ccLib.ServerQueryString & "&" & CStr(NewKey) & "=" & Server.URLEncode( Request.QueryString(NewKey))
        ccLib.ReadStreamBinaryRead = ccLib.ReadStreamBinaryRead Or (ucase(CStr(NewKey)) = "REQUESTBINARY")
    Next
    If Len( ccLib.ServerQueryString ) > 0 then
        ccLib.ServerQueryString = Mid( ccLib.ServerQueryString, 2 )
    End If
    '
    ' Create ServerForm
    '
    If ccLib.ReadStreamBinaryRead Then
        '
        ' binary multipart form 
        '
        TotalBytes = Request.TotalBytes
        If TotalBytes > 0 Then
            Server.ScriptTimeout = 1800
            ccLIb.ServerBinaryHeader = Request.BinaryRead(TotalBytes - 1)
        End If
    Else
        '
        ' non-binary form, create Form String
        '
        For Each NewKey In Request.Form
            ccLib.ServerForm = ccLib.ServerForm & "&" & CStr(NewKey) & "=" & Server.URLEncode( Request.Form(NewKey))
        Next
        If Len( ccLib.ServerForm ) > 0 then
            ccLib.ServerForm = Mid( ccLib.ServerForm, 2 )
        End If
    End If
    '
    ' Create ServerCookie string
    '
    For Each NewKey In Request.Cookies
        ccLib.ServerCookies = ccLib.ServerCookies & "&" & CStr(NewKey) & "=" & Request.Cookies(NewKey)
    Next
    If Len( ccLib.ServerCookies ) > 0 then
        ccLib.ServerCookies = Mid( ccLib.ServerCookies, 2 )
    End If
End Sub
'
'==============================================================================
'	Initialize
'==============================================================================
'
if Response.IsClientConnected then
      Call InitLibAPI()
else
      Response.End()
end if

%>
