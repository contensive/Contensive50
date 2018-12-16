
using System;
using System.Diagnostics;
using System.Collections.Generic;
using Contensive.Processor.Controllers;
using Contensive.BaseClasses;
using Contensive.Processor.Exceptions;

namespace Contensive.Processor {
    //
    // ====================================================================================================
    //
    public class CPUtilsClass : BaseClasses.CPUtilsBaseClass, IDisposable {
        //
        #region COM GUIDs
        public const string ClassId = "BAF47FF8-7D7B-4375-A5BB-06E576AB757B";
        public const string InterfaceId = "78662206-16DF-4C5D-B25E-30292E99EC88";
        public const string EventsId = "88D127A1-BD5C-43C6-8814-BE17CADBF7AC";
        #endregion
        //
        // ====================================================================================================
        //
        private CPClass CP;
        //
        protected bool disposed = false;
        //
        public CPUtilsClass(CPClass CPParentObj) : base() {
            CP = CPParentObj;
        }
        //
        // ====================================================================================================
        // dispose
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                appendDebugLog(".dispose, dereference cp, main, csv");
                if (disposing) {
                    //
                    // call .dispose for managed objects
                    //
                    CP = null;
                }
                //
                // Add code here to release the unmanaged resource.
                //
            }
            this.disposed = true;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Upgrade the current application. If isNewApp is true core tables and content can be created.
        /// </summary>
        /// <param name="isNewApp"></param>
        /// <remarks></remarks>
        public override void Upgrade(bool isNewApp) {
            try {
                throw new GenericException("Installation upgrade through the cp interface is deprecated. Please use the command line tool.");
                // Controllers.appBuilderController.upgrade(CP.core, isNewApp)
            } catch (Exception ex) {
                LogController.handleError(CP.core,ex);
            }
        }
        //
        // ====================================================================================================
        //
        public override string ConvertHTML2Text(string Source) {
            return NUglify.Uglify.HtmlToText(Source).Code;
        }
        //
        // ====================================================================================================
        //
        public override string ConvertText2HTML(string Source) {
            return CP.core.html.convertTextToHtml(Source);
        }
        //
        // ====================================================================================================
        //
        public override string CreateGuid() {
            return GenericController.getGUID();
        }
        //
        // ====================================================================================================
        //
        public override string DecodeUrl(string Url) {
            return GenericController.decodeURL(Url);
        }
        //
        // ====================================================================================================
        //
        public override string EncodeContentForWeb(string Source, string ContextContentName = "", int ContextRecordID = 0, int WrapperID = 0) {
            return ActiveContentController.renderHtmlForWeb(CP.core, Source, ContextContentName, ContextRecordID, 0, "", WrapperID, CPUtilsBaseClass.addonContext.ContextPage);
        }
        //
        // ====================================================================================================
        //
        public override string DecodeHTML(string Source) {
            return HtmlController.decodeHtml(Source);
        }
        //
        // ====================================================================================================
        //
        public override string EncodeHTML(string Source) {
            string returnValue = "";
            //
            if (!string.IsNullOrEmpty(Source)) {
                returnValue = HtmlController.encodeHtml(Source);
            }
            return returnValue;
        }
        //
        // ====================================================================================================
        //
        public override string EncodeUrl(string Source) {
            return GenericController.encodeURL(Source);
        }
        //
        // ====================================================================================================
        //
        public override string GetPleaseWaitEnd() {
            return CP.core.programFiles.readFileText("resources\\WaitPageClose.htm");
        }
        //
        // ====================================================================================================
        //
        public override string GetPleaseWaitStart() {
            return CP.core.programFiles.readFileText("Resources\\WaitPageOpen.htm");
        }
        //
        // ====================================================================================================
        //
        public override void IISReset() {
            if (true) {
                CP.core.webServer.reset();
            }
        }
        //
        // ====================================================================================================
        //
        public override int EncodeInteger(object Expression) {
            return GenericController.encodeInteger(Expression);
        }
        //
        // ====================================================================================================
        //
        public override double EncodeNumber(object Expression) {
            return GenericController.encodeNumber(Expression);
        }
        //
        // ====================================================================================================
        //
        public override string EncodeText(object Expression) {
            return GenericController.encodeText(Expression);
        }
        //
        // ====================================================================================================
        //
        public override bool EncodeBoolean(object Expression) {
            return GenericController.encodeBoolean(Expression);
        }
        //
        // ====================================================================================================
        //
        public override DateTime EncodeDate(object Expression) {
            return GenericController.encodeDate(Expression);
        }
        //
        // ====================================================================================================
        //
        private string ExecuteAddon(string IdGuidOrName, addonExecuteContext executeContext) {
            if (IdGuidOrName.IsNumeric()) {
                executeContext.errorContextMessage += " addon id:" + IdGuidOrName;
                return CP.core.addon.execute(Models.Db.AddonModel.create(CP.core, GenericController.encodeInteger(IdGuidOrName)), executeContext);
            } else if (GenericController.isGuid(IdGuidOrName)) {
                executeContext.errorContextMessage += " addon guid:" + IdGuidOrName;
                return CP.core.addon.execute(Models.Db.AddonModel.create(CP.core, IdGuidOrName), executeContext);
            } else {
                executeContext.errorContextMessage += "addon " + IdGuidOrName;
                return CP.core.addon.execute(Models.Db.AddonModel.createByUniqueName(CP.core, IdGuidOrName), executeContext);
            }
        }
        //
        // ====================================================================================================
        //
        [Obsolete("Deprecated, use cp.addon.Execute",true)]
        public override string ExecuteAddon(string IdGuidOrName, int WrapperId) => ExecuteAddon(
            IdGuidOrName,
            new addonExecuteContext() {
                addonType = addonContext.ContextPage,
                wrapperID = WrapperId,
                instanceGuid = CP.core.docProperties.getText("instanceId")
            }
        );
        //
        // ====================================================================================================
        //
        [Obsolete("Deprecated, use cp.addon.Execute", true)]
        public override string ExecuteAddon(string IdGuidOrName) => ExecuteAddon(
            IdGuidOrName,
            new addonExecuteContext() {
                addonType = addonContext.ContextPage,
                instanceGuid = CP.core.docProperties.getText("instanceId")
            }
        );
        //
        // ====================================================================================================
        //
        [Obsolete("Deprecated, use cp.addon.Execute", true)]
        public override string ExecuteAddon(string IdGuidOrName, addonContext context) => ExecuteAddon(
            IdGuidOrName,
            new addonExecuteContext() {
                addonType = context,
                instanceGuid = CP.core.docProperties.getText("instanceId")
            }
        );
        //
        // ====================================================================================================
        //
        [Obsolete("Deprecated, use cp.addon.Execute", true)]
        public override string ExecuteAddonAsProcess(string AddonIDGuidOrName) {
            try {
                Models.Db.AddonModel addon = null;
                if (EncodeInteger(AddonIDGuidOrName) > 0) {
                    addon = CP.core.addonCache.getAddonById(EncodeInteger(AddonIDGuidOrName));
                } else if (GenericController.isGuid(AddonIDGuidOrName)) {
                    addon = CP.core.addonCache.getAddonByGuid(AddonIDGuidOrName);
                } else {
                    addon = CP.core.addonCache.getAddonByName(AddonIDGuidOrName);
                }
                if (addon != null) {
                    CP.core.addon.executeAsync(addon);
                }
            } catch (Exception ex) {
                LogController.handleError(CP.core, ex);
            }
            return string.Empty;
        }
        //
        // ====================================================================================================
        //
        [Obsolete("Deprecated, use AppendLog")]
        public override void AppendLogFile(string Text) {
            LogController.logInfo(CP.core, Text);
        }
        //
        // ====================================================================================================
        /// <summary>
        /// simpulate legacy append method. First segment of pathFilename is used as the log path, filename w/o extension is used as file prefix
        /// </summary>
        /// <param name="pathFilename"></param>
        /// <param name="Text"></param>
        [Obsolete("Deprecated, file logging is no longer supported. Use AppendLog(message) to log Info level messages")] public override void AppendLog(string pathFilename, string Text) {
            if ((!string.IsNullOrWhiteSpace(pathFilename)) && (!string.IsNullOrWhiteSpace(Text))) {
                pathFilename = GenericController.convertToDosSlash(pathFilename);
                string[] parts = pathFilename.Split('\\');
                LogController.logInfo(CP.core, "legacy logFile: [" + pathFilename + "], " + Text);
            }
        }
        //
        // ====================================================================================================
        //
        public override void AppendLog(string Text) {
            LogController.logInfo(CP.core, Text);
        }
        //
        // ====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override string ConvertLinkToShortLink(string URL, string ServerHost, string ServerVirtualPath) {
            return GenericController.ConvertLinkToShortLink(URL, ServerHost, ServerVirtualPath);
        }
        //
        // ====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override string ConvertShortLinkToLink(string url, string pathPagePrefix) {
            return GenericController.removeUrlPrefix(url, pathPagePrefix);
        }
        //
        // ====================================================================================================
        //
        [Obsolete("Deprecated. Use native methods to convert date formats.",false)]
        public override DateTime DecodeGMTDate(string GMTDate) {
            return GenericController.deprecatedDecodeGMTDate(GMTDate);
        }
        //
        // ====================================================================================================
        //
        public override string DecodeResponseVariable(string Source) {
            return GenericController.decodeResponseVariable(Source);
        }
        //
        // ====================================================================================================
        //
        public override string EncodeJavascript(string Source) {
            return GenericController.EncodeJavascriptStringSingleQuote(Source);
        }
        //
        // ====================================================================================================
        //
        public override string EncodeQueryString(string Source) {
            return GenericController.encodeQueryString(Source);
        }
        //
        // ====================================================================================================
        //
        public override string EncodeRequestVariable(string Source) {
            return GenericController.encodeRequestVariable(Source);
        }
        //
        // ====================================================================================================
        //
        public override string GetArgument(string Name, string ArgumentString, string DefaultValue = "", string Delimiter = "") {
            return GenericController.getValueFromNameValueString(Name, ArgumentString, DefaultValue, Delimiter);
        }
        //
        // ====================================================================================================
        //
        public override string GetFilename(string PathFilename) {
            string filename = "";
            string path = "";
            CP.core.privateFiles.splitDosPathFilename(PathFilename, ref path, ref filename);
            return filename;
        }
        //
        // ====================================================================================================
        //
        public override DateTime GetFirstNonZeroDate(DateTime Date0, DateTime Date1) {
            return GenericController.getFirstNonZeroDate(Date0, Date1);
        }
        //
        // ====================================================================================================
        //
        public override int GetFirstNonZeroInteger(int Integer0, int Integer1) {
            return GenericController.getFirstNonZeroInteger(Integer0, Integer1);
        }
        //
        // ====================================================================================================
        //
        public override string GetIntegerString(int Value, int DigitCount) {
            return GenericController.getIntegerString(Value, DigitCount);
        }
        //
        // ====================================================================================================
        //
        public override string GetLine(string Body) {
            return GenericController.getLine(ref Body);
        }
        //
        // ====================================================================================================
        //
        public override int GetListIndex(string Item, string ListOfItems) {
            return GenericController.GetListIndex(Item, ListOfItems);
        }
        //
        // ====================================================================================================
        //
        public override int GetProcessID() {
            return Process.GetCurrentProcess().Id;
        }
        //
        // ====================================================================================================
        //
        public override int GetRandomInteger() {
            return GenericController.GetRandomInteger(CP.core);
        }
        //
        // ====================================================================================================
        //
        public override bool IsInDelimitedString(string DelimitedString, string TestString, string Delimiter) {
            return GenericController.isInDelimitedString(DelimitedString, TestString, Delimiter);
        }
        //
        // ====================================================================================================
        //
        public override string ModifyLinkQueryString(string Link, string QueryName, string QueryValue, bool AddIfMissing = true) {
            return GenericController.modifyLinkQuery(Link, QueryName, QueryValue, AddIfMissing);
        }
        //
        // ====================================================================================================
        //
        public override string ModifyQueryString(string WorkingQuery, string QueryName, string QueryValue, bool AddIfMissing = true) {
            return GenericController.modifyQueryString(WorkingQuery, QueryName, QueryValue, AddIfMissing);
        }
        //
        // ====================================================================================================
        /// <summary>
        /// seperate a url into its parts
        /// </summary>
        /// <param name="SourceURL"></param>
        /// <param name="Protocol"></param>
        /// <param name="Host"></param>
        /// <param name="Port"></param>
        /// <param name="Path"></param>
        /// <param name="Page"></param>
        /// <param name="QueryString"></param>
        public override void ParseURL(string SourceURL, ref string Protocol, ref string Host, ref string Port, ref string Path, ref string Page, ref string QueryString) {
            GenericController.splitUrl(SourceURL, ref Protocol, ref Host, ref Port, ref Path, ref Page, ref QueryString);
        }
        //
        // ====================================================================================================
        /// <summary>
        /// seperate a url into its parts
        /// </summary>
        /// <param name="SourceURL"></param>
        /// <param name="Protocol"></param>
        /// <param name="Host"></param>
        /// <param name="Path"></param>
        /// <param name="Page"></param>
        /// <param name="QueryString"></param>
        public override void SeparateURL(string SourceURL, ref string Protocol, ref string Host, ref string Path, ref string Page, ref string QueryString) {
            GenericController.splitUrl(SourceURL, ref Protocol, ref Host, ref Path, ref Page, ref QueryString);
        }
        //
        // ====================================================================================================
        //
        public override object SplitDelimited(string WordList, string Delimiter) {
            return GenericController.SplitDelimited(WordList, Delimiter);
        }
        //
        // ====================================================================================================
        //
        public override void Sleep(int timeMSec) {
            System.Threading.Thread.Sleep(timeMSec);
        }
        //
        // ====================================================================================================
        //
        public override string hashMd5(string source) {
            throw new NotImplementedException("hashMd5 not implemented yet");
            //Return HashPasswordForStoringInConfigFile(source, "md5")
        }
        //
        // ====================================================================================================
        //
        public override bool isGuid(string guid) {
            return GenericController.common_isGuid(guid);
        }
        // todo implement taskId return value, create cp.task object to track task status
        //====================================================================================================
        /// <summary>
        /// Install an addon collection file asynchonously. The task is queued and the taskId is returned. Use cp.tasks.getTaskStatus to determine status
        /// </summary>
        /// <param name="privateFile"></param>
        /// <returns></returns>
        public override int installCollectionFromFile(string privateFile) {
            int taskId = 0;
            string ignoreUserMessage = "";
            string ignoreGuid = "";
            var ignoreList = new List<string> { };
            var installedCollections = new List<string>();
            CollectionController.installCollectionsFromPrivateFile(CP.core, privateFile, ref ignoreUserMessage, ref ignoreGuid, false, true, ref ignoreList, "CPUtilsClass.installCollectionFromFile [" + privateFile + "]", ref installedCollections);
            return taskId;
        }
        // todo implement taskId return value, create cp.task object to track task status
        //====================================================================================================
        /// <summary>
        /// Install all addon collections in a folder asynchonously. Optionally delete the folder. The task is queued and the taskId is returned. Use cp.tasks.getTaskStatus to determine status
        /// </summary>
        /// <param name="privateFolder"></param>
        /// <param name="deleteFolderWhenDone"></param>
        /// <returns></returns>
        public override int installCollectionsFromFolder(string privateFolder, bool deleteFolderWhenDone) {
            int taskId = 0;
            string ignoreUserMessage = "";
            List<string> ignoreList1 = new List<string>();
            List<string> ignoreList2 = new List<string>();
            string logPrefix = "CPUtilsClass.installCollectionsFromFolder";
            var installedCollections = new List<string>();
            CollectionController.installCollectionsFromPrivateFolder(CP.core, privateFolder, ref ignoreUserMessage, ref ignoreList1, false, false, ref ignoreList2, logPrefix, ref installedCollections, true);
            return taskId;
        }
        // todo implement taskId return value, create cp.task object to track task status
        //====================================================================================================
        /// <summary>
        /// Install all addon collections in a folder asynchonously. The task is queued and the taskId is returned. Use cp.tasks.getTaskStatus to determine status
        /// </summary>
        /// <param name="privateFolder"></param>
        /// <returns></returns>
        public override int installCollectionsFromFolder(string privateFolder) {
            return installCollectionsFromFolder(privateFolder, false);
        }
        // todo implement taskId return value, create cp.task object to track task status
        //====================================================================================================
        /// <summary>
        /// Install an addon collections from the collection library asynchonously. The task is queued and the taskId is returned. Use cp.tasks.getTaskStatus to determine status
        /// </summary>
        public override int installCollectionFromLibrary(string collectionGuid ) {
            int taskId = 0;
            string ignoreUserMessage = "";
            var installedCollections = new List<string>();
            string logPrefix = "installCollectionFromLibrary";
            var nonCriticalErrorList = new List<string>();
            CollectionController.installCollectionFromRemoteRepo(CP.core, collectionGuid, ref ignoreUserMessage, "", false, false, ref nonCriticalErrorList, logPrefix, ref installedCollections);
            return taskId;
        }
        // todo implement taskId return value, create cp.task object to track task status
        //====================================================================================================
        /// <summary>
        /// Install an addon collections from an endpoint asynchonously. The task is queued and the taskId is returned. Use cp.tasks.getTaskStatus to determine status
        /// </summary>
        /// <param name="privateFolder"></param>
        /// <param name="deleteFolderWhenDone"></param>
        /// <returns></returns>
        public override int installCollectionFromLink(string link) {
            throw new NotImplementedException();
        }
        //
        //====================================================================================================
        /// <summary>
        /// Converts html content to the wysiwyg editor compatible format that includes edit icons for addons. Use this to convert the html content added to wysiwyg editors. Use EncodeHtmlFromWysiwygEditor() before saving back to Db.
        /// </summary>
        /// <param name="Source"></param>
        /// <returns></returns>
        public override string EncodeHtmlForWysiwygEditor(string Source) {
            return ActiveContentController.renderHtmlForWysiwygEditor(CP.core, Source);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Converts html content from wysiwyg editors to be saved. See EncodeHtmlForWysiwygEditor() for more details.
        /// </summary>
        /// <param name="Source"></param>
        /// <returns></returns>
        public override string DecodeHtmlFromWysiwygEditor(string Source) {
            return ActiveContentController.processWysiwygResponseForSave(CP.core, Source);
        }
        //
        // ====================================================================================================
        //
        private void appendDebugLog(string copy) {
            //My.Computer.FileSystem.WriteAllText("c:\clibCpDebug.log", Now & " - cp.utils, " & copy & vbCrLf, True)
            // 'My.Computer.FileSystem.WriteAllText(System.AppDocmc.main_CurrentDocmc.main_BaseDirectory() & "cpLog.txt", Now & " - " & copy & vbCrLf, True)
        }
        //
        // ====================================================================================================
        //
        private void tp(string msg) {
            //Call appendDebugLog(msg)
        }
        #region  IDisposable Support 
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~CPUtilsClass() {
            Dispose(false);
            
            
        }
        #endregion
    }

}