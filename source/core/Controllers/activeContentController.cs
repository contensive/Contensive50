
using System;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using Contensive.BaseClasses;
using Contensive.Core;
using Contensive.Core.Models.DbModels;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;

namespace Contensive.Core.Controllers {
    /// <summary>
    /// Tools used to assemble html document elements. This is not a storage for assembling a document (see docController)
    /// </summary>
    public class activeContentController {
        ////
        //private coreClass cpCore;
        ////
        ////====================================================================================================
        ///// <summary>
        ///// constructor
        ///// </summary>
        ///// <param name="cpCore"></param>
        ///// <remarks></remarks>
        //public activeContentController(coreClass cpCore) {
        //    this.cpCore = cpCore;
        //}
        //
        //====================================================================================================
        // csv_EncodeActiveContent_Internal
        //       ...
        //       AllowLinkEID    Boolean, if yes, the EID=000... string is added to all links in the content
        //                       Use this for email so links will include the members longin.
        //
        //       Some Active elements can not be replaced here because they incorporate content from  the wbeclient.
        //       For instance the Aggregate Function Objects. These elements create
        //       <!-- FPO1 --> placeholders in the content, and commented instructions, one per line, at the top of the content
        //       Replacement instructions
        //       <!-- Replace FPO1,AFObject,ObjectName,OptionString -->
        //           Aggregate Function Object, ProgramID=ObjectName, Optionstring=Optionstring
        //       <!-- Replace FPO1,AFObject,ObjectName,OptionString -->
        //           Aggregate Function Object, ProgramID=ObjectName, Optionstring=Optionstring
        //
        // Tag descriptions:
        //
        //   primary methods
        //
        //   <Ac Type="Date">
        //   <Ac Type="Member" Field="Name">
        //   <Ac Type="Organization" Field="Name">
        //   <Ac Type="Visitor" Field="Name">
        //   <Ac Type="Visit" Field="Name">
        //   <Ac Type="Contact" Member="PeopleID">
        //       displays a For More info block of copy
        //   <Ac Type="Feedback" Member="PeopleID">
        //       displays a feedback note block
        //   <Ac Type="ChildList" Name="ChildListName">
        //       displays a list of child blocks that reference this CHildList Element
        //   <Ac Type="Language" Name="All|English|Spanish|etc.">
        //       blocks content to next language tag to eveyone without this PeopleLanguage
        //   <Ac Type="Image" Record="" Width="" Height="" Alt="" Align="">
        //   <AC Type="Download" Record="" Alt="">
        //       renders as an anchored download icon, with the alt tag
        //       the rendered anchor points back to the root/index, which increments the resource's download count
        //
        //   During Editing, AC tags are converted (Encoded) to EditIcons
        //       these are image tags with AC information stored in the ID attribute
        //       except AC-Image, which are converted into the actual image for editing
        //       during the edit save, the EditIcons are converted (Decoded) back
        //
        //   Remarks
        //
        //   First <Member.FieldName> encountered opens the Members Table, etc.
        //       ( does <OpenTable name="Member" Tablename="ccMembers" ID=(current PeopleID)> )
        //   The copy is divided into Blocks, starting at every tag and running to the next tag.
        //   BlockTag()  The tag without the braces found
        //   BlockCopy() The copy following the tag up to the next tag
        //   BlockLabel()    the string identifier for the block
        //   BlockCount  the total blocks in the message
        //   BlockPointer    the current block being examined
        //====================================================================================================
        //
        //
        private static string convertActiveContent_Internal_activeParts(coreClass cpCore, string Source, int personalizationPeopleId, string ContextContentName, int ContextRecordID, int moreInfoPeopleId, bool AddLinkEID, bool EncodeCachableTags, bool EncodeImages, bool EncodeEditIcons, bool EncodeNonCachableTags, string AddAnchorQuery, string ProtocolHostLink, bool IsEmailContent, string AdminURL, bool personalizationIsAuthenticated, CPUtilsBaseClass.addonContext context = CPUtilsBaseClass.addonContext.ContextPage) {
            string result = "";
            try {
                string ACGuid = null;
                string ACNameCaption = null;
                string GroupIDList = null;
                string IDControlString = null;
                string IconIDControlString = null;
                string Criteria = null;
                string AddonContentName = null;
                string SelectList = "";
                int IconWidth = 0;
                int IconHeight = 0;
                int IconSprites = 0;
                bool AddonIsInline = false;
                string IconAlt = "";
                string IconTitle = "";
                string IconImg = null;
                string TextName = null;
                string ListName = null;
                string SrcOptionSelector = null;
                string ResultOptionSelector = null;
                string SrcOptionList = null;
                int Pos = 0;
                string SrcOptionValueSelector = null;
                string InstanceOptionValue = null;
                string ResultOptionListHTMLEncoded = null;
                string UCaseACName = null;
                string IconFilename = null;
                string FieldName = null;
                int Ptr = 0;
                int ElementPointer = 0;
                int CSVisitor = 0;
                int CSVisit = 0;
                bool CSVisitorSet = false;
                bool CSVisitSet = false;
                string ElementTag = null;
                string ACType = null;
                string ACField = null;
                string ACName = "";
                string Copy = null;
                htmlParserController KmaHTML = null;
                int AttributeCount = 0;
                int AttributePointer = 0;
                string Name = null;
                string Value = null;
                int CS = 0;
                int ACAttrRecordID = 0;
                int ACAttrWidth = 0;
                int ACAttrHeight = 0;
                string ACAttrAlt = null;
                int ACAttrBorder = 0;
                int ACAttrLoop = 0;
                int ACAttrVSpace = 0;
                int ACAttrHSpace = 0;
                string Filename = "";
                string ACAttrAlign = null;
                bool ProcessAnchorTags = false;
                bool ProcessACTags = false;
                string ACLanguageName = null;
                stringBuilderLegacyController Stream = new stringBuilderLegacyController();
                string AnchorQuery = "";
                int CSOrganization = 0;
                bool CSOrganizationSet = false;
                int CSPeople = 0;
                bool CSPeopleSet = false;
                int CSlanguage = 0;
                bool PeopleLanguageSet = false;
                string PeopleLanguage = "";
                string UcasePeopleLanguage = null;
                string serverFilePath = "";
                string ReplaceInstructions = "";
                string Link = null;
                int NotUsedID = 0;
                string addonOptionString = null;
                string AddonOptionStringHTMLEncoded = null;
                string[] SrcOptions = null;
                string SrcOptionName = null;
                int FormCount = 0;
                int FormInputCount = 0;
                string ACInstanceID = null;
                int PosStart = 0;
                int PosEnd = 0;
                string AllowGroups = null;
                string workingContent = null;
                string NewName = null;
                //
                workingContent = Source;
                //
                // Fixup Anchor Query (additional AddonOptionString pairs to add to the end)
                //
                if (AddLinkEID && (personalizationPeopleId != 0)) {
                    AnchorQuery = AnchorQuery + "&EID=" + cpCore.security.encodeToken(genericController.encodeInteger(personalizationPeopleId), DateTime.Now);
                }
                //
                if (!string.IsNullOrEmpty(AddAnchorQuery)) {
                    AnchorQuery = AnchorQuery + "&" + AddAnchorQuery;
                }
                //
                if (!string.IsNullOrEmpty(AnchorQuery)) {
                    AnchorQuery = AnchorQuery.Substring(1);
                }
                //
                // ----- xml contensive process instruction
                //
                //TemplateBodyContent
                //Pos = genericController.vbInstr(1, TemplateBodyContent, "<?contensive", vbTextCompare)
                //If Pos > 0 Then
                //    '
                //    ' convert template body if provided - this is the content that replaces the content box addon
                //    '
                //    TemplateBodyContent = Mid(TemplateBodyContent, Pos)
                //    LayoutEngineOptionString = "data=" & encodeNvaArgument(TemplateBodyContent)
                //    TemplateBodyContent = csv_ExecuteActiveX("aoPrimitives.StructuredDataClass", "Structured Data Engine", nothing, LayoutEngineOptionString, "data=(structured data)", LayoutErrorMessage)
                //End If
                Pos = genericController.vbInstr(1, workingContent, "<?contensive", 1);
                if (Pos > 0) {
                    throw new ApplicationException("Structured xml data commands are no longer supported");
                    //
                    // convert content if provided
                    //
                    //workingContent = Mid(workingContent, Pos)
                    //LayoutEngineOptionString = "data=" & encodeNvaArgument(workingContent)
                    //Dim structuredData As New core_primitivesStructuredDataClass(Me)
                    //workingContent = structuredData.execute()
                    //workingContent = csv_ExecuteActiveX("aoPrimitives.StructuredDataClass", "Structured Data Engine", LayoutEngineOptionString, "data=(structured data)", LayoutErrorMessage)
                }
                //
                // Special Case
                // Convert <!-- STARTGROUPACCESS 10,11,12 --> format to <AC type=GROUPACCESS AllowGroups="10,11,12">
                // Convert <!-- ENDGROUPACCESS --> format to <AC type=GROUPACCESSEND>
                //
                PosStart = genericController.vbInstr(1, workingContent, "<!-- STARTGROUPACCESS ", 1);
                if (PosStart > 0) {
                    PosEnd = genericController.vbInstr(PosStart, workingContent, "-->");
                    if (PosEnd > 0) {
                        AllowGroups = workingContent.Substring(PosStart + 21, PosEnd - PosStart - 23);
                        workingContent = workingContent.Left(PosStart - 1) + "<AC type=\"" + ACTypeAggregateFunction + "\" name=\"block text\" querystring=\"allowgroups=" + AllowGroups + "\">" + workingContent.Substring(PosEnd + 2);
                    }
                }
                //
                PosStart = genericController.vbInstr(1, workingContent, "<!-- ENDGROUPACCESS ", 1);
                if (PosStart > 0) {
                    PosEnd = genericController.vbInstr(PosStart, workingContent, "-->");
                    if (PosEnd > 0) {
                        workingContent = workingContent.Left(PosStart - 1) + "<AC type=\"" + ACTypeAggregateFunction + "\" name=\"block text end\" >" + workingContent.Substring(PosEnd + 2);
                    }
                }
                //
                // ----- Special case -- if any of these are in the source, this is legacy. Convert them to icons,
                //       and they will be converted to AC tags when the icons are saved
                //
                if (EncodeEditIcons) {
                    //
                    IconIDControlString = "AC," + ACTypeTemplateContent + "," + NotUsedID + "," + ACName + ",";
                    IconImg = genericController.GetAddonIconImg(AdminURL, 52, 64, 0, false, IconIDControlString, "/ccLib/images/ACTemplateContentIcon.gif", serverFilePath, "Template Page Content", "Renders as [Template Page Content]", "", 0);
                    workingContent = genericController.vbReplace(workingContent, "{{content}}", IconImg, 1, 99, 1);
                    //WorkingContent = genericController.vbReplace(WorkingContent, "{{content}}", "<img ACInstanceID=""" & ACInstanceID & """ onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as the Template Page Content"" id=""AC," & ACTypeTemplateContent & "," & NotUsedID & "," & ACName & ","" src=""/ccLib/images/ACTemplateContentIcon.gif"" WIDTH=52 HEIGHT=64>", 1, -1, vbTextCompare)
                    //
                    // replace all other {{...}}
                    //
                    //LoopPtr = 0
                    //Pos = 1
                    //Do While Pos > 0 And LoopPtr < 100
                    //    Pos = genericController.vbInstr(Pos, workingContent, "{{" & ACTypeDynamicMenu, vbTextCompare)
                    //    If Pos > 0 Then
                    //        addonOptionString = ""
                    //        PosStart = Pos
                    //        If PosStart <> 0 Then
                    //            'PosStart = PosStart + 2 + Len(ACTypeDynamicMenu)
                    //            PosEnd = genericController.vbInstr(PosStart, workingContent, "}}", vbTextCompare)
                    //            If PosEnd <> 0 Then
                    //                Cmd = Mid(workingContent, PosStart + 2, PosEnd - PosStart - 2)
                    //                Pos = genericController.vbInstr(1, Cmd, "?")
                    //                If Pos <> 0 Then
                    //                    addonOptionString = genericController.decodeHtml(Mid(Cmd, Pos + 1))
                    //                End If
                    //                TextName = cpCore.csv_GetAddonOptionStringValue("menu", addonOptionString)
                    //                '
                    //                addonOptionString = "Menu=" & TextName & "[" & cpCore.csv_GetDynamicMenuACSelect() & "]&NewMenu="
                    //                AddonOptionStringHTMLEncoded = html_EncodeHTML("Menu=" & TextName & "[" & cpCore.csv_GetDynamicMenuACSelect() & "]&NewMenu=")
                    //                '
                    //                IconIDControlString = "AC," & ACTypeDynamicMenu & "," & NotUsedID & "," & ACName & "," & AddonOptionStringHTMLEncoded
                    //                IconImg = genericController.GetAddonIconImg(AdminURL, 52, 52, 0, False, IconIDControlString, "/ccLib/images/ACDynamicMenuIcon.gif", serverFilePath, "Dynamic Menu", "Renders as [Dynamic Menu]", "", 0)
                    //                workingContent = Mid(workingContent, 1, PosStart - 1) & IconImg & Mid(workingContent, PosEnd + 2)
                    //            End If
                    //        End If
                    //    End If
                    //Loop
                }
                //
                // Test early if this needs to run at all
                //
                ProcessACTags = (((EncodeCachableTags || EncodeNonCachableTags || EncodeImages || EncodeEditIcons)) & (workingContent.IndexOf("<AC ", System.StringComparison.OrdinalIgnoreCase) != -1));
                ProcessAnchorTags = (!string.IsNullOrEmpty(AnchorQuery)) & (workingContent.IndexOf("<A ", System.StringComparison.OrdinalIgnoreCase) != -1);
                if ((!string.IsNullOrEmpty(workingContent)) & (ProcessAnchorTags || ProcessACTags)) {
                    //
                    // ----- Load the Active Elements
                    //
                    KmaHTML = new htmlParserController(cpCore);
                    KmaHTML.Load(workingContent);
                    //
                    // ----- Execute and output elements
                    //
                    ElementPointer = 0;
                    if (KmaHTML.ElementCount > 0) {
                        ElementPointer = 0;
                        workingContent = "";
                        serverFilePath = ProtocolHostLink + "/" + cpCore.serverConfig.appConfig.name + "/files/";
                        Stream = new stringBuilderLegacyController();
                        while (ElementPointer < KmaHTML.ElementCount) {
                            Copy = KmaHTML.Text(ElementPointer).ToString();
                            if (KmaHTML.IsTag(ElementPointer)) {
                                ElementTag = genericController.vbUCase(KmaHTML.TagName(ElementPointer));
                                ACName = KmaHTML.ElementAttribute(ElementPointer, "NAME");
                                UCaseACName = genericController.vbUCase(ACName);
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
                                            if ((Copy.IndexOf("contensiveuserform=1", System.StringComparison.OrdinalIgnoreCase) != -1) | (Copy.IndexOf("contensiveuserform=\"1\"", System.StringComparison.OrdinalIgnoreCase) != -1)) {
                                                //
                                                // if it has "contensiveuserform=1" in the form tag, remove it from the form and add the hidden that makes it work
                                                //
                                                Copy = genericController.vbReplace(Copy, "ContensiveUserForm=1", "", 1, 99, 1);
                                                Copy = genericController.vbReplace(Copy, "ContensiveUserForm=\"1\"", "", 1, 99, 1);
                                                if (!EncodeEditIcons) {
                                                    Copy += "<input type=hidden name=ContensiveUserForm value=1>";
                                                }
                                            }
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
                                            AttributeCount = KmaHTML.ElementAttributeCount(ElementPointer);
                                            if (AttributeCount > 0) {
                                                Copy = "<A ";
                                                for (AttributePointer = 0; AttributePointer < AttributeCount; AttributePointer++) {
                                                    Name = KmaHTML.ElementAttributeName(ElementPointer, AttributePointer);
                                                    Value = KmaHTML.ElementAttributeValue(ElementPointer, AttributePointer);
                                                    if (genericController.vbUCase(Name) == "HREF") {
                                                        Link = Value;
                                                        Pos = genericController.vbInstr(1, Link, "://");
                                                        if (Pos > 0) {
                                                            Link = Link.Substring(Pos + 2);
                                                            Pos = genericController.vbInstr(1, Link, "/");
                                                            if (Pos > 0) {
                                                                Link = Link.Left(Pos - 1);
                                                            }
                                                        }
                                                        if ((string.IsNullOrEmpty(Link)) || (("," + cpCore.serverConfig.appConfig.domainList[0] + ",").IndexOf("," + Link + ",", System.StringComparison.OrdinalIgnoreCase) != -1)) {
                                                            //
                                                            // ----- link is for this site
                                                            //
                                                            if (Value.Substring(Value.Length - 1) == "?") {
                                                                //
                                                                // Ends in a questionmark, must be Dwayne (?)
                                                                //
                                                                Value = Value + AnchorQuery;
                                                            } else if (genericController.vbInstr(1, Value, "mailto:", 1) != 0) {
                                                                //
                                                                // catch mailto
                                                                //
                                                                //Value = Value & AnchorQuery
                                                            } else if (genericController.vbInstr(1, Value, "?") == 0) {
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
                                        ACGuid = KmaHTML.ElementAttribute(ElementPointer, "GUID");
                                        switch (genericController.vbUCase(ACType)) {
                                            case ACTypeEnd: {
                                                    //
                                                    // End Tag - Personalization
                                                    //       This tag causes an end to the all tags, like Language
                                                    //       It is removed by with EncodeEditIcons (on the way to the editor)
                                                    //       It is added to the end of the content with Decode(activecontent)
                                                    //
                                                    if (EncodeEditIcons) {
                                                        Copy = "";
                                                    } else if (EncodeNonCachableTags) {
                                                        Copy = "<!-- Language ANY -->";
                                                    }
                                                    break;
                                                }
                                            case ACTypeDate: {
                                                    //
                                                    // Date Tag
                                                    //
                                                    if (EncodeEditIcons) {
                                                        IconIDControlString = "AC," + ACTypeDate;
                                                        IconImg = genericController.GetAddonIconImg(AdminURL, 0, 0, 0, true, IconIDControlString, "", serverFilePath, "Current Date", "Renders as [Current Date]", ACInstanceID, 0);
                                                        Copy = IconImg;
                                                        //Copy = "<img ACInstanceID=""" & ACInstanceID & """ alt=""Add-on"" title=""Rendered as the current date"" ID=""AC," & ACTypeDate & """ src=""/ccLib/images/ACDate.GIF"">"
                                                    } else if (EncodeNonCachableTags) {
                                                        Copy = DateTime.Now.ToString();
                                                    }
                                                    break;
                                                }
                                            case ACTypeMember:
                                            case ACTypePersonalization: {
                                                    //
                                                    // Member Tag works regardless of authentication
                                                    // cm must be sure not to reveal anything
                                                    //
                                                    ACField = genericController.vbUCase(KmaHTML.ElementAttribute(ElementPointer, "FIELD"));
                                                    if (string.IsNullOrEmpty(ACField)) {
                                                        // compatibility for old personalization type
                                                        ACField = htmlController.getAddonOptionStringValue("FIELD", KmaHTML.ElementAttribute(ElementPointer, "QUERYSTRING"));
                                                    }
                                                    FieldName = genericController.EncodeInitialCaps(ACField);
                                                    if (string.IsNullOrEmpty(FieldName)) {
                                                        FieldName = "Name";
                                                    }
                                                    if (EncodeEditIcons) {
                                                        switch (genericController.vbUCase(FieldName)) {
                                                            case "FIRSTNAME":
                                                                //
                                                                IconIDControlString = "AC," + ACType + "," + FieldName;
                                                                IconImg = genericController.GetAddonIconImg(AdminURL, 0, 0, 0, true, IconIDControlString, "", serverFilePath, "User's First Name", "Renders as [User's First Name]", ACInstanceID, 0);
                                                                Copy = IconImg;
                                                                //
                                                                break;
                                                            case "LASTNAME":
                                                                //
                                                                IconIDControlString = "AC," + ACType + "," + FieldName;
                                                                IconImg = genericController.GetAddonIconImg(AdminURL, 0, 0, 0, true, IconIDControlString, "", serverFilePath, "User's Last Name", "Renders as [User's Last Name]", ACInstanceID, 0);
                                                                Copy = IconImg;
                                                                //
                                                                break;
                                                            default:
                                                                //
                                                                IconIDControlString = "AC," + ACType + "," + FieldName;
                                                                IconImg = genericController.GetAddonIconImg(AdminURL, 0, 0, 0, true, IconIDControlString, "", serverFilePath, "User's " + FieldName, "Renders as [User's " + FieldName + "]", ACInstanceID, 0);
                                                                Copy = IconImg;
                                                                //
                                                                break;
                                                        }
                                                    } else if (EncodeNonCachableTags) {
                                                        if (personalizationPeopleId != 0) {
                                                            if (genericController.vbUCase(FieldName) == "EID") {
                                                                Copy = cpCore.security.encodeToken(personalizationPeopleId, DateTime.Now);
                                                            } else {
                                                                if (!CSPeopleSet) {
                                                                    CSPeople = cpCore.db.cs_openContentRecord("People", personalizationPeopleId);
                                                                    CSPeopleSet = true;
                                                                }
                                                                if ((cpCore.db.csOk(CSPeople) & cpCore.db.cs_isFieldSupported(CSPeople, FieldName))) {
                                                                    Copy = cpCore.db.csGetLookup(CSPeople, FieldName);
                                                                }
                                                            }
                                                        }
                                                    }
                                                    break;
                                                }
                                            case ACTypeChildList: {
                                                    //
                                                    // Child List
                                                    //
                                                    ListName = genericController.encodeText((KmaHTML.ElementAttribute(ElementPointer, "name")));

                                                    if (EncodeEditIcons) {
                                                        IconIDControlString = "AC," + ACType + ",," + ACName;
                                                        IconImg = genericController.GetAddonIconImg(AdminURL, 0, 0, 0, true, IconIDControlString, "", serverFilePath, "List of Child Pages", "Renders as [List of Child Pages]", ACInstanceID, 0);
                                                        Copy = IconImg;
                                                    } else if (EncodeCachableTags) {
                                                        //
                                                        // Handle in webclient
                                                        //
                                                        // removed sort method because all child pages are read in together in the order set by the parent - improve this later
                                                        Copy = "{{" + ACTypeChildList + "?name=" + genericController.encodeNvaArgument(ListName) + "}}";
                                                    }
                                                    break;
                                                }
                                            case ACTypeContact: {
                                                    //
                                                    // Formatting Tag
                                                    //
                                                    if (EncodeEditIcons) {
                                                        //
                                                        IconIDControlString = "AC," + ACType;
                                                        IconImg = genericController.GetAddonIconImg(AdminURL, 0, 0, 0, true, IconIDControlString, "", serverFilePath, "Contact Information Line", "Renders as [Contact Information Line]", ACInstanceID, 0);
                                                        Copy = IconImg;
                                                        //
                                                        //Copy = "<img ACInstanceID=""" & ACInstanceID & """ alt=""Add-on"" title=""Rendered as a line of text with contact information for this record's primary contact"" id=""AC," & ACType & """ src=""/ccLib/images/ACContact.GIF"">"
                                                    } else if (EncodeCachableTags) {
                                                        if (moreInfoPeopleId != 0) {
                                                            Copy = pageContentController.getMoreInfoHtml(cpCore, moreInfoPeopleId);
                                                        }
                                                    }
                                                    break;
                                                }
                                            case ACTypeFeedback: {
                                                    //
                                                    // Formatting tag - change from information to be included after submission
                                                    //
                                                    if (EncodeEditIcons) {
                                                        //
                                                        IconIDControlString = "AC," + ACType;
                                                        IconImg = genericController.GetAddonIconImg(AdminURL, 0, 0, 0, false, IconIDControlString, "", serverFilePath, "Feedback Form", "Renders as [Feedback Form]", ACInstanceID, 0);
                                                        Copy = IconImg;
                                                        //
                                                        //Copy = "<img ACInstanceID=""" & ACInstanceID & """ alt=""Add-on"" title=""Rendered as a feedback form, sent to this record's primary contact."" id=""AC," & ACType & """ src=""/ccLib/images/ACFeedBack.GIF"">"
                                                    } else if (EncodeNonCachableTags) {
                                                        if ((moreInfoPeopleId != 0) & (!string.IsNullOrEmpty(ContextContentName)) & (ContextRecordID != 0)) {
                                                            Copy = FeedbackFormNotSupportedComment;
                                                        }
                                                    }
                                                    break;
                                                }
                                            case ACTypeLanguage: {
                                                    //
                                                    // Personalization Tag - block languages not from the visitor
                                                    //
                                                    ACLanguageName = genericController.vbUCase(KmaHTML.ElementAttribute(ElementPointer, "NAME"));
                                                    if (EncodeEditIcons) {
                                                        switch (genericController.vbUCase(ACLanguageName)) {
                                                            case "ANY":
                                                                //
                                                                IconIDControlString = "AC," + ACType + ",," + ACLanguageName;
                                                                IconImg = genericController.GetAddonIconImg(AdminURL, 0, 0, 0, true, IconIDControlString, "", serverFilePath, "All copy following this point is rendered, regardless of the member's language setting", "Renders as [Begin Rendering All Languages]", ACInstanceID, 0);
                                                                Copy = IconImg;
                                                                //
                                                                //Copy = "<img ACInstanceID=""" & ACInstanceID & """ alt=""All copy following this point is rendered, regardless of the member's language setting"" id=""AC," & ACType & ",," & ACLanguageName & """ src=""/ccLib/images/ACLanguageAny.GIF"">"
                                                                //Case "ENGLISH", "FRENCH", "GERMAN", "PORTUGEUESE", "ITALIAN", "SPANISH", "CHINESE", "HINDI"
                                                                //   Copy = "<img ACInstanceID=""" & ACInstanceID & """ alt=""All copy following this point is rendered if the member's language setting matchs [" & ACLanguageName & "]"" id=""AC," & ACType & ",," & ACLanguageName & """ src=""/ccLib/images/ACLanguage" & ACLanguageName & ".GIF"">"
                                                                break;
                                                            default:
                                                                //
                                                                IconIDControlString = "AC," + ACType + ",," + ACLanguageName;
                                                                IconImg = genericController.GetAddonIconImg(AdminURL, 0, 0, 0, true, IconIDControlString, "", serverFilePath, "All copy following this point is rendered if the member's language setting matchs [" + ACLanguageName + "]", "Begin Rendering for language [" + ACLanguageName + "]", ACInstanceID, 0);
                                                                Copy = IconImg;
                                                                //
                                                                //Copy = "<img ACInstanceID=""" & ACInstanceID & """ alt=""All copy following this point is rendered if the member's language setting matchs [" & ACLanguageName & "]"" id=""AC," & ACType & ",," & ACLanguageName & """ src=""/ccLib/images/ACLanguageOther.GIF"">"
                                                                break;
                                                        }
                                                    } else if (EncodeNonCachableTags) {
                                                        if (personalizationPeopleId == 0) {
                                                            PeopleLanguage = "any";
                                                        } else {
                                                            if (!PeopleLanguageSet) {
                                                                if (!CSPeopleSet) {
                                                                    CSPeople = cpCore.db.cs_openContentRecord("people", personalizationPeopleId);
                                                                    CSPeopleSet = true;
                                                                }
                                                                CSlanguage = cpCore.db.cs_openContentRecord("Languages", cpCore.db.csGetInteger(CSPeople, "LanguageID"), 0, false, false, "Name");
                                                                if (cpCore.db.csOk(CSlanguage)) {
                                                                    PeopleLanguage = cpCore.db.csGetText(CSlanguage, "name");
                                                                }
                                                                cpCore.db.csClose(ref CSlanguage);
                                                                PeopleLanguageSet = true;
                                                            }
                                                        }
                                                        UcasePeopleLanguage = genericController.vbUCase(PeopleLanguage);
                                                        if (UcasePeopleLanguage == "ANY") {
                                                            //
                                                            // This person wants all the languages, put in language marker and continue
                                                            //
                                                            Copy = "<!-- Language " + ACLanguageName + " -->";
                                                        } else if ((ACLanguageName != UcasePeopleLanguage) & (ACLanguageName != "ANY")) {
                                                            //
                                                            // Wrong language, remove tag, skip to the end, or to the next language tag
                                                            //
                                                            Copy = "";
                                                            ElementPointer = ElementPointer + 1;
                                                            while (ElementPointer < KmaHTML.ElementCount) {
                                                                ElementTag = genericController.vbUCase(KmaHTML.TagName(ElementPointer));
                                                                if (ElementTag == "AC") {
                                                                    ACType = genericController.vbUCase(KmaHTML.ElementAttribute(ElementPointer, "TYPE"));
                                                                    if (ACType == ACTypeLanguage) {
                                                                        ElementPointer = ElementPointer - 1;
                                                                        break;
                                                                    } else if (ACType == ACTypeEnd) {
                                                                        break;
                                                                    }
                                                                }
                                                                ElementPointer = ElementPointer + 1;
                                                            }
                                                        } else {
                                                            //
                                                            // Right Language, remove tag
                                                            //
                                                            Copy = "";
                                                        }
                                                    }
                                                    break;
                                                }
                                            case ACTypeAggregateFunction: {
                                                    //
                                                    // ----- Add-on
                                                    //
                                                    NotUsedID = 0;
                                                    AddonOptionStringHTMLEncoded = KmaHTML.ElementAttribute(ElementPointer, "QUERYSTRING");
                                                    addonOptionString = genericController.decodeHtml(AddonOptionStringHTMLEncoded);
                                                    if (IsEmailContent) {
                                                        //
                                                        // Addon - for email
                                                        //
                                                        if (EncodeNonCachableTags) {
                                                            //
                                                            // Only hardcoded Add-ons can run in Emails
                                                            //
                                                            switch (genericController.vbLCase(ACName)) {
                                                                case "block text":
                                                                    //
                                                                    // Email is always considered authenticated bc they need their login credentials to get the email.
                                                                    // Allowed to see the content that follows if you are authenticated, admin, or in the group list
                                                                    // This must be done out on the page because the csv does not know about authenticated
                                                                    //
                                                                    Copy = "";
                                                                    GroupIDList = htmlController.getAddonOptionStringValue("AllowGroups", addonOptionString);
                                                                    if (!cpCore.doc.sessionContext.isMemberOfGroupIdList(cpCore, personalizationPeopleId, true, GroupIDList, true)) {
                                                                        //
                                                                        // Block content if not allowed
                                                                        //
                                                                        ElementPointer = ElementPointer + 1;
                                                                        while (ElementPointer < KmaHTML.ElementCount) {
                                                                            ElementTag = genericController.vbUCase(KmaHTML.TagName(ElementPointer));
                                                                            if (ElementTag == "AC") {
                                                                                ACType = genericController.vbUCase(KmaHTML.ElementAttribute(ElementPointer, "TYPE"));
                                                                                if (ACType == ACTypeAggregateFunction) {
                                                                                    if (genericController.vbLCase(KmaHTML.ElementAttribute(ElementPointer, "name")) == "block text end") {
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
                                                                    // always remove end tags because the block text did not remove it
                                                                    //
                                                                    Copy = "";
                                                                    break;
                                                                default:
                                                                    //
                                                                    // all other add-ons, pass out to cpCoreClass to process
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
                                                                        instanceArguments = genericController.convertAddonArgumentstoDocPropertiesList(cpCore, AddonOptionStringHTMLEncoded),
                                                                        instanceGuid = ACInstanceID
                                                                    };
                                                                    addonModel addon = addonModel.createByName(cpCore, ACName);
                                                                    Copy = cpCore.addon.execute(addon, executeContext);
                                                                    //Copy = cpCore.addon.execute_legacy6(0, ACName, AddonOptionStringHTMLEncoded, CPUtilsBaseClass.addonContext.ContextEmail, "", 0, "", ACInstanceID, False, 0, "", True, Nothing, "", Nothing, "", personalizationPeopleId, personalizationIsAuthenticated)
                                                                    break;
                                                            }
                                                        }
                                                    } else {
                                                        //
                                                        // Addon - for web
                                                        //

                                                        if (EncodeEditIcons) {
                                                            //
                                                            // Get IconFilename, update the optionstring, and execute optionstring replacement functions
                                                            //
                                                            AddonContentName = cnAddons;
                                                            if (true) {
                                                                SelectList = "Name,Link,ID,ArgumentList,ObjectProgramID,IconFilename,IconWidth,IconHeight,IconSprites,IsInline,ccGuid";
                                                            }
                                                            if (!string.IsNullOrEmpty(ACGuid)) {
                                                                Criteria = "ccguid=" + cpCore.db.encodeSQLText(ACGuid);
                                                            } else {
                                                                Criteria = "name=" + cpCore.db.encodeSQLText(UCaseACName);
                                                            }
                                                            CS = cpCore.db.csOpen(AddonContentName, Criteria, "Name,ID", false, 0, false, false, SelectList);
                                                            if (cpCore.db.csOk(CS)) {
                                                                 IconFilename = cpCore.db.csGet(CS, "IconFilename");
                                                                SrcOptionList = cpCore.db.csGet(CS, "ArgumentList");
                                                                IconWidth = cpCore.db.csGetInteger(CS, "IconWidth");
                                                                IconHeight = cpCore.db.csGetInteger(CS, "IconHeight");
                                                                IconSprites = cpCore.db.csGetInteger(CS, "IconSprites");
                                                                AddonIsInline = cpCore.db.csGetBoolean(CS, "IsInline");
                                                                ACGuid = cpCore.db.csGetText(CS, "ccGuid");
                                                                IconAlt = ACName;
                                                                IconTitle = "Rendered as the Add-on [" + ACName + "]";
                                                            } else {
                                                                switch (genericController.vbLCase(ACName)) {
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
                                                            cpCore.db.csClose(ref CS);
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
                                                            if (string.IsNullOrEmpty(SrcOptionList)) {
                                                                ResultOptionListHTMLEncoded = "";
                                                            } else {
                                                                ResultOptionListHTMLEncoded = "";
                                                                SrcOptionList = genericController.vbReplace(SrcOptionList, "\r\n", "\r");
                                                                SrcOptionList = genericController.vbReplace(SrcOptionList, "\n", "\r");
                                                                SrcOptions = genericController.stringSplit(SrcOptionList, "\r");
                                                                for (Ptr = 0; Ptr <= SrcOptions.GetUpperBound(0); Ptr++) {
                                                                    SrcOptionName = SrcOptions[Ptr];
                                                                    int LoopPtr2 = 0;

                                                                    while ((SrcOptionName.Length > 1) && (SrcOptionName.Left(1) == "\t") && (LoopPtr2 < 100)) {
                                                                        SrcOptionName = SrcOptionName.Substring(1);
                                                                        LoopPtr2 = LoopPtr2 + 1;
                                                                    }
                                                                    SrcOptionValueSelector = "";
                                                                    SrcOptionSelector = "";
                                                                    Pos = genericController.vbInstr(1, SrcOptionName, "=");
                                                                    if (Pos > 0) {
                                                                        SrcOptionValueSelector = SrcOptionName.Substring(Pos);
                                                                        SrcOptionName = SrcOptionName.Left(Pos - 1);
                                                                        SrcOptionSelector = "";
                                                                        Pos = genericController.vbInstr(1, SrcOptionValueSelector, "[");
                                                                        if (Pos != 0) {
                                                                            SrcOptionSelector = SrcOptionValueSelector.Substring(Pos - 1);
                                                                        }
                                                                    }
                                                                    // all Src and Instance vars are already encoded correctly
                                                                    if (!string.IsNullOrEmpty(SrcOptionName)) {
                                                                        // since AddonOptionString is encoded, InstanceOptionValue will be also
                                                                        InstanceOptionValue = htmlController.getAddonOptionStringValue(SrcOptionName, addonOptionString);
                                                                        //InstanceOptionValue = cpcore.csv_GetAddonOption(SrcOptionName, AddonOptionString)
                                                                        ResultOptionSelector = cpCore.html.getAddonSelector(SrcOptionName, genericController.encodeNvaArgument(InstanceOptionValue), SrcOptionSelector);
                                                                        //ResultOptionSelector = csv_GetAddonSelector(SrcOptionName, InstanceOptionValue, SrcOptionValueSelector)
                                                                        ResultOptionListHTMLEncoded = ResultOptionListHTMLEncoded + "&" + ResultOptionSelector;
                                                                    }
                                                                }
                                                                if (!string.IsNullOrEmpty(ResultOptionListHTMLEncoded)) {
                                                                    ResultOptionListHTMLEncoded = ResultOptionListHTMLEncoded.Substring(1);
                                                                }
                                                            }
                                                            ACNameCaption = genericController.vbReplace(ACName, "\"", "");
                                                            ACNameCaption = genericController.encodeHTML(ACNameCaption);
                                                            IDControlString = "AC," + ACType + "," + NotUsedID + "," + genericController.encodeNvaArgument(ACName) + "," + ResultOptionListHTMLEncoded + "," + ACGuid;
                                                            Copy = genericController.GetAddonIconImg(AdminURL, IconWidth, IconHeight, IconSprites, AddonIsInline, IDControlString, IconFilename, serverFilePath, IconAlt, IconTitle, ACInstanceID, 0);
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
                                            case ACTypeImage: {
                                                    //
                                                    // ----- Image Tag, substitute image placeholder with the link from the REsource Library Record
                                                    //
                                                    if (EncodeImages) {
                                                        Copy = "";
                                                        ACAttrRecordID = genericController.encodeInteger(KmaHTML.ElementAttribute(ElementPointer, "RECORDID"));
                                                        ACAttrWidth = genericController.encodeInteger(KmaHTML.ElementAttribute(ElementPointer, "WIDTH"));
                                                        ACAttrHeight = genericController.encodeInteger(KmaHTML.ElementAttribute(ElementPointer, "HEIGHT"));
                                                        ACAttrAlt = genericController.encodeText(KmaHTML.ElementAttribute(ElementPointer, "ALT"));
                                                        ACAttrBorder = genericController.encodeInteger(KmaHTML.ElementAttribute(ElementPointer, "BORDER"));
                                                        ACAttrLoop = genericController.encodeInteger(KmaHTML.ElementAttribute(ElementPointer, "LOOP"));
                                                        ACAttrVSpace = genericController.encodeInteger(KmaHTML.ElementAttribute(ElementPointer, "VSPACE"));
                                                        ACAttrHSpace = genericController.encodeInteger(KmaHTML.ElementAttribute(ElementPointer, "HSPACE"));
                                                        ACAttrAlign = genericController.encodeText(KmaHTML.ElementAttribute(ElementPointer, "ALIGN"));
                                                        //
                                                        libraryFilesModel file = libraryFilesModel.create(cpCore, ACAttrRecordID);
                                                        if (file != null) {
                                                            Filename = file.Filename;
                                                            Filename = genericController.vbReplace(Filename, "\\", "/");
                                                            Filename = genericController.EncodeURL(Filename);
                                                            Copy += "<img ID=\"AC,IMAGE,," + ACAttrRecordID + "\" src=\"" + genericController.getCdnFileLink(cpCore, Filename) + "\"";
                                                            //
                                                            if (ACAttrWidth == 0) {
                                                                ACAttrWidth = file.pxWidth;
                                                            }
                                                            if (ACAttrWidth != 0) {
                                                                Copy += " width=\"" + ACAttrWidth + "\"";
                                                            }
                                                            //
                                                            if (ACAttrHeight == 0) {
                                                                ACAttrHeight = file.pxHeight;
                                                            }
                                                            if (ACAttrHeight != 0) {
                                                                Copy += " height=\"" + ACAttrHeight + "\"";
                                                            }
                                                            //
                                                            if (ACAttrVSpace != 0) {
                                                                Copy += " vspace=\"" + ACAttrVSpace + "\"";
                                                            }
                                                            //
                                                            if (ACAttrHSpace != 0) {
                                                                Copy += " hspace=\"" + ACAttrHSpace + "\"";
                                                            }
                                                            //
                                                            if (!string.IsNullOrEmpty(ACAttrAlt)) {
                                                                Copy += " alt=\"" + ACAttrAlt + "\"";
                                                            }
                                                            //
                                                            if (!string.IsNullOrEmpty(ACAttrAlign)) {
                                                                Copy += " align=\"" + ACAttrAlign + "\"";
                                                            }
                                                            //
                                                            // no, 0 is an important value
                                                            //If ACAttrBorder <> 0 Then
                                                            Copy += " border=\"" + ACAttrBorder + "\"";
                                                            //    End If
                                                            //
                                                            if (ACAttrLoop != 0) {
                                                                Copy += " loop=\"" + ACAttrLoop + "\"";
                                                            }
                                                            //
                                                            string attr = genericController.encodeText(KmaHTML.ElementAttribute(ElementPointer, "STYLE"));
                                                            if (!string.IsNullOrEmpty(attr)) {
                                                                Copy += " style=\"" + attr + "\"";
                                                            }
                                                            //
                                                            attr = genericController.encodeText(KmaHTML.ElementAttribute(ElementPointer, "CLASS"));
                                                            if (!string.IsNullOrEmpty(attr)) {
                                                                Copy += " class=\"" + attr + "\"";
                                                            }
                                                            //
                                                            Copy += ">";
                                                        }
                                                    }
                                                    //
                                                    //
                                                    break;
                                                }
                                            case ACTypeDownload: {
                                                    //
                                                    // ----- substitute and anchored download image for the AC-Download tag
                                                    //
                                                    ACAttrRecordID = genericController.encodeInteger(KmaHTML.ElementAttribute(ElementPointer, "RECORDID"));
                                                    ACAttrAlt = genericController.encodeText(KmaHTML.ElementAttribute(ElementPointer, "ALT"));
                                                    //
                                                    if (EncodeEditIcons) {
                                                        //
                                                        // Encoding the edit icons for the active editor form
                                                        //
                                                        IconIDControlString = "AC," + ACTypeDownload + ",," + ACAttrRecordID;
                                                        IconImg = genericController.GetAddonIconImg(AdminURL, 16, 16, 0, true, IconIDControlString, "/ccLib/images/IconDownload3.gif", serverFilePath, "Download Icon with a link to a resource", "Renders as [Download Icon with a link to a resource]", ACInstanceID, 0);
                                                        Copy = IconImg;
                                                        //
                                                        //Copy = "<img ACInstanceID=""" & ACInstanceID & """ alt=""Renders as a download icon"" id=""AC," & ACTypeDownload & ",," & ACAttrRecordID & """ src=""/ccLib/images/IconDownload3.GIF"">"
                                                    } else if (EncodeImages) {
                                                        //
                                                        libraryFilesModel file = libraryFilesModel.create(cpCore, ACAttrRecordID);
                                                        if (file != null) {
                                                            if (string.IsNullOrEmpty(ACAttrAlt)) {
                                                                ACAttrAlt = genericController.encodeText(file.AltText);
                                                            }
                                                            Copy = "<a href=\"" + ProtocolHostLink + requestAppRootPath + cpCore.siteProperties.serverPageDefault + "?" + RequestNameDownloadID + "=" + ACAttrRecordID + "\" target=\"_blank\"><img src=\"" + ProtocolHostLink + "/ccLib/images/IconDownload3.gif\" width=\"16\" height=\"16\" border=\"0\" alt=\"" + ACAttrAlt + "\"></a>";
                                                        }
                                                    }
                                                    break;
                                                }
                                            case ACTypeTemplateContent: {
                                                    //
                                                    // ----- Create Template Content
                                                    //
                                                    //ACName = genericController.vbUCase(KmaHTML.ElementAttribute(ElementPointer, "NAME"))
                                                    AddonOptionStringHTMLEncoded = "";
                                                    addonOptionString = "";
                                                    NotUsedID = 0;
                                                    if (EncodeEditIcons) {
                                                        //
                                                        IconIDControlString = "AC," + ACType + "," + NotUsedID + "," + ACName + "," + AddonOptionStringHTMLEncoded;
                                                        IconImg = genericController.GetAddonIconImg(AdminURL, 52, 64, 0, false, IconIDControlString, "/ccLib/images/ACTemplateContentIcon.gif", serverFilePath, "Template Page Content", "Renders as the Template Page Content", ACInstanceID, 0);
                                                        Copy = IconImg;
                                                        //
                                                        //Copy = "<img ACInstanceID=""" & ACInstanceID & """ onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as the Template Page Content"" id=""AC," & ACType & "," & NotUsedID & "," & ACName & "," & AddonOptionStringHTMLEncoded & """ src=""/ccLib/images/ACTemplateContentIcon.gif"" WIDTH=52 HEIGHT=64>"
                                                    } else if (EncodeNonCachableTags) {
                                                        //
                                                        // Add in the Content
                                                        //
                                                        Copy = fpoContentBox;
                                                        //Copy = TemplateBodyContent
                                                        //Copy = "{{" & ACTypeTemplateContent & "}}"
                                                    }
                                                    break;
                                                }
                                            case ACTypeTemplateText: {
                                                    //
                                                    // ----- Create Template Content
                                                    //
                                                    //ACName = genericController.vbUCase(KmaHTML.ElementAttribute(ElementPointer, "NAME"))
                                                    AddonOptionStringHTMLEncoded = KmaHTML.ElementAttribute(ElementPointer, "QUERYSTRING");
                                                    addonOptionString = genericController.decodeHtml(AddonOptionStringHTMLEncoded);
                                                    NotUsedID = 0;
                                                    if (EncodeEditIcons) {
                                                        //
                                                        IconIDControlString = "AC," + ACType + "," + NotUsedID + "," + ACName + "," + AddonOptionStringHTMLEncoded;
                                                        IconImg = genericController.GetAddonIconImg(AdminURL, 52, 52, 0, false, IconIDControlString, "/ccLib/images/ACTemplateTextIcon.gif", serverFilePath, "Template Text", "Renders as a Template Text Box", ACInstanceID, 0);
                                                        Copy = IconImg;
                                                        //
                                                        //Copy = "<img ACInstanceID=""" & ACInstanceID & """ onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as Template Text"" id=""AC," & ACType & "," & NotUsedID & "," & ACName & "," & AddonOptionStringHTMLEncoded & """ src=""/ccLib/images/ACTemplateTextIcon.gif"" WIDTH=52 HEIGHT=52>"
                                                    } else if (EncodeNonCachableTags) {
                                                        //
                                                        // Add in the Content Page
                                                        //
                                                        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                                                        //test - encoding changed
                                                        NewName = htmlController.getAddonOptionStringValue("new", addonOptionString);
                                                        //NewName =  genericController.DecodeResponseVariable(getSimpleNameValue("new", AddonOptionString, "", "&"))
                                                        TextName = htmlController.getAddonOptionStringValue("name", addonOptionString);
                                                        //TextName = getSimpleNameValue("name", AddonOptionString)
                                                        if (string.IsNullOrEmpty(TextName)) {
                                                            TextName = "Default";
                                                        }
                                                        Copy = "{{" + ACTypeTemplateText + "?name=" + genericController.encodeNvaArgument(TextName) + "&new=" + genericController.encodeNvaArgument(NewName) + "}}";
                                                        // ***** can not add it here, if a web hit, it must be encoded from the web client for aggr objects
                                                        //Copy = csv_GetContentCopy(TextName, "Copy Content", "", personalizationpeopleId)
                                                    }
                                                    //Case ACTypeDynamicMenu
                                                    //    '
                                                    //    ' ----- Create Template Menu
                                                    //    '
                                                    //    'ACName = KmaHTML.ElementAttribute(ElementPointer, "NAME")
                                                    //    AddonOptionStringHTMLEncoded = KmaHTML.ElementAttribute(ElementPointer, "QUERYSTRING")
                                                    //    addonOptionString = genericController.decodeHtml(AddonOptionStringHTMLEncoded)
                                                    //    '
                                                    //    ' test for illegal characters (temporary patch to get around not addonencoding during the addon replacement
                                                    //    '
                                                    //    Pos = genericController.vbInstr(1, addonOptionString, "menunew=", vbTextCompare)
                                                    //    If Pos > 0 Then
                                                    //        NewName = Mid(addonOptionString, Pos + 8)
                                                    //        Dim IsOK As Boolean
                                                    //        IsOK = (NewName = genericController.encodeNvaArgument(NewName))
                                                    //        If Not IsOK Then
                                                    //            addonOptionString = Left(addonOptionString, Pos - 1) & "MenuNew=" & genericController.encodeNvaArgument(NewName)
                                                    //        End If
                                                    //    End If
                                                    //    NotUsedID = 0
                                                    //    If EncodeEditIcons Then
                                                    //        If genericController.vbInstr(1, AddonOptionStringHTMLEncoded, "menu=", vbTextCompare) <> 0 Then
                                                    //            '
                                                    //            ' Dynamic Menu
                                                    //            '
                                                    //            '!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                                                    //            ' test - encoding changed
                                                    //            TextName = cpCore.csv_GetAddonOptionStringValue("menu", addonOptionString)
                                                    //            'TextName = cpcore.csv_GetAddonOption("Menu", AddonOptionString)
                                                    //            '
                                                    //            IconIDControlString = "AC," & ACType & "," & NotUsedID & "," & ACName & ",Menu=" & TextName & "[" & cpCore.csv_GetDynamicMenuACSelect() & "]&NewMenu="
                                                    //            IconImg = genericController.GetAddonIconImg(AdminURL, 52, 52, 0, False, IconIDControlString, "/ccLib/images/ACDynamicMenuIcon.gif", serverFilePath, "Dynamic Menu", "Renders as a Dynamic Menu", ACInstanceID, 0)
                                                    //            Copy = IconImg
                                                    //            '
                                                    //            'Copy = "<img ACInstanceID=""" & ACInstanceID & """ onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as a Dynamic Menu"" id=""AC," & ACType & "," & NotUsedID & "," & ACName & ",Menu=" & TextName & "[" & csv_GetDynamicMenuACSelect & "]&NewMenu="" src=""/ccLib/images/ACDynamicMenuIcon.gif"" WIDTH=52 HEIGHT=52>"
                                                    //        Else
                                                    //            '
                                                    //            ' Old Dynamic Menu - values are stored in the icon
                                                    //            '
                                                    //            IconIDControlString = "AC," & ACType & "," & NotUsedID & "," & ACName & "," & AddonOptionStringHTMLEncoded
                                                    //            IconImg = genericController.GetAddonIconImg(AdminURL, 52, 52, 0, False, IconIDControlString, "/ccLib/images/ACDynamicMenuIcon.gif", serverFilePath, "Dynamic Menu", "Renders as a Dynamic Menu", ACInstanceID, 0)
                                                    //            Copy = IconImg
                                                    //            '
                                                    //            'Copy = "<img onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as a Dynamic Menu"" id=""AC," & ACType & "," & NotUsedID & "," & ACName & "," & AddonOptionStringHTMLEncoded & """ src=""/ccLib/images/ACDynamicMenuIcon.gif"" WIDTH=52 HEIGHT=52>"
                                                    //        End If
                                                    //    ElseIf EncodeNonCachableTags Then
                                                    //        '
                                                    //        ' Add in the Content Pag
                                                    //        '
                                                    //        Copy = "{{" & ACTypeDynamicMenu & "?" & addonOptionString & "}}"
                                                    //    End If
                                                    break;
                                                }
                                            case ACTypeWatchList: {
                                                    //
                                                    // ----- Formatting Tag
                                                    //
                                                    //
                                                    // Content Watch replacement
                                                    //   served by the web client because the
                                                    //
                                                    //UCaseACName = genericController.vbUCase(Trim(KmaHTML.ElementAttribute(ElementPointer, "NAME")))
                                                    //ACName = encodeInitialCaps(UCaseACName)
                                                    AddonOptionStringHTMLEncoded = KmaHTML.ElementAttribute(ElementPointer, "QUERYSTRING");
                                                    addonOptionString = genericController.decodeHtml(AddonOptionStringHTMLEncoded);
                                                    if (EncodeEditIcons) {
                                                        //
                                                        IconIDControlString = "AC," + ACType + "," + NotUsedID + "," + ACName + "," + AddonOptionStringHTMLEncoded;
                                                        IconImg = genericController.GetAddonIconImg(AdminURL, 109, 10, 0, true, IconIDControlString, "/ccLib/images/ACWatchList.gif", serverFilePath, "Watch List", "Renders as the Watch List [" + ACName + "]", ACInstanceID, 0);
                                                        Copy = IconImg;
                                                        //
                                                        //Copy = "<img ACInstanceID=""" & ACInstanceID & """ onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as the Watch List [" & ACName & "]"" id=""AC," & ACType & "," & NotUsedID & "," & ACName & "," & AddonOptionStringHTMLEncoded & """ src=""/ccLib/images/ACWatchList.GIF"">"
                                                    } else if (EncodeNonCachableTags) {
                                                        //
                                                        Copy = "{{" + ACTypeWatchList + "?" + addonOptionString + "}}";
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
                    workingContent = Stream.Text;
                    //
                    // Add Contensive User Form if needed
                    //
                    if (FormCount == 0 && FormInputCount > 0) {
                    }
                    workingContent = ReplaceInstructions + workingContent;
                    if (CSPeopleSet) {
                        cpCore.db.csClose(ref CSPeople);
                    }
                    if (CSOrganizationSet) {
                        cpCore.db.csClose(ref CSOrganization);
                    }
                    if (CSVisitorSet) {
                        cpCore.db.csClose(ref CSVisitor);
                    }
                    if (CSVisitSet) {
                        cpCore.db.csClose(ref CSVisit);
                    }
                    KmaHTML = null;
                }
                result = workingContent;
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        //   Decodes ActiveContent and EditIcons into <AC tags
        //       Detect IMG tags
        //           If IMG ID attribute is "AC,IMAGE,recordid", convert to AC Image tag
        //           If IMG ID attribute is "AC,DOWNLOAD,recordid", convert to AC Download tag
        //           If IMG ID attribute is "AC,ACType,ACFieldName,ACInstanceName,QueryStringArguments,AddonGuid", convert it to generic AC tag
        //   ACInstanceID - used to identify an AC tag on a page. Each instance of an AC tag must havea unique ACinstanceID
        //====================================================================================================
        //
        public static string convertEditorResponseToActiveContent(coreClass cpCore, string SourceCopy) {
            string result = "";
            try {
                string imageNewLink = null;
                string ACQueryString = "";
                string ACGuid = null;
                string ACIdentifier = null;
                string RecordFilename = null;
                string RecordFilenameNoExt = null;
                string RecordFilenameExt = null;
                int Ptr = 0;
                string ACInstanceID = null;
                string QSHTMLEncoded = null;
                int Pos = 0;
                string ImageSrcOriginal = null;
                string VirtualFilePathBad = null;
                string[] Paths = null;
                string ImageVirtualFilename = null;
                string ImageFilename = null;
                string ImageFilenameExt = null;
                string ImageFilenameNoExt = null;
                string[] SizeTest = null;
                string[] Styles = null;
                string StyleName = null;
                string StyleValue = null;
                int StyleValueInt = 0;
                string[] Style = null;
                string ImageVirtualFilePath = null;
                string RecordVirtualFilename = null;
                int RecordWidth = 0;
                int RecordHeight = 0;
                string RecordAltSizeList = null;
                string ImageAltSize = null;
                string NewImageFilename = null;
                htmlParserController DHTML = new htmlParserController(cpCore);
                int ElementPointer = 0;
                int ElementCount = 0;
                int AttributeCount = 0;
                string ACType = null;
                string ACFieldName = null;
                string ACInstanceName = null;
                int RecordID = 0;
                string ImageWidthText = null;
                string ImageHeightText = null;
                int ImageWidth = 0;
                int ImageHeight = 0;
                string ElementText = null;
                string ImageID = null;
                string ImageSrc = null;
                string ImageAlt = null;
                int ImageVSpace = 0;
                int ImageHSpace = 0;
                string ImageAlign = null;
                string ImageBorder = null;
                string ImageLoop = null;
                string ImageStyle = null;
                string[] IMageStyleArray = null;
                int ImageStyleArrayCount = 0;
                int ImageStyleArrayPointer = 0;
                string ImageStylePair = null;
                int PositionColon = 0;
                string ImageStylePairName = null;
                string ImageStylePairValue = null;
                stringBuilderLegacyController Stream = new stringBuilderLegacyController();
                string[] ImageIDArray = { };
                int ImageIDArrayCount = 0;
                string QueryString = null;
                string[] QSSplit = null;
                int QSPtr = 0;
                string serverFilePath = null;
                bool ImageAllowSFResize = false;
                imageEditController sf = null;
                //
                result = SourceCopy;
                if (!string.IsNullOrEmpty(result)) {
                    //
                    // leave this in to make sure old <acform tags are converted back
                    // new editor deals with <form, so no more converting
                    //
                    result = genericController.vbReplace(result, "<ACFORM>", "<FORM>");
                    result = genericController.vbReplace(result, "<ACFORM ", "<FORM ");
                    result = genericController.vbReplace(result, "</ACFORM>", "</form>");
                    result = genericController.vbReplace(result, "</ACFORM ", "</FORM ");
                    if (DHTML.Load(result)) {
                        result = "";
                        ElementCount = DHTML.ElementCount;
                        if (ElementCount > 0) {
                            //
                            // ----- Locate and replace IMG Edit icons with AC tags
                            //
                            Stream = new stringBuilderLegacyController();
                            for (ElementPointer = 0; ElementPointer < ElementCount; ElementPointer++) {
                                ElementText = DHTML.Text(ElementPointer).ToString();
                                if (DHTML.IsTag(ElementPointer)) {
                                    switch (genericController.vbUCase(DHTML.TagName(ElementPointer))) {
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
                                                ImageID = DHTML.ElementAttribute(ElementPointer, "id");
                                                ImageSrcOriginal = DHTML.ElementAttribute(ElementPointer, "src");
                                                VirtualFilePathBad = cpCore.serverConfig.appConfig.name + "/files/";
                                                serverFilePath = "/" + VirtualFilePathBad;
                                                if (ImageSrcOriginal.ToLower().Left(VirtualFilePathBad.Length) == genericController.vbLCase(VirtualFilePathBad)) {
                                                    //
                                                    // if the image is from the virtual file path, but the editor did not include the root path, add it
                                                    //
                                                    ElementText = genericController.vbReplace(ElementText, VirtualFilePathBad, "/" + VirtualFilePathBad, 1, 99, 1);
                                                    ImageSrcOriginal = genericController.vbReplace(ImageSrcOriginal, VirtualFilePathBad, "/" + VirtualFilePathBad, 1, 99, 1);
                                                }
                                                ImageSrc = genericController.decodeHtml(ImageSrcOriginal);
                                                ImageSrc = DecodeURL(ImageSrc);
                                                //
                                                // problem with this case is if the addon icon image is from another site.
                                                // not sure how it happened, but I do not think the src of an addon edit icon
                                                // should be able to prevent the addon from executing.
                                                //
                                                ACIdentifier = "";
                                                ACType = "";
                                                ACFieldName = "";
                                                ACInstanceName = "";
                                                ACGuid = "";
                                                ImageIDArrayCount = 0;
                                                if (0 != genericController.vbInstr(1, ImageID, ",")) {
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
                                                        ACIdentifier = genericController.vbUCase(ImageIDArray[0]);
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
                                                if (ACIdentifier == "AC") {
                                                    if (true) {
                                                        if (true) {
                                                            //
                                                            // ----- Process AC Tag
                                                            //
                                                            ACInstanceID = DHTML.ElementAttribute(ElementPointer, "ACINSTANCEID");
                                                            if (string.IsNullOrEmpty(ACInstanceID)) {
                                                                //GUIDGenerator = New guidClass
                                                                ACInstanceID = Guid.NewGuid().ToString();
                                                                //ACInstanceID = Guid.NewGuid.ToString()
                                                            }
                                                            ElementText = "";
                                                            //----------------------------- change to ACType
                                                            switch (genericController.vbUCase(ACType)) {
                                                                case "IMAGE":
                                                                    //
                                                                    // ----- AC Image, Decode Active Images to Resource Library references
                                                                    //
                                                                    if (ImageIDArrayCount >= 4) {
                                                                        RecordID = genericController.encodeInteger(ACInstanceName);
                                                                        ImageWidthText = DHTML.ElementAttribute(ElementPointer, "WIDTH");
                                                                        ImageHeightText = DHTML.ElementAttribute(ElementPointer, "HEIGHT");
                                                                        ImageAlt = genericController.encodeHTML(DHTML.ElementAttribute(ElementPointer, "Alt"));
                                                                        ImageVSpace = genericController.encodeInteger(DHTML.ElementAttribute(ElementPointer, "vspace"));
                                                                        ImageHSpace = genericController.encodeInteger(DHTML.ElementAttribute(ElementPointer, "hspace"));
                                                                        ImageAlign = DHTML.ElementAttribute(ElementPointer, "Align");
                                                                        ImageBorder = DHTML.ElementAttribute(ElementPointer, "BORDER");
                                                                        ImageLoop = DHTML.ElementAttribute(ElementPointer, "LOOP");
                                                                        ImageStyle = DHTML.ElementAttribute(ElementPointer, "STYLE");

                                                                        if (!string.IsNullOrEmpty(ImageStyle)) {
                                                                            //
                                                                            // ----- Process styles, which override attributes
                                                                            //
                                                                            IMageStyleArray = ImageStyle.Split(';');
                                                                            ImageStyleArrayCount = IMageStyleArray.GetUpperBound(0) + 1;
                                                                            for (ImageStyleArrayPointer = 0; ImageStyleArrayPointer < ImageStyleArrayCount; ImageStyleArrayPointer++) {
                                                                                ImageStylePair = IMageStyleArray[ImageStyleArrayPointer].Trim(' ');
                                                                                PositionColon = genericController.vbInstr(1, ImageStylePair, ":");
                                                                                if (PositionColon > 1) {
                                                                                    ImageStylePairName = (ImageStylePair.Left(PositionColon - 1)).Trim(' ');
                                                                                    ImageStylePairValue = (ImageStylePair.Substring(PositionColon)).Trim(' ');
                                                                                    switch (genericController.vbUCase(ImageStylePairName)) {
                                                                                        case "WIDTH":
                                                                                            ImageStylePairValue = genericController.vbReplace(ImageStylePairValue, "px", "");
                                                                                            ImageWidthText = ImageStylePairValue;
                                                                                            break;
                                                                                        case "HEIGHT":
                                                                                            ImageStylePairValue = genericController.vbReplace(ImageStylePairValue, "px", "");
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
                                                                case ACTypeDownload:
                                                                    //
                                                                    // AC Download
                                                                    //
                                                                    if (ImageIDArrayCount >= 4) {
                                                                        RecordID = genericController.encodeInteger(ACInstanceName);
                                                                        ElementText = "<AC type=\"DOWNLOAD\" ACInstanceID=\"" + ACInstanceID + "\" RecordID=\"" + RecordID + "\">";
                                                                    }
                                                                    break;
                                                                case ACTypeDate:
                                                                    //
                                                                    // Date
                                                                    //
                                                                    ElementText = "<AC type=\"" + ACTypeDate + "\">";
                                                                    break;
                                                                case ACTypeVisit:
                                                                case ACTypeVisitor:
                                                                case ACTypeMember:
                                                                case ACTypeOrganization:
                                                                case ACTypePersonalization:
                                                                    //
                                                                    // Visit, etc
                                                                    //
                                                                    ElementText = "<AC type=\"" + ACType + "\" ACInstanceID=\"" + ACInstanceID + "\" field=\"" + ACFieldName + "\">";
                                                                    break;
                                                                case ACTypeChildList:
                                                                case ACTypeLanguage:
                                                                    //
                                                                    // ChildList, Language
                                                                    //
                                                                    if (ACInstanceName == "0") {
                                                                        ACInstanceName = genericController.GetRandomInteger(cpCore).ToString();
                                                                    }
                                                                    ElementText = "<AC type=\"" + ACType + "\" name=\"" + ACInstanceName + "\" ACInstanceID=\"" + ACInstanceID + "\">";
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
                                                                        QSHTMLEncoded = genericController.encodeText(ACQueryString);
                                                                        QueryString = genericController.decodeHtml(QSHTMLEncoded);
                                                                        QSSplit = QueryString.Split('&');
                                                                        for (QSPtr = 0; QSPtr <= QSSplit.GetUpperBound(0); QSPtr++) {
                                                                            Pos = genericController.vbInstr(1, QSSplit[QSPtr], "[");
                                                                            if (Pos > 0) {
                                                                                QSSplit[QSPtr] = QSSplit[QSPtr].Left(Pos - 1);
                                                                            }
                                                                            QSSplit[QSPtr] = genericController.encodeHTML(QSSplit[QSPtr]);
                                                                        }
                                                                        QueryString = string.Join("&", QSSplit);
                                                                    }
                                                                    ElementText = "<AC type=\"" + ACType + "\" name=\"" + ACInstanceName + "\" ACInstanceID=\"" + ACInstanceID + "\" querystring=\"" + QueryString + "\" guid=\"" + ACGuid + "\">";
                                                                    break;
                                                                case ACTypeContact:
                                                                case ACTypeFeedback:
                                                                    //
                                                                    // Contact and Feedback
                                                                    //
                                                                    ElementText = "<AC type=\"" + ACType + "\" ACInstanceID=\"" + ACInstanceID + "\">";
                                                                    break;
                                                                case ACTypeTemplateContent:
                                                                case ACTypeTemplateText:
                                                                    //
                                                                    //
                                                                    //
                                                                    QueryString = "";
                                                                    if (ImageIDArrayCount > 4) {
                                                                        QueryString = genericController.encodeText(ImageIDArray[4]);
                                                                        QSSplit = QueryString.Split('&');
                                                                        for (QSPtr = 0; QSPtr <= QSSplit.GetUpperBound(0); QSPtr++) {
                                                                            QSSplit[QSPtr] = genericController.encodeHTML(QSSplit[QSPtr]);
                                                                        }
                                                                        QueryString = string.Join("&", QSSplit);

                                                                    }
                                                                    ElementText = "<AC type=\"" + ACType + "\" name=\"" + ACInstanceName + "\" ACInstanceID=\"" + ACInstanceID + "\" querystring=\"" + QueryString + "\">";
                                                                    break;
                                                                case ACTypeWatchList:
                                                                    //
                                                                    // Watch List
                                                                    //
                                                                    QueryString = "";
                                                                    if (ImageIDArrayCount > 4) {
                                                                        QueryString = genericController.encodeText(ImageIDArray[4]);
                                                                        QueryString = genericController.decodeHtml(QueryString);
                                                                        QSSplit = QueryString.Split('&');
                                                                        for (QSPtr = 0; QSPtr <= QSSplit.GetUpperBound(0); QSPtr++) {
                                                                            QSSplit[QSPtr] = genericController.encodeHTML(QSSplit[QSPtr]);
                                                                        }
                                                                        QueryString = string.Join("&", QSSplit);
                                                                    }
                                                                    ElementText = "<AC type=\"" + ACType + "\" name=\"" + ACInstanceName + "\" ACInstanceID=\"" + ACInstanceID + "\" querystring=\"" + QueryString + "\">";
                                                                    break;
                                                                case ACTypeRSSLink:
                                                                    //
                                                                    // RSS Link
                                                                    //
                                                                    QueryString = "";
                                                                    if (ImageIDArrayCount > 4) {
                                                                        QueryString = genericController.encodeText(ImageIDArray[4]);
                                                                        QueryString = genericController.decodeHtml(QueryString);
                                                                        QSSplit = QueryString.Split('&');
                                                                        for (QSPtr = 0; QSPtr <= QSSplit.GetUpperBound(0); QSPtr++) {
                                                                            QSSplit[QSPtr] = genericController.encodeHTML(QSSplit[QSPtr]);
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
                                                                        QueryString = genericController.encodeText(ImageIDArray[4]);
                                                                        QueryString = genericController.decodeHtml(QueryString);
                                                                        QSSplit = QueryString.Split('&');
                                                                        for (QSPtr = 0; QSPtr <= QSSplit.GetUpperBound(0); QSPtr++) {
                                                                            QSSplit[QSPtr] = genericController.encodeHTML(QSSplit[QSPtr]);
                                                                        }
                                                                        QueryString = string.Join("&", QSSplit);
                                                                    }
                                                                    ElementText = "<AC type=\"" + ACType + "\" name=\"" + ACInstanceName + "\" ACInstanceID=\"" + ACInstanceID + "\" field=\"" + ACFieldName + "\" querystring=\"" + QueryString + "\">";
                                                                    break;
                                                            }
                                                        }
                                                    }
                                                } else if (genericController.vbInstr(1, ImageSrc, "cclibraryfiles", 1) != 0) {
                                                    ImageAllowSFResize = cpCore.siteProperties.getBoolean("ImageAllowSFResize", true);
                                                    if (ImageAllowSFResize && true) {
                                                        //
                                                        // if it is a real image, check for resize
                                                        //
                                                        Pos = genericController.vbInstr(1, ImageSrc, "cclibraryfiles", 1);
                                                        if (Pos != 0) {
                                                            ImageVirtualFilename = ImageSrc.Substring(Pos - 1);
                                                            Paths = ImageVirtualFilename.Split('/');
                                                            if (Paths.GetUpperBound(0) > 2) {
                                                                if (genericController.vbLCase(Paths[1]) == "filename") {
                                                                    RecordID = genericController.encodeInteger(Paths[2]);
                                                                    if (RecordID != 0) {
                                                                        ImageFilename = Paths[3];
                                                                        ImageVirtualFilePath = genericController.vbReplace(ImageVirtualFilename, ImageFilename, "");
                                                                        Pos = ImageFilename.LastIndexOf(".") + 1;
                                                                        if (Pos > 0) {
                                                                            string ImageFilenameAltSize = "";
                                                                            ImageFilenameExt = ImageFilename.Substring(Pos);
                                                                            ImageFilenameNoExt = ImageFilename.Left(Pos - 1);
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
                                                                                SizeTest = ImageFilenameAltSize.Split('x');
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
                                                                            if (genericController.vbInstr(1, sfImageExtList, ImageFilenameExt, 1) != 0) {
                                                                                //
                                                                                // Determine ImageWidth and ImageHeight
                                                                                //
                                                                                ImageStyle = DHTML.ElementAttribute(ElementPointer, "style");
                                                                                ImageWidth = genericController.encodeInteger(DHTML.ElementAttribute(ElementPointer, "width"));
                                                                                ImageHeight = genericController.encodeInteger(DHTML.ElementAttribute(ElementPointer, "height"));
                                                                                if (!string.IsNullOrEmpty(ImageStyle)) {
                                                                                    Styles = ImageStyle.Split(';');
                                                                                    for (Ptr = 0; Ptr <= Styles.GetUpperBound(0); Ptr++) {
                                                                                        Style = Styles[Ptr].Split(':');
                                                                                        if (Style.GetUpperBound(0) > 0) {
                                                                                            StyleName = genericController.vbLCase(Style[0].Trim(' '));
                                                                                            if (StyleName == "width") {
                                                                                                StyleValue = genericController.vbLCase(Style[1].Trim(' '));
                                                                                                StyleValue = genericController.vbReplace(StyleValue, "px", "");
                                                                                                StyleValueInt = genericController.encodeInteger(StyleValue);
                                                                                                if (StyleValueInt > 0) {
                                                                                                    ImageWidth = StyleValueInt;
                                                                                                }
                                                                                            } else if (StyleName == "height") {
                                                                                                StyleValue = genericController.vbLCase(Style[1].Trim(' '));
                                                                                                StyleValue = genericController.vbReplace(StyleValue, "px", "");
                                                                                                StyleValueInt = genericController.encodeInteger(StyleValue);
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
                                                                                libraryFilesModel file = libraryFilesModel.create(cpCore, RecordID);
                                                                                if (file != null) {
                                                                                    RecordVirtualFilename = file.Filename;
                                                                                    RecordWidth = file.pxWidth;
                                                                                    RecordHeight = file.pxHeight;
                                                                                    RecordAltSizeList = file.AltSizeList;
                                                                                    RecordFilename = RecordVirtualFilename;
                                                                                    Pos = RecordVirtualFilename.LastIndexOf("/") + 1;
                                                                                    if (Pos > 0) {
                                                                                        RecordFilename = RecordVirtualFilename.Substring(Pos);
                                                                                    }
                                                                                    RecordFilenameExt = "";
                                                                                    RecordFilenameNoExt = RecordFilename;
                                                                                    Pos = RecordFilenameNoExt.LastIndexOf(".") + 1;
                                                                                    if (Pos > 0) {
                                                                                        RecordFilenameExt = RecordFilenameNoExt.Substring(Pos);
                                                                                        RecordFilenameNoExt = RecordFilenameNoExt.Left(Pos - 1);
                                                                                    }
                                                                                    //
                                                                                    // if recordwidth or height are missing, get them from the file
                                                                                    //
                                                                                    if (RecordWidth == 0 || RecordHeight == 0) {
                                                                                        sf = new imageEditController();
                                                                                        if (sf.load(genericController.convertCdnUrlToCdnPathFilename(ImageVirtualFilename))) {
                                                                                            file.pxWidth = sf.width;
                                                                                            file.pxHeight = sf.height;
                                                                                            file.save(cpCore);
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
                                                                                            sf = new imageEditController();
                                                                                            if (sf.load(genericController.convertCdnUrlToCdnPathFilename(ImageVirtualFilename))) {
                                                                                                ImageWidth = sf.width;
                                                                                                ImageHeight = sf.height;
                                                                                            }
                                                                                            sf.Dispose();
                                                                                            sf = null;
                                                                                            if ((ImageHeight == 0) && (ImageWidth == 0) && (!string.IsNullOrEmpty(ImageFilenameAltSize))) {
                                                                                                Pos = genericController.vbInstr(1, ImageFilenameAltSize, "x");
                                                                                                if (Pos != 0) {
                                                                                                    ImageWidth = genericController.encodeInteger(ImageFilenameAltSize.Left(Pos - 1));
                                                                                                    ImageHeight = genericController.encodeInteger(ImageFilenameAltSize.Substring(Pos));
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
                                                                                        ImageAltSize = ImageWidth.ToString() + "x" + ImageHeight.ToString();
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
                                                                                            imageNewLink = genericController.EncodeURL(genericController.getCdnFileLink(cpCore, ImageVirtualFilePath) + NewImageFilename);
                                                                                            ElementText = genericController.vbReplace(ElementText, ImageSrcOriginal, genericController.encodeHTML(imageNewLink));
                                                                                        } else if ((RecordWidth < ImageWidth) || (RecordHeight < ImageHeight)) {
                                                                                            //
                                                                                            // OK
                                                                                            // reize image larger then original - go with it as is
                                                                                            //
                                                                                            // images included in email have spaces that must be converted to "%20" or they 404
                                                                                            ElementText = genericController.vbReplace(ElementText, ImageSrcOriginal, genericController.encodeHTML(genericController.EncodeURL(genericController.getCdnFileLink(cpCore, RecordVirtualFilename))));
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
                                                                                                    sf = new imageEditController();
                                                                                                    if (!sf.load(genericController.convertCdnUrlToCdnPathFilename(RecordVirtualFilename))) {
                                                                                                        //
                                                                                                        // image load failed, use raw filename
                                                                                                        //
                                                                                                        throw (new ApplicationException("Unexpected exception")); //cpCore.handleLegacyError3(cpCore.serverConfig.appConfig.name, "Error while loading image to resize, [" & RecordVirtualFilename & "]", "dll", "cpCoreClass", "DecodeAciveContent", Err.Number, Err.Source, Err.Description, False, True, "")
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
                                                                                                        if (genericController.vbInstr(1, ElementText, "height=", 1) == 0) {
                                                                                                            ElementText = genericController.vbReplace(ElementText, ">", " height=\"" + ImageHeight + "\">");
                                                                                                        }
                                                                                                        if (genericController.vbInstr(1, ElementText, "width=", 1) == 0) {
                                                                                                            ElementText = genericController.vbReplace(ElementText, ">", " width=\"" + ImageWidth + "\">");
                                                                                                        }
                                                                                                        //
                                                                                                        // Save new file
                                                                                                        //
                                                                                                        NewImageFilename = RecordFilenameNoExt + "-" + ImageAltSize + "." + RecordFilenameExt;
                                                                                                        sf.save(genericController.convertCdnUrlToCdnPathFilename(ImageVirtualFilePath + NewImageFilename));
                                                                                                        //
                                                                                                        // Update image record
                                                                                                        //
                                                                                                        RecordAltSizeList = RecordAltSizeList + "\r\n" + ImageAltSize;
                                                                                                    }
                                                                                                }
                                                                                                //
                                                                                                // Change the image src to the AltSize
                                                                                                ElementText = genericController.vbReplace(ElementText, ImageSrcOriginal, genericController.encodeHTML(genericController.EncodeURL(genericController.getCdnFileLink(cpCore, ImageVirtualFilePath) + NewImageFilename)));
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                                file.save(cpCore);
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
                cpCore.handleException(ex);
            }
            return result;
            //
        }
        //
        //===================================================================================================
        // To support the special case when the template calls this to encode itself, and the page content has already been rendered.
        //
        private static string convertActiveContent_internal(coreClass cpCore, string Source, int personalizationPeopleId, string ContextContentName, int ContextRecordID, int ContextContactPeopleID, bool PlainText, bool AddLinkEID, bool EncodeActiveFormatting, bool EncodeActiveImages, bool EncodeActiveEditIcons, bool EncodeActivePersonalization, string queryStringForLinkAppend, string ProtocolHostLink, bool IsEmailContent, int ignore_DefaultWrapperID, string ignore_TemplateCaseOnly_Content, CPUtilsBaseClass.addonContext Context, bool personalizationIsAuthenticated, object nothingObject, bool isEditingAnything) {
            string result = Source;
            try {
                //
                const string StartFlag = "<!-- ADDON";
                const string EndFlag = " -->";
                //
                bool DoAnotherPass = false;
                int ArgCnt = 0;
                string AddonGuid = null;
                string ACInstanceID = null;
                string[] ArgSplit = null;
                string AddonName = null;
                string addonOptionString = null;
                int LineStart = 0;
                int LineEnd = 0;
                string Copy = null;
                string[] Wrapper = null;
                string[] SegmentSplit = null;
                string AcCmd = null;
                string SegmentSuffix = null;
                string[] AcCmdSplit = null;
                string ACType = null;
                string[] ContentSplit = null;
                int ContentSplitCnt = 0;
                string Segment = null;
                int Ptr = 0;
                string CopyName = null;
                string ListName = null;
                string SortField = null;
                bool SortReverse = false;
                string AdminURL = null;
                //
                int iPersonalizationPeopleId = personalizationPeopleId;
                if (iPersonalizationPeopleId == 0) {
                    iPersonalizationPeopleId = cpCore.doc.sessionContext.user.id;
                }
                //

                //hint = "csv_EncodeContent9 enter"
                if (!string.IsNullOrEmpty(result)) {
                    AdminURL = "/" + cpCore.serverConfig.appConfig.adminRoute;
                    //
                    //--------
                    // cut-paste from csv_EncodeContent8
                    //--------
                    //
                    // ----- Do EncodeCRLF Conversion
                    //
                    //hint = hint & ",010"
                    if (cpCore.siteProperties.getBoolean("ConvertContentCRLF2BR", false) && (!PlainText)) {
                        result = genericController.vbReplace(result, "\r", "");
                        result = genericController.vbReplace(result, "\n", "<br>");
                    }
                    //
                    // ----- Do upgrade conversions (upgrade legacy objects and upgrade old images)
                    //
                    //hint = hint & ",020"
                    result = upgradeActiveContent(cpCore , result);
                    //
                    // ----- Do Active Content Conversion
                    //
                    //hint = hint & ",030"
                    if (AddLinkEID || EncodeActiveFormatting || EncodeActiveImages || EncodeActiveEditIcons) {
                        result = convertActiveContent_Internal_activeParts(cpCore, result, iPersonalizationPeopleId, ContextContentName, ContextRecordID, ContextContactPeopleID, AddLinkEID, EncodeActiveFormatting, EncodeActiveImages, EncodeActiveEditIcons, EncodeActivePersonalization, queryStringForLinkAppend, ProtocolHostLink, IsEmailContent, AdminURL, personalizationIsAuthenticated, Context);
                    }
                    //
                    // ----- Do Plain Text Conversion
                    //
                    if (PlainText) {
                        result = NUglify.Uglify.HtmlToText(result).Code; // htmlToTextControllers.convert(cpCore, result);
                    }
                    //
                    // Process Active Content that must be run here to access webclass objects
                    //     parse as {{functionname?querystring}}
                    //
                    if ((!EncodeActiveEditIcons) && (result.IndexOf("{{") != -1)) {
                        ContentSplit = genericController.stringSplit(result, "{{");
                        result = "";
                        ContentSplitCnt = ContentSplit.GetUpperBound(0) + 1;
                        Ptr = 0;
                        while (Ptr < ContentSplitCnt) {
                            //hint = hint & ",200"
                            Segment = ContentSplit[Ptr];
                            if (Ptr == 0) {
                                //
                                // Add in the non-command text that is before the first command
                                //
                                result = result + Segment;
                            } else if (!string.IsNullOrEmpty(Segment)) {
                                if (genericController.vbInstr(1, Segment, "}}") == 0) {
                                    //
                                    // No command found, return the marker and deliver the Segment
                                    //
                                    //hint = hint & ",210"
                                    result = result + "{{" + Segment;
                                } else {
                                    //
                                    // isolate the command
                                    //
                                    //hint = hint & ",220"
                                    SegmentSplit = genericController.stringSplit(Segment, "}}");
                                    AcCmd = SegmentSplit[0];
                                    SegmentSplit[0] = "";
                                    SegmentSuffix = string.Join("}}", SegmentSplit).Substring(2);
                                    if (!string.IsNullOrEmpty(AcCmd.Trim(' '))) {
                                        //
                                        // isolate the arguments
                                        //
                                        //hint = hint & ",230"
                                        AcCmdSplit = AcCmd.Split('?');
                                        ACType = AcCmdSplit[0].Trim(' ');
                                        if (AcCmdSplit.GetUpperBound(0) == 0) {
                                            addonOptionString = "";
                                        } else {
                                            addonOptionString = AcCmdSplit[1];
                                            addonOptionString = genericController.decodeHtml(addonOptionString);
                                        }
                                        //
                                        // execute the command
                                        //
                                        switch (genericController.vbUCase(ACType)) {
                                            case ACTypeDynamicForm:
                                                //
                                                // Dynamic Form - run the core addon replacement instead
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
                                                    personalizationPeopleId = iPersonalizationPeopleId,
                                                    instanceArguments = genericController.convertAddonArgumentstoDocPropertiesList(cpCore, addonOptionString)
                                                };
                                                addonModel addon = addonModel.create(cpCore, addonGuidDynamicForm);
                                                result += cpCore.addon.execute(addon, executeContext);
                                                break;
                                            case ACTypeChildList:
                                                //
                                                // Child Page List
                                                //
                                                //hint = hint & ",320"
                                                ListName = addonController.getAddonOption("name", addonOptionString);
                                                result = result + cpCore.doc.getChildPageList(ListName, ContextContentName, ContextRecordID, true);
                                                break;
                                            case ACTypeTemplateText:
                                                //
                                                // Text Box = copied here from gethtmlbody
                                                //
                                                CopyName = addonController.getAddonOption("new", addonOptionString);
                                                if (string.IsNullOrEmpty(CopyName)) {
                                                    CopyName = addonController.getAddonOption("name", addonOptionString);
                                                    if (string.IsNullOrEmpty(CopyName)) {
                                                        CopyName = "Default";
                                                    }
                                                }
                                                result = result + cpCore.html.getContentCopy(CopyName, "", iPersonalizationPeopleId, false, personalizationIsAuthenticated);
                                                break;
                                            case ACTypeWatchList:
                                                //
                                                // Watch List
                                                //
                                                //hint = hint & ",330"
                                                ListName = addonController.getAddonOption("LISTNAME", addonOptionString);
                                                SortField = addonController.getAddonOption("SORTFIELD", addonOptionString);
                                                SortReverse = genericController.encodeBoolean(addonController.getAddonOption("SORTDIRECTION", addonOptionString));
                                                result = result + cpCore.doc.main_GetWatchList(cpCore, ListName, SortField, SortReverse);
                                                break;
                                            default:
                                                //
                                                // Unrecognized command - put all the syntax back in
                                                //
                                                //hint = hint & ",340"
                                                result = result + "{{" + AcCmd + "}}";
                                                break;
                                        }
                                    }
                                    //
                                    // add the SegmentSuffix back on
                                    //
                                    result = result + SegmentSuffix;
                                }
                            }
                            //
                            // Encode into Javascript if required
                            //
                            Ptr = Ptr + 1;
                        }
                    }
                    //
                    // Process Addons
                    //   parse as <!-- Addon "Addon Name","OptionString" -->
                    //   They are handled here because Addons are written against cpCoreClass, not the Content Server class
                    //   ...so Group Email can not process addons 8(
                    //   Later, remove the csv routine that translates <ac to this, and process it directly right here
                    //   Later, rewrite so addons call csv, not cpCoreClass, so email processing can include addons
                    // (2/16/2010) - move csv_EncodeContent to csv, or wait and move it all to CP
                    //    eventually, everything should migrate to csv and/or cp to eliminate the cpCoreClass dependancy
                    //    and all add-ons run as processes the same as they run on pages, or as remote methods
                    // (2/16/2010) - if <!-- AC --> has four arguments, the fourth is the addon guid
                    //
                    if (result.IndexOf(StartFlag) != -1) {
                        while (result.IndexOf(StartFlag) != -1) {
                            LineStart = genericController.vbInstr(1, result, StartFlag);
                            LineEnd = genericController.vbInstr(LineStart, result, EndFlag);
                            if (LineEnd == 0) {
                                logController.appendLog(cpCore, "csv_EncodeContent9, Addon could not be inserted into content because the HTML comment holding the position is not formated correctly");
                                break;
                            } else {
                                AddonName = "";
                                addonOptionString = "";
                                ACInstanceID = "";
                                AddonGuid = "";
                                Copy = result.Substring(LineStart + 10, LineEnd - LineStart - 11);
                                ArgSplit = genericController.SplitDelimited(Copy, ",");
                                ArgCnt = ArgSplit.GetUpperBound(0) + 1;
                                if (!string.IsNullOrEmpty(ArgSplit[0])) {
                                    AddonName = ArgSplit[0].Substring(1, ArgSplit[0].Length - 2);
                                    if (ArgCnt > 1) {
                                        if (!string.IsNullOrEmpty(ArgSplit[1])) {
                                            addonOptionString = ArgSplit[1].Substring(1, ArgSplit[1].Length - 2);
                                            addonOptionString = genericController.decodeHtml(addonOptionString.Trim(' '));
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
                                        personalizationPeopleId = iPersonalizationPeopleId,
                                        instanceGuid = ACInstanceID,
                                        instanceArguments = genericController.convertAddonArgumentstoDocPropertiesList(cpCore, addonOptionString)
                                    };
                                    if (!string.IsNullOrEmpty(AddonGuid)) {
                                        Copy = cpCore.addon.execute(Models.DbModels.addonModel.create(cpCore, AddonGuid), executeContext);
                                        //Copy = cpCore.addon.execute_legacy6(0, AddonGuid, addonOptionString, CPUtilsBaseClass.addonContext.ContextPage, ContextContentName, ContextRecordID, "", ACInstanceID, False, ignore_DefaultWrapperID, ignore_TemplateCaseOnly_Content, False, Nothing, "", Nothing, "", iPersonalizationPeopleId, personalizationIsAuthenticated)
                                    } else {
                                        Copy = cpCore.addon.execute(Models.DbModels.addonModel.createByName(cpCore, AddonName), executeContext);
                                        //Copy = cpCore.addon.execute_legacy6(0, AddonName, addonOptionString, CPUtilsBaseClass.addonContext.ContextPage, ContextContentName, ContextRecordID, "", ACInstanceID, False, ignore_DefaultWrapperID, ignore_TemplateCaseOnly_Content, False, Nothing, "", Nothing, "", iPersonalizationPeopleId, personalizationIsAuthenticated)
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
                    if ((!isEditingAnything) && (result != BlockTextStartMarker)) {
                        DoAnotherPass = true;
                        while ((result.IndexOf(BlockTextStartMarker, System.StringComparison.OrdinalIgnoreCase) != -1) && DoAnotherPass) {
                            LineStart = genericController.vbInstr(1, result, BlockTextStartMarker, 1);
                            if (LineStart == 0) {
                                DoAnotherPass = false;
                            } else {
                                LineEnd = genericController.vbInstr(LineStart, result, BlockTextEndMarker, 1);
                                if (LineEnd <= 0) {
                                    DoAnotherPass = false;
                                    result = result.Left(LineStart - 1);
                                } else {
                                    LineEnd = genericController.vbInstr(LineEnd, result, " -->");
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
                    //
                    // only valid for a webpage
                    //
                    if (true) {
                        //
                        // Add in EditWrappers for Aggregate scripts and replacements
                        //   This is also old -- used here because csv encode content can create replacements and links, but can not
                        //   insert wrappers. This is all done in GetAddonContents() now. This routine is left only to
                        //   handle old style calls in cache.
                        //
                        //hint = hint & ",500, Adding edit wrappers"
                        if (isEditingAnything) {
                            if (result.IndexOf("<!-- AFScript -->", System.StringComparison.OrdinalIgnoreCase) != -1) {
                                //throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError7("returnValue", "AFScript Style edit wrappers are not supported")
                                Copy = cpCore.html.getEditWrapper("Aggregate Script", "##MARKER##");
                                Wrapper = genericController.stringSplit(Copy, "##MARKER##");
                                result = genericController.vbReplace(result, "<!-- AFScript -->", Wrapper[0], 1, 99, 1);
                                result = genericController.vbReplace(result, "<!-- /AFScript -->", Wrapper[1], 1, 99, 1);
                            }
                            if (result.IndexOf("<!-- AFReplacement -->", System.StringComparison.OrdinalIgnoreCase) != -1) {
                                //throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError7("returnValue", "AFReplacement Style edit wrappers are not supported")
                                Copy = cpCore.html.getEditWrapper("Aggregate Replacement", "##MARKER##");
                                Wrapper = genericController.stringSplit(Copy, "##MARKER##");
                                result = genericController.vbReplace(result, "<!-- AFReplacement -->", Wrapper[0], 1, 99, 1);
                                result = genericController.vbReplace(result, "<!-- /AFReplacement -->", Wrapper[1], 1, 99, 1);
                            }
                        }
                        //
                        // Process Feedback form
                        //
                        //hint = hint & ",600, Handle webclient features"
                        if (genericController.vbInstr(1, result, FeedbackFormNotSupportedComment, 1) != 0) {
                            result = genericController.vbReplace(result, FeedbackFormNotSupportedComment, pageContentController.main_GetFeedbackForm(cpCore, ContextContentName, ContextRecordID, ContextContactPeopleID), 1, 99, 1);
                        }
                        //
                        // If any javascript or styles were added during encode, pick them up now
                        //
                        //Copy = cpCore.doc.getNextJavascriptBodyEnd()
                        //Do While Copy <> ""
                        //    Call addScriptCode_body(Copy, "embedded content")
                        //    Copy = cpCore.doc.getNextJavascriptBodyEnd()
                        //Loop
                        //
                        // current
                        //
                        //Copy = cpCore.doc.getNextJSFilename()
                        //Do While Copy <> ""
                        //    If genericController.vbInstr(1, Copy, "://") <> 0 Then
                        //    ElseIf Left(Copy, 1) = "/" Then
                        //    Else
                        //        Copy = cpCore.webServer.requestProtocol & cpCore.webServer.requestDomain & genericController.getCdnFileLink(cpCore, Copy)
                        //    End If
                        //    Call addScriptLink_Head(Copy, "embedded content")
                        //    Copy = cpCore.doc.getNextJSFilename()
                        //Loop
                        //
                        //Copy = cpCore.doc.getJavascriptOnLoad()
                        //Do While Copy <> ""
                        //    Call addOnLoadJs(Copy, "")
                        //    Copy = cpCore.doc.getJavascriptOnLoad()
                        //Loop
                        //
                        //Copy = cpCore.doc.getNextStyleFilenames()
                        //Do While Copy <> ""
                        //    If genericController.vbInstr(1, Copy, "://") <> 0 Then
                        //    ElseIf Left(Copy, 1) = "/" Then
                        //    Else
                        //        Copy = cpCore.webServer.requestProtocol & cpCore.webServer.requestDomain & genericController.getCdnFileLink(cpCore, Copy)
                        //    End If
                        //    Call addStyleLink(Copy, "")
                        //    Copy = cpCore.doc.getNextStyleFilenames()
                        //Loop
                    }
                }
                //
                //result = result;
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            return result;
        }
        //
        // ================================================================================================================
        //   Upgrade old objects in content, and update changed resource library images
        // ================================================================================================================
        //
        public static string upgradeActiveContent( coreClass cpCore, string Source) {
            string result = Source;
            try {
                string RecordVirtualPath = "";
                string RecordVirtualFilename = null;
                string RecordFilename = null;
                string RecordFilenameNoExt = null;
                string RecordFilenameExt = "";
                string[] SizeTest = null;
                string RecordAltSizeList = null;
                int TagPosEnd = 0;
                int TagPosStart = 0;
                bool InTag = false;
                int Pos = 0;
                string FilenameSegment = null;
                int EndPos1 = 0;
                int EndPos2 = 0;
                string[] LinkSplit = null;
                int LinkCnt = 0;
                int LinkPtr = 0;
                string[] TableSplit = null;
                string TableName = null;
                string FieldName = null;
                int RecordID = 0;
                bool SaveChanges = false;
                int EndPos = 0;
                int Ptr = 0;
                string FilePrefixSegment = null;
                bool ImageAllowUpdate = false;
                string ContentFilesLinkPrefix = null;
                string ResourceLibraryLinkPrefix = null;
                string TestChr = null;
                bool ParseError = false;
                result = Source;
                //
                ContentFilesLinkPrefix = "/" + cpCore.serverConfig.appConfig.name + "/files/";
                ResourceLibraryLinkPrefix = ContentFilesLinkPrefix + "ccLibraryFiles/";
                ImageAllowUpdate = cpCore.siteProperties.getBoolean("ImageAllowUpdate", true);
                ImageAllowUpdate = ImageAllowUpdate && (Source.IndexOf(ResourceLibraryLinkPrefix, System.StringComparison.OrdinalIgnoreCase) != -1);
                if (ImageAllowUpdate) {
                    //
                    // ----- Process Resource Library Images (swap in most current file)
                    //
                    //   There is a better way:
                    //   problem with replacing the images is the problem with parsing - too much work to find it
                    //   instead, use new replacement tags <ac type=image src="cclibraryfiles/filename/00001" width=0 height=0>
                    //
                    //hint = hint & ",010"
                    ParseError = false;
                    LinkSplit = genericController.stringSplit(Source, ContentFilesLinkPrefix);
                    LinkCnt = LinkSplit.GetUpperBound(0) + 1;
                    for (LinkPtr = 1; LinkPtr < LinkCnt; LinkPtr++) {
                        //
                        // Each LinkSplit(1...) is a segment that would have started with '/appname/files/'
                        // Next job is to determine if this sement is in a tag (<img src="...">) or in content (&quot...&quote)
                        // For now, skip the ones in content
                        //
                        //hint = hint & ",020"
                        TagPosEnd = genericController.vbInstr(1, LinkSplit[LinkPtr], ">");
                        TagPosStart = genericController.vbInstr(1, LinkSplit[LinkPtr], "<");
                        if (TagPosEnd == 0 && TagPosStart == 0) {
                            //
                            // no tags found, skip it
                            //
                            InTag = false;
                        } else if (TagPosEnd == 0) {
                            //
                            // no end tag, but found a start tag -> in content
                            //
                            InTag = false;
                        } else if (TagPosEnd < TagPosStart) {
                            //
                            // Found end before start - > in tag
                            //
                            InTag = true;
                        } else {
                            //
                            // Found start before end -> in content
                            //
                            InTag = false;
                        }
                        if (InTag) {
                            //hint = hint & ",030"
                            TableSplit = LinkSplit[LinkPtr].Split('/');
                            if (TableSplit.GetUpperBound(0) > 2) {
                                TableName = TableSplit[0];
                                FieldName = TableSplit[1];
                                RecordID = genericController.encodeInteger(TableSplit[2]);
                                FilenameSegment = TableSplit[3];
                                if ((TableName.ToLower() == "cclibraryfiles") && (FieldName.ToLower() == "filename") && (RecordID != 0)) {
                                    libraryFilesModel file = libraryFilesModel.create(cpCore, RecordID);
                                    if (file != null) {
                                        //hint = hint & ",060"
                                        FieldName = "filename";
                                        //SQL = "select filename,altsizelist from " & TableName & " where id=" & RecordID
                                        //CS = app.csv_OpenCSSQL("default", SQL)
                                        //If app.csv_IsCSOK(CS) Then
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
                                            RecordVirtualFilename = file.Filename;
                                            RecordAltSizeList = file.AltSizeList;
                                            if (RecordVirtualFilename == genericController.EncodeJavascript(RecordVirtualFilename)) {
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
                                                Pos = RecordVirtualFilename.LastIndexOf("/") + 1;
                                                RecordFilename = "";
                                                if (Pos > 0) {
                                                    RecordVirtualPath = RecordVirtualFilename.Left(Pos);
                                                    RecordFilename = RecordVirtualFilename.Substring(Pos);
                                                }
                                                Pos = RecordFilename.LastIndexOf(".") + 1;
                                                RecordFilenameNoExt = "";
                                                if (Pos > 0) {
                                                    RecordFilenameExt = genericController.vbLCase(RecordFilename.Substring(Pos));
                                                    RecordFilenameNoExt = genericController.vbLCase(RecordFilename.Left(Pos - 1));
                                                }
                                                FilePrefixSegment = LinkSplit[LinkPtr - 1];
                                                if (FilePrefixSegment.Length > 1) {
                                                    //
                                                    // Look into FilePrefixSegment and see if we are in the querystring attribute of an <AC tag
                                                    //   if so, the filename may be AddonEncoded and delimited with & (so skip it)
                                                    Pos = FilePrefixSegment.LastIndexOf("<") + 1;
                                                    if (Pos > 0) {
                                                        if (genericController.vbLCase(FilePrefixSegment.Substring(Pos, 3)) != "ac ") {
                                                            //
                                                            // look back in the FilePrefixSegment to find the character before the link
                                                            //
                                                            EndPos = 0;
                                                            for (Ptr = FilePrefixSegment.Length; Ptr >= 1; Ptr--) {
                                                                TestChr = FilePrefixSegment.Substring(Ptr - 1, 1);
                                                                switch (TestChr) {
                                                                    case "=":
                                                                        //
                                                                        // Ends in ' ' or '>', find the first
                                                                        //
                                                                        EndPos1 = genericController.vbInstr(1, FilenameSegment, " ");
                                                                        EndPos2 = genericController.vbInstr(1, FilenameSegment, ">");
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
                                                                        EndPos = genericController.vbInstr(1, FilenameSegment, "\"");
                                                                        //todo  WARNING: Exit statements not matching the immediately enclosing block are converted using a 'goto' statement:
                                                                        //ORIGINAL LINE: Exit For
                                                                        goto ExitLabel1;
                                                                    case "(":
                                                                        //
                                                                        // url() style, ends in ')' or a ' '
                                                                        //
                                                                        if (genericController.vbLCase(FilePrefixSegment.Substring(Ptr - 1, 7)) == "(&quot;") {
                                                                            EndPos = genericController.vbInstr(1, FilenameSegment, "&quot;)");
                                                                        } else if (genericController.vbLCase(FilePrefixSegment.Substring(Ptr - 1, 2)) == "('") {
                                                                            EndPos = genericController.vbInstr(1, FilenameSegment, "')");
                                                                        } else if (genericController.vbLCase(FilePrefixSegment.Substring(Ptr - 1, 2)) == "(\"") {
                                                                            EndPos = genericController.vbInstr(1, FilenameSegment, "\")");
                                                                        } else {
                                                                            EndPos = genericController.vbInstr(1, FilenameSegment, ")");
                                                                        }
                                                                        //todo  WARNING: Exit statements not matching the immediately enclosing block are converted using a 'goto' statement:
                                                                        //ORIGINAL LINE: Exit For
                                                                        goto ExitLabel1;
                                                                    case "'":
                                                                        //
                                                                        // Delimited within a javascript pair of apostophys
                                                                        //
                                                                        EndPos = genericController.vbInstr(1, FilenameSegment, "'");
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
                                                                ImageFilename = genericController.DecodeResponseVariable(FilenameSegment.Left(EndPos - 1));
                                                                ImageFilenameNoExt = ImageFilename;
                                                                ImageFilenameExt = "";
                                                                Pos = ImageFilename.LastIndexOf(".") + 1;
                                                                if (Pos > 0) {
                                                                    ImageFilenameNoExt = genericController.vbLCase(ImageFilename.Left(Pos - 1));
                                                                    ImageFilenameExt = genericController.vbLCase(ImageFilename.Substring(Pos));
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
                                                                } else if (genericController.vbInstr(1, ImageFilenameNoExt, RecordFilenameNoExt, 1) != 1) {
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
                                                                        SizeTest = ImageAltSize.Split('x');
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
                                                                    NewRecordFilename = genericController.EncodeURL(NewRecordFilename) + ((string)(string.Join("/", TableSplit))).Substring(3);
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
                    //
                    // Convert ACTypeDynamicForm to Add-on
                    //
                    if (genericController.vbInstr(1, result, "<ac type=\"" + ACTypeDynamicForm, 1) != 0) {
                        result = genericController.vbReplace(result, "type=\"DYNAMICFORM\"", "TYPE=\"aggregatefunction\"", 1, 99, 1);
                        result = genericController.vbReplace(result, "name=\"DYNAMICFORM\"", "name=\"DYNAMIC FORM\"", 1, 99, 1);
                    }
                }
                //hint = hint & ",930"
                if (ParseError) {
                    result = ""
                    + "\r\n<!-- warning: parsing aborted on ccLibraryFile replacement -->"
                    + "\r\n" + result + "\r\n<!-- /warning: parsing aborted on ccLibraryFile replacement -->";
                }
                //
                // {{content}} should be <ac type="templatecontent" etc>
                // the merge is now handled in csv_EncodeActiveContent, but some sites have hand {{content}} tags entered
                //
                //hint = hint & ",940"
                if (genericController.vbInstr(1, result, "{{content}}", 1) != 0) {
                    result = genericController.vbReplace(result, "{{content}}", "<AC type=\"" + ACTypeTemplateContent + "\">", 1, 99, 1);
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Convert an active content field (html data stored with <ac></ac> html tags) to a wysiwyg editor request (html with edit icon <img> for <ac></ac>)
        /// </summary>
        /// <param name="editorValue"></param>
        /// <returns></returns>
        public static string convertActiveContentToHtmlForWysiwygEditor(coreClass cpCore, string editorValue) {
            return convertActiveContent_internal(cpCore, editorValue, 0, "", 0, 0, false, false, false, true, true, false, "", "", false, 0, "", Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextSimple, false, null, false);
        }
        //
        //====================================================================================================
        //
        public static string convertActiveContentToJsonForRemoteMethod(coreClass cpCore, string Source, string ContextContentName, int ContextRecordID, int ContextContactPeopleID, string ProtocolHostString, int DefaultWrapperID, string ignore_TemplateCaseOnly_Content, CPUtilsBaseClass.addonContext addonContext) {
            return convertActiveContent_internal(cpCore, Source, cpCore.doc.sessionContext.user.id, ContextContentName, ContextRecordID, ContextContactPeopleID, false, false, true, true, false, true, "", ProtocolHostString, false, DefaultWrapperID, ignore_TemplateCaseOnly_Content, addonContext, cpCore.doc.sessionContext.isAuthenticated, null, cpCore.doc.sessionContext.isEditingAnything());
            //False, False, True, True, False, True, ""
        }
        //
        //====================================================================================================
        //
        public static string convertActiveContentToHtmlForWebRender(coreClass cpCore, string Source, string ContextContentName, int ContextRecordID, int ContextContactPeopleID, string ProtocolHostString, int DefaultWrapperID, CPUtilsBaseClass.addonContext addonContext) {
            return convertActiveContent_internal(cpCore, Source, cpCore.doc.sessionContext.user.id, ContextContentName, ContextRecordID, ContextContactPeopleID, false, false, true, true, false, true, "", ProtocolHostString, false, DefaultWrapperID, "", addonContext, cpCore.doc.sessionContext.isAuthenticated, null, cpCore.doc.sessionContext.isEditingAnything());
            //False, False, True, True, False, True, ""
        }
        //
        //====================================================================================================
        //
        public static  string convertActiveContentToHtmlForEmailSend(coreClass cpCore, string Source, int personalizationPeopleID, string queryStringForLinkAppend) {
            return convertActiveContent_internal(cpCore, Source, personalizationPeopleID, "", 0, 0, false, true, true, true, false, true, queryStringForLinkAppend, "", true, 0, "", CPUtilsBaseClass.addonContext.ContextEmail, true, null, false);
            //False, False, True, True, False, True, ""
        }
    }
}
