
using System;
using System.Collections.Generic;
using Contensive.BaseClasses;

namespace Contensive.Processor {
    //
    // todo implement or deprecate. might be nice to have this convenient api, but a model does the same, costs one query but will
    // always have the model at the save version as the addon code - this cp interface will match the database, but not the addon.
    // not sure which is better
    public class CPAddonClass : BaseClasses.CPAddonBaseClass, IDisposable {
        //
        // todo remove all com guid references
        #region COM GUIDs
        public const string ClassId = "6F43E5CA-6367-475C-AE65-FC988234922A";
        public const string InterfaceId = "440D19E3-47A9-4CA2-B20C-077221015525";
        public const string EventsId = "70B800AA-148A-4338-9EDB-70C85E1ADBDD";
        #endregion
        //
        private CPClass cp;
        //
        // ====================================================================================================
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cp"></param>
        public CPAddonClass(CPClass cp) : base() => this.cp = cp;
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
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override bool Admin => cp.core.doc.addonModelStack.Peek().admin;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override string ArgumentList => cp.core.doc.addonModelStack.Peek().argumentList;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override bool AsAjax => cp.core.doc.addonModelStack.Peek().admin;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override string BlockDefaultStyles => "";
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override int CollectionID => cp.core.doc.addonModelStack.Peek().collectionID;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override bool Content => cp.core.doc.addonModelStack.Peek().content;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override string Copy => cp.core.doc.addonModelStack.Peek().copy;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override string CopyText => cp.core.doc.addonModelStack.Peek().copyText;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override string CustomStyles => "";
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override string DefaultStyles => cp.core.doc.addonModelStack.Peek().stylesFilename.content;
        //
        //====================================================================================================
        // 
        [Obsolete("Deprecated", true)]
        public override string Description => "";
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override string DotNetClass => cp.core.doc.addonModelStack.Peek().dotNetClass;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override string FormXML => cp.core.doc.addonModelStack.Peek().formXML;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override string Help => cp.core.doc.addonModelStack.Peek().help;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override string HelpLink => cp.core.doc.addonModelStack.Peek().helpLink;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override string IconFilename => cp.core.doc.addonModelStack.Peek().iconFilename;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override int IconHeight => cp.core.doc.addonModelStack.Peek().iconHeight;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override int IconSprites => cp.core.doc.addonModelStack.Peek().iconSprites;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override int IconWidth => cp.core.doc.addonModelStack.Peek().iconWidth;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override bool InFrame => cp.core.doc.addonModelStack.Peek().inFrame;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override bool IsInline => cp.core.doc.addonModelStack.Peek().isInline;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override string JavaScriptBodyEnd => "";
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override string JavascriptInHead => "";
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override string JavaScriptOnLoad => "";
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override string Link => cp.core.doc.addonModelStack.Peek().link;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override string MetaDescription => cp.core.doc.addonModelStack.Peek().metaDescription;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override string MetaKeywordList => cp.core.doc.addonModelStack.Peek().metaKeywordList;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override string Name => cp.core.doc.addonModelStack.Peek().name;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override string NavIconType => "";
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override string ObjectProgramID => cp.core.doc.addonModelStack.Peek().objectProgramID;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override bool OnBodyEnd => cp.core.doc.addonModelStack.Peek().onBodyEnd;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override bool OnBodyStart => cp.core.doc.addonModelStack.Peek().onBodyStart;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override bool OnContentEnd => cp.core.doc.addonModelStack.Peek().onPageEndEvent;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override bool OnContentStart => cp.core.doc.addonModelStack.Peek().onPageStartEvent;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override bool Open(int AddonId) => false;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override bool Open(string AddonNameOrGuid) => false;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override string OtherHeadTags => cp.core.doc.addonModelStack.Peek().otherHeadTags;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override string PageTitle => cp.core.doc.addonModelStack.Peek().pageTitle;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override string ProcessInterval => cp.core.doc.addonModelStack.Peek().processInterval.ToString();
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override DateTime ProcessNextRun => cp.core.doc.addonModelStack.Peek().processNextRun;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override bool ProcessRunOnce => cp.core.doc.addonModelStack.Peek().processRunOnce;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override string RemoteAssetLink => cp.core.doc.addonModelStack.Peek().remoteAssetLink;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override bool RemoteMethod => cp.core.doc.addonModelStack.Peek().remoteMethod;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override string RobotsTxt => cp.core.doc.addonModelStack.Peek().robotsTxt;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override string ScriptCode => cp.core.doc.addonModelStack.Peek().scriptingCode;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override string ScriptEntryPoint => cp.core.doc.addonModelStack.Peek().scriptingEntryPoint;
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override string ScriptLanguage => "";
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override string SharedStyles => "";
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
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