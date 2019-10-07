
using Contensive.Models.Db;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Contensive.Processor.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// static class controller
    /// </summary>
    public class BuildDataMigrationController : IDisposable {
        //
        //====================================================================================================
        /// <summary>
        /// when breaking changes are required for data, update them here
        /// </summary>
        /// <param name="core"></param>
        /// <param name="DataBuildVersion"></param>
        public static void migrateData(CoreController core, string DataBuildVersion, string logPrefix) {
            try {
                //
                // -- Roll the style sheet cache if it is setup
                core.siteProperties.setProperty("StylesheetSerialNumber", (-1).ToString());
                //
                // -- 4.1 to 5.1 conversions
                if(DataBuildVersion.Substring(0,3)=="4.1") {
                    //
                    // -- remove all addon content fieldtype rules
                    Contensive.Models.Db.DbBaseModel.deleteRows<Contensive.Models.Db.AddonContentFieldTypeRulesModel>(core.cpParent, "");
                    //
                    // -- delete /admin www subfolder
                    core.wwwFiles.deleteFolder("admin");
                    //
                    // -- delete .asp and .php files
                    foreach ( CPFileSystemClass.FileDetail file in core.wwwFiles.getFileList("")) {
                        if ( file == null) { continue; }
                        if ( string.IsNullOrWhiteSpace( file.Name )) { continue;  }
                        if ( file.Name.Length < 4 ) { continue;  }
                        string extension = System.IO.Path.GetExtension(file.Name).ToLower(CultureInfo.InvariantCulture);
                        if ((extension == ".php") || (extension == ".asp")) {
                            core.wwwFiles.deleteFile(file.Name);
                        }
                    }
                    //
                    // -- create www /cclib folder and copy in legacy resources
                    core.programFiles.copyFile("cclib.zip", "cclib.zip", core.wwwFiles);
                    core.wwwFiles.unzipFile("cclib.zip");
                    //
                    // -- remove all the old menu entries and leave the navigation entries
                    var navContent = DbBaseModel.createByUniqueName<ContentModel>(core.cpParent, Contensive.Models.Db.NavigatorEntryModel.tableMetadata.contentName);
                    if (navContent != null) {
                        core.db.executeNonQuery("delete from ccMenuEntries where ((contentcontrolid<>0)and(contentcontrolid<>" + navContent.id + ")and(contentcontrolid is not null))");
                    }
                    //
                    // -- reinstall newest font-awesome collection
                    string returnErrorMessage = "";
                    var context = new Stack<string>();
                    var nonCritialErrorList = new List<string>();
                    var collectionsInstalledList = new List<string>();
                    CollectionLibraryController.installCollectionFromLibrary(core, context, Constants.fontAwesomeCollectionGuid, ref returnErrorMessage, false, true, ref nonCritialErrorList, logPrefix, ref collectionsInstalledList);
                    //
                    // -- reinstall newest redactor collection
                    returnErrorMessage = "";
                    context = new Stack<string>();
                    nonCritialErrorList = new List<string>();
                    collectionsInstalledList = new List<string>();
                    CollectionLibraryController.installCollectionFromLibrary(core, context, Constants.redactorCollectionGuid, ref returnErrorMessage, false, true, ref nonCritialErrorList, logPrefix, ref collectionsInstalledList);
                    //
                    // -- addons with active-x -- remove programid and add script code that logs error
                    string newCode = ""
                        + "function m"
                        + " ' + CHAR(13)+CHAR(10) + ' \ncp.Site.ErrorReport(\"deprecated active-X add-on executed [#\" & cp.addon.id & \", \" & cp.addon.name & \"]\")"
                        + " ' + CHAR(13)+CHAR(10) + ' \nend function"
                        + "";
                    string sql = "update ccaggregatefunctions set help='Legacy activeX: ' + objectprogramId, objectprogramId=null, ScriptingCode='" + newCode + "' where (ObjectProgramID is not null)";
                    LogController.logInfo(core, "MigrateData, removing activex addons, adding exception logging, sql [" + sql + "]");
                    core.db.executeNonQuery(sql);
                    //
                    // -- create page menus from section menus
                    sql = "select m.name as menuName, m.id as menuId, p.name as pageName, p.id as pageId"
                        + " from ccDynamicMenus m"
                        + " left join ccDynamicMenuSectionRules r on r.DynamicMenuID = m.id"
                        + " left join ccSections s on s.id = r.SectionID"
                        + " left join ccPageContent p on p.id = s.RootPageID"
                        + " where p.id is not null";
                    using ( var cs = new CsModel(core) ) {
                        do {
                            string menuName = cs.getText("menuName");
                            if (!string.IsNullOrWhiteSpace(menuName)) {
                                var menu = DbBaseModel.createByUniqueName<MenuModel>(core.cpParent, menuName);
                                if (menu == null) {
                                    menu = DbBaseModel.addEmpty<MenuModel>(core.cpParent);
                                    menu.name = menuName;
                                    menu.save(core.cpParent);
                                }
                                var menuPageRule = DbBaseModel.addEmpty<MenuPageRuleModel>(core.cpParent);
                                if (menuPageRule != null) {
                                    menuPageRule.name = "Created from v4.1 menu sections " + DateTime.Now.ToString();
                                    menuPageRule.pageId = cs.getInteger("pageId");
                                    menuPageRule.menuId = menu.id;
                                    menuPageRule.active = true;
                                    menuPageRule.save(core.cpParent);
                                }
                            }
                            cs.goNext();
                        } while (cs.ok());
                    }
                    //
                    core.cpParent.Cache.InvalidateAll();
                }
                //
                // -- Reload
                core.cache.invalidateAll();
                core.clearMetaData();
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
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
        ~BuildDataMigrationController() {
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