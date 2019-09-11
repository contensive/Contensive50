
using Contensive.BaseClasses;
using Contensive.Processor.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Contensive.Processor.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// static class controller
    /// </summary>
    public class AddonListController : IDisposable {
        //
        //====================================================================================================
        /// <summary>
        /// Render the html,css,js for an addonListItem
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="addonListItem"></param>
        public static string render(CPBaseClass cp, AddonListItemModel addonListItem) {
            cp.Doc.SetProperty("instanceId", addonListItem.instanceGuid);
            return cp.Addon.Execute(addonListItem.designBlockTypeGuid).ToString();
        }
        //
        //====================================================================================================
        /// <summary>
        /// render all the renderHtml, css, js nodes in an addon list
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="addonList"></param>
        public static string render(CPBaseClass cp, List<AddonListItemModel> addonList) {
            var result = new StringBuilder();
            foreach (var addon in addonList) {
                var addonHtml = render(cp, addon);
                if (addon.columns != null) {
                    int colPtr = 1;
                    foreach (var column in addon.columns) {
                        string replaceTarget = "<!-- column-" + colPtr + " -->";
                        addonHtml = addonHtml.Replace(replaceTarget, render(cp, column.addonList));
                        colPtr++;
                    }
                }
                result.Append(addonHtml);
            }
            return result.ToString();
        }
        //
        //====================================================================================================
        /// <summary>
        /// Render for editing the html,css,js for an addonListItem
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="addonListItem"></param>
        public static void renderEdit(CPBaseClass cp, AddonListItemModel addonListItem) {
            CoreController core = ((CPClass)(cp)).core;
            int assetSkipCnt = core.doc.htmlAssetList.Count;
            addonListItem.renderedHtml = render(cp, addonListItem);
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
        /// render for editing all the renderHtml, css, js nodes in an addon list
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="addonList"></param>
        public static void renderEdit(CPBaseClass cp, List<AddonListItemModel> addonList) {
            foreach (var addon in addonList) {
                if (addon.columns != null) {
                    foreach (var column in addon.columns) {
                        renderEdit(cp, column.addonList);
                    }
                }
                renderEdit(cp, addon);
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// transverse the addonlist and delete the instance specified. return true if found, false if not found
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="addonList"></param>
        /// <param name="instanceGuid"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static bool deleteInstance(CPBaseClass cp, List<AddonListItemModel> addonList, string instanceGuid) {
            foreach (var addon in addonList) {
                if (addon.columns != null) {
                    foreach (var column in addon.columns) {
                        if (deleteInstance(cp, column.addonList, instanceGuid)) { return true; }
                    }
                }
                if (addon.instanceGuid == instanceGuid) {
                    addonList.Remove(addon);
                    return true;
                }
            }
            return false;
        }
        //
        // ==========================================================================================
        /// <summary>
        /// clean up the addonlist delivered from the UI (remove renderedhtml, update addonName, etc)
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="addonList"></param>
        /// <returns></returns>
        public static void normalizeAddonList(CPBaseClass cp, List<AddonListItemModel> addonList) {
            try {
                foreach (var addon in addonList) {
                    addon.renderedHtml = string.Empty;
                    addon.renderedAssets = new AddonAssetsModel();
                    var addonRecord = Contensive.DbBaseModel.create<AddonModel>(((CPClass)cp).core, addon.designBlockTypeGuid);
                    if (addonRecord != null) {
                        addon.designBlockTypeName = addonRecord.name;
                    }
                    if (addon.columns != null) {
                        foreach (var column in addon.columns) {
                            normalizeAddonList(cp, column.addonList);
                        }
                    }
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
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