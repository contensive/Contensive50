
var ButtonObjectCount = 0;		// number of button objects on the current doc
var MenuObjectCount = 0;		// number of menu objects on the current doc
var MenuHideBlock = false;		// prevents the menus from being cleared
var isNS = (navigator.appName == "Netscape");
var isMacIE = ( (navigator.userAgent.indexOf("IE 4") > -1) && (navigator.userAgent.indexOf("Mac") > -1) );
var layerRef = (isNS) ? "document" : "document.all";
var styleRef = (isNS) ? "" : ".style";
var isDynamic = ( ((document.layers && document.layers['AdminMenuTest']) || (document.all && document.all['AdminMenuTest'])) && !isMacIE );
var activeMenu = 0;


function SetFieldName( FieldName ) { document.all[ 'fn' ].value = FieldName; return true; }

function ButtonFlatSet( TargetObject, Color ) {
	var TargetObjectLayer;
	if ( isNS ) {
		// document[TargetObject].backgroundColor = Color
		// TargetObjectLayer = TargetObject + "Layer"
		// document[TargetObjectLayer].backgroundColor = Color
		// document.getElementById(TargetObject).style.backgroundColor=Color;
	} else {
		// ButtonObject = document.all[TargetObject];
		// ButtonObject.style.backgroundColor = Color;
		}
	}	

function Button3DSet( TargetObject, ColorBase, ColorHilite, ColorShadow ) {
	if ( isNS ) {
	} else {
		document.all[TargetObject+"t"].style.backgroundColor = ColorHilite
		document.all[TargetObject+"l"].style.backgroundColor = ColorHilite
		document.all[TargetObject+"c"].style.backgroundColor = ColorBase
		document.all[TargetObject+"r"].style.backgroundColor = ColorShadow
		document.all[TargetObject+"b"].style.backgroundColor = ColorShadow
		}
	}

function MenuDown( TargetName, TargetParentName ) {
	var TargetPointer;
	var TargetParentPointer;
	var NSTargetName;
	var NSTargetParentName;
	if ( isDynamic ) {
		if ( isNS ) {
			MenuAllOff()
			document[TargetName].visibility = "visible";
		} else {
			MenuHideBlock = false;
			MenuAllOff()
			MenuHideBlock = true;
			TargetPointer = document.all[TargetName]
			TargetParentPointer = document.all[TargetParentName]
			TargetPointer.style.visibility = "visible";
			}
		}
	}

function MenuOff( TargetObject ) {
	if ( isNS ) {
		document[TargetObject].visibility = "hidden";
	} else {
		document.all[TargetObject].style.visibility = "hidden";
		}
	}

function MenuAllOff() {
	var TargetCount;
	var TargetObject;
	if ( isNS ) {
		for ( TargetCount=0; TargetCount < MenuObjectCount; TargetCount += 1 ) {
			TargetObject = "Menu" + TargetCount;
			MenuOff( TargetObject )
			}
	} else {
		if ( !MenuHideBlock ) {
			for ( TargetCount=0; TargetCount < MenuObjectCount; TargetCount += 1 ) {
				TargetObject = "Menu" + TargetCount;
				MenuOff( TargetObject )
				}
			}
	MenuHideBlock = false;
		}
	}
