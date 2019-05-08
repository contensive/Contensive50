//========================================================================



//========================================================================

//
// documentation should be in a new project that inherits these classes. The class names should be the object names in the actual cp project
//


using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Contensive.BaseClasses
{
	public abstract class CPUtilsBaseClass
	{
		public enum addonContext
		{
            /// <summary>
            /// Addon placed on a page.
            /// </summary>
			ContextPage = 1,
            /// <summary>
            /// Addon run by the admin site addon, to be displayed in the dashboard space
            /// </summary>
			ContextAdmin = 2,
            /// <summary>
            /// Addon placed on a template
            /// </summary>
			ContextTemplate = 3,
            /// <summary>
            /// Addon executed when an email is being rendered for an individual
            /// </summary>
			ContextEmail = 4,
            /// <summary>
            /// Addon executed as a remote method and is expected to return html (as opposed to JSON)
            /// </summary>
			ContextRemoteMethodHtml = 5,
            /// <summary>
            /// Addon executed because when a new visit is created. The return is ignored.
            /// </summary>
			ContextOnNewVisit = 6,
            /// <summary>
            /// Addon executed right before the body end html tag. The return is placed in the html
            /// </summary>
			ContextOnPageEnd = 7,
            /// <summary>
            /// Addon executed right after the open body tag. The return is placed in the html.
            /// </summary>
			ContextOnPageStart = 8,
            /// <summary>
            /// Addon executed because it is set as the editor for a content field type. It reads details from the doc and creates an html edit tag(s).
            /// </summary>
			ContextEditor = 9,
            /// <summary>
            /// deprecated
            /// </summary>
			ContextHelpUser = 10,
            /// <summary>
            /// deprecated
            /// </summary>
			ContextHelpAdmin = 11,
            /// <summary>
            /// deprecated
            /// </summary>
			ContextHelpDeveloper = 12,
            /// <summary>
            /// Addon executed by admin site when a content record is changed. Reads details from doc properties and act on the change
            /// </summary>
			ContextOnContentChange = 13,
            /// <summary>
            /// Executes when the html page is complete. Can modify the html document in core.docBody
            /// </summary>
			ContextFilter = 14,
            /// <summary>
            /// Return the addon's return, add artifacts like css to document.
            /// </summary>
			ContextSimple = 15,
            /// <summary>
            /// Executes right after the body start. Return is placed in the html document
            /// </summary>
			ContextOnBodyStart = 16,
            /// <summary>
            /// Executes right before the end body. Return is placed in the html body
            /// </summary>
			ContextOnBodyEnd = 17,
            /// <summary>
            /// Executes as a remote method. If return is a string, it is returned. If the return is any other type, it is serialized to JSON.
            /// </summary>
			ContextRemoteMethodJson = 18
		}
		//
		public class addonExecuteHostRecordContext
		{
			public string contentName;
			public int recordId;
			public string fieldName;
		}
		//
		public class addonExecuteContext
		{
			/// <summary>
			/// This caption is used if the addon cannot be executed.
			/// </summary>
			/// <returns></returns>
			public string errorContextMessage {get; set;}
			/// <summary>
			/// select enumeration option the describes the environment in which the addon is being executed (in an email, on a page, as a remote method, etc)
			/// </summary>
			/// <returns></returns>
			public addonContext addonType {get; set;} = addonContext.ContextSimple;
			/// <summary>
			/// Optional. If the addon is run from a page, it includes an instanceGuid which can be used by addon programming to locate date for this instance.
			/// </summary>
			/// <returns></returns>
			public string instanceGuid {get; set;} = "";
			/// <summary>
			/// Optional. Name value pairs added to the document environment during execution so they be read by addon programming during and after execution with cp.doc.getText(), etc.
			/// </summary>
			/// <returns></returns>
			public Dictionary<string, string> instanceArguments {get; set;} = new Dictionary<string, string>();
			/// <summary>
			/// Optional. If this addon is run automatically because it was included in content, this is the contentName, recordId and fieldName of the record that held that content.
			/// </summary>
			/// <returns></returns>
			public addonExecuteHostRecordContext hostRecord {get; set;} = new addonExecuteHostRecordContext();
			/// <summary>
			/// Optional. If included, this is the id value of a record in the Wrappers content and that wrapper will be added to the addon return result.
			/// </summary>
			/// <returns></returns>
			public int wrapperID {get; set;} = 0;
			/// <summary>
			/// Optional. If included, the addon will be wrapped with a div and this will be the html Id value of the div. May be used to customize the resulting html styles.
			/// </summary>
			/// <returns></returns>
			public string cssContainerId {get; set;} = "";
			/// <summary>
			/// Optional. If included, the addon will be wrapped with a div and this will be the html class value of the div. May be used to customize the resulting html styles.
			/// </summary>
			/// <returns></returns>
			public string cssContainerClass {get; set;} = "";
			/// <summary>
			/// Optional. If included with personizationPeopleId, the addon will be run in a authentication context for this people record. If not included, the current documents authentication context is used. This may be used for cases like addons that send email where email content may include personalization.
			/// </summary>
			/// <returns></returns>
			public int personalizationPeopleId {get; set;} = 0;
			/// <summary>
			/// Optional. If included with personizationPeopleId, the addon will be run in a authentication context for this people record. If not included, the current documents authentication context is used. This may be used for cases like addons that send email where email content may include personalization.
			/// </summary>
			/// <returns></returns>
			public bool personalizationAuthenticated {get; set;} = false;
			/// <summary>
			/// Optional. If true, this addon is called because it was a dependancy, and can only be called once within a document.
			/// </summary>
			/// <returns></returns>
			public bool isIncludeAddon {get; set;} = false;
			/// <summary>
			/// Optional. If set true, the addon being called will be delivered as ah html document, with head, body and html tags. This forces the addon's htmlDocument setting.
			/// </summary>
			/// <returns></returns>
			public bool forceHtmlDocument {get; set;} = false;
            /// <summary>
            /// When true, the environment is run from the task subsystem, without a UI. Assemblies from base collection run from program files. Addon return is ignored.
            /// </summary>
            public bool backgroundProcess { get; set; } = false;
            /// <summary>
            /// When true, an addon's javascript will be put in the head. This also forces javascript for all dependant addons to the head.
            /// </summary>
            public bool forceJavascriptToHead { get; set; } = false;
        }

        public abstract string ConvertHTML2Text(string Source);
		public abstract string ConvertText2HTML(string Source);
		public abstract string CreateGuid();
		public abstract string DecodeUrl(string Url);
		public abstract string EncodeContentForWeb(string Source, string ContextContentName = "", int ContextRecordID = 0, int WrapperID = 0);
		public abstract string EncodeHtmlForWysiwygEditor(string Source);
		public abstract string DecodeHtmlFromWysiwygEditor(string Source);
		public abstract string DecodeHTML(string Source);
		public abstract string EncodeHTML(string Source);
		public abstract string EncodeUrl(string Source);
		public abstract string GetPleaseWaitEnd();
		public abstract string GetPleaseWaitStart();
		public abstract void IISReset();
		public abstract int EncodeInteger(object Expression);
		public abstract double EncodeNumber(object Expression);
		public abstract string EncodeText(object Expression);
		public abstract bool EncodeBoolean(object Expression);
		public abstract DateTime EncodeDate(object Expression);
		public abstract string ExecuteAddon(string IdGuidOrName);
		public abstract string ExecuteAddon(string IdGuidOrName, int WrapperId);
		public abstract string ExecuteAddon(string IdGuidOrName, addonContext context);
		public abstract string ExecuteAddonAsProcess(string IdGuidOrName);
		[Obsolete("Deprecated, use AppendLog",false)] public abstract void AppendLogFile(string Text);
        [Obsolete("Deprecated, file logging is no longer supported. Use AppendLog(message) to log Info level messages", false)] public abstract void AppendLog(string pathFilename, string logText);
		public abstract void AppendLog(string logText);
        [Obsolete("Deprecated", false)] public abstract string ConvertLinkToShortLink(string URL, string ServerHost, string ServerVirtualPath);
        [Obsolete("Deprecated", false)] public abstract string ConvertShortLinkToLink(string URL, string PathPagePrefix);
        [Obsolete("Deprecated. Use native methods to convert date formats.", false)] public abstract DateTime DecodeGMTDate(string GMTDate);
		public abstract string DecodeResponseVariable(string Source);
		public abstract string EncodeJavascript(string Source);
		public abstract string EncodeQueryString(string Source);
		public abstract string EncodeRequestVariable(string Source);
		public abstract string GetArgument(string Name, string ArgumentString, string DefaultValue = "", string Delimiter = "");
		public abstract string GetFilename(string PathFilename);
		public abstract DateTime GetFirstNonZeroDate(DateTime Date0, DateTime Date1);
		public abstract int GetFirstNonZeroInteger(int Integer0, int Integer1);
		public abstract string GetIntegerString(int Value, int DigitCount);
		public abstract string GetLine(string Body);
		public abstract int GetListIndex(string Item, string ListOfItems);
		public abstract int GetProcessID();
		public abstract int GetRandomInteger();
		public abstract bool IsInDelimitedString(string DelimitedString, string TestString, string Delimiter);
		public abstract string ModifyLinkQueryString(string Link, string QueryName, string QueryValue, bool AddIfMissing = true);
		public abstract string ModifyQueryString(string WorkingQuery, string QueryName, string QueryValue, bool AddIfMissing = true);
		public abstract void ParseURL(string SourceURL, ref string Protocol, ref string Host, ref string Port, ref string Path, ref string Page, ref string QueryString);
		public abstract void SeparateURL(string SourceURL, ref string Protocol, ref string Host, ref string Path, ref string Page, ref string QueryString);
		public abstract object SplitDelimited(string WordList, string Delimiter);
		public abstract void Sleep(int timeMSec);
		public abstract string hashMd5(string source);
		public abstract bool isGuid(string guid);
		public abstract void Upgrade(bool isNewApp);
		//
		//====================================================================================================
		/// <summary>
		/// Install an addon collection file asynchonously. The task is queued and the taskId is returned. Use cp.tasks.getTaskStatus to determine status
		/// </summary>
		/// <param name="privateFile"></param>
		/// <returns></returns>
		public abstract int installCollectionFromFile(string privateFile);
		//
		//====================================================================================================
		/// <summary>
		/// Install all addon collections in a folder asynchonously. Optionally delete the folder. The task is queued and the taskId is returned. Use cp.tasks.getTaskStatus to determine status
		/// </summary>
		/// <param name="privateFolder"></param>
		/// <param name="deleteFolderWhenDone"></param>
		/// <returns></returns>
		public abstract int installCollectionsFromFolder(string privateFolder, bool deleteFolderWhenDone);
		//
		//====================================================================================================
		/// <summary>
		/// Install all addon collections in a folder asynchonously. The task is queued and the taskId is returned. Use cp.tasks.getTaskStatus to determine status
		/// </summary>
		/// <param name="privateFolder"></param>
		/// <returns></returns>
		public abstract int installCollectionsFromFolder(string privateFolder);
		//
		//====================================================================================================
		/// <summary>
		/// Install an addon collections from the collection library asynchonously. The task is queued and the taskId is returned. Use cp.tasks.getTaskStatus to determine status
		/// </summary>
		/// <param name="collectionGuid"></param>
		/// <returns></returns>
		public abstract int installCollectionFromLibrary(string collectionGuid);
		//
		//====================================================================================================
		/// <summary>
		/// Install an addon collections from an endpoint asynchonously. The task is queued and the taskId is returned. Use cp.tasks.getTaskStatus to determine status
		/// </summary>
		/// <param name="link"></param>
		/// <returns></returns>
		public abstract int installCollectionFromLink(string link);
	}
}

