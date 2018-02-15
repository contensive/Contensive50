
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Core;
using Contensive.Core.Models.DbModels;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
//
using Contensive.BaseClasses;
//
namespace Contensive.Core {
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
                throw new ApplicationException("Installation upgrade through the cp interface is deprecated. Please use the command line tool.");
                // Controllers.appBuilderController.upgrade(CP.core, isNewApp)
            } catch (Exception ex) {
                CP.core.handleException(ex);
            }
        }
        //
        // ====================================================================================================
        //
        public override string ConvertHTML2Text(string Source) {
            return NUglify.Uglify.HtmlToText(Source).Code; //  htmlToTextControllers.convert(CP.core, Source);
        }
        //
        // ====================================================================================================
        //
        public override string ConvertText2HTML(string Source) {
            return CP.core.html.convertTextToHTML(Source);
        }
        //
        // ====================================================================================================
        //
        public override string CreateGuid() {
            return genericController.createGuid();
        }
        //
        // ====================================================================================================
        //
        public override string DecodeUrl(string Url) {
            return genericController.DecodeURL(Url);
        }
        //
        // ====================================================================================================
        //
        public override string EncodeContentForWeb(string Source, string ContextContentName = "", int ContextRecordID = 0, int WrapperID = 0) {
            return activeContentController.renderHtmlForWeb(CP.core, Source, ContextContentName, ContextRecordID, 0, "", WrapperID, CPUtilsBaseClass.addonContext.ContextPage);
        }
        //
        // ====================================================================================================
        //
        public override string DecodeHTML(string Source) {
            return genericController.decodeHtml(Source);
        }
        //
        // ====================================================================================================
        //
        public override string EncodeHTML(string Source) {
            string returnValue = "";
            //
            if (!string.IsNullOrEmpty(Source)) {
                returnValue = genericController.encodeHTML(Source);
            }
            return returnValue;
        }
        //
        // ====================================================================================================
        //
        public override string EncodeUrl(string Source) {
            return genericController.EncodeURL(Source);
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
            return genericController.encodeInteger(Expression);
        }
        //
        // ====================================================================================================
        //
        public override double EncodeNumber(object Expression) {
            return genericController.encodeNumber(Expression);
        }
        //
        // ====================================================================================================
        //
        public override string EncodeText(object Expression) {
            return genericController.encodeText(Expression);
        }
        //
        // ====================================================================================================
        //
        public override bool EncodeBoolean(object Expression) {
            return genericController.encodeBoolean(Expression);
        }
        //
        // ====================================================================================================
        //
        public override DateTime EncodeDate(object Expression) {
            return genericController.encodeDate(Expression);
        }
        //
        // ====================================================================================================
        //
        private string ExecuteAddon(string IdGuidOrName, addonExecuteContext executeConext) {
            if (IdGuidOrName.IsNumeric()) {
                executeConext.errorCaption = "id:" + IdGuidOrName;
                return CP.core.addon.execute(Models.DbModels.addonModel.create(CP.core, genericController.encodeInteger(IdGuidOrName)), executeConext);
            } else if (genericController.isGuid(IdGuidOrName)) {
                executeConext.errorCaption = "guid:" + IdGuidOrName;
                return CP.core.addon.execute(Models.DbModels.addonModel.create(CP.core, IdGuidOrName), executeConext);
            } else {
                executeConext.errorCaption = IdGuidOrName;
                return CP.core.addon.execute(Models.DbModels.addonModel.createByName(CP.core, IdGuidOrName), executeConext);
            }
        }
        //
        // ====================================================================================================
        //
        public override string ExecuteAddon(string IdGuidOrName, int WrapperId) => ExecuteAddon(
            IdGuidOrName,
            new addonExecuteContext() {
                addonType = addonContext.ContextPage,
                wrapperID = WrapperId
            }
        );
        //
        // ====================================================================================================
        //
        public override string ExecuteAddon(string IdGuidOrName) => ExecuteAddon(
            IdGuidOrName,
            new addonExecuteContext() {
                addonType = addonContext.ContextPage
            }
        );
        //
        // ====================================================================================================
        //
        public override string ExecuteAddon(string IdGuidOrName, addonContext context) => ExecuteAddon(
            IdGuidOrName,
            new addonExecuteContext() {
                addonType = context
            }
        );
        //
        // ====================================================================================================
        //
        public override string ExecuteAddonAsProcess(string IdGuidOrName) {
            return CP.core.addon.executeAsync(IdGuidOrName, CP.core.docProperties.getLegacyOptionStringFromVar());
        }
        [Obsolete("Deprecated, use AppendLog")]
        public override void AppendLogFile(string Text) {
            logController.appendLog(CP.core, Text);
        }
        //
        // ====================================================================================================
        /// <summary>
        /// simpulate legacy append method. First segment of pathFilename is used as the log path, filename w/o extension is used as file prefix
        /// </summary>
        /// <param name="pathFilename"></param>
        /// <param name="Text"></param>
        public override void AppendLog(string pathFilename, string Text) {
            if ((!string.IsNullOrWhiteSpace(pathFilename)) & (!string.IsNullOrWhiteSpace(Text))) {
                pathFilename = genericController.convertToDosSlash(pathFilename);
                string[] parts = pathFilename.Split('\\');
                if (parts.Count() == 1) {
                    //
                    // -- use pathFilename as prefix
                    logController.appendLog( CP.core, Text, "", System.IO.Path.GetFileNameWithoutExtension(pathFilename));
                } else {
                    //
                    // -- use pathFilename[0] as path, filename as prefix
                    logController.appendLog(CP.core, Text, parts[0], System.IO.Path.GetFileNameWithoutExtension(parts[(parts.Count()-1)]));
                }
            }
        }
        //
        // ====================================================================================================
        //
        public override void AppendLog(string Text) {
            logController.appendLog(CP.core, Text);
        }
        //
        // ====================================================================================================
        //
        public override string ConvertLinkToShortLink(string URL, string ServerHost, string ServerVirtualPath) {
            return genericController.ConvertLinkToShortLink(URL, ServerHost, ServerVirtualPath);
        }
        //
        // ====================================================================================================
        //
        public override string ConvertShortLinkToLink(string URL, string PathPagePrefix) {
            return genericController.ConvertShortLinkToLink(URL, PathPagePrefix);
        }
        //
        // ====================================================================================================
        //
        public override DateTime DecodeGMTDate(string GMTDate) {
            return genericController.DecodeGMTDate(GMTDate);
        }
        //
        // ====================================================================================================
        //
        public override string DecodeResponseVariable(string Source) {
            return genericController.DecodeResponseVariable(Source);
        }
        //
        // ====================================================================================================
        //
        public override string EncodeJavascript(string Source) {
            return genericController.EncodeJavascript(Source);
        }
        //
        // ====================================================================================================
        //
        public override string EncodeQueryString(string Source) {
            return genericController.EncodeQueryString(Source);
        }
        //
        // ====================================================================================================
        //
        public override string EncodeRequestVariable(string Source) {
            return genericController.EncodeRequestVariable(Source);
        }
        //
        // ====================================================================================================
        //
        public override string GetArgument(string Name, string ArgumentString, string DefaultValue = "", string Delimiter = "") {
            return genericController.GetArgument(Name, ArgumentString, DefaultValue, Delimiter);
        }
        //
        // ====================================================================================================
        //
        public override string GetFilename(string PathFilename) {
            string filename = "";
            string path = "";
            CP.core.privateFiles.splitPathFilename(PathFilename, ref path, ref filename);
            return filename;
        }
        //
        // ====================================================================================================
        //
        public override DateTime GetFirstNonZeroDate(DateTime Date0, DateTime Date1) {
            return genericController.GetFirstNonZeroDate(Date0, Date1);
        }
        //
        // ====================================================================================================
        //
        public override int GetFirstNonZeroInteger(int Integer0, int Integer1) {
            return genericController.GetFirstNonZeroInteger(Integer0, Integer1);
        }
        //
        // ====================================================================================================
        //
        public override string GetIntegerString(int Value, int DigitCount) {
            return genericController.GetIntegerString(Value, DigitCount);
        }
        //
        // ====================================================================================================
        //
        public override string GetLine(string Body) {
            return genericController.getLine(ref Body);
        }
        //
        // ====================================================================================================
        //
        public override int GetListIndex(string Item, string ListOfItems) {
            return genericController.GetListIndex(Item, ListOfItems);
        }
        //
        // ====================================================================================================
        //
        public override int GetProcessID() {
            return genericController.GetProcessID();
        }
        //
        // ====================================================================================================
        //
        public override int GetRandomInteger() {
            return genericController.GetRandomInteger(CP.core);
        }
        //
        // ====================================================================================================
        //
        public override bool IsInDelimitedString(string DelimitedString, string TestString, string Delimiter) {
            return genericController.IsInDelimitedString(DelimitedString, TestString, Delimiter);
        }
        //
        // ====================================================================================================
        //
        public override string ModifyLinkQueryString(string Link, string QueryName, string QueryValue, bool AddIfMissing = true) {
            return genericController.ModifyLinkQueryString(Link, QueryName, QueryValue, AddIfMissing);
        }
        //
        // ====================================================================================================
        //
        public override string ModifyQueryString(string WorkingQuery, string QueryName, string QueryValue, bool AddIfMissing = true) {
            return genericController.ModifyQueryString(WorkingQuery, QueryName, QueryValue, AddIfMissing);
        }
        //
        // ====================================================================================================
        //
        public override void ParseURL(string SourceURL, ref string Protocol, ref string Host, ref string Port, ref string Path, ref string Page, ref string QueryString) {
            genericController.ParseURL(SourceURL, ref Protocol, ref Host, ref Port, ref Path, ref Page, ref QueryString);
        }
        //
        // ====================================================================================================
        //
        public override void SeparateURL(string SourceURL, ref string Protocol, ref string Host, ref string Path, ref string Page, ref string QueryString) {
            genericController.splitUrl(SourceURL, ref Protocol, ref Host, ref Path, ref Page, ref QueryString);
        }
        //
        // ====================================================================================================
        //
        public override object SplitDelimited(string WordList, string Delimiter) {
            return genericController.SplitDelimited(WordList, Delimiter);
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
            return genericController.common_isGuid(guid);
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
            collectionController.InstallCollectionsFromPrivateFile(CP.core, privateFile, ref ignoreUserMessage, ref ignoreGuid, false, ref ignoreList);
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
            collectionController.InstallCollectionsFromPrivateFolder(CP.core, privateFolder, ref ignoreUserMessage, ref ignoreList1, false, ref ignoreList2);
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
        public override int installCollectionFromLibrary(string collectionGuid) {
            int taskId = 0;
            string ignoreUserMessage = "";
            collectionController.installCollectionFromRemoteRepo(CP.core, collectionGuid, ref ignoreUserMessage, "", false);
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
            return activeContentController.renderHtmlForWysiwygEditor(CP.core, Source);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Converts html content from wysiwyg editors to be saved. See EncodeHtmlForWysiwygEditor() for more details.
        /// </summary>
        /// <param name="Source"></param>
        /// <returns></returns>
        public override string DecodeHtmlFromWysiwygEditor(string Source) {
            return activeContentController.processWysiwygResponseForSave(CP.core, Source);
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
            //todo  NOTE: The base class Finalize method is automatically called from the destructor:
            //base.Finalize();
        }
        #endregion
    }

}