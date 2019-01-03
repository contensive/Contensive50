
using System;
using System.Xml;
using System.Collections.Generic;
using Contensive.Processor.Models.Db;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using System.Data;
using System.Linq;
using Contensive.Processor.Models.Domain;
using Contensive.Processor.Exceptions;

namespace Contensive.Processor.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// code to built and upgrade apps
    /// not IDisposable - not contained classes that need to be disposed
    /// </summary>
    public class AppBuilderController {
        //
        //====================================================================================================
        /// <summary>
        /// verify the current database matches the requirements for this build. Manager SiteProperty("BuildVersion")
        /// </summary>
        /// <param name="core"></param>
        /// <param name="isNewBuild"></param>
        /// <param name="repair"></param>
        public static void upgrade(CoreController core, bool isNewBuild, bool repair) {
            try {
                //
                LogController.logInfo(core, "AppBuilderController.upgrade, app [" + core.appConfig.name + "], repair [" + repair.ToString() + "]");
                string logPrefix = "***** upgrade[" + core.appConfig.name + "]";
                var installedCollections = new List<string>();
                //
                {
                    string DataBuildVersion = core.siteProperties.dataBuildVersion;
                    List<string> nonCriticalErrorList = new List<string>();
                    //
                    // -- determine primary domain
                    string primaryDomain = core.appConfig.name;
                    if (core.appConfig.domainList.Count > 0) {
                        primaryDomain = core.appConfig.domainList[0];
                    }
                    //
                    // -- Verify core table fields (DataSources, Content Tables, Content, Content Fields, Setup, Sort Methods), then other basic system ops work, like site properties
                    verifyBasicTables(core, logPrefix);
                    //
                    // 20180217 - move this before base collection because during install it runs addons (like _oninstall)
                    // if anything is needed that is not there yet, I need to build a list of adds to run after the app goes to app status ok
                    // -- Update server config file
                    LogController.logInfo(core, logPrefix + ", update configuration file");
                    if (!core.appConfig.appStatus.Equals(AppConfigModel.AppStatusEnum.ok)) {
                        core.appConfig.appStatus = AppConfigModel.AppStatusEnum.ok;
                        core.serverConfig.saveObject(core);
                    }
                    //
                    // verify current database meets minimum field requirements (before installing base collection)
                    LogController.logInfo(core, logPrefix + ", verify existing database fields meet requirements");
                    verifySqlfieldCompatibility(core,  logPrefix);
                    //
                    // -- verify base collection
                    LogController.logInfo(core, logPrefix + ", install base collection");
                    CollectionController.installBaseCollection(core, isNewBuild, repair, ref  nonCriticalErrorList, logPrefix, ref installedCollections);
                    //
                    // -- verify iis configuration
                    LogController.logInfo(core, logPrefix + ", verify iis configuration");
                    Controllers.WebServerController.verifySite(core, core.appConfig.name, primaryDomain, core.appConfig.localWwwPath, "default.aspx");
                    //
                    // -- verify root developer
                    LogController.logInfo(core, logPrefix + ", verify developer user");
                    var root = PersonModel.create(core, defaultRootUserGuid);
                    if (root == null) {
                        LogController.logInfo(core, logPrefix + ", root user guid not found, test for root username");
                        var rootList = PersonModel.createList(core, "(username='root')");
                        if ( rootList.Count > 0 ) {
                            LogController.logInfo(core, logPrefix + ", root username found");
                            root = rootList.First();
                        }
                    }
                    if ( root == null ) {
                        LogController.logInfo(core, logPrefix + ", root user not found, adding root/contensive");
                        root = PersonModel.addEmpty(core);
                        root.name = defaultRootUserName;
                        root.firstName = defaultRootUserName;
                        root.username = defaultRootUserUsername;
                        root.password = defaultRootUserPassword;
                        root.developer = true;
                        root.contentControlID = MetaModel.getContentId(core, "people");
                        try 
                            {
                            root.save(core);
                        } catch (Exception) {
                            LogController.logInfo(core, logPrefix + ", error prevented root user update");
                        }
                    }
                    //
                    // -- verify site managers group
                    LogController.logInfo(core, logPrefix + ", verify site managers groups");
                    var group = GroupModel.create(core, defaultSiteManagerGuid);
                    if (group == null) {
                        LogController.logInfo(core, logPrefix + ", verify site manager group");
                        group = GroupModel.addEmpty(core);
                        group.name = defaultSiteManagerName;
                        group.caption = defaultSiteManagerName;
                        group.allowBulkEmail = true;
                        group.ccguid = defaultSiteManagerGuid;
                        try {
                            group.save(core);
                        } catch (Exception) {
                            LogController.logInfo(core, logPrefix + ", error creating site managers group");
                        }
                    }
                    if ((root != null) && (group != null)) {
                        //
                        // -- verify root is in site managers
                        var memberRuleList = MemberRuleModel.createList(core, "(groupid=" + group.id.ToString() + ")and(MemberID=" + root.id.ToString() + ")");
                        if (memberRuleList.Count == 0) {
                            var memberRule = MemberRuleModel.addEmpty(core);
                            memberRule.groupId = group.id;
                            memberRule.MemberID = root.id;
                            memberRule.save(core);
                        }
                    }           
                    //
                    //---------------------------------------------------------------------
                    // ----- Convert Database fields for new Db
                    //---------------------------------------------------------------------
                    //
                    if (isNewBuild) {
                        //
                        // -- set build version so a scratch build will not go through data conversion
                        DataBuildVersion = core.codeVersion();
                        core.siteProperties.dataBuildVersion = core.codeVersion();
                    }
                    //
                    //---------------------------------------------------------------------
                    // ----- Upgrade Database fields if not new
                    //---------------------------------------------------------------------
                    //
                    if (string.CompareOrdinal(DataBuildVersion, core.codeVersion()) < 0) {
                        //
                        // -- data updates
                        LogController.logInfo(core, logPrefix + ", run database conversions, DataBuildVersion [" + DataBuildVersion + "], software version [" + core.codeVersion() + "]");
                        Upgrade_Conversion(core, DataBuildVersion, logPrefix);
                    }
                    //
                    //---------------------------------------------------------------------
                    // ----- Verify content needed internally
                    //---------------------------------------------------------------------
                    //
                    LogController.logInfo(core, logPrefix + ", verify records required");
                    //
                    //  menus are created in ccBase.xml, this just checks for dups
                    verifyAdminMenus(core, DataBuildVersion);
                    verifyLanguageRecords(core);
                    verifyCountries(core);
                    verifyStates(core);
                    verifyLibraryFolders(core);
                    verifyLibraryFileTypes(core);
                    verifyDefaultGroups(core);
                    //
                    //---------------------------------------------------------------------
                    // ----- Set Default SitePropertyDefaults
                    //       must be after upgrade_conversion
                    //---------------------------------------------------------------------
                    //
                    LogController.logInfo(core, logPrefix + ", verify Site Properties");
                    if (repair) {
                        //
                        // -- repair, set values to what the default system uses
                        core.siteProperties.setProperty(siteproperty_serverPageDefault_name, siteproperty_serverPageDefault_defaultValue);
                        core.siteProperties.setProperty("AdminURL", "/" + core.appConfig.adminRoute);
                    }
                    //
                    // todo remove site properties not used, put all in preferences
                    //core.siteProperties.getText("AllowAutoHomeSectionOnce", genericController.encodeText(isNewBuild));
                    core.siteProperties.getText("AllowAutoLogin", "False");
                    core.siteProperties.getText("AllowBake", "True");
                    core.siteProperties.getText("AllowChildMenuHeadline", "True");
                    core.siteProperties.getText("AllowContentAutoLoad", "True");
                    core.siteProperties.getText("AllowContentSpider", "False");
                    core.siteProperties.getText("AllowContentWatchLinkUpdate", "True");
                    core.siteProperties.getText("AllowDuplicateUsernames", "False");
                    core.siteProperties.getText("ConvertContentText2HTML", "False");
                    core.siteProperties.getText("AllowMemberJoin", "False");
                    core.siteProperties.getText("AllowPasswordEmail", "True");
                    //core.siteProperties.getText("AllowPathBlocking", "True");
                    //core.siteProperties.getText("AllowPopupErrors", "True");
                    core.siteProperties.getText("AllowTestPointLogging", "False");
                    core.siteProperties.getText("AllowTestPointPrinting", "False");
                    core.siteProperties.getText("AllowTrapEmail", "True");
                    core.siteProperties.getText("AllowTrapLog", "True");
                    core.siteProperties.getText("AllowWorkflowAuthoring", "False");
                    core.siteProperties.getText("ArchiveAllowFileClean", "False");
                    core.siteProperties.getText("ArchiveRecordAgeDays", "90");
                    core.siteProperties.getText("ArchiveTimeOfDay", "2:00:00 AM");
                    core.siteProperties.getText("BreadCrumbDelimiter", "&nbsp;&gt;&nbsp;");
                    core.siteProperties.getText("CalendarYearLimit", "1");
                    core.siteProperties.getText("ContentPageCompatibility21", "false");
                    core.siteProperties.getText("DefaultFormInputHTMLHeight", "500");
                    core.siteProperties.getText("DefaultFormInputTextHeight", "1");
                    core.siteProperties.getText("DefaultFormInputWidth", "60");
                    core.siteProperties.getText("EditLockTimeout", "5");
                    core.siteProperties.getText("EmailAdmin", "webmaster@" + core.appConfig.domainList[0]);
                    core.siteProperties.getText("EmailFromAddress", "webmaster@" + core.appConfig.domainList[0]);
                    core.siteProperties.getText("EmailPublishSubmitFrom", "webmaster@" + core.appConfig.domainList[0]);
                    core.siteProperties.getText("Language", "English");
                    core.siteProperties.getText("PageContentMessageFooter", "Copyright " + core.appConfig.domainList[0]);
                    core.siteProperties.getText("SelectFieldLimit", "4000");
                    core.siteProperties.getText("SelectFieldWidthLimit", "100");
                    core.siteProperties.getText("SMTPServer", "127.0.0.1");
                    core.siteProperties.getText("TextSearchEndTag", "<!-- TextSearchEnd -->");
                    core.siteProperties.getText("TextSearchStartTag", "<!-- TextSearchStart -->");
                    core.siteProperties.getText("TrapEmail", "");
                    core.siteProperties.getText("TrapErrors", "0");
                    AddonModel defaultRouteAddon = AddonModel.create(core, core.siteProperties.defaultRouteId);
                    if (defaultRouteAddon == null) {
                        defaultRouteAddon = AddonModel.create(core, addonGuidPageManager);
                        if (defaultRouteAddon != null) {
                            core.siteProperties.defaultRouteId = defaultRouteAddon.id;
                        }
                    }
                    //
                    //---------------------------------------------------------------------
                    // ----- Changes that effect the web server or content files, not the Database
                    //---------------------------------------------------------------------
                    //
                    int StyleSN = (core.siteProperties.getInteger("StylesheetSerialNumber"));
                    if (StyleSN > 0) {
                        StyleSN += 1;
                        core.siteProperties.setProperty("StylesheetSerialNumber", StyleSN.ToString());
                        // too lazy
                        //Call core.app.publicFiles.SaveFile(core.app.genericController.convertCdnUrlToCdnPathFilename("templates\Public" & StyleSN & ".css"), core.app.csv_getStyleSheetProcessed)
                        //Call core.app.publicFiles.SaveFile(core.app.genericController.convertCdnUrlToCdnPathFilename("templates\Admin" & StyleSN & ".css", core.app.csv_getStyleSheetDefault)
                    }
                    //
                    // clear all cache
                    //
                    core.cache.invalidateAll();
                    //
                    if (isNewBuild) {
                        //
                        // -- primary domain
                        DomainModel domain = DomainModel.createByUniqueName(core, primaryDomain);
                        if (domain == null) {
                            domain = DomainModel.addEmpty(core);
                            domain.name = primaryDomain;
                        }
                        //
                        // -- Landing Page
                        PageContentModel landingPage = PageContentModel.create(core, DefaultLandingPageGuid);
                        if (landingPage == null) {
                            landingPage = PageContentModel.addEmpty(core);
                            landingPage.name = "Home";
                            landingPage.ccguid = DefaultLandingPageGuid;
                        }
                        //
                        // -- default template
                        PageTemplateModel defaultTemplate = PageTemplateModel.createByUniqueName(core, "Default");
                        if (defaultTemplate == null) {
                            defaultTemplate = PageTemplateModel.addEmpty(core);
                            defaultTemplate.name = "Default";
                        }
                        defaultTemplate.bodyHTML = Properties.Resources.DefaultTemplateHtml;
                        defaultTemplate.save(core);
                        //
                        domain.defaultTemplateId = defaultTemplate.id;
                        domain.name = primaryDomain;
                        domain.pageNotFoundPageId = landingPage.id;
                        domain.rootPageId = landingPage.id;
                        domain.typeId = (int) DomainModel.DomainTypeEnum.Normal;
                        domain.visited = false;
                        domain.save(core);
                        //
                        landingPage.TemplateID = defaultTemplate.id;
                        landingPage.copyfilename.content = Constants.defaultLandingPageHtml;
                        landingPage.save(core);
                        //
                        if (core.siteProperties.getInteger("LandingPageID", landingPage.id) == 0) {
                            core.siteProperties.setProperty("LandingPageID", landingPage.id);
                        }
                    }
                    //
                    //---------------------------------------------------------------------
                    // ----- internal upgrade complete
                    //---------------------------------------------------------------------
                    //
                    {
                        LogController.logInfo(core, logPrefix + ", internal upgrade complete, set Buildversion to " + core.codeVersion());
                        core.siteProperties.setProperty("BuildVersion", core.codeVersion());
                        //
                        //---------------------------------------------------------------------
                        // ----- Upgrade local collections
                        //       This would happen if this is the first site to upgrade after a new build is installed
                        //       (can not be in startup because new addons might fail with DbVersions)
                        //       This means a dataupgrade is required with a new build - You can expect errors
                        //---------------------------------------------------------------------
                        //
                        {
                            //
                            // 4.1.575 - 8/28 - put this code behind the DbOnly check, makes DbOnly beuild MUCH faster
                            //
                            string ErrorMessage = "";
                            LogController.logInfo(core, logPrefix + ", upgrading All Local Collections to new server build.");
                            bool UpgradeOK = CollectionController.upgradeLocalCollectionRepoFromRemoteCollectionRepo(core, ref ErrorMessage, isNewBuild, repair, ref  nonCriticalErrorList, logPrefix, ref installedCollections);
                            if (!string.IsNullOrEmpty(ErrorMessage)) {
                                throw (new GenericException("Unexpected exception")); //core.handleLegacyError3(core.appConfig.name, "During UpgradeAllLocalCollectionsFromLib3 call, " & ErrorMessage, "dll", "builderClass", "Upgrade2", 0, "", "", False, True, "")
                            } else if (!UpgradeOK) {
                                throw (new GenericException("Unexpected exception")); //core.handleLegacyError3(core.appConfig.name, "During UpgradeAllLocalCollectionsFromLib3 call, NotOK was returned without an error message", "dll", "builderClass", "Upgrade2", 0, "", "", False, True, "")
                            }
                            //
                            //---------------------------------------------------------------------
                            // ----- Upgrade all collection for this app (in case collections were installed before the upgrade
                            //---------------------------------------------------------------------
                            //
                            string Collectionname = null;
                            string CollectionGuid = null;
                            bool localCollectionFound = false;
                            LogController.logInfo(core, logPrefix + ", Checking all installed collections for upgrades from Collection Library...Open collectons.xml");
                            try {
                                XmlDocument Doc = new XmlDocument();
                                Doc.LoadXml(CollectionController.getLocalCollectionStoreListXml(core));
                                if (true) {
                                    if (GenericController.vbLCase(Doc.DocumentElement.Name) != GenericController.vbLCase(CollectionListRootNode)) {
                                        throw (new GenericException("Unexpected exception")); //core.handleLegacyError3(core.appConfig.name, "Error loading Collection config file. The Collections.xml file has an invalid root node, [" & Doc.DocumentElement.Name & "] was received and [" & CollectionListRootNode & "] was expected.", "dll", "builderClass", "Upgrade", 0, "", "", False, True, "")
                                    } else {
                                        if (GenericController.vbLCase(Doc.DocumentElement.Name) == "collectionlist") {
                                            //
                                            // now go through each collection in this app and check the last updated agains the one here
                                            //
                                            LogController.logInfo(core, logPrefix + ", Open site collectons, iterate through all collections");
                                            //Dim dt As DataTable
                                            DataTable dt = core.db.executeQuery("select * from ccaddoncollections where (ccguid is not null)and(updatable<>0)");
                                            if (dt.Rows.Count > 0) {
                                                int rowptr = 0;
                                                for (rowptr = 0; rowptr < dt.Rows.Count; rowptr++) {

                                                    ErrorMessage = "";
                                                    CollectionGuid = GenericController.vbLCase(dt.Rows[rowptr]["ccguid"].ToString());
                                                    Collectionname = dt.Rows[rowptr]["name"].ToString();
                                                    LogController.logInfo(core, logPrefix + ", checking collection [" + Collectionname + "], guid [" + CollectionGuid + "]");
                                                    if (CollectionGuid != "{7c6601a7-9d52-40a3-9570-774d0d43d758}") {
                                                        //
                                                        // upgrade all except base collection from the local collections
                                                        //
                                                        localCollectionFound = false;
                                                        bool upgradeCollection = false;
                                                        DateTime LastChangeDate = GenericController.encodeDate(dt.Rows[rowptr]["LastChangeDate"]);
                                                        if (LastChangeDate == DateTime.MinValue) {
                                                            //
                                                            // app version has no lastchangedate
                                                            //
                                                            upgradeCollection = true;
                                                            appendUpgradeLog(core, core.appConfig.name, "upgrade", "Upgrading collection " + dt.Rows[rowptr]["name"].ToString() + " because the collection installed in the application has no LastChangeDate. It may have been installed manually.");
                                                        } else {
                                                            //
                                                            // compare to last change date in collection config file
                                                            //
                                                            string LocalGuid = "";
                                                            DateTime LocalLastChangeDate = DateTime.MinValue;
                                                            foreach (XmlNode LocalListNode in Doc.DocumentElement.ChildNodes) {
                                                                switch (GenericController.vbLCase(LocalListNode.Name)) {
                                                                    case "collection":
                                                                        foreach (XmlNode CollectionNode in LocalListNode.ChildNodes) {
                                                                            switch (GenericController.vbLCase(CollectionNode.Name)) {
                                                                                case "guid":
                                                                                    //
                                                                                    LocalGuid = GenericController.vbLCase(CollectionNode.InnerText);
                                                                                    break;
                                                                                case "lastchangedate":
                                                                                    //
                                                                                    LocalLastChangeDate = GenericController.encodeDate(CollectionNode.InnerText);
                                                                                    break;
                                                                            }
                                                                        }
                                                                        break;
                                                                }
                                                                if (CollectionGuid == GenericController.vbLCase(LocalGuid)) {
                                                                    localCollectionFound = true;
                                                                    LogController.logInfo(core, logPrefix + ", local collection found");
                                                                    if (LocalLastChangeDate != DateTime.MinValue) {
                                                                        if (LocalLastChangeDate > LastChangeDate) {
                                                                            appendUpgradeLog(core, core.appConfig.name, "upgrade", "Upgrading collection " + dt.Rows[rowptr]["name"].ToString() + " because the collection in the local server store has a newer LastChangeDate than the collection installed on this application.");
                                                                            upgradeCollection = true;
                                                                        }
                                                                    }
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                        ErrorMessage = "";
                                                        if (!localCollectionFound) {
                                                            LogController.logInfo(core, logPrefix + ", site collection [" + Collectionname + "] not found in local collection, call UpgradeAllAppsFromLibCollection2 to install it.");
                                                            bool addonInstallOk = CollectionController.installCollectionFromRemoteRepo(core, CollectionGuid, ref  ErrorMessage, "", isNewBuild, repair, ref nonCriticalErrorList, logPrefix, ref installedCollections);
                                                            if (!addonInstallOk) {
                                                                //
                                                                // this may be OK so log, but do not call it an error
                                                                //
                                                                LogController.logInfo(core, logPrefix + ", site collection [" + Collectionname + "] not found in collection Library. It may be a custom collection just for this site. Collection guid [" + CollectionGuid + "]");
                                                            }
                                                        } else {
                                                            if (upgradeCollection) {
                                                                LogController.logInfo(core, logPrefix + ", upgrading collection");
                                                                CollectionController.installCollectionFromLocalRepo(core, CollectionGuid, core.codeVersion(), ref ErrorMessage, "", isNewBuild, repair, ref nonCriticalErrorList, logPrefix, ref installedCollections, true);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                            } catch (Exception ex9) {
                                LogController.handleError( core,ex9);
                            }
                        }
                    }
                    //
                    //---------------------------------------------------------------------
                    // ----- Explain, put up a link and exit without continuing
                    //---------------------------------------------------------------------
                    //
                    core.cache.invalidateAll();
                    LogController.logInfo(core, logPrefix + ", Upgrade Complete");
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// when breaking changes are required for data, update them here
        /// </summary>
        /// <param name="core"></param>
        /// <param name="DataBuildVersion"></param>
        private static void Upgrade_Conversion(CoreController core, string DataBuildVersion, string logPrefix) {
            try {
                //
                // -- Roll the style sheet cache if it is setup
                core.siteProperties.setProperty("StylesheetSerialNumber", (-1).ToString());
                //
                // -- Reload
                core.cache.invalidateAll();
                core.clearMetaData();
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        private static void verifyAdminMenus(CoreController core, string DataBuildVersion) {
            try {
                DataTable dt = null;
                //
                // ----- remove duplicate menus that may have been added during faulty upgrades
                //
                string FieldNew = null;
                string FieldLast = null;
                int FieldRecordID = 0;
                //Dim dt As DataTable
                dt = core.db.executeQuery("Select ID,Name,ParentID from ccMenuEntries where (active<>0) Order By ParentID,Name");
                if (dt.Rows.Count > 0) {
                    FieldLast = "";
                    for (var rowptr = 0; rowptr < dt.Rows.Count; rowptr++) {
                        FieldNew = GenericController.encodeText(dt.Rows[rowptr]["name"]) + "." + GenericController.encodeText(dt.Rows[rowptr]["parentid"]);
                        if (FieldNew == FieldLast) {
                            FieldRecordID = GenericController.encodeInteger(dt.Rows[rowptr]["ID"]);
                            core.db.executeQuery("Update ccMenuEntries set active=0 where ID=" + FieldRecordID + ";");
                        }
                        FieldLast = FieldNew;
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// verify a simple record exists
        /// </summary>
        /// <param name="core"></param>
        /// <param name="contentName"></param>
        /// <param name="name"></param>
        /// <param name="sqlName"></param>
        /// <param name="sqlValue"></param>
        /// <param name="inActive"></param>
        private static void verifyRecord(CoreController core, string contentName, string name, string sqlName = "", string sqlValue = "") {
            try {
                var metaData = MetaModel.createByUniqueName(core, contentName);
                DataTable dt = core.db.executeQuery("SELECT ID FROM " + metaData.tableName + " WHERE NAME=" + DbController.encodeSQLText(name) + ";");
                if (dt.Rows.Count == 0) {
                    string sql1 = "insert into " + metaData.tableName + " (active,name";
                    string sql2 = ") values (1," + DbController.encodeSQLText(name);
                    string sql3 = ")";
                    if (!string.IsNullOrEmpty(sqlName)) {
                        sql1 += "," + sqlName;
                        sql2 += "," + sqlValue;
                    }
                    core.db.executeQuery(sql1 + sql2 + sql3);
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// gaurantee db fields meet minimum requirements. Like dateTime precision
        /// </summary>
        /// <param name="core"></param>
        /// <param name="DataBuildVersion"></param>
        private static void verifySqlfieldCompatibility(CoreController core, string logPrefix) {
            try {
                //
                // verify Db field schema for fields handled internally (fix datatime2(0) problem -- need at least 3 digits for precision)
                var tableList = Models.Db.TableModel.createList(core, "","dataSourceId");
                var dataSource = DataSourceModel.addEmpty(core);
                foreach (TableModel table in tableList) {
                    if ( table.dataSourceID != dataSource.id ) { dataSource = DataSourceModel.create(core, table.dataSourceID); }
                    if ((dataSource == null)) { dataSource = DataSourceModel.addEmpty(core); }
                    var tableSchema = Models.Domain.TableSchemaModel.getTableSchema(core, table.name, "default");

                    if (tableSchema != null) {
                        foreach (Models.Domain.TableSchemaModel.ColumnSchemaModel column in tableSchema.columns) {
                            if ((column.DATA_TYPE.ToLowerInvariant() == "datetime2") && (column.DATETIME_PRECISION < 3)) {
                                //
                                LogController.logInfo(core, logPrefix + ", verifySqlFieldCompatibility, conversion required, table [" + table.name + "], field [" + column.COLUMN_NAME + "], reason [datetime precision too low (" + column.DATETIME_PRECISION.ToString() + ")]");
                                //
                                // drop any indexes that use this field
                                bool indexDropped = false;
                                foreach( Models.Domain.TableSchemaModel.IndexSchemaModel index in tableSchema.indexes) {
                                    if ( index.indexKeyList.Contains(column.COLUMN_NAME) ) {
                                        //
                                        LogController.logInfo(core, logPrefix + ", verifySqlFieldCompatibility, index [" + index.index_name + "] must be dropped");
                                        core.db.deleteSqlIndex(table.name, index.index_name);
                                        indexDropped = true;
                                        //
                                    }
                                }
                                //
                                // -- datetime2(0)...datetime2(2) need to be converted to datetime2(7)
                                // -- rename column to tempName
                                string tempName = "tempDateTime" + GenericController.GetRandomInteger(core).ToString();
                                core.db.executeNonQuery("sp_rename '" + table.name + "." + column.COLUMN_NAME + "', '" + tempName + "', 'COLUMN';");
                                core.db.executeNonQuery("ALTER TABLE " + table.name + " ADD " + column.COLUMN_NAME + " DateTime2(7) NULL;");
                                core.db.executeNonQuery("update " + table.name + " set " + column.COLUMN_NAME + "=" + tempName + " ");
                                core.db.executeNonQuery("ALTER TABLE " + table.name + " DROP COLUMN " + tempName + ";");
                                //
                                // recreate dropped indexes
                                if (indexDropped) {
                                    foreach (Models.Domain.TableSchemaModel.IndexSchemaModel index in tableSchema.indexes) {
                                        if (index.indexKeyList.Contains(column.COLUMN_NAME)) {
                                            //
                                            LogController.logInfo(core, logPrefix + ", verifySqlFieldCompatibility, recreating index [" + index.index_name + "]");
                                            core.db.createSQLIndex(table.name, index.index_name, index.index_keys);
                                            //
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// verify the basic languages are populated
        /// </summary>
        /// <param name="core"></param>
        public static void verifyLanguageRecords(CoreController core) {
            try {
                //
                appendUpgradeLogAddStep(core, core.appConfig.name, "VerifyLanguageRecords", "Verify Language Records.");
                //
                verifyRecord(core, "Languages", "English", "HTTP_Accept_Language", "'en'");
                verifyRecord(core, "Languages", "Spanish", "HTTP_Accept_Language", "'es'");
                verifyRecord(core, "Languages", "French", "HTTP_Accept_Language", "'fr'");
                verifyRecord(core, "Languages", "Any", "HTTP_Accept_Language", "'any'");
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// verify the basic library folders
        /// </summary>
        /// <param name="core"></param>
        private static void verifyLibraryFolders(CoreController core) {
            try {
                DataTable dt = null;
                //
                appendUpgradeLogAddStep(core, core.appConfig.name, "VerifyLibraryFolders", "Verify Library Folders: Images and Downloads");
                //
                dt = core.db.executeQuery("select id from cclibraryfiles");
                if (dt.Rows.Count == 0) {
                    verifyRecord(core, "Library Folders", "Images");
                    verifyRecord(core, "Library Folders", "Downloads");
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// verify library folder types
        /// </summary>
        /// <param name="core"></param>
        private static void verifyLibraryFileTypes(CoreController core) {
            try {
                //
                // Load basic records -- default images are handled in the REsource Library through the /ContensiveBase/config/DefaultValues.txt GetDefaultValue(key) mechanism
                //
                if (MetaController.getRecordIdByUniqueName( core,"Library File Types", "Image") == 0) {
                    verifyRecord(core, "Library File Types", "Image", "ExtensionList", "'GIF,JPG,JPE,JPEG,BMP,PNG'");
                    verifyRecord(core, "Library File Types", "Image", "IsImage", "1");
                    verifyRecord(core, "Library File Types", "Image", "IsVideo", "0");
                    verifyRecord(core, "Library File Types", "Image", "IsDownload", "1");
                    verifyRecord(core, "Library File Types", "Image", "IsFlash", "0");
                }
                //
                if (MetaController.getRecordIdByUniqueName( core,"Library File Types", "Video") == 0) {
                    verifyRecord(core, "Library File Types", "Video", "ExtensionList", "'ASX,AVI,WMV,MOV,MPG,MPEG,MP4,QT,RM'");
                    verifyRecord(core, "Library File Types", "Video", "IsImage", "0");
                    verifyRecord(core, "Library File Types", "Video", "IsVideo", "1");
                    verifyRecord(core, "Library File Types", "Video", "IsDownload", "1");
                    verifyRecord(core, "Library File Types", "Video", "IsFlash", "0");
                }
                //
                if (MetaController.getRecordIdByUniqueName( core,"Library File Types", "Audio") == 0) {
                    verifyRecord(core, "Library File Types", "Audio", "ExtensionList", "'AIF,AIFF,ASF,CDA,M4A,M4P,MP2,MP3,MPA,WAV,WMA'");
                    verifyRecord(core, "Library File Types", "Audio", "IsImage", "0");
                    verifyRecord(core, "Library File Types", "Audio", "IsVideo", "0");
                    verifyRecord(core, "Library File Types", "Audio", "IsDownload", "1");
                    verifyRecord(core, "Library File Types", "Audio", "IsFlash", "0");
                }
                //
                if (MetaController.getRecordIdByUniqueName( core,"Library File Types", "Word") == 0) {
                    verifyRecord(core, "Library File Types", "Word", "ExtensionList", "'DOC'");
                    verifyRecord(core, "Library File Types", "Word", "IsImage", "0");
                    verifyRecord(core, "Library File Types", "Word", "IsVideo", "0");
                    verifyRecord(core, "Library File Types", "Word", "IsDownload", "1");
                    verifyRecord(core, "Library File Types", "Word", "IsFlash", "0");
                }
                //
                if (MetaController.getRecordIdByUniqueName( core,"Library File Types", "Flash") == 0) {
                    verifyRecord(core, "Library File Types", "Flash", "ExtensionList", "'SWF'");
                    verifyRecord(core, "Library File Types", "Flash", "IsImage", "0");
                    verifyRecord(core, "Library File Types", "Flash", "IsVideo", "0");
                    verifyRecord(core, "Library File Types", "Flash", "IsDownload", "1");
                    verifyRecord(core, "Library File Types", "Flash", "IsFlash", "1");
                }
                //
                if (MetaController.getRecordIdByUniqueName( core,"Library File Types", "PDF") == 0) {
                    verifyRecord(core, "Library File Types", "PDF", "ExtensionList", "'PDF'");
                    verifyRecord(core, "Library File Types", "PDF", "IsImage", "0");
                    verifyRecord(core, "Library File Types", "PDF", "IsVideo", "0");
                    verifyRecord(core, "Library File Types", "PDF", "IsDownload", "1");
                    verifyRecord(core, "Library File Types", "PDF", "IsFlash", "0");
                }
                //
                if (MetaController.getRecordIdByUniqueName( core,"Library File Types", "XLS") == 0) {
                    verifyRecord(core, "Library File Types", "Excel", "ExtensionList", "'XLS'");
                    verifyRecord(core, "Library File Types", "Excel", "IsImage", "0");
                    verifyRecord(core, "Library File Types", "Excel", "IsVideo", "0");
                    verifyRecord(core, "Library File Types", "Excel", "IsDownload", "1");
                    verifyRecord(core, "Library File Types", "Excel", "IsFlash", "0");
                }
                //
                if (MetaController.getRecordIdByUniqueName( core,"Library File Types", "PPT") == 0) {
                    verifyRecord(core, "Library File Types", "Power Point", "ExtensionList", "'PPT,PPS'");
                    verifyRecord(core, "Library File Types", "Power Point", "IsImage", "0");
                    verifyRecord(core, "Library File Types", "Power Point", "IsVideo", "0");
                    verifyRecord(core, "Library File Types", "Power Point", "IsDownload", "1");
                    verifyRecord(core, "Library File Types", "Power Point", "IsFlash", "0");
                }
                //
                if (MetaController.getRecordIdByUniqueName( core,"Library File Types", "Default") == 0) {
                    verifyRecord(core, "Library File Types", "Default", "ExtensionList", "''");
                    verifyRecord(core, "Library File Types", "Default", "IsImage", "0");
                    verifyRecord(core, "Library File Types", "Default", "IsVideo", "0");
                    verifyRecord(core, "Library File Types", "Default", "IsDownload", "1");
                    verifyRecord(core, "Library File Types", "Default", "IsFlash", "0");
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// verify a state record
        /// </summary>
        /// <param name="core"></param>
        /// <param name="Name"></param>
        /// <param name="Abbreviation"></param>
        /// <param name="SaleTax"></param>
        /// <param name="CountryID"></param>
        private static void verifyState(CoreController core, string Name, string Abbreviation, double SaleTax, int CountryID) {
            try {
                var state = Models.Db.StateModel.createByUniqueName(core, Name);
                if ( state == null ) state = StateModel.addEmpty(core);
                state.abbreviation = Abbreviation;
                state.salesTax = SaleTax;
                state.countryID = CountryID;
                state.save(core,true);
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// verify all default states
        /// </summary>
        /// <param name="core"></param>
        public static void verifyStates(CoreController core) {
            try {
                //
                appendUpgradeLogAddStep(core, core.appConfig.name, "VerifyStates", "Verify States");
                //
                verifyCountry(core, "United States", "US");
                int CountryID = MetaController.getRecordIdByUniqueName( core,"Countries", "United States");
                //
                verifyState(core, "Alaska", "AK", 0.0D, CountryID);
                verifyState(core, "Alabama", "AL", 0.0D, CountryID);
                verifyState(core, "Arizona", "AZ", 0.0D, CountryID);
                verifyState(core, "Arkansas", "AR", 0.0D, CountryID);
                verifyState(core, "California", "CA", 0.0D, CountryID);
                verifyState(core, "Connecticut", "CT", 0.0D, CountryID);
                verifyState(core, "Colorado", "CO", 0.0D, CountryID);
                verifyState(core, "Delaware", "DE", 0.0D, CountryID);
                verifyState(core, "District of Columbia", "DC", 0.0D, CountryID);
                verifyState(core, "Florida", "FL", 0.0D, CountryID);
                verifyState(core, "Georgia", "GA", 0.0D, CountryID);

                verifyState(core, "Hawaii", "HI", 0.0D, CountryID);
                verifyState(core, "Idaho", "ID", 0.0D, CountryID);
                verifyState(core, "Illinois", "IL", 0.0D, CountryID);
                verifyState(core, "Indiana", "IN", 0.0D, CountryID);
                verifyState(core, "Iowa", "IA", 0.0D, CountryID);
                verifyState(core, "Kansas", "KS", 0.0D, CountryID);
                verifyState(core, "Kentucky", "KY", 0.0D, CountryID);
                verifyState(core, "Louisiana", "LA", 0.0D, CountryID);
                verifyState(core, "Massachusetts", "MA", 0.0D, CountryID);
                verifyState(core, "Maine", "ME", 0.0D, CountryID);

                verifyState(core, "Maryland", "MD", 0.0D, CountryID);
                verifyState(core, "Michigan", "MI", 0.0D, CountryID);
                verifyState(core, "Minnesota", "MN", 0.0D, CountryID);
                verifyState(core, "Missouri", "MO", 0.0D, CountryID);
                verifyState(core, "Mississippi", "MS", 0.0D, CountryID);
                verifyState(core, "Montana", "MT", 0.0D, CountryID);
                verifyState(core, "North Carolina", "NC", 0.0D, CountryID);
                verifyState(core, "Nebraska", "NE", 0.0D, CountryID);
                verifyState(core, "New Hampshire", "NH", 0.0D, CountryID);
                verifyState(core, "New Mexico", "NM", 0.0D, CountryID);

                verifyState(core, "New Jersey", "NJ", 0.0D, CountryID);
                verifyState(core, "New York", "NY", 0.0D, CountryID);
                verifyState(core, "Nevada", "NV", 0.0D, CountryID);
                verifyState(core, "North Dakota", "ND", 0.0D, CountryID);
                verifyState(core, "Ohio", "OH", 0.0D, CountryID);
                verifyState(core, "Oklahoma", "OK", 0.0D, CountryID);
                verifyState(core, "Oregon", "OR", 0.0D, CountryID);
                verifyState(core, "Pennsylvania", "PA", 0.0D, CountryID);
                verifyState(core, "Rhode Island", "RI", 0.0D, CountryID);
                verifyState(core, "South Carolina", "SC", 0.0D, CountryID);

                verifyState(core, "South Dakota", "SD", 0.0D, CountryID);
                verifyState(core, "Tennessee", "TN", 0.0D, CountryID);
                verifyState(core, "Texas", "TX", 0.0D, CountryID);
                verifyState(core, "Utah", "UT", 0.0D, CountryID);
                verifyState(core, "Vermont", "VT", 0.0D, CountryID);
                verifyState(core, "Virginia", "VA", 0.045, CountryID);
                verifyState(core, "Washington", "WA", 0.0D, CountryID);
                verifyState(core, "Wisconsin", "WI", 0.0D, CountryID);
                verifyState(core, "West Virginia", "WV", 0.0D, CountryID);
                verifyState(core, "Wyoming", "WY", 0.0D, CountryID);
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// verify a country
        /// </summary>
        /// <param name="core"></param>
        /// <param name="name"></param>
        /// <param name="abbreviation"></param>
        private static void verifyCountry(CoreController core, string name, string abbreviation) {
            try {
                using (var csData = new CsModel(core)) {
                    csData.open("Countries", "name=" + DbController.encodeSQLText(name));
                    if (!csData.ok()) {
                        csData.close();
                        csData.insert("Countries");
                        if (csData.ok()) {
                            csData.set("ACTIVE", true);
                        }
                    }
                    if (csData.ok()) {
                        csData.set("NAME", name);
                        csData.set("Abbreviation", abbreviation);
                        if (GenericController.vbLCase(name) == "united states") {
                            csData.set("DomesticShipping", "1");
                        }
                    }
                    csData.close();
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Verify all base countries
        /// </summary>
        /// <param name="core"></param>
        public static void verifyCountries(CoreController core) {
            try {
                //
                appendUpgradeLogAddStep(core, core.appConfig.name, "VerifyCountries", "Verify Countries");
                //
                string list = core.fileAppRoot.readFileText("cclib\\config\\DefaultCountryList.txt");
                string[] rows  = GenericController.stringSplit(list, "\r\n");
                foreach( var row in rows) {
                    if (!string.IsNullOrEmpty(row)) {
                        string[] attrs = row.Split(';');
                        foreach (var attr in attrs) {
                            verifyCountry(core, encodeInitialCaps(attr), attrs[1]);
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// verify default groups
        /// </summary>
        /// <param name="core"></param>
        public static void verifyDefaultGroups(CoreController core) {
            try {
                //
                int GroupID = 0;
                string SQL = null;
                //
                appendUpgradeLogAddStep(core, core.appConfig.name, "VerifyDefaultGroups", "Verify Default Groups");
                //
                GroupID = GroupController.add(core, "Site Managers");
                SQL = "Update ccContent Set EditorGroupID=" + DbController.encodeSQLNumber(GroupID) + " where EditorGroupID is null;";
                core.db.executeQuery(SQL);
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Verify all the core tables
        /// </summary>
        /// <param name="core"></param>
        /// <param name="logPrefix"></param>
        internal static void verifyBasicTables(CoreController core, string logPrefix) {
            try {
                //
                {
                    logPrefix += "-verifyBasicTables";
                    LogController.logInfo(core, logPrefix + ", enter");
                    //appendUpgradeLogAddStep(core, core.appConfig.name, "VerifyCoreTables", "Verify Core SQL Tables");
                    //
                    core.db.createSQLTable( "ccDataSources");
                    core.db.createSQLTableField( "ccDataSources", "username", fieldTypeIdText);
                    core.db.createSQLTableField( "ccDataSources", "password", fieldTypeIdText);
                    core.db.createSQLTableField( "ccDataSources", "connString", fieldTypeIdText);
                    core.db.createSQLTableField( "ccDataSources", "endpoint", fieldTypeIdText);
                    core.db.createSQLTableField( "ccDataSources", "dbTypeId", fieldTypeIdLookup);
                    //core.db.createSQLTableField( "ccDataSources", "address", fieldTypeIdText);
                    //
                    core.db.createSQLTable( "ccTables");
                    core.db.createSQLTableField( "ccTables", "DataSourceID", fieldTypeIdLookup);
                    //
                    core.db.createSQLTable( "ccContent");
                    core.db.createSQLTableField( "ccContent", "ContentTableID", fieldTypeIdInteger);
                    core.db.createSQLTableField( "ccContent", "AuthoringTableID", fieldTypeIdInteger);
                    core.db.createSQLTableField( "ccContent", "AllowAdd", fieldTypeIdBoolean);
                    core.db.createSQLTableField( "ccContent", "AllowDelete", fieldTypeIdBoolean);
                    core.db.createSQLTableField( "ccContent", "AllowWorkflowAuthoring", fieldTypeIdBoolean);
                    core.db.createSQLTableField( "ccContent", "DeveloperOnly", fieldTypeIdBoolean);
                    core.db.createSQLTableField( "ccContent", "AdminOnly", fieldTypeIdBoolean);
                    core.db.createSQLTableField( "ccContent", "ParentID", fieldTypeIdInteger);
                    core.db.createSQLTableField( "ccContent", "DefaultSortMethodID", fieldTypeIdInteger);
                    core.db.createSQLTableField( "ccContent", "DropDownFieldList", fieldTypeIdText);
                    core.db.createSQLTableField( "ccContent", "EditorGroupID", fieldTypeIdInteger);
                    core.db.createSQLTableField( "ccContent", "AllowCalendarEvents", fieldTypeIdBoolean);
                    core.db.createSQLTableField( "ccContent", "AllowContentTracking", fieldTypeIdBoolean);
                    core.db.createSQLTableField( "ccContent", "AllowTopicRules", fieldTypeIdBoolean);
                    core.db.createSQLTableField( "ccContent", "AllowContentChildTool", fieldTypeIdBoolean);
                    core.db.createSQLTableField( "ccContent", "IconLink", fieldTypeIdLink);
                    core.db.createSQLTableField( "ccContent", "IconHeight", fieldTypeIdInteger);
                    core.db.createSQLTableField( "ccContent", "IconWidth", fieldTypeIdInteger);
                    core.db.createSQLTableField( "ccContent", "IconSprites", fieldTypeIdInteger);
                    core.db.createSQLTableField( "ccContent", "installedByCollectionId", fieldTypeIdInteger);
                    core.db.createSQLTableField( "ccContent", "IsBaseContent", fieldTypeIdBoolean);
                    //
                    core.db.createSQLTable( "ccFields");
                    core.db.createSQLTableField( "ccFields", "ContentID", fieldTypeIdInteger);
                    core.db.createSQLTableField( "ccFields", "Type", fieldTypeIdInteger);
                    core.db.createSQLTableField( "ccFields", "Caption", fieldTypeIdText);
                    core.db.createSQLTableField( "ccFields", "ReadOnly", fieldTypeIdBoolean);
                    core.db.createSQLTableField( "ccFields", "NotEditable", fieldTypeIdBoolean);
                    core.db.createSQLTableField( "ccFields", "LookupContentID", fieldTypeIdInteger);
                    core.db.createSQLTableField( "ccFields", "RedirectContentID", fieldTypeIdInteger);
                    core.db.createSQLTableField( "ccFields", "RedirectPath", fieldTypeIdText);
                    core.db.createSQLTableField( "ccFields", "RedirectID", fieldTypeIdText);
                    core.db.createSQLTableField( "ccFields", "HelpMessage", fieldTypeIdLongText); // deprecated but Im chicken to remove this
                    core.db.createSQLTableField( "ccFields", "UniqueName", fieldTypeIdBoolean);
                    core.db.createSQLTableField( "ccFields", "TextBuffered", fieldTypeIdBoolean);
                    core.db.createSQLTableField( "ccFields", "Password", fieldTypeIdBoolean);
                    core.db.createSQLTableField( "ccFields", "IndexColumn", fieldTypeIdInteger);
                    core.db.createSQLTableField( "ccFields", "IndexWidth", fieldTypeIdText);
                    core.db.createSQLTableField( "ccFields", "IndexSortPriority", fieldTypeIdInteger);
                    core.db.createSQLTableField( "ccFields", "IndexSortDirection", fieldTypeIdInteger);
                    core.db.createSQLTableField( "ccFields", "EditSortPriority", fieldTypeIdInteger);
                    core.db.createSQLTableField( "ccFields", "AdminOnly", fieldTypeIdBoolean);
                    core.db.createSQLTableField( "ccFields", "DeveloperOnly", fieldTypeIdBoolean);
                    core.db.createSQLTableField( "ccFields", "DefaultValue", fieldTypeIdText);
                    core.db.createSQLTableField( "ccFields", "Required", fieldTypeIdBoolean);
                    core.db.createSQLTableField( "ccFields", "HTMLContent", fieldTypeIdBoolean);
                    core.db.createSQLTableField( "ccFields", "Authorable", fieldTypeIdBoolean);
                    core.db.createSQLTableField( "ccFields", "ManyToManyContentID", fieldTypeIdInteger);
                    core.db.createSQLTableField( "ccFields", "ManyToManyRuleContentID", fieldTypeIdInteger);
                    core.db.createSQLTableField( "ccFields", "ManyToManyRulePrimaryField", fieldTypeIdText);
                    core.db.createSQLTableField( "ccFields", "ManyToManyRuleSecondaryField", fieldTypeIdText);
                    core.db.createSQLTableField( "ccFields", "RSSTitleField", fieldTypeIdBoolean);
                    core.db.createSQLTableField( "ccFields", "RSSDescriptionField", fieldTypeIdBoolean);
                    core.db.createSQLTableField( "ccFields", "MemberSelectGroupID", fieldTypeIdInteger);
                    core.db.createSQLTableField( "ccFields", "EditTab", fieldTypeIdText);
                    core.db.createSQLTableField( "ccFields", "Scramble", fieldTypeIdBoolean);
                    core.db.createSQLTableField( "ccFields", "LookupList", fieldTypeIdText);
                    core.db.createSQLTableField( "ccFields", "IsBaseField", fieldTypeIdBoolean);
                    core.db.createSQLTableField( "ccFields", "installedByCollectionId", fieldTypeIdInteger);
                    //
                    core.db.createSQLTable( "ccFieldHelp");
                    core.db.createSQLTableField( "ccFieldHelp", "FieldID", fieldTypeIdLookup);
                    core.db.createSQLTableField( "ccFieldHelp", "HelpDefault", fieldTypeIdLongText);
                    core.db.createSQLTableField( "ccFieldHelp", "HelpCustom", fieldTypeIdLongText);
                    //
                    core.db.createSQLTable( "ccSetup");
                    core.db.createSQLTableField( "ccSetup", "FieldValue", fieldTypeIdText);
                    core.db.createSQLTableField( "ccSetup", "DeveloperOnly", fieldTypeIdBoolean);
                    //
                    core.db.createSQLTable( "ccSortMethods");
                    core.db.createSQLTableField( "ccSortMethods", "OrderByClause", fieldTypeIdText);
                    //
                    core.db.createSQLTable( "ccFieldTypes");
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //  todo deprecate 
        private static void appendUpgradeLog(CoreController core, string appName, string Method, string Message) {
            LogController.logInfo(core, "app [" + appName + "], Method [" + Method + "], Message [" + Message + "]");
        }
        //
        //====================================================================================================
        // todo deprecate
        private static void appendUpgradeLogAddStep(CoreController core, string appName, string Method, string Message) {
            appendUpgradeLog(core, appName, Method, Message);
        }
        //
        //====================================================================================================
        /// <summary>
        /// verify a nanigator entry
        /// </summary>
        /// <param name="core"></param>
        /// <param name="ccGuid"></param>
        /// <param name="menuNameSpace"></param>
        /// <param name="EntryName"></param>
        /// <param name="ContentName"></param>
        /// <param name="LinkPage"></param>
        /// <param name="SortOrder"></param>
        /// <param name="AdminOnly"></param>
        /// <param name="DeveloperOnly"></param>
        /// <param name="NewWindow"></param>
        /// <param name="Active"></param>
        /// <param name="AddonName"></param>
        /// <param name="NavIconType"></param>
        /// <param name="NavIconTitle"></param>
        /// <param name="InstalledByCollectionID"></param>
        /// <returns></returns>
        public static int verifyNavigatorEntry(CoreController core, string ccGuid, string menuNameSpace, string EntryName, string ContentName, string LinkPage, string SortOrder, bool AdminOnly, bool DeveloperOnly, bool NewWindow, bool Active, string AddonName, string NavIconType, string NavIconTitle, int InstalledByCollectionID) {
            int returnEntry = 0;
            try {
                if (!string.IsNullOrEmpty(EntryName.Trim())) {
                    int addonId = MetaController.getRecordIdByUniqueName( core,Models.Db.AddonModel.contentName, AddonName);
                    int parentId = verifyNavigatorEntry_getParentIdFromNameSpace(core, menuNameSpace);
                    int contentId = MetaModel.getContentId(core, ContentName);
                    string listCriteria = "(name=" + DbController.encodeSQLText(EntryName) + ")and(Parentid=" + parentId + ")";
                    List<Models.Db.NavigatorEntryModel> entryList = NavigatorEntryModel.createList(core, listCriteria, "id");
                    NavigatorEntryModel entry = null;
                    if (entryList.Count == 0) {
                        entry = NavigatorEntryModel.addEmpty(core);
                        entry.name = EntryName.Trim();
                        entry.ParentID = parentId;
                    } else {
                        entry = entryList.First();
                    }
                    if (contentId <= 0) {
                        entry.ContentID = 0;
                    } else {
                        entry.ContentID = contentId;
                    }
                    entry.LinkPage = LinkPage;
                    entry.sortOrder = SortOrder;
                    entry.AdminOnly = AdminOnly;
                    entry.DeveloperOnly = DeveloperOnly;
                    entry.NewWindow = NewWindow;
                    entry.active = Active;
                    entry.AddonID = addonId;
                    entry.ccguid = ccGuid;
                    entry.NavIconTitle = NavIconTitle;
                    entry.NavIconType = GetListIndex(NavIconType, NavIconTypeList);
                    entry.InstalledByCollectionID = InstalledByCollectionID;
                    entry.save(core);
                    returnEntry = entry.id;
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return returnEntry;
        }
        //
        //====================================================================================================
        /// <summary>
        /// get navigator id from namespace
        /// </summary>
        /// <param name="core"></param>
        /// <param name="menuNameSpace"></param>
        /// <returns></returns>
        public static int verifyNavigatorEntry_getParentIdFromNameSpace(CoreController core, string menuNameSpace) {
            int parentRecordId = 0;
            try {
                if (!string.IsNullOrEmpty(menuNameSpace.Trim())) {
                    string[] parents = menuNameSpace.Trim().Split('.');
                    foreach( var parent in parents ) {
                        string recordName = parent.Trim();
                        if (!string.IsNullOrEmpty(recordName)) {
                            string Criteria = "(name=" + DbController.encodeSQLText(recordName) + ")";
                            if (parentRecordId == 0) {
                                Criteria += "and((Parentid is null)or(Parentid=0))";
                            } else {
                                Criteria += "and(Parentid=" + parentRecordId + ")";
                            }
                            int RecordID = 0;
                            using (var csData = new CsModel(core)) {
                                csData.open(NavigatorEntryModel.contentName, Criteria, "ID", true, 0,  "ID", 1);
                                if (csData.ok()) {
                                    RecordID = (csData.getInteger("ID"));
                                }
                                csData.close();
                                if (RecordID == 0) {
                                    csData.insert(NavigatorEntryModel.contentName);
                                    if (csData.ok()) {
                                        RecordID = csData.getInteger("ID");
                                        csData.set("name", recordName);
                                        csData.set("parentID", parentRecordId);
                                    }
                                }
                            }
                            parentRecordId = RecordID;
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return parentRecordId;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Create an entry in the Sort Methods Table
        /// </summary>
        private static void verifySortMethod(CoreController core, string Name, string OrderByCriteria) {
            try {
                //
                DataTable dt = null;
                SqlFieldListClass sqlList = new SqlFieldListClass();
                //
                sqlList.add("name", DbController.encodeSQLText(Name));
                sqlList.add("CreatedBy", "0");
                sqlList.add("OrderByClause", DbController.encodeSQLText(OrderByCriteria));
                sqlList.add("active", SQLTrue);
                sqlList.add("contentControlId", MetaModel.getContentId(core, "Sort Methods").ToString());
                //
                dt = core.db.openTable( "ccSortMethods", "Name=" + DbController.encodeSQLText(Name), "ID", "ID", 1, 1);
                if (dt.Rows.Count > 0) {
                    //
                    // update sort method
                    //
                    int recordId = GenericController.encodeInteger(dt.Rows[0]["ID"]);
                    core.db.updateTableRecord( "ccSortMethods", "ID=" + recordId.ToString(), sqlList, true);
                    SortMethodModel.invalidateRecordCache(core, recordId);
                } else {
                    //
                    // Create the new sort method
                    //
                    core.db.insertTableRecord( "ccSortMethods", sqlList);
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public static void verifySortMethods(CoreController core) {
            try {
                //
                LogController.logInfo(core, "Verify Sort Records");
                //
                verifySortMethod(core, "By Name", "Name");
                verifySortMethod(core, "By Alpha Sort Order Field", "SortOrder");
                verifySortMethod(core, "By Date", "DateAdded");
                verifySortMethod(core, "By Date Reverse", "DateAdded Desc");
                verifySortMethod(core, "By Alpha Sort Order Then Oldest First", "SortOrder,ID");
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Get a ContentID from the ContentName using just the tables
        /// </summary>
        internal static void verifyContentFieldTypes(CoreController core) {
            try {
                //
                int RowsFound = 0;
                int CID = 0;
                bool TableBad = false;
                int RowsNeeded = 0;
                //
                // ----- make sure there are enough records
                //
                TableBad = false;
                RowsFound = 0;
                using (DataTable rs = core.db.executeQuery("Select ID from ccFieldTypes order by id")) {
                    if (!DbController.isDataTableOk(rs)) {
                        //
                        // problem
                        //
                        TableBad = true;
                    } else {
                        //
                        // Verify the records that are there
                        //
                        RowsFound = 0;
                        foreach (DataRow dr in rs.Rows) {
                            RowsFound = RowsFound + 1;
                            if (RowsFound != GenericController.encodeInteger(dr["ID"])) {
                                //
                                // Bad Table
                                //
                                TableBad = true;
                                break;
                            }
                        }
                    }

                }
                //
                // ----- Replace table if needed
                //
                if (TableBad) {
                    core.db.deleteTable( "ccFieldTypes");
                    core.db.createSQLTable( "ccFieldTypes");
                    RowsFound = 0;
                }
                //
                // ----- Add the number of rows needed
                //
                RowsNeeded = fieldTypeIdMax - RowsFound;
                if (RowsNeeded > 0) {
                    CID = MetaModel.getContentId(core, "Content Field Types");
                    if (CID <= 0) {
                        //
                        // Problem
                        //
                        LogController.handleError(core, new GenericException("Content Field Types content definition was not found"));
                    } else {
                        while (RowsNeeded > 0) {
                            core.db.executeQuery("Insert into ccFieldTypes (active,contentcontrolid)values(1," + CID + ")");
                            RowsNeeded = RowsNeeded - 1;
                        }
                    }
                }
                //
                // ----- Update the Names of each row
                //
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='Integer' where ID=1;");
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='Text' where ID=2;");
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='LongText' where ID=3;");
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='Boolean' where ID=4;");
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='Date' where ID=5;");
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='File' where ID=6;");
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='Lookup' where ID=7;");
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='Redirect' where ID=8;");
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='Currency' where ID=9;");
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='TextFile' where ID=10;");
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='Image' where ID=11;");
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='Float' where ID=12;");
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='AutoIncrement' where ID=13;");
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='ManyToMany' where ID=14;");
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='Member Select' where ID=15;");
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='CSS File' where ID=16;");
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='XML File' where ID=17;");
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='Javascript File' where ID=18;");
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='Link' where ID=19;");
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='Resource Link' where ID=20;");
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='HTML' where ID=21;");
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='HTML File' where ID=22;");
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
    }
}
