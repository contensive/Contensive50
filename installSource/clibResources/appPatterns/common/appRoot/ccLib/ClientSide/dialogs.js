/*
Author: Rob Eberhardt, Slingshot Solutions - http://slingfive.com/
Description: fixes failings of modal/modeless dialogs
Fixes: form submission, link targets, cookies, window.opener, keyboard shortcuts, accelerator:true, 
	window.navigate(), window.location.assign(), window.location.reload(), window.location.replace()
Usage: simply call at/after window.onload fires, it automatically detects dialogs & fixes
Params: p_bEnableContextMenu as boolean (enables custom context menu to re-create IE context menu basics)
Not fixed:
	-dynamically setting document.title
	-"window.location = ..."
History:
	2004-09-30	changed form/link fix, uses dynamic BASE tag instead of element looping
	2004-03-10	1st public release

*/




function fixDialog(p_bEnableContextMenu){
	// just for dialogs, else quit
	try{
		if(dialogArguments==undefined){return}
	}catch(e){return}
	
	p_bEnableContextMenu = (p_bEnableContextMenu==true);	// ensure datatype


	// name window, so we can fix targeting
	if(window.name==''){window.name = "winDialog";}
		
	
	goToURL = function(p_strURL){
		var oLink = document.createElement("A");
		document.body.insertAdjacentElement('beforeEnd', oLink);
		with(oLink){
			href = p_strURL;
			target = window.name;
			click();
		}
	}
	

	// fix window.navigate()
	window.navigate = function(p_strURL){
		var oLink = document.createElement("A");
		document.body.insertAdjacentElement('beforeEnd', oLink);
		with(oLink){
			href = p_strURL;
			target = window.name;
			click();
		}
	}
	//window.navigate = goToURL; 
	// fix window.location.assign()
	window.location.assign = function(p_strURL){
		var oLink = document.createElement("A");
		document.body.insertAdjacentElement('beforeEnd', oLink);
		with(oLink){
			href = p_strURL;
			target = window.name;
			click();
		}
	}
	//window.location.assign = goToURL; 
	// fix window.location.reload()
	window.location.reload = function(){window.navigate(window.location.href)}
	// fix window.location.replace() (SORTA!)
	window.location.replace = function(p_strURL){
		var oLink = document.createElement("A");
		document.body.insertAdjacentElement('beforeEnd', oLink);
		with(oLink){
			href = p_strURL;
			target = window.name;
			click();
		}
	}
	//window.location.replace = goToURL; 
	
		

	// if window object was passed to vArguments, hook up as window.opener
	// eg:  showModalDialog(sURL, window, sFeatures)
	var bWasPassedWindow = false;
	try{
		bWasPassedWindow = dialogArguments.location!=undefined;
	}catch(e){}


	if(bWasPassedWindow){
		// fix opener
		window.opener = dialogArguments;

		// fix cookies (make available in dialog) 	<--- seems to work fine without this too?
		document.cookie = dialogArguments.document.cookie;
	}



	// fix document stuff
	attachEvent("onload", function(){
	
		// fix form submissions and link openings
		// insert base tag with target (should be faster & work on dynamically created links)
		var oHead = document.getElementsByTagName("HEAD")[0];
		var oBase= document.createElement("BASE");
		oBase.target = window.name;
		oHead.insertAdjacentElement('AfterBegin', oBase);
		//alert(oHead.outerHTML);
		

/*		//	old form/link fix

		// fix form submission (don't open new window)
		var colForms = document.forms;
		for(var f=0; f<colForms.length; f++){
			if(!colForms[f].target || colForms[f].target==null || colForms[f].target==''){colForms[f].target = window.name;}
		}
		
		// fix link opening (don't open new window)
		var colLinks = document.links;
		for(var l=0; l<colLinks.length; l++){
			if(!colLinks[l].target || colLinks[l].target==null || colLinks[l].target==''){colLinks[l].target = window.name;}
		}
*/


		// fix U accelerator
		var colUtags = document.all.tags("U");
		for(var u=0; u<colUtags.length; u++){
			if(colUtags[u].style.accelerator){
				colUtags[u].style.textDecoration = 'none';
			}
		}

	});


	// fix keyboard stuff
	document.attachEvent("onkeydown", function(){
//		alert(event.keyCode);
		switch(event.keyCode){
			case 27: {	// enable ESC to close dialog
				close();
				break;
			}

			case 18: {	// alt, recreating accelerator
				if(event.repeat){break}	// ignore if key is held down
				var colUtags = document.all.tags("U");
				for(var u=0; u<colUtags.length; u++){
					if(colUtags[u].style.accelerator){
						colUtags[u].style.textDecoration = (colUtags[u].style.textDecoration!='none') ? 'none': 'underline';	// toggle underline
					}
				}
				setTimeout("document.body.focus();", 200);	// fixes toggle (sorta)
				//event.returnValue = false;
				event.cancelBubble = true;
				//return false;
				break;
			}

			//==== fix reload ====
			case 116: {	// f5 
				window.location.reload();
				break;
			}
			case 82: {	// ctl-R
				if(event.ctrlKey){
				window.location.reload();
				}
				break;
			}

			//==== fix keyboard navigation (does Not work, history object is always empty..) ====
			case 8: {	// backspace
				history.back();
				break;
			}
			case 37: {	// alt-left arrow
				if(event.altKey){history.back();}
				break;
			}
			case 39: {	// alt-right arrow
				if(event.altKey){history.forward();} 
				break;
			}
		}
	});


	// enable basics of normal context (right-click) menu
	document.attachEvent("oncontextmenu", function(){
		if(p_bEnableContextMenu){

		
/*		TODO: somehow reenable normal context menu...
//		var oEvtClick = createEventObject();
		var oEvtClick = new Object();
		oEvtClick.button = 2;	// right-click
//		alert(oEvtClick.button);
		//document.body.fireEvent('onclick', oEvtClick);
		return;
*/

		
			var strMenuHTML = 
				'<html><body>' + 
				'<st'+'yle type="text/css">\r\n' +
				'BODY {background:buttonface;  padding:2px; font:x-small Tahoma, sans-serif; font-size:80%;}\r\n' +
				'A {padding:0 4px 0 4px; text-decoration:none; color:buttontext; WIDTH:115%; display:block; cursor:default;}\r\n' +
				'A:hover {background:highlight; color:highlighttext;}\r\n' +
				'</st'+'yle>\r\n' + 
				'<scr'+'ipt language="Javascript">\r\n' +
				'function reload(){parent.location=parent.location.href}\r\n' +
				'</scr'+'ipt>\r\n' +
				'<a href="#" onclick="parent.window.opener.external.AddFavorite(parent.document.location.href, parent.document.title)">Add To Favorites</a>\r\n' +
				'<a href="#" onclick="parent.location=\'view-source:\'+parent.document.location.href">View Source</a>\r\n' +
				'<a href="#" onclick="parent.print()">Print</a>\r\n' +
				'<a href="#" onclick="parent.location.reload()">Refresh</a>\r\n' +
				'</body></html>' +
				'';

			var oPop = window.createPopup();
			var oPopBody = oPop.document.body;
			oPopBody.style.cssText = 'border:2px threedhighlight outset;';
			oPopBody.innerHTML = strMenuHTML;
			//alert(oPopBody.innerHTML);
			oPop.show(event.clientX, event.clientY, 130, 75, document.body);
			return false;

		}
	})


}