<?php
//
//==============================================================================
//	This header file contains site wide definitions and is required
//	by all asp documents in the site.
//
//	Sample public site page
//		include this page
//	 	echo getContensivePublicPage( "My App Name" );
//
//	Sample admin site page
//		include this page
//		echo getContensiveAdminPage( "My App Name" );
//========================================================================
//
// -----
//
function testFunction1 ( $testValue )
{
	return $testValue;
}
function getContensivePublicPage( $appName ) 
{
	getContensivePublicPage = getContensivePage( $appName, false );
}
//
// -----
//
function getContensiveAdminPage( $appName ) 
{
	getContensiveAdminPage = getContensivePage( $appName, true );
}
//
// -----
//
function getContensivePage( $appName, $isAdmin )
{ 
//
// setup IIS Response
//
//	Response.CacheControl = "no-cache"
//	Response.Expires = -1
//	Response.Buffer = True
//
// Create Contensive object
//
	$ccLib = new com("ccWeb3.MainClass");
	$ccLib->UseASPObjects = false;
//	'
//	' Setup Contensive
//	'
	$ccLib->ServerPathPage = new variant($_SERVER["SCRIPT_NAME"]);
	$ccLib->ServerReferrer = new variant($_SERVER["HTTP_REFERER"]);
	$ccLib->ServerHost = new variant($_SERVER["SERVER_NAME"]);
	$ccLib->BrowserLanguage = new variant($_SERVER["HTTP_ACCEPT_LANGUAGE"]);
	$ccLib->ServerPageSecure = new variant($_SERVER["SERVER_PORT_SECURE"]);
	// $ccLib->PhysicalWWWPath = new variant(realpath("index.php"));
	// $ccLib->PhysicalccLibPath = Server.MapPath( "/ccLib" )
	$ccLib->VisitRemoteIP = new variant($_SERVER["REMOTE_ADDR"]);
	$ccLib->VisitBrowser = new variant($_SERVER["HTTP_USER_AGENT"]);
	$ccLib->HTTP_Accept = new variant($_SERVER["HTTP_ACCEPT"]);
	$ccLib->HTTP_Accept_charset = new variant($_SERVER["HTTP_ACCEPT_CHARSET"]);
	$ccLib->HTTP_Profile = new variant($_SERVER["HTTP_PROFILE"]);
	$ccLib->HTTP_X_Wap_Profile = new variant($_SERVER["HTTP_X_WAP_PROFILE"]);
	//
	// Create ServerQueryString -- need to URLencode name/value
	//
	$c='';
	foreach($_GET as $variable => $value) {
		$c = $c . "&" . $variable . "=" . $value;
		if (strtoupper($variable)=="REQUESTBINARY") {
			$ccLib->ReadStreamBinaryRead=true;
		}
	}
	if ($c<>"") {
		$c=substr($c,1);
	}
	$ccLib->ServerQueryString = $c;
	//
	// Create Form -- must urlencode name/value
	//
	if ( $ccLib->ReadStreamBinaryRead ) {
		//
		// Binary Read
		//
		echo "Binary Read";
		$ccLib->ServerBinaryHeader = $HTTP_RAW_POST_DATA;
	} else {
		//
		// non Binary Read
		//
		$c='';
		foreach($_POST as $variable => $value) {
			$c = $c . "&" . $variable . "=" . $value;
		}
		if ($c<>"") {
			$c=substr($c,1);
		}
		$ccLib->ServerForm = $c;
	}
	//
	// Create ServerCookie string -- must urlencode name/value
	//
	$c='';
	foreach($_COOKIE as $variable => $value) {
		$c = $c . "&" . $variable . "=" . $value;
	}
	if ($c<>"") {
		$c=substr($c,1);
	}
	$ccLib->ServerCookies = $c;
	//
	// get Contensive page
	//
	$s = $ccLib->getHtml2( $appName, $isAdmin );
	//
	// post processing
	//
	$c = ccLib->responseRedirect;
	if ($c <> "" ) {
		//
		// redirect
		//
		//response.redirect c
		//response.end
	} else {
		//
		// set content type
		//
		$c = $ccLib->responseContentType;
		if ( $c <> "" ) {
			// response.contentType = c
		}
		//
		// set cookies
		//
		$c = $ccLib->responseCookies
		if ( $c <> "" ) {
//			row = split( c, vbcrlf )
//			do while (rowPtr+5)<=ubound( row )
//				cName = row(rowPtr+0)
//				if cName <> "" then
//					response.Cookies(cName) = row(rowPtr+1)
//					c = row(rowPtr+2)
//					if c<>"" then
//						response.Cookies(cName).expires = cstr( c )
//					end if
//					c = row(rowPtr+3)
//					if c <> "" then
//						response.Cookies(cName).domain = c
//					end if
//					c = row(rowPtr+4)
//					if c <> "" then
//						response.Cookies(cName).path = c
//					end if
//					c = row(rowPtr+5)
//					if c <> "" then
//						response.Cookies(cName).secure = c
//					end if
//				end if
//				rowPtr=rowPtr+6
//			loop
		}
		//
		// set headers
		//
		$c = $ccLib->responseHeader;
		if ( $c <> "" ) {
//			row = split( c, vbcrlf )
//			do while (rowPtr+1)<=ubound( row )
//				cName = row(rowPtr+0)
//				if cName <> "" then
//					call response.AddHeader(cName,cstr(row(rowPtr+1)))
//				end if
//				rowPtr=rowPtr+2
//			loop
		}
		//
		// set http status
		//
		$c = $ccLib->responseStatus;
		if ( $c <> "" ) {
//			response.status = status
		}
	return $s
}
?>