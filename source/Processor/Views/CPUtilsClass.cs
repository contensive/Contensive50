
using System;
using System.Diagnostics;
using System.Collections.Generic;
using Contensive.Processor.Controllers;
using Contensive.BaseClasses;
using Contensive.Processor.Exceptions;
using Contensive.Processor.Models.Db;
using static Contensive.Processor.Constants;
using System.IO;

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
        /// <summary>
        /// dependencies
        /// </summary>
        private CPClass cp;
        //
        // ====================================================================================================
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="cp"></param>
        public CPUtilsClass(CPClass cp) {
            this.cp = cp;
        }
        //
        // ====================================================================================================
        /// <summary>
        /// Return a text approximation of an Html document
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public override string ConvertHTML2Text(string source) {
            return NUglify.Uglify.HtmlToText(source).Code;
        }
        //
        // ====================================================================================================
        /// <summary>
        /// return an html approximation of a text document
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public override string ConvertText2HTML(string source) {
            return cp.core.html.convertTextToHtml(source);
        }
        //
        // ====================================================================================================
        /// <summary>
        /// Create a new guid in the systems format (registry format "{...}")
        /// </summary>
        /// <returns></returns>
        public override string CreateGuid() {
            return GenericController.getGUID();
        }
        //
        // ====================================================================================================
        //
        public override string EncodeContentForWeb(string Source, string ContextContentName = "", int ContextRecordID = 0, int WrapperID = 0) {
            return ActiveContentController.renderHtmlForWeb(cp.core, Source, ContextContentName, ContextRecordID, 0, "", WrapperID, CPUtilsBaseClass.addonContext.ContextPage);
        }
        //
        // ====================================================================================================
        //
        public override void IISReset() {
            if (true) {
                cp.core.webServer.reset();
            }
        }
        //
        // ====================================================================================================
        //
        public override int EncodeInteger(object expression) {
            return GenericController.encodeInteger(expression);
        }
        //
        // ====================================================================================================
        //
        public override double EncodeNumber(object expression) {
            return GenericController.encodeNumber(expression);
        }
        //
        // ====================================================================================================
        //
        public override string EncodeText(object expression) {
            return GenericController.encodeText(expression);
        }
        //
        // ====================================================================================================
        //
        public override bool EncodeBoolean(object expression) {
            return GenericController.encodeBoolean(expression);
        }
        //
        // ====================================================================================================
        //
        public override DateTime EncodeDate(object expression) {
            return GenericController.encodeDate(expression);
        }
        //
        // ====================================================================================================
        //
        public override void AppendLog(string Text) {
            LogController.logInfo(cp.core, Text);
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
        public override string EncodeRequestVariable(string Source) {
            return GenericController.encodeRequestVariable(Source);
        }
        //
        // ====================================================================================================
        //
        public override string GetArgument(string Name, string ArgumentString, string DefaultValue, string Delimiter) {
            return GenericController.getValueFromKeyValueString(Name, ArgumentString, DefaultValue, Delimiter);
        }
        public override string GetArgument(string Name, string ArgumentString, string DefaultValue) {
            return GenericController.getValueFromKeyValueString(Name, ArgumentString, DefaultValue, "");
        }
        public override string GetArgument(string Name, string ArgumentString) {
            return GenericController.getValueFromKeyValueString(Name, ArgumentString, "", "");
        }
        //
        // ====================================================================================================
        //
        public override string GetFilename(string PathFilename) {
            string filename = "";
            string path = "";
            cp.core.filePrivate.splitDosPathFilename(PathFilename, ref path, ref filename);
            return filename;
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
        public override int GetRandomInteger() {
            return GenericController.GetRandomInteger(cp.core);
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
        /// <param name="Path"></param>
        /// <param name="Page"></param>
        /// <param name="QueryString"></param>
        public override void SeparateURL(string SourceURL, ref string Protocol, ref string Host, ref string Path, ref string Page, ref string QueryString) {
            GenericController.splitUrl(SourceURL, ref Protocol, ref Host, ref Path, ref Page, ref QueryString);
        }
        public override void SeparateURL(string SourceURL, ref string Protocol, ref string Host, ref string port, ref string Path, ref string Page, ref string QueryString) {
            GenericController.splitUrl(SourceURL, ref Protocol, ref Host, ref port, ref Path, ref Page, ref QueryString);
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
        public override bool IsGuid(string guid) {
            return GenericController.common_isGuid(guid);
        }
        // todo implement taskId return value, create cp.task object to track task status
        //====================================================================================================
        /// <summary>
        /// Install an addon collection file asynchonously. The task is queued and the taskId is returned. Use cp.tasks.getTaskStatus to determine status
        /// </summary>
        /// <param name="privateFile"></param>
        /// <returns></returns>
        public override int InstallCollectionFromFile(string privateFile) {
            int taskId = 0;
            string ignoreUserMessage = "";
            string ignoreGuid = "";
            var ignoreList = new List<string> { };
            var installedCollections = new List<string>();
            CollectionController.installCollectionsFromPrivateFile(cp.core, privateFile, ref ignoreUserMessage, ref ignoreGuid, false, true, ref ignoreList, "CPUtilsClass.installCollectionFromFile [" + privateFile + "]", ref installedCollections);
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
        public override int InstallCollectionsFromFolder(string privateFolder, bool deleteFolderWhenDone) {
            int taskId = 0;
            string ignoreUserMessage = "";
            List<string> ignoreList1 = new List<string>();
            List<string> ignoreList2 = new List<string>();
            string logPrefix = "CPUtilsClass.installCollectionsFromFolder";
            var installedCollections = new List<string>();
            CollectionController.installCollectionsFromPrivateFolder(cp.core, privateFolder, ref ignoreUserMessage, ref ignoreList1, false, false, ref ignoreList2, logPrefix, ref installedCollections, true);
            return taskId;
        }
        // todo implement taskId return value, create cp.task object to track task status
        //====================================================================================================
        /// <summary>
        /// Install all addon collections in a folder asynchonously. The task is queued and the taskId is returned. Use cp.tasks.getTaskStatus to determine status
        /// </summary>
        /// <param name="privateFolder"></param>
        /// <returns></returns>
        public override int InstallCollectionsFromFolder(string privateFolder) {
            return InstallCollectionsFromFolder(privateFolder, false);
        }
        // todo implement taskId return value, create cp.task object to track task status
        //====================================================================================================
        /// <summary>
        /// Install an addon collections from the collection library asynchonously. The task is queued and the taskId is returned. Use cp.tasks.getTaskStatus to determine status
        /// </summary>
        public override int InstallCollectionFromLibrary(string collectionGuid ) {
            int taskId = 0;
            string ignoreUserMessage = "";
            var installedCollections = new List<string>();
            string logPrefix = "installCollectionFromLibrary";
            var nonCriticalErrorList = new List<string>();
            CollectionController.installCollectionFromRemoteRepo(cp.core, collectionGuid, ref ignoreUserMessage, "", false, false, ref nonCriticalErrorList, logPrefix, ref installedCollections);
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
        public override int InstallCollectionFromLink(string link) {
            throw new NotImplementedException("installCollectionFromLink not implemented");
        }
        //
        //====================================================================================================
        /// <summary>
        /// Converts html content to the wysiwyg editor compatible format that includes edit icons for addons. Use this to convert the html content added to wysiwyg editors. Use EncodeHtmlFromWysiwygEditor() before saving back to Db.
        /// </summary>
        /// <param name="Source"></param>
        /// <returns></returns>
        public override string EncodeHtmlForWysiwygEditor(string Source) {
            return ActiveContentController.renderHtmlForWysiwygEditor(cp.core, Source);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Converts html content from wysiwyg editors to be saved. See EncodeHtmlForWysiwygEditor() for more details.
        /// </summary>
        /// <param name="Source"></param>
        /// <returns></returns>
        public override string DecodeHtmlFromWysiwygEditor(string Source) {
            return ActiveContentController.processWysiwygResponseForSave(cp.core, Source);
        }
        //
        // ====================================================================================================
        //
        private void appendDebugLog(string copy) {
            //My.Computer.FileSystem.WriteAllText("c:\clibCpDebug.log", Now & " - cp.utils, " & copy & vbCrLf, True)
            // 'My.Computer.FileSystem.WriteAllText(System.AppDocmc.main_CurrentDocmc.main_BaseDirectory() & "cpLog.txt", Now & " - " & copy & vbCrLf, True)
        }
        //
        //====================================================================================================
        //
        public override void ExportCsv(string sql, string exportName, string filename) {
            try {
                var ExportCSVAddon = AddonModel.create(cp.core, addonGuidExportCSV);
                if (ExportCSVAddon == null) {
                    LogController.handleError(cp.core, new GenericException("ExportCSV addon not found. Task could not be added to task queue."));
                } else {
                    var cmdDetail = new TaskModel.CmdDetailClass() {
                        addonId = ExportCSVAddon.id,
                        addonName = ExportCSVAddon.name,
                        args = new Dictionary<string, string> {
                            { "sql", sql },
                            { "ExportName", exportName },
                            { "filename", filename }
                        }
                    };
                    TaskSchedulerController.addTaskToQueue(cp.core, cmdDetail, false);
                }
            } catch (Exception) {
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// convert a file link (like /ccLibraryFiles/imageFilename/000001/this.png) to a full URL
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        public override string EncodeAppRootPath(string link) {
            return GenericController.encodeVirtualPath(GenericController.encodeText(link), cp.core.appConfig.cdnFileUrl, appRootPath, cp.core.webServer.requestDomain);
        }
        //
        //==============================================================================================================
        /// <summary>
        /// convert fileInfo array to parsable string [filename-attributes-creationTime-lastAccessTime-lastWriteTime-length-entension]
        /// </summary>
        /// <param name="FileInfo"></param>
        /// <returns></returns>
        public override string Upgrade51ConvertFileInfoArrayToParseString(List<CPFileSystemBaseClass.FileDetail> FileInfo) {
            return UpgradeController.Upgrade51ConvertFileInfoArrayToParseString(FileInfo);
        }
        //
        //==============================================================================================================
        /// <summary>
        /// convert directoryInfo object to parsable string [filename-attributes-creationTime-lastAccessTime-lastWriteTime-extension]
        /// </summary>
        /// <param name="DirectoryInfo"></param>
        /// <returns></returns>
        public override string Upgrade51ConvertDirectoryInfoArrayToParseString(List<CPFileSystemBaseClass.FolderDetail> DirectoryInfo) {
            return UpgradeController.Upgrade51ConvertDirectoryInfoArrayToParseString(DirectoryInfo);
        }
        //
        //====================================================================================================
        // deprecated
        //
        [Obsolete("Installation upgrade through the cp interface is deprecated. Please use the command line tool.", true)]
        public override void Upgrade(bool isNewApp) {
            try {
                throw new GenericException("Installation upgrade through the cp interface is deprecated. Please use the command line tool.");
                // Controllers.appBuilderController.upgrade(CP.core, isNewApp)
            } catch (Exception ex) {
                LogController.handleError(cp.core, ex);
            }
        }
        //
        [Obsolete("deprecated", true)]
        public override string DecodeUrl(string Url) {
            return GenericController.decodeURL(Url);
        }
        //
        [Obsolete("deprecated", true)]
        public override string DecodeHTML(string Source) {
            return HtmlController.decodeHtml(Source);
        }
        //
        [Obsolete("deprecated", true)]
        public override string EncodeHTML(string Source) {
            string returnValue = "";
            //
            if (!string.IsNullOrEmpty(Source)) {
                returnValue = HtmlController.encodeHtml(Source);
            }
            return returnValue;
        }
        //
        [Obsolete("deprecated", true)]
        public override string EncodeUrl(string Source) {
            return GenericController.encodeURL(Source);
        }
        //
        [Obsolete("deprecated", true)]
        public override string GetPleaseWaitEnd() {
            return cp.core.programFiles.readFileText("resources\\WaitPageClose.htm");
        }
        //
        [Obsolete("deprecated", true)]
        public override string GetPleaseWaitStart() {
            return cp.core.programFiles.readFileText("Resources\\WaitPageOpen.htm");
        }
        //
        [Obsolete("Deprecated, use cp.addon.Execute", true)]
        public override string ExecuteAddon(string IdGuidOrName, int WrapperId) => (string)cp.Addon.Execute(
            IdGuidOrName,
            new addonExecuteContext() {
                addonType = addonContext.ContextPage,
                wrapperID = WrapperId,
                instanceGuid = cp.core.docProperties.getText("instanceId")
            }
        );
        //
        [Obsolete("Deprecated, use cp.addon.Execute", true)]
        public override string ExecuteAddon(string IdGuidOrName) => (string)cp.Addon.Execute(
            IdGuidOrName,
            new addonExecuteContext() {
                addonType = addonContext.ContextPage,
                instanceGuid = cp.core.docProperties.getText("instanceId")
            }
        );
        //
        [Obsolete("Deprecated, use cp.addon.Execute", true)]
        public override string ExecuteAddon(string IdGuidOrName, addonContext context) => (string)cp.Addon.Execute(
            IdGuidOrName,
            new addonExecuteContext() {
                addonType = context,
                instanceGuid = cp.core.docProperties.getText("instanceId")
            }
        );
        //
        [Obsolete("Deprecated, use cp.addon.Execute", true)]
        public override string ExecuteAddonAsProcess(string AddonIDGuidOrName) {
            try {
                Models.Db.AddonModel addon = null;
                if (EncodeInteger(AddonIDGuidOrName) > 0) {
                    addon = cp.core.addonCache.getAddonById(EncodeInteger(AddonIDGuidOrName));
                } else if (GenericController.isGuid(AddonIDGuidOrName)) {
                    addon = cp.core.addonCache.getAddonByGuid(AddonIDGuidOrName);
                } else {
                    addon = cp.core.addonCache.getAddonByName(AddonIDGuidOrName);
                }
                if (addon != null) {
                    cp.core.addon.executeAsync(addon);
                }
            } catch (Exception ex) {
                LogController.handleError(cp.core, ex);
            }
            return string.Empty;
        }
        //
        [Obsolete("Deprecated, use AppendLog")]
        public override void AppendLogFile(string Text) {
            LogController.logInfo(cp.core, Text);
        }
        //
        [Obsolete("Deprecated, file logging is no longer supported. Use AppendLog(message) to log Info level messages")]
        public override void AppendLog(string pathFilename, string Text) {
            if ((!string.IsNullOrWhiteSpace(pathFilename)) && (!string.IsNullOrWhiteSpace(Text))) {
                pathFilename = GenericController.convertToDosSlash(pathFilename);
                string[] parts = pathFilename.Split('\\');
                LogController.logInfo(cp.core, "legacy logFile: [" + pathFilename + "], " + Text);
            }
        }
        //
        [Obsolete("Deprecated", true)]
        public override string ConvertLinkToShortLink(string URL, string ServerHost, string ServerVirtualPath) {
            return GenericController.ConvertLinkToShortLink(URL, ServerHost, ServerVirtualPath);
        }
        //
        [Obsolete("Deprecated", true)]
        public override string ConvertShortLinkToLink(string url, string pathPagePrefix) {
            return GenericController.removeUrlPrefix(url, pathPagePrefix);
        }
        //
        [Obsolete("Deprecated. Use native methods to convert date formats.", true)]
        public override DateTime DecodeGMTDate(string GMTDate) {
            return GenericController.deprecatedDecodeGMTDate(GMTDate);
        }
        //
        [Obsolete("Deprecated.", true)]
        public override bool isGuid(string guid) {
            throw new NotImplementedException();
        }
        //
        [Obsolete("Deprecated.", true)]
        public override int installCollectionFromFile(string privateFile) {
            throw new NotImplementedException();
        }
        //
        [Obsolete("Deprecated.", true)]
        public override int installCollectionsFromFolder(string privateFolder, bool deleteFolderWhenDone) {
            throw new NotImplementedException();
        }
        //
        [Obsolete("Deprecated.", true)]
        public override int installCollectionsFromFolder(string privateFolder) {
            throw new NotImplementedException();
        }
        //
        [Obsolete("Deprecated.", true)]
        public override int installCollectionFromLibrary(string collectionGuid) {
            throw new NotImplementedException();
        }
        //
        [Obsolete("Deprecated.", true)]
        public override int installCollectionFromLink(string link) {
            throw new NotImplementedException();
        }
        [Obsolete("Use SeparateURL(), true ")]
        public override void ParseURL(string url, ref string return_protocol, ref string return_domain, ref string return_port, ref string return_path, ref string return_page, ref string return_queryString) {
            SeparateURL(url, ref return_protocol, ref return_domain, ref return_port, ref return_path, ref return_queryString);
        }
        //
        [Obsolete("Deprecated.", true)]
        public override string EncodeJavascript(string Source) {
            return GenericController.EncodeJavascriptStringSingleQuote(Source);
        }
        //
        [Obsolete("Deprecated.", true)]
        public override string EncodeQueryString(string Source) {
            return GenericController.encodeQueryString(Source);
        }
        //
        [Obsolete("Deprecated.", true)]
        public override DateTime GetFirstNonZeroDate(DateTime Date0, DateTime Date1) {
            return GenericController.getFirstNonZeroDate(Date0, Date1);
        }
        //
        [Obsolete("Deprecated.", true)]
        public override int GetFirstNonZeroInteger(int Integer0, int Integer1) {
            return GenericController.getFirstNonZeroInteger(Integer0, Integer1);
        }
        //
        [Obsolete("Deprecated.", true)]
        public override string GetIntegerString(int Value, int DigitCount) {
            return GenericController.getIntegerString(Value, DigitCount);
        }
        //
        [Obsolete("Deprecated.", true)]
        public override string GetLine(string Body) {
            return GenericController.getLine(ref Body);
        }
        //
        [Obsolete("Deprecated.", true)]
        public override int GetProcessID() {
            return Process.GetCurrentProcess().Id;
        }
        //
        [Obsolete("Deprecated.", true)]
        public override void Sleep(int timeMSec) {
            System.Threading.Thread.Sleep(timeMSec);
        }
        //
        [Obsolete("Deprecated.", true)]
        public override string hashMd5(string source) {
            throw new NotImplementedException("hashMd5 not implemented");
            //Return HashPasswordForStoringInConfigFile(source, "md5")
        }
        //
        // dispose
        //
        #region  IDisposable Support 
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
                }
                //
                // Add code here to release the unmanaged resource.
                //
            }
            this.disposed = true;
        }
        protected bool disposed = false;
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