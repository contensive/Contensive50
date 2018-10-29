
var numRows;
var numCols;
var initialized = 0;
var topAndBottomBarHeight = 50;
var ActiveContentDivID = '';
var ActiveTabTableID = '';
var ContentHeight=0;

function TabInit() {
	var i;
	return false;
	if (initialized == 0) {
		initialized  = 1;
		var AllContentDivs = document.getElementsByTagName("div");
		var ContentTop=999;
		var styleTop=0;
		var FirstTop=0;
		//
		for (i = 0; i < AllContentDivs.length; i++) {
			if (AllContentDivs[i].className=='ccAdminTabBody') {
				if ( AllContentDivs[i].style.display == "block" ) {
					FirstTop = getTop( AllContentDivs[i] );
				} else {
				}
				styleTop = getTop( AllContentDivs[i] );
				if ( ContentTop > styleTop ) {
					ContentTop = styleTop;
				}
				if (AllContentDivs[i].offsetHeight>ContentHeight) {
					ContentHeight=AllContentDivs[i].offsetHeight;
				}
			}
		}
	}
}


function switchLiveTab2(ContentDivID, ClickedTab, TabTableID, StylePrefix, TabWrapperID ) {
//alert('switchLiveTab2');
	var e, ePtr
	var i,TCnt,BCnt,RCnt;
	var newIndex;
	var oldIndex;
	var TabTable;
	var TBodyNode,TRNode,TDNode
	var NewContentDiv;
	var OldContentDiv;
	var TabWrapper;
	var StyleHeight;
	var width;
	//
	if (ActiveTabTableID==TabTableID) {return false;}
	NewContentDiv = document.getElementById(ContentDivID);
	OldContentDiv=document.getElementById(ActiveContentDivID);
	TabWrapper = document.getElementById(TabWrapperID);
	//
	// initialize ContentHeight and width
	//
//	if ( ContentHeight==0 ) {ContentHeight=OldContentDiv.offsetHeight;}
//	width=OldContentDiv.offsetWidth
	//
	// hide old content
	//
	OldContentDiv.style.visibility='hidden';
OldContentDiv.style.position='absolute';
//	NewContentDiv.style.width=width+'px';
//	NewContentDiv.style.top=0;
NewContentDiv.style.position='relative';
NewContentDiv.style.visibility = "visible";
NewContentDiv.style.zIndex = "1";
	// NewContentDiv.style.height = StyleHeight;
	//
	// convert new content to block
	//
//	NewContentDiv.style.visibility = 'hidden';
//	if (NewContentDiv.offsetHeight>ContentHeight) {
//		ContentHeight=NewContentDiv.offsetHeight;
//	}
//	if (browser.isIE) {
//		StyleHeight = ContentHeight;
//	} else {
//		StyleHeight = ContentHeight-getPadding(NewContentDiv);
//	}
//	TabWrapper.style.height = ContentHeight;
	//
	// Show new content
	//
//	NewContentDiv.style.visibility = "visible";
//	NewContentDiv.style.zIndex = "1";
//	NewContentDiv.style.height = StyleHeight;
//	//
//	// wiggle any iewiggle input boxes - for IE bug
//	// problem is ie does not calculate the position of relative elements if they are display:none
//	//
//	e = document.getElementsByName('iewiggle')
//	if ( e ) {
//		for (ePtr = 0; ePtr<e.length; ePtr++ ) {
//			e[ePtr].style.display='block';
//			e[ePtr].style.display='none';
//		}
//	}
	//
	// turn off current active tab
	//
	TabTable = document.getElementById(ActiveTabTableID)
	for (TCnt = 0; TCnt < TabTable.childNodes.length; TCnt++) {
		TBodyNode=TabTable.childNodes[TCnt];
		if (TBodyNode.tagName == "TBODY") {
			for (BCnt = 0; BCnt < TBodyNode.childNodes.length; BCnt++) {
				TRNode=TBodyNode.childNodes[BCnt];
				for (RCnt = 0; RCnt < TRNode.childNodes.length; RCnt++) {
					TDNode=TRNode.childNodes[RCnt];
					if (TDNode.className==StylePrefix+'Hit') {
						TDNode.className=StylePrefix;
					}
				}
			}
		}
	}
	ActiveTabTableID=TabTableID
	// turn on new active tab
	TabTable = document.getElementById(TabTableID)
	for (TCnt = 0; TCnt < TabTable.childNodes.length; TCnt++) {
		TBodyNode=TabTable.childNodes[TCnt];
		if (TBodyNode.tagName == "TBODY") {
			for (BCnt = 0; BCnt < TBodyNode.childNodes.length; BCnt++) {
				TRNode=TBodyNode.childNodes[BCnt];
				for (RCnt = 0; RCnt < TRNode.childNodes.length; RCnt++) {
					TDNode=TRNode.childNodes[RCnt];
					if (TDNode.className==StylePrefix) {
						TDNode.className=StylePrefix+'Hit';
					}
				}
			}
		}
	}
	// toggle tab links
	var tabLinks = document.getElementsByName("tabLink");
	for (i = 0; i < tabLinks.length; i++) {
		if (tabLinks[i] != ClickedTab) {
			tabLinks[i].className = StylePrefix+'Link';
			tabLinks[i].blur();
		}
		else {
			tabLinks[i].className = StylePrefix+'HitLink';
			tabLinks[i].blur();
		}
	}
	ActiveContentDivID=ContentDivID;
//	//
//	// This is an IE fix - if a dynamic tree from ccnav.dll is on a div exposed by this click, the
//	// the relative positioned icons are not positioned correctly
//	//
//	if ( window.WiggleTree ) { WiggleTree() }

}
//
function getTop(el) {
	var y, p;
	p = el.offsetParent;
	y = el.offsetTop;
	while (p !=null){
		y+=p.offsetTop;
		p=p.offsetParent;
	}
	return y;
}
function getPadding(x)
{
	if (x.currentStyle) {
		var Top = x.currentStyle['paddingTop'];
		var Bottom = x.currentStyle['paddingBottom'];
	} else if (window.getComputedStyle) {
		var Top = document.defaultView.getComputedStyle(x,null).getPropertyValue('padding-top');
		var Bottom = document.defaultView.getComputedStyle(x,null).getPropertyValue('padding-bottom');
	}
	var TopNoPx = Top.split('p');
	var BottomNoPx = Top.split('p');
	y = parseInt(TopNoPx[0])+parseInt(BottomNoPx[0])
	return y;
}
//
// id bug fix - relative positioned elements do not paint correctly if they are created in elements with display:none
// to fix it, add a call to this inside the element that is display:none
// <script Language="JavaScript" type="text/javascript">EmbedWiggle();</script>
//
function EmbedWiggle() {
	document.write('<input type=text name="iewiggle" style="width:1px;display:none;enable:false">')
}
