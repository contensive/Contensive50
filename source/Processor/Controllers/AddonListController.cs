
using Contensive.BaseClasses;
using Contensive.Processor.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Contensive.Processor.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// static class controller
    /// </summary>
    public class AddonListController : IDisposable {
        /// <summary>
        /// Render the html,css,js for an addonListItem
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="addonListItem"></param>
        public static void renderAddonListItem(CPBaseClass cp, AddonListItemModel addonListItem) {
            CoreController core = ((CPClass)(cp)).core;
            int assetSkipCnt = core.doc.htmlAssetList.Count;
            cp.Doc.SetProperty("instanceId", addonListItem.instanceGuid);
            addonListItem.renderedHtml = cp.Addon.Execute(addonListItem.designBlockTypeGuid).ToString();
            addonListItem.renderedAssets = new AddonAssetsModel();
            foreach (var asset in core.doc.htmlAssetList.Skip(assetSkipCnt)) {
                string key = ((asset.assetType == Constants.HtmlAssetTypeEnum.script) ? "js" : "css");
                key += ((asset.inHead) ? "-head" : "-body");
                key += ((asset.isLink) ? "-link" : "-inline");
                switch (key) {
                    case "js-head-link":
                        addonListItem.renderedAssets.headJsLinks.Add(asset.content);
                        break;
                    case "js-head-inline":
                        addonListItem.renderedAssets.headJs.Add(asset.content);
                        break;
                    case "js-body-link":
                        addonListItem.renderedAssets.bodyJsLinks.Add(asset.content);
                        break;
                    case "js-body-inline":
                        addonListItem.renderedAssets.bodyJs.Add(asset.content);
                        break;
                    case "css-head-link":
                    case "css-body-link":
                        addonListItem.renderedAssets.headStylesheetLinks.Add(asset.content);
                        break;
                    case "css-head-inline":
                    case "css-body-inline":
                        addonListItem.renderedAssets.headStyles.Add(asset.content);
                        break;
                }
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// render all the renderHtml, css, js nodes in an addon list
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="addonList"></param>
        public static void renderAddonList( CPBaseClass cp, List<AddonListItemModel> addonList) {
            foreach (var addon in addonList) {
                if (addon.columns != null) {
                    foreach (var column in addon.columns) {
                        renderAddonList(cp, column.addonList);
                    }
                }
                renderAddonListItem(cp, addon);
            }
        }
        //
        //====================================================================================================
        #region  IDisposable Support 
        //
        // this class must implement System.IDisposable
        // never throw an exception in dispose
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        //====================================================================================================
        //
        protected bool disposed = false;
        //
        public void Dispose() {
            // do not add code here. Use the Dispose(disposing) overload
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        //
        ~AddonListController() {
            // do not add code here. Use the Dispose(disposing) overload
            Dispose(false);
        }
        //
        //====================================================================================================
        /// <summary>
        /// dispose.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                this.disposed = true;
                if (disposing) {
                    //If (cacheClient IsNot Nothing) Then
                    //    cacheClient.Dispose()
                    //End If
                }
                //
                // cleanup non-managed objects
                //
            }
        }
        #endregion
    }
    //
}