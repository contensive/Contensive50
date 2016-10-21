// DHTML Editing Component Constants for JavaScript
// Copyright 1998 Microsoft Corporation.  All rights reserved.
//

//
// Command IDs
//
DECMD_BOLD =                      5000
DECMD_COPY =                      5002
DECMD_CUT =                       5003
DECMD_DELETE =                    5004
DECMD_DELETECELLS =               5005
DECMD_DELETECOLS =                5006
DECMD_DELETEROWS =                5007
DECMD_FINDTEXT =                  5008
DECMD_FONT =                      5009
DECMD_GETBACKCOLOR =              5010
DECMD_GETBLOCKFMT =               5011
DECMD_GETBLOCKFMTNAMES =          5012
DECMD_GETFONTNAME =               5013
DECMD_GETFONTSIZE =               5014
DECMD_GETFORECOLOR =              5015
DECMD_HYPERLINK =                 5016
DECMD_IMAGE =                     5017
DECMD_INDENT =                    5018
DECMD_INSERTCELL =                5019
DECMD_INSERTCOL =                 5020
DECMD_INSERTROW =                 5021
DECMD_INSERTTABLE =               5022
DECMD_ITALIC =                    5023
DECMD_JUSTIFYCENTER =             5024
DECMD_JUSTIFYLEFT =               5025
DECMD_JUSTIFYRIGHT =              5026
DECMD_LOCK_ELEMENT =              5027
DECMD_MAKE_ABSOLUTE =             5028
DECMD_MERGECELLS =                5029
DECMD_ORDERLIST =                 5030
DECMD_OUTDENT =                   5031
DECMD_PASTE =                     5032
DECMD_REDO =                      5033
DECMD_REMOVEFORMAT =              5034
DECMD_SELECTALL =                 5035
DECMD_SEND_BACKWARD =             5036
DECMD_BRING_FORWARD =             5037
DECMD_SEND_BELOW_TEXT =           5038
DECMD_BRING_ABOVE_TEXT =          5039
DECMD_SEND_TO_BACK =              5040
DECMD_BRING_TO_FRONT =            5041
DECMD_SETBACKCOLOR =              5042
DECMD_SETBLOCKFMT =               5043
DECMD_SETFONTNAME =               5044
DECMD_SETFONTSIZE =               5045
DECMD_SETFORECOLOR =              5046
DECMD_SPLITCELL =                 5047
DECMD_UNDERLINE =                 5048
DECMD_UNDO =                      5049
DECMD_UNLINK =                    5050
DECMD_UNORDERLIST =               5051
DECMD_PROPERTIES =                5052

//
// Enums
//

// OLECMDEXECOPT  
OLECMDEXECOPT_DODEFAULT =         0 
OLECMDEXECOPT_PROMPTUSER =        1
OLECMDEXECOPT_DONTPROMPTUSER =    2

// DHTMLEDITCMDF
DECMDF_NOTSUPPORTED =             0 
DECMDF_DISABLED =                 1 
DECMDF_ENABLED =                  3
DECMDF_LATCHED =                  7
DECMDF_NINCHED =                  11

// DHTMLEDITAPPEARANCE
DEAPPEARANCE_FLAT =               0
DEAPPEARANCE_3D =                 1 

// OLE_TRISTATE
OLE_TRISTATE_UNCHECKED =          0
OLE_TRISTATE_CHECKED =            1
OLE_TRISTATE_GRAY =               2


function window_onload() {
	// EditTable.EditForm.DHTMLEdit.DocumentHTML = "<P align=left><FONT size=3>Enter your page text here...</FONT></P>";
	}

function LoadDHTML( FieldName ) {
	document.all["DHTMLEdit"].DocumentHTML = document.all[ FieldName ].value;
	return true;
	}

function StoreDHTML( FieldName ) {
	document.all[ FieldName ].value = document.all["DHTMLEdit"].DOM.body.innerHTML;
	return true;
	}

function btnBold_onclick() {
	document.all["DHTMLEdit"].ExecCommand(DECMD_BOLD);
	}

function btnItalics_onclick() {
	document.all["DHTMLEdit"].ExecCommand(DECMD_ITALIC);
	}

function btnUnderline_onclick() {
	document.all["DHTMLEdit"].ExecCommand(DECMD_UNDERLINE);
	}

function btnLeft_onclick() {
	document.all["DHTMLEdit"].ExecCommand(DECMD_JUSTIFYLEFT);
	}

function btnCenter_onclick() {
	document.all["DHTMLEdit"].ExecCommand(DECMD_JUSTIFYCENTER);
	}

function btnRight_onclick() {
	document.all["DHTMLEdit"].ExecCommand(DECMD_JUSTIFYRIGHT);
	}

function btnIndent_onclick() {
	document.all["DHTMLEdit"].ExecCommand(DECMD_INDENT);
	}

function btnOutdent_onclick() {
	document.all["DHTMLEdit"].ExecCommand(DECMD_OUTDENT);
	}

function btnUL_onclick() {
	document.all["DHTMLEdit"].ExecCommand(DECMD_UNORDERLIST);
	}

function btnOL_onclick() {
	document.all["DHTMLEdit"].ExecCommand(DECMD_ORDERLIST);
	}

function btnFont_onclick() {
	document.all["DHTMLEdit"].ExecCommand(DECMD_FONT);
	}

function btnHyperlink_onclick() {
	document.all["DHTMLEdit"].ExecCommand(DECMD_HYPERLINK);
	}

function btnImage_onclick() {
	document.all["DHTMLEdit"].ExecCommand(DECMD_IMAGE);
	}

function btnViewHTML_onclick() {
	alert(document.all["DHTMLEdit"].DocumentHTML)
	}

function btnViewBody_onclick() {
	alert(document.all["DHTMLEdit"].DOM.body.innerHTML)
	}

function btnPaste_onclick() {
	document.all["DHTMLEdit"].ExecCommand(DECMD_PASTE);
	}

// function btnViewSelectedOnclick() {
//	sel = document.all["DHTMLEdit"].DOM.selection;
// 	if ( "Text" == sel.type ){
//		range = sel.createRange();
//		alert(range.htmlText);
//	} else {
//		alert("No text selected");
//		}
//	}

function DHTMLEdit_DisplayChanged() {
	// This event is fired whenever the user makes any change whatsoever in the document,
	// including simply moving the insertion point.
	// To determine whether a toolbar button or a menu command should be enabled or disabled,
	// you call the QueryStatus method.
	}

function DHTMLEdit_ShowContextMenu() {
	// This event is fired whenever the user right-clicks to bring up the context menu.
	// This is where you would check the selection and create the appropriate context menu
	}

function InsertText( Text ) {
	var DOM = document.all["DHTMLEdit"].DOM;
	var DocSelection = DOM.selection;
	var TextRange = DocSelection.createRange();
	TextRange.text = Text;
	}

