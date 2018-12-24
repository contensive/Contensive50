
using System;
using System.Collections.Generic;
using Contensive.BaseClasses;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;

namespace Contensive.Processor {
    public class CPHtmlClass : BaseClasses.CPHtmlBaseClass, IDisposable {
        //
        #region COM GUIDs
        public const string ClassId = "637E3815-0DA6-4672-84E9-A319D85F2101";
        public const string InterfaceId = "24267471-9CE4-44F9-B4BD-8E9CE357D6E6";
        public const string EventsId = "4021B791-0F55-4841-90AE-64C7FAFB9756";
        #endregion
        /// <summary>
        /// dependencies
        /// </summary>
        private CPClass cp;
        //
        // ====================================================================================================
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cpParent"></param>
        //
        public CPHtmlClass(CPClass cpParent) : base() {
            cp = cpParent;
        }
        //
        // ====================================================================================================
        //
        public override void AddEvent(string htmlId, string domEvent, string javaScript) {
            cp.core.html.javascriptAddEvent(htmlId, domEvent, javaScript);
        }
        //
        // ====================================================================================================
        //
        public override string AdminHint(string innerHtml) {
            if (cp.core.session.isEditingAnything() || cp.core.session.user.Admin) {
                return ""
                    + "<div class=\"ccHintWrapper\">"
                        + "<div  class=\"ccHintWrapperContent\">"
                        + "<b>Administrator</b>"
                        + "<br>"
                        + "<br>" + GenericController.encodeText(innerHtml) + "</div>"
                    + "</div>";
            }
            return string.Empty;
        }
        //
        // ====================================================================================================
        //
        public override string Button(string htmlName, string htmlValue, string htmlClass, string htmlId) {
            return HtmlController.getHtmlInputSubmit(htmlValue, htmlName, htmlId, "", false, htmlClass);
        }
        //
        public override string Button(string htmlName, string htmlValue, string htmlClass) => Button(htmlName, htmlValue, htmlClass, "");
        //
        public override string Button(string htmlName, string htmlValue) => Button(htmlName, htmlValue, "", "");
        //
        public override string Button(string htmlName) => Button(htmlName, "", "", "");
        //
        // ====================================================================================================
        //
        public override string CheckBox(string htmlName, bool htmlValue, string htmlClass, string htmlId) {
            return HtmlController.checkbox(htmlName, htmlValue, htmlId, false, htmlClass);
        }
        //
        public override string CheckBox(string htmlName, bool htmlValue, string htmlClass) => CheckBox(htmlName, htmlValue, htmlClass, "");
        //
        public override string CheckBox(string htmlName, bool htmlValue) => CheckBox(htmlName, htmlValue, "", "");
        //
        public override string CheckBox(string htmlName) => CheckBox(htmlName, false, "", "");
        //
        // ====================================================================================================
        //
        public override string Div(string innerHtml, string htmlClass, string htmlId) => HtmlController.div(innerHtml, htmlClass, htmlId);
        //
        public override string Div(string innerHtml, string htmlClass) => HtmlController.div(innerHtml, htmlClass);
        //
        public override string Div(string innerHtml) => HtmlController.div(innerHtml);
        //
        // ====================================================================================================
        //
        //public override string Form(string innerHtml, string htmlName = "", string htmlClass = "", string htmlId = "", string actionQueryString = "", string method = "post")
        public override string Form(string innerHtml, string htmlName, string htmlClass, string htmlId, string actionQueryString, string method ) {
            if (method.ToLowerInvariant() == "get") {
                return HtmlController.form(cp.core, innerHtml, actionQueryString, htmlName, htmlId, method);
            } else {
                return HtmlController.formMultipart(cp.core, innerHtml, actionQueryString, htmlName, htmlClass, htmlId);
            }
        }
        //
        public override string Form(string innerHtml, string htmlName, string htmlClass, string htmlId, string actionQueryString) => Form(innerHtml, htmlName, htmlClass, htmlId, actionQueryString, "post");
        //
        public override string Form(string innerHtml, string htmlName, string htmlClass, string htmlId) => Form(innerHtml, htmlName, htmlClass, htmlId, "", "post");
        //
        public override string Form(string innerHtml, string htmlName, string htmlClass) => Form(innerHtml, htmlName, htmlClass, "", "", "post");
        //
        public override string Form(string innerHtml, string htmlName) => Form(innerHtml, htmlName, "", "", "", "post");
        //
        public override string Form(string innerHtml) => Form(innerHtml, "", "", "", "", "post");
        //
        // ==========================================================================================
        //
        public override string H1(string innerHtml, string htmlClass, string htmlId) => HtmlController.h1(innerHtml, htmlClass, htmlId);
        //
        public override string H1(string innerHtml, string htmlClass) => HtmlController.h1(innerHtml, htmlClass);
        //
        public override string H1(string innerHtml) => HtmlController.h1(innerHtml);
        //
        // ==========================================================================================
        //
        public override string H2(string innerHtml, string htmlClass, string htmlId) => HtmlController.genericBlockTag("h2", innerHtml, htmlClass, htmlId);
        //
        public override string H2(string innerHtml, string htmlClass) => HtmlController.genericBlockTag("h2", innerHtml, htmlClass);
        //
        public override string H2(string innerHtml) => HtmlController.genericBlockTag("h2", innerHtml);
        //
        // ==========================================================================================
        //
        public override string H3(string innerHtml, string htmlClass, string htmlId) => HtmlController.genericBlockTag("h3", innerHtml, htmlClass, htmlId);
        //
        public override string H3(string innerHtml, string htmlClass) => HtmlController.genericBlockTag("h3", innerHtml, htmlClass);
        //
        public override string H3(string innerHtml) => HtmlController.genericBlockTag("h3", innerHtml);
        //
        // ==========================================================================================
        //
        public override string H4(string innerHtml, string htmlClass, string htmlId) => HtmlController.genericBlockTag("h4", innerHtml, htmlClass, htmlId);
        //
        public override string H4(string innerHtml, string htmlClass) => HtmlController.genericBlockTag("h4", innerHtml, htmlClass);
        //
        public override string H4(string innerHtml) => HtmlController.genericBlockTag("h4", innerHtml);
        //
        // ==========================================================================================
        //
        public override string H5(string innerHtml, string htmlClass, string htmlId) => HtmlController.genericBlockTag("h5", innerHtml, htmlClass, htmlId);
        //
        public override string H5(string innerHtml, string htmlClass) => HtmlController.genericBlockTag("h5", innerHtml, htmlClass);
        //
        public override string H5(string innerHtml) => HtmlController.genericBlockTag("h5", innerHtml);
        //
        // ==========================================================================================
        //
        public override string H6(string innerHtml, string htmlClass, string htmlId) => HtmlController.genericBlockTag("h6", innerHtml, htmlClass, htmlId);
        //
        public override string H6(string innerHtml, string htmlClass) => HtmlController.genericBlockTag("h6", innerHtml, htmlClass);
        //
        public override string H6(string innerHtml) => HtmlController.genericBlockTag("h6", innerHtml);
        //
        // ====================================================================================================
        //
        public override string Hidden(string htmlName, string htmlValue, string htmlClass, string htmlId) => HtmlController.inputHidden(htmlName, htmlValue, htmlClass, htmlId);
        public override string Hidden(string htmlName, string htmlValue, string htmlClass) => HtmlController.inputHidden(htmlName, htmlValue, htmlClass);
        public override string Hidden(string htmlName, string htmlValue) => HtmlController.inputHidden(htmlName, htmlValue);
        //
        public override string Hidden(string htmlName, int htmlValue, string htmlClass, string htmlId) => HtmlController.inputHidden(htmlName, htmlValue, htmlClass, htmlId);
        public override string Hidden(string htmlName, int htmlValue, string htmlClass) => HtmlController.inputHidden(htmlName, htmlValue, htmlClass);
        public override string Hidden(string htmlName, int htmlValue) => HtmlController.inputHidden(htmlName, htmlValue);
        //
        public override string Hidden(string htmlName, bool htmlValue, string htmlClass, string htmlId) => HtmlController.inputHidden(htmlName, htmlValue, htmlClass, htmlId);
        public override string Hidden(string htmlName, bool htmlValue, string htmlClass) => HtmlController.inputHidden(htmlName, htmlValue, htmlClass);
        public override string Hidden(string htmlName, bool htmlValue) => HtmlController.inputHidden(htmlName, htmlValue);
        //
        public override string Hidden(string htmlName, DateTime htmlValue, string htmlClass, string htmlId) => HtmlController.inputHidden(htmlName, htmlValue, htmlClass, htmlId);
        public override string Hidden(string htmlName, DateTime htmlValue, string htmlClass) => HtmlController.inputHidden(htmlName, htmlValue, htmlClass);
        public override string Hidden(string htmlName, DateTime htmlValue) => HtmlController.inputHidden(htmlName, htmlValue);
        //
        // ====================================================================================================
        //
        public override string Indent(string sourceHtml, int tabCnt) => HtmlController.indent(sourceHtml, tabCnt);
        public override string Indent(string sourceHtml) => HtmlController.indent(sourceHtml);
        //
        // ====================================================================================================
        //
        public override string InputDate(string htmlName, DateTime htmlValue, string htmlClass, string htmlId) => HtmlController.inputDate(cp.core, htmlName, htmlValue, "", htmlId, htmlClass);
        public override string InputDate(string htmlName, DateTime htmlValue, string htmlClass) => HtmlController.inputDate(cp.core, htmlName, htmlValue, "", "", htmlClass);
        public override string InputDate(string htmlName, DateTime htmlValue) => HtmlController.inputDate(cp.core, htmlName, htmlValue);
        public override string InputDate(string htmlName) => HtmlController.inputDate(cp.core, htmlName, null);
        //
        // ====================================================================================================
        //
        public override string InputFile(string htmlName, string htmlClass, string htmlId) => cp.core.html.inputFile(htmlName, htmlId, htmlClass);
        public override string InputFile(string htmlName, string htmlClass) => cp.core.html.inputFile(htmlName, "", htmlClass);
        public override string InputFile(string htmlName) => cp.core.html.inputFile(htmlName);
        //
        // ====================================================================================================
        //
        // todo implement wysiwyg features
        public override string InputHtml(string htmlName, int maxLength, string HtmlValue, string htmlClass, string htmlId, List<SimplestDataModel> addonList) {
            string addonListJSON = cp.core.html.getWysiwygAddonList(CPHtmlBaseClass.EditorContentType.contentTypeWeb);
            return cp.core.html.getFormInputHTML(htmlName, HtmlValue, "", "", false, true, addonListJSON, "", "", false);
        }
        // todo implement wysiwyg features
        public override string InputHtml(string htmlName, int maxLength, string HtmlValue, string htmlClass, string htmlId, bool viewAsHtmlCode) {
            string addonListJSON = cp.core.html.getWysiwygAddonList(CPHtmlBaseClass.EditorContentType.contentTypeWeb);
            return cp.core.html.getFormInputHTML(htmlName, HtmlValue, "", "", false, true, addonListJSON, "", "", false);
        }
        // todo implement wysiwyg features
        public override string InputHtml(string htmlName, int maxLength, string HtmlValue, string htmlClass, string htmlId) {
            string addonListJSON = cp.core.html.getWysiwygAddonList(CPHtmlBaseClass.EditorContentType.contentTypeWeb);
            return cp.core.html.getFormInputHTML(htmlName, HtmlValue, "", "", false, true, addonListJSON, "", "", false);
        }
        // todo implement wysiwyg features
        public override string InputHtml(string htmlName, int maxLength, string HtmlValue, string htmlClass) {
            string addonListJSON = cp.core.html.getWysiwygAddonList(CPHtmlBaseClass.EditorContentType.contentTypeWeb);
            return cp.core.html.getFormInputHTML(htmlName, HtmlValue, "", "", false, true, addonListJSON, "", "", false);
        }
        // todo implement wysiwyg features
        public override string InputHtml(string htmlName, int maxLength, string HtmlValue) {
            string addonListJSON = cp.core.html.getWysiwygAddonList(CPHtmlBaseClass.EditorContentType.contentTypeWeb);
            return cp.core.html.getFormInputHTML(htmlName, HtmlValue, "", "", false, true, addonListJSON, "", "", false);
        }
        // todo implement wysiwyg features
        public override string InputHtml(string htmlName, int maxLength) {
            string addonListJSON = cp.core.html.getWysiwygAddonList(CPHtmlBaseClass.EditorContentType.contentTypeWeb);
            return cp.core.html.getFormInputHTML(htmlName, "", "", "", false, true, addonListJSON, "", "", false);
        }
        //
        // ====================================================================================================
        //
        public override string InputPassword(string htmlName, int maxLength, string htmlValue, string htmlClass, string htmlId) => HtmlController.inputText(cp.core, htmlName, htmlValue, -1, 20, htmlId, true, false, htmlClass, maxLength);
        public override string InputPassword(string htmlName, int maxLength, string htmlValue, string htmlClass) => HtmlController.inputText(cp.core, htmlName, htmlValue, -1, 20, "", true, false, htmlClass, maxLength);
        public override string InputPassword(string htmlName, int maxLength, string htmlValue) => HtmlController.inputText(cp.core, htmlName, htmlValue, -1, 20, "", true, false, "", maxLength);
        public override string InputPassword(string htmlName, int maxLength) => HtmlController.inputText(cp.core, htmlName, "", -1, 20, "", true, false, "", maxLength);
        //
        // ====================================================================================================
        //
        public override string InputText(string htmlName, int maxLength, string htmlValue, string htmlClass, string htmlId) => HtmlController.inputText(cp.core, htmlName, htmlValue, -1, 20, htmlId, false, false, htmlClass, maxLength);
        public override string InputText(string htmlName, int maxLength, string htmlValue, string htmlClass) => HtmlController.inputText(cp.core, htmlName, htmlValue, -1, 20, "", false, false, htmlClass, maxLength);
        public override string InputText(string htmlName, int maxLength, string htmlValue) => HtmlController.inputText(cp.core, htmlName, htmlValue, -1, 20, "", false, false, "", maxLength);
        public override string InputText(string htmlName, int maxLength) => HtmlController.inputText(cp.core, htmlName, "", -1, 20, "", false, false, "", maxLength);
        //
        // ====================================================================================================
        //
        public override string Li(string innerHtml, string htmlClass, string htmlId) => HtmlController.li(innerHtml, htmlClass, htmlId);
        public override string Li(string innerHtml, string htmlClass) => HtmlController.li(innerHtml, htmlClass);
        public override string Li(string innerHtml) => HtmlController.li(innerHtml);
        //
        // ====================================================================================================
        //
        public override string Ol(string innerHtml, string htmlClass, string htmlId) => HtmlController.ol(innerHtml, htmlClass, htmlId);
        public override string Ol(string innerHtml, string htmlClass) => HtmlController.ol(innerHtml, htmlClass);
        public override string Ol(string innerHtml) => HtmlController.ol(innerHtml);
        //
        // ====================================================================================================
        //
        public override string P(string innerHtml, string htmlClass, string htmlId) => HtmlController.p(innerHtml, htmlClass, htmlId);
        public override string P(string innerHtml, string htmlClass) => HtmlController.p(innerHtml, htmlClass);
        public override string P(string innerHtml) => HtmlController.p(innerHtml);
        //
        // ====================================================================================================
        //
        public override string Ul(string innerHtml, string htmlClass, string htmlId) => HtmlController.ul(innerHtml, htmlClass, htmlId);
        public override string Ul(string innerHtml, string htmlClass) => HtmlController.ul(innerHtml, htmlClass);
        public override string Ul(string innerHtml) => HtmlController.ul(innerHtml);












        //
        //
        // ====================================================================================================
        //
        public override void ProcessCheckList(string htmlName, string PrimaryContentName, string PrimaryRecordID, string SecondaryContentName, string RulesContentName, string RulesPrimaryFieldname, string RulesSecondaryFieldName) {
            cp.core.html.processCheckList(htmlName, PrimaryContentName, PrimaryRecordID, SecondaryContentName, RulesContentName, RulesPrimaryFieldname, RulesSecondaryFieldName);
        }









        //
        // ====================================================================================================
        //Inherits BaseClasses.CPHtmlBaseClass.CheckBox
        public override string CheckList(string htmlName, string PrimaryContentName, int PrimaryRecordId, string SecondaryContentName, string RulesContentName, string RulesPrimaryFieldname, string RulesSecondaryFieldName, string SecondaryContentSelectSQLCriteria = "", string CaptionFieldName = "", bool IsReadOnly = false, string htmlClass = "", string htmlId = "") {
            return cp.core.html.getCheckList2(htmlName, PrimaryContentName, PrimaryRecordId, SecondaryContentName, RulesContentName, RulesPrimaryFieldname, RulesSecondaryFieldName, SecondaryContentSelectSQLCriteria, CaptionFieldName, IsReadOnly);
        }
        //
        // ==========================================================================================
        //
        public override string RadioBox(string htmlName, string HtmlValue, string CurrentValue, string htmlClass = "", string htmlId = "") {
            return cp.core.html.inputRadio(htmlName, HtmlValue, CurrentValue, htmlId,htmlClass);
        }
        //
        // ==========================================================================================
        //
        public override string RadioBox(string htmlName, int HtmlValue, int CurrentValue, string htmlClass = "", string htmlId = "") {
            return cp.core.html.inputRadio(htmlName, HtmlValue.ToString(), CurrentValue.ToString(), htmlId, htmlClass);
        }
        //
        // ==========================================================================================
        //
        public override string SelectContent(string htmlName, string HtmlValue, string ContentName, string SQLCriteria = "", string NoneCaption = "", string htmlClass = "", string htmlId = "") {
            string tempSelectContent = null;
            tempSelectContent = cp.core.html.selectFromContent(htmlName, GenericController.encodeInteger(HtmlValue), ContentName, SQLCriteria, NoneCaption);
            if (!string.IsNullOrEmpty(htmlClass)) {
                tempSelectContent = tempSelectContent.Replace("<select ", "<select class=\"" + htmlClass + "\" ");
            }
            if (!string.IsNullOrEmpty(htmlId)) {
                tempSelectContent = tempSelectContent.Replace("<select ", "<select id=\"" + htmlId + "\" ");
            }
            return tempSelectContent;
        }
        //
        // ==========================================================================================
        //
        public override string SelectList(string htmlName, string HtmlValue, string OptionList, string NoneCaption = "", string htmlClass = "", string htmlId = "") {
            string tempSelectList = null;
            tempSelectList = HtmlController.selectFromList( cp.core, htmlName, GenericController.encodeInteger( HtmlValue ), OptionList.Split(','), NoneCaption, htmlId);
            if (!string.IsNullOrEmpty(htmlClass)) {
                tempSelectList = tempSelectList.Replace("<select ", "<select class=\"" + htmlClass + "\" ");
            }
            return tempSelectList;
        }
        //
        //====================================================================================================
        /// <summary>
        /// process an html file element to cdnFiles to the optional path. If the path is ommitted, the path "upload"
        /// </summary>
        [Obsolete("Instead, use cp.cdeFiles.saveUpload() or similar fileSystem object.")]
        public override void ProcessInputFile(string htmlName, string VirtualFilePath = "") {
            string ignoreFilename = "";
            cp.core.cdnFiles.upload(htmlName, VirtualFilePath, ref ignoreFilename);
        }
        //
        // ====================================================================================================
        //
        public override string InputTextExpandable(string htmlName, string HtmlValue = "", int Rows = 0, string StyleWidth = "", bool ignore = false, string htmlClass = "", string htmlId = "") {
            string result = HtmlController.inputTextarea( cp.core,htmlName, HtmlValue, Rows, 20, htmlId, ignore,false,htmlClass);
            if (!string.IsNullOrEmpty(StyleWidth)) {
                result = result.Replace(">", " style=\"width:" + StyleWidth + "\">");
            }
            return result;
        }
        //
        // ====================================================================================================
        //
        public override string SelectUser(string htmlName, int HtmlValue, int GroupId, string NoneCaption = "", string htmlClass = "", string htmlId = "") {
            return cp.core.html.selectUserFromGroup(htmlName, HtmlValue, GroupId, NoneCaption, htmlId);
        }
        //
        // ====================================================================================================
        // deprecated
        [Obsolete("Use uppercase methods", true)]
        public override string div(string innerHtml, string htmlName, string htmlClass, string htmlId) => Div(innerHtml, htmlClass, htmlId);
        //
        [Obsolete("Use uppercase methods", true)]
        public override string div(string innerHtml, string htmlName, string htmlClass) => Div(innerHtml, htmlClass);
        //
        [Obsolete("Use uppercase methods", true)]
        public override string div(string innerHtml, string htmlName) => Div(innerHtml);
        //
        [Obsolete("Use uppercase methods", true)]
        public override string div(string innerHtml) => Div(innerHtml);
        //
        [Obsolete("Use uppercase methods", true)]
        public override string adminHint(string innerHtml) => AdminHint(innerHtml);
        //
        [Obsolete("Use uppercase methods", true)]
        public override string h1(string innerHtml, string htmlName, string htmlClass, string htmlId) => H1(innerHtml, htmlClass, htmlId);
        //
        [Obsolete("Use uppercase methods", true)]
        public override string h1(string innerHtml, string htmlName, string htmlClass) => H1(innerHtml, htmlClass);
        //
        [Obsolete("Use uppercase methods", true)]
        public override string h1(string innerHtml, string htmlName) => H1(innerHtml);
        //
        [Obsolete("Use uppercase methods", true)]
        public override string h1(string innerHtml) => H1(innerHtml);
        //
        [Obsolete("Use uppercase methods", true)]
        public override string h2(string innerHtml, string htmlName, string htmlClass, string htmlId) => H2(innerHtml, htmlClass, htmlId);
        //
        [Obsolete("Use uppercase methods", true)]
        public override string h2(string innerHtml, string htmlName, string htmlClass) => H2(innerHtml, htmlClass);
        //
        [Obsolete("Use uppercase methods", true)]
        public override string h2(string innerHtml, string htmlName) => H2(innerHtml);
        //
        [Obsolete("Use uppercase methods", true)]
        public override string h2(string innerHtml) => H2(innerHtml);
        //
        [Obsolete("Use uppercase methods", true)]
        public override string h3(string innerHtml, string htmlName, string htmlClass, string htmlId) => H3(innerHtml, htmlClass, htmlId);
        //
        [Obsolete("Use uppercase methods", true)]
        public override string h3(string innerHtml, string htmlName, string htmlClass) => H3(innerHtml, htmlClass);
        //
        [Obsolete("Use uppercase methods", true)]
        public override string h3(string innerHtml, string htmlName) => H3(innerHtml);
        //
        [Obsolete("Use uppercase methods", true)]
        public override string h3(string innerHtml) => H3(innerHtml);
        //
        [Obsolete("Use uppercase methods", true)]
        public override string h4(string innerHtml, string htmlName, string htmlClass, string htmlId) => H4(innerHtml, htmlClass, htmlId);
        //
        [Obsolete("Use uppercase methods", true)]
        public override string h4(string innerHtml, string htmlName, string htmlClass) => H4(innerHtml, htmlClass);
        //
        [Obsolete("Use uppercase methods", true)]
        public override string h4(string innerHtml, string htmlName) => H4(innerHtml);
        //
        [Obsolete("Use uppercase methods", true)]
        public override string h4(string innerHtml) => H4(innerHtml);
        //
        [Obsolete("Use uppercase methods", true)]
        public override string h5(string innerHtml, string htmlName, string htmlClass, string htmlId) => H5(innerHtml, htmlClass, htmlId);
        //
        [Obsolete("Use uppercase methods", true)]
        public override string h5(string innerHtml, string htmlName, string htmlClass) => H5(innerHtml, htmlClass);
        //
        [Obsolete("Use uppercase methods", true)]
        public override string h5(string innerHtml, string htmlName) => H5(innerHtml);
        //
        [Obsolete("Use uppercase methods", true)]
        public override string h5(string innerHtml) => H5(innerHtml);
        //
        [Obsolete("Use uppercase methods", true)]
        public override string h6(string innerHtml, string htmlName, string htmlClass, string htmlId) => H6(innerHtml, htmlClass, htmlId);
        //
        [Obsolete("Use uppercase methods", true)]
        public override string h6(string innerHtml, string htmlName, string htmlClass) => H6(innerHtml, htmlClass);
        //
        [Obsolete("Use uppercase methods", true)]
        public override string h6(string innerHtml, string htmlName) => H6(innerHtml);
        //
        [Obsolete("Use uppercase methods", true)]
        public override string h6(string innerHtml) => H6(innerHtml);
        //
        [Obsolete("Use uppercase methods", true)]
        public override string p(string innerHtml, string htmlName, string htmlClass, string htmlId) => P(innerHtml, htmlClass, htmlId );
        //
        [Obsolete("Use uppercase methods", true)]
        public override string p(string innerHtml, string htmlName, string htmlClass) => P(innerHtml, htmlClass);
        //
        [Obsolete("Use uppercase methods", true)]
        public override string p(string innerHtml, string htmlName) => P(innerHtml);
        //
        [Obsolete("Use uppercase methods", true)]
        public override string p(string innerHtml) => P(innerHtml);
        //
        [Obsolete("Use uppercase methods", true)]
        public override string li(string innerHtml, string htmlName, string htmlClass, string htmlId) => Li( innerHtml, htmlClass, htmlId );
        //
        [Obsolete("Use uppercase methods", true)]
        public override string li(string innerHtml, string htmlName, string htmlClass) => Li(innerHtml, htmlClass);
        //
        [Obsolete("Use uppercase methods", true)]
        public override string li(string innerHtml, string htmlName) => Li(innerHtml);
        //
        [Obsolete("Use uppercase methods", true)]
        public override string li(string innerHtml) => Li(innerHtml);
        //
        [Obsolete("Use uppercase methods", true)]
        public override string ul(string innerHtml, string htmlName, string htmlClass, string htmlId) => Ul(innerHtml, htmlClass, htmlId);
        //
        [Obsolete("Use uppercase methods", true)]
        public override string ul(string innerHtml, string htmlName, string htmlClass) => Ul(innerHtml, htmlClass);
        //
        [Obsolete("Use uppercase methods", true)]
        public override string ul(string innerHtml, string htmlName) => Ul(innerHtml);
        //
        [Obsolete("Use uppercase methods", true)]
        public override string ul(string innerHtml) => Ul(innerHtml);
        //
        [Obsolete("Use uppercase methods", true)]
        public override string ol(string innerHtml, string htmlName, string htmlClass, string htmlId) => HtmlController.genericBlockTag("ol", innerHtml, htmlClass, htmlId);
        //
        [Obsolete("Use uppercase methods", true)]
        public override string ol(string innerHtml, string htmlName, string htmlClass) => HtmlController.genericBlockTag("ol", innerHtml, htmlClass, "");
        //
        [Obsolete("Use uppercase methods", true)]
        public override string ol(string innerHtml, string htmlName) => HtmlController.genericBlockTag("ol", innerHtml, "", "");
        //
        [Obsolete("Use uppercase methods", true)]
        public override string ol(string innerHtml) => HtmlController.genericBlockTag("ol", innerHtml);
        //
        [Obsolete("Use InputText( string, string, string, string, int, bool", true)]
        public override string InputText(string htmlName, string htmlValue, string Height, string Width, bool IsPassword, string htmlClass, string htmlId) => HtmlController.inputText(cp.core, htmlName, htmlValue, GenericController.encodeInteger(Height), GenericController.encodeInteger(Width), htmlId, IsPassword, false, htmlClass);
        //
        [Obsolete("Use InputText( string, string, string, string, int, bool", true)]
        public override string InputText(string htmlName, string htmlValue, string Height, string Width, bool IsPassword, string htmlClass) => HtmlController.inputText(cp.core, htmlName, htmlValue, GenericController.encodeInteger(Height), GenericController.encodeInteger(Width), "", IsPassword, false, htmlClass);
        //
        [Obsolete("Use InputText( string, string, string, string, int, bool", true)]
        public override string InputText(string htmlName, string htmlValue, string Height, string Width, bool IsPassword) => HtmlController.inputText(cp.core, htmlName, htmlValue, GenericController.encodeInteger(Height), GenericController.encodeInteger(Width), "", IsPassword);
        //
        [Obsolete("Use InputText( string, string, string, string, int, bool", true)]
        public override string InputText(string htmlName, string htmlValue, string Height, string Width) => HtmlController.inputText(cp.core, htmlName, htmlValue, GenericController.encodeInteger(Height), GenericController.encodeInteger(Width));
        //
        [Obsolete("Use InputText( string, string, string, string, int, bool", true)]
        public override string InputWysiwyg(string htmlName, string HtmlValue, EditorUserRole UserScope, EditorContentType ContentScope, string Height, string Width, string htmlClass, string htmlId)
           => cp.core.html.getFormInputHTML(htmlName, HtmlValue, Height, Width);
        //
        //
        //
        #region  IDisposable Support 
        protected bool disposed = false;
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                if (disposing) {
                    //
                    // call .dispose for managed objects
                    //
                }
                //
                // Add code here to release the unmanaged resource.
                //
            }
            this.disposed = true;
        }
        //
        // ====================================================================================================
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        //
        // ====================================================================================================
        //
        ~CPHtmlClass() {
            Dispose(false);
            
            
        }
        #endregion
    }
}