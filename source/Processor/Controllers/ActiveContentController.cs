
using System;
using Contensive.BaseClasses;
using Contensive.Processor.Models.Db;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.constants;

namespace Contensive.Processor.Controllers {
    /// <summary>
    /// interpret dynamic elements with content including <AC></AC> tags and {% {} %} JSON-based content commands.
    /// </summary>
    public class ActiveContentController {
        //
        //  active content:
        //      1) addons dropped into wysiwyg editor
        //              processed here
        //      2) json formatted content commands
        //              contentCommandController called (see contentcommandcontroller for syntax and details
        //
        //====================================================================================================
        /// <summary>
        /// render active content for a web page
        /// </summary>
        /// <param name="core"></param>
        /// <param name="source"></param>
        /// <param name="contextContentName">optional, content from which the data being rendered originated (like 'Page Content')</param>
        /// <param name="ContextRecordID">optional, id of the record from which the data being rendered originated</param>
        /// <param name="ContextContactPeopleID">optional, the id of the person who should be contacted for this content. If 0, uses current user.</param>
        /// <param name="ProtocolHostString">The protocol + domain to be used to build URLs if the content includes dynamically generated images (resource library active content) and the domain is different from where the content is being rendered already. Leave blank and the URL will start with a slash.</param>
        /// <param name="DefaultWrapperID">optional, if provided and addon is html on a page, the content will be wrapped in the wrapper indicated</param>
        /// <param name="addonContext">Where this addon is being executed, like as a process, or in an email, or on a page. If not provided page context is assumed (adding assets like js and css to document)</param>
        /// <returns></returns>
        public static string renderHtmlForWeb(CoreController core, string source, string contextContentName = "", int ContextRecordID = 0, int ContextContactPeopleID = 0, string ProtocolHostString = "", int DefaultWrapperID = 0, CPUtilsBaseClass.addonContext addonContext = CPUtilsBaseClass.addonContext.ContextPage) {
            string result = ContentCmdController.executeContentCommands(core, source, CPUtilsBaseClass.addonContext.ContextAdmin, core.session.user.id, core.session.visit.visitAuthenticated);
            return encode(core, result, core.session.user.id, contextContentName, ContextRecordID, ContextContactPeopleID, false, false, true, true, false, true, "", ProtocolHostString, false, DefaultWrapperID, "", addonContext, core.session.isAuthenticated, null, core.session.isEditingAnything());
        }
        //
        //====================================================================================================
        /// <summary>
        /// render addLinkAuthToAllLinks, ActiveFormatting, ActiveImages and ActiveEditIcons. 
        /// 1) addLinkAuthToAllLinks adds a link authentication querystring to all anchor tags pointed to this application's domains.
        /// 2) ActiveFormatting converts <AC type=""></AC> tags into thier rendered equvalent.
        /// 3) ActiveImages ?
        /// 4) ActiveEditIcons: if true, it converts <AC type=""></AC> tags into <img> tags with instance properties encoded
        ///
        /// </summary>
        /// <param name="core"></param>
        /// <param name="sourceHtmlContent">The html source to be parsed.</param>
        /// <param name="personalizationPeopleId">The user to whom this rendering will be targeted</param>
        /// <param name="ContextContentName">If this content is from a DbModel, this is the content name.</param>
        /// <param name="ContextRecordID">If this content is from a DbModel, this is the record id.</param>
        /// <param name="moreInfoPeopleId">If the content includes either a more-information link, or a feedback form, this is the person to whom the feedback or more-information applies.</param>
        /// <param name="addLinkAuthenticationToAllLinks">If true, link authentication is added to all anchor tags</param>
        /// <param name="ignore"></param>
        /// <param name="encodeACResourceLibraryImages">To be deprecated: this was a way to store only a reference to library images in the content, then replace with img tag while rendering</param>
        /// <param name="encodeForWysiwygEditor">When true, active content (and addons?) are converted to images for the editor. process</param>
        /// <param name="EncodeNonCachableTags">to be deprecated: some tags could be cached and some not, this was a way to divide them.</param>
        /// <param name="queryStringToAppendToAllLinks">If provided, this querystring will be added to all anchor tags that link back to the domains for this application</param>
        /// <param name="protocolHost">The protocol plus domain desired if encoding Resource Library Images or encoding for the Wysiwyg editor</param>
        /// <param name="IsEmailContent">If true, this rendering is for an email.</param>
        /// <param name="AdminURL"></param>
        /// <param name="personalizationIsAuthenticated"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private static string renderActiveContent(CoreController core, string sourceHtmlContent, int personalizationPeopleId, string ContextContentName, int ContextRecordID, int moreInfoPeopleId, bool addLinkAuthenticationToAllLinks, bool ignore, bool encodeACResourceLibraryImages, bool encodeForWysiwygEditor, bool EncodeNonCachableTags, string queryStringToAppendToAllLinks, string protocolHost, bool IsEmailContent, string AdminURL, bool personalizationIsAuthenticated, CPUtilsBaseClass.addonContext context = CPUtilsBaseClass.addonContext.ContextPage) {
            string result = sourceHtmlContent;
            try {
                //
                // Fixup Anchor Query (additional AddonOptionString pairs to add to the end)
                string AnchorQuery = "";
                if (addLinkAuthenticationToAllLinks && (personalizationPeopleId != 0)) {
                    AnchorQuery += "&eid=" + SecurityController.encodeToken(core, personalizationPeopleId, DateTime.Now);
                }
                //
                if (!string.IsNullOrEmpty(queryStringToAppendToAllLinks)) {
                    AnchorQuery += "&" + queryStringToAppendToAllLinks;
                }
                //
                if (!string.IsNullOrEmpty(AnchorQuery)) {
                    AnchorQuery = AnchorQuery.Substring(1);
                }
                ////
                //// ----- xml contensive process instruction
                //{
                //    int Pos = genericController.vbInstr(1, workingContent, "<?contensive", 1);
                //    if (Pos > 0) {
                //        throw new ApplicationException("Structured xml data commands are no longer supported");
                //    }
                //}
                //
                // -- start-end gtroup - deprecated
                //if (workingContent.IndexOf("<!-- STARTGROUPACCESS ") > 0) {
                //    throw new ApplicationException("Structured xml data commands are no longer supported");
                //}
                //if (workingContent.IndexOf("<!-- ENDGROUPACCESS ") > 0) {
                //    throw new ApplicationException("Structured xml data commands are no longer supported");
                //}
                //
                // Test early if this needs to run at all
                bool ProcessACTags = (((EncodeNonCachableTags || encodeACResourceLibraryImages || encodeForWysiwygEditor)) & (result.IndexOf("<AC ", System.StringComparison.OrdinalIgnoreCase) != -1));
                bool ProcessAnchorTags = (!string.IsNullOrEmpty(AnchorQuery)) & (result.IndexOf("<A ", System.StringComparison.OrdinalIgnoreCase) != -1);
                if ((!string.IsNullOrEmpty(result)) & (ProcessAnchorTags || ProcessACTags)) {
                    //
                    // ----- Load the Active Elements
                    //
                    HtmlParserController KmaHTML = new HtmlParserController(core);
                    KmaHTML.Load(result);
                    StringBuilderLegacyController Stream = new StringBuilderLegacyController(); int ElementPointer = 0;
                    int FormCount = 0;
                    int FormInputCount = 0;
                    if (KmaHTML.ElementCount > 0) {
                        ElementPointer = 0;
                        result = "";
                        string serverFilePath = protocolHost + "/" + core.appConfig.name + "/files/";
                        while (ElementPointer < KmaHTML.ElementCount) {
                            string Copy = KmaHTML.Text(ElementPointer).ToString();
                            if (KmaHTML.IsTag(ElementPointer)) {
                                string ElementTag = GenericController.vbUCase(KmaHTML.TagName(ElementPointer));
                                string ACName = KmaHTML.ElementAttribute(ElementPointer, "NAME");
                                string ACType = "";
                                int NotUsedID = 0;
                                string addonOptionString = null;
                                string AddonOptionStringHTMLEncoded = null;
                                string ACInstanceID = null;
                                switch (ElementTag) {
                                    case "FORM":
                                        //
                                        // Form created in content
                                        // EncodeEditIcons -> remove the
                                        //
                                        if (EncodeNonCachableTags) {
                                            FormCount = FormCount + 1;
                                            //
                                            // 5/14/2009 - DM said it is OK to remove UserResponseForm Processing
                                            // however, leave this one because it is needed to make current forms work.
                                            //
                                            // 20180914 - contensiveuserform deprecated (has not been used forever, so not needed)
                                            //if ((Copy.IndexOf("contensiveuserform=1", System.StringComparison.OrdinalIgnoreCase) != -1) | (Copy.IndexOf("contensiveuserform=\"1\"", System.StringComparison.OrdinalIgnoreCase) != -1)) {
                                            //    //
                                            //    // if it has "contensiveuserform=1" in the form tag, remove it from the form and add the hidden that makes it work
                                            //    //
                                            //    Copy = genericController.vbReplace(Copy, "ContensiveUserForm=1", "", 1, 99, 1);
                                            //    Copy = genericController.vbReplace(Copy, "ContensiveUserForm=\"1\"", "", 1, 99, 1);
                                            //    if (!encodeForWysiwygEditor) {
                                            //        Copy += "<input type=hidden name=ContensiveUserForm value=1>";
                                            //    }
                                            //}
                                        }
                                        break;
                                    case "INPUT":
                                        if (EncodeNonCachableTags) {
                                            FormInputCount = FormInputCount + 1;
                                        }
                                        break;
                                    case "A":
                                        if (!string.IsNullOrEmpty(AnchorQuery)) {
                                            //
                                            // ----- Add ?eid=0000 to all anchors back to the same site so emails
                                            //       can be sent that will automatically log the person in when they
                                            //       arrive.
                                            //
                                            int AttributeCount = KmaHTML.ElementAttributeCount(ElementPointer);
                                            if (AttributeCount > 0) {
                                                Copy = "<A ";
                                                for (int AttributePointer = 0; AttributePointer < AttributeCount; AttributePointer++) {
                                                    string Name = KmaHTML.ElementAttributeName(ElementPointer, AttributePointer);
                                                    string Value = KmaHTML.ElementAttributeValue(ElementPointer, AttributePointer);
                                                    if (GenericController.vbUCase(Name) == "HREF") {
                                                        string Link = Value;
                                                        int Pos = GenericController.vbInstr(1, Link, "://");
                                                        if (Pos > 0) {
                                                            Link = Link.Substring(Pos + 2);
                                                            Pos = GenericController.vbInstr(1, Link, "/");
                                                            if (Pos > 0) {
                                                                Link = Link.Left(Pos - 1);
                                                            }
                                                        }
                                                        if ((string.IsNullOrEmpty(Link)) || (("," + core.appConfig.domainList[0] + ",").IndexOf("," + Link + ",", System.StringComparison.OrdinalIgnoreCase) != -1)) {
                                                            //
                                                            // ----- link is for this site
                                                            //
                                                            if (Value.Substring(Value.Length - 1) == "?") {
                                                                //
                                                                // Ends in a questionmark, must be Dwayne (?)
                                                                //
                                                                Value = Value + AnchorQuery;
                                                            } else if (GenericController.vbInstr(1, Value, "mailto:", 1) != 0) {
                                                                //
                                                                // catch mailto
                                                                //
                                                                //Value = Value & AnchorQuery
                                                            } else if (GenericController.vbInstr(1, Value, "?") == 0) {
                                                                //
                                                                // No questionmark there, add it
                                                                //
                                                                Value = Value + "?" + AnchorQuery;
                                                            } else {
                                                                //
                                                                // Questionmark somewhere, add new value with amp;
                                                                //
                                                                Value = Value + "&" + AnchorQuery;
                                                            }
                                                            //    End If
                                                        }
                                                    }
                                                    Copy += " " + Name + "=\"" + Value + "\"";
                                                }
                                                Copy += ">";
                                            }
                                        }
                                        break;
                                    case "AC":
                                        //
                                        // ----- decode all AC tags
                                        //
                                        ACType = KmaHTML.ElementAttribute(ElementPointer, "TYPE");
                                        ACInstanceID = KmaHTML.ElementAttribute(ElementPointer, "ACINSTANCEID");
                                        string ACGuid = KmaHTML.ElementAttribute(ElementPointer, "GUID");
                                        switch (ACType.ToUpper()) {
                                            case ACTypeAggregateFunction: {
                                                    //
                                                    // -- Add-on
                                                    NotUsedID = 0;
                                                    AddonOptionStringHTMLEncoded = KmaHTML.ElementAttribute(ElementPointer, "QUERYSTRING");
                                                    addonOptionString = HtmlController.decodeHtml(AddonOptionStringHTMLEncoded);
                                                    if (IsEmailContent) {
                                                        //
                                                        // -- Addon for email
                                                        if (EncodeNonCachableTags) {
                                                            switch (GenericController.vbLCase(ACName)) {
                                                                case "block text":
                                                                    //
                                                                    // -- start block text
                                                                    Copy = "";
                                                                    string GroupIDList = HtmlController.getAddonOptionStringValue("AllowGroups", addonOptionString);
                                                                    if (!core.session.isMemberOfGroupIdList(core, personalizationPeopleId, true, GroupIDList, true)) {
                                                                        //
                                                                        // Block content if not allowed
                                                                        //
                                                                        ElementPointer = ElementPointer + 1;
                                                                        while (ElementPointer < KmaHTML.ElementCount) {
                                                                            ElementTag = GenericController.vbUCase(KmaHTML.TagName(ElementPointer));
                                                                            if (ElementTag == "AC") {
                                                                                ACType = GenericController.vbUCase(KmaHTML.ElementAttribute(ElementPointer, "TYPE"));
                                                                                if (ACType == ACTypeAggregateFunction) {
                                                                                    if (GenericController.vbLCase(KmaHTML.ElementAttribute(ElementPointer, "name")) == "block text end") {
                                                                                        break;
                                                                                    }
                                                                                }
                                                                            }
                                                                            ElementPointer = ElementPointer + 1;
                                                                        }
                                                                    }
                                                                    break;
                                                                case "block text end":
                                                                    //
                                                                    // -- end block text
                                                                    Copy = "";
                                                                    break;
                                                                default:
                                                                    //
                                                                    // -- addons
                                                                    CPUtilsBaseClass.addonExecuteContext executeContext = new CPUtilsBaseClass.addonExecuteContext() {
                                                                        addonType = CPUtilsBaseClass.addonContext.ContextEmail,
                                                                        cssContainerClass = "",
                                                                        cssContainerId = "",
                                                                        hostRecord = new CPUtilsBaseClass.addonExecuteHostRecordContext() {
                                                                            contentName = ContextContentName,
                                                                            fieldName = "",
                                                                            recordId = ContextRecordID
                                                                        },
                                                                        personalizationAuthenticated = personalizationIsAuthenticated,
                                                                        personalizationPeopleId = personalizationPeopleId,
                                                                        instanceArguments = GenericController.convertAddonArgumentstoDocPropertiesList(core, AddonOptionStringHTMLEncoded),
                                                                        instanceGuid = ACInstanceID,
                                                                        errorContextMessage = "rendering addon found in active content within an email"
                                                                    };
                                                                    AddonModel addon = AddonModel.createByUniqueName(core, ACName);
                                                                    Copy = core.addon.execute(addon, executeContext);
                                                                    break;
                                                            }
                                                        }
                                                    } else {
                                                        //
                                                        // Addon - for web
                                                        //

                                                        if (encodeForWysiwygEditor) {
                                                            //
                                                            // Get IconFilename, update the optionstring, and execute optionstring replacement functions
                                                            //
                                                            string AddonContentName = cnAddons;
                                                            string SelectList = "Name,Link,ID,ArgumentList,ObjectProgramID,IconFilename,IconWidth,IconHeight,IconSprites,IsInline,ccGuid";
                                                            string Criteria = "";
                                                            int IconWidth = 0;
                                                            int IconHeight = 0;
                                                            int IconSprites = 0;
                                                            string IconAlt = "";
                                                            string IconTitle = "";
                                                            bool AddonIsInline = false;
                                                            string SrcOptionList = "";
                                                            string IconFilename = "";
                                                            if (!string.IsNullOrEmpty(ACGuid)) {
                                                                Criteria = "ccguid=" + core.db.encodeSQLText(ACGuid);
                                                            } else {
                                                                Criteria = "name=" + core.db.encodeSQLText(ACName.ToUpper());
                                                            }
                                                            int CS = core.db.csOpen(AddonContentName, Criteria, "Name,ID", false, 0, false, false, SelectList);
                                                            if (core.db.csOk(CS)) {
                                                                IconFilename = core.db.csGet(CS, "IconFilename");
                                                                SrcOptionList = core.db.csGet(CS, "ArgumentList");
                                                                IconWidth = core.db.csGetInteger(CS, "IconWidth");
                                                                IconHeight = core.db.csGetInteger(CS, "IconHeight");
                                                                IconSprites = core.db.csGetInteger(CS, "IconSprites");
                                                                AddonIsInline = core.db.csGetBoolean(CS, "IsInline");
                                                                ACGuid = core.db.csGetText(CS, "ccGuid");
                                                                IconAlt = ACName;
                                                                IconTitle = "Rendered as the Add-on [" + ACName + "]";
                                                            } else {
                                                                switch (GenericController.vbLCase(ACName)) {
                                                                    case "block text":
                                                                        IconFilename = "";
                                                                        SrcOptionList = AddonOptionConstructor_ForBlockText;
                                                                        IconWidth = 0;
                                                                        IconHeight = 0;
                                                                        IconSprites = 0;
                                                                        AddonIsInline = true;
                                                                        ACGuid = "";
                                                                        break;
                                                                    case "block text end":
                                                                        IconFilename = "";
                                                                        SrcOptionList = "";
                                                                        IconWidth = 0;
                                                                        IconHeight = 0;
                                                                        IconSprites = 0;
                                                                        AddonIsInline = true;
                                                                        ACGuid = "";
                                                                        break;
                                                                    default:
                                                                        IconFilename = "";
                                                                        SrcOptionList = "";
                                                                        IconWidth = 0;
                                                                        IconHeight = 0;
                                                                        IconSprites = 0;
                                                                        AddonIsInline = false;
                                                                        IconAlt = "Unknown Add-on [" + ACName + "]";
                                                                        IconTitle = "Unknown Add-on [" + ACName + "]";
                                                                        ACGuid = "";
                                                                        break;
                                                                }
                                                            }
                                                            core.db.csClose(ref CS);
                                                            //
                                                            // Build AddonOptionStringHTMLEncoded from SrcOptionList (for names), itself (for current settings), and SrcOptionList (for select options)
                                                            //
                                                            if (SrcOptionList.IndexOf("wrapper", System.StringComparison.OrdinalIgnoreCase) == -1) {
                                                                if (AddonIsInline) {
                                                                    SrcOptionList = SrcOptionList + "\r\n" + AddonOptionConstructor_Inline;
                                                                } else {
                                                                    SrcOptionList = SrcOptionList + "\r\n" + AddonOptionConstructor_Block;
                                                                }
                                                            }
                                                            string ResultOptionListHTMLEncoded = "";
                                                            if (!string.IsNullOrEmpty(SrcOptionList)) {
                                                                ResultOptionListHTMLEncoded = "";
                                                                SrcOptionList = GenericController.vbReplace(SrcOptionList, "\r\n", "\r");
                                                                SrcOptionList = GenericController.vbReplace(SrcOptionList, "\n", "\r");
                                                                string[] SrcOptions = GenericController.stringSplit(SrcOptionList, "\r");
                                                                for (int Ptr = 0; Ptr <= SrcOptions.GetUpperBound(0); Ptr++) {
                                                                    string SrcOptionName = SrcOptions[Ptr];
                                                                    int LoopPtr2 = 0;

                                                                    while ((SrcOptionName.Length > 1) && (SrcOptionName.Left(1) == "\t") && (LoopPtr2 < 100)) {
                                                                        SrcOptionName = SrcOptionName.Substring(1);
                                                                        LoopPtr2 = LoopPtr2 + 1;
                                                                    }
                                                                    string SrcOptionValueSelector = "";
                                                                    string SrcOptionSelector = "";
                                                                    int Pos = GenericController.vbInstr(1, SrcOptionName, "=");
                                                                    if (Pos > 0) {
                                                                        SrcOptionValueSelector = SrcOptionName.Substring(Pos);
                                                                        SrcOptionName = SrcOptionName.Left(Pos - 1);
                                                                        SrcOptionSelector = "";
                                                                        Pos = GenericController.vbInstr(1, SrcOptionValueSelector, "[");
                                                                        if (Pos != 0) {
                                                                            SrcOptionSelector = SrcOptionValueSelector.Substring(Pos - 1);
                                                                        }
                                                                    }
                                                                    // all Src and Instance vars are already encoded correctly
                                                                    if (!string.IsNullOrEmpty(SrcOptionName)) {
                                                                        // since AddonOptionString is encoded, InstanceOptionValue will be also
                                                                        string InstanceOptionValue = HtmlController.getAddonOptionStringValue(SrcOptionName, addonOptionString);
                                                                        //InstanceOptionValue = core.csv_GetAddonOption(SrcOptionName, AddonOptionString)
                                                                        string ResultOptionSelector = core.html.getAddonSelector(SrcOptionName, GenericController.encodeNvaArgument(InstanceOptionValue), SrcOptionSelector);
                                                                        //ResultOptionSelector = csv_GetAddonSelector(SrcOptionName, InstanceOptionValue, SrcOptionValueSelector)
                                                                        ResultOptionListHTMLEncoded = ResultOptionListHTMLEncoded + "&" + ResultOptionSelector;
                                                                    }
                                                                }
                                                                if (!string.IsNullOrEmpty(ResultOptionListHTMLEncoded)) {
                                                                    ResultOptionListHTMLEncoded = ResultOptionListHTMLEncoded.Substring(1);
                                                                }
                                                            }
                                                            string ACNameCaption = GenericController.vbReplace(ACName, "\"", "");
                                                            ACNameCaption = HtmlController.encodeHtml(ACNameCaption);
                                                            string IDControlString = "AC," + ACType + "," + NotUsedID + "," + GenericController.encodeNvaArgument(ACName) + "," + ResultOptionListHTMLEncoded + "," + ACGuid;
                                                            Copy = AddonController.getAddonIconImg(AdminURL, IconWidth, IconHeight, IconSprites, AddonIsInline, IDControlString, IconFilename, serverFilePath, IconAlt, IconTitle, ACInstanceID, 0);
                                                        } else if (EncodeNonCachableTags) {
                                                            //
                                                            // Add-on Experiment - move all processing to the Webclient
                                                            // just pass the name and arguments back in th FPO
                                                            // HTML encode and quote the name and AddonOptionString
                                                            //
                                                            Copy = ""
                                                            + ""
                                                            + "<!-- ADDON "
                                                            + "\"" + ACName + "\""
                                                            + ",\"" + AddonOptionStringHTMLEncoded + "\""
                                                            + ",\"" + ACInstanceID + "\""
                                                            + ",\"" + ACGuid + "\""
                                                            + " -->"
                                                            + "";
                                                        }
                                                        //
                                                    }
                                                    break;
                                                }
                                            //case ACTypeEnd: {
                                            //        //
                                            //        // End Tag - Personalization
                                            //        //       This tag causes an end to the all tags, like Language
                                            //        //       It is removed by with EncodeEditIcons (on the way to the editor)
                                            //        //       It is added to the end of the content with Decode(activecontent)
                                            //        //
                                            //        if (encodeForWysiwygEditor) {
                                            //            Copy = "";
                                            //        } else if (EncodeNonCachableTags) {
                                            //            Copy = "<!-- Language ANY -->";
                                            //        }
                                            //        break;
                                            //    }
                                            //case ACTypeDate: {
                                            //        //
                                            //        // Date Tag
                                            //        //
                                            //        if (encodeForWysiwygEditor) {
                                            //            string IconIDControlString = "AC," + ACTypeDate;
                                            //            Copy = AddonController.getAddonIconImg(AdminURL, 0, 0, 0, true, IconIDControlString, "", serverFilePath, "Current Date", "Renders as [Current Date]", ACInstanceID, 0);
                                            //            //Copy = IconImg;
                                            //            //Copy = "<img ACInstanceID=""" & ACInstanceID & """ alt=""Add-on"" title=""Rendered as the current date"" ID=""AC," & ACTypeDate & """ src=""/ccLib/images/ACDate.GIF"">"
                                            //        } else if (EncodeNonCachableTags) {
                                            //            Copy = DateTime.Now.ToString();
                                            //        }
                                            //        break;
                                            //    }
                                            //case ACTypeOrganization: {
                                            //        string fieldName = KmaHTML.ElementAttribute(ElementPointer, "FIELD").ToLower();
                                            //        if (string.IsNullOrWhiteSpace(fieldName)) {
                                            //            fieldName = "name";
                                            //        }
                                            //        if (encodeForWysiwygEditor) {
                                            //            string IconIDControlString = "AC," + ACType + "," + fieldName;
                                            //            Copy = AddonController.getAddonIconImg(AdminURL, 0, 0, 0, true, IconIDControlString, "", serverFilePath, "User's Organization " + fieldName, "Renders as [User's Organization " + fieldName + "]", ACInstanceID, 0);
                                            //        } else if (EncodeNonCachableTags) {
                                            //            if ( !core.db.csOk(csOrganization)) {
                                            //                if (!core.db.csOk(csPeople)) {
                                            //                    csPeople = core.db.csOpen(personModel.contentName, "(id=" + personalizationPeopleId + ")");
                                            //                }
                                            //                if ( core.db.csOk( csPeople)) {
                                            //                    csOrganization = core.db.csOpen(organizationModel.contentName, "(id=" + core.db.csGetInteger(csPeople, "organizationId") + ")");
                                            //                }
                                            //            }
                                            //            if (core.db.csOk(csOrganization)) {
                                            //                Copy = core.db.csGetLookup(csOrganization, fieldName);
                                            //            }
                                            //        }
                                            //        break;
                                            //    }
                                            //case ACTypeMember:
                                            //case ACTypePersonalization: {
                                            //        //
                                            //        // Member Tag works regardless of authentication
                                            //        // cm must be sure not to reveal anything
                                            //        //
                                            //        string fieldName = genericController.vbUCase(KmaHTML.ElementAttribute(ElementPointer, "FIELD"));
                                            //        if (string.IsNullOrEmpty(fieldName)) {
                                            //            fieldName = HtmlController.getAddonOptionStringValue("FIELD", KmaHTML.ElementAttribute(ElementPointer, "QUERYSTRING"));
                                            //        }
                                            //        string fieldNameInitialCaps = genericController.encodeInitialCaps(fieldName);
                                            //        if (string.IsNullOrEmpty(fieldNameInitialCaps)) {
                                            //            fieldNameInitialCaps = "Name";
                                            //        }
                                            //        if (encodeForWysiwygEditor) {
                                            //            switch (genericController.vbUCase(fieldNameInitialCaps)) {
                                            //                case "FIRSTNAME":
                                            //                    //
                                            //                    string IconIDControlString = "AC," + ACType + "," + fieldNameInitialCaps;
                                            //                    Copy = AddonController.getAddonIconImg(AdminURL, 0, 0, 0, true, IconIDControlString, "", serverFilePath, "User's First Name", "Renders as [User's First Name]", ACInstanceID, 0);
                                            //                    //Copy = IconImg;
                                            //                    //
                                            //                    break;
                                            //                case "LASTNAME":
                                            //                    //
                                            //                    IconIDControlString = "AC," + ACType + "," + fieldNameInitialCaps;
                                            //                    Copy = AddonController.getAddonIconImg(AdminURL, 0, 0, 0, true, IconIDControlString, "", serverFilePath, "User's Last Name", "Renders as [User's Last Name]", ACInstanceID, 0);
                                            //                    //Copy = IconImg;
                                            //                    //
                                            //                    break;
                                            //                default:
                                            //                    //
                                            //                    IconIDControlString = "AC," + ACType + "," + fieldNameInitialCaps;
                                            //                    Copy = AddonController.getAddonIconImg(AdminURL, 0, 0, 0, true, IconIDControlString, "", serverFilePath, "User's " + fieldNameInitialCaps, "Renders as [User's " + fieldNameInitialCaps + "]", ACInstanceID, 0);
                                            //                    //Copy = IconImg;
                                            //                    //
                                            //                    break;
                                            //            }
                                            //        } else if (EncodeNonCachableTags) {
                                            //            if (personalizationPeopleId != 0) {
                                            //                if (genericController.vbUCase(fieldNameInitialCaps) == "EID") {
                                            //                    Copy = SecurityController.encodeToken(core, personalizationPeopleId, DateTime.Now);
                                            //                } else {
                                            //                    if (!core.db.csOk(csPeople)) {
                                            //                        csPeople = core.db.csOpen(personModel.contentName, "(id=" + personalizationPeopleId + ")");
                                            //                    }
                                            //                    if (core.db.csOk(csPeople)) {
                                            //                        Copy = core.db.csGetLookup(csPeople, fieldName);
                                            //                    }
                                            //                }
                                            //            }
                                            //        }
                                            //        break;
                                            //    }
                                            //case ACTypeChildList: {
                                            //        //
                                            //        // Child List
                                            //        //
                                            //        string ListName = genericController.encodeText((KmaHTML.ElementAttribute(ElementPointer, "name")));

                                            //        if (encodeForWysiwygEditor) {
                                            //            string IconIDControlString = "AC," + ACType + ",," + ACName;
                                            //            Copy = addonController.GetAddonIconImg(AdminURL, 0, 0, 0, true, IconIDControlString, "", serverFilePath, "List of Child Pages", "Renders as [List of Child Pages]", ACInstanceID, 0);
                                            //            //Copy = IconImg;
                                            //        } else if (EncodeCachableTags) {
                                            //            //
                                            //            // Handle in webclient
                                            //            //
                                            //            // removed sort method because all child pages are read in together in the order set by the parent - improve this later
                                            //            Copy = "{{" + ACTypeChildList + "?name=" + genericController.encodeNvaArgument(ListName) + "}}";
                                            //        }
                                            //        break;
                                            //    }
                                            //case ACTypeContact: {
                                            //        //
                                            //        // Formatting Tag
                                            //        //
                                            //        if (encodeForWysiwygEditor) {
                                            //            //
                                            //            string IconIDControlString = "AC," + ACType;
                                            //            Copy = AddonController.getAddonIconImg(AdminURL, 0, 0, 0, true, IconIDControlString, "", serverFilePath, "Contact Information Line", "Renders as [Contact Information Line]", ACInstanceID, 0);
                                            //            //Copy = IconImg;
                                            //            //
                                            //            //Copy = "<img ACInstanceID=""" & ACInstanceID & """ alt=""Add-on"" title=""Rendered as a line of text with contact information for this record's primary contact"" id=""AC," & ACType & """ src=""/ccLib/images/ACContact.GIF"">"
                                            //        } else if (EncodeCachableTags) {
                                            //            if (moreInfoPeopleId != 0) {
                                            //                Copy = pageContentController.getMoreInfoHtml(core, moreInfoPeopleId);
                                            //            }
                                            //        }
                                            //        break;
                                            //    }
                                            //case ACTypeFeedback: {
                                            //        //
                                            //        // Formatting tag - change from information to be included after submission
                                            //        //
                                            //        if (encodeForWysiwygEditor) {
                                            //            //
                                            //            string IconIDControlString = "AC," + ACType;
                                            //            Copy = AddonController.getAddonIconImg(AdminURL, 0, 0, 0, false, IconIDControlString, "", serverFilePath, "Feedback Form", "Renders as [Feedback Form]", ACInstanceID, 0);
                                            //            //Copy = IconImg;
                                            //            //
                                            //            //Copy = "<img ACInstanceID=""" & ACInstanceID & """ alt=""Add-on"" title=""Rendered as a feedback form, sent to this record's primary contact."" id=""AC," & ACType & """ src=""/ccLib/images/ACFeedBack.GIF"">"
                                            //        } else if (EncodeNonCachableTags) {
                                            //            if ((moreInfoPeopleId != 0) & (!string.IsNullOrEmpty(ContextContentName)) & (ContextRecordID != 0)) {
                                            //                Copy = FeedbackFormNotSupportedComment;
                                            //            }
                                            //        }
                                            //        break;
                                            //    }
                                            //case ACTypeImage: {
                                            //        //
                                            //        // ----- Image Tag, substitute image placeholder with the link from the REsource Library Record
                                            //        if (encodeACResourceLibraryImages) {
                                            //            Copy = "";
                                            //            int ACAttrRecordID = genericController.encodeInteger(KmaHTML.ElementAttribute(ElementPointer, "RECORDID"));
                                            //            int ACAttrWidth = genericController.encodeInteger(KmaHTML.ElementAttribute(ElementPointer, "WIDTH"));
                                            //            int ACAttrHeight = genericController.encodeInteger(KmaHTML.ElementAttribute(ElementPointer, "HEIGHT"));
                                            //            string ACAttrAlt = genericController.encodeText(KmaHTML.ElementAttribute(ElementPointer, "ALT"));
                                            //            int ACAttrBorder = genericController.encodeInteger(KmaHTML.ElementAttribute(ElementPointer, "BORDER"));
                                            //            int ACAttrLoop = genericController.encodeInteger(KmaHTML.ElementAttribute(ElementPointer, "LOOP"));
                                            //            int ACAttrVSpace = genericController.encodeInteger(KmaHTML.ElementAttribute(ElementPointer, "VSPACE"));
                                            //            int ACAttrHSpace = genericController.encodeInteger(KmaHTML.ElementAttribute(ElementPointer, "HSPACE"));
                                            //            string ACAttrAlign = genericController.encodeText(KmaHTML.ElementAttribute(ElementPointer, "ALIGN"));
                                            //            //
                                            //            libraryFilesModel file = libraryFilesModel.create(core, ACAttrRecordID);
                                            //            if (file != null) {
                                            //                string Filename = file.filename;
                                            //                Filename = genericController.vbReplace(Filename, "\\", "/");
                                            //                Filename = genericController.encodeURL(Filename);
                                            //                Copy += "<img ID=\"AC,IMAGE,," + ACAttrRecordID + "\" src=\"" + genericController.getCdnFileLink(core, Filename) + "\"";
                                            //                //
                                            //                if (ACAttrWidth == 0) {
                                            //                    ACAttrWidth = file.width;
                                            //                }
                                            //                if (ACAttrWidth != 0) {
                                            //                    Copy += " width=\"" + ACAttrWidth + "\"";
                                            //                }
                                            //                //
                                            //                if (ACAttrHeight == 0) {
                                            //                    ACAttrHeight = file.height;
                                            //                }
                                            //                if (ACAttrHeight != 0) {
                                            //                    Copy += " height=\"" + ACAttrHeight + "\"";
                                            //                }
                                            //                //
                                            //                if (ACAttrVSpace != 0) {
                                            //                    Copy += " vspace=\"" + ACAttrVSpace + "\"";
                                            //                }
                                            //                //
                                            //                if (ACAttrHSpace != 0) {
                                            //                    Copy += " hspace=\"" + ACAttrHSpace + "\"";
                                            //                }
                                            //                //
                                            //                if (!string.IsNullOrEmpty(ACAttrAlt)) {
                                            //                    Copy += " alt=\"" + ACAttrAlt + "\"";
                                            //                }
                                            //                //
                                            //                if (!string.IsNullOrEmpty(ACAttrAlign)) {
                                            //                    Copy += " align=\"" + ACAttrAlign + "\"";
                                            //                }
                                            //                //
                                            //                // no, 0 is an important value
                                            //                //If ACAttrBorder <> 0 Then
                                            //                Copy += " border=\"" + ACAttrBorder + "\"";
                                            //                //    End If
                                            //                //
                                            //                if (ACAttrLoop != 0) {
                                            //                    Copy += " loop=\"" + ACAttrLoop + "\"";
                                            //                }
                                            //                //
                                            //                string attr = genericController.encodeText(KmaHTML.ElementAttribute(ElementPointer, "STYLE"));
                                            //                if (!string.IsNullOrEmpty(attr)) {
                                            //                    Copy += " style=\"" + attr + "\"";
                                            //                }
                                            //                //
                                            //                attr = genericController.encodeText(KmaHTML.ElementAttribute(ElementPointer, "CLASS"));
                                            //                if (!string.IsNullOrEmpty(attr)) {
                                            //                    Copy += " class=\"" + attr + "\"";
                                            //                }
                                            //                //
                                            //                Copy += ">";
                                            //            }
                                            //        }
                                            //        break;
                                            //    }
                                            //case ACTypeDownload: {
                                            //        //
                                            //        // ----- substitute and anchored download image for the AC-Download tag
                                            //        int ACAttrRecordID = genericController.encodeInteger(KmaHTML.ElementAttribute(ElementPointer, "RECORDID"));
                                            //        string ACAttrAlt = genericController.encodeText(KmaHTML.ElementAttribute(ElementPointer, "ALT"));
                                            //        //
                                            //        if (encodeForWysiwygEditor) {
                                            //            //
                                            //            // Encoding the edit icons for the active editor form
                                            //            string IconIDControlString = "AC," + ACTypeDownload + ",," + ACAttrRecordID;
                                            //            Copy = AddonController.getAddonIconImg(AdminURL, 16, 16, 0, true, IconIDControlString, "/ccLib/images/IconDownload3.gif", serverFilePath, "Download Icon with a link to a resource", "Renders as [Download Icon with a link to a resource]", ACInstanceID, 0);
                                            //            //Copy = IconImg;
                                            //            //
                                            //            //Copy = "<img ACInstanceID=""" & ACInstanceID & """ alt=""Renders as a download icon"" id=""AC," & ACTypeDownload & ",," & ACAttrRecordID & """ src=""/ccLib/images/IconDownload3.GIF"">"
                                            //        } else if (encodeACResourceLibraryImages) {
                                            //            //
                                            //            libraryFilesModel file = libraryFilesModel.create(core, ACAttrRecordID);
                                            //            if (file != null) {
                                            //                if (string.IsNullOrEmpty(ACAttrAlt)) {
                                            //                    ACAttrAlt = genericController.encodeText(file.altText);
                                            //                }
                                            //                Copy = "<a href=\"" + protocolHost + "/" + core.siteProperties.serverPageDefault + "?" + RequestNameDownloadID + "=" + ACAttrRecordID + "\" target=\"_blank\"><img src=\"" + protocolHost + "/ccLib/images/IconDownload3.gif\" width=\"16\" height=\"16\" border=\"0\" alt=\"" + ACAttrAlt + "\"></a>";
                                            //            }
                                            //        }
                                            //        break;
                                            //    }
                                            case ACTypeTemplateContent: {
                                                    //
                                                    // ----- Create Template Content
                                                    AddonOptionStringHTMLEncoded = "";
                                                    addonOptionString = "";
                                                    NotUsedID = 0;
                                                    if (encodeForWysiwygEditor) {
                                                        //
                                                        string IconIDControlString = "AC," + ACType + "," + NotUsedID + "," + ACName + "," + AddonOptionStringHTMLEncoded;
                                                        Copy = AddonController.getAddonIconImg(AdminURL, 52, 64, 0, false, IconIDControlString, "/ccLib/images/ACTemplateContentIcon.gif", serverFilePath, "Template Page Content", "Renders as the Template Page Content", ACInstanceID, 0);
                                                        //Copy = IconImg;
                                                    } else if (EncodeNonCachableTags) {
                                                        //
                                                        // Add in the Content
                                                        Copy = fpoContentBox;
                                                    }
                                                    break;
                                                }
                                                // 20180914 - deprecate, template text should be an addon, this was an old special case
                                                //case ACTypeTemplateText: {
                                                //        //
                                                //        // ----- Create Template Content
                                                //        AddonOptionStringHTMLEncoded = KmaHTML.ElementAttribute(ElementPointer, "QUERYSTRING");
                                                //        addonOptionString = HtmlController.decodeHtml(AddonOptionStringHTMLEncoded);
                                                //        NotUsedID = 0;
                                                //        if (encodeForWysiwygEditor) {
                                                //            //
                                                //            string IconIDControlString = "AC," + ACType + "," + NotUsedID + "," + ACName + "," + AddonOptionStringHTMLEncoded;
                                                //            Copy = AddonController.getAddonIconImg(AdminURL, 52, 52, 0, false, IconIDControlString, "/ccLib/images/ACTemplateTextIcon.gif", serverFilePath, "Template Text", "Renders as a Template Text Box", ACInstanceID, 0);
                                                //            //Copy = IconImg;
                                                //            //
                                                //            //Copy = "<img ACInstanceID=""" & ACInstanceID & """ onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as Template Text"" id=""AC," & ACType & "," & NotUsedID & "," & ACName & "," & AddonOptionStringHTMLEncoded & """ src=""/ccLib/images/ACTemplateTextIcon.gif"" WIDTH=52 HEIGHT=52>"
                                                //        } else if (EncodeNonCachableTags) {
                                                //            //
                                                //            // Add in the Content Page
                                                //            string NewName = HtmlController.getAddonOptionStringValue("new", addonOptionString);
                                                //            string TextName = HtmlController.getAddonOptionStringValue("name", addonOptionString);
                                                //            if (string.IsNullOrEmpty(TextName)) {
                                                //                TextName = "Default";
                                                //            }
                                                //            Copy = "{{" + ACTypeTemplateText + "?name=" + genericController.encodeNvaArgument(TextName) + "&new=" + genericController.encodeNvaArgument(NewName) + "}}";
                                                //        }
                                                //        break;
                                                //    }
                                                // 20180914 - deprecate, watch list should be an addon
                                                //case ACTypeWatchList: {
                                                //        //
                                                //        // ----- Formatting Tag
                                                //        AddonOptionStringHTMLEncoded = KmaHTML.ElementAttribute(ElementPointer, "QUERYSTRING");
                                                //        addonOptionString = HtmlController.decodeHtml(AddonOptionStringHTMLEncoded);
                                                //        if (encodeForWysiwygEditor) {
                                                //            //
                                                //            string IconIDControlString = "AC," + ACType + "," + NotUsedID + "," + ACName + "," + AddonOptionStringHTMLEncoded;
                                                //            Copy = AddonController.getAddonIconImg(AdminURL, 109, 10, 0, true, IconIDControlString, "/ccLib/images/ACWatchList.gif", serverFilePath, "Watch List", "Renders as the Watch List [" + ACName + "]", ACInstanceID, 0);
                                                //            //Copy = IconImg;
                                                //        } else if (EncodeNonCachableTags) {
                                                //            Copy = "{{" + ACTypeWatchList + "?" + addonOptionString + "}}";
                                                //        }
                                                //        break;
                                                //    }
                                        }
                                        break;
                                }
                            }
                            //
                            // ----- Output the results
                            //
                            Stream.Add(Copy);
                            ElementPointer = ElementPointer + 1;
                        }
                    }
                    result = Stream.Text;
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return result;
        }
        //   
        //====================================================================================================
        /// <summary>
        /// Decodes ActiveContent and EditIcons into AC tags.
        /// Detect IMG tags:
        /// - If IMG ID attribute is "AC,IMAGE,recordid", convert to AC Image tag
        /// - If IMG ID attribute is "AC,DOWNLOAD,recordid", convert to AC Download tag
        /// - If IMG ID attribute is "AC,ACType,ACFieldName,ACInstanceName,QueryStringArguments,AddonGuid", convert it to generic AC tag
        /// - ACInstanceID - used to identify an AC tag on a page. Each instance of an AC tag must havea unique ACinstanceID
        /// </summary>
        /// <param name="core"></param>
        /// <param name="sourceHtmlContent"></param>
        /// <returns></returns>
        public static string processWysiwygResponseForSave(CoreController core, string sourceHtmlContent) {
            string result = sourceHtmlContent;
            try {
                if (!string.IsNullOrEmpty(result)) {
                    //
                    // leave this in to make sure old <acform tags are converted back, new editor deals with <form, so no more converting
                    result = GenericController.vbReplace(result, "<ACFORM>", "<FORM>");
                    result = GenericController.vbReplace(result, "<ACFORM ", "<FORM ");
                    result = GenericController.vbReplace(result, "</ACFORM>", "</form>");
                    result = GenericController.vbReplace(result, "</ACFORM ", "</FORM ");
                    HtmlParserController DHTML = new HtmlParserController(core);
                    if (DHTML.Load(result)) {
                        result = "";
                        int ElementCount = DHTML.ElementCount;
                        StringBuilderLegacyController Stream = new StringBuilderLegacyController();
                        if (ElementCount > 0) {
                            //
                            // ----- Locate and replace IMG Edit icons with {$ {} %} notation
                            Stream = new StringBuilderLegacyController();
                            int ElementPointer = 0;
                            for (ElementPointer = 0; ElementPointer < ElementCount; ElementPointer++) {
                                string ElementText = DHTML.Text(ElementPointer).ToString();
                                if (DHTML.IsTag(ElementPointer)) {
                                    int AttributeCount = 0;
                                    switch (GenericController.vbUCase(DHTML.TagName(ElementPointer))) {
                                        case "FORM":
                                            //
                                            // User created form - add the attribute "Contensive=1"
                                            //
                                            // 5/14/2009 - DM said it is OK to remove UserResponseForm Processing
                                            //ElementText = genericController.vbReplace(ElementText, "<FORM", "<FORM ContensiveUserForm=1 ", vbTextCompare)
                                            break;
                                        case "IMG":
                                            AttributeCount = DHTML.ElementAttributeCount(ElementPointer);

                                            if (AttributeCount > 0) {
                                                string ImageID = DHTML.ElementAttribute(ElementPointer, "id");
                                                string ImageSrcOriginal = DHTML.ElementAttribute(ElementPointer, "src");
                                                string VirtualFilePathBad = core.appConfig.name + "/files/";
                                                string serverFilePath = "/" + VirtualFilePathBad;
                                                if (ImageSrcOriginal.ToLower().Left(VirtualFilePathBad.Length) == GenericController.vbLCase(VirtualFilePathBad)) {
                                                    //
                                                    // if the image is from the virtual file path, but the editor did not include the root path, add it
                                                    //
                                                    ElementText = GenericController.vbReplace(ElementText, VirtualFilePathBad, "/" + VirtualFilePathBad, 1, 99, 1);
                                                    ImageSrcOriginal = GenericController.vbReplace(ImageSrcOriginal, VirtualFilePathBad, "/" + VirtualFilePathBad, 1, 99, 1);
                                                }
                                                string ImageSrc = HtmlController.decodeHtml(ImageSrcOriginal);
                                                ImageSrc = decodeURL(ImageSrc);
                                                //
                                                // problem with this case is if the addon icon image is from another site.
                                                // not sure how it happened, but I do not think the src of an addon edit icon
                                                // should be able to prevent the addon from executing.
                                                //
                                                string ACIdentifier = "";
                                                string ACType = "";
                                                string ACFieldName = "";
                                                string ACInstanceName = "";
                                                string ACGuid = "";
                                                int ImageIDArrayCount = 0;
                                                string ACQueryString = "";
                                                int Ptr = 0;
                                                string[] ImageIDArray = { };
                                                if (0 != GenericController.vbInstr(1, ImageID, ",")) {
                                                    ImageIDArray = ImageID.Split(',');
                                                    ImageIDArrayCount = ImageIDArray.GetUpperBound(0) + 1;
                                                    if (ImageIDArrayCount > 5) {
                                                        for (Ptr = 5; Ptr < ImageIDArrayCount; Ptr++) {
                                                            ACGuid = ImageIDArray[Ptr];
                                                            if ((ACGuid.Left(1) == "{") && (ACGuid.Substring(ACGuid.Length - 1) == "}")) {
                                                                //
                                                                // this element is the guid, go with it
                                                                //
                                                                break;
                                                            } else if ((string.IsNullOrEmpty(ACGuid)) && (Ptr == (ImageIDArrayCount - 1))) {
                                                                //
                                                                // this is the last element, leave it as the guid
                                                                //
                                                                break;
                                                            } else {
                                                                //
                                                                // not a valid guid, add it to element 4 and try the next
                                                                //
                                                                ImageIDArray[4] = ImageIDArray[4] + "," + ACGuid;
                                                                ACGuid = "";
                                                            }
                                                        }
                                                    }
                                                    if (ImageIDArrayCount > 1) {
                                                        ACIdentifier = GenericController.vbUCase(ImageIDArray[0]);
                                                        ACType = ImageIDArray[1];
                                                        if (ImageIDArrayCount > 2) {
                                                            ACFieldName = ImageIDArray[2];
                                                            if (ImageIDArrayCount > 3) {
                                                                ACInstanceName = ImageIDArray[3];
                                                                if (ImageIDArrayCount > 4) {
                                                                    ACQueryString = ImageIDArray[4];
                                                                    //If ImageIDArrayCount > 5 Then
                                                                    //    ACGuid = ImageIDArray(5)
                                                                    //End If
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                int Pos = 0;
                                                int RecordID = 0;
                                                string ImageStyle = null;
                                                if (ACIdentifier == "AC") {
                                                    if (true) {
                                                        if (true) {
                                                            //
                                                            // ----- Process AC Tag
                                                            //
                                                            string ACInstanceID = DHTML.ElementAttribute(ElementPointer, "ACINSTANCEID");
                                                            if (string.IsNullOrEmpty(ACInstanceID)) {
                                                                ACInstanceID = GenericController.getGUID();
                                                            }
                                                            ElementText = "";
                                                            string QueryString = null;
                                                            string[] QSSplit = null;
                                                            int QSPtr = 0;
                                                            //----------------------------- change to ACType
                                                            switch (GenericController.vbUCase(ACType)) {
                                                                case "IMAGE":
                                                                    //
                                                                    // ----- AC Image, Decode Active Images to Resource Library references
                                                                    //
                                                                    if (ImageIDArrayCount >= 4) {
                                                                        RecordID = GenericController.encodeInteger(ACInstanceName);
                                                                        string ImageWidthText = DHTML.ElementAttribute(ElementPointer, "WIDTH");
                                                                        string ImageHeightText = DHTML.ElementAttribute(ElementPointer, "HEIGHT");
                                                                        string ImageAlt = HtmlController.encodeHtml(DHTML.ElementAttribute(ElementPointer, "Alt"));
                                                                        int ImageVSpace = GenericController.encodeInteger(DHTML.ElementAttribute(ElementPointer, "vspace"));
                                                                        int ImageHSpace = GenericController.encodeInteger(DHTML.ElementAttribute(ElementPointer, "hspace"));
                                                                        string ImageAlign = DHTML.ElementAttribute(ElementPointer, "Align");
                                                                        string ImageBorder = DHTML.ElementAttribute(ElementPointer, "BORDER");
                                                                        string ImageLoop = DHTML.ElementAttribute(ElementPointer, "LOOP");
                                                                        ImageStyle = DHTML.ElementAttribute(ElementPointer, "STYLE");

                                                                        if (!string.IsNullOrEmpty(ImageStyle)) {
                                                                            //
                                                                            // ----- Process styles, which override attributes
                                                                            //
                                                                            string[] IMageStyleArray = ImageStyle.Split(';');
                                                                            int ImageStyleArrayCount = IMageStyleArray.GetUpperBound(0) + 1;
                                                                            int ImageStyleArrayPointer = 0;
                                                                            for (ImageStyleArrayPointer = 0; ImageStyleArrayPointer < ImageStyleArrayCount; ImageStyleArrayPointer++) {
                                                                                string ImageStylePair = IMageStyleArray[ImageStyleArrayPointer].Trim(' ');
                                                                                int PositionColon = GenericController.vbInstr(1, ImageStylePair, ":");
                                                                                if (PositionColon > 1) {
                                                                                    string ImageStylePairName = (ImageStylePair.Left(PositionColon - 1)).Trim(' ');
                                                                                    string ImageStylePairValue = (ImageStylePair.Substring(PositionColon)).Trim(' ');
                                                                                    switch (GenericController.vbUCase(ImageStylePairName)) {
                                                                                        case "WIDTH":
                                                                                            ImageStylePairValue = GenericController.vbReplace(ImageStylePairValue, "px", "");
                                                                                            ImageWidthText = ImageStylePairValue;
                                                                                            break;
                                                                                        case "HEIGHT":
                                                                                            ImageStylePairValue = GenericController.vbReplace(ImageStylePairValue, "px", "");
                                                                                            ImageHeightText = ImageStylePairValue;
                                                                                            break;
                                                                                    }
                                                                                    //If genericController.vbInstr(1, ImageStylePair, "WIDTH", vbTextCompare) = 1 Then
                                                                                    //    End If
                                                                                }
                                                                            }
                                                                        }
                                                                        ElementText = "<AC type=\"IMAGE\" ACInstanceID=\"" + ACInstanceID + "\" RecordID=\"" + RecordID + "\" Style=\"" + ImageStyle + "\" Width=\"" + ImageWidthText + "\" Height=\"" + ImageHeightText + "\" VSpace=\"" + ImageVSpace + "\" HSpace=\"" + ImageHSpace + "\" Alt=\"" + ImageAlt + "\" Align=\"" + ImageAlign + "\" Border=\"" + ImageBorder + "\" Loop=\"" + ImageLoop + "\">";
                                                                    }
                                                                    break;
                                                                //case ACTypeDownload:
                                                                //    //
                                                                //    // AC Download
                                                                //    //
                                                                //    if (ImageIDArrayCount >= 4) {
                                                                //        RecordID = genericController.encodeInteger(ACInstanceName);
                                                                //        ElementText = "<AC type=\"DOWNLOAD\" ACInstanceID=\"" + ACInstanceID + "\" RecordID=\"" + RecordID + "\">";
                                                                //    }
                                                                //    break;
                                                                //case ACTypeDate:
                                                                //    //
                                                                //    // Date
                                                                //    //
                                                                //    ElementText = "<AC type=\"" + ACTypeDate + "\">";
                                                                //    break;
                                                                //case ACTypeVisit:
                                                                //case ACTypeVisitor:
                                                                //case ACTypeMember:
                                                                //case ACTypeOrganization:
                                                                //case ACTypePersonalization:
                                                                //    //
                                                                //    // Visit, etc
                                                                //    //
                                                                //    ElementText = "<AC type=\"" + ACType + "\" ACInstanceID=\"" + ACInstanceID + "\" field=\"" + ACFieldName + "\">";
                                                                //    break;
                                                                ////case ACTypeChildList:
                                                                //case ACTypeLanguage:
                                                                //    //
                                                                //    // ChildList, Language
                                                                //    //
                                                                //    if (ACInstanceName == "0") {
                                                                //        ACInstanceName = genericController.GetRandomInteger(core).ToString();
                                                                //    }
                                                                //    ElementText = "<AC type=\"" + ACType + "\" name=\"" + ACInstanceName + "\" ACInstanceID=\"" + ACInstanceID + "\">";
                                                                //    break;
                                                                case ACTypeAggregateFunction:
                                                                    //
                                                                    // Function
                                                                    //
                                                                    QueryString = "";
                                                                    if (!string.IsNullOrEmpty(ACQueryString)) {
                                                                        // I added this because single stepping through it I found it split on the & in &amp;
                                                                        // I had added an Add-on and was saving
                                                                        // I find it VERY odd that this could be the case
                                                                        //
                                                                        string QSHTMLEncoded = GenericController.encodeText(ACQueryString);
                                                                        QueryString = HtmlController.decodeHtml(QSHTMLEncoded);
                                                                        QSSplit = QueryString.Split('&');
                                                                        for (QSPtr = 0; QSPtr <= QSSplit.GetUpperBound(0); QSPtr++) {
                                                                            Pos = GenericController.vbInstr(1, QSSplit[QSPtr], "[");
                                                                            if (Pos > 0) {
                                                                                QSSplit[QSPtr] = QSSplit[QSPtr].Left(Pos - 1);
                                                                            }
                                                                            QSSplit[QSPtr] = HtmlController.encodeHtml(QSSplit[QSPtr]);
                                                                        }
                                                                        QueryString = string.Join("&", QSSplit);
                                                                    }
                                                                    ElementText = "<AC type=\"" + ACType + "\" name=\"" + ACInstanceName + "\" ACInstanceID=\"" + ACInstanceID + "\" querystring=\"" + QueryString + "\" guid=\"" + ACGuid + "\">";
                                                                    break;
                                                                // c/*ase ACType*/Contact:
                                                                //case ACTypeFeedback:
                                                                //    //
                                                                //    // Contact and Feedback
                                                                //    //
                                                                //    ElementText = "<AC type=\"" + ACType + "\" ACInstanceID=\"" + ACInstanceID + "\">";
                                                                //    break;
                                                                case ACTypeTemplateContent:
                                                                case ACTypeTemplateText:
                                                                    //
                                                                    //
                                                                    //
                                                                    QueryString = "";
                                                                    if (ImageIDArrayCount > 4) {
                                                                        QueryString = GenericController.encodeText(ImageIDArray[4]);
                                                                        QSSplit = QueryString.Split('&');
                                                                        for (QSPtr = 0; QSPtr <= QSSplit.GetUpperBound(0); QSPtr++) {
                                                                            QSSplit[QSPtr] = HtmlController.encodeHtml(QSSplit[QSPtr]);
                                                                        }
                                                                        QueryString = string.Join("&", QSSplit);

                                                                    }
                                                                    ElementText = "<AC type=\"" + ACType + "\" name=\"" + ACInstanceName + "\" ACInstanceID=\"" + ACInstanceID + "\" querystring=\"" + QueryString + "\">";
                                                                    break;
                                                                //case ACTypeWatchList:
                                                                //    //
                                                                //    // Watch List
                                                                //    //
                                                                //    QueryString = "";
                                                                //    if (ImageIDArrayCount > 4) {
                                                                //        QueryString = genericController.encodeText(ImageIDArray[4]);
                                                                //        QueryString = HtmlController.decodeHtml(QueryString);
                                                                //        QSSplit = QueryString.Split('&');
                                                                //        for (QSPtr = 0; QSPtr <= QSSplit.GetUpperBound(0); QSPtr++) {
                                                                //            QSSplit[QSPtr] = HtmlController.encodeHtml(QSSplit[QSPtr]);
                                                                //        }
                                                                //        QueryString = string.Join("&", QSSplit);
                                                                //    }
                                                                //    ElementText = "<AC type=\"" + ACType + "\" name=\"" + ACInstanceName + "\" ACInstanceID=\"" + ACInstanceID + "\" querystring=\"" + QueryString + "\">";
                                                                //    break;
                                                                //case ACTypeRSSLink:
                                                                //    //
                                                                //    // RSS Link
                                                                //    //
                                                                //    QueryString = "";
                                                                //    if (ImageIDArrayCount > 4) {
                                                                //        QueryString = genericController.encodeText(ImageIDArray[4]);
                                                                //        QueryString = HtmlController.decodeHtml(QueryString);
                                                                //        QSSplit = QueryString.Split('&');
                                                                //        for (QSPtr = 0; QSPtr <= QSSplit.GetUpperBound(0); QSPtr++) {
                                                                //            QSSplit[QSPtr] = HtmlController.encodeHtml(QSSplit[QSPtr]);
                                                                //        }
                                                                //        QueryString = string.Join("&", QSSplit);
                                                                //    }
                                                                //    ElementText = "<AC type=\"" + ACType + "\" name=\"" + ACInstanceName + "\" ACInstanceID=\"" + ACInstanceID + "\" querystring=\"" + QueryString + "\">";
                                                                //    break;
                                                                default:
                                                                    //
                                                                    // All others -- added querystring from element(4) to all others to cover the group access AC object
                                                                    //
                                                                    QueryString = "";
                                                                    if (ImageIDArrayCount > 4) {
                                                                        QueryString = GenericController.encodeText(ImageIDArray[4]);
                                                                        QueryString = HtmlController.decodeHtml(QueryString);
                                                                        QSSplit = QueryString.Split('&');
                                                                        for (QSPtr = 0; QSPtr <= QSSplit.GetUpperBound(0); QSPtr++) {
                                                                            QSSplit[QSPtr] = HtmlController.encodeHtml(QSSplit[QSPtr]);
                                                                        }
                                                                        QueryString = string.Join("&", QSSplit);
                                                                    }
                                                                    ElementText = "<AC type=\"" + ACType + "\" name=\"" + ACInstanceName + "\" ACInstanceID=\"" + ACInstanceID + "\" field=\"" + ACFieldName + "\" querystring=\"" + QueryString + "\">";
                                                                    break;
                                                            }
                                                        }
                                                    }
                                                } else if (GenericController.vbInstr(1, ImageSrc, "cclibraryfiles", 1) != 0) {
                                                    bool ImageAllowSFResize = core.siteProperties.getBoolean("ImageAllowSFResize", true);
                                                    if (ImageAllowSFResize && true) {
                                                        //
                                                        // if it is a real image, check for resize
                                                        //
                                                        Pos = GenericController.vbInstr(1, ImageSrc, "cclibraryfiles", 1);
                                                        if (Pos != 0) {
                                                            string ImageVirtualFilename = ImageSrc.Substring(Pos - 1);
                                                            string[] Paths = ImageVirtualFilename.Split('/');
                                                            if (Paths.GetUpperBound(0) > 2) {
                                                                if (GenericController.vbLCase(Paths[1]) == "filename") {
                                                                    RecordID = GenericController.encodeInteger(Paths[2]);
                                                                    if (RecordID != 0) {
                                                                        string ImageFilename = Paths[3];
                                                                        string ImageVirtualFilePath = GenericController.vbReplace(ImageVirtualFilename, ImageFilename, "");
                                                                        Pos = ImageFilename.LastIndexOf(".") + 1;
                                                                        if (Pos > 0) {
                                                                            string ImageFilenameAltSize = "";
                                                                            string ImageFilenameExt = ImageFilename.Substring(Pos);
                                                                            string ImageFilenameNoExt = ImageFilename.Left(Pos - 1);
                                                                            Pos = ImageFilenameNoExt.LastIndexOf("-") + 1;
                                                                            if (Pos > 0) {
                                                                                //
                                                                                // ImageAltSize should be set from the width and height of the img tag,
                                                                                // NOT from the actual width and height of the image file
                                                                                // NOT from the suffix of the image filename
                                                                                // ImageFilenameAltSize is used when the image has been resized, then 'reset' was hit
                                                                                //  on the properties dialog before the save. The width and height come from this suffix
                                                                                //
                                                                                ImageFilenameAltSize = ImageFilenameNoExt.Substring(Pos);
                                                                                string[] SizeTest = ImageFilenameAltSize.Split('x');
                                                                                if (SizeTest.GetUpperBound(0) != 1) {
                                                                                    ImageFilenameAltSize = "";
                                                                                } else {
                                                                                    if ((SizeTest[0].IsNumeric() & SizeTest[1].IsNumeric())) {
                                                                                        ImageFilenameNoExt = ImageFilenameNoExt.Left(Pos - 1);
                                                                                        //RecordVirtualFilenameNoExt = Mid(RecordVirtualFilename, 1, Pos - 1)
                                                                                    } else {
                                                                                        ImageFilenameAltSize = "";
                                                                                    }
                                                                                }
                                                                                //ImageFilenameNoExt = Mid(ImageFilenameNoExt, 1, Pos - 1)
                                                                            }
                                                                            if (GenericController.vbInstr(1, sfImageExtList, ImageFilenameExt, 1) != 0) {
                                                                                //
                                                                                // Determine ImageWidth and ImageHeight
                                                                                //
                                                                                ImageStyle = DHTML.ElementAttribute(ElementPointer, "style");
                                                                                int ImageWidth = GenericController.encodeInteger(DHTML.ElementAttribute(ElementPointer, "width"));
                                                                                int ImageHeight = GenericController.encodeInteger(DHTML.ElementAttribute(ElementPointer, "height"));
                                                                                if (!string.IsNullOrEmpty(ImageStyle)) {
                                                                                    string[] Styles = ImageStyle.Split(';');
                                                                                    for (Ptr = 0; Ptr <= Styles.GetUpperBound(0); Ptr++) {
                                                                                        string[] Style = Styles[Ptr].Split(':');
                                                                                        if (Style.GetUpperBound(0) > 0) {
                                                                                            string StyleName = GenericController.vbLCase(Style[0].Trim(' '));
                                                                                            string StyleValue = null;
                                                                                            int StyleValueInt = 0;
                                                                                            if (StyleName == "width") {
                                                                                                StyleValue = GenericController.vbLCase(Style[1].Trim(' '));
                                                                                                StyleValue = GenericController.vbReplace(StyleValue, "px", "");
                                                                                                StyleValueInt = GenericController.encodeInteger(StyleValue);
                                                                                                if (StyleValueInt > 0) {
                                                                                                    ImageWidth = StyleValueInt;
                                                                                                }
                                                                                            } else if (StyleName == "height") {
                                                                                                StyleValue = GenericController.vbLCase(Style[1].Trim(' '));
                                                                                                StyleValue = GenericController.vbReplace(StyleValue, "px", "");
                                                                                                StyleValueInt = GenericController.encodeInteger(StyleValue);
                                                                                                if (StyleValueInt > 0) {
                                                                                                    ImageHeight = StyleValueInt;
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                                //
                                                                                // Get the record values
                                                                                //
                                                                                LibraryFilesModel file = LibraryFilesModel.create(core, RecordID);
                                                                                if (file != null) {
                                                                                    string RecordVirtualFilename = file.filename;
                                                                                    int RecordWidth = file.width;
                                                                                    int RecordHeight = file.height;
                                                                                    string RecordAltSizeList = file.altSizeList;
                                                                                    string RecordFilename = RecordVirtualFilename;
                                                                                    Pos = RecordVirtualFilename.LastIndexOf("/") + 1;
                                                                                    if (Pos > 0) {
                                                                                        RecordFilename = RecordVirtualFilename.Substring(Pos);
                                                                                    }
                                                                                    string RecordFilenameExt = "";
                                                                                    string RecordFilenameNoExt = RecordFilename;
                                                                                    Pos = RecordFilenameNoExt.LastIndexOf(".") + 1;
                                                                                    if (Pos > 0) {
                                                                                        RecordFilenameExt = RecordFilenameNoExt.Substring(Pos);
                                                                                        RecordFilenameNoExt = RecordFilenameNoExt.Left(Pos - 1);
                                                                                    }
                                                                                    ImageEditController sf = null;
                                                                                    //
                                                                                    // if recordwidth or height are missing, get them from the file
                                                                                    //
                                                                                    if (RecordWidth == 0 || RecordHeight == 0) {
                                                                                        sf = new ImageEditController();
                                                                                        if (sf.load(ImageVirtualFilename, core.cdnFiles)) {
                                                                                            file.width = sf.width;
                                                                                            file.height = sf.height;
                                                                                            file.save(core);
                                                                                        }
                                                                                        sf.Dispose();
                                                                                        sf = null;
                                                                                    }
                                                                                    //
                                                                                    // continue only if we have record width and height
                                                                                    //
                                                                                    if (RecordWidth != 0 & RecordHeight != 0) {
                                                                                        //
                                                                                        // set ImageWidth and ImageHeight if one of them is missing
                                                                                        //
                                                                                        if ((ImageWidth == RecordWidth) && (ImageHeight == 0)) {
                                                                                            //
                                                                                            // Image only included width, set default height
                                                                                            //
                                                                                            ImageHeight = RecordHeight;
                                                                                        } else if ((ImageHeight == RecordHeight) && (ImageWidth == 0)) {
                                                                                            //
                                                                                            // Image only included height, set default width
                                                                                            //
                                                                                            ImageWidth = RecordWidth;
                                                                                        } else if ((ImageHeight == 0) && (ImageWidth == 0)) {
                                                                                            //
                                                                                            // Image has no width or height, default both
                                                                                            // This happens when you hit 'reset' on the image properties dialog
                                                                                            //
                                                                                            sf = new ImageEditController();
                                                                                            if (sf.load(ImageVirtualFilename, core.cdnFiles)) {
                                                                                                ImageWidth = sf.width;
                                                                                                ImageHeight = sf.height;
                                                                                            }
                                                                                            sf.Dispose();
                                                                                            sf = null;
                                                                                            if ((ImageHeight == 0) && (ImageWidth == 0) && (!string.IsNullOrEmpty(ImageFilenameAltSize))) {
                                                                                                Pos = GenericController.vbInstr(1, ImageFilenameAltSize, "x");
                                                                                                if (Pos != 0) {
                                                                                                    ImageWidth = GenericController.encodeInteger(ImageFilenameAltSize.Left(Pos - 1));
                                                                                                    ImageHeight = GenericController.encodeInteger(ImageFilenameAltSize.Substring(Pos));
                                                                                                }
                                                                                            }
                                                                                            if (ImageHeight == 0 && ImageWidth == 0) {
                                                                                                ImageHeight = RecordHeight;
                                                                                                ImageWidth = RecordWidth;
                                                                                            }
                                                                                        }
                                                                                        //
                                                                                        // Set the ImageAltSize to what was requested from the img tag
                                                                                        // if the actual image is a few rounding-error pixels off does not matter
                                                                                        // if either is 0, let altsize be 0, set real value for image height/width
                                                                                        //
                                                                                        string ImageAltSize = ImageWidth.ToString() + "x" + ImageHeight.ToString();
                                                                                        string NewImageFilename = null;
                                                                                        //
                                                                                        // determine if we are OK, or need to rebuild
                                                                                        //
                                                                                        if ((RecordVirtualFilename == (ImageVirtualFilePath + ImageFilename)) && ((RecordWidth == ImageWidth) || (RecordHeight == ImageHeight))) {
                                                                                            //
                                                                                            // OK
                                                                                            // this is the raw image
                                                                                            // image matches record, and the sizes are the same
                                                                                            //
                                                                                            //RecordVirtualFilename = RecordVirtualFilename;
                                                                                        } else if ((RecordVirtualFilename == ImageVirtualFilePath + ImageFilenameNoExt + "." + ImageFilenameExt) && (RecordAltSizeList.IndexOf(ImageAltSize, System.StringComparison.OrdinalIgnoreCase) != -1)) {
                                                                                            //
                                                                                            // OK
                                                                                            // resized image, and altsize is in the list - go with resized image name
                                                                                            //
                                                                                            NewImageFilename = ImageFilenameNoExt + "-" + ImageAltSize + "." + ImageFilenameExt;
                                                                                            // images included in email have spaces that must be converted to "%20" or they 404
                                                                                            string imageNewLink = GenericController.encodeURL(GenericController.getCdnFileLink(core, ImageVirtualFilePath) + NewImageFilename);
                                                                                            ElementText = GenericController.vbReplace(ElementText, ImageSrcOriginal, HtmlController.encodeHtml(imageNewLink));
                                                                                        } else if ((RecordWidth < ImageWidth) || (RecordHeight < ImageHeight)) {
                                                                                            //
                                                                                            // OK
                                                                                            // reize image larger then original - go with it as is
                                                                                            //
                                                                                            // images included in email have spaces that must be converted to "%20" or they 404
                                                                                            ElementText = GenericController.vbReplace(ElementText, ImageSrcOriginal, HtmlController.encodeHtml(GenericController.encodeURL(GenericController.getCdnFileLink(core, RecordVirtualFilename))));
                                                                                        } else {
                                                                                            //
                                                                                            // resized image - create NewImageFilename (and add new alt size to the record)
                                                                                            //
                                                                                            if (RecordWidth == ImageWidth && RecordHeight == ImageHeight) {
                                                                                                //
                                                                                                // set back to Raw image untouched, use the record image filename
                                                                                                //
                                                                                                //ElementText = ElementText;
                                                                                                //ElementText = genericController.vbReplace(ElementText, ImageVirtualFilename, RecordVirtualFilename)
                                                                                            } else {
                                                                                                //
                                                                                                // Raw image filename in content, but it is resized, switch to an alternate size
                                                                                                //
                                                                                                NewImageFilename = RecordFilename;
                                                                                                if ((ImageWidth == 0) || (ImageHeight == 0)) {
                                                                                                    //
                                                                                                    // Alt image has not been built
                                                                                                    //
                                                                                                    sf = new ImageEditController();
                                                                                                    if (!sf.load(RecordVirtualFilename, core.cdnFiles)) {
                                                                                                        //
                                                                                                        // image load failed, use raw filename
                                                                                                        //
                                                                                                        throw (new ApplicationException("Unexpected exception")); //core.handleLegacyError3(core.appConfig.name, "Error while loading image to resize, [" & RecordVirtualFilename & "]", "dll", "coreClass", "DecodeAciveContent", Err.Number, Err.Source, Err.Description, False, True, "")
                                                                                                    } else {
                                                                                                        //
                                                                                                        //
                                                                                                        //
                                                                                                        RecordWidth = sf.width;
                                                                                                        RecordHeight = sf.height;
                                                                                                        if (ImageWidth == 0) {
                                                                                                            //
                                                                                                            //
                                                                                                            //
                                                                                                            sf.height = ImageHeight;
                                                                                                        } else if (ImageHeight == 0) {
                                                                                                            //
                                                                                                            //
                                                                                                            //
                                                                                                            sf.width = ImageWidth;
                                                                                                        } else if (RecordHeight == ImageHeight) {
                                                                                                            //
                                                                                                            // change the width
                                                                                                            //
                                                                                                            sf.width = ImageWidth;
                                                                                                        } else {
                                                                                                            //
                                                                                                            // change the height
                                                                                                            //
                                                                                                            sf.height = ImageHeight;
                                                                                                        }
                                                                                                        //
                                                                                                        // if resized only width or height, set the other
                                                                                                        //
                                                                                                        if (ImageWidth == 0) {
                                                                                                            ImageWidth = sf.width;
                                                                                                            ImageAltSize = ImageWidth.ToString() + "x" + ImageHeight.ToString();
                                                                                                        }
                                                                                                        if (ImageHeight == 0) {
                                                                                                            ImageHeight = sf.height;
                                                                                                            ImageAltSize = ImageWidth.ToString() + "x" + ImageHeight.ToString();
                                                                                                        }
                                                                                                        //
                                                                                                        // set HTML attributes so image properties will display
                                                                                                        //
                                                                                                        if (GenericController.vbInstr(1, ElementText, "height=", 1) == 0) {
                                                                                                            ElementText = GenericController.vbReplace(ElementText, ">", " height=\"" + ImageHeight + "\">");
                                                                                                        }
                                                                                                        if (GenericController.vbInstr(1, ElementText, "width=", 1) == 0) {
                                                                                                            ElementText = GenericController.vbReplace(ElementText, ">", " width=\"" + ImageWidth + "\">");
                                                                                                        }
                                                                                                        //
                                                                                                        // Save new file
                                                                                                        //
                                                                                                        NewImageFilename = RecordFilenameNoExt + "-" + ImageAltSize + "." + RecordFilenameExt;
                                                                                                        sf.save(ImageVirtualFilePath + NewImageFilename, core.cdnFiles);
                                                                                                        //
                                                                                                        // Update image record
                                                                                                        //
                                                                                                        RecordAltSizeList = RecordAltSizeList + "\r\n" + ImageAltSize;
                                                                                                    }
                                                                                                }
                                                                                                //
                                                                                                // Change the image src to the AltSize
                                                                                                ElementText = GenericController.vbReplace(ElementText, ImageSrcOriginal, HtmlController.encodeHtml(GenericController.encodeURL(GenericController.getCdnFileLink(core, ImageVirtualFilePath) + NewImageFilename)));
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                    file.save(core);
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
                                            break;
                                    }
                                }
                                Stream.Add(ElementText);
                            }
                        }
                        result = Stream.Text;
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
            return result;
            //
        }
        //
        //===================================================================================================
        /// <summary>
        /// Internal routine to render htmlContent.
        /// </summary>
        /// <param name="core"></param>
        /// <param name="sourceHtmlContent"></param>
        /// <param name="personalizationPeopleId"></param>
        /// <param name="ContextContentName"></param>
        /// <param name="ContextRecordID"></param>
        /// <param name="ContextContactPeopleID"></param>
        /// <param name="convertHtmlToText">if true, the html source will be converted to plain text.</param>
        /// <param name="addLinkAuthToAllLinks"></param>
        /// <param name="EncodeActiveFormatting"></param>
        /// <param name="EncodeActiveImages"></param>
        /// <param name="EncodeActiveEditIcons"></param>
        /// <param name="EncodeActivePersonalization"></param>
        /// <param name="queryStringForLinkAppend"></param>
        /// <param name="ProtocolHostLink"></param>
        /// <param name="IsEmailContent"></param>
        /// <param name="ignore_DefaultWrapperID"></param>
        /// <param name="ignore_TemplateCaseOnly_Content"></param>
        /// <param name="Context"></param>
        /// <param name="personalizationIsAuthenticated"></param>
        /// <param name="nothingObject"></param>
        /// <param name="isEditingAnything"></param>
        /// <returns></returns>
        //
        public static string encode(CoreController core, string sourceHtmlContent, int personalizationPeopleId, string ContextContentName, int ContextRecordID, int ContextContactPeopleID, bool convertHtmlToText, bool addLinkAuthToAllLinks, bool EncodeActiveFormatting, bool EncodeActiveImages, bool EncodeActiveEditIcons, bool EncodeActivePersonalization, string queryStringForLinkAppend, string ProtocolHostLink, bool IsEmailContent, int ignore_DefaultWrapperID, string ignore_TemplateCaseOnly_Content, CPUtilsBaseClass.addonContext Context, bool personalizationIsAuthenticated, object nothingObject, bool isEditingAnything) {
            string result = sourceHtmlContent;
            try {
                if (!string.IsNullOrEmpty(sourceHtmlContent)) {
                    int LineStart = 0;
                    //
                    if (personalizationPeopleId <= 0) {
                        personalizationPeopleId = core.session.user.id;
                    }
                    // 20180124 removed, cannot find a use case for this
                    //if (core.siteProperties.getBoolean("ConvertContentCRLF2BR", false) && (!convertHtmlToText)) {
                    //    result = genericController.vbReplace(result, "\r", "");
                    //    result = genericController.vbReplace(result, "\n", "<br>");
                    //}
                    //
                    // Convert ACTypeDynamicForm to Add-on
                    //if (genericController.vbInstr(1, result, "<ac type=\"" + ACTypeDynamicForm, 1) != 0) {
                    //    result = genericController.vbReplace(result, "type=\"DYNAMICFORM\"", "TYPE=\"aggregatefunction\"", 1, 99, 1);
                    //    result = genericController.vbReplace(result, "name=\"DYNAMICFORM\"", "name=\"DYNAMIC FORM\"", 1, 99, 1);
                    //}
                    //
                    // -- resize images
                    result = optimizeLibraryFileImagesInHtmlContent(core, result);
                    //
                    // -- Do Active Content Conversion
                    if (addLinkAuthToAllLinks || EncodeActiveFormatting || EncodeActiveImages || EncodeActiveEditIcons) {
                        string AdminURL = "/" + core.appConfig.adminRoute;
                        result = renderActiveContent(core, result, personalizationPeopleId, ContextContentName, ContextRecordID, ContextContactPeopleID, addLinkAuthToAllLinks, EncodeActiveFormatting, EncodeActiveImages, EncodeActiveEditIcons, EncodeActivePersonalization, queryStringForLinkAppend, ProtocolHostLink, IsEmailContent, AdminURL, personalizationIsAuthenticated, Context);
                    }
                    //
                    // -- Do Plain Text Conversion
                    if (convertHtmlToText) {
                        result = NUglify.Uglify.HtmlToText(result).Code; // htmlToTextControllers.convert(core, result);
                    }
                    //
                    // -- Process Active Content that must be run here to access webclass objects parse as {{functionname?querystring}}
                    //if ((!EncodeActiveEditIcons) && (result.IndexOf("{{") != -1)) {
                    //    string[] ContentSplit = genericController.stringSplit(result, "{{");
                    //    result = "";
                    //    int ContentSplitCnt = ContentSplit.GetUpperBound(0) + 1;
                    //    int Ptr = 0;
                    //    while (Ptr < ContentSplitCnt) {
                    //        //hint = hint & ",200"
                    //        string Segment = ContentSplit[Ptr];
                    //        if (Ptr == 0) {
                    //            //
                    //            // Add in the non-command text that is before the first command
                    //            //
                    //            result += Segment;
                    //        } else if (!string.IsNullOrEmpty(Segment)) {
                    //            if (genericController.vbInstr(1, Segment, "}}") == 0) {
                    //                //
                    //                // No command found, return the marker and deliver the Segment
                    //                //
                    //                //hint = hint & ",210"
                    //                result += "{{" + Segment;
                    //            } else {
                    //                //
                    //                // isolate the command
                    //                //
                    //                //hint = hint & ",220"
                    //                string[] SegmentSplit = genericController.stringSplit(Segment, "}}");
                    //                string AcCmd = SegmentSplit[0];
                    //                SegmentSplit[0] = "";
                    //                string SegmentSuffix = string.Join("}}", SegmentSplit).Substring(2);
                    //                if (!string.IsNullOrEmpty(AcCmd.Trim(' '))) {
                    //                    //
                    //                    // isolate the arguments
                    //                    //
                    //                    //hint = hint & ",230"
                    //                    string[] AcCmdSplit = AcCmd.Split('?');
                    //                    string ACType = AcCmdSplit[0].Trim(' ');
                    //                    string addonOptionString = "";
                    //                    if (AcCmdSplit.GetUpperBound(0) == 0) {
                    //                        addonOptionString = "";
                    //                    } else {
                    //                        addonOptionString = AcCmdSplit[1];
                    //                        addonOptionString = HtmlController.decodeHtml(addonOptionString);
                    //                    }
                    //                    //
                    //                    // execute the command
                    //                    //
                    //                    switch (genericController.vbUCase(ACType)) {
                    //                        //case ACTypeDynamicForm:
                    //                        //    //
                    //                        //    // Dynamic Form - run the core addon replacement instead
                    //                        //    CPUtilsBaseClass.addonExecuteContext executeContext = new CPUtilsBaseClass.addonExecuteContext() {
                    //                        //        addonType = CPUtilsBaseClass.addonContext.ContextPage,
                    //                        //        cssContainerClass = "",
                    //                        //        cssContainerId = "",
                    //                        //        hostRecord = new CPUtilsBaseClass.addonExecuteHostRecordContext() {
                    //                        //            contentName = ContextContentName,
                    //                        //            fieldName = "",
                    //                        //            recordId = ContextRecordID
                    //                        //        },
                    //                        //        personalizationAuthenticated = personalizationIsAuthenticated,
                    //                        //        personalizationPeopleId = personalizationPeopleId,
                    //                        //        instanceArguments = genericController.convertAddonArgumentstoDocPropertiesList(core, addonOptionString),
                    //                        //        errorContextMessage = "rendering a dynamic form found in active content"
                    //                        //    };
                    //                        //    AddonModel addon = AddonModel.create(core, addonGuidDynamicForm);
                    //                        //    result += core.addon.execute(addon, executeContext);
                    //                        //    break;
                    //                        //case ACTypeChildList:
                    //                        //    //
                    //                        //    // Child Page List
                    //                        //    //
                    //                        //    //hint = hint & ",320"
                    //                        //    string ListName = addonController.getAddonOption("name", addonOptionString);
                    //                        //    result += pageContentController.getChildPageList(core, ListName, ContextContentName, ContextRecordID, true);
                    //                        //    break;
                    //                        case ACTypeTemplateText:
                    //                            //
                    //                            // Text Box = copied here from gethtmlbody
                    //                            //
                    //                            string CopyName = AddonController.getAddonOption("new", addonOptionString);
                    //                            if (string.IsNullOrEmpty(CopyName)) {
                    //                                CopyName = AddonController.getAddonOption("name", addonOptionString);
                    //                                if (string.IsNullOrEmpty(CopyName)) {
                    //                                    CopyName = "Default";
                    //                                }
                    //                            }
                    //                            result += core.html.getContentCopy(CopyName, "", personalizationPeopleId, false, personalizationIsAuthenticated);
                    //                            break;
                    //                        //case ACTypeWatchList:
                    //                        //    //
                    //                        //    // Watch List
                    //                        //    //
                    //                        //    //hint = hint & ",330"
                    //                        //    string ListName = AddonController.getAddonOption("LISTNAME", addonOptionString);
                    //                        //    string SortField = AddonController.getAddonOption("SORTFIELD", addonOptionString);
                    //                        //    bool SortReverse = genericController.encodeBoolean(AddonController.getAddonOption("SORTDIRECTION", addonOptionString));
                    //                        //    result += core.doc.main_GetWatchList(core, ListName, SortField, SortReverse);
                    //                        //    break;
                    //                        default:
                    //                            //
                    //                            // Unrecognized command - put all the syntax back in
                    //                            //
                    //                            //hint = hint & ",340"
                    //                            result += "{{" + AcCmd + "}}";
                    //                            break;
                    //                    }
                    //                }
                    //                //
                    //                // add the SegmentSuffix back on
                    //                //
                    //                result += SegmentSuffix;
                    //            }
                    //        }
                    //        //
                    //        // Encode into Javascript if required
                    //        //
                    //        Ptr = Ptr + 1;
                    //    }
                    //}
                    //
                    // Process Addons
                    //   parse as <!-- Addon "Addon Name","OptionString" -->
                    //   They are handled here because Addons are written against coreClass, not the Content Server class
                    //   ...so Group Email can not process addons 8(
                    //   Later, remove the csv routine that translates <ac to this, and process it directly right here
                    //   Later, rewrite so addons call csv, not coreClass, so email processing can include addons
                    // (2/16/2010) - move csv_EncodeContent to csv, or wait and move it all to CP
                    //    eventually, everything should migrate to csv and/or cp to eliminate the coreClass dependancy
                    //    and all add-ons run as processes the same as they run on pages, or as remote methods
                    // (2/16/2010) - if <!-- AC --> has four arguments, the fourth is the addon guid
                    //
                    // todo - deprecate execute addons based on this comment system "<!-- addon"
                    const string StartFlag = "<!-- ADDON";
                    const string EndFlag = " -->";
                    if (result.IndexOf(StartFlag) != -1) {
                        int LineEnd = 0;
                        while (result.IndexOf(StartFlag) != -1) {
                            LineStart = GenericController.vbInstr(1, result, StartFlag);
                            LineEnd = GenericController.vbInstr(LineStart, result, EndFlag);
                            string Copy = "";
                            if (LineEnd == 0) {
                                LogController.logWarn(core, "csv_EncodeContent9, Addon could not be inserted into content because the HTML comment holding the position is not formated correctly");
                                break;
                            } else {
                                string AddonName = "";
                                string addonOptionString = "";
                                string ACInstanceID = "";
                                string AddonGuid = "";
                                int copyLength = LineEnd - LineStart - 11;
                                if(copyLength<=0) {
                                    //
                                    // -- nothing between start and end, someone added a comment <!-- ADDON -->
                                } else {
                                    Copy = result.Substring(LineStart + 10, copyLength);
                                    string[] ArgSplit = GenericController.SplitDelimited(Copy, ",");
                                    int ArgCnt = ArgSplit.GetUpperBound(0) + 1;
                                    if (!string.IsNullOrEmpty(ArgSplit[0])) {
                                        AddonName = ArgSplit[0].Substring(1, ArgSplit[0].Length - 2);
                                        if (ArgCnt > 1) {
                                            if (!string.IsNullOrEmpty(ArgSplit[1])) {
                                                addonOptionString = ArgSplit[1].Substring(1, ArgSplit[1].Length - 2);
                                                addonOptionString = HtmlController.decodeHtml(addonOptionString.Trim(' '));
                                            }
                                            if (ArgCnt > 2) {
                                                if (!string.IsNullOrEmpty(ArgSplit[2])) {
                                                    ACInstanceID = ArgSplit[2].Substring(1, ArgSplit[2].Length - 2);
                                                }
                                                if (ArgCnt > 3) {
                                                    if (!string.IsNullOrEmpty(ArgSplit[3])) {
                                                        AddonGuid = ArgSplit[3].Substring(1, ArgSplit[3].Length - 2);
                                                    }
                                                }
                                            }
                                        }
                                        // dont have any way of getting fieldname yet

                                        CPUtilsBaseClass.addonExecuteContext executeContext = new CPUtilsBaseClass.addonExecuteContext() {
                                            addonType = CPUtilsBaseClass.addonContext.ContextPage,
                                            cssContainerClass = "",
                                            cssContainerId = "",
                                            hostRecord = new CPUtilsBaseClass.addonExecuteHostRecordContext() {
                                                contentName = ContextContentName,
                                                fieldName = "",
                                                recordId = ContextRecordID
                                            },
                                            personalizationAuthenticated = personalizationIsAuthenticated,
                                            personalizationPeopleId = personalizationPeopleId,
                                            instanceGuid = ACInstanceID,
                                            instanceArguments = GenericController.convertAddonArgumentstoDocPropertiesList(core, addonOptionString),
                                            errorContextMessage = "rendering active content with guid [" + AddonGuid + "] or name [" + AddonName + "]"
                                        };
                                        if (!string.IsNullOrEmpty(AddonGuid)) {
                                            Copy = core.addon.execute(AddonModel.create(core, AddonGuid), executeContext);
                                        } else {
                                            Copy = core.addon.execute(AddonModel.createByUniqueName(core, AddonName), executeContext);
                                        }
                                    }
                                }
                            }
                            result = result.Left(LineStart - 1) + Copy + result.Substring(LineEnd + 3);
                        }
                    }
                    //
                    // process out text block comments inserted by addons
                    // remove all content between BlockTextStartMarker and the next BlockTextEndMarker, or end of copy
                    // exception made for the content with just the startmarker because when the AC tag is replaced with
                    // with the marker, encode content is called with the result, which is just the marker, and this
                    // section will remove it
                    //
                    bool DoAnotherPass = false;
                    if ((!isEditingAnything) && (result != BlockTextStartMarker)) {
                        DoAnotherPass = true;
                        while ((result.IndexOf(BlockTextStartMarker, System.StringComparison.OrdinalIgnoreCase) != -1) && DoAnotherPass) {
                            LineStart = GenericController.vbInstr(1, result, BlockTextStartMarker, 1);
                            if (LineStart == 0) {
                                DoAnotherPass = false;
                            } else {
                                int LineEnd = GenericController.vbInstr(LineStart, result, BlockTextEndMarker, 1);
                                if (LineEnd <= 0) {
                                    DoAnotherPass = false;
                                    result = result.Left(LineStart - 1);
                                } else {
                                    LineEnd = GenericController.vbInstr(LineEnd, result, " -->");
                                    if (LineEnd <= 0) {
                                        DoAnotherPass = false;
                                    } else {
                                        result = result.Left(LineStart - 1) + result.Substring(LineEnd + 3);
                                        //returnValue = Mid(returnValue, 1, LineStart - 1) & Copy & Mid(returnValue, LineEnd + 4)
                                    }
                                }
                            }
                        }
                    }
                    if (isEditingAnything) {
                        if (result.IndexOf("<!-- AFScript -->", System.StringComparison.OrdinalIgnoreCase) != -1) {
                            string Copy = AdminUIController.getEditWrapper(core, "Aggregate Script", "##MARKER##");
                            string[] Wrapper = GenericController.stringSplit(Copy, "##MARKER##");
                            result = GenericController.vbReplace(result, "<!-- AFScript -->", Wrapper[0], 1, 99, 1);
                            result = GenericController.vbReplace(result, "<!-- /AFScript -->", Wrapper[1], 1, 99, 1);
                        }
                        if (result.IndexOf("<!-- AFReplacement -->", System.StringComparison.OrdinalIgnoreCase) != -1) {
                            string Copy = AdminUIController.getEditWrapper(core, "Aggregate Replacement", "##MARKER##");
                            string[] Wrapper = GenericController.stringSplit(Copy, "##MARKER##");
                            result = GenericController.vbReplace(result, "<!-- AFReplacement -->", Wrapper[0], 1, 99, 1);
                            result = GenericController.vbReplace(result, "<!-- /AFReplacement -->", Wrapper[1], 1, 99, 1);
                        }
                    }
                    //
                    // Process Feedback form
                    if (GenericController.vbInstr(1, result, FeedbackFormNotSupportedComment, 1) != 0) {
                        result = GenericController.vbReplace(result, FeedbackFormNotSupportedComment, PageContentController.getFeedbackForm(core, ContextContentName, ContextRecordID, ContextContactPeopleID), 1, 99, 1);
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
            return result;
        }
        //
        //================================================================================================================
        /// <summary>
        /// for html content, this routine optimizes images referenced in the html if they are from library file
        /// </summary>
        public static string optimizeLibraryFileImagesInHtmlContent(CoreController core, string htmlContent) {
            string result = htmlContent;
            try {
                // todo - upgradeActiveContent runs every render, can it be eliminated/minimized
                string ContentFilesLinkPrefix = ContentFilesLinkPrefix = "/" + core.appConfig.name + "/files/";
                string ResourceLibraryLinkPrefix = ContentFilesLinkPrefix + "ccLibraryFiles/";
                bool ImageAllowUpdate = core.siteProperties.getBoolean("ImageAllowUpdate", true);
                ImageAllowUpdate = ImageAllowUpdate && (htmlContent.IndexOf(ResourceLibraryLinkPrefix, System.StringComparison.OrdinalIgnoreCase) != -1);
                bool SaveChanges = false;
                bool ParseError = false;
                if (ImageAllowUpdate) {
                    //
                    // ----- Process Resource Library Images (swap in most current file)
                    string[] LinkSplit = GenericController.stringSplit(htmlContent, ContentFilesLinkPrefix);
                    int LinkCnt = LinkSplit.GetUpperBound(0) + 1;
                    bool InTag = false;
                    for (int LinkPtr = 1; LinkPtr < LinkCnt; LinkPtr++) {
                        //
                        // Each LinkSplit(1...) is a segment that would have started with '/appname/files/'
                        // Next job is to determine if this sement is in a tag (<img src="...">) or in content (&quot...&quote)
                        // For now, skip the ones in content
                        int TagPosEnd = GenericController.vbInstr(1, LinkSplit[LinkPtr], ">");
                        int TagPosStart = GenericController.vbInstr(1, LinkSplit[LinkPtr], "<");
                        if (TagPosEnd == 0 && TagPosStart == 0) {
                            //
                            // no tags found, skip it
                            InTag = false;
                        } else if (TagPosEnd == 0) {
                            //
                            // no end tag, but found a start tag -> in content
                            InTag = false;
                        } else if (TagPosEnd < TagPosStart) {
                            //
                            // Found end before start - > in tag
                            InTag = true;
                        } else {
                            //
                            // Found start before end -> in content
                            InTag = false;
                        }
                        if (InTag) {
                            string[] TableSplit = LinkSplit[LinkPtr].Split('/');
                            if (TableSplit.GetUpperBound(0) > 2) {
                                string TableName = TableSplit[0];
                                string FieldName = TableSplit[1];
                                int RecordID = GenericController.encodeInteger(TableSplit[2]);
                                string FilenameSegment = TableSplit[3];
                                if ((TableName.ToLower() == "cclibraryfiles") && (FieldName.ToLower() == "filename") && (RecordID != 0)) {
                                    LibraryFilesModel file = LibraryFilesModel.create(core, RecordID);
                                    if (file != null) {
                                        FieldName = "filename";
                                        if (true) {
                                            //
                                            // now figure out how the link is delimited by how it starts
                                            //   go to the left and look for:
                                            //   ' ' - ignore spaces, continue forward until we find one of these
                                            //   '=' - means space delimited (src=/image.jpg), ends in ' ' or '>'
                                            //   '"' - means quote delimited (src="/image.jpg"), ends in '"'
                                            //   '>' - means this is not in an HTML tag - skip it (<B>image.jpg</b>)
                                            //   '<' - means god knows what, but its wrong, skip it
                                            //   '(' - means it is a URL(/image.jpg), go to ')'
                                            //
                                            // odd cases:
                                            //   URL( /image.jpg) -
                                            //
                                            string RecordVirtualFilename = file.filename;
                                            //RecordAltSizeList = file.AltSizeList;
                                            if (RecordVirtualFilename == GenericController.EncodeJavascriptStringSingleQuote(RecordVirtualFilename)) {
                                                //
                                                // The javascript version of the filename must match the filename, since we have no way
                                                // of differentiating a ligitimate file, from a block of javascript. If the file
                                                // contains an apostrophe, the original code could have encoded it, but we can not here
                                                // so the best plan is to skip it
                                                //
                                                // example:
                                                // RecordVirtualFilename = "jay/files/cclibraryfiles/filename/000005/test.png"
                                                //
                                                // RecordFilename = "test.png"
                                                // RecordFilenameAltSize = "" (does not exist - the record has the raw filename in it)
                                                // RecordFilenameExt = "png"
                                                // RecordFilenameNoExt = "test"
                                                //
                                                // RecordVirtualFilename = "jay/files/cclibraryfiles/filename/000005/test-100x200.png"
                                                // this is a specail case - most cases to not have the alt size format saved in the filename
                                                // RecordFilename = "test-100x200.png"
                                                // RecordFilenameAltSize (does not exist - the record has the raw filename in it)
                                                // RecordFilenameExt = "png"
                                                // RecordFilenameNoExt = "test-100x200"
                                                // this is wrong
                                                //   xRecordFilenameAltSize = "100x200"
                                                //   xRecordFilenameExt = "png"
                                                //   xRecordFilenameNoExt = "test"
                                                //
                                                //hint = hint & ",080"
                                                int Pos = RecordVirtualFilename.LastIndexOf("/") + 1;
                                                string RecordVirtualPath = "";
                                                string RecordFilename = "";
                                                if (Pos > 0) {
                                                    RecordVirtualPath = RecordVirtualFilename.Left(Pos);
                                                    RecordFilename = RecordVirtualFilename.Substring(Pos);
                                                }
                                                Pos = RecordFilename.LastIndexOf(".") + 1;
                                                string RecordFilenameNoExt = "";
                                                string RecordFilenameExt = "";
                                                if (Pos > 0) {
                                                    RecordFilenameExt = GenericController.vbLCase(RecordFilename.Substring(Pos));
                                                    RecordFilenameNoExt = GenericController.vbLCase(RecordFilename.Left(Pos - 1));
                                                }
                                                string FilePrefixSegment = LinkSplit[LinkPtr - 1];
                                                if (FilePrefixSegment.Length > 1) {
                                                    //
                                                    // Look into FilePrefixSegment and see if we are in the querystring attribute of an <AC tag
                                                    //   if so, the filename may be AddonEncoded and delimited with & (so skip it)
                                                    Pos = FilePrefixSegment.LastIndexOf("<") + 1;
                                                    if (Pos > 0) {
                                                        if (GenericController.vbLCase(FilePrefixSegment.Substring(Pos, 3)) != "ac ") {
                                                            //
                                                            // look back in the FilePrefixSegment to find the character before the link
                                                            //
                                                            int EndPos = 0;
                                                            for (int Ptr = FilePrefixSegment.Length; Ptr >= 1; Ptr--) {
                                                                string TestChr = FilePrefixSegment.Substring(Ptr - 1, 1);
                                                                switch (TestChr) {
                                                                    case "=":
                                                                        //
                                                                        // Ends in ' ' or '>', find the first
                                                                        //
                                                                        int EndPos1 = GenericController.vbInstr(1, FilenameSegment, " ");
                                                                        int EndPos2 = GenericController.vbInstr(1, FilenameSegment, ">");
                                                                        if (EndPos1 != 0 & EndPos2 != 0) {
                                                                            if (EndPos1 < EndPos2) {
                                                                                EndPos = EndPos1;
                                                                            } else {
                                                                                EndPos = EndPos2;
                                                                            }
                                                                        } else if (EndPos1 != 0) {
                                                                            EndPos = EndPos1;
                                                                        } else if (EndPos2 != 0) {
                                                                            EndPos = EndPos2;
                                                                        } else {
                                                                            EndPos = 0;
                                                                        }
                                                                        //todo  WARNING: Exit statements not matching the immediately enclosing block are converted using a 'goto' statement:
                                                                        //ORIGINAL LINE: Exit For
                                                                        goto ExitLabel1;
                                                                    case "\"":
                                                                        //
                                                                        // Quoted, ends is '"'
                                                                        //
                                                                        EndPos = GenericController.vbInstr(1, FilenameSegment, "\"");
                                                                        //todo  WARNING: Exit statements not matching the immediately enclosing block are converted using a 'goto' statement:
                                                                        //ORIGINAL LINE: Exit For
                                                                        goto ExitLabel1;
                                                                    case "(":
                                                                        //
                                                                        // url() style, ends in ')' or a ' '
                                                                        //
                                                                        if (GenericController.vbLCase(FilePrefixSegment.Substring(Ptr - 1, 7)) == "(&quot;") {
                                                                            EndPos = GenericController.vbInstr(1, FilenameSegment, "&quot;)");
                                                                        } else if (GenericController.vbLCase(FilePrefixSegment.Substring(Ptr - 1, 2)) == "('") {
                                                                            EndPos = GenericController.vbInstr(1, FilenameSegment, "')");
                                                                        } else if (GenericController.vbLCase(FilePrefixSegment.Substring(Ptr - 1, 2)) == "(\"") {
                                                                            EndPos = GenericController.vbInstr(1, FilenameSegment, "\")");
                                                                        } else {
                                                                            EndPos = GenericController.vbInstr(1, FilenameSegment, ")");
                                                                        }
                                                                        //todo  WARNING: Exit statements not matching the immediately enclosing block are converted using a 'goto' statement:
                                                                        //ORIGINAL LINE: Exit For
                                                                        goto ExitLabel1;
                                                                    case "'":
                                                                        //
                                                                        // Delimited within a javascript pair of apostophys
                                                                        //
                                                                        EndPos = GenericController.vbInstr(1, FilenameSegment, "'");
                                                                        //todo  WARNING: Exit statements not matching the immediately enclosing block are converted using a 'goto' statement:
                                                                        //ORIGINAL LINE: Exit For
                                                                        goto ExitLabel1;
                                                                    case ">":
                                                                    case "<":
                                                                        //
                                                                        // Skip this link
                                                                        //
                                                                        ParseError = true;
                                                                        //todo  WARNING: Exit statements not matching the immediately enclosing block are converted using a 'goto' statement:
                                                                        //ORIGINAL LINE: Exit For
                                                                        goto ExitLabel1;
                                                                }
                                                            }
                                                            ExitLabel1:
                                                            //
                                                            // check link
                                                            //
                                                            if (EndPos == 0) {
                                                                ParseError = true;
                                                                break;
                                                            } else {
                                                                string ImageFilename = null;
                                                                string SegmentAfterImage = null;

                                                                string ImageFilenameNoExt = null;
                                                                string ImageFilenameExt = null;
                                                                string ImageAltSize = null;

                                                                //hint = hint & ",120"
                                                                SegmentAfterImage = FilenameSegment.Substring(EndPos - 1);
                                                                ImageFilename = GenericController.decodeResponseVariable(FilenameSegment.Left(EndPos - 1));
                                                                ImageFilenameNoExt = ImageFilename;
                                                                ImageFilenameExt = "";
                                                                Pos = ImageFilename.LastIndexOf(".") + 1;
                                                                if (Pos > 0) {
                                                                    ImageFilenameNoExt = GenericController.vbLCase(ImageFilename.Left(Pos - 1));
                                                                    ImageFilenameExt = GenericController.vbLCase(ImageFilename.Substring(Pos));
                                                                }
                                                                //
                                                                // Get ImageAltSize
                                                                //
                                                                //hint = hint & ",130"
                                                                ImageAltSize = "";
                                                                if (ImageFilenameNoExt == RecordFilenameNoExt) {
                                                                    //
                                                                    // Exact match
                                                                    //
                                                                } else if (GenericController.vbInstr(1, ImageFilenameNoExt, RecordFilenameNoExt, 1) != 1) {
                                                                    //
                                                                    // There was a change and the recordfilename is not part of the imagefilename
                                                                    //
                                                                } else {
                                                                    //
                                                                    // the recordfilename is the first part of the imagefilename - Get ImageAltSize
                                                                    //
                                                                    ImageAltSize = ImageFilenameNoExt.Substring(RecordFilenameNoExt.Length);
                                                                    if (ImageAltSize.Left(1) != "-") {
                                                                        ImageAltSize = "";
                                                                    } else {
                                                                        ImageAltSize = ImageAltSize.Substring(1);
                                                                        string[] SizeTest = ImageAltSize.Split('x');
                                                                        if (SizeTest.GetUpperBound(0) != 1) {
                                                                            ImageAltSize = "";
                                                                        } else {
                                                                            if (SizeTest[0].IsNumeric() & SizeTest[1].IsNumeric()) {
                                                                                ImageFilenameNoExt = RecordFilenameNoExt;
                                                                                //ImageFilenameNoExt = Mid(ImageFilenameNoExt, 1, Pos - 1)
                                                                                //RecordFilenameNoExt = Mid(RecordFilename, 1, Pos - 1)
                                                                            } else {
                                                                                ImageAltSize = "";
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                                //
                                                                // problem - in the case where the recordfilename = img-100x200, the imagefilenamenoext is img
                                                                //
                                                                //hint = hint & ",140"
                                                                if ((RecordFilenameNoExt != ImageFilenameNoExt) | (RecordFilenameExt != ImageFilenameExt)) {
                                                                    //
                                                                    // There has been a change
                                                                    //
                                                                    string NewRecordFilename = null;
                                                                    NewRecordFilename = RecordVirtualPath + RecordFilenameNoExt + "." + RecordFilenameExt;
                                                                    //
                                                                    // realtime image updates replace without creating new size - that is for the edit interface
                                                                    //
                                                                    // put the New file back into the tablesplit in case there are more then 4 splits
                                                                    //
                                                                    TableSplit[0] = "";
                                                                    TableSplit[1] = "";
                                                                    TableSplit[2] = "";
                                                                    TableSplit[3] = SegmentAfterImage;
                                                                    NewRecordFilename = GenericController.encodeURL(NewRecordFilename) + ((string)(string.Join("/", TableSplit))).Substring(3);
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
                    //hint = hint & ",910"
                    if (SaveChanges && (!ParseError)) {
                        result = string.Join(ContentFilesLinkPrefix, LinkSplit);
                    }
                }
                //hint = hint & ",920"
                if (!ParseError) {
                    ////
                    //// Convert ACTypeDynamicForm to Add-on
                    ////
                    //if (genericController.vbInstr(1, result, "<ac type=\"" + ACTypeDynamicForm, 1) != 0) {
                    //    result = genericController.vbReplace(result, "type=\"DYNAMICFORM\"", "TYPE=\"aggregatefunction\"", 1, 99, 1);
                    //    result = genericController.vbReplace(result, "name=\"DYNAMICFORM\"", "name=\"DYNAMIC FORM\"", 1, 99, 1);
                    //}
                }
                //hint = hint & ",930"
                if (ParseError) {
                    result = ""
                    + "\r\n<!-- warning: parsing aborted on ccLibraryFile replacement -->"
                    + "\r\n" + result + "\r\n<!-- /warning: parsing aborted on ccLibraryFile replacement -->";
                }
                // 20180914 - deprecate -- content box is an addon run with either an image-icon, or json string
                //
                // {{content}} should be <ac type="templatecontent" etc>
                // the merge is now handled in csv_EncodeActiveContent, but some sites have hand {{content}} tags entered
                //
                //hint = hint & ",940"
                //if (genericController.vbInstr(1, result, "{{content}}", 1) != 0) {
                //    result = genericController.vbReplace(result, "{{content}}", "<AC type=\"" + ACTypeTemplateContent + "\">", 1, 99, 1);
                //}
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Convert an active content field (html data stored with <ac></ac> html tags) to a wysiwyg editor request (html with edit icon <img> for <ac></ac>)
        /// </summary>
        public static string renderHtmlForWysiwygEditor(CoreController core, string editorValue) {
            string result = editorValue;
            result = encode(core, result, 0, "", 0, 0, false, false, false, true, true, false, "", "", false, 0, "", Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextSimple, false, null, false);
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// (future) for remote methods that render in JSON
        /// </summary>
        /// <param name="core"></param>
        /// <param name="Source"></param>
        /// <param name="ContextContentName"></param>
        /// <param name="ContextRecordID"></param>
        /// <param name="ContextContactPeopleID"></param>
        /// <param name="ProtocolHostString"></param>
        /// <param name="DefaultWrapperID"></param>
        /// <param name="ignore_TemplateCaseOnly_Content"></param>
        /// <param name="addonContext"></param>
        /// <returns></returns>
        public static string renderJSONForRemoteMethod(CoreController core, string Source, string ContextContentName, int ContextRecordID, int ContextContactPeopleID, string ProtocolHostString, int DefaultWrapperID, string ignore_TemplateCaseOnly_Content, CPUtilsBaseClass.addonContext addonContext) {
            string result = Source;
            result = ContentCmdController.executeContentCommands(core, result, CPUtilsBaseClass.addonContext.ContextAdmin, ContextContactPeopleID, false);
            result = encode(core, result, core.session.user.id, ContextContentName, ContextRecordID, ContextContactPeopleID, false, false, true, true, false, true, "", ProtocolHostString, false, DefaultWrapperID, ignore_TemplateCaseOnly_Content, addonContext, core.session.isAuthenticated, null, core.session.isEditingAnything());
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// render active content for an email
        /// </summary>
        /// <param name="core"></param>
        /// <param name="Source"></param>
        /// <param name="personalizationPeopleID"></param>
        /// <param name="queryStringForLinkAppend"></param>
        /// <returns></returns>
        public static string renderHtmlForEmail(CoreController core, string Source, int personalizationPeopleID, string queryStringForLinkAppend) {
            string result = Source;
            result = ContentCmdController.executeContentCommands(core, result, CPUtilsClass.addonContext.ContextEmail, personalizationPeopleID, true);
            result = encode(core, result, personalizationPeopleID, "", 0, 0, false, true, true, true, false, true, queryStringForLinkAppend, "", true, 0, "", CPUtilsBaseClass.addonContext.ContextEmail, true, null, false);
            return result;
        }
    }
}
