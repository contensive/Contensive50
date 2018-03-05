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
			ContextPage = 1,
			ContextAdmin = 2,
			ContextTemplate = 3,
			ContextEmail = 4,
			ContextRemoteMethodHtml = 5,
			ContextOnNewVisit = 6,
			ContextOnPageEnd = 7,
			ContextOnPageStart = 8,
			ContextEditor = 9,
			ContextHelpUser = 10,
			ContextHelpAdmin = 11,
			ContextHelpDeveloper = 12,
			ContextOnContentChange = 13,
			ContextFilter = 14,
			ContextSimple = 15,
			ContextOnBodyStart = 16,
			ContextOnBodyEnd = 17,
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
			public string errorCaption {get; set;}
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

