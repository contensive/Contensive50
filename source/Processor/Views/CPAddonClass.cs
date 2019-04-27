
using System;
using System.Collections.Generic;
using Contensive.BaseClasses;
using Contensive.Processor.Controllers;

namespace Contensive.Processor {
    //
    // todo implement or deprecate. might be nice to have this convenient api, but a model does the same, costs one query but will
    // always have the model at the save version as the addon code - this cp interface will match the database, but not the addon.
    // not sure which is better
    public class CPAddonClass : CPAddonBaseClass, IDisposable {
        //
        // todo remove all com guid references
        #region COM GUIDs
        public const string ClassId = "6F43E5CA-6367-475C-AE65-FC988234922A";
        public const string InterfaceId = "440D19E3-47A9-4CA2-B20C-077221015525";
        public const string EventsId = "70B800AA-148A-4338-9EDB-70C85E1ADBDD";
        #endregion
        //
        // ====================================================================================================
        /// <summary>
        /// dependencies
        /// </summary>
        private CPClass cp;
        //
        // ====================================================================================================
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cp"></param>
        public CPAddonClass(CPClass cp) => this.cp = cp;
        //
        //====================================================================================================
        /// <summary>
        /// The id of the addon currently executing
        /// </summary>
        public override int ID => cp.core.doc.addonModelStack.Peek().id;
        //
        //====================================================================================================
        /// <summary>
        /// The guid of the addon currently executing
        /// </summary>
        public override string ccGuid => cp.core.doc.addonModelStack.Peek().ccguid;
        //
        //====================================================================================================
        //
        public override object Execute(string addonGuid) {
            return cp.core.addon.execute(Models.Db.AddonModel.create(cp.core, addonGuid), new BaseClasses.CPUtilsBaseClass.addonExecuteContext());
        }
        //
        //====================================================================================================
        //
        public override object Execute(string addonGuid, Dictionary<string, string> argumentKeyValuePairs) {
            return cp.core.addon.execute(Models.Db.AddonModel.create(cp.core, addonGuid), new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                 argumentKeyValuePairs = argumentKeyValuePairs
            });
        }
        //
        //====================================================================================================
        //
        public override object Execute(string addonGuid, CPUtilsBaseClass.addonExecuteContext executeContext) {
            return cp.core.addon.execute(Models.Db.AddonModel.create(cp.core, addonGuid), executeContext);
        }
        //
        //====================================================================================================
        //
        public override object Execute(int addonId) {
            return cp.core.addon.execute(Models.Db.AddonModel.create(cp.core, addonId), new BaseClasses.CPUtilsBaseClass.addonExecuteContext());
        }
        //
        //====================================================================================================
        //
        public override object Execute(int addonId, Dictionary<string, string> argumentKeyValuePairs) {
            return cp.core.addon.execute(Models.Db.AddonModel.create(cp.core, addonId), new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                argumentKeyValuePairs = argumentKeyValuePairs
            });
        }
        //
        //====================================================================================================
        //
        public override object Execute(int addonId, CPUtilsBaseClass.addonExecuteContext executeContext) {
            return cp.core.addon.execute(Models.Db.AddonModel.create(cp.core, addonId), executeContext);
        }
        //
        //====================================================================================================
        //
        public override object ExecuteByUniqueName(string addonName) {
            return cp.core.addon.execute(Models.Db.AddonModel.createByUniqueName(cp.core, addonName), new BaseClasses.CPUtilsBaseClass.addonExecuteContext());
        }
        //
        //====================================================================================================
        //
        public override object ExecuteByUniqueName(string addonName, Dictionary<string, string> argumentKeyValuePairs) {
            return cp.core.addon.execute(Models.Db.AddonModel.createByUniqueName(cp.core, addonName), new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                argumentKeyValuePairs = argumentKeyValuePairs
            });
        }
        //
        //====================================================================================================
        //
        public override object ExecuteByUniqueName(string addonName, CPUtilsBaseClass.addonExecuteContext executeContext) {
            return cp.core.addon.execute(Models.Db.AddonModel.createByUniqueName(cp.core, addonName), executeContext);
        }
        //
        //====================================================================================================
        /// <summary>
        /// execute an addon asyncronously. The session environment will include the same user, visit, doc. Include argument keyValuePairs available to the addon through cp.doc.get
        /// </summary>
        /// <param name="Addonid"></param>
        /// <param name="keyValuePairs"></param>
        public override void ExecuteAsync(int Addonid, Dictionary<string, string> keyValuePairs) {
            cp.core.addon.executeAsync(Models.Db.AddonModel.create(cp.core, Addonid), keyValuePairs);
        }
        //
        //====================================================================================================
        //
        public override void ExecuteAsync(int Addonid) {
            cp.core.addon.executeAsync(Models.Db.AddonModel.create(cp.core, Addonid), new Dictionary<string, string>());
        }
        //
        //====================================================================================================
        //
        public override void ExecuteAsync(string guid, Dictionary<string, string> keyValuePairs) {
            cp.core.addon.executeAsync(Models.Db.AddonModel.create(cp.core, guid), keyValuePairs);
        }
        //
        //====================================================================================================
        //
        public override void ExecuteAsync(string guid) {
            cp.core.addon.executeAsync(Models.Db.AddonModel.create(cp.core, guid), new Dictionary<string, string>());
        }
        //
        //====================================================================================================
        //
        public override void ExecuteAsyncByUniqueName(string name, Dictionary<string, string> keyValuePairs) {
            cp.core.addon.executeAsync(Models.Db.AddonModel.createByUniqueName(cp.core, name), keyValuePairs);
        }
        //
        //====================================================================================================
        //
        public override void ExecuteAsyncByUniqueName(string name) {
            cp.core.addon.executeAsync(Models.Db.AddonModel.createByUniqueName(cp.core, name), new Dictionary<string, string>());
        }
        //==========================================================================================
        /// <summary>
        /// Install an uploaded collection file from a private folder. Return true if successful, else the issue is in the returnUserError
        /// </summary>
        /// <param name="privatePathFilename"></param>
        /// <param name="returnUserError"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public override bool InstallCollectionFile(string privatePathFilename, ref string returnUserError) {
            bool returnOk = false;
            try {
                string ignoreReturnedCollectionGuid = "";
                var tmpList = new List<string> { };
                string logPrefix = "CPSiteClass.installCollectionFile";
                var installedCollections = new List<string>();
                var context = new Stack<string>();
                context.Push("Api call cp.addon.InstallCollectionFile [" + privatePathFilename + "]");
                returnOk = Controllers.CollectionController.installCollectionFromPrivateFile(cp.core, context, privatePathFilename, ref returnUserError, ref ignoreReturnedCollectionGuid, false, true, ref tmpList, logPrefix, ref installedCollections);
            } catch (Exception ex) {
                Controllers.LogController.handleError(cp.core, ex);
                if (!cp.core.siteProperties.trapErrors) {
                    throw;
                }
            }
            return returnOk;
        }
        //
        public override int InstallCollectionFileAsync(string privatePathFilename ) { throw new NotImplementedException(); }
        //
        //====================================================================================================
        /// <summary>
        /// Install all addon collections in a folder asynchonously. Optionally delete the folder. The task is queued and the taskId is returned. Use cp.tasks.getTaskStatus to determine status
        /// </summary>
        /// <param name="privateFolder"></param>
        /// <param name="deleteFolderWhenDone"></param>
        /// <returns></returns>
        public override bool InstallCollectionsFromFolder(string privateFolder, bool deleteFolderWhenDone, ref string returnUserError) {
            string ignoreUserMessage = "";
            List<string> ignoreList1 = new List<string>();
            List<string> ignoreList2 = new List<string>();
            string logPrefix = "CPUtilsClass.installCollectionsFromFolder";
            var collectionsInstalledList = new List<string>();
            var collectionsDownloaded = new List<string>();
            var context = new Stack<string>();
            context.Push("Api call cp.addon.InstallCollectionFromFolder [" + privateFolder + "]");
            return CollectionController.installCollectionsFromPrivateFolder(cp.core, context, privateFolder, ref ignoreUserMessage, ref collectionsInstalledList, false, false, ref ignoreList2, logPrefix, true, ref collectionsDownloaded);
        }
        //
        public override int InstallCollectionsFromFolderAsync(string privateFolder, bool deleteFolderWhenDone) { throw new NotImplementedException(); }
        //
        //====================================================================================================
        /// <summary>
        /// Install an addon collections from the collection library asynchonously. The task is queued and the taskId is returned. Use cp.tasks.getTaskStatus to determine status
        /// </summary>
        public override bool InstallCollectionFromLibrary(string collectionGuid, ref string returnUserError) {
            string ignoreUserMessage = "";
            var installedCollections = new List<string>();
            string logPrefix = "installCollectionFromLibrary";
            var nonCriticalErrorList = new List<string>();
            var context = new Stack<string>();
            context.Push("Api call cp.addon.InstallCollectionFromLibrary [" + collectionGuid + "]");
            return CollectionController.installCollectionFromLibrary(cp.core, context, collectionGuid, ref ignoreUserMessage, false, false, ref nonCriticalErrorList, logPrefix, ref installedCollections);
        }
        //
        public override int InstallCollectionFromLibraryAsync(string collectionGuid) { throw new NotImplementedException(); }
        //
        //====================================================================================================
        //
        public override bool InstallCollectionFromLink(string link, ref string returnUserError) { throw new NotImplementedException(); }
        //
        public override int InstallCollectionFromLinkAsync(string link) { throw new NotImplementedException(); }
        //
        //====================================================================================================
        // Deprecated methods
        //
        [Obsolete("Deprecated", false)]
        public override bool Admin => cp.core.doc.addonModelStack.Peek().admin;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override string ArgumentList => cp.core.doc.addonModelStack.Peek().argumentList;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override bool AsAjax => cp.core.doc.addonModelStack.Peek().admin;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override string BlockDefaultStyles => "";
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override int CollectionID => cp.core.doc.addonModelStack.Peek().collectionID;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override bool Content => cp.core.doc.addonModelStack.Peek().content;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override string Copy => cp.core.doc.addonModelStack.Peek().copy;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override string CopyText => cp.core.doc.addonModelStack.Peek().copyText;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override string CustomStyles => "";
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override string DefaultStyles => cp.core.doc.addonModelStack.Peek().stylesFilename.content;
        //
        //====================================================================================================
        // 
        [Obsolete("Deprecated", false)]
        public override string Description => "";
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override string DotNetClass => cp.core.doc.addonModelStack.Peek().dotNetClass;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override string FormXML => cp.core.doc.addonModelStack.Peek().formXML;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override string Help => cp.core.doc.addonModelStack.Peek().help;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override string HelpLink => cp.core.doc.addonModelStack.Peek().helpLink;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override string IconFilename => cp.core.doc.addonModelStack.Peek().iconFilename;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override int IconHeight => cp.core.doc.addonModelStack.Peek().iconHeight;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override int IconSprites => cp.core.doc.addonModelStack.Peek().iconSprites;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override int IconWidth => cp.core.doc.addonModelStack.Peek().iconWidth;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override bool InFrame => cp.core.doc.addonModelStack.Peek().inFrame;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override bool IsInline => cp.core.doc.addonModelStack.Peek().isInline;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override string JavaScriptBodyEnd => "";
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override string JavascriptInHead => "";
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override string JavaScriptOnLoad => "";
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override string Link => cp.core.doc.addonModelStack.Peek().link;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override string MetaDescription => cp.core.doc.addonModelStack.Peek().metaDescription;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override string MetaKeywordList => cp.core.doc.addonModelStack.Peek().metaKeywordList;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override string Name => cp.core.doc.addonModelStack.Peek().name;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override string NavIconType => "";
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override string ObjectProgramID => cp.core.doc.addonModelStack.Peek().objectProgramID;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override bool OnBodyEnd => cp.core.doc.addonModelStack.Peek().onBodyEnd;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override bool OnBodyStart => cp.core.doc.addonModelStack.Peek().onBodyStart;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override bool OnContentEnd => cp.core.doc.addonModelStack.Peek().onPageEndEvent;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override bool OnContentStart => cp.core.doc.addonModelStack.Peek().onPageStartEvent;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override bool Open(int AddonId) => false;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override bool Open(string AddonNameOrGuid) => false;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override string OtherHeadTags => cp.core.doc.addonModelStack.Peek().otherHeadTags;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override string PageTitle => cp.core.doc.addonModelStack.Peek().pageTitle;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override string ProcessInterval => cp.core.doc.addonModelStack.Peek().processInterval.ToString();
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override DateTime ProcessNextRun => cp.core.doc.addonModelStack.Peek().processNextRun;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override bool ProcessRunOnce => cp.core.doc.addonModelStack.Peek().processRunOnce;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override string RemoteAssetLink => cp.core.doc.addonModelStack.Peek().remoteAssetLink;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override bool RemoteMethod => cp.core.doc.addonModelStack.Peek().remoteMethod;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override string RobotsTxt => cp.core.doc.addonModelStack.Peek().robotsTxt;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override string ScriptCode => cp.core.doc.addonModelStack.Peek().scriptingCode;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override string ScriptEntryPoint => cp.core.doc.addonModelStack.Peek().scriptingEntryPoint;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override string ScriptLanguage => "";
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override string SharedStyles => "";
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", false)]
        public override bool Template => cp.core.doc.addonModelStack.Peek().template;
        //
        #region  IDisposable Support 
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        //
        // ====================================================================================================
        /// <summary>
        /// must call to dispose
        /// </summary>
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                if (disposing) {
                    //
                    // call .dispose for managed objects
                    //
                    cp = null;
                }
                //
                // Add code here to release the unmanaged resource.
                //
            }
            this.disposed = true;
        }
        //
        protected bool disposed = false;
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        //
        ~CPAddonClass() {
            Dispose(false);
        }
        #endregion
    }
}