
//
// -- contensive base: included for all base addons: adminSite, pageManager, etc
//
var ImagePropertyEditorObject;
var DialogObject;
var ACPropertyEditorObject;
var ACPropertyObject;
var TableCellPropertyObject;
var TableRowPropertyObject;
var TablePropertyObject;
var TableBodyPropertyObject;
var tableDialogEditor;
var currentRow;
var currentCol;
var rtNumRows;
var rtNumCols;
var rtTblAlign;
var rtTblWidth;
var outOfContext;
//
// FlyoutMenu Globals
//
var activeButton = null;
var selectsHidden = false;
var menuClicks = 0;
var width;
var height;
var flag = 0;
var updated = 0;
var clickButton;
var lastClipCheck = false;
var lastSelCheck = false;
var currentPanel;
//
var ButtonObjectCount = 0;
var MenuObjectCount = 0;
var MenuHideBlock = false;
var isNS = (navigator.appName == "Netscape");
var isMacIE = ((navigator.userAgent.indexOf("IE 4") > -1) && (navigator.userAgent.indexOf("Mac") > -1));
var browser = new BrowserType();
var RLMediaType = '';
//
//	build admin site workspace
//
var AFinnerHTML;
//
//
// cross browser window size function
//
window.size = function () {
    console.log("contensive.base.window.size");
    var w = 0;
    var h = 0;

    //IE
    if (!window.innerWidth) {
        //strict mode
        if (!(document.documentElement.clientWidth == 0)) {
            w = document.documentElement.clientWidth;
            h = document.documentElement.clientHeight;
        }
        //quirks mode
        else {
            w = document.body.clientWidth;
            h = document.body.clientHeight;
        }
    }
    //w3c
    else {
        w = window.innerWidth;
        h = window.innerHeight;
    }
    return { width: w, height: h };
}
//
//----------
//
function DeleteCheck() {
    console.log("base.DeleteCheck");
    return confirm("This action will delete this record.\n\nAre you sure you would like to delete this record?\n\n")
}
//
//----------
//
function DeleteCheckWithChildren() {
    console.log("base.DeleteCheckWithChildren");
    return confirm("This action will delete this record and its child records.\n\nAre you sure you would like to delete this record?\n\n")
}
//
//----------
//
function DeletePageCheck() {
    console.log("base.DeletePageCheck");
    return confirm("This action will delete this page and its child pages.\n\nAre you sure you would like to delete this page?\n\n")
}
//
//----------
//
function CheckInputs(TargetID, SetValue) {
    console.log("base.CheckInputs");
    var e, ptr;
    e = document.getElementsByTagName('input');
    for (ptr = 0; ptr < e.length; ptr++) {
        if (e[ptr].id == TargetID) { e[ptr].checked = SetValue }
    }
}
//
//----------
//
function SetFieldName(FieldName) {
    console.log("base.SetFieldName");
    document.all['fn'].value = FieldName; return true;
}
//
// ----- hide select menus
//
function hideselect(hiddenIn) {
    console.log("base.hideselect");
    var i, j, index, objs, wmode, paramName
    //
    if (hiddenIn) {
        menuClicks++;
    }
    //
    if (!hiddenIn && menuClicks == 0 && !selectsHidden) {
        return;
    }
    // Hide MS Selects
    if (browser.isIE) {
        //alert('ie');
        objs = document.getElementsByTagName("SELECT");
        for (i = 0; i < objs.length; i++) {
            if (selectsHidden && menuClicks == 0) {
                objs[i].style.visibility = "";
            } else {
                objs[i].style.visibility = "hidden";
            }
        }
        // Hide Objects if no <Param name=wmode> for ie
        objs = document.getElementsByTagName("OBJECT");
        for (i = 0; i < objs.length; i++) {
            if (selectsHidden && menuClicks == 0) {
                objs[i].style.visibility = "";
            } else {
                //var wmodeSet=false;
                var p = objs[i].getElementsByTagName("param");
                for (pi = 0; pi < p.length; pi++) {
                    paramName = p[pi].name.toLowerCase();
                    if (paramName == 'wmode') {
                        wmode = p[pi].value;
                        if (wmode) {
                            wmode = wmode.toLowerCase();
                            if (wmode == 'opaque') {
                                // it hides itself
                            } else if (wmode == 'transparent') {
                                // it hides itself
                            } else if (wmode == 'window') {
                                // it hides itself
                                objs[i].style.visibility = "hidden";
                            } else {
                                objs[i].style.visibility = "hidden";
                            }
                        }
                    }
                }
            }
        }
    } else {
        // Hide MS Embeds for Mozilla
        objs = document.getElementsByTagName("EMBED");
        for (i = 0; i < objs.length; i++) {
            if (selectsHidden && menuClicks == 0) {
                objs[i].style.visibility = "";
            } else {
                wmode = objs[i].getAttribute("wmode")
                if (wmode) {
                    wmode = wmode.toLowerCase();
                }
                if (wmode == 'opaque') {
                    // it hides itself
                } else if (wmode == 'transparent') {
                    // it hides itself
                } else if (wmode == 'window') {
                    // it hides itself
                } else {
                    objs[i].style.visibility = "hidden";
                }
            }
        }
    }
    //
    if (selectsHidden && menuClicks == 0) {
        selectsHidden = false;
    }
    //
    else if (selectsHidden && menuClicks != 0 && clickButton != 2) {
        menuClicks--;
    }
    //
    else {
        selectsHidden = true;
    }
}
//
// ----- Browser Detect
//
function BrowserType() {
    console.log("base.BrowserType");
    var ua, s, i;
    this.isIE = false;  // Internet Explorer
    this.isNS = false;  // Netscape
    //this.version = null;
    this.isMac = false;
    ua = navigator.userAgent;
    s = "MSIE";
    this.version = 0.0;
    if (navigator.appVersion.indexOf("Mac") != -1) {
        this.isMac = true;
    }
    if ((i = ua.indexOf(s)) >= 0) {
        this.isIE = true;
        this.version = parseFloat(ua.substr(i + s.length));
        return;
    }
    s = "Netscape6/";
    if ((i = ua.indexOf(s)) >= 0) {
        this.isNS = true;
        this.version = parseFloat(ua.substr(i + s.length));
        return;
    }
    s = "Gecko";
    if ((i = ua.indexOf(s)) >= 0) {
        this.isNS = true;
        this.version = 6.1;
        return;
    }
}
//
// ----- Code for handling the menu bar and active button.
//
function PageClick(event) {
    console.log("base.PageClick");
    var el;
    if (activeButton == null) return;
    if (browser.isIE)
        el = window.event.srcElement;
    else
        el = (event.target.tagName ? event.target : event.target.parentNode);

    if (el == activeButton)
        return;
    if (getContainerWith(el, "DIV", "kmaMenu") == null) {
        resetButton(activeButton);
        activeButton = null;
    }
}
//
// ----- Clear the menu
//
function ClearMenu() {
    console.log("base.ClearMenu");
    if (activeButton == null) return;
    resetButton(activeButton);
    activeButton = null;
}
//
// -----
//
function ccFlyoutHoverMode(val) {
    console.log("base.ccFlyoutHoverMode");
    flag = val;
    if (val == 0) {
        setTimeout(timerEvent, 1000);
    }
    if (val == 1) {
        updated = 1;
    }
    return;
}
//
// -----
//
function timerEvent() {
    console.log("base.timerEvent");
    if (flag == 0) {
        resetFlag = 1;
        resetButton(activeButton);
        menuClicks = 0;
        hideselect(0);
        updated = 1;
    }

}
//
// -----
//
function setAnchorTagsToColor(classToFind, colorToSet) {
    console.log("base.setAnchorTagsToColor");
    var i;
    var aTags = document.getElementsByTagName("A");
    for (i = 0; i < aTags.length; i++) {
        if (hasClassName(aTags[i], classToFind)) {
            aTags[i].style.color = colorToSet;
        }
    }
}
//
// -----
//
function ccFlyoutButtonClick(event, menuId, position, StyleSheetPrefix, OpenOnHover) {
    console.log("base.ccFlyoutButtonClick");
    var button;
    var x, y;
    var offY;
    //
    if ((!(clickButton == 2 && outOfContext))) {
        //if ((!(clickButton == 2 && outOfContext)) && browser.isIE) {
        hideselect(true);
    }
    if (browser.isIE) {
        offY = event.offsetY;
        button = event.srcElement;
    } else {
        button = event.currentTarget;
    }
    if (button.tagName != 'A') {
        button = getContainer(button, 'A')
        if (button == null) return false;
    }
    if (menuId == '') return false;
    width = document.body.scrollWidth;
    height = document.body.scrollHeight;
    button.blur();
    if (button.menu == null) {
        button.menu = document.getElementById(menuId);
        if (button.menu.title == "")
            menuInit(button.menu);
    }
    if (activeButton != null)
        resetButton(activeButton);
    if (button != activeButton || updated == 1) {
        updated = 0;
        // Update the button's style so it looks down
        if (clickButton != 2) {
            button.className += " kmaMenuDown " + StyleSheetPrefix + "ButtonDown";
        }
        //button.className += " ccFlyoutButtonDown";
        // put menu under button
        if (position == 1) {
            // Flyout to the right
            x = getPageOffsetLeft(button) + button.offsetWidth;
            y = getPageOffsetTop(button);
        } else if (position == 2) {
            // Flyout up
            x = getPageOffsetLeft(button);
            y = getPageOffsetTop(button) - button.menu.offsetHeight;
        } else if (position == 3) {
            // Flyout to the left
            x = getPageOffsetLeft(button) - button.menu.offsetWidth;
            y = getPageOffsetTop(button);
        } else {
            // Flyout down
            x = getPageOffsetLeft(button);
            y = getPageOffsetTop(button) + button.offsetHeight;

        }

        if (clickButton == 2) {
            x = event.clientX;
            y = event.clientY;
            if (document.body) {
                x += document.body.scrollLeft;
                y += document.body.scrollTop;
            }
        }

        if ((x + button.menu.scrollWidth) > width) {
            x = width - button.menu.scrollWidth;
        }

        button.menu.style.left = x + "px";
        button.menu.style.top = y + "px";
        // turn it on
        button.menu.style.visibility = "visible";
        activeButton = button;
    }
    else {
        if (!OpenOnHover) {
            activeButton = null;
        }
    }
    clickButton = 1;
    return false;
}
//
// -----
//
function ccFlyoutButtonHover(event, menuId, position) {
    console.log("base.ccFlyoutButtonHover");
    var button;
    if (browser.isIE)
        button = window.event.srcElement;
    else
        button = event.currentTarget;
    if (activeButton != null && activeButton != button)
        ccFlyoutButtonClick(event, menuId, position);
}
//
// -----
//
function resetButton(button) {
    console.log("base.resetButton");
    if (button) {
        removeClassNameAfter(button, "kmaMenuDown");
        if (button.menu != null) {
            closeSubMenu(button.menu);
            button.menu.style.visibility = "hidden";
        }
    }
}
//
// -----
//
function activateButton(button) {
    console.log("base.activateButton");
    if (button.menu != null) {
        button.menu.style.visibility = "visible";
    }
}
//
// -----
//
function ccFlyoutPanelHover(event, StyleSheetPrefix) {
    console.log("base.ccFlyoutPanelHover");
    var menu;
    if (browser.isIE)
        menu = getContainerWith(window.event.srcElement, "DIV", StyleSheetPrefix + "Panel");
    //menu = getContainerWith(window.event.srcElement, "DIV", "ccFlyoutPanel");
    else
        menu = event.currentTarget;
    if (menu.title != "") {
        if (menu.activeItem == null) {
            // do nothing
        } else {
            closeSubMenu(menu);
        }
    }
}
//
// -----
//
function ccFlyoutPanelButtonHover(event, menuId, StyleSheetPrefix) {
    console.log("base.ccFlyoutPanelButtonHover");
    var item, menu, x, y;
    var maxX, maxY;
    if (browser.isIE)
        item = getContainerWith(window.event.srcElement, "A", StyleSheetPrefix + "PanelButton");
    //item = getContainerWith(window.event.srcElement, "A", "ccFlyoutPanelButton");
    else
        item = event.currentTarget;
    menu = getContainerWith(item, "DIV", StyleSheetPrefix + "Panel");
    if (menu.activeItem == null) {
        // nothing
    } else {
        closeSubMenu(menu);
    }
    if (!(menuId)) return false
    if (menuId == '') return false;
    menu.activeItem = item;
    item.className += " kmaMenuDown " + StyleSheetPrefix + "PanelButtonDown";
    if (item.subMenu == null) {
        item.subMenu = document.getElementById(menuId);
        if (item.subMenu.title == "")
            //if (item.subMenu.isInitialized == null)
            menuInit(item.subMenu);
    }
    x = getPageOffsetLeft(item) + item.offsetWidth;
    y = getPageOffsetTop(item);
    if (browser.isNS) {
        maxX = window.scrollX + window.innerWidth;
        maxY = window.scrollY + window.innerHeight;
    }
    if (browser.isIE) {
        maxX = (document.documentElement.scrollLeft != 0 ? document.documentElement.scrollLeft : document.body.scrollLeft)
            + (document.documentElement.clientWidth != 0 ? document.documentElement.clientWidth : document.body.clientWidth);
        maxY = (document.documentElement.scrollTop != 0 ? document.documentElement.scrollTop : document.body.scrollTop)
            + (document.documentElement.clientHeight != 0 ? document.documentElement.clientHeight : document.body.clientHeight);
    }
    maxX -= item.subMenu.offsetWidth;
    maxY -= item.subMenu.offsetHeight;
    if (x > maxX)
        x = Math.max(0, x - item.offsetWidth - item.subMenu.offsetWidth + (menu.offsetWidth - item.offsetWidth));
    y = Math.max(0, Math.min(y, maxY));
    item.subMenu.style.left = x + "px";
    if (browser.isMac) {
        if (browser.version > 5.1) {
            item.subMenu.style.top = y + "px";
        }
    } else {
        item.subMenu.style.top = y + "px";
    }
    item.subMenu.style.visibility = "visible";
    if (browser.isIE)
        window.event.cancelBubble = true;
    else
        event.stopPropagation();
}
//
// -----
//
function closeSubMenu(menu) {
    console.log("base.closeSubMenu");
    if (!menu)
        return;
    if (menu.activeItem == null)
        return;
    if (menu.activeItem.subMenu) {
        closeSubMenu(menu.activeItem.subMenu);
        menu.activeItem.subMenu.style.visibility = "hidden";
        menu.activeItem.subMenu = null;
    }
    removeClassNameAfter(menu.activeItem, "kmaMenuDown");
    //removeClassName(menu.activeItem, "ccFlyoutPanelButtonDown");
    menu.activeItem = null;
}
//
// -----
//
function menuInit(menu) {
    console.log("base.menuInit");
    var itemList, spanList;
    var textEl, arrowEl;
    var itemWidth;
    var w, dw;
    var i, j;
    itemList = menu.getElementsByTagName("A");
    if (itemList.length == 0) {
        //menu.title = 'Select One';
        menu.activeItem = null
        return false;
    }
    if (browser.isIE) {
        w = itemList[0].offsetWidth;
        itemList[0].style.width = w + "px";
        dw = itemList[0].offsetWidth - w;
        w -= dw;
        itemList[0].style.width = w + "px";
    }
    //menu.title = 'Select One';
    menu.activeItem = null
}
//
// -----
//
function getContainerWith(node, tagName, className) {
    console.log("base.getContainerWith");
    while (node != null) {
        if (node.tagName != null && node.tagName == tagName &&
            hasClassName(node, className))
            return node;
        node = node.parentNode;
    }
    return node;
}
//
// -----
//
function getContainer(node, tagName) {
    console.log("base.getContainer");
    while (node != null) {
        if (node.tagName != null && node.tagName == tagName)
            return node;
        node = node.parentNode;
    }
    return node;
}
//
// -----
//
function hasClassName(el, name) {
    console.log("base.hasClassName");
    var i, list;
    list = el.className.split(" ");
    for (i = 0; i < list.length; i++)
        if (list[i] == name) return true;
    return false;
}
//
// -----
//
function removeClassName(el, name) {
    console.log("base.removeClassName");
    var i, curList, newList;
    if (el.className == null) return;
    //newList = new Array();
    curList = el.className.split(" ");
    for (i = 0; i < curList.length; i++)
        if (curList[i] != name) {
            newList = newList + ' ' + curList[i];
        } else {
            //newList=newList+' '+curList[i];
            el.className = newList;
            return;
        }
}
//
// -----
//
function removeClassNameAfter(el, name) {
    console.log("base.removeClassNameAfter");
    var i, curList, newList;
    if (el.className == null) return;
    curList = el.className.split(" ");
    newList = '';
    for (i = 0; i < curList.length; i++)
        if (curList[i] != name) {
            newList = newList + ' ' + curList[i];
        } else {
            // newList=newList+' '+curList[i];
            el.className = newList;
            return;
        }
}
//
//----------
//
function getPageOffsetLeft(el) {
    console.log("base.getPageOffsetLeft");
    var offset = jQuery(el).position();
    return offset.left;
}
//
//----------
//
function getPageOffsetTop(el) {
    console.log("base.getPageOffsetTop");
    var offset = jQuery(el).position();
    return offset.top;
}
//
//----------
//
function updateClipboardLinks() {
    console.log("base.updateClipboardLinks");
    if (!browser.isIE) {
        return;
    }
    var SelectedObject;
    var TextRange;
    setAnchorTagsToColor("ccPaste", "#000000");
    lastClipCheck = true;
    SelectedObject = document.selection;
    TextRange = SelectedObject.createRange();
    if (TextRange.text) {
        if (TextRange.text.length != 0) {
            if (!lastSelCheck) {
                setAnchorTagsToColor("ccSelect", "#000000");
                lastSelCheck = true;
            }
        }
        else {
            if (lastSelCheck) {
                setAnchorTagsToColor("ccSelect", "#FFFFFF");
                lastSelCheck = false;
            }
        }
    }

}
//
//----------
//
function updateClipboardLinksInit() {
    console.log("base.updateClipboardLinksInit");
    if (!browser.isIE) {
        return;
    }
    setAnchorTagsToColor("ccPaste", "#000000");
    lastClipCheck = true;
    setAnchorTagsToColor("ccSelect", "#FFFFFF");
}
//
//----------
//
function SaveFieldPlus(ObjectName) {
    console.log("base.SaveFieldPlus");
    var RefreshTemp;
    RefreshTemp = document.all.tags('div')[ObjectName].innerHTML;
    document.all[ObjectName + 'Field'].value = RefreshTemp;
}
//
//----------
//
function OpenImagePropertyWindow(EditorObject, ImageObject) {
    console.log("base.OpenImagePropertyWindow");
    DialogObject = ImageObject;
    showModalDialog("/ContensiveBase/Popup/ActiveEditorImageProperties.htm", window, "status:false;dialogWidth:32em;dialogHeight:25em");
}
//
//----------
//
function OpenACPropertyWindow(EditorObject, ACObject) {
    console.log("base.OpenACPropertyWindow");
    ACPropertyObject = ACObject;
    showModalDialog("/ContensiveBase/Popup/ActiveEditorACProperties.htm", window, "status:false;dialogWidth:18em;dialogHeight:14em");
}
//
//----------
//
function OpenSiteExplorerWindow(TargetInputID) {
    console.log("base.OpenSiteExplorerWindow");
    window.open('?ccIPage=kdif3318sd&LinkObjectName=' + TargetInputID, 'PageSelector', 'menubar=no,toolbar=no,location=no,status=no,scrollbars=yes,resizable')
}
//
//----------
//
function OpenResourceLinkWindow(TargetInputID) {
    console.log("base.OpenResourceLinkWindow");
    window.open('/admin/index.asp?ccIPage=s033l8dm15&SourceMode=0&LinkObjectName=' + TargetInputID, 'ResourceSelector', 'menubar=no,toolbar=no,location=no,status=no,scrollbars=yes,resizable')
}
//
//----------
//
function OpenTablePropertyWindow(EditorObject, TableObject, TableBodyObject) {
    console.log("base.OpenTablePropertyWindow");
    TablePropertyObject = TableObject;
    TableBodyPropertyObject = TableBodyObject;
    ClearMenu();
    var h = new Object();
    h.win = window;
    h.fieldName = EditorObject;
    showModalDialog("/ContensiveBase/Popup/tablepop.htm", h, "status:false;dialogWidth:315px;dialogHeight:380px;");
}
//
//----------
//
var globalAddonIconObj;
//
function OpenAddonPropertyWindow(AddonIconObj, AdminURL) {
    console.log("base.OpenAddonPropertyWindow");
    var RawString, SplitString, addonOptionString, addonGuid;
    //
    globalAddonIconObj = AddonIconObj;
    ACPropertyObject = AddonIconObj;
    RawString = new String(globalAddonIconObj.id);
    SplitString = RawString.split(",");
    try {
        addonOptionString = SplitString[4];
        addonGuid = SplitString[5];
    } catch (err) {
    }
    if ((!addonOptionString) && (addonGuid)) {
        cj.ajax.qsCallback('ajaxfn=ccGetDefaultAddonOptionString&guid=' + addonGuid, OpenAPWithOptionString, AdminURL)
    } else {
        return OpenAPWithOptionString(addonOptionString, AdminURL);
    }
}
//
//----------
//
function OpenAPWithOptionString(addonOptionString, AdminURL) {
    console.log("base.OpenAPWithOptionString");
    var RawString, SplitString;
    var PairPointer;
    var PairSplit;
    var Spacer;
    var NameVar;
    var ValueVar;
    var NVPair;
    var s;
    var RowPtr;
    var OptionStart;
    var OptionEnd;
    var FormInput;
    var Options;
    var OptionSplit;
    var OptionPtr;
    var CurrentValue;
    var OptionValue;
    var OptionCaption;
    var buttonRow = '';
    var args = '';
    RawString = new String(globalAddonIconObj.id);
    SplitString = RawString.split(",")
    Spacer = '<img src=/ContensiveBase/Images/Spacer.gif width=1 height=20>'
    s = '';
    s += '<table border=0 cellpadding="5" cellspacing="0" width="100%">';
    PairPointer = 0
    NVPair = addonOptionString.split('&');
    RowPtr = 0;
    while (NVPair[PairPointer]) {
        PairSplit = NVPair[PairPointer].split('=');
        if (PairSplit[0]) {
            NameVar = PairSplit[0];
            NameVar = DecodeAddonOptionArgument(NameVar);
            ValueVar = '';
            Options = '';
            if (PairSplit[1]) {
                PairSplit[0] = '';
                ValueVar = PairSplit.join('=');
                ValueVar = ValueVar.substr(1);
            }
            OptionStart = ValueVar.indexOf('[');
            OptionEnd = ValueVar.indexOf(']');
            FormInput = '';
            if ((OptionStart != -1) && (OptionEnd != -1)) {
                // selector in format: name=value[caption:option|caption:option]type
                var Type = ValueVar.substr(OptionEnd + 1).toLowerCase()
                CurrentValue = '';
                if (OptionStart > 0) {
                    CurrentValue = ValueVar.slice(0, OptionStart);
                    CurrentValue = DecodeAddonOptionArgument(CurrentValue);
                }
                var CurrentValueList = ',' + CurrentValue + ','
                CurrentValueList = CurrentValueList.toLowerCase();
                Options = ValueVar.slice(OptionStart + 1, OptionEnd);
                OptionSplit = Options.split('|');
                OptionPtr = 0;
                while (OptionSplit[OptionPtr]) {
                    OptionValue = OptionSplit[OptionPtr]
                    Pos = OptionValue.indexOf(':')
                    if (Pos > -1) {
                        OptionCaption = OptionValue.substr(0, Pos);
                        OptionCaption = DecodeAddonOptionArgument(OptionCaption)
                        OptionValue = OptionValue.substr(Pos + 1);
                        OptionValue = DecodeAddonOptionArgument(OptionValue)
                    } else {
                        OptionValue = DecodeAddonOptionArgument(OptionValue);
                        OptionCaption = OptionValue;
                    }
                    switch (Type) {
                        case 'checkbox':
                            if (CurrentValueList.indexOf(',' + OptionValue.toLowerCase() + ',') >= 0) {
                                FormInput += '<div style="white-space:nowrap"><input type="checkbox" name="' + NameVar + '" value="' + OptionValue + '" checked="checked"/>' + OptionCaption + '</div>';
                            } else {
                                FormInput += '<div style="white-space:nowrap"><input type="checkbox" name="' + NameVar + '" value="' + OptionValue + '" />' + OptionCaption + '</div>';
                            }
                            break;
                        case 'radio':
                            if (CurrentValueList.indexOf(',' + OptionValue.toLowerCase() + ',') >= 0) {
                                FormInput += '<div style="white-space:nowrap"><input type="radio" name="' + NameVar + '" value="' + OptionValue + '" checked="checked"/>' + OptionCaption + '</div>';
                            } else {
                                FormInput += '<div style="white-space:nowrap"><input type="radio" name="' + NameVar + '" value="' + OptionValue + '" />' + OptionCaption + '</div>';
                            }
                            break;
                        default:
                            if (CurrentValue == OptionValue) {
                                FormInput += '<option value="' + OptionValue + '" selected>' + OptionCaption + '</option>';
                            } else {
                                FormInput += '<option value="' + OptionValue + '">' + OptionCaption + '</option>';
                            }
                    }
                    OptionPtr++;
                }
                switch (Type) {
                    case 'checkbox':
                        FormInput += '<INPUT ID="NVType' + PairPointer + '" name="' + NameVar + '" Type=Hidden Value="checkbox">';
                        FormInput += '<INPUT ID="NV' + PairPointer + '" name="' + NameVar + '" Type=Hidden Value="checkbox">';
                        break;
                    case 'radio':
                        FormInput += '<INPUT ID="NVType' + PairPointer + '" name="' + NameVar + '" Type=Hidden Value="radio">';
                        FormInput += '<INPUT ID="NV' + PairPointer + '" name="' + NameVar + '" Type=Hidden Value="radio">';
                        break;
                    case 'resourcelink':
                        ValueVar = DecodeAddonOptionArgument(ValueVar)
                        FormInput += '<INPUT ID="NVType' + PairPointer + '" name="' + NameVar + '" Type=Hidden Value="resourcelink">';
                        FormInput += '<INPUT Name="' + NameVar + '" Type=Text ID="NV' + PairPointer + '" Value="' + CurrentValue + '" Size=20>';
                        FormInput += '&nbsp;<a href="#" onClick="'
                        FormInput += "window.open('" + AdminURL + "?ccIPage=s033l8dm15&SourceMode=0&LinkObjectName=NV" + PairPointer + "','ResourceSelector','menubar=no,toolbar=no,location=no,status=no,scrollbars=yes,resizable');"
                        FormInput += 'return false;"><img src="/ContensiveBase/images/ResourceLink1616.gif" width=16 height=16 border=0 alt="Link to a resource" title="Link to a resource"></a>'
                        break;
                    default:
                        FormInput += '<INPUT ID="NVType' + PairPointer + '" name="' + NameVar + '" Type=Hidden Value="select">';
                        FormInput = '<select ID="NV' + PairPointer + '" name="' + NameVar + '" size=1 />' + FormInput + '</select>';
                }
                FormInput += '<INPUT Name="NVO' + PairPointer + '" Type=Hidden ID="NVO' + PairPointer + '" Value="[' + Options + ']' + Type + '">';

            } else {
                ValueVar = DecodeAddonOptionArgument(ValueVar)
                FormInput += '<INPUT ID="NVType' + PairPointer + '" name="' + NameVar + '" Type=Hidden Value="text">';
                FormInput += '<INPUT Name="' + NameVar + '" Type=Text ID="NV' + PairPointer + '" Value="' + ValueVar + '" Size=20>';
            }
            s += '<tr>';
            s += '<td class="left">' + NameVar + '</td>'
            s += '<td class="right">' + FormInput + '</td>'
            s += '</tr>';
            RowPtr++
        }
        PairPointer++
    }
    if (RowPtr == 0) {
        s += '<tr><td class=ccPanelNormal align="center" colspan="2">This object has no properties.</td></tr>';
    }
    s += '</table>';
    copyButton = ''
    if (RowPtr != 0) {
        copyButton += '<INPUT Type=Button Value="Update" onClick="SaveForm(); window.close();">';
    }
    s = '\n<div class="ccCon">' +
        '\n<table border=0 cellpadding=0 cellspacing=0 class="ccDialogCon">' +
        '\n<tr><td class="ccButtonCon">' + copyButton + '</td></tr>' +
        '\n<tr><td class="ccContentCon">' + s + '</td></tr>' +
        '\n</table>' +
        '\n</div>';
    var obj = document.createElement('div');
    obj.style.position = 'absolute';
    obj.style.visibility = 'hidden';
    obj.innerHTML = s;
    var b = document.getElementsByTagName("body")[0];
    b.appendChild(obj);
    AFinnerHTML = s;
    args = 'scroll:no;status:false;help:no;dialogWidth:' + (obj.scrollWidth) + 'px;dialogHeight:' + (obj.scrollHeight) + 'px'
    showModalDialog('/ContensiveBase/Popup/AFTest.htm', window, args);
}
//
//----------
//
function InsertTag(OpenTag, CloseTag) {
    console.log("base.InsertTag");
    var SelectionObject;
    var SelectionObjectType;
    var ControlRangeCollection;
    var ParentElementObject;
    var Pointer;
    var ControlRange;
    //
    SelectionObject = document.selection;
    SelectionObjectType = SelectionObject.type;
    if (SelectionObjectType == "Control") {
        ControlRangeCollection = SelectionObject.createRange();
        for (Pointer = 0; Pointer < ControlRangeCollection.length; Pointer++) {
            ControlRange = ControlRangeCollection(Pointer);
            ControlRange.outerHTML = OpenTag + ControlRange.outerHTML + CloseTag;
        }
    } else {
        var TextRange = SelectionObject.createRange();
        var text = TextRange.text;
        if (text) {
            TextRange.pasteHTML(OpenTag + text + CloseTag);
        } else {
            TextRange.pasteHTML(OpenTag + CloseTag);
        }
        ClearMenu();
    }
}
//
//----------
//
function PrintElement(ElementIdentifier) {
    console.log("base.PrintElement");
    var ElementObject = document.all(ElementIdentifier);
    var tr = ElementObject.createTextRange();
    var msgWind = window.open('', '_blank', 'scrollbars=yes,toolbar=no,status=no,resizable=yes');
    msgWind.document.body.innerHTML = tr.text;
    msgWind.print();
    msgWind.close();
}
//
//----------
//
function RemoveStyle(ElementIdentifier) {
    console.log("base.RemoveStyle");
    var ElementObject;
    var SelectedObject;
    var TextRange;
    ElementObject = document.all(ElementIdentifier);
    ElementObject.focus;
    SelectedObject = document.selection;
    TextRange = SelectedObject.createRange();
    TextRange.pasteHTML(TextRange.text);
    ClearMenu();
}

//
//----------
//
function ExecCmdPlus(cmd, ui, opt, ObjectName) {
    console.log("base.ExecCmdPlus");
    var CommandPassed, editor;
    editor = document.all[ObjectName]
    if (!editor.document.queryCommandSupported(cmd)) {
        alert('This function is not supported. Your browser may not support this command, or you may need to make a text selection before using this function.')
    } else {
        document.body.all.tags('div')[ObjectName].focus();
        CommandPassed = editor.document.execCommand(cmd, ui, opt);
        if (CommandPassed) {
            // removed to allow formating undo -- onblur covers the save case
            //SaveFieldPlus(ObjectName);
            document.body.all.tags('div')[ObjectName].focus();
        } else {
            alert('This function failed. Your browser may not support this command, or you may need to make a text selection before using this function.')
        }
    }
    ClearMenu();
}
//
//----------
//
function OpenResourceLibrary(EditorObjectName) {
    console.log("base.OpenResourceLibrary");
    window.open('?ccIPage=s033l8dm15&EditorObjectName=' + EditorObjectName, 'ImageSelector', 'menubar=no,toolbar=no,location=no,status=no,scrollbars=yes,resizable')
}
//
//----------
//
function InsertImage(Name, Src, Alt, Width, Height, ObjectName) {
    console.log("base.InsertImage");
    var tag
    tag = '<IMG SRC="' + Src + '"'
    tag += 'name="' + Name + '" ID="' + Name + '"'
    if (Width != 0) tag += ' WIDTH="' + Width + '"'
    if (Height != 0) tag += ' HEIGHT="' + Height + '"'
    tag += ' BORDER=0>'
    oUtil.obj.insertHTML(tag);
}
//
//----------
//
function InsertDownload(RecordID, ObjectName, ImageLink) {
    console.log("base.InsertDownload");
    var tag;
    tag = '<a href="?downloadid=' + RecordID + '" target="_blank"><img src="' + ImageLink + '" border=0 width=16 height=16 alt="Download" ></a>';
    oUtil.obj.insertHTML(tag);

}
//
//----------
//
function InsertActiveContentPlus(ACType, ACSubType, ACArg1, ObjectName) {
    console.log("base.InsertActiveContentPlus");
    var SelectionObject;
    var SelectionObjectType;
    var ControlRangeObject;
    var ParentElementObject;
    var Pointer;
    var ImageObject;
    ExecCmdPlus('InsertImage', false, '/ContensiveBase/images/AC' + ACType + ACSubType + '.gif', ObjectName);
    SelectionObject = document.selection;
    SelectionObjectType = SelectionObject.type;
    if (SelectionObjectType != "Control") {
        // 
    } else {
        ControlRangeObject = SelectionObject.createRange();
        for (Pointer = 0; Pointer < ControlRangeObject.length; Pointer++) {
            ImageObject = ControlRangeObject(Pointer);
            if (ImageObject.tagName == "IMG") {
                ImageObject.id = 'AC,' + ACType + ',' + ACSubType + ',' + ACArg1;
            } else {
                // Non-Image found in Control Object
            }
        }
    }
    ClearMenu();
}
//
//----------
//
function InsertActiveChildList(ACType, ObjectName) {
    console.log("base.InsertActiveChildList");
    var RandomNumber;
    RandomNumber = Math.floor(Math.random() * 1000000);
    InsertActiveContentPlus(ACType, '', RandomNumber, ObjectName);
}
//
//----------
//
function OpenPropertiesWindowPlus(ObjectName) {
    console.log("base.OpenPropertiesWindowPlus");
    var EditorObject;
    var SelectionObject;
    var TextRangeObject;
    var ControlRangeObject;
    var Message;
    var ElementObject;
    var Pointer;
    var t;
    // Locate the object being clicked 
    SelectionObject = document.selection;

    if (SelectionObject.type == "Text") {
        // Text Object Found
    } else if (SelectionObject.type == "Control") {
        ControlRangeObject = SelectionObject.createRange();
        for (Pointer = 0; Pointer < ControlRangeObject.length; Pointer++) {
            ElementObject = ControlRangeObject(Pointer);
            if (ElementObject.tagName == "IMG") {
                if (ElementObject.id.substring(0, 3) == "AC,") {
                    if (ElementObject.id.substring(0, 8) == "AC,IMAGE") {
                        OpenImagePropertyWindow(document.all[ObjectName], ElementObject);
                    } else if (ElementObject.id.substring(0, 20) == "AC,AGGREGATEFUNCTION") {
                        OpenAggregateFunctionPropertyWindow(document.all[ObjectName], ElementObject);
                    } else {
                        OpenACPropertyWindow(document.all[ObjectName], ElementObject);
                    }
                } else {
                    OpenImagePropertyWindow(document.all[ObjectName], ElementObject);
                }
            }
            else if (ElementObject.tagName == "TABLE") {
                var tBody = ElementObject.getElementsByTagName("TBODY");
                OpenTablePropertyWindow(ObjectName, ElementObject, tBody[0]);
            }
            else {
                // Non-Image found in Control Object
            }
        }
    } else {
        // SelectObject is not Control or Text
    }
}
//
//----------
//
function tableDialog(ObjectName) {
    console.log("base.tableDialog");
    ClearMenu();
    var rtNumRows = null;
    var rtNumCols = null;
    var rtTblAlign = null;
    var rtTblWidth = null;
    tableDialogEditor = ObjectName;
    showModalDialog("/ContensiveBase/popup/tablepop.htm", window, "status:false;dialogWidth:275px;");
}
//
//----------
//
function tdDialog(tabIndex, fieldName) {
    console.log("base.tdDialog");
    ClearMenu();
    var h = new Object();
    h.win = window;
    h.tab = tabIndex;
    h.fieldName = fieldName;
    showModalDialog("/ContensiveBase/popup/tdpopup.htm", h, "scroll:off;status:false;dialogWidth:274px;dialogHeight:400px;");
}
//
//----------
//
function createTable() {
    console.log("base.createTable");
    var HTMLEditor = document.all[tableDialogEditor]
    var CellWidth;
    if (rtNumRows == "" || rtNumRows == "0") {
        rtNumRows = "1";
    }
    if (rtNumCols == "" || rtNumCols == "0") {
        rtNumCols = "1";
    }
    var rttrnum = 1;
    var rttdnum = 1;
    var rtNewTable = "<P><table border='1' align='" + rtTblAlign + "' cellpadding='0' cellspacing='0' width='" + rtTblWidth + "'>"
    CellWidth = (100 / rtNumCols);
    while (rttrnum <= rtNumRows) {
        rttrnum = rttrnum + 1
        rtNewTable = rtNewTable + "<tr>"
        while (rttdnum <= rtNumCols) {
            rtNewTable = rtNewTable + '<td width="' + CellWidth + '%">&nbsp;</td>'
            rttdnum = rttdnum + 1
        }
        rttdnum = 1
        rtNewTable = rtNewTable + "</tr>"
    }
    rtNewTable = rtNewTable + "</table></P>"
    HTMLEditor.focus();
    InsertTag(rtNewTable, '');
}
//
//----------
//
function clickIE() {
    console.log("base.clickIE");
    return false;
}
//
//----------
//
function createTableNew() {
    console.log("base.createTableNew");
    rtNumRows = currentRow;
    rtNumCols = currentCol;
    rtTblAlign = "";
    rtTblWidth = "100%";
    createTable();
    rtNumRows = null;
    rtNumCols = null;
    rtTblAlign = null;
    rtTblWidth = null;
}
//
//----------
//
function hideTableIF(ObjectName) {
    console.log("base.hideTableIF");
    var tableDiv = document.all[ObjectName];
    tableDiv.style.visibility = "hidden";
    var tableCreateIF = tableDiv.getElementsByTagName("TABLE");
    var tableCells = tableCreateIF[0].getElementsByTagName("TD");
    for (i = 0; i < tableCells.length; i++) {
        tableCells[i].style.backgroundColor = "#FFFFFF";
    }
}
//
//----------
//
function highlightTDs(ObjectName, num, CellDataHolder) {
    console.log("base.highlightTDs");
    var tableCreateIF = document.all[ObjectName];
    var tableCells = tableCreateIF.getElementsByTagName("TD");
    var tableRows = tableCreateIF.getElementsByTagName("TR");
    var tableWidth = tableCells.length / tableRows.length;
    var col;
    var row;
    var i;
    if (num == 0) {
        col = 1;
    }
    else {
        col = (num + 1) % tableWidth;
        if (col == 0) {
            col = tableWidth;
        }
    }
    row = Math.floor((num + tableWidth) / tableWidth);
    currentCol = col;
    currentRow = row;
    for (i = 0; i < tableCells.length; i++) {
        if (colVal(i, tableWidth) <= col && rowVal(i, tableWidth) <= row) {
            tableCells[i].style.backgroundColor = "#185A98";
        }
        else {
            tableCells[i].style.backgroundColor = "#FFFFFF";
        }
    }
    var TableSize = document.all[CellDataHolder];
    TableSize.value = row + " x " + col;
}
//
//----------
//
function rowVal(num, tableWidth) {
    console.log("base.rowVal");
    return Math.floor((num + tableWidth) / tableWidth);
}
//
//----------
//
function colVal(num, tableWidth) {
    console.log("base.colVal");
    var retVal;
    if (num == 0) {
        retVal = 1;
    }
    else {
        retVal = (num + 1) % tableWidth;
        if (retVal == 0) {
            retVal = tableWidth;
        }
    }
    return retVal;
}
//
//----------
//
function setEditor(objectName) {
    console.log("base.setEditor");
    tableDialogEditor = objectName;
}
//
//----------
//
function editorRightClick(prefix, divName) {
    console.log("base.editorRightClick");
    var possibleTableElement;
    if (parseInt(navigator.appVersion) > 3) {
        var clickType = 1;
        if (navigator.appName == "Netscape") clickType = e.which;
        else clickType = event.button;
        if (clickType != 1) {
            var SelectedObject;
            var tRange;
            SelectedObject = document.selection;
            tRange = SelectedObject.createRange();

            if (SelectedObject.type == "Control") {
                if (tRange.length >= 1) {
                    var selectedEl = tRange(0);
                }
            }
            else {
                var selectedEl = tRange.parentElement();
            }
            if (selectedEl.tagName != "TD") {
                selectedEl = document.elementFromPoint(event.x, event.y);
            }

            if (selectedEl.tagName != "TD") {
                possibleTableElement = findTagAmongstParents(selectedEl, "TD", divName);
            }

            if (possibleTableElement != null) {
                selectedEl = possibleTableElement;
            }
            if (selectedEl.tagName == "TD") {
                var tempEl2 = selectedEl.parentElement;
                do {
                    if (tempEl2.tagName == "TR") {
                        TableRowPropertyObject = tempEl2;
                    }
                    tempEl2 = tempEl2.parentElement;
                } while (tempEl2.tagName != "TBODY");

                TableCellPropertyObject = selectedEl;
                TableBodyPropertyObject = tempEl2;
                TablePropertyObject = tempEl2.parentElement;

                outOfContext = false;
                return prefix + 'TableMenu';

            } // if (selectedEl.tagName == "TD")
            else {
                outOfContext = true;
                return '';
            }
        } // if (clickType!=1)
    } // if (parseInt(navigator.appVersion)>3)
}
//
//----------
//
function findTagAmongstParents(startingElement, tagName, divName) {
    console.log("base.findTagAmongstParents");
    var found;
    var currEl;
    found = false;
    if ((startingElement.tagName == "DIV") && (startingElement.name == divName)) {
        return null;
    }
    currEl = startingElement.parentElement;
    while (currEl != null && !found) {
        if ((currEl.tagName == "DIV") && (currEl.name == divName)) {
            return null;
        }
        if (currEl.tagName == tagName) {
            found = true;
        }
        else {
            currEl = currEl.parentElement;
        }
    }
    return currEl;
}
//
//----------
//
function hideRightMenu(ObjectName) {
    console.log("base.hideRightMenu");
    var i;
    var tableMenu = document.all[ObjectName];
    for (i = 0; i < tableMenu.childNodes.length; i++) {
        if (tableMenu.childNodes[i].tagName == "DIV") {
            tableMenu.childNodes[i].style.visibility = "hidden";
        }
    }
    tableMenu.style.visibility = "hidden";
}
//
//----------
//
function insertRowBefore() {
    console.log("base.insertRowBefore");
    var tRows = TablePropertyObject.getElementsByTagName("TR");
    var tCells = TablePropertyObject.getElementsByTagName("TD");
    var tCols = Math.round(tCells.length / tRows.length);
    trElement = document.createElement("TR");
    TableBodyPropertyObject.insertBefore(trElement, TableRowPropertyObject);
    var i;
    for (i = 0; i < tCols; i++) {
        var nc = trElement.insertCell();
        nc.insertAdjacentText("afterBegin", " ");
    }
    ClearMenu();
}
//
//----------
//
function insertRowAfter() {
    console.log("base.insertRowAfter");
    var tRows = TablePropertyObject.getElementsByTagName("TR");
    var tCells = TablePropertyObject.getElementsByTagName("TD");
    var tCols = Math.round(tCells.length / tRows.length);
    trElement = document.createElement("TR");
    TableBodyPropertyObject.insertBefore(trElement, TableRowPropertyObject);
    TableRowPropertyObject.removeNode(true);
    TableBodyPropertyObject.insertBefore(TableRowPropertyObject, trElement);
    var i;
    for (i = 0; i < tCols; i++) {
        var nc = trElement.insertCell();
        nc.insertAdjacentText("afterBegin", " ");
    }
    ClearMenu();
}
//
//----------
//
function findColumn() {
    console.log("base.findColumn");
    var i;
    for (i = 0; i < TableRowPropertyObject.childNodes.length; i++) {
        if (TableRowPropertyObject.childNodes[i] == TableCellPropertyObject) {
            return i;
        }
    }
}
//
//----------
//
function insertColLeft() {
    console.log("base.insertColLeft");
    var colIndex = findColumn();
    var tRows = TableBodyPropertyObject.getElementsByTagName("TR");
    for (i = 0; i < tRows.length; i++) {
        if (tRows[i].childNodes[colIndex]) {
            if (tRows[i].childNodes[colIndex].tagName == "TD") {
                var newTD = tRows[i].insertCell(colIndex);
                newTD.insertAdjacentText("afterBegin", " ");
                newTD.style.width = Math.round(50 / tRows[i].childNodes.length) + "%";
            }
        }
    }
    ClearMenu();
}
//
//----------
//
function deleteColumn() {
    console.log("base.deleteColumn");
    var colIndex = findColumn();
    var tRows = TableBodyPropertyObject.getElementsByTagName("TR");
    for (i = 0; i < tRows.length; i++) {
        if (tRows[i].childNodes[colIndex]) {
            if (tRows[i].childNodes[colIndex].tagName == "TD") {
                tRows[i].childNodes[colIndex].removeNode(true);
            }
        }
    }
    ClearMenu();
}
//
//----------
//
function insertColRight() {
    console.log("base.insertColRight");
    var colIndex = findColumn();
    var tRows = TableBodyPropertyObject.getElementsByTagName("TR");
    for (i = 0; i < tRows.length; i++) {
        if (tRows[i].childNodes[colIndex]) {
            if (tRows[i].childNodes[colIndex].tagName == "TD") {
                var newTD = tRows[i].insertCell(colIndex + 1);
                newTD.insertAdjacentText("afterBegin", " ");
                newTD.style.width = Math.round(50 / tRows[i].childNodes.length) + "%";
            }
        }
    }
    ClearMenu();
}
//
//----------
//
function deleteRow() {
    console.log("base.deleteRow");
    TableRowPropertyObject.removeNode(true);
    ClearMenu();
}
//
//----------
//
function showPanel(panelNum) {
    console.log("base.showPanel");
    if (currentPanel != null) {
        hidePanel();
    }
    document.getElementById('panel' + panelNum).style.visibility = 'visible';
    currentPanel = panelNum;
    setState(panelNum);
}
//
//----------
//
function hidePanel() {
    console.log("base.hidePanel");
    document.getElementById('panel' + currentPanel).style.visibility = 'hidden';
    document.getElementById('tab' + currentPanel).style.backgroundColor = '#ffffff';
    document.getElementById('tab' + currentPanel).style.color = 'navy';
}
//
//----------
//
function setState(tabNum) {
    console.log("base.setState");
    if (tabNum == currentPanel) {
        document.getElementById('tab' + tabNum).style.backgroundColor = '#ddddff';
        document.getElementById('tab' + tabNum).style.color = 'red';
    } else {
        document.getElementById('tab' + tabNum).style.backgroundColor = '#ffffff';
        document.getElementById('tab' + tabNum).style.color = 'navy';
    }
}
//
//----------
//
function hover(tab) {
    console.log("base.hover");
    tab.style.backgroundColor = 'ddddff';
}
//
//----------
//
function showTab(tabnum) {
    console.log("base.showTab");
    var tabsDef = new Array(4)
    for (i = 1; i <= tabsDef.length; i++) {
        tabName = 'tabContent' + i;
        eval(tabName + '.style.display="none"');
        tabName = 'tabLabel' + i;
        eval(tabName + '.style.backgroundColor="lightgrey"');
        eval(tabName + '.style.fontWeight="normal"');
        undName = 'tabUnderline' + i;
        eval(undName + '.style.backgroundColor="white"');
    }
    tabName = 'tabContent' + tabnum;
    eval(tabName + '.style.display="block"');
    tabName = 'tabLabel' + tabnum;
    eval(tabName + '.style.backgroundColor="lightgrey"');
    eval(tabName + '.style.fontWeight="bold"');
    undName = 'tabUnderline' + tabnum;
    eval(undName + '.style.backgroundColor="lightgrey"');
}
//
//----------
//
function ImagePreview(ImageURL, ImageHeight, ImageWidth) {
    console.log("base.ImagePreview");
    var Options
    Options = 'menubar=no,toolbar=no,location=no,status=no,scrollbars=no,resizable,directories=no'
    if (ImageWidth) {
        Options += ',width=' + ImageWidth;
    }
    if (ImageHeight) {
        Options += ',height=' + ImageHeight;
    }
    var MsgWin = window.open('', '_blank', Options)
    var MsgText = '<HTML><body bgcolor="white" topmargin="0" leftmargin="0" marginwidth="0" marginheight="0">' + ImageURL + '</BODY></HTML>'
    MsgWin.document.write(MsgText);
}
//
//----------
//
var LastBubbleID;
function HelpBubbleOn(BubbleID, Source) {
    console.log("base.HelpBubbleOn");
    var DivTarget, DivLeft, DivTop, pageYOffset;
    //
    HelpBubbleOff(LastBubbleID);
    DivTarget = document.getElementById(BubbleID);
    if (DivTarget) {
        if (navigator.appName == "Microsoft Internet Explorer") {
            pageYOffset = window.document.body.scrollTop;
        } else {
            pageYOffset = window.pageYOffset;
        }
        DivLeft = getPageOffsetLeft(Source) + Source.offsetWidth;
        if (DivLeft > document.body.scrollWidth) {
            DivLeft = document.body.scrollWidth - DivTarget.offsetWidth;
        }
        DivTop = getPageOffsetTop(Source) - DivTarget.offsetHeight + 15;
        if (DivTop < pageYOffset) {
            DivTop = pageYOffset;
        }
        // convert to inline
        //DivTarget.style.left = DivLeft + "px";
        //DivTarget.style.top = DivTop + "px";
        DivTarget.style.visibility = 'visible';
        DivTarget.style.display = 'block'; //move to inline, not end of body
        LastBubbleID = BubbleID;
    }
}
//
//----------
//
function HelpBubbleOff(BubbleID) {
    console.log("base.HelpBubbleOff");
    var DivTarget = document.getElementById(BubbleID);
    if (DivTarget) {
        DivTarget.style.visibility = 'hidden';
        DivTarget.style.display = 'none'; //move to inline, not end of body
    }
}
//
//----------
//
function HelpBubbleHover(BubbleID, Source) {
    console.log("base.HelpBubbleHover");
    var DivTarget;
    var DivLeft, DivTop;
    //
    DivTarget = document.getElementById(BubbleID);
    if (DivTarget) {
        DivTarget.style.visibility = 'visible';
    }
}
//
//----------
//
function HelpBubbleAjaxOn(BubbleID, Source, ContentName, FieldName) {
    console.log("base.HelpBubbleAjaxOn");
    var DivTarget;
    var DivLeft, DivTop;
    //
    HelpBubbleOff(LastBubbleID);
    DivTarget = document.getElementById(BubbleID);
    if (DivTarget) {
        DivLeft = getPageOffsetLeft(Source) + Source.offsetWidth;
        if (DivLeft > document.body.scrollWidth) {
            DivLeft = document.body.scrollWidth - DivTarget.offsetWidth;
        }
        DivTop = getPageOffsetTop(Source) - DivTarget.offsetHeight + 15;
        if (DivTop < 0) {
            DivTop = 0;
        }
        DivTarget.style.left = DivLeft + "px";
        DivTarget.style.top = DivTop + "px";
        DivTarget.style.visibility = 'visible';
        LastBubbleID = BubbleID;
        if (DivTarget.innerHTML = 'Loading...') {
            //			GetRemoteAjax( 'http://support.contensive.com/index.asp','GetFieldHelp','name='+ContentName+'.'+FieldName,'',BubbleID)
        }
    }
}
//
//------------------------------------------------------------------------------------------------------
//	Update the help message
//
function updateFieldHelp(fieldId, helpEditorId, helpClosedContentId) {
    var eDst = document.getElementById(helpClosedContentId);
    var eSrc = document.getElementById(helpEditorId);
    eDst.innerHTML = eSrc.value;
    jQuery.ajax({
        url: "/setAdminSiteFieldHelp?fieldId=" + fieldId + "&helpCustom=" + eSrc.value,
    }).done(function (data) {
        //
    });
}
//
//----------
//
function InsertFolderRow() {
    console.log("base.InsertFolderRow");
    var RLTable = document.getElementById('AddFolderTable');
    var CountElement = document.getElementById('AddFolderCount');
    var tCols, tRows, NewRowNumber;
    var NewRow, NewCell1, NewCell2, NewCell3, NewCell4;
    var RowCnt = RLTable.rows.length;
    if (RLTable) {
        NewRowNumber = parseInt(CountElement.value) + 1;
        CountElement.value = NewRowNumber;
        tRows = RLTable.getElementsByTagName("TR");
        NewRow = RLTable.insertRow(RowCnt);
        NewCell1 = NewRow.insertCell(0);
        NewCell1.align = 'right';
        NewCell1.innerHTML = NewRowNumber + '&nbsp;';
        NewCell3 = NewRow.insertCell(1);
        NewCell3.innerHTML = '<INPUT TYPE="Text" NAME="FolderName.' + NewRowNumber + '" SIZE="30">';
        NewCell4 = NewRow.insertCell(2);
        NewCell4.innerHTML = '<INPUT TYPE="Text" NAME="FolderDescription.' + NewRowNumber + '" SIZE="40">';
    }
}
//
//----------
//
function InsertUploadRow() {
    console.log("base.InsertUploadRow");
    var RLTable = document.getElementById('UploadInsert');
    var CountElement = document.getElementById('LibraryUploadCount');
    var tCols, tRows, NewRowNumber;
    var RowCnt = RLTable.rows.length;
    var NewRow, NewCell1, NewCell2, NewCell3, NewCell4;
    if (RLTable) {
        NewRowNumber = parseInt(CountElement.value) + 1;
        CountElement.value = NewRowNumber;
        tRows = RLTable.getElementsByTagName("TR");
        NewRow = RLTable.insertRow(RowCnt);
        NewCell1 = NewRow.insertCell(0);
        NewCell1.align = 'right';
        NewCell1.innerHTML = NewRowNumber + '&nbsp;';
        NewCell2 = NewRow.insertCell(1);
        NewCell2.innerHTML = '<INPUT TYPE="file" name="LibraryUpload.' + NewRowNumber + '">';
        NewCell3 = NewRow.insertCell(2);
        NewCell3.innerHTML = '<INPUT TYPE="Text" NAME="LibraryName.' + NewRowNumber + '" SIZE="30">';
        NewCell4 = NewRow.insertCell(3);
        NewCell4.innerHTML = '<INPUT TYPE="Text" NAME="LibraryDescription.' + NewRowNumber + '" SIZE="40">';
    }
}
//
//----------
//
function InsertUpload() {
    console.log("base.InsertUpload");
    var RLTable = document.getElementById('UploadInsert');
    var CountElement = document.getElementById('UploadCount');
    var tCols, tRows, NewRowNumber;
    var RowCnt = RLTable.rows.length;
    var NewRow, NewCell1, NewCell2, NewCell3, NewCell4;
    if (RLTable) {
        NewRowNumber = parseInt(CountElement.value);
        CountElement.value = NewRowNumber + 1;
        tRows = RLTable.getElementsByTagName("TR");
        NewRow = RLTable.insertRow(RowCnt);
        NewCell2 = NewRow.insertCell(0);
        NewCell2.innerHTML = '<INPUT TYPE="file" name="Upload' + NewRowNumber + '">';
    }
}
//
//----------
//
function RLRowClick(IsChecked, RowID) {
    console.log("base.RLRowClick");
    var RLRow = document.getElementById(RowID);
    if (IsChecked) {
        RLRow.style.backgroundColor = "#F0F0F0";
    } else {
        RLRow.style.backgroundColor = "";
    }
}
//
//----------
//
var DPanel = '0';
function ToggleToolPanelDev() {
    console.log("base.ToggleToolPanelDev");
    if (DPanel == '0') {
        DPanel = '1';
        document.all['ToolPanelDev'].style.visibility = 'visible';
    } else {
        DPanel = '0';
        document.all['ToolPanelDev'].style.visibility = 'hidden';
    }
    return false;
}
//
//----------
//
function SubmitToolsPanel() {
    console.log("base.SubmitToolsPanel");
    var SelectedObject;
    var TextRange;
    SelectedObject = document.selection;
    TextRange = SelectedObject.createRange();
    if (TextRange.text) {
        if (TextRange.text.length != 0) {
            document.all['ToolsPanelSelection'].value = TextRange.htmlText;
        }
    }
    return true;
}
//
//----------
//
function InsertTemplateIcon(ImageLink, ObjectName) {
    console.log("base.InsertTemplateIcon");
    ExecCmdPlus('InsertImage', false, ImageLink, ObjectName);
    ClearMenu();
}
//
//----------
//
function switchContentFolderDiv(ShowID, HideID, ContentCaptionDivID, ContentCaption, EmptyDivID) {
    console.log("base.switchContentFolderDiv");
    var ShowDiv, HideDiv, CaptionDiv, EmptyDiv, ptr;
    var HiddenInput, Hit;
    //
    if (ShowID == HideID) { return false; }
    //
    // hide old content
    //
    Hit = false;
    HideDiv = document.getElementById(HideID);
    if (HideDiv) {
        if (HideDiv.id == HideID) {
            HideDiv.style.display = "none";
            Hit = true;
        }
    }
    if (!Hit) {
        HiddenInput = document.getElementsByName(HideID);
        if (HiddenInput.length > 0) {
            for (ptr = 0; ptr < HiddenInput.length; ptr++) {
                HiddenInput[ptr].parentNode.style.display = "none";
            }
        }
    }
    if (!Hit) {
        EmptyDiv = document.getElementById(EmptyDivID)
        if (EmptyDiv) {
            EmptyDiv.style.display = "none";
        }
    }
    //
    // show new content
    //
    Hit = false;
    ShowDiv = document.getElementById(ShowID);
    if (ShowDiv) {
        if (ShowDiv.id == ShowID) {
            //ShowDiv.style.visibility = "hidden";
            ShowDiv.style.display = "block";
            //ShowDiv.style.visibility = "visible";
            Hit = true;
        }
    }
    if (!Hit) {
        HiddenInput = document.getElementsByName(ShowID);
        if (HiddenInput.length > 0) {
            Hit = true
            for (ptr = 0; ptr < HiddenInput.length; ptr++) {
                //HiddenInput[ptr].parentNode.style.visibility = "hidden";
                HiddenInput[ptr].parentNode.style.display = "block";
                //HiddenInput[ptr].parentNode.style.visibility = "visible";
            }
        }
    }
    if (!Hit) {
        EmptyDiv = document.getElementById(EmptyDivID)
        if (EmptyDiv) {
            EmptyDiv.style.display = "block";
        }
    }

    //
    // convert new content to block
    //
    //CaptionDiv=document.getElementById(ContentCaptionDivID);
    //if (CaptionDiv) {
    //	CaptionDiv.innerHTML=ContentCaption;
    //}

}
//
//----------
//
function SetDisplay(Id, Value) {
    console.log("base.SetDisplay");
    document.getElementById(Id).style.display = Value;
}
//
// Close TextAreaExpandable
//
function CloseTextArea(TextAreaID) {
    console.log("base.CloseTextArea");
    var tc = document.getElementById(TextAreaID);
    var to = document.getElementById(TextAreaID + 'Opened');
    var hc = document.getElementById(TextAreaID + 'Head')
    var ho = document.getElementById(TextAreaID + 'HeadOpened')
    var HeadHeight = 24;
    scroll(0, 0);
    tc.value = to.value;
    ho.style.display = 'none';
    to.style.display = 'none';
    hc.style.display = 'block';
    tc.style.display = 'block';
    document.body.style.overflow = "";
    tc.focus();
}

//
// Open TextAreaExpandable
//
function OpenTextArea(TextAreaID) {
    console.log("base.OpenTextArea");
    window.scroll(0, 0);
    var tc = document.getElementById(TextAreaID);
    var to = document.getElementById(TextAreaID + 'Opened');
    var hc = document.getElementById(TextAreaID + 'Head');
    var ho = document.getElementById(TextAreaID + 'HeadOpened');
    var HeadHeight = hc.offsetHeight;
    var h;
    to.value = tc.value;
    document.body.style.overflow = "hidden";
    if (document.compatMode && document.compatMode != "BackCompat") {
        //using doctype
        document.body.style.height = "100%";
        h = window.size().height;
        w = window.size().width;
        document.body.style.height = "";
    } else {
        if (document.body.style.overflow == "hidden") {
            w = document.body.offsetWidth;
        } else {
            w = document.body.offsetWidth - 22;
        }
        h = document.body.offsetHeight - 4;
    }
    if (!browser.isIE) {
        //alert('moz');
        //h=window.innerHeight-HeadHeight;
        ho.style.width = "100%";
        to.style.width = "100%";
        to.style.top = HeadHeight + 'px';
        to.style.height = h + 'px';
        to.style.overflow = "auto";
        to.style.zIndex = "9999";
    } else {
        //alert('ie 8');
        ho.style.width = w + 'px';
        to.style.width = w + 'px';
        to.style.top = HeadHeight + 'px';
        to.style.height = (h - HeadHeight) + "px";
        to.style.overflow = "auto";
        to.style.zIndex = "9999";
    }
    ho.style.display = 'block';
    to.style.display = 'block';
    to.focus()
}
//
// Disable all buttons on the page
//
function processSubmit(e) {
    console.log("base.processSubmit");
    if (typeof (docLoaded) != "undefined") {
        if (!docLoaded) {
            alert('This page has not loaded completed. Please wait for the page to load before submitting the form. If the page has loaded, there may have been an error. Please refresh the page.');
            return false;
        }
    }
    var x = document.getElementsByTagName("input");
    for (var i = 0; i < x.length; i++) {
        if ((x[i].type == 'submit') && (x[i] != e)) {
            x[i].disabled = true;
        }
    }
}
//
//
//
function EncodeAddonOptionArgument(EncodedArg) {
    console.log("base.EncodeAddonOptionArgument");
    var a = EncodedArg;
    a = a.replace(/&/g, "#0038#");
    a = a.replace(/=/g, "#0061#");
    a = a.replace(/,/g, "#0044#");
    a = a.replace(/\\/g, "#0034#");
    a = a.replace(/'/g, "#0039#");
    a = a.replace(/\|/g, "#0124#");
    a = a.replace(/\[/g, "#0091#");
    a = a.replace(/]/g, "#0093#");
    a = a.replace(/:/g, "#0058#");
    return a;
}
//
//
//
function DecodeAddonOptionArgument(EncodedArg) {
    console.log("base.DecodeAddonOptionArgument");
    var a = EncodedArg;
    a = a.replace(/#0058#/g, ":");
    a = a.replace(/#0093#/g, "]");
    a = a.replace(/#0091#/g, "[");
    a = a.replace(/#0124#/g, "|");
    a = a.replace(/#0039#/g, "'");
    a = a.replace(/#0034#/g, "\"");
    a = a.replace(/#0044#/g, ",");
    a = a.replace(/#0061#/g, "=");
    a = a.replace(/#0038#/g, "&");
    return a;
}
//
//-------------------------------------------------------------------------
// Add an onLoad event
// source webreference.com, SimonWilson.net
//-------------------------------------------------------------------------
//
function cjAddLoadEvent(func) {
    console.log("base.cjAddLoadEvent");
    var oldonload = window.onload;
    if (typeof window.onload != 'function') {
        window.onload = func;
    } else {
        window.onload = function () {
            if (oldonload) {
                oldonload();
            }
            func();
        }
    }
}
//
//-------------------------------------------------------------------------
// Add head script code
// adding to the head after load has no effect. Instead, add it to the document
// with eval.
//-------------------------------------------------------------------------
//
function cjAddHeadScriptCode(codeAsString) {
    console.log("base.cjAddHeadScriptCode");
    eval(codeAsString)
}
//
//-------------------------------------------------------------------------
// Add head script link
//-------------------------------------------------------------------------
//
function cjAddHeadScriptLink(link) {
    console.log("base.cjAddHeadScriptLink");
    var st = document.createElement('script');
    st.type = 'text/javascript';
    st.src = link;
    var ht = document.getElementsByTagName("head")[0];
    ht.appendChild(st);
}
//
//-------------------------------------------------------------------------
// Add head style link tag
//-------------------------------------------------------------------------
//
function cjAddHeadStyleLink(linkHref) {
    console.log("base.cjAddHeadStyleLink");
    var st = document.createElement('link');
    st.rel = 'stylesheet';
    st.type = 'text/css';
    st.href = Source;
    var ht = document.getElementsByTagName("head")[0];
    ht.appendChild(st);
}
//
//-------------------------------------------------------------------------
// Add head style tag
//-------------------------------------------------------------------------
//
function cjAddHeadStyle(styles) {
    console.log("base.cjAddHeadStyle");
    var st = document.createElement('style');
    st.type = 'text/css';
    if (st.styleSheet) {
        st.styleSheet.cssText = styles;
    } else {
        st.appendChild(document.createTextNode(styles));
    }
    //st.innerHTML=styles;
    var ht = document.getElementsByTagName("head")[0];
    ht.appendChild(st);
}
//
//-------------------------------------------------------------------------
// Set iFrame height to size of div container
//	width is 100% of its container
//	height is the height of the content
//
//  <iframe src="whatever.html" id="ID" onLoad="setFrameHeight('the_iframe');" style="width:100%;height:200px"></iframe>
//-------------------------------------------------------------------------
//

function cjSetFrameHeight(frameId) {
    console.log("base.cjSetFrameHeight");
    var e, h, f;
    e = document.getElementById(frameId);
    if (e) {
        h = e.contentWindow.document.body.offsetHeight;
        //e.style.height=h+'px';
        e.style.height = (h + 16) + 'px';
    }
}
//
//-------------------------------------------------------------------------
// convert textarea tab key to a tab character
//-------------------------------------------------------------------------
//

function cjEncodeTextAreaKey(sender, e) {
    console.log("base.cjEncodeTextAreaKey");
    if (e.keyCode == 9) {
        if (e.srcElement) {
            sender.selection = document.selection.createRange();
            sender.selection.text = "\t";
        } else if (e.target) {
            var start = sender.value.substring(0, sender.selectionStart);
            var end = sender.value.substring(sender.selectionEnd, sender.value.length);
            var newRange = sender.selectionEnd + 1;
            var scrollPos = sender.scrollTop;
            sender.value = String.concat(start, "\t", end);
            sender.setSelectionRange(newRange, newRange);
            sender.scrollTop = scrollPos;
        }
        return false;
    } else {
        return true;
    }
}
//
//----------
// Do NOT call directly, call as cj.ajax.url()
//
// should be cjAjaxURL()
//
// Perform an Ajax call to a URL, and push the results into the object with ID=DestinationID
// LocalURL is a URL on this site (/index.asp or index.asp?a=1 for instance)
// FormID is the ID of a form on this page that should be submitted with the call
// DestinationID is the ID of the DIV tag where the returned content will go
// onEmptyHideID is the ID of a DIV to hide if the ajax call returns nothing
// onEmptyShowID is the ID of a DIV to show if the ajax call returns nothing
//----------
//
function cjAjaxURL(LocalURL, FormID, DestinationID, onEmptyHideID, onEmptyShowID) {
    console.log("base.cjAjaxURL");
    var xmlHttp, fo, i, url, serverResponse, el1, pos, ret, e;
    var eSelected = new Array();
    try {
        // Firefox, Opera 8.0+, Safari
        xmlHttp = new XMLHttpRequest();
    } catch (e) {
        // Internet Explorer
        try {
            xmlHttp = new ActiveXObject("Msxml2.XMLHTTP");
        } catch (e) {
            try {
                xmlHttp = new ActiveXObject("Microsoft.XMLHTTP");
            } catch (e) {
                alert("Your browser does not support this function");
                return false;
            }
        }
    }
    xmlHttp.onreadystatechange = function () {
        if (xmlHttp.readyState == 4 && xmlHttp.status == 200) {
            serverResponse = xmlHttp.responseText;
            if ((serverResponse != '') && (DestinationID != '')) {
                if (document.getElementById) {
                    var el1 = document.getElementById(DestinationID);
                } else if (document.all) {
                    var el1 = document.All[DestinationID];
                } else if (document.layers) {
                    var el1 = document.layers[DestinationID];
                }
                if (el1) {
                    el1.innerHTML = serverResponse
                    // run embedded scripts
                    var oldOnLoad = window.onload;
                    window.onload = '';
                    var scripts = el1.getElementsByTagName('script');
                    for (var i = 0; i < scripts.length; i++) {
                        if (scripts[i].src) {
                            var s = document.createElement("script");
                            s.src = scripts[i].src;
                            s.type = "text/javascript";
                            document.getElementsByTagName("head")[0].appendChild(s);
                        } else {
                            eval(scripts[i].innerHTML);
                        }
                    }
                    if (window.onload) {
                        window.onload();
                    }
                }
            }
            if (serverResponse == '') {
                if (document.getElementById(onEmptyHideID)) { document.getElementById(onEmptyHideID).style.display = 'none' }
                //if (onEmptyHideID!='') {document.getElementById(onEmptyHideID).style.display='none'}
                if (document.getElementById(onEmptyShowID)) { document.getElementById(onEmptyShowID).style.display = 'block' }
                //if (onEmptyShowID!='') {document.getElementById(onEmptyShowID).style.display='block'}
            }
        }
    }
    // get host from document url
    url = document.URL;
    pos = url.indexOf("#");
    if (pos != -1) { url = url.substr(0, pos) }
    pos = url.indexOf("?");
    if (pos != -1) { url = url.substr(0, pos) }
    pos = url.indexOf("://");
    if (pos != -1) {
        pos = url.indexOf("/", pos + 3);
        if (pos != -1) { url = url.substr(0, pos) }
    }
    url += LocalURL;
    pos = url.indexOf("?")
    if (pos == -1) { url += '?' } else { url += '&' }
    url += 'nocache=' + Math.random();
    fo = document.getElementById(FormID);
    if (fo) {
        if (fo.elements) {
            for (i = 0; i < fo.elements.length; i++) {
                e = fo.elements[i];
                ret = '';
                if (e.type == 'select-multiple') {
                    while (e.selectedIndex != -1) {
                        eSelected.push(e.selectedIndex);
                        if (ret) ret += ',';
                        ret += e.options[e.selectedIndex].value;
                        e.options[e.selectedIndex].selected = false;
                    }
                    while (eSelected.length > 0) {
                        e.options[eSelected.pop()].selected = true;
                    }
                    url += '&' + escape(e.name) + '=' + escape(ret)
                } else if (e.type == 'radio') {
                    if (e.checked) {
                        ret = e.value;
                        url += '&' + escape(e.name) + '=' + escape(ret)
                    }
                } else if (e.type == 'checkbox') {
                    if (e.checked) {
                        ret = e.value;
                        url += '&' + escape(e.name) + '=' + escape(ret)
                    }
                } else {
                    ret = e.value
                    url += '&' + escape(e.name) + '=' + escape(ret)
                }
            }
        }
    }
    //alert('cjAjaxURL, done, url='+url);	
    xmlHttp.open("GET", url, true);
    xmlHttp.send(null);
}
//
//----------
// deprecated
//----------
//
function GetURLAjax(LocalURL, FormID, DestinationID, onEmptyHideID, onEmptyShowID) {
    console.log("base.GetURLAjax");
    cjAjaxURL(LocalURL, FormID, DestinationID, onEmptyHideID, onEmptyShowID);
}
//
//----------
// Do NOT call directly, call as cj.ajax.addon()
//
// should be cjAjaxAddon() -- will be renamed
//
// Perform an Ajax call, and push the results into the object with ID=DestinationID
// AddonName is the name of the Ajax Addon to call serverside
// QueryString is to be added to the current URL -- less its querystring
// FormID is the ID of a form on this page that should be submitted with the call
// DestinationID is the ID of the DIV tag where the returned content will go
// onEmptyHideID is the ID of a DIV to hide if the ajax call returns nothing
// onEmptyShowID is the ID of a DIV to show if the ajax call returns nothing
//----------
//
function cjAjaxAddon(AddonName, QueryString, FormID, DestinationID, onEmptyHideID, onEmptyShowID) {
    console.log("base.cjAjaxAddon");
    //
    // compatibility mode
    //
    var url, pos;
    url = document.URL;
    pos = url.indexOf("#");
    if (pos != -1) { url = url.substr(0, pos) }
    pos = url.indexOf("?");
    if (pos != -1) { url = url.substr(0, pos) }
    pos = url.indexOf("://");
    if (pos != -1) {
        pos = url.indexOf("/", pos + 3);
        if (pos != -1) { url = url.substr(pos) }
    }
    url += '?nocache=0';
    if (AddonName != '') { url += '&remotemethodaddon=' + AddonName }
    if (QueryString != "") { url += '&' + QueryString }
    cjAjaxURL(url, FormID, DestinationID, onEmptyHideID, onEmptyShowID);
}
//
//----------
// deprecated
//----------
//
function GetAjax(AddonName, QueryString, FormID, DestinationID, onEmptyHideID, onEmptyShowID) {
    console.log("base.GetAjax");
    cjAjaxAddon(AddonName, QueryString, FormID, DestinationID, onEmptyHideID, onEmptyShowID);
}
//
//----------
// Do NOT call directly, call as cj.ajax.addonCallback()
//
// cjAjaxAddonCallback()
//
// Perform an Ajax call, and call a callback with the results
// AddonName is the name of the Ajax Addon to call serverside
// QueryString is to be added to the current URL -- less its querystring
// Callback is a javascript function to be called when the data returns. The
//    function must accept two arguments, the ajax return and an argument CallbackArg
//    that can be passed in to the ajax call. Use this second argument to pass
//    status etc to the callback (like what id the response goes in)
//----------
//
function cjAjaxAddonCallback(AddonName, QueryString, Callback, CallbackArg) {
    console.log("base.cjAjaxAddonCallback");
    var xmlHttp, url, pos;
    try {
        // Firefox, Opera 8.0+, Safari
        xmlHttp = new XMLHttpRequest();
    } catch (e) {
        // Internet Explorer
        try {
            xmlHttp = new ActiveXObject("Msxml2.XMLHTTP");
        } catch (e) {
            try {
                xmlHttp = new ActiveXObject("Microsoft.XMLHTTP");
            } catch (e) {
                alert("Your browser does not support this function");
                return false;
            }
        }
    }
    xmlHttp.onreadystatechange = function () {
        var serverResponse, sLen, i, scripts, sLen, srcArray, codeArray;
        if (xmlHttp.readyState == 4 && xmlHttp.status == 200) {
            serverResponse = xmlHttp.responseText;
            var isSrcArray = new Array();
            var codeArray = new Array();
            var oldOnLoad = window.onload;
            if (serverResponse != '') {
                // remove embedded scripts
                var e = document.createElement('div');
                if (e) {
                    // create array of scripts to add
                    // this is a workaround for an issue with ie
                    // where the scripts collection is 0ed after the first eval
                    window.onload = '';
                    e.innerHTML = serverResponse;
                    scripts = e.getElementsByTagName('script');
                    sLen = scripts.length;
                    if (sLen > 0) {
                        for (i = 0; i < sLen; i++) {
                            if (scripts[i].src) {
                                isSrcArray.push(true);
                                codeArray.push(scripts[i].src);
                                scripts[i].parentNode.removeChild(scripts[i]);
                                //scripts[i].src='';
                            } else {
                                isSrcArray.push(false);
                                codeArray.push(scripts[i].innerHTML);
                                scripts[i].parentNode.removeChild(scripts[i]);
                                //scripts[i].innerHTML='';
                            }
                        }
                        serverResponse = e.innerHTML;
                    }
                }
            }
            // execute the callback which may save the response
            if (Callback) {
                Callback(serverResponse, CallbackArg);
            }
            // execute any scripts found if response
            for (i = 0; i < codeArray.length; i++) {
                if (isSrcArray[i]) {
                    var s = document.createElement("script");
                    s.src = codeArray[i];
                    s.type = "text/javascript";
                    document.getElementsByTagName("head")[0].appendChild(s);
                } else {
                    eval(codeArray[i]);
                }

            }
            if (window.onload) {
                window.onload();
            }

        }
    }
    // create LocalURL
    url = document.URL;
    pos = url.indexOf("#");
    if (pos != -1) { url = url.substr(0, pos) }
    pos = url.indexOf("?");
    if (pos != -1) { url = url.substr(0, pos) }
    pos = url.indexOf("://");
    if (pos != -1) {
        pos = url.indexOf("/", pos + 3);
        if (pos != -1) { url = url.substr(pos) }
    }
    url += '?nocache=' + Math.random()
    if (AddonName != '') { url += '&remotemethodaddon=' + AddonName; }
    if (QueryString != "") { url += '&' + QueryString; }
    xmlHttp.open("GET", url, true);
    xmlHttp.send(null);
}
//
//----------
// Do NOT call directly, call as cj.ajax.qs()
//
// Perform an Ajax call, and push the results into the object with ID=DestinationID
// QueryString is to be added to the current URL -- less its querystring
// FormID is the ID of a form on this page that should be submitted with the call
// DestinationID is the ID of the DIV tag where the returned content will go
// onEmptyHideID is the ID of a DIV to hide if the ajax call returns nothing
// onEmptyShowID is the ID of a DIV to show if the ajax call returns nothing
//----------
//
function cjAjaxQs(QueryString, FormID, DestinationID, onEmptyHideID, onEmptyShowID) {
    console.log("base.cjAjaxQs");
    var url;
    var pos;
    // create LocalURL
    url = document.URL;
    pos = url.indexOf("#");
    if (pos != -1) { url = url.substr(0, pos) }
    pos = url.indexOf("?");
    if (pos != -1) { url = url.substr(0, pos) }
    pos = url.indexOf("://");
    if (pos != -1) {
        pos = url.indexOf("/", pos + 3);
        if (pos != -1) { url = url.substr(pos) }
    }
    url += '?nocache=' + Math.random()
    if (QueryString != "") { url += '&' + QueryString; }
    cjAjaxURL(url, FormID, DestinationID, onEmptyHideID, onEmptyShowID);
}
//
//----------
// Do NOT call directly, call as cj.ajax.qsCallback()
//
// Perform an Ajax call, and on return, calls a callback routine
// QueryString is to be added to the current URL -- less its querystring
//----------
//
function cjAjaxQsCallback(QueryString, Callback, CallbackArg) {
    console.log("base.cjAjaxQsCallback");
    cjAjaxAddonCallback('', QueryString, Callback, CallbackArg);
}
//
//-------------------------------------------------------------------------
// cjAjaxData
//	handler - the event handler to be called when the ajax call returns. The
//		response from the ajax call is passed to the hander as it's argument
//	queryKey - a string that uniquely identifies the query record in the Remote Query content.
//		see ccLib.GetAjaxQueryKey
//	args - the name=value&name=value etc pairs.
//		Each name and each value must be indivually escaped before assembling the string
//		Depending on the QueryType setup for this key, the args can be used differently
//			SQLQuery - args are replacement name=value pairs
//			ContentInsert - args are fieldname=value pairs, but all fieldnames must be in the fieldnamelist
//			ContentUpdate - args are fieldname=value pairs, but all fieldnames must be in the fieldnamelist
//	pageSize - records to be returned
//	pageNumber - the page where the records begin
//	responseFormat
//		"table" - Google Visualization format
//			rows[0].id='fieldname'
//			rows[0].c[0].v='value'
//		"namearray" - returns arrays named for each field
//			dataadded[0]='value'
//			name[0]='value'
//		"namevalue" - returns only the first row of a table, with no arrays
//			dateadded='value'
//	ajaxMethod - the method used for this query
//		"data" - use the remote query table as the source for update details
//		"setvisitproperty" - run serverside SetVisitProperty
//		"getvisitproperty" - run serverside GetVisitProperty
//
//
//-------------------------------------------------------------------------
//
function cjAjaxData(handler, queryKey, args, pageSize, pageNumber, responseFormat, ajaxMethod) {
    console.log("base.cjAjaxData");
    var xmlHttp;
    var url;
    var serverResponse;
    var el1;
    var pos;
    var response;
    try {
        // Firefox, Opera 8.0+, Safari
        xmlHttp = new XMLHttpRequest();
    } catch (e) {
        // Internet Explorer
        try {
            xmlHttp = new ActiveXObject("Msxml2.XMLHTTP");
        } catch (e) {
            try {
                xmlHttp = new ActiveXObject("Microsoft.XMLHTTP");
            } catch (e) {
                alert("Your browser does not support this function");
                return false;
            }
        }
    }
    xmlHttp.onreadystatechange = function () {
        if (xmlHttp.readyState == 4 && xmlHttp.status == 200) {
            response = xmlHttp.responseText;
            var str = response.toLowerCase();
            pos = str.indexOf("</data>");
            if (pos != -1) { response = response.substr(0, pos) }
            pos = str.indexOf("<data>")
            if (pos != -1) { response = response.substr(pos + 6) }
            if (handler) { handler(response) }
        }
    }
    // get url w/o QS
    url = document.URL;
    pos = url.indexOf("#");
    if (pos != -1) { url = url.substr(0, pos) }
    pos = url.indexOf("://");
    if (pos != -1) {
        pos = url.indexOf("/", pos + 3);
        if (pos != -1) { url = url.substr(0, pos + 1) }
    }
    pos = url.indexOf("?")
    if (pos = -1) { url += '?' } else { url += '&' }
    url += 'ajaxfn=' + ajaxMethod;
    url += '&key=' + queryKey;
    if (pageSize) url += '&pagesize=' + escape(pageSize)
    if (pageNumber) url += '&pagenumber=' + escape(pageNumber)
    if (args) url += '&args=' + escape(args)
    url += '&nocache=' + Math.random();
    url += '&responseformat=' + responseFormat;
    xmlHttp.open("GET", url, true);
    xmlHttp.send(null);
}
//
//-------------------------------------------------------------------------
// cjAjaxGetTable
//	runs cj.ajax.data with a 'jsontable' responseFormat
//-------------------------------------------------------------------------
//
function cjAjaxGetTable(handler, queryKey, args, pageSize, pageNumber) {
    console.log("base.cjAjaxGetTable");
    return this.data(handler, queryKey, args, pageSize, pageNumber, 'jsontable', 'data')
}
//
//-------------------------------------------------------------------------
// cjAjaxGetNameArray
//	runs cj.ajax.data with a 'namearray' responseFormat
//-------------------------------------------------------------------------
//
function cjAjaxGetNameArray(handler, queryKey, args, pageSize, pageNumber) {
    console.log("base.cjAjaxGetNameArray");
    return this.data(handler, queryKey, args, pageSize, pageNumber, 'jsonnamearray', 'data')
}
//
//-------------------------------------------------------------------------
// cjAjaxGetNameValue
//	runs cj.ajax.data with 1 row and a 'namevalue' responseFormat
//-------------------------------------------------------------------------
//
function cjAjaxGetNameValue(handler, queryKey, args) {
    console.log("base.cjAjaxGetNameValue");
    return this.data(handler, queryKey, args, 1, 1, 'jsonnamevalue', 'data')
}
//
//-------------------------------------------------------------------------
// cjAjaxSetVisitProperty
//	sets a visit property for the current visit
//-------------------------------------------------------------------------
//
function cjAjaxSetVisitProperty(handler, propertyName, propertyValue) {
    console.log("base.cjAjaxSetVisitProperty");
    return this.data(handler, '', escape(propertyName) + '=' + escape(propertyValue), 0, 0, 'jsonnamevalue', 'setvisitproperty')
}
//
//-------------------------------------------------------------------------
// cjAjaxGetVisitProperty
//	gets a visit property for the current visit
//	it is returned in jsonnamevalue format, so you use it like jo.PropertyName=PropertyValue
//-------------------------------------------------------------------------
//
function cjAjaxGetVisitProperty(handler, propertyName, propertyValueDefault) {
    console.log("base.cjAjaxGetVisitProperty");
    return this.data(handler, '', escape(propertyName) + '=' + escape(propertyValueDefault), 0, 0, 'jsonnamevalue', 'getvisitproperty')
}
//
//-------------------------------------------------------------------------
// cjAjaxUpdate
//	Runs a Content Update query
//	handler - the routine called when the results return
//	queryKey - the key used to lookup the query (ccLib.getAjaxQueryKey) for this update
//	criteria - sql compatible criteria
//	setPairs - array of name=value pairs formatted as:
//		setPair=[['name0','value0'],['name1','value1'],...]
//		where names are valid field names in the content
//-------------------------------------------------------------------------
//
function cjAjaxUpdate(handler, queryKey, criteria, setPairs) {
    console.log("base.cjAjaxUpdate");
    var nameValue, argSet, args, x;
    argSet = '';
    for (x = 0; x < setPairs.length; x++) {
        argSet += '&' + escape(setPairs[x][0]) + '=' + escape(setPairs[x][1])
    }
    args = 'criteria=' + escape(criteria) + '&setpairs=' + escape(argSet);
    return this.data(handler, queryKey, args, 0, 0, '', 'data')
}
//
//-------------------------------------------------------------------------
// Hook an event listener
//
// Example
// cj.addListener(e,"click",function(){ alert('onclick hook');})
//-------------------------------------------------------------------------
//
function cjAddListener(element, event, listener, bubble) {
    console.log("base.cjAddListener");
    if (element.addEventListener) {
        if (typeof (bubble) == "undefined") bubble = false;
        element.addEventListener(event, listener, bubble);
    } else if (this.attachEvent) {
        element.attachEvent("on" + event, listener);
    }
}
//
//-------------------------------------------------------------------------
//
//-------------------------------------------------------------------------
//
function cjEncodeHTML(source) {
    console.log("base.cjEncodeHTML");
    if (source) {
        var r = source;
        var r = r.replace(/&/g, "&amp;");
        r = r.replace(/</g, "&lt;");
        r = r.replace(/>/g, "&gt;");
        r = r.replace(/"/g, "&quot;");
        r = r.replace(/\n/, "<br>");
        return r;
    }
}
//
//-------------------------------------------------------------------------
//
//-------------------------------------------------------------------------
//
function cjHide(id) {
    console.log("base.cjHide");
    var e = document.getElementById(id);
    if (e) e.style.display = 'none';
}
//
//-------------------------------------------------------------------------
//
//-------------------------------------------------------------------------
//
function cjShow(id) {
    console.log("base.cjShow");
    var e = document.getElementById(id);
    if (e) e.style.display = 'block';
}
//
//-------------------------------------------------------------------------
//
//-------------------------------------------------------------------------
//
function cjInvisible(id) {
    console.log("base.cjInvisible");
    var e = document.getElementById(id);
    if (e) e.style.visibility = 'hidden';
}
//
//-------------------------------------------------------------------------
//
//-------------------------------------------------------------------------
//
function cjVisible(id) {
    console.log("base.cjVisible");
    var e = document.getElementById(id);
    e.style.visibility = 'visible';
}
//
//-------------------------------------------------------------------------
//	adds a spinner to a target div - used when ajaxing in content
//-------------------------------------------------------------------------
//
function cjSetSpinner(destinationID, message, destinationHeight) {
    console.log("base.cjSetSpinner");
    //
    if (document.getElementById) {
        var el1 = document.getElementById(destinationID);
    } else if (document.all) {
        var el1 = document.All[destinationID];
    } else if (document.layers) {
        var el1 = document.layers[destinationID];
    }
    if (el1) {
        if (message) {
            el1.innerHTML = '<div style="text-align: center;"><img src="/ContensiveBase/images/ajax-loader-big.gif" /><p>' + message + '</p></div>';
        } else {
            el1.innerHTML = '<div style="text-align: center;"><img src="/ContensiveBase/images/ajax-loader-big.gif" /></div>';
        }
        if (destinationHeight) {
            el1.style.height = destinationHeight;
        }
    }
    //
}
//
//-------------------------------------------------------------------------
// cj.xml
// Load an xml string into an xml object and return it
// 	from multiple sources, 
//	including http://www.webreference.com/programming/javascript/definitive2/2.html
//-------------------------------------------------------------------------
//
function cjXMLLoadString(txt) {
    console.log("base.cjXMLLoadString");
    if (typeof DOMParser != "undefined") {
        // Mozilla, Firefox, and related browsers 
        return (new DOMParser()).parseFromString(text, "application/xml");
    } else if (typeof ActiveXObject != "undefined") {
        // Internet Explorer. 
        var doc = XML.newDocument();  // Create an empty document 
        doc.loadXML(text);            // Parse text into it 
        return doc;                   // Return it 
    } else {
        // As a last resort, try loading the document from a data: URL 
        // This is supposed to work in Safari. Thanks to Manos Batsis and 
        // his Sarissa library (sarissa.sourceforge.net) for this technique. 
        var url = "data:text/xml;charset=utf-8," + encodeURIComponent(text);
        var request = new XMLHttpRequest();
        request.open("GET", url, false);
        request.send(null);
        return request.responseXML;
    }
}
//
//-------------------------------------------------------------------------
// cj.admin
//	save all the field names that are empty into a targetField
//-------------------------------------------------------------------------
//
function cjAdminSaveEmptyFieldList(targetFieldId) {
    console.log("base.cjAdminSaveEmptyFieldList");
    var e = document.getElementById(targetFieldId);
    var c = document.getElementsByTagName('input');
    for (i = 0; i < c.length; i++) {
        if (c[i].type == 'checkbox') {
            if (c[i].checked == false) {
                e.value += c[i].name + ','
            }
        } else if (c[i].type == 'radio') {
            if (c[i].checked == false) {
                e.value += c[i].name + ','
            }
        } else if (c[i].value == '') {
            e.value += c[i].name + ','
        }
    }
    c = document.getElementsByTagName('select');
    for (i = 0; i < c.length; i++) {
        if (c[i].value == '') {
            e.value += c[i].name + ','
        }
    }
}
//
//-------------------------------------------------------------------------
// cj.admin object
//	routines for the admin site
//-------------------------------------------------------------------------
//
function cjAdminClass() {
    console.log("base.cjAdminClass");
    this.saveEmptyFieldList = cjAdminSaveEmptyFieldList;
}
//
//-------------------------------------------------------------------------
// cj.ajax object
//	perform remote calls back to the server (ajax)
//	cj.ajax.getTable()
//	cj.ajax.getRow()
//-------------------------------------------------------------------------
//
function cjAjaxClass() {
    console.log("base.cjAjaxClass");
    this.getTable = cjAjaxGetTable;
    this.getNameArray = cjAjaxGetNameArray;
    this.getNameValue = cjAjaxGetNameValue;
    this.update = cjAjaxUpdate;
    this.data = cjAjaxData;
    this.setVisitProperty = cjAjaxSetVisitProperty;
    this.getVisitProperty = cjAjaxGetVisitProperty;
    this.url = cjAjaxURL;
    this.addon = cjAjaxAddon;
    this.addonCallback = cjAjaxAddonCallback;
    this.qs = cjAjaxQs;
    this.qsCallback = cjAjaxQsCallback;
}
//
//-------------------------------------------------------------------------
// cj.xml object
//	perform common xml tasks
//		cj.xml.loadString()
//-------------------------------------------------------------------------
//
function cjXMLClass() {
    console.log("base.cjXMLClass");
    this.loadString = cjXMLLoadString;
}
//
//-------------------------------------------------------------------------
// Do NOT call directly
//
// should be cj.remote()
//
// cj.method(options) 
//		options:
//			'name': string - addonname
//			'formId': string - html Id of form to be submitted
//			'callback': function - function to call on completion, two arguments, response and callbackArg
//			'callbackArg': object - passed direction to the callback function
//			'destinationId': string - html Id of an element to set innerHtml
//			'onEmptyHideId': string - if return is empty, hide this element
//			'onEmptyShowId': string - if return is empty, show this element
//			'queryString': string - queryString formatted name=value pairs added to post
//			'url': string - link to hit if not addon name provided
//-------------------------------------------------------------------------
//
function cjRemote(options) {
    console.log("base.cjRemote");
    var url, fo, ptr, e, ret, pos;
    if (options.url) {
        url = options.url;
    }
    else {
        url = document.URL;
        pos = url.indexOf("#");
        if (pos != -1) { url = url.substr(0, pos) }
        pos = url.indexOf("?");
        if (pos != -1) { url = url.substr(0, pos) }
        pos = url.indexOf("://");
        if (pos != -1) {
            pos = url.indexOf("/", pos + 3);
            if (pos != -1) { url = url.substr(pos) }
        }
    }
    url += '?nocache=' + Math.random()
    if (options.method) { url += '&remotemethodaddon=' + options.method }
    if (options.queryString) { url += '&' + options.queryString }
    if (options.formId) {
        fo = document.getElementById(options.formId);
        if (fo) {
            if (fo.elements) {
                for (ptr = 0; ptr < fo.elements.length; ptr++) {
                    e = fo.elements[ptr];
                    ret = '';
                    if (e.type == 'select-multiple') {
                        while (e.selectedIndex != -1) {
                            eSelected.push(e.selectedIndex);
                            if (ret) ret += ',';
                            ret += e.options[e.selectedIndex].value;
                            e.options[e.selectedIndex].selected = false;
                        }
                        while (eSelected.length > 0) {
                            e.options[eSelected.pop()].selected = true;
                        }
                        url += '&' + escape(e.name) + '=' + escape(ret)
                    }
                    else if (e.type == 'radio') {
                        if (e.checked) {
                            ret = e.value;
                            url += '&' + escape(e.name) + '=' + escape(ret)
                        }
                    }
                    else if (e.type == 'checkbox') {
                        if (e.checked) {
                            ret = e.value;
                            url += '&' + escape(e.name) + '=' + escape(ret)
                        }
                    }
                    else {
                        ret = e.value
                        url += '&' + escape(e.name) + '=' + escape(ret)
                    }
                }
            }
        }
    }
    jQuery.get(url,
        function (serverResponse) {
            var e, start, end, test;
            var isSrcArray = new Array();
            var codeArray = new Array();
            var oldOnLoad = window.onload;
            if (serverResponse == '') {
                if (options.onEmptyHideId) {
                    e = document.getElementById(options.onEmptyHideId);
                    if (e) {
                        e.style.display = 'none'
                    }
                }
                if (options.onEmptyShowID) {
                    e = document.getElementById(options.onEmptyShowID);
                    if (e) {
                        e.style.display = 'block'
                    }
                }
            }
            else {
                // remove embedded scripts
                e = document.createElement('div');
                if (e) {
                    // create array of scripts to add
                    // this is a workaround for an issue with ie
                    // where the scripts collection is 0ed after the first eval
                    window.onload = '';
                    e.innerHTML = serverResponse;
                    if ((serverResponse != '') && (e.innerHTML == '')) {
                        //ie7-8,blocks scripts in response
                        test = serverResponse;
                        start = test.indexOf('<script');
                        while (start > -1) {
                            end = test.indexOf('/script>');
                            if (end > -1) {
                                serverResponse = serverResponse.substr(0, start) + serverResponse.substr(end + 8);
                            }
                            else {
                                serverResponse = serverResponse.substr(0, start);
                            }
                            test = serverResponse;
                            start = test.indexOf('<script');
                        }
                    }
                    else {
                        //e.innerText = serverResponse;
                        //jQuery(e).html(serverResponse);
                        scripts = e.getElementsByTagName('script');
                        sLen = scripts.length;
                        if (sLen > 0) {
                            for (i = 0; i < sLen; i++) {
                                if (scripts[i].src) {
                                    isSrcArray.push(true);
                                    codeArray.push(scripts[i].src);
                                    scripts[i].parentNode.removeChild(scripts[i]);
                                }
                                else {
                                    isSrcArray.push(false);
                                    codeArray.push(scripts[i].innerHTML);
                                    scripts[i].parentNode.removeChild(scripts[i]);
                                }
                            }
                            serverResponse = e.innerHTML;
                        }
                    }
                }
                // execute any scripts found
                for (i = 0; i < codeArray.length; i++) {
                    if (isSrcArray[i]) {
                        var s = document.createElement("script");
                        s.src = codeArray[i];
                        s.type = "text/javascript";
                        document.getElementsByTagName("head")[0].appendChild(s);
                    }
                    else {
                        eval(codeArray[i]);
                    }

                }
                // if destination set, store html response.
                if (options.destinationId) {
                    if (document.getElementById) {
                        var el1 = document.getElementById(options.destinationId);
                    }
                    else if (document.all) {
                        var el1 = document.All[options.destinationId];
                    }
                    else if (document.layers) {
                        var el1 = document.layers[options.destinationId];
                    }
                    if (el1) {
                        el1.innerHTML = serverResponse
                    }
                }
            }
            // execute the callback which may save the response
            if (options.callback) {
                options.callback(serverResponse, options.callbackArg);
            }
            if (window.onload) {
                window.onload();
            }
        },
        "text"
    );
}
//
//-------------------------------------------------------------------------
// Contensive Javascript object
//-------------------------------------------------------------------------
//
function ContensiveJavascriptClass() {
    console.log("base.ContensiveJavascriptClass");
    this.encodeHTML = cjEncodeHTML;
    this.invisible = cjInvisible;
    this.visible = cjVisible;
    this.hide = cjHide;
    this.show = cjShow;
    this.setFrameHeight = cjSetFrameHeight;
    this.encodeTextAreaKey = cjEncodeTextAreaKey;
    this.addHeadScriptLink = cjAddHeadScriptLink;
    this.addHeadStyle = cjAddHeadStyle;
    this.addHeadStyleLink = cjAddHeadStyleLink;
    this.addHeadScriptCode = cjAddHeadScriptCode;
    this.addLoadEvent = cjAddLoadEvent;
    this.addListener = cjAddListener;
    this.setSpinner = cjSetSpinner;
    //
    this.ajax = new cjAjaxClass();
    this.xml = new cjXMLClass();
    this.admin = new cjAdminClass();
    this.remote = cjRemote;
    //
    //-------------------------------------------------------------------------
    // Add head tag
    //-------------------------------------------------------------------------
    //
    this.addHeadTag = function (tag) {
        var i, oTag;
        var ht = document.getElementsByTagName("head")[0];
        try {
            ht.innerHTML += tag;
        }
        catch (e) {
            oTag = document.createElement('div');
            try {
                oTag.outerHTML = tag;
                ht.appendChild(oTag);
            }
            catch (e) {
                oTag.innerHTML = tag;
                for (i = 0; i < oTag.childNodes.length; i++) {
                    ht.appendChild(oTag.childNodes[i]);
                }
            }
        }
    }
}
//
//-------------------------------------------------------------------------
//	build cj object
//-------------------------------------------------------------------------
//
var cj = new ContensiveJavascriptClass()
//
//-------------------------------------------------------------------------
// Legacy
//-------------------------------------------------------------------------
//
function cjAddHeadTag(tag) {
    console.log("base.cjAddHeadTag");
    cj.addHeadTag(tag)
}
//
//-------------------------------------------------------------------------
//	setup pageclick to hide menuing
//-------------------------------------------------------------------------
//
//if (browser.isIE) {
//    document.onmousedown = PageClick;
//    if (document.body) {
//        document.body.onclick = BodyOnClick;
//        document.body.oncontextmenu = BodyOnClick;
//    }
//    else {
//        window.onclick = BodyOnClick;
//        window.oncontextmenu = BodyOnClick;
//    }
//    updateClipboardLinksInit();
//}
//else if (browser.version > 5) {
//    document.addEventListener("mousedown", PageClick, true);
//    document.addEventListener("mouseclick", BodyOnClick, true);
//}
//
// -- child list methods
function saveSortable(listId) {
    console.log("base.saveSortable");
    var e, c, s;
    s = listId;
    e = document.getElementById(listId);
    for (i = 0; i < e.childNodes.length; i++) {
        c = e.childNodes[i];
        if (c.id) { s += ',' + c.id }
    }
    cj.ajax.addon('savePageManagerChildListSort', 'sortlist=' + s)
}
jQuery(document).ready(function () {
    jQuery(".ccEditWrapper .ccChildList").sortable({
        stop: function (event, ui) {
            saveSortable(jQuery(this).attr('id'));
        }
    });
})
/*
* Admin List, submit FindButton if enter while find input field focus 
*/
jQuery('.findInput').keypress(function (e) {
    if (e.which == 13) {
        e.stopPropagation();
        e.preventDefault();
        jQuery('#FindButton').click();
    }
});
/*
 *  dynamicTab
 */

var numRows;
var numCols;
var initialized = 0;
var topAndBottomBarHeight = 50;
// moved to dynamic code. 
//var ActiveContentDivID = '';
//var ActiveTabTableID = '';
var ContentHeight = 0;

function TabInit() {
    var i;
    return false;
    if (initialized == 0) {
        initialized = 1;
        var AllContentDivs = document.getElementsByTagName("div");
        var ContentTop = 999;
        var styleTop = 0;
        var FirstTop = 0;
        //
        for (i = 0; i < AllContentDivs.length; i++) {
            if (AllContentDivs[i].className == 'ccAdminTabBody') {
                if (AllContentDivs[i].style.display == "block") {
                    FirstTop = getTop(AllContentDivs[i]);
                } else {
                }
                styleTop = getTop(AllContentDivs[i]);
                if (ContentTop > styleTop) {
                    ContentTop = styleTop;
                }
                if (AllContentDivs[i].offsetHeight > ContentHeight) {
                    ContentHeight = AllContentDivs[i].offsetHeight;
                }
            }
        }
    }
}


function switchLiveTab2(ContentDivID, ClickedTab, TabTableID, StylePrefix, TabWrapperID) {
    //alert('switchLiveTab2');
    var e, ePtr
    var i, TCnt, BCnt, RCnt;
    var newIndex;
    var oldIndex;
    var TabTable;
    var TBodyNode, TRNode, TDNode
    var NewContentDiv;
    var OldContentDiv;
    var TabWrapper;
    var StyleHeight;
    var width;
    //
    if (ActiveTabTableID == TabTableID) { return false; }
    NewContentDiv = document.getElementById(ContentDivID);
    OldContentDiv = document.getElementById(ActiveContentDivID);
    TabWrapper = document.getElementById(TabWrapperID);
    var classHide = NewContentDiv.className;
    var classShow = OldContentDiv.className;
    //
    // hide old content
    //
    OldContentDiv.className = classHide;
    NewContentDiv.className = classShow;
    //
    // turn off current active tab
    //
    TabTable = document.getElementById(ActiveTabTableID)
    for (TCnt = 0; TCnt < TabTable.childNodes.length; TCnt++) {
        TBodyNode = TabTable.childNodes[TCnt];
        if (TBodyNode.tagName == "TBODY") {
            for (BCnt = 0; BCnt < TBodyNode.childNodes.length; BCnt++) {
                TRNode = TBodyNode.childNodes[BCnt];
                for (RCnt = 0; RCnt < TRNode.childNodes.length; RCnt++) {
                    TDNode = TRNode.childNodes[RCnt];
                    if (TDNode.className == StylePrefix + 'Hit') {
                        TDNode.className = StylePrefix;
                    }
                }
            }
        }
    }
    ActiveTabTableID = TabTableID
    // turn on new active tab
    TabTable = document.getElementById(TabTableID)
    for (TCnt = 0; TCnt < TabTable.childNodes.length; TCnt++) {
        TBodyNode = TabTable.childNodes[TCnt];
        if (TBodyNode.tagName == "TBODY") {
            for (BCnt = 0; BCnt < TBodyNode.childNodes.length; BCnt++) {
                TRNode = TBodyNode.childNodes[BCnt];
                for (RCnt = 0; RCnt < TRNode.childNodes.length; RCnt++) {
                    TDNode = TRNode.childNodes[RCnt];
                    if (TDNode.className == StylePrefix) {
                        TDNode.className = StylePrefix + 'Hit';
                    }
                }
            }
        }
    }
    // toggle tab links
    var tabLinks = document.getElementsByName("tabLink");
    for (i = 0; i < tabLinks.length; i++) {
        if (tabLinks[i] != ClickedTab) {
            tabLinks[i].className = StylePrefix + 'Link';
            tabLinks[i].blur();
        }
        else {
            tabLinks[i].className = StylePrefix + 'HitLink';
            tabLinks[i].blur();
        }
    }
    ActiveContentDivID = ContentDivID;
    //	//
    //	// This is an IE fix - if a dynamic tree from ccnav.dll is on a div exposed by this click, the
    //	// the relative positioned icons are not positioned correctly
    //	//
    //	if ( window.WiggleTree ) { WiggleTree() }

}
//
function getTop(el) {
    var p = el.offsetParent;
    var y = el.offsetTop;
    while (p != null) {
        y += p.offsetTop;
        p = p.offsetParent;
    }
    return y;
}
function getPadding(x) {
    if (x.currentStyle) {
        var Top = x.currentStyle['paddingTop'];
        var Bottom = x.currentStyle['paddingBottom'];
    } else if (window.getComputedStyle) {
        var Top = document.defaultView.getComputedStyle(x, null).getPropertyValue('padding-top');
        var Bottom = document.defaultView.getComputedStyle(x, null).getPropertyValue('padding-bottom');
    }
    var TopNoPx = Top.split('p');
    var BottomNoPx = Top.split('p');
    y = parseInt(TopNoPx[0]) + parseInt(BottomNoPx[0])
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
/*
 * body on click
 */
function BodyOnClick() {
    console.log("base.BodyOnClick");
    hideselect(false);
}
/*
 * on ready bindings
 */
jQuery(document).ready(function () {
    jQuery('body').on('click',BodyOnClick);
    document.addEventListener("mousedown", PageClick, true);
    document.addEventListener("mouseclick", BodyOnClick, true);
})

