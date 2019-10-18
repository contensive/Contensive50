﻿
using Contensive.Models.Db;
using System;
using System.Collections.Generic;
using System.Data;
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
                CPClass cp = core.cpParent;
                //
                // -- Roll the style sheet cache if it is setup
                core.siteProperties.setProperty("StylesheetSerialNumber", (-1).ToString());
                //
                // -- verify ID is primary key on all tables
                foreach (TableModel table in DbBaseModel.createList<TableModel>(cp)) {
                    if (!string.IsNullOrWhiteSpace(table.name)) {
                        //
                        // -- read table primary key field
                        string sql = "" 
                            + " SELECT Col.Column_Name"
                            + " from INFORMATION_SCHEMA.TABLE_CONSTRAINTS Tab, INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE Col"
                            + " WHERE (Col.Constraint_Name = Tab.Constraint_Name) AND (Col.Table_Name = Tab.Table_Name) AND (Constraint_Type = 'PRIMARY KEY') AND (Col.Table_Name = '" + table.name + "')";
                        bool idFound = false;
                        foreach ( DataRow dr in core.db.executeQuery(sql).Rows ) {
                            if ( GenericController.encodeText( dr["Column_Name"]).ToLower().Equals("id") ) {
                                idFound = true;
                                break;
                            }
                        }
                        if(!idFound) {
                            core.db.executeNonQuery("alter table " + table.name + " add primary key (ID)");
                        }
                    }
                }
                //
                // -- 4.1 to 5.1 conversions
                if (DataBuildVersion.Substring(0, 3) == "4.1") {
                    //
                    // -- create Data Migration Assets collection
                    var migrationCollection = DbBaseModel.createByUniqueName<AddonCollectionModel>(cp, "Data Migration Assets");
                    if (migrationCollection == null) {
                        migrationCollection = DbBaseModel.addDefault<AddonCollectionModel>(cp);
                        migrationCollection.name = "Data Migration Assets";
                    }
                    //
                    // -- remove all addon content fieldtype rules
                    Contensive.Models.Db.DbBaseModel.deleteRows<Contensive.Models.Db.AddonContentFieldTypeRulesModel>(cp, "(1=1)");
                    //
                    // -- delete /admin www subfolder
                    core.wwwFiles.deleteFolder("admin");
                    //
                    // -- delete .asp and .php files
                    foreach (CPFileSystemClass.FileDetail file in core.wwwFiles.getFileList("")) {
                        if (file == null) { continue; }
                        if (string.IsNullOrWhiteSpace(file.Name)) { continue; }
                        if (file.Name.Length < 4) { continue; }
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
                    var navContent = DbBaseModel.createByUniqueName<ContentModel>(cp, Contensive.Models.Db.NavigatorEntryModel.tableMetadata.contentName);
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
                    using (var cs = new CsModel(core)) {
                        sql = "select m.name as menuName, m.id as menuId, p.name as pageName, p.id as pageId, s.name as sectionName, m.*"
                            + " from ccDynamicMenus m"
                            + " left join ccDynamicMenuSectionRules r on r.DynamicMenuID = m.id"
                            + " left join ccSections s on s.id = r.SectionID"
                            + " left join ccPageContent p on p.id = s.RootPageID"
                            + " where (p.id is not null)and(s.active>0)"
                            + " order by m.id, s.sortorder,s.id";
                        if (cs.openSql(sql)) {
                            int sortOrder = 0;
                            do {
                                string menuName = cs.getText("menuName");
                                if (!string.IsNullOrWhiteSpace(menuName)) {
                                    var menu = DbBaseModel.createByUniqueName<MenuModel>(cp, menuName);
                                    if (menu == null) {
                                        menu = DbBaseModel.addEmpty<MenuModel>(cp);
                                        menu.name = menuName;
                                        try {
                                            menu.classItemActive = cs.getText("classItemActive");
                                            menu.classItemFirst = cs.getText("classItemFirst");
                                            menu.classItemHover = cs.getText("classItemHover");
                                            menu.classItemLast = cs.getText("classItemLast");
                                            menu.classTierAnchor = cs.getText("classTierItem");
                                            menu.classTierItem = cs.getText("classTierItem");
                                            menu.classTierList = cs.getText("classTierList");
                                            menu.classTopAnchor = cs.getText("classTopItem");
                                            menu.classTopItem = cs.getText("classTopItem");
                                            menu.classTopList = cs.getText("classTopList");
                                            //menu.classTopParentAnchor = cs.getText("classTopParentAnchor");
                                            //menu.classTopParentItem = cs.getText("classTopParentItem");
                                            menu.classTopWrapper = cs.getText("classTopWrapper");
                                        } catch (Exception ex) {
                                            LogController.logError(core, ex, "migrateData error populating menu from dynamic menu.");
                                        }
                                        menu.save(cp);
                                    }
                                    //
                                    // -- set the root page's menuHeadline to the section name
                                    var page = DbBaseModel.create<PageContentModel>(cp, cs.getInteger("pageId"));
                                    if (page != null) {
                                        page.menuHeadline = cs.getText("sectionName");
                                        page.save(cp);
                                        //
                                        // -- create a menu-page rule to attach this page to the menu in the current order
                                        var menuPageRule = DbBaseModel.addEmpty<MenuPageRuleModel>(cp);
                                        if (menuPageRule != null) {
                                            menuPageRule.name = "Created from v4.1 menu sections " + DateTime.Now.ToString();
                                            menuPageRule.pageId = page.id;
                                            menuPageRule.menuId = menu.id;
                                            menuPageRule.active = true;
                                            menuPageRule.sortOrder = sortOrder.ToString().PadLeft(4, '0');
                                            menuPageRule.save(cp);
                                            sortOrder += 10;
                                        }
                                    }
                                }
                                cs.goNext();
                            } while (cs.ok());
                        }
                    }
                    //
                    // -- create a theme addon for each template for styles and meta content
                    using (var csTemplate = cp.CSNew()) {
                        if (csTemplate.Open("page templates")) {
                            do {
                                int templateId = csTemplate.GetInteger("id");
                                string templateStylePrepend = "";
                                string templateStyles = csTemplate.GetText("StylesFilename");
                                //
                                // -- add shared styles to the template stylesheet
                                using (var csStyleRule = cp.CSNew()) {
                                    if (csStyleRule.Open("shared styles template rules", "(TemplateID=" + templateId + ")")) {
                                        do {
                                            int sharedStyleId = csStyleRule.GetInteger("styleid");
                                            using (var csStyle = cp.CSNew()) {
                                                if (csStyleRule.Open("shared styles", "(id=" + sharedStyleId + ")")) {
                                                    //
                                                    // -- prepend lines beginning with @ t
                                                    string styles = csStyleRule.GetText("StyleFilename");
                                                    if (!string.IsNullOrWhiteSpace(styles)) {
                                                        //
                                                        // -- trim off leading spaces, newlines, comments
                                                        styles = styles.Trim();
                                                        while (!string.IsNullOrWhiteSpace(styles) && styles.Substring(0, 1).Equals("@")) {
                                                            if (styles.IndexOf(Environment.NewLine) >= 0) {
                                                                templateStylePrepend += styles.Substring(0, styles.IndexOf(Environment.NewLine));
                                                                styles = styles.Substring(styles.IndexOf(Environment.NewLine) + 1).Trim();
                                                            } else {
                                                                templateStylePrepend += styles;
                                                                styles = string.Empty;
                                                            }
                                                        };
                                                        templateStyles += Environment.NewLine + styles;
                                                    }
                                                }
                                            }
                                            csStyleRule.GoNext();
                                        } while (csStyleRule.OK());
                                    }
                                }
                                // 
                                // -- create an addon
                                var themeAddon = DbBaseModel.addDefault<AddonModel>(cp);
                                themeAddon.name = "Theme assets for template " + csTemplate.GetText("name");
                                themeAddon.otherHeadTags = csTemplate.GetText("otherheadtags");
                                themeAddon.javaScriptBodyEnd = csTemplate.GetText("jsendbody");
                                themeAddon.stylesFilename.content = templateStylePrepend + Environment.NewLine + templateStyles;
                                themeAddon.collectionID = migrationCollection.id;
                                themeAddon.save(cp);
                                // 
                                // -- create an addon template rule to set dependency
                                var rule = DbBaseModel.addEmpty<AddonTemplateRuleModel>(cp);
                                rule.addonId = themeAddon.id;
                                rule.templateId = templateId;
                                rule.save(cp);
                                //
                                csTemplate.GoNext();
                            } while (csTemplate.OK());

                        }
                    }
                    //
                    // -- reset the html minify so it is easier to resolve other issues
                    core.siteProperties.setProperty("ALLOW HTML MINIFY", false);
                    //
                    // -- remove contentcategoryid from all edit page
                    cp.Db.ExecuteNonQuery("update ccfields set Authorable=0 where name='contentcategoryid'");
                    cp.Db.ExecuteNonQuery("update ccfields set Authorable=0 where name='editsourceid'");
                    cp.Db.ExecuteNonQuery("update ccfields set Authorable=0 where name='editarchive'");
                    cp.Db.ExecuteNonQuery("update ccfields set Authorable=0 where name='editblank'");
                    //
                    // -- remove legacy workflow fields
                    UpgradeController.dropLegacyWorkflowField(core, "editsourceid");
                    cp.Db.ExecuteNonQuery("delete from ccfields where name='editsourceid'");
                    //
                    UpgradeController.dropLegacyWorkflowField(core, "editblank");
                    cp.Db.ExecuteNonQuery("delete from ccfields where name='editblank'");
                    //
                    UpgradeController.dropLegacyWorkflowField(core, "editarchive");
                    cp.Db.ExecuteNonQuery("delete from ccfields where name='editarchive'");
                    //
                    UpgradeController.dropLegacyWorkflowField(core, "contentcategoryid");
                    cp.Db.ExecuteNonQuery("delete from ccfields where name='contentcategoryid'");
                    //
                    //
                    // -- end of 4.1 to 5 conversion
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