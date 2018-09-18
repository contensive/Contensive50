

using Models.Entity;

using Controllers;
using Contensive.BaseClasses;

namespace Controllers {
    
    // '' <summary>
    // '' Tools used to assemble html document elements. This is not a storage for assembling a document (see docController)
    // '' </summary>
    public class htmlController {
        
        // 
        private coreClass cpCore;
        
        // 
        // ====================================================================================================
        // '' <summary>
        // '' constructor
        // '' </summary>
        // '' <param name="cpCore"></param>
        // '' <remarks></remarks>
        public htmlController(coreClass cpCore) {
            this.cpCore = cpCore;
        }
        
        // 
        // ====================================================================================================
        // 
        public void addScriptCode_body(string code, string addedByMessage) {
            try {
                if (!string.IsNullOrEmpty(code)) {
                    cpCore.doc.htmlAssetList.Add(new htmlAssetClass(), With, {, ., assetType=htmlAssetTypeEnum.script, ., addedByMessage=addedByMessage, ., isLink=False, ., content=genericController.removeScriptTag(codeUnknown);
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
            }
            
        }
        
        // 
        // ====================================================================================================
        // 
        public void addScriptCode_head(string code, string addedByMessage) {
            try {
                if (!string.IsNullOrEmpty(code)) {
                    cpCore.doc.htmlAssetList.Add(new htmlAssetClass(), With, {, ., assetType=htmlAssetTypeEnum.script, ., inHead=True, ., addedByMessage=addedByMessage, ., isLink=False, ., content=genericController.removeScriptTag(codeUnknown);
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
            }
            
        }
        
        // 
        // =========================================================================================================
        // 
        public void addScriptLink_Head(string Filename, string addedByMessage) {
            try {
                if (!string.IsNullOrEmpty(Filename)) {
                    cpCore.doc.htmlAssetList.Add(new htmlAssetClass(), With, {, ., assetType=htmlAssetTypeEnum.script, ., addedByMessage=addedByMessage, ., isLink=True, ., inHead=True, ., content=Filename);
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
            }
            
        }
        
        // 
        // =========================================================================================================
        // 
        public void addScriptLink_Body(string Filename, string addedByMessage) {
            try {
                if (!string.IsNullOrEmpty(Filename)) {
                    cpCore.doc.htmlAssetList.Add(new htmlAssetClass(), With, {, ., assetType=htmlAssetTypeEnum.script, ., addedByMessage=addedByMessage, ., isLink=True, ., content=Filename);
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
            }
            
        }
        
        // 
        // =========================================================================================================
        // 
        public void addTitle(string pageTitle, string addedByMessage, void =, void ) {
            try {
                if (!string.IsNullOrEmpty(pageTitle.Trim())) {
                    cpCore.doc.htmlMetaContent_TitleList.Add(new htmlMetaClass(), With, {, ., addedByMessage=addedByMessage, ., content=pageTitle);
                    // Warning!!! Optional parameters not supported
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
            }
            
        }
        
        // 
        // =========================================================================================================
        // 
        public void addMetaDescription(string MetaDescription, string addedByMessage, void =, void ) {
            try {
                if (!string.IsNullOrEmpty(MetaDescription.Trim())) {
                    cpCore.doc.htmlMetaContent_Description.Add(new htmlMetaClass(), With, {, ., addedByMessage=addedByMessage, ., content=MetaDescription);
                    // Warning!!! Optional parameters not supported
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
            }
            
        }
        
        // 
        // =========================================================================================================
        // 
        public void addStyleLink(string StyleSheetLink, string addedByMessage) {
            try {
                if (!string.IsNullOrEmpty(StyleSheetLink.Trim())) {
                    cpCore.doc.htmlAssetList.Add(new htmlAssetClass(), With, {, ., addedByMessage=addedByMessage, ., assetType=htmlAssetTypeEnum.style, ., inHead=True, ., isLink=True, ., content=StyleSheetLink);
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
            }
            
        }
        
        // 
        // =========================================================================================================
        // 
        public void addStyleCode(string code, string addedByMessage, void =, void ) {
            try {
                if (!string.IsNullOrEmpty(code.Trim())) {
                    cpCore.doc.htmlAssetList.Add(new htmlAssetClass(), With, {, ., addedByMessage=addedByMessage, ., assetType=htmlAssetTypeEnum.style, ., inHead=True, ., isLink=False, ., content=code);
                    // Warning!!! Optional parameters not supported
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
            }
            
        }
        
        // 
        // =========================================================================================================
        // 
        public void addMetaKeywordList(string MetaKeywordList, string addedByMessage, void =, void ) {
            try {
                foreach (string keyword in ",".Split(c)) {
                    if (!string.IsNullOrEmpty(keyword)) {
                        cpCore.doc.htmlMetaContent_KeyWordList.Add(new htmlMetaClass(), With, {, ., addedByMessage=addedByMessage, ., content=keyword);
                        // Warning!!! Optional parameters not supported
                    }
                    
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
            }
            
        }
        
        // 
        // =========================================================================================================
        // 
        public void addHeadTag(string HeadTag, string addedByMessage, void =, void ) {
            try {
                cpCore.doc.htmlMetaContent_OtherTags.Add(new htmlMetaClass(), With, {, ., addedByMessage=addedByMessage, ., content=HeadTag);
                // Warning!!! Optional parameters not supported
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
            }
            
        }
        
        // 
        // ===================================================================================================
        // 
        public string getEditWrapper(string Caption, string Content) {
            string result = Content;
            try {
                if (cpCore.doc.authContext.isEditingAnything()) {
                    result = (html_GetLegacySiteStyles() + "<table border=0 width=\"100%\" cellspacing=0 cellpadding=0><tr><td class=\"ccEditWrapper\">");
                    if ((Caption != "")) {
                        ("" + ("<table border=0 width=\"100%\" cellspacing=0 cellpadding=0><tr><td class=\"ccEditWrapperCaption\">" 
                                    + (genericController.encodeText(Caption) + ("<!-- <img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=1 height=22 align=absmiddle> -->" + "</td></tr></table>"))));
                    }
                    
                    ("" + ("<table border=0 width=\"100%\" cellspacing=0 cellpadding=0><tr><td class=\"ccEditWrapperContent\" id=\"edi" +
                    "tWrapper" 
                                + (cpCore.doc.editWrapperCnt + ("\">" 
                                + (genericController.encodeText(Content) + ("</td></tr></table>" + "</td></tr></table>"))))));
                    cpCore.doc.editWrapperCnt = (cpCore.doc.editWrapperCnt + 1);
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
            }
            
            return result;
        }
        
        // 
        // ===================================================================================================
        //  To support the special case when the template calls this to encode itself, and the page content has already been rendered.
        // 
        private string convertActiveContent_internal(
                    string Source, 
                    int personalizationPeopleId, 
                    string ContextContentName, 
                    int ContextRecordID, 
                    int ContextContactPeopleID, 
                    bool PlainText, 
                    bool AddLinkEID, 
                    bool EncodeActiveFormatting, 
                    bool EncodeActiveImages, 
                    bool EncodeActiveEditIcons, 
                    bool EncodeActivePersonalization, 
                    string queryStringForLinkAppend, 
                    string ProtocolHostLink, 
                    bool IsEmailContent, 
                    int ignore_DefaultWrapperID, 
                    string ignore_TemplateCaseOnly_Content, 
                    CPUtilsBaseClass.addonContext Context, 
                    bool personalizationIsAuthenticated, 
                    object nothingObject, 
                    bool isEditingAnything) {
            string result = Source;
            try {
                // 
                const object StartFlag = "<!-- ADDON";
                const object EndFlag = " -->";
                bool DoAnotherPass;
                int ArgCnt;
                string AddonGuid;
                string ACInstanceID;
                string[] ArgSplit;
                string AddonName;
                string addonOptionString;
                int LineStart;
                int LineEnd;
                string Copy;
                string[] Wrapper;
                string[] SegmentSplit;
                string AcCmd;
                string SegmentSuffix;
                string[] AcCmdSplit;
                string ACType;
                string[] ContentSplit;
                int ContentSplitCnt;
                string Segment;
                int Ptr;
                string CopyName;
                string ListName;
                string SortField;
                bool SortReverse;
                string AdminURL;
                // 
                htmlToTextControllers converthtmlToText;
                // 
                int iPersonalizationPeopleId;
                iPersonalizationPeopleId = personalizationPeopleId;
                if ((iPersonalizationPeopleId == 0)) {
                    iPersonalizationPeopleId = cpCore.doc.authContext.user.id;
                }
                
                // 
                // hint = "csv_EncodeContent9 enter"
                if ((result != "")) {
                    AdminURL = ("/" + cpCore.serverConfig.appConfig.adminRoute);
                    // 
                    // --------
                    //  cut-paste from csv_EncodeContent8
                    // --------
                    // 
                    //  ----- Do EncodeCRLF Conversion
                    // 
                    // hint = hint & ",010"
                    if ((cpCore.siteProperties.getBoolean("ConvertContentCRLF2BR", false) 
                                && !PlainText)) {
                        result = genericController.vbReplace(result, "\r", "");
                        result = genericController.vbReplace(result, "\n", "<br>");
                    }
                    
                    // 
                    //  ----- Do upgrade conversions (upgrade legacy objects and upgrade old images)
                    // 
                    // hint = hint & ",020"
                    result = upgradeActiveContent(result);
                    // 
                    //  ----- Do Active Content Conversion
                    // 
                    // hint = hint & ",030"
                    if ((AddLinkEID 
                                || (EncodeActiveFormatting 
                                || (EncodeActiveImages || EncodeActiveEditIcons)))) {
                        result = convertActiveContent_Internal_activeParts(result, iPersonalizationPeopleId, ContextContentName, ContextRecordID, ContextContactPeopleID, AddLinkEID, EncodeActiveFormatting, EncodeActiveImages, EncodeActiveEditIcons, EncodeActivePersonalization, queryStringForLinkAppend, ProtocolHostLink, IsEmailContent, AdminURL, personalizationIsAuthenticated, Context);
                    }
                    
                    // 
                    //  ----- Do Plain Text Conversion
                    // 
                    // hint = hint & ",040"
                    if (PlainText) {
                        converthtmlToText = new htmlToTextControllers(cpCore);
                        result = converthtmlToText.convert(result);
                        converthtmlToText = null;
                    }
                    
                    // 
                    //  Process Active Content that must be run here to access webclass objects
                    //      parse as {{functionname?querystring}}
                    // 
                    // hint = hint & ",110"
                    if ((!EncodeActiveEditIcons 
                                && ((result.IndexOf("{{", 0) + 1) 
                                != 0))) {
                        ContentSplit = result.Split("{{");
                        result = "";
                        ContentSplitCnt = (UBound(ContentSplit) + 1);
                        Ptr = 0;
                        while ((Ptr < ContentSplitCnt)) {
                            // hint = hint & ",200"
                            Segment = ContentSplit[Ptr];
                            if ((Ptr == 0)) {
                                // 
                                //  Add in the non-command text that is before the first command
                                // 
                                result = (result + Segment);
                            }
                            else if ((Segment != "")) {
                                if ((genericController.vbInstr(1, Segment, "}}") == 0)) {
                                    // 
                                    //  No command found, return the marker and deliver the Segment
                                    // 
                                    // hint = hint & ",210"
                                    result = (result + ("{{" + Segment));
                                }
                                else {
                                    // 
                                    //  isolate the command
                                    // 
                                    // hint = hint & ",220"
                                    SegmentSplit = Segment.Split("}}");
                                    AcCmd = SegmentSplit[0];
                                    SegmentSplit[0] = "";
                                    SegmentSuffix = Join(SegmentSplit, "}}").Substring(2);
                                    if ((AcCmd.Trim() != "")) {
                                        // 
                                        //  isolate the arguments
                                        // 
                                        // hint = hint & ",230"
                                        AcCmdSplit = AcCmd.Split("?");
                                        ACType = AcCmdSplit[0].Trim();
                                        if ((UBound(AcCmdSplit) == 0)) {
                                            addonOptionString = "";
                                        }
                                        else {
                                            addonOptionString = AcCmdSplit[1];
                                            addonOptionString = genericController.decodeHtml(addonOptionString);
                                        }
                                        
                                        // 
                                        //  execute the command
                                        // 
                                        switch (genericController.vbUCase(ACType)) {
                                            case ACTypeDynamicForm:
                                                // 
                                                //  Dynamic Form - run the core addon replacement instead
                                                CPUtilsBaseClass.addonExecuteContext executeContext = new CPUtilsBaseClass.addonExecuteContext();
                                                // With...
                                                addonType = CPUtilsBaseClass.addonContext.ContextPage;
                                                cssContainerClass = "";
                                                cssContainerId = "";
                                                hostRecord = new CPUtilsBaseClass.addonExecuteHostRecordContext();
                                                // With...
                                                contentName = ContextContentName;
                                                fieldName = "";
                                                recordId = ContextRecordID;
                                                personalizationAuthenticated = personalizationIsAuthenticated;
                                                personalizationPeopleId = iPersonalizationPeopleId;
                                                instanceArguments = genericController.convertAddonArgumentstoDocPropertiesList(cpCore, addonOptionString);
                                                Models.Entity.addonModel addon = Models.Entity.addonModel.create(cpCore, addonGuidDynamicForm);
                                                cpCore.addon.execute(addon, executeContext);
                                                ACTypeChildList;
                                                // 
                                                //  Child Page List
                                                // 
                                                // hint = hint & ",320"
                                                ListName = addonController.getAddonOption("name", addonOptionString);
                                                result = (result + cpCore.doc.getChildPageList(ListName, ContextContentName, ContextRecordID, true));
                                                ACTypeTemplateText;
                                                // 
                                                //  Text Box = copied here from gethtmlbody
                                                // 
                                                CopyName = addonController.getAddonOption("new", addonOptionString);
                                                if ((CopyName == "")) {
                                                    CopyName = addonController.getAddonOption("name", addonOptionString);
                                                    if ((CopyName == "")) {
                                                        CopyName = "Default";
                                                    }
                                                    
                                                }
                                                
                                                result = (result + html_GetContentCopy(CopyName, "", iPersonalizationPeopleId, false, personalizationIsAuthenticated));
                                                ACTypeWatchList;
                                                // 
                                                //  Watch List
                                                // 
                                                // hint = hint & ",330"
                                                ListName = addonController.getAddonOption("LISTNAME", addonOptionString);
                                                SortField = addonController.getAddonOption("SORTFIELD", addonOptionString);
                                                SortReverse = genericController.EncodeBoolean(addonController.getAddonOption("SORTDIRECTION", addonOptionString));
                                                result = (result + cpCore.doc.main_GetWatchList(cpCore, ListName, SortField, SortReverse));
                                                // 
                                                //  Unrecognized command - put all the syntax back in
                                                // 
                                                // hint = hint & ",340"
                                                result = (result + ("{{" 
                                                            + (AcCmd + "}}")));
                                                // 
                                                //  add the SegmentSuffix back on
                                                // 
                                                result = (result + SegmentSuffix);
                                                break;
                                        }
                                    }
                                    
                                }
                                
                                // 
                                //  Encode into Javascript if required
                                // 
                                Ptr = (Ptr + 1);
                            }
                            
                            // 
                            //  Process Addons
                            //    parse as <!-- Addon "Addon Name","OptionString" -->
                            //    They are handled here because Addons are written against cpCoreClass, not the Content Server class
                            //    ...so Group Email can not process addons 8(
                            //    Later, remove the csv routine that translates <ac to this, and process it directly right here
                            //    Later, rewrite so addons call csv, not cpCoreClass, so email processing can include addons
                            //  (2/16/2010) - move csv_EncodeContent to csv, or wait and move it all to CP
                            //     eventually, everything should migrate to csv and/or cp to eliminate the cpCoreClass dependancy
                            //     and all add-ons run as processes the same as they run on pages, or as remote methods
                            //  (2/16/2010) - if <!-- AC --> has four arguments, the fourth is the addon guid
                            // 
                            if (((result.IndexOf(StartFlag, 0) + 1) 
                                        != 0)) {
                                while (((result.IndexOf(StartFlag, 0) + 1) 
                                            != 0)) {
                                    LineStart = genericController.vbInstr(1, result, StartFlag);
                                    LineEnd = genericController.vbInstr(LineStart, result, EndFlag);
                                    if ((LineEnd == 0)) {
                                        logController.appendLog(cpCore, "csv_EncodeContent9, Addon could not be inserted into content because the HTML comment holding the pos" +
                                            "ition is not formated correctly");
                                        break; //Warning!!! Review that break works as 'Exit Do' as it could be in a nested instruction like switch
                                    }
                                    else {
                                        AddonName = "";
                                        addonOptionString = "";
                                        ACInstanceID = "";
                                        AddonGuid = "";
                                        Copy = result.Substring((LineStart + 10), (LineEnd 
                                                        - (LineStart - 11)));
                                        ArgSplit = genericController.SplitDelimited(Copy, ",");
                                        ArgCnt = (UBound(ArgSplit) + 1);
                                        if ((ArgSplit[0] != "")) {
                                            AddonName = ArgSplit[0].Substring(1, (ArgSplit[0].Length - 2));
                                            if ((ArgCnt > 1)) {
                                                if ((ArgSplit[1] != "")) {
                                                    addonOptionString = ArgSplit[1].Substring(1, (ArgSplit[1].Length - 2));
                                                    addonOptionString = genericController.decodeHtml(addonOptionString.Trim());
                                                }
                                                
                                                if ((ArgCnt > 2)) {
                                                    if ((ArgSplit[2] != "")) {
                                                        ACInstanceID = ArgSplit[2].Substring(1, (ArgSplit[2].Length - 2));
                                                    }
                                                    
                                                    if ((ArgCnt > 3)) {
                                                        if ((ArgSplit[3] != "")) {
                                                            AddonGuid = ArgSplit[3].Substring(1, (ArgSplit[3].Length - 2));
                                                        }
                                                        
                                                    }
                                                    
                                                }
                                                
                                            }
                                            
                                            //  dont have any way of getting fieldname yet
                                            CPUtilsBaseClass.addonExecuteContext executeContext = new CPUtilsBaseClass.addonExecuteContext();
                                            // With...
                                            addonType = CPUtilsBaseClass.addonContext.ContextPage;
                                            cssContainerClass = "";
                                            cssContainerId = "";
                                            hostRecord = new CPUtilsBaseClass.addonExecuteHostRecordContext();
                                            // With...
                                            contentName = ContextContentName;
                                            fieldName = "";
                                            recordId = ContextRecordID;
                                            personalizationAuthenticated = personalizationIsAuthenticated;
                                            personalizationPeopleId = iPersonalizationPeopleId;
                                            instanceGuid = ACInstanceID;
                                            instanceArguments = genericController.convertAddonArgumentstoDocPropertiesList(cpCore, addonOptionString);
                                            if ((AddonGuid != "")) {
                                                Copy = cpCore.addon.execute(Models.Entity.addonModel.create(cpCore, AddonGuid), executeContext);
                                                // Copy = cpCore.addon.execute_legacy6(0, AddonGuid, addonOptionString, CPUtilsBaseClass.addonContext.ContextPage, ContextContentName, ContextRecordID, "", ACInstanceID, False, ignore_DefaultWrapperID, ignore_TemplateCaseOnly_Content, False, Nothing, "", Nothing, "", iPersonalizationPeopleId, personalizationIsAuthenticated)
                                            }
                                            else {
                                                Copy = cpCore.addon.execute(Models.Entity.addonModel.createByName(cpCore, AddonName), executeContext);
                                                // Copy = cpCore.addon.execute_legacy6(0, AddonName, addonOptionString, CPUtilsBaseClass.addonContext.ContextPage, ContextContentName, ContextRecordID, "", ACInstanceID, False, ignore_DefaultWrapperID, ignore_TemplateCaseOnly_Content, False, Nothing, "", Nothing, "", iPersonalizationPeopleId, personalizationIsAuthenticated)
                                            }
                                            
                                            result = (result.Substring(0, (LineStart - 1)) 
                                                        + (Copy + result.Substring((LineEnd + 3))));
                                        }
                                        
                                        // 
                                        //  process out text block comments inserted by addons
                                        //  remove all content between BlockTextStartMarker and the next BlockTextEndMarker, or end of copy
                                        //  exception made for the content with just the startmarker because when the AC tag is replaced with
                                        //  with the marker, encode content is called with the result, which is just the marker, and this
                                        //  section will remove it
                                        // 
                                        if ((!isEditingAnything 
                                                    && (result != BlockTextStartMarker))) {
                                            DoAnotherPass = true;
                                            while ((((result.IndexOf(BlockTextStartMarker, 0, System.StringComparison.OrdinalIgnoreCase) + 1) 
                                                        != 0) 
                                                        && DoAnotherPass)) {
                                                LineStart = genericController.vbInstr(1, result, BlockTextStartMarker, vbTextCompare);
                                                if ((LineStart == 0)) {
                                                    DoAnotherPass = false;
                                                }
                                                else {
                                                    LineEnd = genericController.vbInstr(LineStart, result, BlockTextEndMarker, vbTextCompare);
                                                    if ((LineEnd <= 0)) {
                                                        DoAnotherPass = false;
                                                        result = result.Substring(0, (LineStart - 1));
                                                    }
                                                    else {
                                                        LineEnd = genericController.vbInstr(LineEnd, result, " -->");
                                                        if ((LineEnd <= 0)) {
                                                            DoAnotherPass = false;
                                                        }
                                                        else {
                                                            result = (result.Substring(0, (LineStart - 1)) + result.Substring((LineEnd + 3)));
                                                            // returnValue = Mid(returnValue, 1, LineStart - 1) & Copy & Mid(returnValue, LineEnd + 4)
                                                        }
                                                        
                                                    }
                                                    
                                                }
                                                
                                            }
                                            
                                        }
                                        
                                        // 
                                        //  only valid for a webpage
                                        // 
                                        if (true) {
                                            // 
                                            //  Add in EditWrappers for Aggregate scripts and replacements
                                            //    This is also old -- used here because csv encode content can create replacements and links, but can not
                                            //    insert wrappers. This is all done in GetAddonContents() now. This routine is left only to
                                            //    handle old style calls in cache.
                                            // 
                                            // hint = hint & ",500, Adding edit wrappers"
                                            if (isEditingAnything) {
                                                if (((result.IndexOf("<!-- AFScript -->", 0, System.StringComparison.OrdinalIgnoreCase) + 1) 
                                                            != 0)) {
                                                    throw new ApplicationException("Unexpected exception");
                                                    //  Call cpcore.handleLegacyError7("returnValue", "AFScript Style edit wrappers are not supported")
                                                    Copy = this.getEditWrapper("Aggregate Script", "##MARKER##");
                                                    Wrapper = Copy.Split("##MARKER##");
                                                    result = genericController.vbReplace(result, "<!-- AFScript -->", Wrapper[0], 1, 99, vbTextCompare);
                                                    result = genericController.vbReplace(result, "<!-- /AFScript -->", Wrapper[1], 1, 99, vbTextCompare);
                                                }
                                                
                                                if (((result.IndexOf("<!-- AFReplacement -->", 0, System.StringComparison.OrdinalIgnoreCase) + 1) 
                                                            != 0)) {
                                                    throw new ApplicationException("Unexpected exception");
                                                    //  Call cpcore.handleLegacyError7("returnValue", "AFReplacement Style edit wrappers are not supported")
                                                    Copy = this.getEditWrapper("Aggregate Replacement", "##MARKER##");
                                                    Wrapper = Copy.Split("##MARKER##");
                                                    result = genericController.vbReplace(result, "<!-- AFReplacement -->", Wrapper[0], 1, 99, vbTextCompare);
                                                    result = genericController.vbReplace(result, "<!-- /AFReplacement -->", Wrapper[1], 1, 99, vbTextCompare);
                                                }
                                                
                                            }
                                            
                                            // 
                                            //  Process Feedback form
                                            // 
                                            // hint = hint & ",600, Handle webclient features"
                                            if ((genericController.vbInstr(1, result, FeedbackFormNotSupportedComment, vbTextCompare) != 0)) {
                                                result = genericController.vbReplace(result, FeedbackFormNotSupportedComment, pageContentController.main_GetFeedbackForm(cpCore, ContextContentName, ContextRecordID, ContextContactPeopleID), 1, 99, vbTextCompare);
                                            }
                                            
                                            // '
                                            // ' If any javascript or styles were added during encode, pick them up now
                                            // '
                                            // Copy = cpCore.doc.getNextJavascriptBodyEnd()
                                            // Do While Copy <> ""
                                            //     Call addScriptCode_body(Copy, "embedded content")
                                            //     Copy = cpCore.doc.getNextJavascriptBodyEnd()
                                            // Loop
                                            // '
                                            // ' current
                                            // '
                                            // Copy = cpCore.doc.getNextJSFilename()
                                            // Do While Copy <> ""
                                            //     If genericController.vbInstr(1, Copy, "://") <> 0 Then
                                            //     ElseIf Left(Copy, 1) = "/" Then
                                            //     Else
                                            //         Copy = cpCore.webServer.requestProtocol & cpCore.webServer.requestDomain & genericController.getCdnFileLink(cpCore, Copy)
                                            //     End If
                                            //     Call addScriptLink_Head(Copy, "embedded content")
                                            //     Copy = cpCore.doc.getNextJSFilename()
                                            // Loop
                                            // '
                                            // Copy = cpCore.doc.getJavascriptOnLoad()
                                            // Do While Copy <> ""
                                            //     Call addOnLoadJs(Copy, "")
                                            //     Copy = cpCore.doc.getJavascriptOnLoad()
                                            // Loop
                                            // 
                                            // Copy = cpCore.doc.getNextStyleFilenames()
                                            // Do While Copy <> ""
                                            //     If genericController.vbInstr(1, Copy, "://") <> 0 Then
                                            //     ElseIf Left(Copy, 1) = "/" Then
                                            //     Else
                                            //         Copy = cpCore.webServer.requestProtocol & cpCore.webServer.requestDomain & genericController.getCdnFileLink(cpCore, Copy)
                                            //     End If
                                            //     Call addStyleLink(Copy, "")
                                            //     Copy = cpCore.doc.getNextStyleFilenames()
                                            // Loop
                                        }
                                        
                                    }
                                    
                                    // 
                                    result = result;
                                    ((Exception)(ex));
                                    cpCore.handleException(ex);
                                    try {
                                        return result;
                                    }
                                    
                                    // 
                                    //  ================================================================================================================
                                    //    Upgrade old objects in content, and update changed resource library images
                                    //  ================================================================================================================
                                    // 
                                    ((string)(upgradeActiveContent(((string)(Source)))));
                                    string result = Source;
                                    try {
                                        string RecordVirtualPath = String.Empty;
                                        string RecordVirtualFilename;
                                        string RecordFilename;
                                        string RecordFilenameNoExt;
                                        string RecordFilenameExt = String.Empty;
                                        string[] SizeTest;
                                        string RecordAltSizeList;
                                        int TagPosEnd;
                                        int TagPosStart;
                                        bool InTag;
                                        int Pos;
                                        string FilenameSegment;
                                        int EndPos1;
                                        int EndPos2;
                                        string[] LinkSplit;
                                        int LinkCnt;
                                        int LinkPtr;
                                        string[] TableSplit;
                                        string TableName;
                                        string FieldName;
                                        int RecordID;
                                        bool SaveChanges;
                                        int EndPos;
                                        int Ptr;
                                        string FilePrefixSegment;
                                        bool ImageAllowUpdate;
                                        string ContentFilesLinkPrefix;
                                        string ResourceLibraryLinkPrefix;
                                        string TestChr;
                                        bool ParseError;
                                        result = Source;
                                        // 
                                        ContentFilesLinkPrefix = ("/" 
                                                    + (cpCore.serverConfig.appConfig.name + "/files/"));
                                        ResourceLibraryLinkPrefix = (ContentFilesLinkPrefix + "ccLibraryFiles/");
                                        ImageAllowUpdate = cpCore.siteProperties.getBoolean("ImageAllowUpdate", true);
                                        ImageAllowUpdate = (ImageAllowUpdate 
                                                    & ((Source.IndexOf(ResourceLibraryLinkPrefix, 0, System.StringComparison.OrdinalIgnoreCase) + 1) 
                                                    != 0));
                                        if (ImageAllowUpdate) {
                                            // 
                                            //  ----- Process Resource Library Images (swap in most current file)
                                            // 
                                            //    There is a better way:
                                            //    problem with replacing the images is the problem with parsing - too much work to find it
                                            //    instead, use new replacement tags <ac type=image src="cclibraryfiles/filename/00001" width=0 height=0>
                                            // 
                                            // 'hint = hint & ",010"
                                            ParseError = false;
                                            LinkSplit = Split(Source, ContentFilesLinkPrefix, ,, vbTextCompare);
                                            LinkCnt = (UBound(LinkSplit) + 1);
                                            for (LinkPtr = 1; (LinkPtr 
                                                        <= (LinkCnt - 1)); LinkPtr++) {
                                                // 
                                                //  Each LinkSplit(1...) is a segment that would have started with '/appname/files/'
                                                //  Next job is to determine if this sement is in a tag (<img src="...">) or in content ("..."e)
                                                //  For now, skip the ones in content
                                                // 
                                                // 'hint = hint & ",020"
                                                TagPosEnd = genericController.vbInstr(1, LinkSplit[LinkPtr], ">");
                                                TagPosStart = genericController.vbInstr(1, LinkSplit[LinkPtr], "<");
                                                if (((TagPosEnd == 0) 
                                                            && (TagPosStart == 0))) {
                                                    // 
                                                    //  no tags found, skip it
                                                    // 
                                                    InTag = false;
                                                }
                                                else if ((TagPosEnd == 0)) {
                                                    // 
                                                    //  no end tag, but found a start tag -> in content
                                                    // 
                                                    InTag = false;
                                                }
                                                else if ((TagPosEnd < TagPosStart)) {
                                                    // 
                                                    //  Found end before start - > in tag
                                                    // 
                                                    InTag = true;
                                                }
                                                else {
                                                    // 
                                                    //  Found start before end -> in content
                                                    // 
                                                    InTag = false;
                                                }
                                                
                                                if (InTag) {
                                                    // 'hint = hint & ",030"
                                                    TableSplit = LinkSplit[LinkPtr].Split("/");
                                                    if ((UBound(TableSplit) > 2)) {
                                                        TableName = TableSplit[0];
                                                        FieldName = TableSplit[1];
                                                        RecordID = genericController.EncodeInteger(TableSplit[2]);
                                                        FilenameSegment = TableSplit[3];
                                                        if (((TableName.ToLower() == "cclibraryfiles") 
                                                                    && ((FieldName.ToLower() == "filename") 
                                                                    && (RecordID != 0)))) {
                                                            Models.Entity.libraryFilesModel file = Models.Entity.libraryFilesModel.create(cpCore, RecordID);
                                                            if (file) {
                                                                IsNot;
                                                                null;
                                                                // 'hint = hint & ",060"
                                                                FieldName = "filename";
                                                                if (true) {
                                                                    // 
                                                                    //  now figure out how the link is delimited by how it starts
                                                                    //    go to the left and look for:
                                                                    //    ' ' - ignore spaces, continue forward until we find one of these
                                                                    //    '=' - means space delimited (src=/image.jpg), ends in ' ' or '>'
                                                                    //    '"' - means quote delimited (src="/image.jpg"), ends in '"'
                                                                    //    '>' - means this is not in an HTML tag - skip it (<B>image.jpg</b>)
                                                                    //    '<' - means god knows what, but its wrong, skip it
                                                                    //    '(' - means it is a URL(/image.jpg), go to ')'
                                                                    // 
                                                                    //  odd cases:
                                                                    //    URL( /image.jpg) -
                                                                    // 
                                                                    RecordVirtualFilename = file.Filename;
                                                                    RecordAltSizeList = file.AltSizeList;
                                                                    if ((RecordVirtualFilename == genericController.EncodeJavascript(RecordVirtualFilename))) {
                                                                        // 
                                                                        //  The javascript version of the filename must match the filename, since we have no way
                                                                        //  of differentiating a ligitimate file, from a block of javascript. If the file
                                                                        //  contains an apostrophe, the original code could have encoded it, but we can not here
                                                                        //  so the best plan is to skip it
                                                                        // 
                                                                        //  example:
                                                                        //  RecordVirtualFilename = "jay/files/cclibraryfiles/filename/000005/test.png"
                                                                        // 
                                                                        //  RecordFilename = "test.png"
                                                                        //  RecordFilenameAltSize = "" (does not exist - the record has the raw filename in it)
                                                                        //  RecordFilenameExt = "png"
                                                                        //  RecordFilenameNoExt = "test"
                                                                        // 
                                                                        //  RecordVirtualFilename = "jay/files/cclibraryfiles/filename/000005/test-100x200.png"
                                                                        //  this is a specail case - most cases to not have the alt size format saved in the filename
                                                                        //  RecordFilename = "test-100x200.png"
                                                                        //  RecordFilenameAltSize (does not exist - the record has the raw filename in it)
                                                                        //  RecordFilenameExt = "png"
                                                                        //  RecordFilenameNoExt = "test-100x200"
                                                                        //  this is wrong
                                                                        //    xRecordFilenameAltSize = "100x200"
                                                                        //    xRecordFilenameExt = "png"
                                                                        //    xRecordFilenameNoExt = "test"
                                                                        // 
                                                                        // 'hint = hint & ",080"
                                                                        Pos = InStrRev(RecordVirtualFilename, "/");
                                                                        RecordFilename = "";
                                                                        if ((Pos > 0)) {
                                                                            RecordVirtualPath = RecordVirtualFilename.Substring(0, Pos);
                                                                            RecordFilename = RecordVirtualFilename.Substring(Pos);
                                                                        }
                                                                        
                                                                        Pos = InStrRev(RecordFilename, ".");
                                                                        RecordFilenameNoExt = "";
                                                                        if ((Pos > 0)) {
                                                                            RecordFilenameExt = genericController.vbLCase(RecordFilename.Substring(Pos));
                                                                            RecordFilenameNoExt = genericController.vbLCase(RecordFilename.Substring(0, (Pos - 1)));
                                                                        }
                                                                        
                                                                        FilePrefixSegment = LinkSplit[(LinkPtr - 1)];
                                                                        if ((FilePrefixSegment.Length > 1)) {
                                                                            // 
                                                                            //  Look into FilePrefixSegment and see if we are in the querystring attribute of an <AC tag
                                                                            //    if so, the filename may be AddonEncoded and delimited with & (so skip it)
                                                                            Pos = InStrRev(FilePrefixSegment, "<");
                                                                            if ((Pos > 0)) {
                                                                                if ((genericController.vbLCase(FilePrefixSegment.Substring(Pos, 3)) != "ac ")) {
                                                                                    // 
                                                                                    //  look back in the FilePrefixSegment to find the character before the link
                                                                                    // 
                                                                                    EndPos = 0;
                                                                                    for (Ptr = FilePrefixSegment.Length; (Ptr <= 1); Ptr = (Ptr + -1)) {
                                                                                        TestChr = FilePrefixSegment.Substring((Ptr - 1), 1);
                                                                                        switch (TestChr) {
                                                                                            case "=":
                                                                                                EndPos1 = genericController.vbInstr(1, FilenameSegment, " ");
                                                                                                EndPos2 = genericController.vbInstr(1, FilenameSegment, ">");
                                                                                                if (((EndPos1 != 0) 
                                                                                                            && (EndPos2 != 0))) {
                                                                                                    if ((EndPos1 < EndPos2)) {
                                                                                                        EndPos = EndPos1;
                                                                                                    }
                                                                                                    else {
                                                                                                        EndPos = EndPos2;
                                                                                                    }
                                                                                                    
                                                                                                }
                                                                                                else if ((EndPos1 != 0)) {
                                                                                                    EndPos = EndPos1;
                                                                                                }
                                                                                                else if ((EndPos2 != 0)) {
                                                                                                    EndPos = EndPos2;
                                                                                                }
                                                                                                else {
                                                                                                    EndPos = 0;
                                                                                                }
                                                                                                
                                                                                                break; // TODO: Warning!!! Review that break works as 'Exit For' as it is inside another 'breakable' statement:Switch
                                                                                                break;
                                                                                            case "\"":
                                                                                                EndPos = genericController.vbInstr(1, FilenameSegment, "\"");
                                                                                                break; // TODO: Warning!!! Review that break works as 'Exit For' as it is inside another 'breakable' statement:Switch
                                                                                                break;
                                                                                            case "(":
                                                                                                if ((genericController.vbLCase(FilePrefixSegment.Substring((Ptr - 1), 7)) == "("")) {
                                                                                                    EndPos = genericController.vbInstr(1, FilenameSegment, "")");
                                                                                                }
                                                                                                else if ((genericController.vbLCase(FilePrefixSegment.Substring((Ptr - 1), 2)) == "(\'")) {
                                                                                                    EndPos = genericController.vbInstr(1, FilenameSegment, "\')");
                                                                                                }
                                                                                                else if ((genericController.vbLCase(FilePrefixSegment.Substring((Ptr - 1), 2)) == "(\"")) {
                                                                                                    EndPos = genericController.vbInstr(1, FilenameSegment, "\")");
                                                                                                }
                                                                                                else {
                                                                                                    EndPos = genericController.vbInstr(1, FilenameSegment, ")");
                                                                                                }
                                                                                                
                                                                                                break; // TODO: Warning!!! Review that break works as 'Exit For' as it is inside another 'breakable' statement:Switch
                                                                                                break;
                                                                                            case "\'":
                                                                                                EndPos = genericController.vbInstr(1, FilenameSegment, "\'");
                                                                                                break; // TODO: Warning!!! Review that break works as 'Exit For' as it is inside another 'breakable' statement:Switch
                                                                                                break;
                                                                                            case ">":
                                                                                            case "<":
                                                                                                ParseError = true;
                                                                                                break; // TODO: Warning!!! Review that break works as 'Exit For' as it is inside another 'breakable' statement:Switch
                                                                                                break;
                                                                                        }
                                                                                    }
                                                                                    
                                                                                    // 
                                                                                    //  check link
                                                                                    // 
                                                                                    if ((EndPos == 0)) {
                                                                                        ParseError = true;
                                                                                        break;
                                                                                    }
                                                                                    else {
                                                                                        string ImageFilename;
                                                                                        string SegmentAfterImage;
                                                                                        string ImageFilenameNoExt;
                                                                                        string ImageFilenameExt;
                                                                                        string ImageAltSize;
                                                                                        // 'hint = hint & ",120"
                                                                                        SegmentAfterImage = FilenameSegment.Substring((EndPos - 1));
                                                                                        ImageFilename = genericController.DecodeResponseVariable(FilenameSegment.Substring(0, (EndPos - 1)));
                                                                                        ImageFilenameNoExt = ImageFilename;
                                                                                        ImageFilenameExt = "";
                                                                                        Pos = InStrRev(ImageFilename, ".");
                                                                                        if ((Pos > 0)) {
                                                                                            ImageFilenameNoExt = genericController.vbLCase(ImageFilename.Substring(0, (Pos - 1)));
                                                                                            ImageFilenameExt = genericController.vbLCase(ImageFilename.Substring(Pos));
                                                                                        }
                                                                                        
                                                                                        // 
                                                                                        //  Get ImageAltSize
                                                                                        // 
                                                                                        // 'hint = hint & ",130"
                                                                                        ImageAltSize = "";
                                                                                        if ((ImageFilenameNoExt == RecordFilenameNoExt)) {
                                                                                            // 
                                                                                            //  Exact match
                                                                                            // 
                                                                                        }
                                                                                        else if ((genericController.vbInstr(1, ImageFilenameNoExt, RecordFilenameNoExt, vbTextCompare) != 1)) {
                                                                                            // 
                                                                                            //  There was a change and the recordfilename is not part of the imagefilename
                                                                                            // 
                                                                                        }
                                                                                        else {
                                                                                            // 
                                                                                            //  the recordfilename is the first part of the imagefilename - Get ImageAltSize
                                                                                            // 
                                                                                            ImageAltSize = ImageFilenameNoExt.Substring(RecordFilenameNoExt.Length);
                                                                                            if ((ImageAltSize.Substring(0, 1) != "-")) {
                                                                                                ImageAltSize = "";
                                                                                            }
                                                                                            else {
                                                                                                ImageAltSize = ImageAltSize.Substring(1);
                                                                                                SizeTest = ImageAltSize.Split("x");
                                                                                                if ((UBound(SizeTest) != 1)) {
                                                                                                    ImageAltSize = "";
                                                                                                }
                                                                                                else if ((genericController.vbIsNumeric(SizeTest[0]) && genericController.vbIsNumeric(SizeTest[1]))) {
                                                                                                    ImageFilenameNoExt = RecordFilenameNoExt;
                                                                                                    // ImageFilenameNoExt = Mid(ImageFilenameNoExt, 1, Pos - 1)
                                                                                                    // RecordFilenameNoExt = Mid(RecordFilename, 1, Pos - 1)
                                                                                                }
                                                                                                else {
                                                                                                    ImageAltSize = "";
                                                                                                }
                                                                                                
                                                                                            }
                                                                                            
                                                                                        }
                                                                                        
                                                                                        // 
                                                                                        //  problem - in the case where the recordfilename = img-100x200, the imagefilenamenoext is img
                                                                                        // 
                                                                                        // 'hint = hint & ",140"
                                                                                        if (((RecordFilenameNoExt != ImageFilenameNoExt) 
                                                                                                    || (RecordFilenameExt != ImageFilenameExt))) {
                                                                                            // 
                                                                                            //  There has been a change
                                                                                            // 
                                                                                            string NewRecordFilename;
                                                                                            int ImageHeight;
                                                                                            int ImageWidth;
                                                                                            NewRecordFilename = (RecordVirtualPath 
                                                                                                        + (RecordFilenameNoExt + ("." + RecordFilenameExt)));
                                                                                            // 
                                                                                            //  realtime image updates replace without creating new size - that is for the edit interface
                                                                                            // 
                                                                                            //  put the New file back into the tablesplit in case there are more then 4 splits
                                                                                            // 
                                                                                            TableSplit[0] = "";
                                                                                            TableSplit[1] = "";
                                                                                            TableSplit[2] = "";
                                                                                            TableSplit[3] = SegmentAfterImage;
                                                                                            NewRecordFilename = (genericController.EncodeURL(NewRecordFilename) + Join(TableSplit, "/").Substring(3));
                                                                                            LinkSplit[LinkPtr] = NewRecordFilename;
                                                                                            SaveChanges = true;
                                                                                        }
                                                                                        
                                                                                    }
                                                                                    
                                                                                }
                                                                                
                                                                            }
                                                                            
                                                                        }
                                                                        
                                                                    }
                                                                    
                                                                }
                                                                
                                                            }
                                                            
                                                        }
                                                        
                                                    }
                                                    
                                                }
                                                
                                                if (ParseError) {
                                                    break;
                                                }
                                                
                                            }
                                            
                                            // 'hint = hint & ",910"
                                            if ((SaveChanges 
                                                        && !ParseError)) {
                                                result = Join(LinkSplit, ContentFilesLinkPrefix);
                                            }
                                            
                                        }
                                        
                                        // 'hint = hint & ",920"
                                        if (!ParseError) {
                                            // 
                                            //  Convert ACTypeDynamicForm to Add-on
                                            // 
                                            if ((genericController.vbInstr(1, result, ("<ac type=\"" + ACTypeDynamicForm), vbTextCompare) != 0)) {
                                                result = genericController.vbReplace(result, "type=\"DYNAMICFORM\"", "TYPE=\"aggregatefunction\"", 1, 99, vbTextCompare);
                                                result = genericController.vbReplace(result, "name=\"DYNAMICFORM\"", "name=\"DYNAMIC FORM\"", 1, 99, vbTextCompare);
                                            }
                                            
                                        }
                                        
                                        // 'hint = hint & ",930"
                                        if (ParseError) {
                                            result = ("" + ("\r\n" + ("<!-- warning: parsing aborted on ccLibraryFile replacement -->" + ("\r\n" 
                                                        + (result + ("\r\n" + "<!-- /warning: parsing aborted on ccLibraryFile replacement -->"))))));
                                        }
                                        
                                        // 
                                        //  {{content}} should be <ac type="templatecontent" etc>
                                        //  the merge is now handled in csv_EncodeActiveContent, but some sites have hand {{content}} tags entered
                                        // 
                                        // 'hint = hint & ",940"
                                        if ((genericController.vbInstr(1, result, "{{content}}", vbTextCompare) != 0)) {
                                            result = genericController.vbReplace(result, "{{content}}", ("<AC type=\"" 
                                                            + (ACTypeTemplateContent + "\">")), 1, 99, vbTextCompare);
                                        }
                                        
                                    }
                                    catch (Exception ex) {
                                        cpCore.handleException(ex);
                                    }
                                    
                                    return result;
                                    // 
                                    // ============================================================================
                                    //    csv_GetContentCopy3
                                    //        To get them, cp.content.getCopy must call the cpCoreClass version, which calls this for the content
                                    // ============================================================================
                                    // 
                                    ((string)(html_GetContentCopy(((string)(CopyName)), ((string)(DefaultContent)), ((int)(personalizationPeopleId)), ((bool)(AllowEditWrapper)), ((bool)(personalizationIsAuthenticated)))));
                                    string returnCopy = "";
                                    try {
                                        // 
                                        int CS;
                                        int RecordID;
                                        int contactPeopleId;
                                        string Return_ErrorMessage = "";
                                        CS = cpCore.db.csOpen("copy content", ("Name=" + cpCore.db.encodeSQLText(CopyName)), "ID", ,, 0, ,, "Name,ID,Copy,modifiedBy");
                                        if (!cpCore.db.csOk(CS)) {
                                            cpCore.db.csClose(CS);
                                            CS = cpCore.db.csInsertRecord("copy content", 0);
                                            if (cpCore.db.csOk(CS)) {
                                                RecordID = cpCore.db.csGetInteger(CS, "ID");
                                                cpCore.db.csSet(CS, "name", CopyName);
                                                cpCore.db.csSet(CS, "copy", genericController.encodeText(DefaultContent));
                                                cpCore.db.csSave2(CS);
                                                //    Call cpCore.workflow.publishEdit("copy content", RecordID)
                                            }
                                            
                                        }
                                        
                                        if (cpCore.db.csOk(CS)) {
                                            RecordID = cpCore.db.csGetInteger(CS, "ID");
                                            contactPeopleId = cpCore.db.csGetInteger(CS, "modifiedBy");
                                            returnCopy = cpCore.db.csGet(CS, "Copy");
                                            returnCopy = executeContentCommands(null, returnCopy, CPUtilsBaseClass.addonContext.ContextPage, personalizationPeopleId, personalizationIsAuthenticated, Return_ErrorMessage);
                                            returnCopy = convertActiveContentToHtmlForWebRender(returnCopy, "copy content", RecordID, personalizationPeopleId, "", 0, CPUtilsBaseClass.addonContext.ContextPage);
                                            // returnCopy = convertActiveContent_internal(returnCopy, personalizationPeopleId, "copy content", RecordID, contactPeopleId, False, False, True, True, False, True, "", "", False, 0, "", CPUtilsBaseClass.addonContext.ContextPage, False, Nothing, False)
                                            // 
                                            if (true) {
                                                if (cpCore.doc.authContext.isEditingAnything()) {
                                                    returnCopy = (cpCore.db.csGetRecordEditLink(CS, false) + returnCopy);
                                                    if (AllowEditWrapper) {
                                                        returnCopy = this.getEditWrapper("copy content", returnCopy);
                                                    }
                                                    
                                                }
                                                
                                            }
                                            
                                        }
                                        
                                        cpCore.db.csClose(CS);
                                    }
                                    catch (Exception ex) {
                                        cpCore.handleException(ex);
                                        throw;
                                    }
                                    
                                    return returnCopy;
                                    // 
                                    // 
                                    // 
                                    main_AddTabEntry(((string)(Caption)), ((string)(Link)), ((bool)(IsHit)), Optional, StylePrefixAsString=, Optional, LiveBodyAsString=);
                                    // TODO: On Error GoTo Warning!!!: The statement is not translatable 
                                    // 'Dim th as integer : th = profileLogMethodEnter("AddTabEntry")
                                    // 
                                    //  should use the ccNav object, no the ccCommon module for this code
                                    // 
                                    cpCore.menuTab.AddEntry(genericController.encodeText(Caption), genericController.encodeText(Link), genericController.EncodeBoolean(IsHit), genericController.encodeText(StylePrefix));
                                    // Call ccAddTabEntry(genericController.encodeText(Caption), genericController.encodeText(Link), genericController.EncodeBoolean(IsHit), genericController.encodeText(StylePrefix), genericController.encodeText(LiveBody))
                                    // 
                                    return;
                                ErrorTrap:
                                    throw new ApplicationException("Unexpected exception");
                                    //  Call cpcore.handleLegacyError18("main_AddTabEntry")
                                    //         '
                                    //         '
                                    //         '
                                    //         Public Function main_GetTabs() As String
                                    //             On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetTabs")
                                    //             '
                                    //             ' should use the ccNav object, no the ccCommon module for this code
                                    //             '
                                    //             '
                                    //             main_GetTabs = menuTab.GetTabs()
                                    //             '    main_GetTabs = ccGetTabs()
                                    //             '
                                    //             Exit Function
                                    // ErrorTrap:
                                    //             throw new applicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_GetTabs")
                                    //         End Function
                                    // 
                                    // 
                                    // 
                                    main_AddLiveTabEntry(((string)(Caption)), ((string)(LiveBody)), Optional, StylePrefixAsString=);
                                    // TODO: On Error GoTo Warning!!!: The statement is not translatable 
                                    // 'Dim th as integer : th = profileLogMethodEnter("AddLiveTabEntry")
                                    // 
                                    //  should use the ccNav object, no the ccCommon module for this code
                                    // 
                                    if ((cpCore.doc.menuLiveTab == null)) {
                                        cpCore.doc.menuLiveTab = new menuLiveTabController();
                                    }
                                    
                                    cpCore.doc.menuLiveTab.AddEntry(genericController.encodeText(Caption), genericController.encodeText(LiveBody), genericController.encodeText(StylePrefix));
                                    // 
                                    return;
                                ErrorTrap:
                                    throw new ApplicationException("Unexpected exception");
                                    //  Call cpcore.handleLegacyError18("main_AddLiveTabEntry")
                                    // 
                                    // 
                                    // 
                                    ((string)(main_GetLiveTabs()));
                                    // TODO: On Error GoTo Warning!!!: The statement is not translatable 
                                    // Dim th as integer: th = profileLogMethodEnter("GetLiveTabs")
                                    // 
                                    //  should use the ccNav object, no the ccCommon module for this code
                                    // 
                                    if ((cpCore.doc.menuLiveTab == null)) {
                                        cpCore.doc.menuLiveTab = new menuLiveTabController();
                                    }
                                    
                                    main_GetLiveTabs = cpCore.doc.menuLiveTab.GetTabs();
                                    // 
                                    // TODO: Exit Function: Warning!!! Need to return the value
                                    return;
                                ErrorTrap:
                                    throw new ApplicationException("Unexpected exception");
                                    //  Call cpcore.handleLegacyError18("main_GetLiveTabs")
                                    // 
                                    // 
                                    // 
                                    menu_AddComboTabEntry(((string)(Caption)), ((string)(Link)), ((string)(AjaxLink)), ((string)(LiveBody)), ((bool)(IsHit)), ((string)(ContainerClass)));
                                    // TODO: On Error GoTo Warning!!!: The statement is not translatable 
                                    // Dim th as integer: th = profileLogMethodEnter("AddComboTabEntry")
                                    // 
                                    //  should use the ccNav object, no the ccCommon module for this code
                                    // 
                                    if ((cpCore.doc.menuComboTab == null)) {
                                        cpCore.doc.menuComboTab = new menuComboTabController();
                                    }
                                    
                                    cpCore.doc.menuComboTab.AddEntry(Caption, Link, AjaxLink, LiveBody, IsHit, ContainerClass);
                                    // 
                                    return;
                                ErrorTrap:
                                    throw new ApplicationException("Unexpected exception");
                                    //  Call cpcore.handleLegacyError18("main_AddComboTabEntry")
                                    // 
                                    // 
                                    // 
                                    ((string)(menu_GetComboTabs()));
                                    // TODO: On Error GoTo Warning!!!: The statement is not translatable 
                                    // Dim th as integer: th = profileLogMethodEnter("GetComboTabs")
                                    // 
                                    //  should use the ccNav object, no the ccCommon module for this code
                                    // 
                                    if ((cpCore.doc.menuComboTab == null)) {
                                        cpCore.doc.menuComboTab = new menuComboTabController();
                                    }
                                    
                                    menu_GetComboTabs = cpCore.doc.menuComboTab.GetTabs();
                                    // 
                                    // TODO: Exit Function: Warning!!! Need to return the value
                                    return;
                                ErrorTrap:
                                    throw new ApplicationException("Unexpected exception");
                                    //  Call cpcore.handleLegacyError18("main_GetComboTabs")
                                    // '
                                    // '================================================================================================================
                                    // '   main_Get SharedStyleFilelist
                                    // '
                                    // '   SharedStyleFilelist is a list of filenames (with conditional comments) that should be included on pages
                                    // '   that call out the SharedFileIDList
                                    // '
                                    // '   Suffix and Prefix are for Conditional Comments around the style tag
                                    // '
                                    // '   SharedStyleFileList is
                                    // '       crlf filename < Prefix< Suffix
                                    // '       crlf filename < Prefix< Suffix
                                    // '       ...
                                    // '       Prefix and Suffix are htmlencoded
                                    // '
                                    // '   SharedStyleMap file
                                    // '       crlf StyleID tab StyleFilename < Prefix < Suffix, IncludedStyleFilename < Prefix < Suffix, ...
                                    // '       crlf StyleID tab StyleFilename < Prefix < Suffix, IncludedStyleFilename < Prefix < Suffix, ...
                                    // '       ...
                                    // '       StyleID is 0 if Always include is set
                                    // '       The Prefix and Suffix have had crlf removed, and comma replaced with ,
                                    // '================================================================================================================
                                    // '
                                    // Friend Shared Function main_GetSharedStyleFileList(cpCore As coreClass, SharedStyleIDList As String, main_IsAdminSite As Boolean) As String
                                    //     Dim result As String = ""
                                    //     '
                                    //     Dim Prefix As String
                                    //     Dim Suffix As String
                                    //     Dim Files() As String
                                    //     Dim Pos As Integer
                                    //     Dim SrcID As Integer
                                    //     Dim Srcs() As String
                                    //     Dim SrcCnt As Integer
                                    //     Dim IncludedStyleFilename As String
                                    //     Dim styleId As Integer
                                    //     Dim LastStyleID As Integer
                                    //     Dim CS As Integer
                                    //     Dim Ptr As Integer
                                    //     Dim MapList As String
                                    //     Dim Map() As String
                                    //     Dim MapCnt As Integer
                                    //     Dim MapRow As Integer
                                    //     Dim Filename As String
                                    //     Dim FileList As String
                                    //     Dim SQL As String = String.Empty
                                    //     Dim BakeName As String
                                    //     '
                                    //     If main_IsAdminSite Then
                                    //         BakeName = "SharedStyleMap-Admin"
                                    //     Else
                                    //         BakeName = "SharedStyleMap-Public"
                                    //     End If
                                    //     MapList = genericController.encodeText(cpCore.cache.getObject(Of String)(BakeName))
                                    //     If MapList = "" Then
                                    //         '
                                    //         ' BuildMap
                                    //         '
                                    //         MapList = ""
                                    //         If True Then
                                    //             '
                                    //             ' add prefix and suffix conditional comments
                                    //             '
                                    //             SQL = "select s.ID,s.Stylefilename,s.Prefix,s.Suffix,i.StyleFilename as iStylefilename,s.AlwaysInclude,i.Prefix as iPrefix,i.Suffix as iSuffix" _
                                    //                 & " from ((ccSharedStyles s" _
                                    //                 & " left join ccSharedStylesIncludeRules r on r.StyleID=s.id)" _
                                    //                 & " left join ccSharedStyles i on i.id=r.IncludedStyleID)" _
                                    //                 & " where ( s.active<>0 )and((i.active is null)or(i.active<>0))"
                                    //         End If
                                    //         CS = cpCore.db.cs_openSql(SQL)
                                    //         LastStyleID = 0
                                    //         Do While cpCore.db.cs_ok(CS)
                                    //             styleId = cpCore.db.cs_getInteger(CS, "ID")
                                    //             If styleId <> LastStyleID Then
                                    //                 Filename = cpCore.db.cs_get(CS, "StyleFilename")
                                    //                 Prefix = genericController.vbReplace(cpCore.html.main_encodeHTML(cpCore.db.cs_get(CS, "Prefix")), ",", ",")
                                    //                 Suffix = genericController.vbReplace(cpCore.html.main_encodeHTML(cpCore.db.cs_get(CS, "Suffix")), ",", ",")
                                    //                 If (Not main_IsAdminSite) And cpCore.db.cs_getBoolean(CS, "alwaysinclude") Then
                                    //                     MapList = MapList & vbCrLf & "0" & vbTab & Filename & "<" & Prefix & "<" & Suffix
                                    //                 Else
                                    //                     MapList = MapList & vbCrLf & styleId & vbTab & Filename & "<" & Prefix & "<" & Suffix
                                    //                 End If
                                    //             End If
                                    //             IncludedStyleFilename = cpCore.db.cs_getText(CS, "iStylefilename")
                                    //             Prefix = cpCore.html.main_encodeHTML(cpCore.db.cs_get(CS, "iPrefix"))
                                    //             Suffix = cpCore.html.main_encodeHTML(cpCore.db.cs_get(CS, "iSuffix"))
                                    //             If IncludedStyleFilename <> "" Then
                                    //                 MapList = MapList & "," & IncludedStyleFilename & "<" & Prefix & "<" & Suffix
                                    //             End If
                                    //             Call cpCore.db.cs_goNext(CS)
                                    //         Loop
                                    //         If MapList = "" Then
                                    //             MapList = ","
                                    //         End If
                                    //         Call cpCore.cache.setObject(BakeName, MapList, "Shared Styles")
                                    //     End If
                                    //     If (MapList <> "") And (MapList <> ",") Then
                                    //         Srcs = Split(SharedStyleIDList, ",")
                                    //         SrcCnt = UBound(Srcs) + 1
                                    //         Map = Split(MapList, vbCrLf)
                                    //         MapCnt = UBound(Map) + 1
                                    //         '
                                    //         ' Add stylesheets with AlwaysInclude set (ID is saved as 0 in Map)
                                    //         '
                                    //         FileList = ""
                                    //         For MapRow = 0 To MapCnt - 1
                                    //             If genericController.vbInstr(1, Map(MapRow), "0" & vbTab) = 1 Then
                                    //                 Pos = genericController.vbInstr(1, Map(MapRow), vbTab)
                                    //                 If Pos > 0 Then
                                    //                     FileList = FileList & "," & Mid(Map(MapRow), Pos + 1)
                                    //                 End If
                                    //             End If
                                    //         Next
                                    //         '
                                    //         ' create a filelist of everything that is needed, might be duplicates
                                    //         '
                                    //         For Ptr = 0 To SrcCnt - 1
                                    //             SrcID = genericController.EncodeInteger(Srcs(Ptr))
                                    //             If SrcID <> 0 Then
                                    //                 For MapRow = 0 To MapCnt - 1
                                    //                     If genericController.vbInstr(1, Map(MapRow), SrcID & vbTab) <> 0 Then
                                    //                         Pos = genericController.vbInstr(1, Map(MapRow), vbTab)
                                    //                         If Pos > 0 Then
                                    //                             FileList = FileList & "," & Mid(Map(MapRow), Pos + 1)
                                    //                         End If
                                    //                     End If
                                    //                 Next
                                    //             End If
                                    //         Next
                                    //         '
                                    //         ' dedup the filelist and convert it to crlf delimited
                                    //         '
                                    //         If FileList <> "" Then
                                    //             Files = Split(FileList, ",")
                                    //             For Ptr = 0 To UBound(Files)
                                    //                 Filename = Files(Ptr)
                                    //                 If genericController.vbInstr(1, result, Filename, vbTextCompare) = 0 Then
                                    //                     result = result & vbCrLf & Filename
                                    //                 End If
                                    //             Next
                                    //         End If
                                    //     End If
                                    //     Return result
                                    // End Function
                                    // 
                                    // 
                                    // 
                                    ((string)(main_GetResourceLibrary2(((string)(RootFolderName)), ((bool)(AllowSelectResource)), ((string)(SelectResourceEditorName)), ((string)(SelectLinkObjectName)), ((bool)(AllowGroupAdd)))));
                                    string addonGuidResourceLibrary = "{564EF3F5-9673-4212-A692-0942DD51FF1A}";
                                    Dictionary<string, string> arguments = new Dictionary<string, string>();
                                    arguments.Add("RootFolderName", RootFolderName);
                                    arguments.Add("AllowSelectResource", AllowSelectResource.ToString());
                                    arguments.Add("SelectResourceEditorName", SelectResourceEditorName);
                                    arguments.Add("SelectLinkObjectName", SelectLinkObjectName);
                                    arguments.Add("AllowGroupAdd", AllowGroupAdd.ToString());
                                    return cpCore.addon.execute(addonModel.create(cpCore, addonGuidResourceLibrary), new CPUtilsBaseClass.addonExecuteContext(), With, {, ., addonType=CPUtilsBaseClass.addonContext.ContextAdmin, ., instanceArguments=arguments);
                                    // Dim Option_String As String
                                    // Option_String = "" _
                                    //     & "RootFolderName=" & RootFolderName _
                                    //     & "&AllowSelectResource=" & AllowSelectResource _
                                    //     & "&SelectResourceEditorName=" & SelectResourceEditorName _
                                    //     & "&SelectLinkObjectName=" & SelectLinkObjectName _
                                    //     & "&AllowGroupAdd=" & AllowGroupAdd _
                                    //     & ""
                                    // Return cpCore.addon.execute_legacy4(addonGuidResourceLibrary, Option_String, CPUtilsBaseClass.addonContext.ContextAdmin)
                                    // 
                                    // ========================================================================
                                    //  Read and save a main_GetFormInputCheckList
                                    //    see main_GetFormInputCheckList for an explaination of the input
                                    // ========================================================================
                                    // 
                                    main_ProcessCheckList(((string)(TagName)), ((string)(PrimaryContentName)), ((string)(PrimaryRecordID)), ((string)(SecondaryContentName)), ((string)(RulesContentName)), ((string)(RulesPrimaryFieldname)), ((string)(RulesSecondaryFieldName)));
                                    // 
                                    string rulesTablename;
                                    string SQL;
                                    DataTable currentRules;
                                    int currentRulesCnt;
                                    bool RuleFound;
                                    int RuleId;
                                    int Ptr;
                                    int TestRecordIDLast;
                                    int TestRecordID;
                                    string dupRuleIdList;
                                    int GroupCnt;
                                    int GroupPtr;
                                    string MethodName;
                                    int SecondaryRecordID;
                                    bool RuleNeeded;
                                    int CSRule;
                                    bool RuleContentChanged;
                                    bool SupportRuleCopy;
                                    string RuleCopy;
                                    // 
                                    MethodName = "ProcessCheckList";
                                    GroupCnt = cpCore.docProperties.getInteger((TagName + ".RowCount"));
                                    if ((GroupCnt > 0)) {
                                        // 
                                        //  Test if RuleCopy is supported
                                        // 
                                        SupportRuleCopy = Models.Complex.cdefModel.isContentFieldSupported(cpCore, RulesContentName, "RuleCopy");
                                        if (SupportRuleCopy) {
                                            SupportRuleCopy = (SupportRuleCopy & Models.Complex.cdefModel.isContentFieldSupported(cpCore, SecondaryContentName, "AllowRuleCopy"));
                                            if (SupportRuleCopy) {
                                                SupportRuleCopy = (SupportRuleCopy & Models.Complex.cdefModel.isContentFieldSupported(cpCore, SecondaryContentName, "RuleCopyCaption"));
                                            }
                                            
                                        }
                                        
                                        // 
                                        //  Go through each checkbox and check for a rule
                                        // 
                                        // 
                                        //  try
                                        // 
                                        currentRulesCnt = 0;
                                        dupRuleIdList = "";
                                        rulesTablename = Models.Complex.cdefModel.getContentTablename(cpCore, RulesContentName);
                                        SQL = ("select " 
                                                    + (RulesSecondaryFieldName + (",id from " 
                                                    + (rulesTablename + (" where (" 
                                                    + (RulesPrimaryFieldname + ("=" 
                                                    + (PrimaryRecordID + (")and(active<>0) order by " + RulesSecondaryFieldName)))))))));
                                        currentRulesCnt = 0;
                                        currentRules = cpCore.db.executeQuery(SQL);
                                        currentRulesCnt = currentRules.Rows.Count;
                                        for (GroupPtr = 0; (GroupPtr 
                                                    <= (GroupCnt - 1)); GroupPtr++) {
                                            // 
                                            //  ----- Read Response
                                            // 
                                            SecondaryRecordID = cpCore.docProperties.getInteger((TagName + ("." 
                                                            + (GroupPtr + ".ID"))));
                                            RuleCopy = cpCore.docProperties.getText((TagName + ("." 
                                                            + (GroupPtr + ".RuleCopy"))));
                                            RuleNeeded = cpCore.docProperties.getBoolean((TagName + ("." + GroupPtr)));
                                            // 
                                            //  ----- Update Record
                                            // 
                                            RuleFound = false;
                                            RuleId = 0;
                                            TestRecordIDLast = 0;
                                            for (Ptr = 0; (Ptr 
                                                        <= (currentRulesCnt - 1)); Ptr++) {
                                                TestRecordID = genericController.EncodeInteger(currentRules.Rows[Ptr].Item[0]);
                                                if ((TestRecordID == 0)) {
                                                    // 
                                                    //  skip
                                                    // 
                                                }
                                                else if ((TestRecordID == SecondaryRecordID)) {
                                                    // 
                                                    //  hit
                                                    // 
                                                    RuleFound = true;
                                                    RuleId = genericController.EncodeInteger(currentRules.Rows[Ptr].Item[1]);
                                                    break;
                                                }
                                                else if ((TestRecordID == TestRecordIDLast)) {
                                                    // 
                                                    //  dup
                                                    // 
                                                    dupRuleIdList = (dupRuleIdList + ("," + genericController.EncodeInteger(currentRules.Rows[Ptr].Item[1])));
                                                    currentRules.Rows[Ptr].Item[0] = 0;
                                                }
                                                
                                                TestRecordIDLast = TestRecordID;
                                            }
                                            
                                            if ((SupportRuleCopy 
                                                        && (RuleNeeded && RuleFound))) {
                                                // 
                                                //  Record exists and is needed, update the rule copy
                                                // 
                                                SQL = ("update " 
                                                            + (rulesTablename + (" set rulecopy=" 
                                                            + (cpCore.db.encodeSQLText(RuleCopy) + (" where id=" + RuleId)))));
                                                cpCore.db.executeQuery(SQL);
                                            }
                                            else if ((RuleNeeded 
                                                        && !RuleFound)) {
                                                // 
                                                //  No record exists, and one is needed
                                                // 
                                                CSRule = cpCore.db.csInsertRecord(RulesContentName);
                                                if (cpCore.db.csOk(CSRule)) {
                                                    cpCore.db.csSet(CSRule, "Active", RuleNeeded);
                                                    cpCore.db.csSet(CSRule, RulesPrimaryFieldname, PrimaryRecordID);
                                                    cpCore.db.csSet(CSRule, RulesSecondaryFieldName, SecondaryRecordID);
                                                    if (SupportRuleCopy) {
                                                        cpCore.db.csSet(CSRule, "RuleCopy", RuleCopy);
                                                    }
                                                    
                                                }
                                                
                                                cpCore.db.csClose(CSRule);
                                                RuleContentChanged = true;
                                            }
                                            else if ((!RuleNeeded 
                                                        && RuleFound)) {
                                                // 
                                                //  Record exists and it is not needed
                                                // 
                                                SQL = ("delete from " 
                                                            + (rulesTablename + (" where id=" + RuleId)));
                                                cpCore.db.executeQuery(SQL);
                                                RuleContentChanged = true;
                                            }
                                            
                                        }
                                        
                                        // 
                                        //  delete dups
                                        // 
                                        if ((dupRuleIdList != "")) {
                                            SQL = ("delete from " 
                                                        + (rulesTablename + (" where id in (" 
                                                        + (dupRuleIdList.Substring(1) + ")"))));
                                            cpCore.db.executeQuery(SQL);
                                            RuleContentChanged = true;
                                        }
                                        
                                    }
                                    
                                    if (RuleContentChanged) {
                                        cpCore.cache.invalidateAllObjectsInContent(RulesContentName);
                                    }
                                    
                                    // '
                                    // '========================================================================
                                    // ' ----- Ends an HTML page
                                    // '========================================================================
                                    // '
                                    // Public Function getHtmlDoc_afterBodyHtml() As String
                                    //     Return "" _
                                    //         & cr & "</body>" _
                                    //         & vbCrLf & "</html>"
                                    // End Function
                                    // 
                                    // ========================================================================
                                    //  main_GetRecordEditLink( iContentName, iRecordID )
                                    // 
                                    //    iContentName The content for this link
                                    //    iRecordID    The ID of the record in the Table
                                    // ========================================================================
                                    // 
                                    ((string)(main_GetRecordEditLink(((string)(ContentName)), ((int)(RecordID)), Optional, AllowCutAsBoolean=False)));
                                    main_GetRecordEditLink = main_GetRecordEditLink2(ContentName, RecordID, genericController.EncodeBoolean(AllowCut), "", cpCore.doc.authContext.isEditing(ContentName));
                                    // 
                                    // ========================================================================
                                    //  main_GetRecordEditLink2( iContentName, iRecordID, AllowCut, RecordName )
                                    // 
                                    //    ContentName The content for this link
                                    //    RecordID    The ID of the record in the Table
                                    //    AllowCut
                                    //    RecordName
                                    //    IsEditing
                                    // ========================================================================
                                    // 
                                    ((string)(main_GetRecordEditLink2(((string)(ContentName)), ((int)(RecordID)), ((bool)(AllowCut)), ((string)(RecordName)), ((bool)(IsEditing)))));
                                    // TODO: On Error GoTo Warning!!!: The statement is not translatable 
                                    // 'Dim th as integer : th = profileLogMethodEnter("GetRecordEditLink2")
                                    // 
                                    // If Not (true) Then Exit Function
                                    // 
                                    int CS;
                                    string SQL;
                                    int ContentID;
                                    string Link;
                                    string MethodName;
                                    string iContentName;
                                    int iRecordID;
                                    string RootEntryName;
                                    string ClipBoard;
                                    string WorkingLink;
                                    bool iAllowCut;
                                    string Icon;
                                    string ContentCaption;
                                    // 
                                    iContentName = genericController.encodeText(ContentName);
                                    iRecordID = genericController.EncodeInteger(RecordID);
                                    iAllowCut = genericController.EncodeBoolean(AllowCut);
                                    ContentCaption = genericController.encodeHTML(iContentName);
                                    if ((genericController.vbLCase(ContentCaption) == "aggregate functions")) {
                                        ContentCaption = "Add-on";
                                    }
                                    
                                    if ((genericController.vbLCase(ContentCaption) == "aggregate function objects")) {
                                        ContentCaption = "Add-on";
                                    }
                                    
                                    ContentCaption = (ContentCaption + " record");
                                    if ((RecordName != "")) {
                                        ContentCaption = (ContentCaption + (", named \'" 
                                                    + (RecordName + "\'")));
                                    }
                                    
                                    // 
                                    MethodName = "main_GetRecordEditLink2";
                                    main_GetRecordEditLink2 = "";
                                    if ((iContentName == "")) {
                                        throw new ApplicationException(("ContentName [" 
                                                        + (ContentName + "] is invalid")));
                                    }
                                    else if ((iRecordID < 1)) {
                                        throw new ApplicationException(("RecordID [" 
                                                        + (RecordID + "] is invalid")));
                                    }
                                    else if (IsEditing) {
                                        // 
                                        //  Edit link, main_Get the CID
                                        // 
                                        ContentID = Models.Complex.cdefModel.getContentId(cpCore, iContentName);
                                        // 
                                        main_GetRecordEditLink2 = (main_GetRecordEditLink2 + ("<a" + (" class=\"ccRecordEditLink\" " + (" TabIndex=-1" + (" href=\"" 
                                                    + (genericController.encodeHTML(("/" 
                                                        + (cpCore.serverConfig.appConfig.adminRoute + ("?cid=" 
                                                        + (ContentID + ("&id=" 
                                                        + (iRecordID + "&af=4&aa=2&ad=1"))))))) + "\""))))));
                                        main_GetRecordEditLink2 = (main_GetRecordEditLink2 + ("><img" + (" src=\"/ccLib/images/IconContentEdit.gif\"" + (" border=\"0\"" + (" alt=\"Edit this " 
                                                    + (genericController.encodeHTML(ContentCaption) + ("\"" + (" title=\"Edit this " 
                                                    + (genericController.encodeHTML(ContentCaption) + ("\"" + (" align=\"absmiddle\"" + "></a>")))))))))));
                                        if (iAllowCut) {
                                            WorkingLink = genericController.modifyLinkQuery((cpCore.webServer.requestPage + ("?" + cpCore.doc.refreshQueryString)), RequestNameCut, (genericController.encodeText(ContentID) + ("." + genericController.encodeText(RecordID))), true);
                                            main_GetRecordEditLink2 = ("" 
                                                        + (main_GetRecordEditLink2 + ("<a class=\"ccRecordCutLink\" TabIndex=\"-1\" href=\"" 
                                                        + (genericController.encodeHTML(WorkingLink) + ("\"><img src=\"/ccLib/images/Contentcut.gif\" border=\"0\" alt=\"Cut this " 
                                                        + (ContentCaption + (" to clipboard\" title=\"Cut this " 
                                                        + (ContentCaption + " to clipboard\" align=\"absmiddle\"></a>"))))))));
                                        }
                                        
                                        // 
                                        //  Help link if enabled
                                        // 
                                        string helpLink;
                                        helpLink = "";
                                        main_GetRecordEditLink2 = ("" 
                                                    + (main_GetRecordEditLink2 + helpLink));
                                        // 
                                        main_GetRecordEditLink2 = ("<span class=\"ccRecordLinkCon\" style=\"white-space:nowrap;\">" 
                                                    + (main_GetRecordEditLink2 + "</span>"));
                                    }
                                    
                                    // 
                                    // TODO: Exit Function: Warning!!! Need to return the value
                                    return;
                                    // 
                                    //  ----- Error Trap
                                    // 
                                ErrorTrap:
                                    throw new ApplicationException("Unexpected exception");
                                    //  todo - remove this - handleLegacyError18(MethodName)
                                    // 
                                    // 
                                    // ========================================================================
                                    //  Print an add link for the current ContentSet
                                    //    iCSPointer is the content set to be added to
                                    //    PresetNameValueList is a name=value pair to force in the added record
                                    // ========================================================================
                                    // 
                                    ((string)(main_cs_getRecordAddLink(((int)(CSPointer)), Optional, PresetNameValueListAsString=, Optional, AllowPasteAsBoolean=False)));
                                    // TODO: On Error GoTo Warning!!!: The statement is not translatable 
                                    // 'Dim th as integer : th = profileLogMethodEnter("cs_getRecordAddLink")
                                    // 
                                    // If Not (true) Then Exit Function
                                    // 
                                    string ContentName;
                                    string iPresetNameValueList;
                                    string MethodName;
                                    int iCSPointer;
                                    // 
                                    iCSPointer = genericController.EncodeInteger(CSPointer);
                                    iPresetNameValueList = genericController.encodeEmptyText(PresetNameValueList, "");
                                    // 
                                    MethodName = "main_cs_getRecordAddLink";
                                    if ((iCSPointer < 0)) {
                                        throw new ApplicationException(("invalid ContentSet pointer [" 
                                                        + (iCSPointer + "]")));
                                    }
                                    else {
                                        // 
                                        //  Print an add tag to the iCSPointers Content
                                        // 
                                        ContentName = cpCore.db.cs_getContentName(iCSPointer);
                                        if ((ContentName == "")) {
                                            throw new ApplicationException("main_cs_getRecordAddLink was called with a ContentSet that was created with an SQL statement. The fun" +
                                                "ction requires a ContentSet opened with an OpenCSContent.");
                                        }
                                        else {
                                            main_cs_getRecordAddLink = main_GetRecordAddLink(ContentName, iPresetNameValueList, AllowPaste);
                                        }
                                        
                                    }
                                    
                                    // TODO: Exit Function: Warning!!! Need to return the value
                                    return;
                                    // 
                                    //  ----- Error Trap
                                    // 
                                ErrorTrap:
                                    throw new ApplicationException("Unexpected exception");
                                    //  todo - remove this - handleLegacyError18(MethodName)
                                    // 
                                    // 
                                    // ========================================================================
                                    //  main_GetRecordAddLink( iContentName, iPresetNameValueList )
                                    // 
                                    //    Returns a string of add tags for the Content Definition included, and all
                                    //    child contents of that area.
                                    // 
                                    //    iContentName The content for this link
                                    //    iPresetNameValueList The sql equivalent used to select the record.
                                    //            translates to name0=value0,name1=value1.. pairs separated by ,
                                    // 
                                    //    LowestRootMenu - The Menu in the flyout structure that is the furthest down
                                    //    in the chain that the user has content access to. This is so a content manager
                                    //    does not have to navigate deep into a structure to main_Get to content he can
                                    //    edit.
                                    //    Basically, the entire menu is created down from the MenuName, and populated
                                    //    with all the entiries this user has access to. The LowestRequiredMenuName is
                                    //    is returned from the _branch routine, and that is to root on-which the
                                    //    main_GetMenu uses
                                    // ========================================================================
                                    // 
                                    ((string)(main_GetRecordAddLink(((string)(ContentName)), ((string)(PresetNameValueList)), Optional, AllowPasteAsBoolean=False)));
                                    main_GetRecordAddLink = main_GetRecordAddLink2(genericController.encodeText(ContentName), genericController.encodeText(PresetNameValueList), AllowPaste, cpCore.doc.authContext.isEditing(ContentName));
                                    // 
                                    // ========================================================================
                                    //  main_GetRecordAddLink2
                                    // 
                                    //    Returns a string of add tags for the Content Definition included, and all
                                    //    child contents of that area.
                                    // 
                                    //    iContentName The content for this link
                                    //    iPresetNameValueList The sql equivalent used to select the record.
                                    //            translates to name0=value0,name1=value1.. pairs separated by ,
                                    // 
                                    //    LowestRootMenu - The Menu in the flyout structure that is the furthest down
                                    //    in the chain that the user has content access to. This is so a content manager
                                    //    does not have to navigate deep into a structure to main_Get to content he can
                                    //    edit.
                                    //    Basically, the entire menu is created down from the MenuName, and populated
                                    //    with all the entiries this user has access to. The LowestRequiredMenuName is
                                    //    is returned from the _branch routine, and that is to root on-which the
                                    //    main_GetMenu uses
                                    // ========================================================================
                                    // 
                                    ((string)(main_GetRecordAddLink2(((string)(ContentName)), ((string)(PresetNameValueList)), ((bool)(AllowPaste)), ((bool)(IsEditing)))));
                                    // TODO: On Error GoTo Warning!!!: The statement is not translatable 
                                    // 'Dim th as integer : th = profileLogMethodEnter("GetRecordAddLink")
                                    // 
                                    // If Not (true) Then Exit Function
                                    // 
                                    int ParentID;
                                    string BufferString;
                                    string MethodName;
                                    string iContentName;
                                    int iContentID;
                                    string iPresetNameValueList;
                                    string MenuName;
                                    bool MenuHasBranches;
                                    string LowestRequiredMenuName = String.Empty;
                                    string ClipBoard;
                                    string PasteLink = String.Empty;
                                    int Position;
                                    string[] ClipBoardArray;
                                    int ClipboardContentID;
                                    int ClipChildRecordID;
                                    bool iAllowPaste;
                                    bool useFlyout;
                                    int csChildContent;
                                    string Link;
                                    // 
                                    MethodName = "main_GetRecordAddLink";
                                    main_GetRecordAddLink2 = "";
                                    if (IsEditing) {
                                        iContentName = genericController.encodeText(ContentName);
                                        iPresetNameValueList = genericController.encodeText(PresetNameValueList);
                                        iPresetNameValueList = genericController.vbReplace(iPresetNameValueList, "&", ",");
                                        iAllowPaste = genericController.EncodeBoolean(AllowPaste);
                                        if ((iContentName == "")) {
                                            throw new ApplicationException("Method called with blank ContentName");
                                        }
                                        else {
                                            iContentID = Models.Complex.cdefModel.getContentId(cpCore, iContentName);
                                            csChildContent = cpCore.db.csOpen("Content", ("ParentID=" + iContentID), ,, ,, ,, "id");
                                            useFlyout = cpCore.db.csOk(csChildContent);
                                            cpCore.db.csClose(csChildContent);
                                            // 
                                            if (!useFlyout) {
                                                Link = ("/" 
                                                            + (cpCore.serverConfig.appConfig.adminRoute + ("?cid=" 
                                                            + (iContentID + "&af=4&aa=2&ad=1"))));
                                                if ((PresetNameValueList != "")) {
                                                    Link = (Link + ("&wc=" + genericController.EncodeRequestVariable(PresetNameValueList)));
                                                }
                                                
                                                main_GetRecordAddLink2 = (main_GetRecordAddLink2 + ("<a" + (" TabIndex=-1" + (" href=\"" 
                                                            + (genericController.encodeHTML(Link) + "\"")))));
                                                main_GetRecordAddLink2 = (main_GetRecordAddLink2 + ("><img" + (" src=\"/ccLib/images/IconContentAdd.gif\"" + (" border=\"0\"" + (" alt=\"Add record\"" + (" title=\"Add record\"" + (" align=\"absmiddle\"" + "></a>")))))));
                                            }
                                            else {
                                                // 
                                                MenuName = genericController.GetRandomInteger().ToString;
                                                cpCore.menuFlyout.menu_AddEntry(MenuName, ,, "/ccLib/images/IconContentAdd.gif", ,, ,, "stylesheet", "stylesheethover");
                                                LowestRequiredMenuName = main_GetRecordAddLink_AddMenuEntry(iContentName, iPresetNameValueList, "", MenuName, MenuName);
                                            }
                                            
                                            // 
                                            //  Add in the paste entry, if needed
                                            // 
                                            if (iAllowPaste) {
                                                ClipBoard = cpCore.visitProperty.getText("Clipboard", "");
                                                if ((ClipBoard != "")) {
                                                    Position = genericController.vbInstr(1, ClipBoard, ".");
                                                    if ((Position != 0)) {
                                                        ClipBoardArray = ClipBoard.Split(".");
                                                        if ((UBound(ClipBoardArray) > 0)) {
                                                            ClipboardContentID = genericController.EncodeInteger(ClipBoardArray[0]);
                                                            ClipChildRecordID = genericController.EncodeInteger(ClipBoardArray[1]);
                                                            // iContentID = main_GetContentID(iContentName)
                                                            if (Models.Complex.cdefModel.isWithinContent(cpCore, ClipboardContentID, iContentID)) {
                                                                if ((genericController.vbInstr(1, iPresetNameValueList, "PARENTID=", vbTextCompare) != 0)) {
                                                                    // 
                                                                    //  must test for main_IsChildRecord
                                                                    // 
                                                                    BufferString = iPresetNameValueList;
                                                                    BufferString = genericController.vbReplace(BufferString, "(", "");
                                                                    BufferString = genericController.vbReplace(BufferString, ")", "");
                                                                    BufferString = genericController.vbReplace(BufferString, ",", "&");
                                                                    ParentID = genericController.EncodeInteger(genericController.main_GetNameValue_Internal(cpCore, BufferString, "Parentid"));
                                                                }
                                                                
                                                                if (((ParentID != 0) 
                                                                            && !pageContentController.isChildRecord(cpCore, iContentName, ParentID, ClipChildRecordID))) {
                                                                    // 
                                                                    //  Can not paste as child of itself
                                                                    // 
                                                                    PasteLink = (cpCore.webServer.requestPage + ("?" + cpCore.doc.refreshQueryString));
                                                                    PasteLink = genericController.modifyLinkQuery(PasteLink, RequestNamePaste, "1", true);
                                                                    PasteLink = genericController.modifyLinkQuery(PasteLink, RequestNamePasteParentContentID, iContentID.ToString(), true);
                                                                    PasteLink = genericController.modifyLinkQuery(PasteLink, RequestNamePasteParentRecordID, ParentID.ToString(), true);
                                                                    PasteLink = genericController.modifyLinkQuery(PasteLink, RequestNamePasteFieldList, iPresetNameValueList, true);
                                                                    main_GetRecordAddLink2 = (main_GetRecordAddLink2 + ("<a class=\"ccRecordCutLink\" TabIndex=\"-1\" href=\"" 
                                                                                + (genericController.encodeHTML(PasteLink) + "\"><img src=\"/ccLib/images/ContentPaste.gif\" border=\"0\" alt=\"Paste record in clipboard here\" title=\"Pa" +
                                                                                "ste record in clipboard here\" align=\"absmiddle\"></a>")));
                                                                }
                                                                
                                                            }
                                                            
                                                        }
                                                        
                                                    }
                                                    
                                                }
                                                
                                            }
                                            
                                            // 
                                            //  Add in the available flyout Navigator Entries
                                            // 
                                            if ((LowestRequiredMenuName != "")) {
                                                main_GetRecordAddLink2 = (main_GetRecordAddLink2 + cpCore.menuFlyout.getMenu(LowestRequiredMenuName, 0));
                                                main_GetRecordAddLink2 = genericController.vbReplace(main_GetRecordAddLink2, "class=\"ccFlyoutButton\" ", "", 1, 99, vbTextCompare);
                                                if ((PasteLink != "")) {
                                                    main_GetRecordAddLink2 = (main_GetRecordAddLink2 + ("<a TabIndex=-1 href=\"" 
                                                                + (genericController.encodeHTML(PasteLink) + "\"><img src=\"/ccLib/images/ContentPaste.gif\" border=\"0\" alt=\"Paste content from clipboard\" align=\"absm" +
                                                                "iddle\"></a>")));
                                                }
                                                
                                            }
                                            
                                            // 
                                            //  Help link if enabled
                                            // 
                                            string helpLink;
                                            helpLink = "";
                                            main_GetRecordAddLink2 = (main_GetRecordAddLink2 + helpLink);
                                            // 
                                            if ((main_GetRecordAddLink2 != "")) {
                                                main_GetRecordAddLink2 = ("" + ("\r\n" + ('\t' + ("<div style=\"display:inline;\">" 
                                                            + (genericController.htmlIndent(main_GetRecordAddLink2) + ("\r\n" + ('\t' + "</div>")))))));
                                            }
                                            
                                            // 
                                            //  ----- Add the flyout panels to the content to return
                                            //        This must be here so if the call is made after main_ClosePage, the panels will still deliver
                                            // 
                                            if ((LowestRequiredMenuName != "")) {
                                                main_GetRecordAddLink2 = (main_GetRecordAddLink2 + cpCore.menuFlyout.menu_GetClose());
                                                if ((genericController.vbInstr(1, main_GetRecordAddLink2, "IconContentAdd.gif", vbTextCompare) != 0)) {
                                                    main_GetRecordAddLink2 = genericController.vbReplace(main_GetRecordAddLink2, "IconContentAdd.gif\" ", "IconContentAdd.gif\" align=\"absmiddle\" ");
                                                }
                                                
                                            }
                                            
                                            main_GetRecordAddLink2 = genericController.vbReplace(main_GetRecordAddLink2, "target=", "xtarget=", 1, 99, vbTextCompare);
                                        }
                                        
                                    }
                                    
                                    // 
                                    // TODO: Exit Function: Warning!!! Need to return the value
                                    return;
                                    // 
                                    //  ----- Error Trap
                                    // 
                                ErrorTrap:
                                    throw new ApplicationException("Unexpected exception");
                                    //  todo - remove this - handleLegacyError18(MethodName)
                                    // 
                                    // 
                                    // ========================================================================
                                    //  main_GetRecordAddLink_AddMenuEntry( ContentName, PresetNameValueList, ContentNameList, MenuName )
                                    // 
                                    //    adds an add entry for the content name, and all the child content
                                    //    returns the MenuName of the lowest branch that has valid
                                    //    Navigator Entries.
                                    // 
                                    //    ContentName The content for this link
                                    //    PresetNameValueList The sql equivalent used to select the record.
                                    //            translates to (name0=value0)&(name1=value1).. pairs separated by &
                                    //    ContentNameList is a comma separated list of names of the content included so far
                                    //    MenuName is the name of the root branch, for flyout menu
                                    // 
                                    //    IsMember(), main_IsAuthenticated() And Member_AllowLinkAuthoring must already be checked
                                    // ========================================================================
                                    // 
                                    ((string)(main_GetRecordAddLink_AddMenuEntry(((string)(ContentName)), ((string)(PresetNameValueList)), ((string)(ContentNameList)), ((string)(MenuName)), ((string)(ParentMenuName)))));
                                    string result = "";
                                    string Copy;
                                    int CS;
                                    string SQL;
                                    int csChildContent;
                                    int ContentID;
                                    string Link;
                                    string MyContentNameList;
                                    string ButtonCaption;
                                    bool ContentRecordFound;
                                    bool ContentAllowAdd;
                                    bool GroupRulesAllowAdd;
                                    DateTime MemberRulesDateExpires;
                                    bool MemberRulesAllow;
                                    int ChildMenuButtonCount;
                                    string ChildMenuName;
                                    string ChildContentName;
                                    // 
                                    Link = "";
                                    MyContentNameList = ContentNameList;
                                    if ((ContentName == "")) {
                                        throw new ApplicationException("main_GetRecordAddLink, ContentName is empty");
                                    }
                                    else if (((MyContentNameList.IndexOf(("," 
                                                    + (genericController.vbUCase(ContentName) + ",")), 0) + 1) 
                                                >= 0)) {
                                        throw new ApplicationException(("result , Content Child [" 
                                                        + (ContentName + "] is one of its own parents")));
                                    }
                                    else {
                                        MyContentNameList = (MyContentNameList + ("," 
                                                    + (genericController.vbUCase(ContentName) + ",")));
                                        ContentRecordFound = false;
                                        if (cpCore.doc.authContext.isAuthenticatedAdmin(cpCore)) {
                                            // 
                                            //  ----- admin member, they have access, main_Get ContentID and set markers true
                                            // 
                                            SQL = ("SELECT ID as ContentID, AllowAdd as ContentAllowAdd, 1 as GroupRulesAllowAdd, null as MemberRulesDate" +
                                            "Expires" + (" FROM ccContent" + (" WHERE (" + (" (ccContent.Name=" 
                                                        + (cpCore.db.encodeSQLText(ContentName) + (")" + (" AND(ccContent.active<>0)" + " );")))))));
                                            CS = cpCore.db.csOpenSql(SQL);
                                            if (cpCore.db.csOk(CS)) {
                                                // 
                                                //  Entry was found
                                                // 
                                                ContentRecordFound = true;
                                                ContentID = cpCore.db.csGetInteger(CS, "ContentID");
                                                ContentAllowAdd = cpCore.db.csGetBoolean(CS, "ContentAllowAdd");
                                                GroupRulesAllowAdd = true;
                                                MinValue;
                                                MemberRulesAllow = true;
                                            }
                                            
                                            cpCore.db.csClose(CS);
                                        }
                                        else {
                                            // 
                                            //  non-admin member, first check if they have access and main_Get true markers
                                            // 
                                            SQL = ("SELECT ccContent.ID as ContentID, ccContent.AllowAdd as ContentAllowAdd, ccGroupRules.AllowAdd as Gro" +
                                            "upRulesAllowAdd, ccMemberRules.DateExpires as MemberRulesDateExpires" + (" FROM (((ccContent" + (" LEFT JOIN ccGroupRules ON ccGroupRules.ContentID=ccContent.ID)" + (" LEFT JOIN ccgroups ON ccGroupRules.GroupID=ccgroups.ID)" + (" LEFT JOIN ccMemberRules ON ccgroups.ID=ccMemberRules.GroupID)" + (" LEFT JOIN ccMembers ON ccMemberRules.MemberID=ccMembers.ID" + (" WHERE (" + (" (ccContent.Name=" 
                                                        + (cpCore.db.encodeSQLText(ContentName) + (")" + (" AND(ccContent.active<>0)" + (" AND(ccGroupRules.active<>0)" + (" AND(ccMemberRules.active<>0)" + (" AND((ccMemberRules.DateExpires is Null)or(ccMemberRules.DateExpires>" 
                                                        + (cpCore.db.encodeSQLDate(cpCore.doc.profileStartTime) + ("))" + (" AND(ccgroups.active<>0)" + (" AND(ccMembers.active<>0)" + (" AND(ccMembers.ID=" 
                                                        + (cpCore.doc.authContext.user.id + (")" + " );")))))))))))))))))))));
                                            CS = cpCore.db.csOpenSql(SQL);
                                            if (cpCore.db.csOk(CS)) {
                                                // 
                                                //  ----- Entry was found, member has some kind of access
                                                // 
                                                ContentRecordFound = true;
                                                ContentID = cpCore.db.csGetInteger(CS, "ContentID");
                                                ContentAllowAdd = cpCore.db.csGetBoolean(CS, "ContentAllowAdd");
                                                GroupRulesAllowAdd = cpCore.db.csGetBoolean(CS, "GroupRulesAllowAdd");
                                                MemberRulesDateExpires = cpCore.db.csGetDate(CS, "MemberRulesDateExpires");
                                                MemberRulesAllow = false;
                                                MinValue;
                                                MemberRulesAllow = true;
                                            }
                                            else if ((MemberRulesDateExpires > cpCore.doc.profileStartTime)) {
                                                MemberRulesAllow = true;
                                            }
                                            
                                        }
                                        
                                    }
                                    
                                    // 
                                    //  ----- No entry found, this member does not have access, just main_Get ContentID
                                    // 
                                    ContentRecordFound = true;
                                    ContentID = Models.Complex.cdefModel.getContentId(cpCore, ContentName);
                                    ContentAllowAdd = false;
                                    GroupRulesAllowAdd = false;
                                    MemberRulesAllow = false;
                                    cpCore.db.csClose(CS);
                                    ContentRecordFound;
                                    // 
                                    //  Add the Menu Entry* to the current menu (MenuName)
                                    // 
                                    Link = "";
                                    ButtonCaption = ContentName;
                                    result = MenuName;
                                    if ((ContentAllowAdd 
                                                && (GroupRulesAllowAdd && MemberRulesAllow))) {
                                        Link = ("/" 
                                                    + (cpCore.serverConfig.appConfig.adminRoute + ("?cid=" 
                                                    + (ContentID + "&af=4&aa=2&ad=1"))));
                                        if ((PresetNameValueList != "")) {
                                            string NameValueList;
                                            NameValueList = PresetNameValueList;
                                            Link = (Link + ("&wc=" + genericController.EncodeRequestVariable(PresetNameValueList)));
                                        }
                                        
                                    }
                                    
                                    cpCore.menuFlyout.menu_AddEntry((MenuName + (":" + ContentName)), ParentMenuName, ,, Link, ButtonCaption, "", "", true);
                                    // 
                                    //  Create child submenu if Child Entries found
                                    // 
                                    csChildContent = cpCore.db.csOpen("Content", ("ParentID=" + ContentID), ,, ,, ,, "name");
                                    if (!cpCore.db.csOk(csChildContent)) {
                                        // 
                                        //  No child menu
                                        // 
                                    }
                                    else {
                                        // 
                                        //  Add the child menu
                                        // 
                                        ChildMenuName = (MenuName + (":" + ContentName));
                                        ChildMenuButtonCount = 0;
                                        // 
                                        //  ----- Create the ChildPanel with all Children found
                                        // 
                                        while (cpCore.db.csOk(csChildContent)) {
                                            ChildContentName = cpCore.db.csGetText(csChildContent, "name");
                                            Copy = main_GetRecordAddLink_AddMenuEntry(ChildContentName, PresetNameValueList, MyContentNameList, MenuName, ParentMenuName);
                                            if ((Copy != "")) {
                                                ChildMenuButtonCount = (ChildMenuButtonCount + 1);
                                            }
                                            
                                            if (((result == "") 
                                                        && (Copy != ""))) {
                                                result = Copy;
                                            }
                                            
                                            cpCore.db.csGoNext(csChildContent);
                                        }
                                        
                                    }
                                    
                                    cpCore.db.csClose(csChildContent);
                                    if (result) {
                                        
                                    }
                                    
                                    // 
                                    // ========================================================================
                                    //    main_GetPanel( Panel, Optional StylePanel, Optional StyleHilite, Optional StyleShadow, Optional Width, Optional Padding, Optional HeightMin) As String
                                    //  Return a panel with the input as center
                                    // ========================================================================
                                    // 
                                    ((string)(main_GetPanel(((string)(Panel)), Optional, StylePanelAsString=, Optional, StyleHiliteAsString=ccPanelHilite, Optional, StyleShadowAsString=ccPanelShadow, Optional, WidthAsString=100%, Optional, PaddingAsInteger=5, Optional, HeightMinAsInteger=1)));
                                    string ContentPanelWidth;
                                    string MethodName;
                                    string MyStylePanel;
                                    string MyStyleHilite;
                                    string MyStyleShadow;
                                    string MyWidth;
                                    string MyPadding;
                                    string MyHeightMin;
                                    string s0;
                                    string s1;
                                    string s2;
                                    string s3;
                                    string s4;
                                    string contentPanelWidthStyle;
                                    // 
                                    MethodName = "main_GetPanelTop";
                                    MyStylePanel = genericController.encodeEmptyText(StylePanel, "ccPanel");
                                    MyStyleHilite = genericController.encodeEmptyText(StyleHilite, "ccPanelHilite");
                                    MyStyleShadow = genericController.encodeEmptyText(StyleShadow, "ccPanelShadow");
                                    MyWidth = genericController.encodeEmptyText(Width, "100%");
                                    MyPadding = Padding.ToString;
                                    MyHeightMin = HeightMin.ToString;
                                    // 
                                    if (genericController.vbIsNumeric(MyWidth)) {
                                        ContentPanelWidth = (int.Parse(MyWidth) - 2).ToString;
                                        contentPanelWidthStyle = (ContentPanelWidth + "px");
                                    }
                                    else {
                                        ContentPanelWidth = "100%";
                                        contentPanelWidthStyle = ContentPanelWidth;
                                    }
                                    
                                    // 
                                    // 
                                    // 
                                    s0 = ("" 
                                                + (cr + ("<td style=\"padding:" 
                                                + (MyPadding + ("px;vertical-align:top\" class=\"" 
                                                + (MyStylePanel + ("\">" 
                                                + (genericController.htmlIndent(genericController.encodeText(Panel)) 
                                                + (cr + ("</td>" + ""))))))))));
                                    s1 = ("" 
                                                + (cr + ("<tr>" 
                                                + (genericController.htmlIndent(s0) 
                                                + (cr + ("</tr>" + ""))))));
                                    s2 = ("" 
                                                + (cr + ("<table style=\"width:" 
                                                + (contentPanelWidthStyle + (";border:0px;\" class=\"" 
                                                + (MyStylePanel + ("\" cellspacing=\"0\">" 
                                                + (genericController.htmlIndent(s1) 
                                                + (cr + ("</table>" + ""))))))))));
                                    s3 = ("" 
                                                + (cr + ("<td width=\"1\" height=\"" 
                                                + (MyHeightMin + ("\" class=\"" 
                                                + (MyStyleHilite + ("\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" height=\"" 
                                                + (MyHeightMin + ("\" width=\"1\" ></td>" 
                                                + (cr + ("<td width=\"" 
                                                + (ContentPanelWidth + ("\" valign=\"top\" align=\"left\" class=\"" 
                                                + (MyStylePanel + ("\">" 
                                                + (genericController.htmlIndent(s2) 
                                                + (cr + ("</td>" 
                                                + (cr + ("<td width=\"1\" class=\"" 
                                                + (MyStyleShadow + ("\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" height=\"1\" width=\"1\" ></td>" + ""))))))))))))))))))))));
                                    s4 = ("" 
                                                + (cr + ("<tr>" 
                                                + (cr2 + ("<td colspan=\"3\" class=\"" 
                                                + (MyStyleHilite + ("\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" height=\"1\" width=\"" 
                                                + (MyWidth + ("\" ></td>" 
                                                + (cr + ("</tr>" 
                                                + (cr + ("<tr>" 
                                                + (genericController.htmlIndent(s3) 
                                                + (cr + ("</tr>" 
                                                + (cr + ("<tr>" 
                                                + (cr2 + ("<td colspan=\"3\" class=\"" 
                                                + (MyStyleShadow + ("\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" height=\"1\" width=\"" 
                                                + (MyWidth + ("\" ></td>" 
                                                + (cr + ("</tr>" + ""))))))))))))))))))))))))));
                                    main_GetPanel = ("" 
                                                + (cr + ("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"" 
                                                + (MyWidth + ("\" class=\"" 
                                                + (MyStylePanel + ("\">" 
                                                + (genericController.htmlIndent(s4) 
                                                + (cr + ("</table>" + ""))))))))));
                                    // 
                                    // ========================================================================
                                    //    main_GetPanel( Panel, Optional StylePanel, Optional StyleHilite, Optional StyleShadow, Optional Width, Optional Padding, Optional HeightMin) As String
                                    //  Return a panel with the input as center
                                    // ========================================================================
                                    // 
                                    ((string)(main_GetReversePanel(((string)(Panel)), Optional, StylePanelAsString=, Optional, StyleHiliteAsString=ccPanelShadow, Optional, StyleShadowAsString=ccPanelHilite, Optional, WidthAsString=, Optional, PaddingAsString=, Optional, HeightMinAsString=)));
                                    string MyStyleHilite;
                                    string MyStyleShadow;
                                    // 
                                    MyStyleHilite = genericController.encodeEmptyText(StyleHilite, "ccPanelShadow");
                                    MyStyleShadow = genericController.encodeEmptyText(StyleShadow, "ccPanelHilite");
                                    main_GetReversePanel = (main_GetPanelTop(StylePanel, MyStyleHilite, MyStyleShadow, Width, Padding, HeightMin) 
                                                + (genericController.encodeText(Panel) + main_GetPanelBottom(StylePanel, MyStyleHilite, MyStyleShadow, Width, Padding)));
                                    // 
                                    // ========================================================================
                                    //  Return a panel header with the header message reversed out of the left
                                    // ========================================================================
                                    // 
                                    ((string)(main_GetPanelHeader(((string)(HeaderMessage)), Optional, RightSideMessageAsString=)));
                                    string iHeaderMessage;
                                    string iRightSideMessage;
                                    adminUIController Adminui = new adminUIController(cpCore);
                                    // 
                                    // If Not (true) Then Exit Function
                                    // 
                                    iHeaderMessage = genericController.encodeText(HeaderMessage);
                                    iRightSideMessage = genericController.encodeEmptyText(RightSideMessage, FormatDateTime(cpCore.doc.profileStartTime));
                                    main_GetPanelHeader = Adminui.GetHeader(iHeaderMessage, iRightSideMessage);
                                    // 
                                    // ========================================================================
                                    //  Prints the top of display panel
                                    //    Must be closed with PrintPanelBottom
                                    // ========================================================================
                                    // 
                                    ((string)(main_GetPanelTop(Optional, StylePanelAsString=, Optional, StyleHiliteAsString=, Optional, StyleShadowAsString=, Optional, WidthAsString=, Optional, PaddingAsString=, Optional, HeightMinAsString=)));
                                    string ContentPanelWidth;
                                    string MethodName;
                                    string MyStylePanel;
                                    string MyStyleHilite;
                                    string MyStyleShadow;
                                    string MyWidth;
                                    string MyPadding;
                                    string MyHeightMin;
                                    // 
                                    main_GetPanelTop = "";
                                    MyStylePanel = genericController.encodeEmptyText(StylePanel, "ccPanel");
                                    MyStyleHilite = genericController.encodeEmptyText(StyleHilite, "ccPanelHilite");
                                    MyStyleShadow = genericController.encodeEmptyText(StyleShadow, "ccPanelShadow");
                                    MyWidth = genericController.encodeEmptyText(Width, "100%");
                                    MyPadding = genericController.encodeEmptyText(Padding, "5");
                                    MyHeightMin = genericController.encodeEmptyText(HeightMin, "1");
                                    MethodName = "main_GetPanelTop";
                                    if (genericController.vbIsNumeric(MyWidth)) {
                                        ContentPanelWidth = (int.Parse(MyWidth) - 2).ToString;
                                    }
                                    else {
                                        ContentPanelWidth = "100%";
                                    }
                                    
                                    main_GetPanelTop = (main_GetPanelTop 
                                                + (cr + ("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"" 
                                                + (MyWidth + ("\" class=\"" 
                                                + (MyStylePanel + "\">"))))));
                                    main_GetPanelTop = (main_GetPanelTop 
                                                + (cr2 + ("<tr>" 
                                                + (cr3 + ("<td colspan=\"3\" class=\"" 
                                                + (MyStyleHilite + ("\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" height=\"1\" width=\"" 
                                                + (MyWidth + ("\" ></td>" 
                                                + (cr2 + "</tr>"))))))))));
                                    main_GetPanelTop = (main_GetPanelTop 
                                                + (cr2 + ("<tr>" 
                                                + (cr3 + ("<td width=\"1\" height=\"" 
                                                + (MyHeightMin + ("\" class=\"" 
                                                + (MyStyleHilite + ("\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" height=\"" 
                                                + (MyHeightMin + ("\" width=\"1\" ></td>" 
                                                + (cr3 + ("<td width=\"" 
                                                + (ContentPanelWidth + ("\" valign=\"top\" align=\"left\" class=\"" 
                                                + (MyStylePanel + ("\">" 
                                                + (cr4 + ("<table border=\"0\" cellpadding=\"" 
                                                + (MyPadding + ("\" cellspacing=\"0\" width=\"" 
                                                + (ContentPanelWidth + ("\" class=\"" 
                                                + (MyStylePanel + ("\">" 
                                                + (cr5 + ("<tr>" 
                                                + (cr6 + ("<td valign=\"top\" class=\"" 
                                                + (MyStylePanel + ("\"><Span class=\"" 
                                                + (MyStylePanel + "\">"))))))))))))))))))))))))))))))));
                                    // 
                                    // ========================================================================
                                    //  Return a panel with the input as center
                                    // ========================================================================
                                    // 
                                    ((string)(main_GetPanelBottom(Optional, StylePanelAsString=, Optional, StyleHiliteAsString=, Optional, StyleShadowAsString=, Optional, WidthAsString=, Optional, PaddingAsString=)));
                                    string result = String.Empty;
                                    try {
                                        // Dim MyStylePanel As String
                                        // Dim MyStyleHilite As String
                                        string MyStyleShadow;
                                        string MyWidth;
                                        // Dim MyPadding As String
                                        // 
                                        // MyStylePanel = genericController.encodeEmptyText(StylePanel, "ccPanel")
                                        // MyStyleHilite = genericController.encodeEmptyText(StyleHilite, "ccPanelHilite")
                                        MyStyleShadow = genericController.encodeEmptyText(StyleShadow, "ccPanelShadow");
                                        MyWidth = genericController.encodeEmptyText(Width, "100%");
                                        // MyPadding = genericController.encodeEmptyText(Padding, "5")
                                        // 
                                        result = (result 
                                                    + (cr6 + ("</span></td>" 
                                                    + (cr5 + ("</tr>" 
                                                    + (cr4 + ("</table>" 
                                                    + (cr3 + ("</td>" 
                                                    + (cr3 + ("<td width=\"1\" class=\"" 
                                                    + (MyStyleShadow + ("\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" height=\"1\" width=\"1\" ></td>" 
                                                    + (cr2 + ("</tr>" 
                                                    + (cr2 + ("<tr>" 
                                                    + (cr3 + ("<td colspan=\"3\" class=\"" 
                                                    + (MyStyleShadow + ("\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" height=\"1\" width=\"" 
                                                    + (MyWidth + ("\" ></td>" 
                                                    + (cr2 + ("</tr>" 
                                                    + (cr + "</table>"))))))))))))))))))))))))));
                                    }
                                    catch (Exception ex) {
                                        cpCore.handleException(ex);
                                    }
                                    
                                    return result;
                                    // 
                                    // ========================================================================
                                    // 
                                    // ========================================================================
                                    // 
                                    ((string)(main_GetPanelButtons(((string)(ButtonValueList)), ((string)(ButtonName)), Optional, PanelWidthAsString=, Optional, PanelHeightMinAsString=)));
                                    adminUIController Adminui = new adminUIController(cpCore);
                                    main_GetPanelButtons = Adminui.GetButtonBar(Adminui.GetButtonsFromList(ButtonValueList, true, true, ButtonName), "");
                                    // 
                                    // 
                                    // 
                                    ((string)(main_GetPanelRev(((string)(PanelContent)), Optional, PanelWidthAsString=, Optional, PanelHeightMinAsString=)));
                                    main_GetPanelRev = main_GetPanel(PanelContent, "ccPanel", "ccPanelShadow", "ccPanelHilite", PanelWidth, 2, genericController.EncodeInteger(PanelHeightMin));
                                    // 
                                    // 
                                    // 
                                    ((string)(main_GetPanelInput(((string)(PanelContent)), Optional, PanelWidthAsString=, Optional, PanelHeightMinAsString=1)));
                                    main_GetPanelInput = main_GetPanel(PanelContent, "ccPanelInput", "ccPanelShadow", "ccPanelHilite", PanelWidth, 2, genericController.EncodeInteger(PanelHeightMin));
                                    // 
                                    // ========================================================================
                                    //  Print the tools panel at the bottom of the page
                                    // ========================================================================
                                    // 
                                    ((string)(main_GetToolsPanel()));
                                    string result = String.Empty;
                                    try {
                                        string copyNameValue;
                                        string CopyName;
                                        string copyValue;
                                        string[] copyNameValueSplit;
                                        int VisitMin;
                                        int VisitHrs;
                                        int VisitSec;
                                        string DebugPanel = String.Empty;
                                        string Copy;
                                        string[] CopySplit;
                                        int Ptr;
                                        string EditTagID;
                                        string QuickEditTagID;
                                        string AdvancedEditTagID;
                                        string WorkflowTagID;
                                        string Tag;
                                        string MethodName;
                                        string TagID;
                                        stringBuilderLegacyController ToolsPanel;
                                        string OptionsPanel = String.Empty;
                                        stringBuilderLegacyController LinkPanel;
                                        string LoginPanel = String.Empty;
                                        bool iValueBoolean;
                                        string WorkingQueryString;
                                        string BubbleCopy;
                                        stringBuilderLegacyController AnotherPanel;
                                        adminUIController Adminui = new adminUIController(cpCore);
                                        bool ShowLegacyToolsPanel;
                                        string QS;
                                        // 
                                        MethodName = "main_GetToolsPanel";
                                        if (cpCore.doc.authContext.user.AllowToolsPanel) {
                                            ShowLegacyToolsPanel = cpCore.siteProperties.getBoolean("AllowLegacyToolsPanel", true);
                                            // 
                                            //  --- Link Panel - used for both Legacy Tools Panel, and without it
                                            // 
                                            LinkPanel = new stringBuilderLegacyController();
                                            LinkPanel.Add(SpanClassAdminSmall);
                                            LinkPanel.Add(("Contensive " 
                                                            + (cpCore.codeVersion() + " | ")));
                                            LinkPanel.Add((FormatDateTime(cpCore.doc.profileStartTime) + " | "));
                                            LinkPanel.Add("<a class=\"ccAdminLink\" target=\"_blank\" href=\"http://support.Contensive.com/\">Support</A> | ");
                                            LinkPanel.Add(("<a class=\"ccAdminLink\" href=\"" 
                                                            + (genericController.encodeHTML(("/" + cpCore.serverConfig.appConfig.adminRoute)) + "\">Admin Home</A> | ")));
                                            LinkPanel.Add(("<a class=\"ccAdminLink\" href=\"" 
                                                            + (genericController.encodeHTML(("http://" + cpCore.webServer.requestDomain)) + "\">Public Home</A> | ")));
                                            LinkPanel.Add(("<a class=\"ccAdminLink\" target=\"_blank\" href=\"" 
                                                            + (genericController.encodeHTML(("/" 
                                                                + (cpCore.serverConfig.appConfig.adminRoute + ("?" 
                                                                + (RequestNameHardCodedPage + ("=" + HardCodedPageMyProfile)))))) + "\">My Profile</A> | ")));
                                            if (cpCore.siteProperties.getBoolean("AllowMobileTemplates", false)) {
                                                if (cpCore.doc.authContext.visit.Mobile) {
                                                    QS = cpCore.doc.refreshQueryString;
                                                    QS = genericController.ModifyQueryString(QS, "method", "forcenonmobile");
                                                    LinkPanel.Add(("<a class=\"ccAdminLink\" href=\"?" 
                                                                    + (QS + "\">Non-Mobile Version</A> | ")));
                                                }
                                                else {
                                                    QS = cpCore.doc.refreshQueryString;
                                                    QS = genericController.ModifyQueryString(QS, "method", "forcemobile");
                                                    LinkPanel.Add(("<a class=\"ccAdminLink\" href=\"?" 
                                                                    + (QS + "\">Mobile Version</A> | ")));
                                                }
                                                
                                            }
                                            
                                            LinkPanel.Add("</span>");
                                            // 
                                            if (ShowLegacyToolsPanel) {
                                                ToolsPanel = new stringBuilderLegacyController();
                                                WorkingQueryString = genericController.ModifyQueryString(cpCore.doc.refreshQueryString, "ma", "", false);
                                                // 
                                                //  ----- Tools Panel Caption
                                                // 
                                                string helpLink;
                                                helpLink = "";
                                                BubbleCopy = "Use the Tools Panel to enable features such as editing and debugging tools. It also includes links to" +
                                                " the admin site, the support site and the My Profile page.";
                                                result = (result + main_GetPanelHeader(("Contensive Tools Panel" + helpLink)));
                                                // 
                                                ToolsPanel.Add(cpCore.html.html_GetFormStart(WorkingQueryString));
                                                ToolsPanel.Add(cpCore.html.html_GetFormInputHidden("Type", FormTypeToolsPanel));
                                                // 
                                                if (true) {
                                                    // 
                                                    //  ----- Create the Options Panel
                                                    // 
                                                    // PathsContentID = main_GetContentID("Paths")
                                                    //                 '
                                                    //                 ' Allow Help Links
                                                    //                 '
                                                    //                 iValueBoolean = visitProperty.getboolean("AllowHelpIcon")
                                                    //                 TagID =  "AllowHelpIcon"
                                                    //                 OptionsPanel = OptionsPanel & "" _
                                                    //                     & CR & "<div class=""ccAdminSmall"">" _
                                                    //                     & cr2 & "<LABEL for=""" & TagID & """>" & main_GetFormInputCheckBox2(TagID, iValueBoolean, TagID) & " Help</LABEL>" _
                                                    //                     & CR & "</div>"
                                                    // 
                                                    EditTagID = "AllowEditing";
                                                    QuickEditTagID = "AllowQuickEditor";
                                                    AdvancedEditTagID = "AllowAdvancedEditor";
                                                    WorkflowTagID = "AllowWorkflowRendering";
                                                    helpLink = "";
                                                    iValueBoolean = cpCore.visitProperty.getBoolean("AllowEditing");
                                                    Tag = cpCore.html.html_GetFormInputCheckBox2(EditTagID, iValueBoolean, EditTagID);
                                                    Tag = genericController.vbReplace(Tag, ">", (" onClick=\"document.getElementById(\'" 
                                                                    + (QuickEditTagID + ("\').checked=false;document.getElementById(\'" 
                                                                    + (AdvancedEditTagID + "\').checked=false;\">")))));
                                                    OptionsPanel = (OptionsPanel 
                                                                + (cr + ("<div class=\"ccAdminSmall\">" 
                                                                + (cr2 + ("<LABEL for=\"" 
                                                                + (EditTagID + ("\">" 
                                                                + (Tag + (" Edit</LABEL>" 
                                                                + (helpLink 
                                                                + (cr + "</div>")))))))))));
                                                    helpLink = "";
                                                    iValueBoolean = cpCore.visitProperty.getBoolean("AllowQuickEditor");
                                                    Tag = cpCore.html.html_GetFormInputCheckBox2(QuickEditTagID, iValueBoolean, QuickEditTagID);
                                                    Tag = genericController.vbReplace(Tag, ">", (" onClick=\"document.getElementById(\'" 
                                                                    + (EditTagID + ("\').checked=false;document.getElementById(\'" 
                                                                    + (AdvancedEditTagID + "\').checked=false;\">")))));
                                                    OptionsPanel = (OptionsPanel 
                                                                + (cr + ("<div class=\"ccAdminSmall\">" 
                                                                + (cr2 + ("<LABEL for=\"" 
                                                                + (QuickEditTagID + ("\">" 
                                                                + (Tag + (" Quick Edit</LABEL>" 
                                                                + (helpLink 
                                                                + (cr + "</div>")))))))))));
                                                    helpLink = "";
                                                    iValueBoolean = cpCore.visitProperty.getBoolean("AllowAdvancedEditor");
                                                    Tag = cpCore.html.html_GetFormInputCheckBox2(AdvancedEditTagID, iValueBoolean, AdvancedEditTagID);
                                                    Tag = genericController.vbReplace(Tag, ">", (" onClick=\"document.getElementById(\'" 
                                                                    + (QuickEditTagID + ("\').checked=false;document.getElementById(\'" 
                                                                    + (EditTagID + "\').checked=false;\">")))));
                                                    OptionsPanel = (OptionsPanel 
                                                                + (cr + ("<div class=\"ccAdminSmall\">" 
                                                                + (cr2 + ("<LABEL for=\"" 
                                                                + (AdvancedEditTagID + ("\">" 
                                                                + (Tag + (" Advanced Edit</LABEL>" 
                                                                + (helpLink 
                                                                + (cr + "</div>")))))))))));
                                                    helpLink = "";
                                                    helpLink = "";
                                                    iValueBoolean = cpCore.visitProperty.getBoolean("AllowDebugging");
                                                    TagID = "AllowDebugging";
                                                    Tag = cpCore.html.html_GetFormInputCheckBox2(TagID, iValueBoolean, TagID);
                                                    OptionsPanel = (OptionsPanel 
                                                                + (cr + ("<div class=\"ccAdminSmall\">" 
                                                                + (cr2 + ("<LABEL for=\"" 
                                                                + (TagID + ("\">" 
                                                                + (Tag + (" Debug</LABEL>" 
                                                                + (helpLink 
                                                                + (cr + "</div>")))))))))));
                                                    OptionsPanel = (OptionsPanel + ("" 
                                                                + (cr + ("<div class=\"ccButtonCon\">" 
                                                                + (cr2 + ("<input type=submit name=" + ("mb value=\"" 
                                                                + (ButtonApply + ("\">" 
                                                                + (cr + ("</div>" + "")))))))))));
                                                }
                                                
                                                // 
                                                //  ----- Create the Login Panel
                                                // 
                                                if ((cpCore.doc.authContext.user.name.Trim() == "")) {
                                                    Copy = ("You are logged in as member #" 
                                                                + (cpCore.doc.authContext.user.id + "."));
                                                }
                                                else {
                                                    Copy = ("You are logged in as " 
                                                                + (cpCore.doc.authContext.user.name + "."));
                                                }
                                                
                                                LoginPanel = (LoginPanel + ("" 
                                                            + (cr + ("<div class=\"ccAdminSmall\">" 
                                                            + (cr2 
                                                            + (Copy + ("" 
                                                            + (cr + "</div>"))))))));
                                                string Caption;
                                                if (cpCore.siteProperties.getBoolean("allowEmailLogin", false)) {
                                                    Caption = "Username or Email";
                                                }
                                                else {
                                                    Caption = "Username";
                                                }
                                                
                                                TagID = "Username";
                                                LoginPanel = (LoginPanel + ("" 
                                                            + (cr + ("<div class=\"ccAdminSmall\">" 
                                                            + (cr2 + ("<LABEL for=\"" 
                                                            + (TagID + ("\">" 
                                                            + (cpCore.html.html_GetFormInputText2(TagID, "", 1, 30, TagID, false) + (" " 
                                                            + (Caption + ("</LABEL>" 
                                                            + (cr + "</div>")))))))))))));
                                                if (cpCore.siteProperties.getBoolean("allownopasswordLogin", false)) {
                                                    Caption = "Password (optional)";
                                                }
                                                else {
                                                    Caption = "Password";
                                                }
                                                
                                                TagID = "Password";
                                                LoginPanel = (LoginPanel + ("" 
                                                            + (cr + ("<div class=\"ccAdminSmall\">" 
                                                            + (cr2 + ("<LABEL for=\"" 
                                                            + (TagID + ("\">" 
                                                            + (cpCore.html.html_GetFormInputText2(TagID, "", 1, 30, TagID, true) + (" " 
                                                            + (Caption + ("</LABEL>" 
                                                            + (cr + "</div>")))))))))))));
                                                if (cpCore.siteProperties.getBoolean("AllowAutoLogin", false)) {
                                                    if (cpCore.doc.authContext.visit.CookieSupport) {
                                                        TagID = "autologin";
                                                        LoginPanel = (LoginPanel + ("" 
                                                                    + (cr + ("<div class=\"ccAdminSmall\">" 
                                                                    + (cr2 + ("<LABEL for=\"" 
                                                                    + (TagID + ("\">" 
                                                                    + (cpCore.html.html_GetFormInputCheckBox2(TagID, true, TagID) + (" Login automatically from this computer</LABEL>" 
                                                                    + (cr + "</div>")))))))))));
                                                    }
                                                    
                                                }
                                                
                                                // 
                                                //  Buttons
                                                // 
                                                LoginPanel = (LoginPanel + Adminui.GetButtonBar(Adminui.GetButtonsFromList((ButtonLogin + ("," + ButtonLogout)), true, true, "mb"), ""));
                                                // 
                                                //  ----- assemble tools panel
                                                // 
                                                Copy = ("" 
                                                            + (cr + ("<td width=\"50%\" class=\"ccPanelInput\" style=\"vertical-align:bottom;\">" 
                                                            + (genericController.htmlIndent(LoginPanel) 
                                                            + (cr + ("</td>" 
                                                            + (cr + ("<td width=\"50%\" class=\"ccPanelInput\" style=\"vertical-align:bottom;\">" 
                                                            + (genericController.htmlIndent(OptionsPanel) 
                                                            + (cr + "</td>"))))))))));
                                                Copy = ("" 
                                                            + (cr + ("<tr>" 
                                                            + (genericController.htmlIndent(Copy) 
                                                            + (cr + ("</tr>" + ""))))));
                                                Copy = ("" 
                                                            + (cr + ("<table border=\"0\" cellpadding=\"3\" cellspacing=\"0\" width=\"100%\">" 
                                                            + (genericController.htmlIndent(Copy) 
                                                            + (cr + "</table>")))));
                                                ToolsPanel.Add(main_GetPanelInput(Copy));
                                                ToolsPanel.Add(cpCore.html.html_GetFormEnd);
                                                result = (result + main_GetPanel(ToolsPanel.Text, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 5));
                                                // 
                                                result = (result + main_GetPanel(LinkPanel.Text, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 5));
                                                // 
                                                LinkPanel = null;
                                                ToolsPanel = null;
                                                AnotherPanel = null;
                                            }
                                            
                                            // 
                                            //  --- Developer Debug Panel
                                            // 
                                            if (cpCore.visitProperty.getBoolean("AllowDebugging")) {
                                                // 
                                                //  --- Debug Panel Header
                                                // 
                                                LinkPanel = new stringBuilderLegacyController();
                                                LinkPanel.Add(SpanClassAdminSmall);
                                                // LinkPanel.Add( "WebClient " & main_WebClientVersion & " | "
                                                LinkPanel.Add(("Contensive " 
                                                                + (cpCore.codeVersion() + " | ")));
                                                LinkPanel.Add((FormatDateTime(cpCore.doc.profileStartTime) + " | "));
                                                LinkPanel.Add("<a class=\"ccAdminLink\" target=\"_blank\" href=\"http: //support.Contensive.com/\">Support</A> | ");
                                                LinkPanel.Add(("<a class=\"ccAdminLink\" href=\"" 
                                                                + (genericController.encodeHTML(("/" + cpCore.serverConfig.appConfig.adminRoute)) + "\">Admin Home</A> | ")));
                                                LinkPanel.Add(("<a class=\"ccAdminLink\" href=\"" 
                                                                + (genericController.encodeHTML(("http://" + cpCore.webServer.requestDomain)) + "\">Public Home</A> | ")));
                                                LinkPanel.Add(("<a class=\"ccAdminLink\" target=\"_blank\" href=\"" 
                                                                + (genericController.encodeHTML(("/" 
                                                                    + (cpCore.serverConfig.appConfig.adminRoute + ("?" 
                                                                    + (RequestNameHardCodedPage + ("=" + HardCodedPageMyProfile)))))) + "\">My Profile</A> | ")));
                                                LinkPanel.Add("</span>");
                                                // 
                                                // 
                                                // 
                                                // DebugPanel = DebugPanel & main_GetPanel(LinkPanel.Text, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", "5")
                                                // 
                                                DebugPanel = (DebugPanel 
                                                            + (cr + ("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">" 
                                                            + (cr2 + ("<tr>" 
                                                            + (cr3 + ("<td width=\"100\" class=\"ccPanel\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"100\" height=\"1" +
                                                            "\" ></td>" 
                                                            + (cr3 + ("<td width=\"100%\" class=\"ccPanel\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"1\" height=\"1\"" +
                                                            " ></td>" 
                                                            + (cr2 + "</tr>"))))))))));
                                                DebugPanel = (DebugPanel + getDebugPanelRow("DOM", "<a class=\"ccAdminLink\" href=\"/ccLib/clientside/DOMViewer.htm\" target=\"_blank\">Click</A>"));
                                                DebugPanel = (DebugPanel + getDebugPanelRow("Trap Errors", genericController.encodeHTML(cpCore.siteProperties.trapErrors.ToString)));
                                                DebugPanel = (DebugPanel + getDebugPanelRow("Trap Email", genericController.encodeHTML(cpCore.siteProperties.getText("TrapEmail"))));
                                                DebugPanel = (DebugPanel + getDebugPanelRow("main_ServerLink", genericController.encodeHTML(cpCore.webServer.requestUrl)));
                                                DebugPanel = (DebugPanel + getDebugPanelRow("main_ServerDomain", genericController.encodeHTML(cpCore.webServer.requestDomain)));
                                                DebugPanel = (DebugPanel + getDebugPanelRow("main_ServerProtocol", genericController.encodeHTML(cpCore.webServer.requestProtocol)));
                                                DebugPanel = (DebugPanel + getDebugPanelRow("main_ServerHost", genericController.encodeHTML(cpCore.webServer.requestDomain)));
                                                DebugPanel = (DebugPanel + getDebugPanelRow("main_ServerPath", genericController.encodeHTML(cpCore.webServer.requestPath)));
                                                DebugPanel = (DebugPanel + getDebugPanelRow("main_ServerPage", genericController.encodeHTML(cpCore.webServer.requestPage)));
                                                Copy = "";
                                                if ((cpCore.webServer.requestQueryString != "")) {
                                                    CopySplit = cpCore.webServer.requestQueryString.Split("&");
                                                    for (Ptr = 0; (Ptr <= UBound(CopySplit)); Ptr++) {
                                                        copyNameValue = CopySplit[Ptr];
                                                        if ((copyNameValue != "")) {
                                                            copyNameValueSplit = copyNameValue.Split("=");
                                                            CopyName = genericController.DecodeResponseVariable(copyNameValueSplit[0]);
                                                            copyValue = "";
                                                            if ((UBound(copyNameValueSplit) > 0)) {
                                                                copyValue = genericController.DecodeResponseVariable(copyNameValueSplit[1]);
                                                            }
                                                            
                                                            Copy = (Copy 
                                                                        + (cr + ("<br>" + genericController.encodeHTML((CopyName + ("=" + copyValue))))));
                                                        }
                                                        
                                                    }
                                                    
                                                    Copy = Copy.Substring(7);
                                                }
                                                
                                                DebugPanel = (DebugPanel + getDebugPanelRow("main_ServerQueryString", Copy));
                                                Copy = "";
                                                foreach (string key in cpCore.docProperties.getKeyList()) {
                                                    docPropertiesClass docProperty = cpCore.docProperties.getProperty(key);
                                                    if (docProperty.IsForm) {
                                                        Copy = (Copy 
                                                                    + (cr + ("<br>" + genericController.encodeHTML(docProperty.NameValue))));
                                                    }
                                                    
                                                }
                                                
                                                DebugPanel = (DebugPanel + getDebugPanelRow("Render Time >= ", (Format((cpCore.doc.appStopWatch.ElapsedMilliseconds / 1000), "0.000") + " sec")));
                                                if (true) {
                                                    VisitHrs = int.Parse((cpCore.doc.authContext.visit.TimeToLastHit / 3600));
                                                    VisitMin = (int.Parse((cpCore.doc.authContext.visit.TimeToLastHit / 60)) - (60 * VisitHrs));
                                                    VisitSec = (cpCore.doc.authContext.visit.TimeToLastHit % 60);
                                                    DebugPanel = (DebugPanel + getDebugPanelRow("Visit Length", (cpCore.doc.authContext.visit.TimeToLastHit.ToString() + (" sec, (" 
                                                                    + (VisitHrs + (" hrs " 
                                                                    + (VisitMin + (" mins " 
                                                                    + (VisitSec + " secs)")))))))));
                                                    // DebugPanel = DebugPanel & main_DebugPanelRow("Visit Length", CStr(main_VisitTimeToLastHit) & " sec, (" & Int(main_VisitTimeToLastHit / 60) & " min " & (main_VisitTimeToLastHit Mod 60) & " sec)")
                                                }
                                                
                                                DebugPanel = (DebugPanel + getDebugPanelRow("Addon Profile", ("<hr><ul class=\"ccPanel\">" + ("<li>tbd</li>" 
                                                                + (cr + "</ul>")))));
                                                // 
                                                DebugPanel = (DebugPanel + "</table>");
                                                if (ShowLegacyToolsPanel) {
                                                    // 
                                                    //  Debug Panel as part of legacy tools panel
                                                    // 
                                                    result = (result + main_GetPanel(DebugPanel, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 5));
                                                }
                                                else {
                                                    // 
                                                    //  Debug Panel without Legacy Tools panel
                                                    // 
                                                    result = (result 
                                                                + (main_GetPanelHeader("Debug Panel") 
                                                                + (main_GetPanel(LinkPanel.Text) + main_GetPanel(DebugPanel, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 5))));
                                                }
                                                
                                            }
                                            
                                            result = (cr + ("<div class=\"ccCon\">" 
                                                        + (genericController.htmlIndent(result) 
                                                        + (cr + "</div>"))));
                                        }
                                        
                                    }
                                    catch (Exception ex) {
                                        cpCore.handleException(ex);
                                    }
                                    
                                    return result;
                                    // 
                                    // 
                                    // 
                                    ((string)(getDebugPanelRow(((string)(Label)), ((string)(Value)))));
                                    return (cr2 + ("<tr><td valign=\"top\" class=\"ccPanel ccAdminSmall\">" 
                                                + (Label + ("</td><td valign=\"top\" class=\"ccPanel ccAdminSmall\">" 
                                                + (Value + "</td></tr>")))));
                                    // 
                                    // =================================================================================================================
                                    //    csv_GetAddonOptionStringValue
                                    // 
                                    //    gets the value from a list matching the name
                                    // 
                                    //    InstanceOptionstring is an "AddonEncoded" name=AddonEncodedValue[selector]descriptor&name=value string
                                    // =================================================================================================================
                                    // 
                                    ((string)(getAddonOptionStringValue(((string)(OptionName)), ((string)(addonOptionString)))));
                                    string result = genericController.getSimpleNameValue(OptionName, addonOptionString, "", "&");
                                    int Pos = genericController.vbInstr(1, result, "[");
                                    if ((Pos > 0)) {
                                        result = result.Substring(0, (Pos - 1));
                                    }
                                    
                                    return genericController.decodeNvaArgument(result).Trim();
                                    // 
                                    // ====================================================================================================
                                    // '' <summary>
                                    // '' Create the full html doc from the accumulated elements
                                    // '' </summary>
                                    // '' <param name="htmlBody"></param>
                                    // '' <param name="htmlBodyTag"></param>
                                    // '' <param name="allowLogin"></param>
                                    // '' <param name="allowTools"></param>
                                    // '' <param name="blockNonContentExtras"></param>
                                    // '' <param name="isAdminSite"></param>
                                    // '' <returns></returns>
                                    ((string)(getHtmlDoc(((string)(htmlBody)), ((string)(htmlBodyTag)), Optional, allowLoginAsBoolean=True, Optional, allowToolsAsBoolean=True)));
                                    string result = "";
                                    try {
                                        string htmlHead = getHtmlHead();
                                        string htmlBeforeEndOfBody = getHtmlDoc_beforeEndOfBodyHtml(allowLogin, allowTools);
                                        result = ("" 
                                                    + (cpCore.siteProperties.docTypeDeclaration + ("\r\n" + ("<html>" + ("\r\n" + ("<head>" 
                                                    + (htmlHead + ("\r\n" + ("</head>" + ("\r\n" 
                                                    + (htmlBodyTag 
                                                    + (htmlBody 
                                                    + (htmlBeforeEndOfBody + ("\r\n" + ("</body>" + ("\r\n" + ("</html>" + "")))))))))))))))));
                                    }
                                    catch (Exception ex) {
                                        cpCore.handleException(ex);
                                    }
                                    
                                    return result;
                                    // '
                                    // ' assemble all the html parts
                                    // '
                                    // Public Function assembleHtmlDoc(ByVal head As String, ByVal bodyTag As String, ByVal Body As String) As String
                                    //     Return "" _
                                    //         & cpCore.siteProperties.docTypeDeclarationAdmin _
                                    //         & cr & "<html>" _
                                    //         & cr2 & "<head>" _
                                    //         & genericController.htmlIndent(head) _
                                    //         & cr2 & "</head>" _
                                    //         & cr2 & bodyTag _
                                    //         & genericController.htmlIndent(Body) _
                                    //         & cr2 & "</body>" _
                                    //         & cr & "</html>"
                                    // End Function
                                    // '
                                    // '========================================================================
                                    // ' ----- Starts an HTML page (for an admin page -- not a public page)
                                    // '========================================================================
                                    // '
                                    // Public Function getHtmlDoc_beforeBodyHtml(Optional ByVal Title As String = "", Optional ByVal PageMargin As Integer = 0) As String
                                    //     If Title <> "" Then
                                    //         Call main_AddPagetitle(Title)
                                    //     End If
                                    //     If main_MetaContent_Title = "" Then
                                    //         Call main_AddPagetitle("Admin-" & cpCore.webServer.webServerIO_requestDomain)
                                    //     End If
                                    //     cpCore.webServer.webServerIO_response_NoFollow = True
                                    //     Call main_SetMetaContent(0, 0)
                                    //     '
                                    //     Return "" _
                                    //         & cpCore.siteProperties.docTypeDeclarationAdmin _
                                    //         & vbCrLf & "<html>" _
                                    //         & vbCrLf & "<head>" _
                                    //         & getHTMLInternalHead(True) _
                                    //         & vbCrLf & "</head>" _
                                    //         & vbCrLf & "<body class=""ccBodyAdmin ccCon"">"
                                    // End Function
                                    // 
                                    // ====================================================================================================
                                    // '' <summary>
                                    // '' legacy compatibility
                                    // '' </summary>
                                    // '' <param name="cpCore"></param>
                                    // '' <param name="ButtonList"></param>
                                    // '' <returns></returns>
                                    ((string)(legacy_closeFormTable(((coreClass)(cpCore)), ((string)(ButtonList)))));
                                    if ((ButtonList != "")) {
                                        legacy_closeFormTable = ("</td></tr></TABLE>" 
                                                    + (cpCore.html.main_GetPanelButtons(ButtonList, "Button") + "</form>"));
                                    }
                                    else {
                                        legacy_closeFormTable = "</td></tr></TABLE></form>";
                                    }
                                    
                                    // 
                                    // ====================================================================================================
                                    // '' <summary>
                                    // '' legacy compatibility
                                    // '' </summary>
                                    // '' <param name="cpCore"></param>
                                    // '' <param name="ButtonList"></param>
                                    // '' <returns></returns>
                                    ((string)(legacy_openFormTable(((coreClass)(cpCore)), ((string)(ButtonList)))));
                                    string result = "";
                                    try {
                                        result = cpCore.html.html_GetFormStart();
                                        if ((ButtonList != "")) {
                                            result = (result + cpCore.html.main_GetPanelButtons(ButtonList, "Button"));
                                        }
                                        
                                        result = (result + "<table border=\"0\" cellpadding=\"10\" cellspacing=\"0\" width=\"100%\"><tr><TD>");
                                    }
                                    catch (Exception ex) {
                                        cpCore.handleException(ex);
                                    }
                                    
                                    return result;
                                    // 
                                    // ====================================================================================================
                                    // 
                                    ((string)(getHtmlHead()));
                                    List<string> headList = new List<string>();
                                    try {
                                        // 
                                        //  -- meta content
                                        if ((cpCore.doc.htmlMetaContent_TitleList.Count > 0)) {
                                            string content = "";
                                            foreach (asset in cpCore.doc.htmlMetaContent_TitleList) {
                                                if ((cpCore.doc.visitPropertyAllowDebugging 
                                                            && !string.IsNullOrEmpty(asset.addedByMessage))) {
                                                    headList.Add(("<!-- added by " 
                                                                    + (asset.addedByMessage + " -->")));
                                                }
                                                
                                                (" | " + asset.content);
                                            }
                                            
                                            headList.Add(("<title>" 
                                                            + (encodeHTML(content.Substring(3)) + "</title>")));
                                        }
                                        
                                        if ((cpCore.doc.htmlMetaContent_KeyWordList.Count > 0)) {
                                            string content = "";
                                            foreach (asset in cpCore.doc.htmlMetaContent_KeyWordList.FindAll(Function, a[!string.IsNullOrEmpty(a.content)])) {
                                                if ((cpCore.doc.visitPropertyAllowDebugging 
                                                            && !string.IsNullOrEmpty(asset.addedByMessage))) {
                                                    headList.Add(("<!-- \'" 
                                                                    + (encodeHTML((asset.content + ("\' added by " + asset.addedByMessage))) + " -->")));
                                                }
                                                
                                                ("," + asset.content);
                                            }
                                            
                                            if (!string.IsNullOrEmpty(content)) {
                                                headList.Add(("<meta name=\"keywords\" content=\"" 
                                                                + (encodeHTML(content.Substring(1)) + "\" >")));
                                            }
                                            
                                        }
                                        
                                        if ((cpCore.doc.htmlMetaContent_Description.Count > 0)) {
                                            string content = "";
                                            foreach (asset in cpCore.doc.htmlMetaContent_Description) {
                                                if ((cpCore.doc.visitPropertyAllowDebugging 
                                                            && !string.IsNullOrEmpty(asset.addedByMessage))) {
                                                    headList.Add(("<!-- \'" 
                                                                    + (encodeHTML((asset.content + ("\' added by " + asset.addedByMessage))) + " -->")));
                                                }
                                                
                                                ("," + asset.content);
                                            }
                                            
                                            headList.Add(("<meta name=\"description\" content=\"" 
                                                            + (encodeHTML(content.Substring(1)) + "\" >")));
                                        }
                                        
                                        // 
                                        //  -- favicon
                                        string VirtualFilename = cpCore.siteProperties.getText("faviconfilename");
                                        switch (IO.Path.GetExtension(VirtualFilename).ToLower) {
                                            case ".ico":
                                                headList.Add(("<link rel=\"icon\" type=\"image/vnd.microsoft.icon\" href=\"" 
                                                                + (genericController.getCdnFileLink(cpCore, VirtualFilename) + "\" >")));
                                                break;
                                            case ".png":
                                                headList.Add(("<link rel=\"icon\" type=\"image/png\" href=\"" 
                                                                + (genericController.getCdnFileLink(cpCore, VirtualFilename) + "\" >")));
                                                break;
                                            case ".gif":
                                                headList.Add(("<link rel=\"icon\" type=\"image/gif\" href=\"" 
                                                                + (genericController.getCdnFileLink(cpCore, VirtualFilename) + "\" >")));
                                                break;
                                            case ".jpg":
                                                headList.Add(("<link rel=\"icon\" type=\"image/jpg\" href=\"" 
                                                                + (genericController.getCdnFileLink(cpCore, VirtualFilename) + "\" >")));
                                                break;
                                        }
                                        // 
                                        //  -- misc caching, etc
                                        string encoding = genericController.encodeHTML(cpCore.siteProperties.getText("Site Character Encoding", "utf-8"));
                                        headList.Add(("<meta http-equiv=\"content-type\" content=\"text/html; charset=" 
                                                        + (encoding + "\">")));
                                        headList.Add("<meta http-equiv=\"content-language\" content=\"en-us\">");
                                        headList.Add("<meta http-equiv=\"cache-control\" content=\"no-cache\">");
                                        headList.Add("<meta http-equiv=\"expires\" content=\"-1\">");
                                        headList.Add("<meta http-equiv=\"pragma\" content=\"no-cache\">");
                                        headList.Add("<meta name=\"generator\" content=\"Contensive\">");
                                        // 
                                        //  -- no-follow
                                        if (cpCore.webServer.response_NoFollow) {
                                            headList.Add("<meta name=\"robots\" content=\"nofollow\" >");
                                            headList.Add("<meta name=\"mssmarttagspreventparsing\" content=\"true\" >");
                                        }
                                        
                                        // 
                                        //  -- base is needed for Link Alias case where a slash is in the URL (page named 1/2/3/4/5)
                                        if (!string.IsNullOrEmpty(cpCore.webServer.serverFormActionURL)) {
                                            string BaseHref = cpCore.webServer.serverFormActionURL;
                                            if (!string.IsNullOrEmpty(cpCore.doc.refreshQueryString)) {
                                                ("?" + cpCore.doc.refreshQueryString);
                                            }
                                            
                                            headList.Add(("<base href=\"" 
                                                            + (BaseHref + "\" >")));
                                        }
                                        
                                        // 
                                        if ((cpCore.doc.htmlAssetList.Count > 0)) {
                                            List<string> scriptList = new List<string>();
                                            List<string> styleList = new List<string>();
                                            foreach (asset in cpCore.doc.htmlAssetList.FindAll(Function, ((htmlAssetClass)(item))[item.inHead])) {
                                                if (cpCore.doc.allowDebugLog) {
                                                    if ((cpCore.doc.visitPropertyAllowDebugging 
                                                                && !string.IsNullOrEmpty(asset.addedByMessage))) {
                                                        headList.Add(("<!-- \'" 
                                                                        + (encodeHTML((asset.content + ("\' added by " + asset.addedByMessage))) + " -->")));
                                                    }
                                                    
                                                }
                                                
                                                if (asset.assetType.Equals(htmlAssetTypeEnum.style)) {
                                                    if (asset.isLink) {
                                                        styleList.Add(("<link rel=\"stylesheet\" type=\"text/css\" href=\"" 
                                                                        + (asset.content + "\" >")));
                                                    }
                                                    else {
                                                        styleList.Add(("<style>" 
                                                                        + (asset.content + "</style>")));
                                                    }
                                                    
                                                }
                                                else if (asset.assetType.Equals(htmlAssetTypeEnum.script)) {
                                                    if (asset.isLink) {
                                                        scriptList.Add(("<script type=\"text/javascript\" src=\"" 
                                                                        + (asset.content + "\"></script>")));
                                                    }
                                                    else {
                                                        scriptList.Add(("<script type=\"text/javascript\">" 
                                                                        + (asset.content + "</script>")));
                                                    }
                                                    
                                                }
                                                
                                            }
                                            
                                            headList.AddRange(styleList);
                                            headList.AddRange(scriptList);
                                        }
                                        
                                        // 
                                        //  -- other head tags - always last
                                        foreach (asset in cpCore.doc.htmlMetaContent_OtherTags.FindAll(Function, a[!string.IsNullOrEmpty(a.content)])) {
                                            if (cpCore.doc.allowDebugLog) {
                                                if ((cpCore.doc.visitPropertyAllowDebugging 
                                                            && !string.IsNullOrEmpty(asset.addedByMessage))) {
                                                    headList.Add(("<!-- \'" 
                                                                    + (encodeHTML((asset.content + ("\' added by " + asset.addedByMessage))) + " -->")));
                                                }
                                                
                                            }
                                            
                                            headList.Add(asset.content);
                                        }
                                        
                                    }
                                    catch (Exception ex) {
                                        cpCore.handleException(ex);
                                    }
                                    
                                    return string.Join(cr, headList);
                                }
                                
                            }
                            
                        }
                        
                    }
                    
                }
                
            }
            
        }
    }
}