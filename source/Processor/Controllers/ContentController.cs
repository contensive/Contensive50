
using Contensive.BaseClasses;
using Contensive.Models.Db;
using Contensive.Processor.Models.Domain;
using System;
using System.Collections.Generic;

namespace Contensive.Processor.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// static class controller
    /// </summary>
    public class ContentController : IDisposable {
        //====================================================================================================
        /// <summary>
        /// Process manual changes needed for special cases
        /// </summary>
        /// <param name="isDelete"></param>
        /// <param name="contentName"></param>
        /// <param name="recordID"></param>
        /// <param name="recordName"></param>
        /// <param name="recordParentID"></param>
        /// <param name="useContentWatchLink"></param>
        public static void processAfterSave(CoreController core, bool isDelete, string contentName, int recordID, string recordName, int recordParentID, bool useContentWatchLink) {
            try {
                int contentID = ContentMetadataModel.getContentId(core, contentName);
                string tableName = MetadataController.getContentTablename(core, contentName);
                PageContentModel.markReviewed(core.cpParent, recordID);
                //
                // -- invalidate the specific cache for this record
                core.cache.invalidateDbRecord(recordID, tableName);
                int activityLogOrganizationID = 0;
                //
                switch (tableName.ToLower()) {
                    case AddonCollectionModel.contentTableNameLowerCase:
                        //
                        // -- if this is an add or delete, manage the collection folders
                        if (isDelete) {
                            //
                            // todo - if a collection is deleted, consider deleting the collection folder (or saving as archive)
                        } else {
                            var addonCollection = AddonCollectionModel.create<AddonCollectionModel>(core.cpParent, recordID);
                            if (addonCollection != null) {
                                string CollectionVersionFolderName = CollectionFolderController.verifyCollectionVersionFolderName(core, addonCollection.ccguid, addonCollection.name);
                                if (string.IsNullOrEmpty(CollectionVersionFolderName)) {
                                    //
                                    // -- new collection
                                    string CollectionVersionFolder = core.addon.getPrivateFilesAddonPath() + CollectionVersionFolderName;
                                    core.privateFiles.createPath(CollectionVersionFolder);
                                    CollectionFolderController.updateCollectionFolderConfig(core, addonCollection.name, addonCollection.ccguid, DateTime.Now, CollectionVersionFolderName);
                                }

                            }
                        }
                        break;
                    case LinkForwardModel.contentTableNameLowerCase:
                        //
                        core.routeMapCacheClear();
                        break;
                    case LinkAliasModel.contentTableNameLowerCase:
                        //
                        core.routeMapCacheClear();
                        break;
                    case AddonModel.contentTableNameLowerCase:
                        //
                        core.routeMapCacheClear();
                        core.cache.invalidateDbRecord(recordID, tableName);
                        break;
                    case PersonModel.contentTableNameLowerCase:
                        //
                        using (var csData = new CsModel(core)) {
                            csData.openRecord("people", recordID, "Name,OrganizationID");
                            if (csData.ok()) {
                                activityLogOrganizationID = csData.getInteger("OrganizationID");
                            }
                        }
                        if (isDelete) {
                            LogController.addSiteActivity(core, "deleting user #" + recordID + " (" + recordName + ")", recordID, activityLogOrganizationID);
                        } else {
                            LogController.addSiteActivity(core, "saving changes to user #" + recordID + " (" + recordName + ")", recordID, activityLogOrganizationID);
                        }
                        break;
                    case OrganizationModel.contentTableNameLowerCase:
                        //
                        // Log Activity for changes to people and organizattions
                        //
                        //hint = hint & ",120"
                        if (isDelete) {
                            LogController.addSiteActivity(core, "deleting organization #" + recordID + " (" + recordName + ")", 0, recordID);
                        } else {
                            LogController.addSiteActivity(core, "saving changes to organization #" + recordID + " (" + recordName + ")", 0, recordID);
                        }
                        break;
                    case SitePropertyModel.contentTableNameLowerCase:
                        //
                        // Site Properties
                        //
                        //hint = hint & ",130"
                        switch (GenericController.vbLCase(recordName)) {
                            case "allowlinkalias":
                                PageContentModel.invalidateCacheOfTable<PageContentModel>(core.cpParent);
                                break;
                            case "sectionlandinglink":
                                PageContentModel.invalidateCacheOfTable<PageContentModel>(core.cpParent);
                                break;
                            case Constants._siteproperty_serverPageDefault_name:
                                PageContentModel.invalidateCacheOfTable<PageContentModel>(core.cpParent);
                                break;
                        }
                        break;
                    case PageContentModel.contentTableNameLowerCase:
                        //
                        // set ChildPagesFound true for parent page
                        //
                        //hint = hint & ",140"
                        if (recordParentID > 0) {
                            if (!isDelete) {
                                core.db.executeQuery("update ccpagecontent set ChildPagesfound=1 where ID=" + recordParentID);
                            }
                        }
                        //
                        // Page Content special cases for delete
                        //
                        if (isDelete) {
                            //
                            // Clear the Landing page and page not found site properties
                            //
                            if (recordID == GenericController.encodeInteger(core.siteProperties.getText("PageNotFoundPageID", "0"))) {
                                core.siteProperties.setProperty("PageNotFoundPageID", "0");
                            }
                            if (recordID == core.siteProperties.landingPageID) {
                                core.siteProperties.setProperty("landingPageId", "0");
                            }
                            //
                            // Delete Link Alias entries with this PageID
                            //
                            core.db.executeQuery("delete from cclinkAliases where PageID=" + recordID);
                        }
                        DbBaseModel.invalidateCacheOfRecord<PageContentModel>(core.cpParent, recordID);
                        break;
                    case LibraryFilesModel.contentTableNameLowerCase:
                        //
                        // if a AltSizeList is blank, make large,medium,small and thumbnails
                        //
                        //hint = hint & ",180"
                        if (core.siteProperties.getBoolean("ImageAllowSFResize", true) && (!isDelete)) {
                            using (var csData = new CsModel(core)) {
                                if (csData.openRecord("library files", recordID)) {
                                    string Filename = csData.getText("filename");
                                    int Pos = Filename.LastIndexOf("/") + 1;
                                    string FilePath = "";
                                    if (Pos > 0) {
                                        FilePath = Filename.Left(Pos);
                                        Filename = Filename.Substring(Pos);
                                    }
                                    csData.set("filesize", core.wwwFiles.getFileSize(FilePath + Filename));
                                    Pos = Filename.LastIndexOf(".") + 1;
                                    if (Pos > 0) {
                                        string FilenameExt = Filename.Substring(Pos);
                                        string FilenameNoExt = Filename.Left(Pos - 1);
                                        if (GenericController.vbInstr(1, "jpg,gif,png", FilenameExt, 1) != 0) {
                                            ImageEditController sf = new ImageEditController();
                                            if (sf.load(FilePath + Filename, core.wwwFiles)) {
                                                //
                                                //
                                                //
                                                csData.set("height", sf.height);
                                                csData.set("width", sf.width);
                                                string AltSizeList = csData.getText("AltSizeList");
                                                bool RebuildSizes = (string.IsNullOrEmpty(AltSizeList));
                                                if (RebuildSizes) {
                                                    AltSizeList = "";
                                                    //
                                                    // Attempt to make 640x
                                                    //
                                                    if (sf.width >= 640) {
                                                        sf.height = GenericController.encodeInteger(sf.height * (640 / sf.width));
                                                        sf.width = 640;
                                                        sf.save(FilePath + FilenameNoExt + "-640x" + sf.height + "." + FilenameExt, core.wwwFiles);
                                                        AltSizeList = AltSizeList + Environment.NewLine + "640x" + sf.height;
                                                    }
                                                    //
                                                    // Attempt to make 320x
                                                    //
                                                    if (sf.width >= 320) {
                                                        sf.height = GenericController.encodeInteger(sf.height * (320 / sf.width));
                                                        sf.width = 320;
                                                        sf.save(FilePath + FilenameNoExt + "-320x" + sf.height + "." + FilenameExt, core.wwwFiles);

                                                        AltSizeList = AltSizeList + Environment.NewLine + "320x" + sf.height;
                                                    }
                                                    //
                                                    // Attempt to make 160x
                                                    //
                                                    if (sf.width >= 160) {
                                                        sf.height = GenericController.encodeInteger(sf.height * (160 / sf.width));
                                                        sf.width = 160;
                                                        sf.save(FilePath + FilenameNoExt + "-160x" + sf.height + "." + FilenameExt, core.wwwFiles);
                                                        AltSizeList = AltSizeList + Environment.NewLine + "160x" + sf.height;
                                                    }
                                                    //
                                                    // Attempt to make 80x
                                                    //
                                                    if (sf.width >= 80) {
                                                        sf.height = GenericController.encodeInteger(sf.height * (80 / sf.width));
                                                        sf.width = 80;
                                                        sf.save(FilePath + FilenameNoExt + "-180x" + sf.height + "." + FilenameExt, core.wwwFiles);
                                                        AltSizeList = AltSizeList + Environment.NewLine + "80x" + sf.height;
                                                    }
                                                    csData.set("AltSizeList", AltSizeList);
                                                }
                                                sf.Dispose();
                                                sf = null;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
                //
                // Process Addons marked to trigger a process call on content change
                //
                Dictionary<string, string> instanceArguments;
                bool onChangeAddonsAsync = core.siteProperties.getBoolean("execute oncontentchange addons async", false);
                using (var csData = new CsModel(core)) {
                    csData.open("Add-on Content Trigger Rules", "ContentID=" + contentID, "", false, 0, "addonid");
                    string Option_String = null;
                    if (isDelete) {
                        instanceArguments = new Dictionary<string, string>() {
                            {"action","contentdelete"},
                            {"contentid",contentID.ToString()},
                            {"recordid",recordID.ToString()}
                        };
                        Option_String = ""
                            + Environment.NewLine + "action=contentdelete"
                            + Environment.NewLine + "contentid=" + contentID
                            + Environment.NewLine + "recordid=" + recordID + "";
                    } else {
                        instanceArguments = new Dictionary<string, string>() {
                            {"action","contentchange"},
                            {"contentid",contentID.ToString()},
                            {"recordid",recordID.ToString()}
                        };
                        Option_String = ""
                            + Environment.NewLine + "action=contentchange"
                            + Environment.NewLine + "contentid=" + contentID
                            + Environment.NewLine + "recordid=" + recordID + "";
                    }
                    while (csData.ok()) {
                        var addon = DbBaseModel.create<AddonModel>(core.cpParent, csData.getInteger("Addonid"));
                        if (addon != null) {
                            if (onChangeAddonsAsync) {
                                //
                                // -- execute addon async
                                core.addon.executeAsync(addon, instanceArguments);
                            } else {
                                //
                                // -- execute addon
                                core.addon.execute(addon, new CPUtilsBaseClass.addonExecuteContext() {
                                    addonType = CPUtilsBaseClass.addonContext.ContextOnContentChange,
                                    backgroundProcess = false,
                                    errorContextMessage = "",
                                    argumentKeyValuePairs = instanceArguments
                                });
                            }
                        }
                        csData.goNext();
                    }
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
        }
        //  
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
        ~ContentController() {
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