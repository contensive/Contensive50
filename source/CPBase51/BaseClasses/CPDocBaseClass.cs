
using System;
using System.Collections.Generic;

namespace Contensive.BaseClasses {
    public abstract class CPDocBaseClass {
        //
        //====================================================================================================
        //
        //
        // -- htmlAssetTypes
        public enum HtmlAssetTypeEnum {
            /// <summary>
            /// script at end of body (code or link)
            /// </summary>
            script,
            /// <summary>
            /// css style at end of body (code or link)
            /// </summary>
            style,
            /// <summary>
            /// special case, text is assumed to be script to run on load
            /// </summary>
            scriptOnLoad
        }
        //
        // -- assets to be added to the head section (and end-of-body) of html documents
        public class HtmlAssetClass {
            /// <summary>
            /// the type of asset, css, js, etc
            /// </summary>
            public HtmlAssetTypeEnum assetType { get; set; }
            /// <summary>
            /// if true, asset goes in head else it goes at end of body
            /// </summary>
            public bool inHead { get; set; }
            /// <summary>
            /// if true, the content property is a link to the asset, else use the content as the asset
            /// </summary>
            public bool isLink { get; set; }
            /// <summary>
            /// either link or content depending on the isLink property
            /// </summary>
            public string content { get; set; }
            /// <summary>
            /// message used during debug to show where the asset came from
            /// </summary>
            public string addedByMessage { get; set; }
            /// <summary>
            /// if this asset was added from an addon, this is the addonId.
            /// </summary>
            public int sourceAddonId { get; set; }
            //
            public bool canBeMerged { get; set; }
        }
        //
        //====================================================================================================
        //
        public abstract List<HtmlAssetClass> HtmlAssetList { get; }
        //
        //====================================================================================================
        //
        public abstract bool NoFollow { get; set; }
        //
        //====================================================================================================
        //
        public abstract int PageId { get; }
        //
        //====================================================================================================
        //
        public abstract string PageName { get; }
        //
        //====================================================================================================
        //
        public abstract string RefreshQueryString { get; }
        //
        //====================================================================================================
        //
        public abstract DateTime StartTime { get; }
        //
        //====================================================================================================
        //
        public abstract int TemplateId { get; }
        //
        //====================================================================================================
        //
        public abstract string Type { get; }
        //
        //====================================================================================================
        //
        public abstract void AddHeadStyle(string styleSheet);
        //
        //====================================================================================================
        //
        public abstract void AddHeadStyleLink(string styleSheetLink);
        //
        //====================================================================================================
        //
        public abstract void AddHeadJavascript(string code);
        //
        //====================================================================================================
        //
        public abstract void AddHeadJavascriptLink(string codeLink);
        //
        //====================================================================================================
        //
        public abstract void AddBodyJavascript(string code);
        //
        //====================================================================================================
        //
        public abstract void AddBodyJavascriptLink(string codeLink);
        //
        //====================================================================================================
        //
        public abstract void AddHeadTag(string htmlTag);
        //
        //====================================================================================================
        //
        public abstract void AddMetaDescription(string metaDescription);
        //
        //====================================================================================================
        //
        public abstract void AddMetaKeywordList(string metaKeywordList);
        //
        //====================================================================================================
        //
        public abstract void AddOnLoadJavascript(string code);
        //
        //====================================================================================================
        //
        public abstract void AddTitle(string pageTitle);
        //
        //====================================================================================================
        //
        public abstract void AddBodyEnd(string html);
        //
        //====================================================================================================
        //
        public abstract string Body { get; set; }
        //
        //====================================================================================================
        //
        public abstract void SetProperty(string key, string value);
        public abstract void SetProperty(string key, bool value);
        public abstract void SetProperty(string key, int value);
        public abstract void SetProperty(string key, DateTime value);
        public abstract void SetProperty(string key, double value);
        //
        //====================================================================================================
        //
        public abstract string GetProperty(string key, string defaultValue);
        public abstract string GetProperty(string key);
        //
        //====================================================================================================
        //
        public abstract string GetText(string key, string defaultValue);
        public abstract string GetText(string key );
        //
        //====================================================================================================
        //
        public abstract bool GetBoolean(string key, bool defaultValue);
        public abstract bool GetBoolean(string key);
        //
        //====================================================================================================
        //
        public abstract DateTime GetDate(string key, DateTime defaultValue);
        public abstract DateTime GetDate(string key);
        //
        //====================================================================================================
        //
        public abstract int GetInteger(string key, int defaultValue);
        public abstract int GetInteger(string key);
        //
        //====================================================================================================
        //
        public abstract double GetNumber(string key, double defaultValue);
        public abstract double GetNumber(string key);
        //
        //====================================================================================================
        //
        public abstract bool IsProperty(string key);
        //
        //====================================================================================================
        //
        public abstract bool IsAdminSite { get; }
        //
        //====================================================================================================
        //
        public abstract void AddRefreshQueryString(string key, string value);
        //
        //====================================================================================================
        //
        public abstract void AddRefreshQueryString(string key, int value);
        //
        //====================================================================================================
        //
        public abstract void AddRefreshQueryString(string key, Double value);
        //
        //====================================================================================================
        //
        public abstract void AddRefreshQueryString(string key, bool value);
        //
        //====================================================================================================
        //
        public abstract void AddRefreshQueryString(string key, DateTime value);
        //
        //====================================================================================================
        // deprecated
        //
        [Obsolete("Use addon navigation.", false)]
        public abstract string NavigationStructure { get; }
        //
        [Obsolete("Section is no longer supported", false)]
        public abstract int SectionId { get; }
        //
        [Obsolete("Site styles are no longer supported. Include styles and javascript in addons.", false)]
        public abstract string SiteStylesheet { get; }
        //
        [Obsolete("Use GetText().", false)]
        public abstract string get_GlobalVar(string Index);
        //
        [Obsolete("Use SetProperty().", false)]
        public abstract void set_GlobalVar(string Index, string Value);
        //
        [Obsolete("Use IsProperty().", false)]
        public abstract bool get_IsGlobalVar(string Index);
        //
        [Obsolete("Use IsProperty().", false)]
        public abstract bool get_IsVar(string Index);
        //
        [Obsolete("Use GetText().", false)]
        public abstract string get_Var(string Index);
        //
        [Obsolete("Use SetProperty().", false)]
        public abstract void set_Var(string Index, string Value);
        //
        [Obsolete("Filter addons are deprecated", false)]
        public abstract string Content { get; set; }
        //
        [Obsolete("Use GetBoolean(string,bool)", false)]
        public abstract bool GetBoolean(string key, string defaultValue);
        //
        [Obsolete("Use GetDate(string,DateTime)", false)]
        public abstract DateTime GetDate(string key, string defaultValue);
        //
        [Obsolete("Use GetInteger(string,int)", false)]
        public abstract int GetInteger(string key, string defaultValue);
        //
        [Obsolete("Use GetNumber(string,double)", false)]
        public abstract double GetNumber(string key, string defaultValue);
    }
}

