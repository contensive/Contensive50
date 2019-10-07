
using System;
using Contensive.BaseClasses;

using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.Processor.Exceptions;
using Contensive.Addons.AdminSite.Controllers;
using System.Collections.Generic;
using System.Linq;
using Contensive.Models.Db;
using System.Globalization;

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
        /// <param name="deprecated_ContextContactPeopleID">optional, the id of the person who should be contacted for this content. If 0, uses current user.</param>
        /// <param name="ProtocolHostString">The protocol + domain to be used to build URLs if the content includes dynamically generated images (resource library active content) and the domain is different from where the content is being rendered already. Leave blank and the URL will start with a slash.</param>
        /// <param name="DefaultWrapperID">optional, if provided and addon is html on a page, the content will be wrapped in the wrapper indicated</param>
        /// <param name="addonContext">Where this addon is being executed, like as a process, or in an email, or on a page. If not provided page context is assumed (adding assets like js and css to document)</param>
        /// <returns></returns>
        public static string renderHtmlForWeb(CoreController core, string source, string contextContentName = "", int ContextRecordID = 0, int deprecated_ContextContactPeopleID = 0, string ProtocolHostString = "", int DefaultWrapperID = 0, CPUtilsBaseClass.addonContext addonContext = CPUtilsBaseClass.addonContext.ContextPage) {
            string result = ContentCmdController.executeContentCommands(core, source, CPUtilsBaseClass.addonContext.ContextAdmin, core.session.user.id, core.session.visit.visitAuthenticated);
            return encode(core, result, core.session.user.id, contextContentName, ContextRecordID, deprecated_ContextContactPeopleID, false, false, true, true, false, true, "", ProtocolHostString, false, DefaultWrapperID, "", addonContext, core.session.isAuthenticated, null, core.session.isEditing());
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
        /// <param name="deprecated_personalizationPeopleId">The user to whom this rendering will be targeted</param>
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
        /// <param name="deprecated_personalizationIsAuthenticated"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private static string renderActiveContent(CoreController core, string sourceHtmlContent, int deprecated_personalizationPeopleId, string ContextContentName, int ContextRecordID, int moreInfoPeopleId, bool addLinkAuthenticationToAllLinks, bool ignore, bool encodeACResourceLibraryImages, bool encodeForWysiwygEditor, bool EncodeNonCachableTags, string queryStringToAppendToAllLinks, string protocolHost, bool IsEmailContent, string AdminURL, bool deprecated_personalizationIsAuthenticated, CPUtilsBaseClass.addonContext context = CPUtilsBaseClass.addonContext.ContextPage) {
            string result = sourceHtmlContent;
            try {
                //
                // Fixup Anchor Query (additional AddonOptionString pairs to add to the end)
                string AnchorQuery = "";
                if (addLinkAuthenticationToAllLinks && (deprecated_personalizationPeopleId != 0)) {
                    AnchorQuery += "&eid=" + SecurityController.encodeToken(core, deprecated_personalizationPeopleId, DateTime.Now);
                }
                //
                if (!string.IsNullOrEmpty(queryStringToAppendToAllLinks)) {
                    AnchorQuery += "&" + queryStringToAppendToAllLinks;
                }
                //
                if (!string.IsNullOrEmpty(AnchorQuery)) {
                    AnchorQuery = AnchorQuery.Substring(1);
                }
                //
                // Test early if this needs to run at all
                bool ProcessACTags = (((EncodeNonCachableTags || encodeACResourceLibraryImages || encodeForWysiwygEditor)) && (result.IndexOf("<AC ", System.StringComparison.OrdinalIgnoreCase) != -1));
                bool ProcessAnchorTags = (!string.IsNullOrEmpty(AnchorQuery)) && (result.IndexOf("<A ", System.StringComparison.OrdinalIgnoreCase) != -1);
                if ((!string.IsNullOrEmpty(result)) && (ProcessAnchorTags || ProcessACTags)) {
                    //
                    // ----- Load the Active Elements
                    //
                    HtmlParserController KmaHTML = new HtmlParserController(core);
                    KmaHTML.Load(result);
                    StringBuilderLegacyController Stream = new StringBuilderLegacyController(); int ElementPointer = 0;
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
                                                                    if (!GroupController.isInGroupList(core, deprecated_personalizationPeopleId, true, GroupIDList, true)) {
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
                                                                        argumentKeyValuePairs = GenericController.convertQSNVAArgumentstoDocPropertiesList(core, AddonOptionStringHTMLEncoded),
                                                                        instanceGuid = ACInstanceID,
                                                                        errorContextMessage = "rendering addon found in active content within an email"
                                                                    };
                                                                    AddonModel addon = DbBaseModel.createByUniqueName<AddonModel>(core.cpParent, ACName);
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
                                                            string AddonContentName = AddonModel.tableMetadata.contentName;
                                                            string SelectList = "Name,Link,ID,ArgumentList,ObjectProgramID,IconFilename,IconWidth,IconHeight,IconSprites,IsInline,ccGuid";
                                                            int IconWidth = 0;
                                                            int IconHeight = 0;
                                                            int IconSprites = 0;
                                                            string IconAlt = "";
                                                            string IconTitle = "";
                                                            bool AddonIsInline = false;
                                                            string SrcOptionList = "";
                                                            string IconFilename = "";
                                                            using (var csData = new CsModel(core)) {
                                                                string Criteria = "";
                                                                if (!string.IsNullOrEmpty(ACGuid)) {
                                                                    Criteria = "ccguid=" + DbController.encodeSQLText(ACGuid);
                                                                } else {
                                                                    Criteria = "name=" + DbController.encodeSQLText(ACName.ToUpper());
                                                                }
                                                                if (csData.open(AddonContentName, Criteria, "Name,ID", false, 0, SelectList)) {
                                                                    IconFilename = csData.getText("IconFilename");
                                                                    SrcOptionList = csData.getText("ArgumentList");
                                                                    IconWidth = csData.getInteger("IconWidth");
                                                                    IconHeight = csData.getInteger("IconHeight");
                                                                    IconSprites = csData.getInteger("IconSprites");
                                                                    AddonIsInline = csData.getBoolean("IsInline");
                                                                    ACGuid = csData.getText("ccGuid");
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
                                                                csData.close();
                                                            }
                                                            //
                                                            // Build AddonOptionStringHTMLEncoded from SrcOptionList (for names), itself (for current settings), and SrcOptionList (for select options)
                                                            //
                                                            if (SrcOptionList.IndexOf("wrapper", System.StringComparison.OrdinalIgnoreCase) == -1) {
                                                                if (AddonIsInline) {
                                                                    SrcOptionList = SrcOptionList + Environment.NewLine + AddonOptionConstructor_Inline;
                                                                } else {
                                                                    SrcOptionList = SrcOptionList + Environment.NewLine + AddonOptionConstructor_Block;
                                                                }
                                                            }
                                                            string ResultOptionListHTMLEncoded = "";
                                                            if (!string.IsNullOrEmpty(SrcOptionList)) {
                                                                ResultOptionListHTMLEncoded = "";
                                                                SrcOptionList = GenericController.vbReplace(SrcOptionList, Environment.NewLine, "\r");
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
                                            case ACTypeTemplateContent: {
                                                    //
                                                    // ----- Create Template Content
                                                    AddonOptionStringHTMLEncoded = "";
                                                    addonOptionString = "";
                                                    NotUsedID = 0;
                                                    if (encodeForWysiwygEditor) {
                                                        //
                                                        string IconIDControlString = "AC," + ACType + "," + NotUsedID + "," + ACName + "," + AddonOptionStringHTMLEncoded;
                                                        Copy = AddonController.getAddonIconImg(AdminURL, 52, 64, 0, false, IconIDControlString, "https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/images/ACTemplateContentIcon.gif", serverFilePath, "Template Page Content", "Renders as the Template Page Content", ACInstanceID, 0);
                                                        //Copy = IconImg;
                                                    } else if (EncodeNonCachableTags) {
                                                        //
                                                        // Add in the Content
                                                        Copy = fpoContentBox;
                                                    }
                                                    break;
                                                }
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
                LogController.logError(core, ex);
                // throw;
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
                                                if (ImageSrcOriginal.ToLowerInvariant().Left(VirtualFilePathBad.Length) == GenericController.vbLCase(VirtualFilePathBad)) {
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
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                int Pos = 0;
                                                int RecordID = 0;
                                                string ImageStyle = null;
                                                if (ACIdentifier == "AC") {
                                                    {
                                                        {
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
                                                                                }
                                                                            }
                                                                        }
                                                                        ElementText = "<AC type=\"IMAGE\" ACInstanceID=\"" + ACInstanceID + "\" RecordID=\"" + RecordID + "\" Style=\"" + ImageStyle + "\" Width=\"" + ImageWidthText + "\" Height=\"" + ImageHeightText + "\" VSpace=\"" + ImageVSpace + "\" HSpace=\"" + ImageHSpace + "\" Alt=\"" + ImageAlt + "\" Align=\"" + ImageAlign + "\" Border=\"" + ImageBorder + "\" Loop=\"" + ImageLoop + "\">";
                                                                    }
                                                                    break;
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
                                                                                LibraryFilesModel file = LibraryFilesModel.create<LibraryFilesModel>(core.cpParent, RecordID);
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
                                                                                    //ImageEditController imageEditor = null;
                                                                                    //
                                                                                    // if recordwidth or height are missing, get them from the file
                                                                                    //
                                                                                    if (RecordWidth == 0 || RecordHeight == 0) {
                                                                                        using (var imageEditor = new ImageEditController()) {
                                                                                            if (imageEditor.load(ImageVirtualFilename, core.cdnFiles)) {
                                                                                                file.width = imageEditor.width;
                                                                                                file.height = imageEditor.height;
                                                                                                file.save(core.cpParent);
                                                                                            }
                                                                                        }
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
                                                                                            using (var imageEditor = new ImageEditController()) {
                                                                                                if (imageEditor.load(ImageVirtualFilename, core.cdnFiles)) {
                                                                                                    ImageWidth = imageEditor.width;
                                                                                                    ImageHeight = imageEditor.height;
                                                                                                }
                                                                                            }
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
                                                                                                    using (var imageEditor = new ImageEditController()) {
                                                                                                        if (!imageEditor.load(RecordVirtualFilename, core.cdnFiles)) {
                                                                                                            //
                                                                                                            // image load failed, use raw filename
                                                                                                            //
                                                                                                            LogController.logWarn(core, new GenericException("ImageEditController failed to load filename [" + RecordVirtualFilename + "]"));
                                                                                                        } else {
                                                                                                            //
                                                                                                            //
                                                                                                            //
                                                                                                            RecordWidth = imageEditor.width;
                                                                                                            RecordHeight = imageEditor.height;
                                                                                                            if (ImageWidth == 0) {
                                                                                                                //
                                                                                                                //
                                                                                                                //
                                                                                                                imageEditor.height = ImageHeight;
                                                                                                            } else if (ImageHeight == 0) {
                                                                                                                //
                                                                                                                //
                                                                                                                //
                                                                                                                imageEditor.width = ImageWidth;
                                                                                                            } else if (RecordHeight == ImageHeight) {
                                                                                                                //
                                                                                                                // change the width
                                                                                                                //
                                                                                                                imageEditor.width = ImageWidth;
                                                                                                            } else {
                                                                                                                //
                                                                                                                // change the height
                                                                                                                //
                                                                                                                imageEditor.height = ImageHeight;
                                                                                                            }
                                                                                                            //
                                                                                                            // if resized only width or height, set the other
                                                                                                            //
                                                                                                            if (ImageWidth == 0) {
                                                                                                                ImageWidth = imageEditor.width;
                                                                                                                ImageAltSize = ImageWidth.ToString() + "x" + ImageHeight.ToString();
                                                                                                            }
                                                                                                            if (ImageHeight == 0) {
                                                                                                                ImageHeight = imageEditor.height;
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
                                                                                                            imageEditor.save(ImageVirtualFilePath + NewImageFilename, core.cdnFiles);
                                                                                                            //
                                                                                                            // Update image record
                                                                                                            //
                                                                                                            RecordAltSizeList = RecordAltSizeList + Environment.NewLine + ImageAltSize;
                                                                                                        }
                                                                                                    }

                                                                                                }
                                                                                                //
                                                                                                // Change the image src to the AltSize
                                                                                                ElementText = GenericController.vbReplace(ElementText, ImageSrcOriginal, HtmlController.encodeHtml(GenericController.encodeURL(GenericController.getCdnFileLink(core, ImageVirtualFilePath) + NewImageFilename)));
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                    file.save(core.cpParent);
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
                LogController.logError(core, ex);
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
        /// <param name="deprecated_personalizationPeopleId"></param>
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
        public static string encode(CoreController core, string sourceHtmlContent, int deprecated_personalizationPeopleId, string ContextContentName, int ContextRecordID, int ContextContactPeopleID, bool convertHtmlToText, bool addLinkAuthToAllLinks, bool EncodeActiveFormatting, bool EncodeActiveImages, bool EncodeActiveEditIcons, bool EncodeActivePersonalization, string queryStringForLinkAppend, string ProtocolHostLink, bool IsEmailContent, int ignore_DefaultWrapperID, string ignore_TemplateCaseOnly_Content, CPUtilsBaseClass.addonContext Context, bool personalizationIsAuthenticated, object nothingObject, bool isEditingAnything) {
            string result = sourceHtmlContent;
            string hint = "0";
            try {
                if (!string.IsNullOrEmpty(sourceHtmlContent)) {
                    hint = "10";
                    int LineStart = 0;
                    //
                    if (deprecated_personalizationPeopleId <= 0) {
                        deprecated_personalizationPeopleId = core.session.user.id;
                    }
                    //
                    // -- resize images
                    hint = "20";
                    result = optimizeLibraryFileImagesInHtmlContent(core, result);
                    //
                    // -- Do Active Content Conversion
                    hint = "30";
                    if (addLinkAuthToAllLinks || EncodeActiveFormatting || EncodeActiveImages || EncodeActiveEditIcons) {
                        string AdminURL = "/" + core.appConfig.adminRoute;
                        result = renderActiveContent(core, result, deprecated_personalizationPeopleId, ContextContentName, ContextRecordID, ContextContactPeopleID, addLinkAuthToAllLinks, EncodeActiveFormatting, EncodeActiveImages, EncodeActiveEditIcons, EncodeActivePersonalization, queryStringForLinkAppend, ProtocolHostLink, IsEmailContent, AdminURL, personalizationIsAuthenticated, Context);
                    }
                    //
                    // -- Do Plain Text Conversion
                    hint = "40";
                    if (convertHtmlToText) {
                        NUglify.Html.HtmlToTextOptions options = NUglify.Html.HtmlToTextOptions.KeepFormatting;
                        result = NUglify.Uglify.HtmlToText(result, options).Code; // htmlToTextControllers.convert(core, result);
                    }
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
                    try {
                        hint = "50";
                        const string StartFlag = "<!-- ADDON";
                        const string EndFlag = " -->";
                        if (result.IndexOf(StartFlag) != -1) {
                            hint = "51";
                            int LineEnd = 0;
                            while (result.IndexOf(StartFlag) != -1) {
                                hint = "52";
                                LineStart = GenericController.vbInstr(1, result, StartFlag);
                                LineEnd = GenericController.vbInstr(LineStart, result, EndFlag);
                                string Copy = "";
                                if (LineEnd == 0) {
                                    LogController.logWarn(core, "csv_EncodeContent9, Addon could not be inserted into content because the HTML comment holding the position is not formated correctly");
                                    break;
                                } else {
                                    hint = "53";
                                    string AddonName = "";
                                    string addonOptionString = "";
                                    string ACInstanceID = "";
                                    string AddonGuid = "";
                                    int copyLength = LineEnd - LineStart - 11;
                                    if (copyLength <= 0) {
                                        //
                                        // -- nothing between start and end, someone added a comment <!-- ADDON -->
                                    } else {
                                        hint = "54";
                                        Copy = result.Substring(LineStart + 10, copyLength);
                                        string[] ArgSplit = GenericController.SplitDelimited(Copy, ",");
                                        int ArgCnt = ArgSplit.GetUpperBound(0) + 1;
                                        if (!string.IsNullOrEmpty(ArgSplit[0])) {
                                            hint = "55";
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
                                            hint = "56";
                                            CPUtilsBaseClass.addonExecuteContext executeContext = new CPUtilsBaseClass.addonExecuteContext() {
                                                addonType = CPUtilsBaseClass.addonContext.ContextPage,
                                                cssContainerClass = "",
                                                cssContainerId = "",
                                                hostRecord = new CPUtilsBaseClass.addonExecuteHostRecordContext() {
                                                    contentName = ContextContentName,
                                                    fieldName = "",
                                                    recordId = ContextRecordID
                                                },
                                                instanceGuid = ACInstanceID,
                                                argumentKeyValuePairs = GenericController.convertQSNVAArgumentstoDocPropertiesList(core, addonOptionString),
                                                errorContextMessage = "rendering active content with guid [" + AddonGuid + "] or name [" + AddonName + "]"
                                            };
                                            hint = "57, AddonGuid [" + AddonGuid + "], AddonName [" + AddonName + "]";
                                            if (!string.IsNullOrEmpty(AddonGuid)) {
                                                Copy = core.addon.execute(DbBaseModel.create<AddonModel>(core.cpParent, AddonGuid), executeContext);
                                            } else {
                                                Copy = core.addon.execute(DbBaseModel.createByUniqueName<AddonModel>(core.cpParent, AddonName), executeContext);
                                            }
                                        }
                                    }
                                }
                                hint = "58";
                                result = result.Left(LineStart - 1) + Copy + result.Substring(LineEnd + 3);
                            }
                        }
                    } catch (Exception ex) {
                        //
                        // -- handle error, but don't abort encode
                        LogController.logError(core, ex, "hint [" + hint + "]");
                    }
                    //
                    // process out text block comments inserted by addons
                    // remove all content between BlockTextStartMarker and the next BlockTextEndMarker, or end of copy
                    // exception made for the content with just the startmarker because when the AC tag is replaced with
                    // with the marker, encode content is called with the result, which is just the marker, and this
                    // section will remove it
                    //
                    hint = "60";
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
                    hint = "70";
                    if (GenericController.vbInstr(1, result, FeedbackFormNotSupportedComment, 1) != 0) {
                        result = GenericController.vbReplace(result, FeedbackFormNotSupportedComment, PageContentController.getFeedbackForm(core, ContextContentName, ContextRecordID, ContextContactPeopleID), 1, 99, 1);
                    }
                }
            } catch (Exception ex) {
                LogController.logError(core, ex, "hint [" + hint + "]");
            }
            return result;
        }
        //
        //================================================================================================================
        /// <summary>
        /// for html content, this routine optimizes images referenced in the html if they are from library file
        /// </summary>
        public static string optimizeLibraryFileImagesInHtmlContent(CoreController core, string htmlContent) {
            try {
                //
                // -- exit if nothing there or not allowed
                if (string.IsNullOrWhiteSpace(htmlContent)) { return htmlContent; }
                if (!core.siteProperties.imageAllowUpdate) { return htmlContent; }
                //
                // this used to be "/" + core.appConfig.name + "/files/"
                int posLink = htmlContent.IndexOf(core.appConfig.cdnFileUrl + "ccLibraryFiles/", StringComparison.OrdinalIgnoreCase);
                if (posLink == -1) { return htmlContent; }
                //
                // ----- Process Resource Library Images (swap in most current file)
                // -- LibraryFileSegments = an array that have key 1 and on start with core.appConfig.cdnFileUrl (usually /appname/files/)
                List<string> libaryFileSegmentList = stringSplit(htmlContent, core.appConfig.cdnFileUrl).ToList();
                string htmlContentUpdated = libaryFileSegmentList.First();
                foreach (var libraryFileSegment in libaryFileSegmentList.Skip(1)) {
                    string htmlContentSegment = libraryFileSegment;
                    //
                    // Determine if this sement is in a tag (<img src="...">) or in content (&quot...&quote)
                    // For now, skip the ones in content
                    int TagPosEnd = GenericController.vbInstr(1, htmlContentSegment, ">");
                    int TagPosStart = GenericController.vbInstr(1, htmlContentSegment, "<");
                    if ((TagPosStart != 0) && (TagPosEnd < TagPosStart)) {
                        //
                        // break pathfilename off the quote to the end
                        int posQuote = htmlContentSegment.IndexOf("\"");
                        if (posQuote == -1) { continue; }
                        string htmlContentSegment_file = htmlContentSegment.Substring(0, posQuote);
                        string[] libraryFileSplit = htmlContentSegment_file.Split('/');
                        if (libraryFileSplit.GetUpperBound(0) > 2) {
                            int libraryRecordID = encodeInteger(libraryFileSplit[2]);
                            if ((libraryFileSplit[0].ToLower(CultureInfo.InvariantCulture) == "cclibraryfiles") && (libraryFileSplit[1].ToLower(CultureInfo.InvariantCulture) == "filename") && (libraryRecordID != 0)) {
                                LibraryFilesModel file = LibraryFilesModel.create<LibraryFilesModel>(core.cpParent, libraryRecordID);
                                if ((file != null) && (htmlContentSegment_file != file.filename)) { htmlContentSegment_file = file.filename; }
                            }
                        }
                        htmlContentSegment = htmlContentSegment_file + htmlContentSegment.Substring(posQuote);
                    }
                    htmlContentUpdated += core.appConfig.cdnFileUrl + htmlContentSegment;
                }
                return htmlContentUpdated;
            } catch (Exception ex) {
                LogController.logError(core, ex);
                return htmlContent;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Convert an active content field (html data stored with <ac></ac> html tags) to a wysiwyg editor request (html with edit icon <img> for <ac></ac>)
        /// </summary>
        public static string renderHtmlForWysiwygEditor(CoreController core, string editorValue) {
            return encode(core, editorValue, 0, "", 0, 0, false, false, false, true, true, false, "", "", false, 0, "", Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextSimple, false, null, false);
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
        /// <param name="deprecated_ContextContactPeopleID"></param>
        /// <param name="ProtocolHostString"></param>
        /// <param name="DefaultWrapperID"></param>
        /// <param name="ignore_TemplateCaseOnly_Content"></param>
        /// <param name="addonContext"></param>
        /// <returns></returns>
        public static string renderJSONForRemoteMethod(CoreController core, string Source, string ContextContentName, int ContextRecordID, int deprecated_ContextContactPeopleID, string ProtocolHostString, int DefaultWrapperID, string ignore_TemplateCaseOnly_Content, CPUtilsBaseClass.addonContext addonContext) {
            string result = Source;
            result = ContentCmdController.executeContentCommands(core, result, CPUtilsBaseClass.addonContext.ContextAdmin, deprecated_ContextContactPeopleID, false);
            result = encode(core, result, core.session.user.id, ContextContentName, ContextRecordID, deprecated_ContextContactPeopleID, false, false, true, true, false, true, "", ProtocolHostString, false, DefaultWrapperID, ignore_TemplateCaseOnly_Content, addonContext, core.session.isAuthenticated, null, core.session.isEditing());
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// render active content for an email
        /// </summary>
        /// <param name="core"></param>
        /// <param name="Source"></param>
        /// <param name="sendToPersonId"></param>
        /// <param name="queryStringForLinkAppend"></param>
        /// <returns></returns>
        public static string renderHtmlForEmail(CoreController core, string Source, int sendToPersonId, string queryStringForLinkAppend) {
            string result = Source;
            //
            // -- create session context for this user and queue the email.
            using (CPClass cp = new CPClass(core.appConfig.name, core.serverConfig)) {
                if(cp.User.LoginByID(sendToPersonId)) {
                    result = ContentCmdController.executeContentCommands(cp.core, result, CPUtilsClass.addonContext.ContextEmail, sendToPersonId, true);
                    result = encode(cp.core, result, sendToPersonId, "", 0, 0, false, true, true, true, false, true, queryStringForLinkAppend, "", true, 0, "", CPUtilsBaseClass.addonContext.ContextEmail, true, null, false);
                }
            };
            return result;
        }
    }
}
