<%
'
'==============================================================================
'	This header file contains site wide definitions and is required
'	by all documents in the site.
'
'	set the appName to the Contensive application name
'========================================================================
'
const appName = "replaceWithContensiveAppName"
'
function getContensivePage( cp, isAdmin )
	on error resume next
	'
	Dim s,NewKey,TotalBytes,isBinaryRequest,c,cName,cValue,row,rowPtr,keyPointer,keyValue
	'
	' setup IIS Response
	'
	s = ""
	Response.CacheControl = "no-cache"
	Response.Expires = -1
	Response.Buffer = True
	'
	' Setup Contensive
	'
	cp.context.appName = appName
	cp.context.domain = Request.ServerVariables("SERVER_NAME")
	cp.context.pathPage = CStr( Request.ServerVariables("SCRIPT_NAME"))
	cp.context.referrer = CStr( Request.ServerVariables("HTTP_REFERER"))
	cp.context.isSecure = cBool( Request.ServerVariables("SERVER_PORT_SECURE"))
	cp.context.remoteIp = CStr( Request.ServerVariables("REMOTE_ADDR"))
	cp.context.browserUserAgent = CStr( Request.ServerVariables("HTTP_USER_AGENT"))
	cp.context.acceptLanguage = CStr( Request.ServerVariables("HTTP_ACCEPT_LANGUAGE"))
	cp.context.accept = CStr( Request.ServerVariables("HTTP_ACCEPT"))
	cp.context.acceptCharSet   = CStr( Request.ServerVariables("HTTP_ACCEPT_CHARSET"))
	cp.context.profileUrl = CStr( Request.ServerVariables("HTTP_PROFILE"))
	cp.context.xWapProfile = CStr( Request.ServerVariables("HTTP_X_WAP_PROFILE"))
	'
	' Create ServerQueryString
	'
	isBinaryRequest=false
	c=""
	For Each NewKey In Request.QueryString
		c = c & "&" & Server.URLEncode(CStr(NewKey)) & "=" & Server.URLEncode(Request.QueryString(NewKey))
		isBinaryRequest = isBinaryRequest Or (lcase(CStr(NewKey)) = "requestbinary")
	Next
	If Len( c ) > 0 then
		c = Mid( c, 2 )
	End If
	cp.context.isBinaryRequest = isBinaryRequest
	cp.context.queryString = c
	'
	' Create ServerForm
	'
	if isBinaryRequest then
		'
		' binary multipart form 
		'
		TotalBytes = Request.TotalBytes
		If TotalBytes > 0 Then
			Server.ScriptTimeout = 1800
			cp.context.binaryRequest = Request.BinaryRead(TotalBytes - 1)
		End If
	Else
		'
		' non-binary form, create Form String
		'
		c = ""
		For Each NewKey In Request.Form
			if cp.encodeInteger(Request.Form(NewKey).count) > 1 then
				keyValue = ""
				for keyPointer = 1 to Request.Form(NewKey).count
					if keyValue <> "" then
						keyValue = keyValue & ","
					end if
					keyValue = keyValue & Request.Form(NewKey)(keyPointer)
				next
				c = c & "&" & Server.URLEncode(CStr(NewKey)) & "=" & Server.URLEncode(cstr(keyValue))
			else
				c = c & "&" & Server.URLEncode(CStr(NewKey)) & "=" & Server.URLEncode( Request.Form(NewKey))
			end if
		Next
		If Len( c ) > 0 then
			c = Mid( c, 2 )
		End If
		cp.context.form = c
	End If
	'
	' Create ServerCookie string
	'
	c = ""
	For Each NewKey In Request.Cookies
		c = c & "&" & CStr(NewKey) & "=" & Server.URLEncode( Request.Cookies(NewKey))
		'c = c & "&" & Server.URLEncode(CStr(NewKey)) & "=" & Server.URLEncode( Request.Cookies(NewKey))
	Next
	If Len( c ) > 0 then
		c = Mid( c, 2 )
	End If
	cp.context.cookies = c
	'
	' get doc and process Contensive output
	'
	s = s & cp.getDoc(isAdmin)
	'
	' post processing
	'
	c = cp.context.responseRedirect
	if c <> "" then
		'
		' redirect
		'
		response.redirect c
		response.end
	else
		'
		' concatinate writestream data to the end of the body
		'
		s = s & cp.context.responseBuffer
		'
		' set content type
		'
		c = cp.context.responseContentType
		if c <>"" then
			response.contentType = c
		end if
		'
		' set cookies
		'
		c = cp.context.responseCookies
		if c<>"" then
			row = split( c, vbcrlf )
			do while (rowPtr+5)<=ubound( row )
				cName = row(rowPtr+0)
				if cName <> "" then
					response.Cookies(cName) = row(rowPtr+1)
					c = row(rowPtr+2)
					if c<>"" then
						response.Cookies(cName).expires = cstr( c )
					end if
					c = row(rowPtr+3)
					if c <> "" then
						response.Cookies(cName).domain = c
					end if
					c = row(rowPtr+4)
					if c <> "" then
						response.Cookies(cName).path = c
					end if
					c = row(rowPtr+5)
					if c <> "" then
						response.Cookies(cName).secure = c
					end if
				end if
				rowPtr=rowPtr+6
			loop
		end if
		'
		' set headers
		'
		c = cp.context.responseHeaders
		if c<>"" then
			row = split( c, vbcrlf )
			rowPtr = 0
			do while (rowPtr+1)<=ubound( row )
				cName = row(rowPtr+0)
				if cName <> "" then
					call response.AddHeader(cName,cstr(row(rowPtr+1)))
				end if
				rowPtr=rowPtr+2
			loop
		end if
		'
		' set http status
		'
		c = cp.context.responseStatus
		if c<>"" then
			response.status = status
		end if
	end if
	getContensivePage = s
End Function

%>
