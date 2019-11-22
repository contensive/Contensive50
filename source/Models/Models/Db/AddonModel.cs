
using Contensive.BaseClasses;
using System;
using System.Collections.Generic;

namespace Contensive.Models.Db {
    [Serializable]
    public class AddonModel : DbBaseModel {
        //
        //====================================================================================================
        /// <summary>
        /// table definition
        /// </summary>
        public static DbBaseTableMetadataModel tableMetadata { get; } = new DbBaseTableMetadataModel("add-ons", "ccaggregatefunctions", "default", true);
        //
        //====================================================================================================
        /// <summary>
        /// field properties
        /// </summary>
        public bool admin { get; set; }
        public string argumentList { get; set; }
        public bool asAjax { get; set; }
        public bool blockEditTools { get; set; }
        public int collectionId { get; set; }
        public bool content { get; set; }
        public string copy { get; set; }
        public string copyText { get; set; }
        public bool diagnostic { get; set; }
        public string dotNetClass { get; set; }
        public bool email { get; set; }
        public bool filter { get; set; }
        public string formXML { get; set; }
        public string help { get; set; }
        public string helpLink { get; set; }
        public bool inFrame { get; set; }
        public bool isInline { get; set; }
        public bool javascriptForceHead { get; set; }
        public string jsHeadScriptSrc { get; set; }
        public FieldTypeJavascriptFile jsFilename { get; set; }
        public string link { get; set; }
        public string metaDescription { get; set; }
        public string metaKeywordList { get; set; }
        public int navTypeId { get; set; }
        public string objectProgramId { get; set; }
        public bool onBodyEnd { get; set; }
        public bool onBodyStart { get; set; }
        public bool onNewVisitEvent { get; set; }
        public bool onPageEndEvent { get; set; }
        public bool onPageStartEvent { get; set; }
        public bool htmlDocument { get; set; }
        public string otherHeadTags { get; set; }
        public string pageTitle { get; set; }
        public int? processInterval { get; set; }
        public DateTime? processNextRun { get; set; }
        public bool processRunOnce { get; set; }
        public string processServerKey { get; set; }
        public string remoteAssetLink { get; set; }
        public bool remoteMethod { get; set; }
        public string robotsTxt { get; set; }
        public string scriptingCode { get; set; }
        public string scriptingEntryPoint { get; set; }
        public int scriptingLanguageId { get; set; }
        public string scriptingTimeout { get; set; }
        public FieldTypeCSSFile stylesFilename { get; set; }
        public string stylesLinkHref { get; set; }
        public bool template { get; set; }
        /// <summary>
        /// The time in seconds for this addon if run the background
        /// </summary>
        public int? processTimeout { get; set; }
        /// <summary>
        /// html to be used for the icon. The icon is for the dashboard and addon manager, etc
        /// </summary>
        public string iconHtml { get; set; }
        /// <summary>
        /// When this addon is rendered in Page Builder, use this html if the addon's actual rendering is not acceptable
        /// </summary>
        public string editPlaceholderHtml { get; set; }
        /// <summary>
        /// if iconHtml is null or whitespace, this image url has the icon to use
        /// </summary>
        public string iconFilename { get; set; }
        /// <summary>
        /// the height of the icon filename
        /// </summary>
        public int? iconHeight { get; set; }
        /// <summary>
        /// the width of the icon filename
        /// </summary>
        public int? iconWidth { get; set; }
        /// <summary>
        /// the number of sprites in the icon
        /// </summary>
        public int? iconSprites { get; set; }
        //
        // -- deprecated, but for leave for now and log error
        public string javaScriptBodyEnd { get; set; }
        public string jsBodyScriptSrc { get; set; }
        //
        // -- deprecated
        // -Public Property JavaScriptOnLoad As String
        //
        //====================================================================================================
        //
        public static List<AddonModel> createList_pageDependencies(CPBaseClass cp, int pageId) {
            List<AddonModel> result = new List<AddonModel>();
            try {
                result = createList<AddonModel>(cp, "(id in (select addonId from ccAddonPageRules where (pageId=" + pageId + ")))");
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        //
        public static List<AddonModel> createList_templateDependencies(CPBaseClass cp, int templateId) {
            List<AddonModel> result = new List<AddonModel>();
            try {
                result = createList<AddonModel>(cp, "(id in (select addonId from ccAddonTemplateRules where (templateId=" + templateId + ")))");
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
            return result;
        }

    }
}
