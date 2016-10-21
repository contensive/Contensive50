<?php
//
//==============================================================================
//	This header file contains site wide definitions and is required
//	by all documents in the site.
//
//	set the appName to the Contensive application name
//
//========================================================================
//
//
// -----
//
function getContensivePage( $cp, $isAdmin ) { 
	$s = "";
	$appName = "replaceWithContensiveAppName";
	$cpContext = $cp->context;
	$referer = "";
	//
	// Create Contensive web client object in function scope
	//
	$cpContext->appName = $appName;
	$cpContext->allowProfileLog = false;
	$cpContext->isBinaryRequest = false;
	$cpContext->requestNameSpaceAsUnderscore = true;
	$cpContext->requestNameDotAsUnderscore = true;
	$cpContext->domain = getServerVar( "SERVER_NAME" );
	$cpContext->pathPage = getServerVar( "SCRIPT_NAME" );
	$cpContext->referrer = getServerVar( "HTTP_REFERER" );
	$cpContext->isSecure = getServerVar( "SERVER_PORT_SECURE" );
	$cpContext->remoteIp = getServerVar( "REMOTE_ADDR" );
	$cpContext->browserUserAgent = getServerVar( "HTTP_USER_AGENT" );
	$cpContext->acceptLanguage = getServerVar( "HTTP_ACCEPT_LANGUAGE" );
	$cpContext->accept = getServerVar( "HTTP_ACCEPT" );
	$cpContext->acceptCharSet = getServerVar( "HTTP_ACCEPT_CHARSET" );
	$cpContext->profileUrl = getServerVar( "HTTP_PROFILE" );
	$cpContext->xWapProfile = getServerVar( "HTTP_X_WAP_PROFILE" );
	$cpContext->queryString = getServerVar( "QUERY_STRING" );
	//
	// test for php post size issue
	//
	if ($_SERVER['REQUEST_METHOD'] == 'POST' && empty($_POST) && $_SERVER['CONTENT_LENGTH'] > 0) {
		$c = "There was an error loading your form. The data may be too large.";
		$cp->userError->Add( $c );
    }
	//
	// Build ServerForm and ServerFormFiles
	//
	$cForm='';
	$cFile='';
	$i = 0;
	foreach($_POST as $name => $value) {
		if( is_array($_POST[$name])) {
			foreach ($_POST[$name] as $value) {
				$cForm .= "&" . urlencode($name) . "[]=" . urlencode($value);
			}
		} else {
			$cForm .= "&" . urlencode($name) . "=" . urlencode($value);
		}
	}
	while ($file = current($_FILES)) {
		$formname = urlencode(key($_FILES));
		$filename = urlencode($file["name"]);
		if( $filename<>"" ) {
			$cFile .= "&" . $i . "formname=" . $formname;
			$cFile .= "&" . $i . "filename=" . $filename;
			$cFile .= "&" . $i . "type=" . urlencode($file["type"]);
			$cFile .= "&" . $i . "tmpfile=" . urlencode($file["tmp_name"]);
			$cFile .= "&" . $i . "error=" . urlencode($file["error"]);
			$cFile .= "&" . $i . "size=" . urlencode($file["size"]);
			$i++;
		}
		next($_FILES);
	}
	if ($cForm<>"") $cForm=substr($cForm,1);
	$cpContext->form = $cForm;
	if ($cFile<>"") $cFile=substr($cFile,1);
	$cpContext->formFiles = $cFile;
	//
	// Build ServerCookie string -- must urlencode name/value
	//
	$c='';
	foreach($_COOKIE as $name => $value) {
		$c = $c . "&" . urlencode($name) . "=" . urlencode($value);
	}
	if ($c<>"") $c=substr($c,1);
	$cpContext->cookies = $c;
	//
	// get Contensive page
	//
	$s .= $cp->getDoc( $isAdmin );
	//
	// setup IIS Response
	//
	header("Expires: Wed, 16 Mar 2011 05:00:00 GMT");
	header("Cache-Control: no-store, no-cache");
	header("Pragma: no-cache");
	//
	// post processing
	//
	$c = $cpContext->responseRedirect;
	if ($c <> "" ) {
		//
		// redirect
		//
		header( 'Location: ' . $c ) ;
	} else {
		//
		// concatinate writestream data to the end of the body
		//
		$s .= $cpContext->responseBuffer;
		//
		// set content type
		//
		$c = $cpContext->responseContentType;
		if ( $c <> "" ) {
			header("content-type:" . $c);
		}
		//
		// set cookies
		//
		$c = $cpContext->responseCookies;
		if ( $c <> "" ) {
			$row = explode( PHP_EOL, $c );
			$rowSize = count($row);
			$rowPtr = 0;
			while (( $rowPtr+5 ) < $rowSize ) {
				$cName = $row[ $rowPtr+0 ];
				if ($cName<>"") {
					$cValue = urldecode( $row[ $rowPtr+1 ]);
					$cExpires = strtotime( urldecode( $row[ $rowPtr+2 ]));
					$cDomain = urldecode( $row[ $rowPtr+3 ]);
					$cPath = urldecode( $row[ $rowPtr+4 ]);
					$cSecure = urldecode( $row[ $rowPtr+5 ]);
					if( $cExpires=="" ) $cExpires=null;
					if( $cPath=="" ) $cPath=null;
					if( $cDomain=="" ) $cDomain=null;
					if( $cSecure=="true" ) {
						$cSecure=TRUE;
					} else {
						$cSecure=FALSE;
					}
					setcookie( $cName, $cValue, $cExpires, $cPath, $cDomain);
					//setcookie($cName, $cValue, $cExpires, $cPath, $cDomain, $cSecure );
					$rowPtr+=6;
				}
			}
		}
		//
		// set headers
		//
		$c = $cpContext->responseHeaders;
		if ( $c <> "" ) {
			$row = explode( "\r\n", $c );
			$rowSize = count($row);
			$rowPtr = 0;
			while (( $rowPtr+1 ) < $rowSize ) {
				$cName = urldecode( $row[ $rowPtr+0 ] );
				if ($cName<>"") {
					$cValue = urldecode( $row[ $rowPtr+1 ] );
					$rowPtr+=2;
					header($cName . ":" . $cValue);
				}
			}
		}
		//
		// set http status
		//
		$c = $cpContext->responseStatus;
		if ( $c <> "" ) {
			header("Status: " . $c);
		}
	}
	return $s;
}
function getServerVar( $requestName ) {
	$value = "";
	if (isset($_SERVER[$requestName])) {
		$value = new variant($_SERVER[$requestName]);
	} 
	return $value;
}
?>