
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Contensive.BaseClasses
{
	/// <summary>
	/// CP.Addon - The Addon class represents the instance of an add-on. To use this class, use its constructor and open an cpcore.addon. 
	/// Use these properties to retrieve it's configuration
	/// </summary>
	/// <remarks></remarks>
	public abstract class CPAddonBaseClass
	{
		//
		//====================================================================================================
		/// <summary>
		/// When true, this add-on is displayed on and can be used from the admin navigator.
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract bool Admin {get;}
		//
		//====================================================================================================
		/// <summary>
		/// A crlf delimited list of name=value pairs. These pairs create an options dialog available to administrators in advance edit mode. When the addon is executed, the values selected are available through the cp.doc.var("name") method.
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract string ArgumentList {get;}
		//
		//====================================================================================================
		/// <summary>
		/// When true, this addon returns the javascript code necessary to implement this object as ajax.
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract bool AsAjax {get;}
        //
        //====================================================================================================
        /// <summary>
        /// When true, the system only uses the custom styles field when building the page. This field is not updated with add-on updates.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        [Obsolete("This is no longer supported. Add a overriding style in another stylesheet instead of modifying", true)]
        public abstract string BlockDefaultStyles {get;}
		//
		//====================================================================================================
		/// <summary>
		/// The guid used to uniquely identify the add-on
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract string ccGuid {get;}
		//
		//====================================================================================================
		/// <summary>
		/// The ID local to this site of the collection which installed this cpcore.addon.
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract int CollectionID {get;}
		//
		//====================================================================================================
		/// <summary>
		/// When true, this addon can be placed in the content of pages.
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract bool Content {get;}
		//
		//====================================================================================================
		/// <summary>
		/// text copy is added to the addon content during execution.
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks>
		/// Addon content is assembled in the following order: TextContent + HTMLContent + IncludeContent + ScriptCallbackContent + FormContent + RemoteAssetContent + ScriptContent + ObjectContent + AssemblyContent.
		/// </remarks>
		public abstract string Copy {get;}
		//
		//====================================================================================================
		/// <summary>
		/// text copy is added to the addon content during execution.
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks>
		/// Addon content is assembled in the following order: TextContent + HTMLContent + IncludeContent + ScriptCallbackContent + FormContent + RemoteAssetContent + ScriptContent + ObjectContent + AssemblyContent.
		/// </remarks>
		public abstract string CopyText {get;}
        //
        //====================================================================================================
        /// <summary>
        /// Styles that are rendered on the page when the addon is executed. Custom styles are editable and are not modified when the add-on is updated.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        [Obsolete("This is no longer supported. Add a overriding style in another stylesheet instead of modifying", true)]
        public abstract string CustomStyles {get;}
        //
        //====================================================================================================
        /// <summary>
        /// Styles that are included with the add-on and are updated when the add-on is updated. See BlockdefaultStyles to block these.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        [Obsolete("This is no longer supported. Add a overriding style in another stylesheet instead of modifying", true)]
        public abstract string DefaultStyles {get;}
        //
        //====================================================================================================
        /// <summary>
        /// The add-on description is displayed in the addon manager
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        [Obsolete("This is no longer supported. Add a overriding style in another stylesheet instead of modifying", true)]
        public abstract string Description {get;}
		//
		//====================================================================================================
		/// <summary>
		/// When present, the system calls the execute method of an objected created from this dot net class namespace.
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks>
		/// Addon content is assembled in the following order: TextContent + HTMLContent + IncludeContent + ScriptCallbackContent + FormContent + RemoteAssetContent + ScriptContent + ObjectContent + AssemblyContent.
		/// </remarks>
		public abstract string DotNetClass {get;}
		//
		//====================================================================================================
		/// <summary>
		/// This is an xml stucture that the system executes to create an admin form. See the support.contensive.com site for more details.
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract string FormXML {get;}
		//
		//====================================================================================================
		/// <summary>
		/// This copy is displayed when the help icon for this addon is clicked.
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract string Help {get;}
		//
		//====================================================================================================
		/// <summary>
		/// If present, this link is displayed when the addon icon is clicked.
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract string HelpLink {get;}
		//
		//====================================================================================================
		/// <summary>
		/// When present, this icon will be used when the add-on is displayed in the addon manager and when edited. The height, width and sprites must also be set.
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract string IconFilename {get;}
		//
		//====================================================================================================
		/// <summary>
		/// The height in pixels of the icon referenced by the iconfilename.
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract int IconHeight {get;}
		//
		//====================================================================================================
		/// <summary>
		/// The number of images in the icon. There can be multiple images stacked top-to-bottom in the file. The first is the normal image. the second is the hover-over image. The third is the clicked image.
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract int IconSprites {get;}
		//
		//====================================================================================================
		/// <summary>
		/// The width of the icon referenced by the iconfilename
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract int IconWidth {get;}
		//
		//====================================================================================================
		/// <summary>
		/// The local ID of this addon on this site.
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract int ID {get;}
		//
		//====================================================================================================
		/// <summary>
		/// When true, this addon will be displayed in an html iframe.
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract bool InFrame {get;}
		//
		//====================================================================================================
		/// <summary>
		/// When true, the system will assume the addon returns html that is inline, as opposed to block. This is used to vary the edit icon behaviour.
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract bool IsInline {get;}
        //
        //====================================================================================================
        /// <summary>
        /// Javascript code that will be placed in the document right before the end-body tag. Do not include script tags.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        [Obsolete("This is no longer supported.", true)]
        public abstract string JavaScriptBodyEnd {get;}
        //
        //====================================================================================================
        /// <summary>
        /// Javascript code that will be placed in the head of the document. Do no include script tags.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        [Obsolete("This is no longer supported.", true)]
        public abstract string JavascriptInHead {get;}
		//
		//====================================================================================================
		/// <summary>
		/// Javascript that will be executed in the documents onload event.
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		[Obsolete("Create onready or onload events within your javascript. This method will be deprecated.", false)]
		public abstract string JavaScriptOnLoad {get;}
		//
		//====================================================================================================
		/// <summary>
		/// A URL to a webserver that returns javascript. This URL will be added as the src attribute of a script tag, and placed in the content where this Add-on is inserted. This URL can be to any server-side program on any server, provided it returns javascript.
		/// For instance, if you have a script page that returns javascript,put the URL of that page here. The addon can be dropped on any page and will execute the script. Your script can be from any site. This technique is used in widgets and avoids the security issues with ajaxing from another site.
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract string Link {get;}
		//
		//====================================================================================================
		/// <summary>
		/// Text here will be added to the meta description section of the document head.
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract string MetaDescription {get;}
		//
		//====================================================================================================
		/// <summary>
		/// This is a comma or crlf delimited list of phrases that will be added to the document's meta keyword list
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract string MetaKeywordList {get;}
		//
		//====================================================================================================
		/// <summary>
		/// The name of the cpcore.addon.
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract string Name {get;}
		//
		//====================================================================================================
		/// <summary>
		/// The type of navigator entry to be made. Choices are: Add-on,Report,Setting,Tool
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract string NavIconType {get;}
		//
		//====================================================================================================
		/// <summary>
		/// If present, this string will be used as an activex programid to create an object and call it's execute method.
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract string ObjectProgramID {get;}
		//
		//====================================================================================================
		/// <summary>
		/// If true, this addon will be execute at the end of every page and its content added to right before the end-body tag
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract bool OnBodyEnd {get;}
		//
		//====================================================================================================
		/// <summary>
		/// If true, this addon will be execute at the start of every page and it's content added to right after the body tag
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract bool OnBodyStart {get;}
		//
		//====================================================================================================
		/// <summary>
		/// if true, this add-on will be executed on every page and its content added right after the content box.
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract bool OnContentEnd {get;}
		//
		//====================================================================================================
		/// <summary>
		/// If true, this add-on will be executed on every page and its content added right before the content box
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract bool OnContentStart {get;}
        //
        //====================================================================================================
        [Obsolete("Deprecated", true)]
        public abstract bool Open(int AddonId);
        //
        //====================================================================================================
        [Obsolete("Deprecated", true)]
        public abstract bool Open(string AddonNameOrGuid);
		//
		//====================================================================================================
		/// <summary>
		/// All content in the field will be added directly, as-is to the document head.
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract string OtherHeadTags {get;}
		//
		//====================================================================================================
		/// <summary>
		/// All content in the field will be added to the documents title tag
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract string PageTitle {get;}
        //
        //====================================================================================================
        /// <summary>
        /// When present, this add-on will be executed stand-alone without a webpage periodically at this interval (in minutes).
        /// </summary>
        public abstract string ProcessInterval { get; }
        //
        //====================================================================================================
        /// <summary>
        /// The next time this add-on is scheduled to run as a processs
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract DateTime ProcessNextRun {get;}
		//
		//====================================================================================================
		/// <summary>
		/// Check true, this addon will be run once within the next minute as a stand-alone process.
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract bool ProcessRunOnce {get;}
		//
		//====================================================================================================
		/// <summary>
		/// 
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract string RemoteAssetLink {get;}
		//
		//====================================================================================================
		/// <summary>
		/// if true, this add-on can be executed as a remote method. The name of the addon is used as the url.
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract bool RemoteMethod {get;}
		//
		//====================================================================================================
		/// <summary>
		/// When present, this text will be added to the robots.txt content for the site. This content is editable through the preferences page
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract string RobotsTxt {get;}
		//
		//====================================================================================================
		/// <summary>
		/// When present, the first routine of this script will be executed when the add-on is executed and its return added to the add-ons return
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract string ScriptCode {get;}
		//
		//====================================================================================================
		/// <summary>
		/// if the ScriptCode has more than one routine and you want to run one other than the first, list is here.
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract string ScriptEntryPoint {get;}
		//
		//====================================================================================================
		/// <summary>
		/// The script language selected for this script.
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract string ScriptLanguage {get;}
        //
        //====================================================================================================
        /// <summary>
        /// A comma delimited list of the local id values of shared style record that will display with this add-on
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        [Obsolete("Deprecated", true)]
        public abstract string SharedStyles {get;}
		//
		//====================================================================================================
		/// <summary>
		/// 
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract bool Template {get;}
	}
}
