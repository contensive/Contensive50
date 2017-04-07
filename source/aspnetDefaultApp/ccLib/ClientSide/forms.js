// -- PURPOSE: to add a single item to a <select>
// -- Usage: addItemToList(list to be added to, value of item, displayed text of item)

function addItemToList(myList, myValue, myText) {
	if (myValue != "" && myValue != null) {
		// first, remove any items that have no value
		while (myList.options[0] && (myList.options[0].value == "" || myList.options[0].value == null)) {
			myList.options[0] = null;
		}
		var itemNum = myList.options.length;
		myList.options[itemNum] = new Option(myText, myValue, false, false);
		myList.options[itemNum].selected = true;
	}
}


// -- PURPOSE: to validate a field to only contain email addresses

// -- Usage: checkEmail(field to check)


function checkEmail(myField, errorMessage){

	// we only pattern match email addresses, checking a domain registrar would take too long

	if (myField.value.search(/[\w\-\.\_]+\@[\w\-\.\_]+\.[\w\-\.\_]+/) == -1 && myField.value != "") {

		alert('Please enter a valid email address.');

		myField.focus();

		myField.select();

		return false;

	}

	return true;

}


// -- PURPOSE: to validate a field only contains numbers
// -- Usage: checkIsNumber(field to check)

function checkIsNumber(myField, errorMessage) {
	if (myField.value.search(/\d{1,2}/) == -1 && myField.value != "") {
		alert('Please enter a valid number.');
		myField.focus();
		myField.select();
		return false;
	}
	return true;
}

// -- PURPOSE: to validate a field to only contain integers
// -- Usage: checkRequired(the form element to check)

function checkRequired(myForm) {
	var requiredFields = myForm.required.value.split(",");
	var numfields = requiredFields.length;
	var errorString = '';
	for (var i=0; i<numfields; i++) {
		var parts = requiredFields[i].split("=");
		var field = parts[0];
		var prompt = parts[1];		
		var dualFields = new Array("");
		if (field.indexOf("|") != -1) {
			dualFields = field.split("|");
			var isDualField = true;
			for (var k=0; k<dualFields.length; k++) {
				for (var j=0; j<myForm.elements.length; j++) {
					var myElement = myForm.elements[j];
					if (dualFields[k] == myElement.name && myElement.style.display != "none") {
						if (myElement.type == "select-one" || myElement.type == "select-multiple") {
							if (myElement.options[myElement.selectedIndex].value != null && myElement.options[myElement.selectedIndex].value != '') {
								isDualField = false;
							}
						}
						else if (myElement.value != null && myElement.value.search(/\w/) != -1) {
							isDualField = false;
						}
					}
				}
			}
			if (isDualField) {
				errorString += prompt + ", ";
			}
		}
		else {			
			for (var j=0; j<myForm.elements.length; j++) {
				var myElement = myForm.elements[j];
				if (myElement.name == field && myElement.style.display != "none") {
					if (myElement.type == "select-one" || myElement.type == "select-multiple") {
						if ((myElement.options[myElement.selectedIndex].value == null || myElement.options[myElement.selectedIndex].value == '') && errorString.indexOf(prompt) == -1) {
							errorString += prompt + ", ";
						}
					}
					else if ((myElement.value == null || myElement.value.search(/\w/) == -1) && errorString.indexOf(prompt) == -1) {
							errorString += prompt + ", ";
					}
				}
			}
		}
	}
	if (errorString != '') {
		window.alert('Please fill in the following required fields before submitting this form:\n\n' + errorString.slice(0,errorString.length-2));
		return false;
	}
	else {
		return true;
	}
}

// -- PURPOSE: to clear all fields in a form (differs from reset if form is prepopulated
// -- Usage: clearForm(form to be cleared)

function clearForm(myForm) {
	for (var i=0; i<myForm.elements.length; i++) {
		var myElement = myForm.elements[i];
		switch (myElement.type) {
			case "text":
			case "textarea":
			case "password":
				myElement.value="";
				break;
			case "checkbox":
				myElement.checked = false;
				break;
			case "select-one":
			case "select-multiple":
				myElement.selectedIndex = 0;
				break;
			default:
				break;
		} 
	} 
}

// -- PURPOSE: to empty a <select>
// -- Usage: clearLlist(list to empty, text to display afterwards)

function clearList(myList, defaultText) {
    for (var i=myList.options.length-1; i>=0; i--){
        myList.options[i] = null;
    }
    myList.options[0] = new Option(defaultText, "", false, false);
}

//-- looks for an object with ID "text", and 
//assumes it's either an IFRAME or a TEXTAREA
//
function clearMessage() {
	var editor_obj  = document.getElementById('text');

	if (editor_obj) { //make sure we found an object with the right name
	
		//IFRAME treatment
	  if (editor_obj.tagName == 'IFRAME') {
			editor_obj.contentWindow.document.body.innerHTML = '';
		}
		
		//TEXTAREA treatment (the richtext editor displays a TEXTAREA for Safari instead of an IFRAME like normal)
		if (editor_obj.tagName == 'TEXTAREA') {
			if (tinyMCE) {
				tinyMCE.getInstanceById('mce_editor_0').getWin().document.body.innerHTML = '';
			}
			else {
				editor_obj.value = '';
			}
		}
	}
}

// -- PURPOSE: to collapse all elements
// -- Usage: collapseAll(list of items to collapse)

function collapseAll() {
    for (var i=0; i<collapseAll.arguments.length; i++) {
        var element = document.getElementById(collapseAll.arguments[i]);
        element.style.display = "none";
    }
}



// -- PURPOSE: to expand an item and collapse many others
// -- Usage: expandFirst(item to expand, list of items to collapse)

function collapseFirst() {
	document.getElementById(collapseFirst.arguments[0]).style.display = "none";
	for (var i=1; i<collapseFirst.arguments.length; i++) {
		document.getElementById(collapseFirst.arguments[i]).style.display = "block";
	}
}

//-- looks for an object with ID "text", and 
//assumes it's either an IFRAME or a TEXTAREA
//
function replaceMessage(newMessage) {
	var editor_obj  = document.getElementById('text');

	if (editor_obj) { //make sure we found an object with the right name
		//IFRAME treatment
	  if (editor_obj.tagName == 'IFRAME') {
			editor_obj.contentWindow.document.body.innerHTML = newMessage;
		}
		
		//TEXTAREA treatment (the richtext editor displays a TEXTAREA for Safari instead of an IFRAME like normal)
		if (editor_obj.tagName == 'TEXTAREA') {
			if (tinyMCE) {
				tinyMCE.getInstanceById('mce_editor_0').getWin().document.body.innerHTML = newMessage;
			}
			else {
				editor_obj.value = newMessage;
			}
		}
	}
}
