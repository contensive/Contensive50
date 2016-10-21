<?php 
//
// uncomment enable errors and warnings
//
// error_reporting(E_ERROR | E_PARSE);
// ini_set("display_errors", 1);
//
include_once( "../includes/ContensiveConfig42.php" );
//
$cp = new com( "Contensive.Processor.CPClass" );
echo getContensivePage( $cp, true );
$cp->dispose();
?>