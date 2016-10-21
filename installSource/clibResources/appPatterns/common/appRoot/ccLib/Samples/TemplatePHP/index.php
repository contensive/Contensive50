<?php 
//
// uncomment to enable PHP errors and warnings
//
// error_reporting(E_ERROR | E_PARSE);
// ini_set("display_errors", 1);
//
include_once( "includes/ContensiveConfig42.php" );
//
$cp = new com( "Contensive.Processor.CPClass" );
$page = getContensivePage( $cp, false );
if ($cp->response->isOpen()) {
	//
	// page is open, modify it. The sample adds the user's name to the top line
	//
	//$page = "<div>Your user name is: " . $cp->user->name . "</div>" . $page;
	//
	// uncomment to implement php replacement from the content
	// if you use this, take care to htmlEncode anything the user has access to: thier name, etc.
	//
	// 	$segment = explode( "<"."?php", $page );
	// 	if( count( $segment ) > 0  ) {
	//		$body = $segment[0];
	//		for ($i = 1; $i <= count( $segment ); $i++) {
	//			$segmentPart = explode( "?".">", $segment[$i] );
	//			$page .= eval( $segmentPart[0] );
	//			$page .= $segmentPart[1];
	//		}	
	//	}
	//
	// inline Scripts
	// created from code in the inline script tab of add-ons.
	// see support.contensive.com/using-inline-scripts for more details.
	//
	$segment = explode( "<!-- inlineScript[", $page );
 	if( count( $segment ) > 0  ) {
		$page = $segment[0];
		for ($i = 1; $i < count( $segment ); $i++) {
			$endPos = strpos( $segment[$i], "] -->" );
			if ( $endPos>=0 ) {
				$inlineScript = htmlspecialchars_decode( substr( $segment[$i], 0, $endPos ), ENT_QUOTES );
				//$page .= '['.$inlineScript.']';
				$page .= eval( $inlineScript );
				$page .= substr( $segment[$i], $endPos+5 );
			}
		}	
	}
}
echo $page;
$cp->dispose();
?>