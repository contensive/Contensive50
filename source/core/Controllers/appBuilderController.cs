
using System;
using System.Xml;
using System.Collections.Generic;
using Contensive.Processor.Models.DbModels;
using static Contensive.Processor.Controllers.genericController;
using static Contensive.Processor.constants;
using System.Data;
using System.Linq;
using Contensive.Processor.Models.Context;
//
namespace Contensive.Processor.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// code to built and upgrade apps
    /// not IDisposable - not contained classes that need to be disposed
    /// </summary>
    public class appBuilderController {
        // 
        //=========================================================================
        //
        public static void upgrade(CoreController core, bool isNewBuild, bool repair) {
            try {
                if (core.doc.upgradeInProgress) {
                    // leftover from 4.1
                } else {
                    core.doc.upgradeInProgress = true;
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
                    VerifyBasicTables(core);
                    //
                    // 20180217 - move this before base collection because during install it runs addons (like _oninstall)
                    // if anything is needed that is not there yet, I need to build a list of adds to run after the app goes to app status ok
                    // -- Update server config file
                    logController.logInfo(core, "Update configuration file");
                    if (!core.appConfig.appStatus.Equals(appConfigModel.appStatusEnum.ok)) {
                        core.appConfig.appStatus = appConfigModel.appStatusEnum.ok;
                        core.serverConfig.saveObject(core);
                    }
                    //
                    // verify current database meets minimum field requirements (before installing base collection)
                    logController.logInfo(core, "Verify existing database fields meet requirements");
                    VerifySqlfieldCompatibility(core);
                    //
                    // -- verify base collection
                    logController.logInfo(core, "Install base collection");
                    CollectionController.installBaseCollection(core, isNewBuild, repair, ref  nonCriticalErrorList);
                    //
                    // -- verify iis configuration
                    logController.logInfo(core, "Verify iis configuration");
                    Controllers.IisController.verifySite(core, core.appConfig.name, primaryDomain, core.appConfig.localWwwPath, "default.aspx");
                    //
                    // -- verify root developer
                    logController.logInfo(core, "verify developer user");
                    var root = personModel.create(core, defaultRootUserGuid);
                    if (root == null) {
                        logController.logInfo(core, "root user guid not found, test for root username");
                        var rootList = personModel.createList(core, "(username='root')");
                        if ( rootList.Count > 0 ) {
                            logController.logInfo(core, "root username found");
                            root = rootList.First();
                        }
                    }
                    if ( root == null ) {
                        logController.logInfo(core, "root user not found, adding root/contensive");
                        root = personModel.add(core);
                        root.name = defaultRootUserName;
                        root.FirstName = defaultRootUserName;
                        root.Username = defaultRootUserUsername;
                        root.Password = defaultRootUserPassword;
                        root.Developer = true;
                        try {
                            root.save(core);
                        } catch (Exception) {
                            logController.logInfo(core, "error prevented root user update");
                        }
                    }
                    //
                    // -- verify site managers group
                    logController.logInfo(core, "verify site managers groups");
                    var group = groupModel.create(core, defaultSiteManagerGuid);
                    if (group == null) {
                        logController.logInfo(core, "verify site manager group");
                        group.name = defaultSiteManagerName;
                        group.Caption = defaultSiteManagerName;
                        group.AllowBulkEmail = true;
                        group.ccguid = defaultSiteManagerGuid;
                        try {
                            group.save(core);
                        } catch (Exception) {
                            logController.logInfo(core, "error creating site managers group");
                        }
                    }
                    if ((root != null) & (group != null)) {
                        //
                        // -- verify root is in site managers
                        var memberRuleList = memberRuleModel.createList(core, "(groupid=" + group.id.ToString() + ")and(MemberID=" + root.id.ToString() + ")");
                        if (memberRuleList.Count == 0) {
                            var memberRule = memberRuleModel.add(core);
                            memberRule.GroupID = group.id;
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
                        logController.logInfo(core, "Run database conversions, DataBuildVersion [" + DataBuildVersion + "], software version [" + core.codeVersion() + "]");
                        Upgrade_Conversion(core, DataBuildVersion);
                    }
                    //
                    //---------------------------------------------------------------------
                    // ----- Verify content needed internally
                    //---------------------------------------------------------------------
                    //
                    logController.logInfo(core, "Verify records required");
                    //
                    //  menus are created in ccBase.xml, this just checks for dups
                    VerifyAdminMenus(core, DataBuildVersion);
                    VerifyLanguageRecords(core);
                    VerifyCountries(core);
                    VerifyStates(core);
                    VerifyLibraryFolders(core);
                    VerifyLibraryFileTypes(core);
                    VerifyDefaultGroups(core);
                    VerifyScriptingRecords(core);
                    //
                    //---------------------------------------------------------------------
                    // ----- Set Default SitePropertyDefaults
                    //       must be after upgrade_conversion
                    //---------------------------------------------------------------------
                    //
                    logController.logInfo(core, "Verify Site Properties");
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
                    core.siteProperties.getText("AllowTransactionLog", "False");
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
                    addonModel defaultRouteAddon = addonModel.create(core, core.siteProperties.defaultRouteId);
                    if (defaultRouteAddon == null) {
                        defaultRouteAddon = addonModel.create(core, addonGuidPageManager);
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
                        domainModel domain = domainModel.createByName(core, primaryDomain);
                        if (domain == null) {
                            domain = domainModel.add(core);
                            domain.name = primaryDomain;
                        }
                        //
                        // -- Landing Page
                        pageContentModel landingPage = pageContentModel.create(core, DefaultLandingPageGuid);
                        if (landingPage == null) {
                            landingPage = pageContentModel.add(core);
                            landingPage.name = "Home";
                            landingPage.ccguid = DefaultLandingPageGuid;
                        }
                        //
                        // -- default template
                        pageTemplateModel defaultTemplate = pageTemplateModel.createByName(core, "Default");
                        if (defaultTemplate == null) {
                            defaultTemplate = pageTemplateModel.add(core);
                            defaultTemplate.name = "Default";
                        }
                        defaultTemplate.bodyHTML = Properties.Resources.DefaultTemplateHtml;
                        defaultTemplate.save(core);
                        //
                        domain.defaultTemplateId = defaultTemplate.id;
                        domain.name = primaryDomain;
                        domain.pageNotFoundPageId = landingPage.id;
                        domain.rootPageId = landingPage.id;
                        domain.typeId = (int) domainModel.domainTypeEnum.Normal;
                        domain.visited = false;
                        domain.save(core);
                        //
                        landingPage.TemplateID = defaultTemplate.id;
                        landingPage.Copyfilename.content = constants.defaultLandingPageHtml;
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
                        logController.logInfo(core, "Internal upgrade complete, set Buildversion to " + core.codeVersion());
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
                            bool IISResetRequired = false;
                            //RegisterList = ""
                            logController.logInfo(core, "Upgrading All Local Collections to new server build.");
                            string tmpString = "";
                            bool UpgradeOK = CollectionController.upgradeLocalCollectionRepoFromRemoteCollectionRepo(core, ref ErrorMessage, ref tmpString, ref  IISResetRequired, isNewBuild, repair, ref  nonCriticalErrorList);
                            if (!string.IsNullOrEmpty(ErrorMessage)) {
                                throw (new ApplicationException("Unexpected exception")); //core.handleLegacyError3(core.appConfig.name, "During UpgradeAllLocalCollectionsFromLib3 call, " & ErrorMessage, "dll", "builderClass", "Upgrade2", 0, "", "", False, True, "")
                            } else if (!UpgradeOK) {
                                throw (new ApplicationException("Unexpected exception")); //core.handleLegacyError3(core.appConfig.name, "During UpgradeAllLocalCollectionsFromLib3 call, NotOK was returned without an error message", "dll", "builderClass", "Upgrade2", 0, "", "", False, True, "")
                            }
                            //
                            //---------------------------------------------------------------------
                            // ----- Upgrade collections added during upgrade process
                            //---------------------------------------------------------------------
                            //
                            //Call appendUpgradeLog(core, "Installing Add-on Collections gathered during upgrade")
                            //If InstallCollectionList = "" Then
                            //    Call appendUpgradeLog(core.app.config.name, MethodName, "No Add-on collections added during upgrade")
                            //Else
                            //    ErrorMessage = ""
                            //    Guids = Split(InstallCollectionList, ",")
                            //    For Ptr = 0 To UBound(Guids)
                            //        ErrorMessage = ""
                            //        Guid = Guids[Ptr]
                            //        If Guid <> "" Then
                            //            saveLogFolder = classLogFolder
                            //            Call addonInstall.GetCollectionConfig(Guid, CollectionPath, LastChangeDate, "")
                            //            classLogFolder = saveLogFolder
                            //            If CollectionPath <> "" Then
                            //                '
                            //                ' This collection is installed locally, install from local collections
                            //                '
                            //                saveLogFolder = classLogFolder
                            //                Call addonInstall.installCollectionFromLocalRepo(Me, IISResetRequired, Guid, core.version, ErrorMessage, "", "", isNewBuild)
                            //                classLogFolder = saveLogFolder
                            //                'Call AddonInstall.UpgradeAppFromLocalCollection( Me, ParentNavigatorID, IISResetRequired, Guid, getcodeversion, ErrorMessage, RegisterList, "")
                            //            Else
                            //                '
                            //                ' This is a new collection, install to the server and force it on this site
                            //                '
                            //                saveLogFolder = classLogFolder
                            //                addonInstallOk = addonInstall.installCollectionFromRemoteRepo(Guid, DataBuildVersion, IISResetRequired, "", ErrorMessage, "", isNewBuild)
                            //                classLogFolder = saveLogFolder
                            //                If Not addonInstallOk Then
                            //                   throw (New ApplicationException("Unexpected exception"))'core.handleLegacyError3(core.app.config.name, "Error upgrading Addon Collection [" & Guid & "], " & ErrorMessage, "dll", "builderClass", "Upgrade2", 0, "", "", False, True, "")
                            //                End If
                            //            End If
                            //        End If
                            //    Next
                            //End If
                            //
                            //---------------------------------------------------------------------
                            // ----- Upgrade all collection for this app (in case collections were installed before the upgrade
                            //---------------------------------------------------------------------
                            //
                            string Collectionname = null;
                            string CollectionGuid = null;
                            bool localCollectionFound = false;
                            logController.logInfo(core, "Checking all installed collections for upgrades from Collection Library");
                            logController.logInfo(core, "...Open collectons.xml");
                            try {
                                XmlDocument Doc = new XmlDocument();
                                Doc.LoadXml(CollectionController.getLocalCollectionStoreListXml(core));
                                if (true) {
                                    if (genericController.vbLCase(Doc.DocumentElement.Name) != genericController.vbLCase(CollectionListRootNode)) {
                                        throw (new ApplicationException("Unexpected exception")); //core.handleLegacyError3(core.appConfig.name, "Error loading Collection config file. The Collections.xml file has an invalid root node, [" & Doc.DocumentElement.Name & "] was received and [" & CollectionListRootNode & "] was expected.", "dll", "builderClass", "Upgrade", 0, "", "", False, True, "")
                                    } else {
                                        if (genericController.vbLCase(Doc.DocumentElement.Name) == "collectionlist") {
                                            //
                                            // now go through each collection in this app and check the last updated agains the one here
                                            //
                                            logController.logInfo(core, "...Open site collectons, iterate through all collections");
                                            //Dim dt As DataTable
                                            DataTable dt = core.db.executeQuery("select * from ccaddoncollections where (ccguid is not null)and(updatable<>0)");
                                            if (dt.Rows.Count > 0) {
                                                int rowptr = 0;
                                                for (rowptr = 0; rowptr < dt.Rows.Count; rowptr++) {

                                                    ErrorMessage = "";
                                                    CollectionGuid = genericController.vbLCase(dt.Rows[rowptr]["ccguid"].ToString());
                                                    Collectionname = dt.Rows[rowptr]["name"].ToString();
                                                    logController.logInfo(core, "...checking collection [" + Collectionname + "], guid [" + CollectionGuid + "]");
                                                    if (CollectionGuid != "{7c6601a7-9d52-40a3-9570-774d0d43d758}") {
                                                        //
                                                        // upgrade all except base collection from the local collections
                                                        //
                                                        localCollectionFound = false;
                                                        bool upgradeCollection = false;
                                                        DateTime LastChangeDate = genericController.encodeDate(dt.Rows[rowptr]["LastChangeDate"]);
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
                                                                switch (genericController.vbLCase(LocalListNode.Name)) {
                                                                    case "collection":
                                                                        foreach (XmlNode CollectionNode in LocalListNode.ChildNodes) {
                                                                            switch (genericController.vbLCase(CollectionNode.Name)) {
                                                                                case "guid":
                                                                                    //
                                                                                    LocalGuid = genericController.vbLCase(CollectionNode.InnerText);
                                                                                    break;
                                                                                case "lastchangedate":
                                                                                    //
                                                                                    LocalLastChangeDate = genericController.encodeDate(CollectionNode.InnerText);
                                                                                    break;
                                                                            }
                                                                        }
                                                                        break;
                                                                }
                                                                if (CollectionGuid == genericController.vbLCase(LocalGuid)) {
                                                                    localCollectionFound = true;
                                                                    logController.logInfo(core, "...local collection found");
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
                                                            logController.logInfo(core, "...site collection [" + Collectionname + "] not found in local collection, call UpgradeAllAppsFromLibCollection2 to install it.");
                                                            bool addonInstallOk = CollectionController.installCollectionFromRemoteRepo(core, CollectionGuid, ref  ErrorMessage, "", isNewBuild, repair, ref nonCriticalErrorList);
                                                            if (!addonInstallOk) {
                                                                //
                                                                // this may be OK so log, but do not call it an error
                                                                //
                                                                logController.logInfo(core, "...site collection [" + Collectionname + "] not found in collection Library. It may be a custom collection just for this site. Collection guid [" + CollectionGuid + "]");
                                                            }
                                                        } else {
                                                            if (upgradeCollection) {
                                                                logController.logInfo(core, "...upgrading collection");
                                                                CollectionController.installCollectionFromLocalRepo(core, CollectionGuid, core.codeVersion(), ref ErrorMessage, "", isNewBuild, repair, ref nonCriticalErrorList);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                            } catch (Exception ex9) {
                                logController.handleError( core,ex9);
                            }
                        }
                    }
                    //
                    //---------------------------------------------------------------------
                    // ----- Explain, put up a link and exit without continuing
                    //---------------------------------------------------------------------
                    //
                    core.cache.invalidateAll();
                    logController.logInfo(core, "Upgrade Complete");
                    core.doc.upgradeInProgress = false;
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        private static void VerifyAdminMenus(CoreController core, string DataBuildVersion) {
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
                        FieldNew = genericController.encodeText(dt.Rows[rowptr]["name"]) + "." + genericController.encodeText(dt.Rows[rowptr]["parentid"]);
                        if (FieldNew == FieldLast) {
                            FieldRecordID = genericController.encodeInteger(dt.Rows[rowptr]["ID"]);
                            core.db.executeQuery("Update ccMenuEntries set active=0 where ID=" + FieldRecordID + ";");
                        }
                        FieldLast = FieldNew;
                    }
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        // Get the Menu for FormInputHTML
        //
        private static void VerifyRecord(CoreController core, string ContentName, string Name, string CodeFieldName = "", string Code = "", bool InActive = false) {
            try {
                bool Active = false;
                DataTable dt = null;
                string sql1 = null;
                string sql2 = null;
                string sql3 = null;
                //
                Active = !InActive;
                Models.Complex.cdefModel cdef = Models.Complex.cdefModel.getCdef(core, ContentName);
                string tableName = cdef.contentTableName;
                int cid = cdef.id;
                //
                dt = core.db.executeQuery("SELECT ID FROM " + tableName + " WHERE NAME=" + core.db.encodeSQLText(Name) + ";");
                if (dt.Rows.Count == 0) {
                    sql1 = "insert into " + tableName + " (contentcontrolid,createkey,active,name";
                    sql2 = ") values (" + cid + ",0," + core.db.encodeSQLBoolean(Active) + "," + core.db.encodeSQLText(Name);
                    sql3 = ")";
                    if (!string.IsNullOrEmpty(CodeFieldName)) {
                        sql1 += "," + CodeFieldName;
                        sql2 += "," + Code;
                    }
                    core.db.executeQuery(sql1 + sql2 + sql3);
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// gaurantee db fields meet minimum requirements. Like dateTime precision
        /// </summary>
        /// <param name="core"></param>
        /// <param name="DataBuildVersion"></param>
        private static void VerifySqlfieldCompatibility(CoreController core) {
            try {
                //
                // verify Db field schema for fields handled internally (fix datatime2(0) problem -- need at least 3 digits for precision)
                var tableList = Models.DbModels.tableModel.createList(core, "");
                foreach (tableModel table in tableList) {
                    var tableSchema = Models.Complex.TableSchemaModel.getTableSchema(core, table.name, "");
                    if (tableSchema != null) {
                        foreach (Models.Complex.TableSchemaModel.ColumnSchemaModel column in tableSchema.columns) {
                            if ((column.DATA_TYPE.ToLower() == "datetime2") & (column.DATETIME_PRECISION < 3)) {
                                //
                                logController.logInfo(core, "verifySqlFieldCompatibility, conversion required, table [" + table.name + "], field [" + column.COLUMN_NAME + "], reason [datetime precision too low (" + column.DATETIME_PRECISION.ToString() + ")]");
                                //
                                // drop any indexes that use this field
                                bool indexDropped = false;
                                foreach( Models.Complex.TableSchemaModel.IndexSchemaModel index in tableSchema.indexes) {
                                    if ( index.indexKeyList.Contains(column.COLUMN_NAME) ) {
                                        //
                                        logController.logInfo(core, "verifySqlFieldCompatibility, index [" + index.index_name + "] must be dropped");
                                        core.db.deleteSqlIndex("", table.name, index.index_name);
                                        indexDropped = true;
                                        //
                                    }
                                }
                                //
                                // -- datetime2(0)...datetime2(2) need to be converted to datetime2(7)
                                // -- rename column to tempName
                                string tempName = "tempDateTime" + genericController.GetRandomInteger(core).ToString();
                                core.db.executeNonQuery("sp_rename '" + table.name + "." + column.COLUMN_NAME + "', '" + tempName + "', 'COLUMN';");
                                core.db.executeNonQuery("ALTER TABLE " + table.name + " ADD " + column.COLUMN_NAME + " DateTime2(7) NULL;");
                                core.db.executeNonQuery("update " + table.name + " set " + column.COLUMN_NAME + "=" + tempName + " ");
                                core.db.executeNonQuery("ALTER TABLE " + table.name + " DROP COLUMN " + tempName + ";");
                                //
                                // recreate dropped indexes
                                if (indexDropped) {
                                    foreach (Models.Complex.TableSchemaModel.IndexSchemaModel index in tableSchema.indexes) {
                                        if (index.indexKeyList.Contains(column.COLUMN_NAME)) {
                                            //
                                            logController.logInfo(core, "verifySqlFieldCompatibility, recreating index [" + index.index_name + "]");
                                            core.db.createSQLIndex("", table.name, index.index_name, index.index_keys);
                                            //
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                logController.handleError(core, ex);
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// when breaking changes are required for data, update them here
        /// </summary>
        /// <param name="core"></param>
        /// <param name="DataBuildVersion"></param>
        private static void Upgrade_Conversion(CoreController core, string DataBuildVersion) {
            try {
                //
                // -- Roll the style sheet cache if it is setup
                core.siteProperties.setProperty("StylesheetSerialNumber", (-1).ToString());
                //
                // -- Reload
                core.cache.invalidateAll();
                core.doc.clearMetaData();
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //=========================================================================================
        //
        //
        public static void VerifyScriptingRecords(CoreController core) {
            try {
                //
                appendUpgradeLogAddStep(core, core.appConfig.name, "VerifyScriptingRecords", "Verify Scripting Records.");
                //
                VerifyRecord(core, "Scripting Languages", "VBScript", "", "");
                VerifyRecord(core, "Scripting Languages", "JScript", "", "");
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //=========================================================================================
        //
        public static void VerifyLanguageRecords(CoreController core) {
            try {
                //
                appendUpgradeLogAddStep(core, core.appConfig.name, "VerifyLanguageRecords", "Verify Language Records.");
                //
                VerifyRecord(core, "Languages", "English", "HTTP_Accept_Language", "'en'");
                VerifyRecord(core, "Languages", "Spanish", "HTTP_Accept_Language", "'es'");
                VerifyRecord(core, "Languages", "French", "HTTP_Accept_Language", "'fr'");
                VerifyRecord(core, "Languages", "Any", "HTTP_Accept_Language", "'any'");
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //   Verify Library Folder records
        //
        private static void VerifyLibraryFolders(CoreController core) {
            try {
                DataTable dt = null;
                //
                appendUpgradeLogAddStep(core, core.appConfig.name, "VerifyLibraryFolders", "Verify Library Folders: Images and Downloads");
                //
                dt = core.db.executeQuery("select id from cclibraryfiles");
                if (dt.Rows.Count == 0) {
                    VerifyRecord(core, "Library Folders", "Images");
                    VerifyRecord(core, "Library Folders", "Downloads");
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //=============================================================================
        //
        private static int GetIDBYName(CoreController core, string TableName, string RecordName) {
            int tempGetIDBYName = 0;
            int returnid = 0;
            try {
                //
                DataTable rs;
                //
                rs = core.db.executeQuery("Select ID from " + TableName + " where name=" + core.db.encodeSQLText(RecordName));
                if (DbController.isDataTableOk(rs)) {
                    tempGetIDBYName = genericController.encodeInteger(rs.Rows[0]["ID"]);
                }
                rs.Dispose();
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
            return returnid;
        }
        //
        //   Verify Library Folder records
        //
        private static void VerifyLibraryFileTypes(CoreController core) {
            try {
                //
                // Load basic records -- default images are handled in the REsource Library through the /ccLib/config/DefaultValues.txt GetDefaultValue(key) mechanism
                //
                if (core.db.getRecordID("Library File Types", "Image") == 0) {
                    VerifyRecord(core, "Library File Types", "Image", "ExtensionList", "'GIF,JPG,JPE,JPEG,BMP,PNG'", false);
                    VerifyRecord(core, "Library File Types", "Image", "IsImage", "1", false);
                    VerifyRecord(core, "Library File Types", "Image", "IsVideo", "0", false);
                    VerifyRecord(core, "Library File Types", "Image", "IsDownload", "1", false);
                    VerifyRecord(core, "Library File Types", "Image", "IsFlash", "0", false);
                }
                //
                if (core.db.getRecordID("Library File Types", "Video") == 0) {
                    VerifyRecord(core, "Library File Types", "Video", "ExtensionList", "'ASX,AVI,WMV,MOV,MPG,MPEG,MP4,QT,RM'", false);
                    VerifyRecord(core, "Library File Types", "Video", "IsImage", "0", false);
                    VerifyRecord(core, "Library File Types", "Video", "IsVideo", "1", false);
                    VerifyRecord(core, "Library File Types", "Video", "IsDownload", "1", false);
                    VerifyRecord(core, "Library File Types", "Video", "IsFlash", "0", false);
                }
                //
                if (core.db.getRecordID("Library File Types", "Audio") == 0) {
                    VerifyRecord(core, "Library File Types", "Audio", "ExtensionList", "'AIF,AIFF,ASF,CDA,M4A,M4P,MP2,MP3,MPA,WAV,WMA'", false);
                    VerifyRecord(core, "Library File Types", "Audio", "IsImage", "0", false);
                    VerifyRecord(core, "Library File Types", "Audio", "IsVideo", "0", false);
                    VerifyRecord(core, "Library File Types", "Audio", "IsDownload", "1", false);
                    VerifyRecord(core, "Library File Types", "Audio", "IsFlash", "0", false);
                }
                //
                if (core.db.getRecordID("Library File Types", "Word") == 0) {
                    VerifyRecord(core, "Library File Types", "Word", "ExtensionList", "'DOC'", false);
                    VerifyRecord(core, "Library File Types", "Word", "IsImage", "0", false);
                    VerifyRecord(core, "Library File Types", "Word", "IsVideo", "0", false);
                    VerifyRecord(core, "Library File Types", "Word", "IsDownload", "1", false);
                    VerifyRecord(core, "Library File Types", "Word", "IsFlash", "0", false);
                }
                //
                if (core.db.getRecordID("Library File Types", "Flash") == 0) {
                    VerifyRecord(core, "Library File Types", "Flash", "ExtensionList", "'SWF'", false);
                    VerifyRecord(core, "Library File Types", "Flash", "IsImage", "0", false);
                    VerifyRecord(core, "Library File Types", "Flash", "IsVideo", "0", false);
                    VerifyRecord(core, "Library File Types", "Flash", "IsDownload", "1", false);
                    VerifyRecord(core, "Library File Types", "Flash", "IsFlash", "1", false);
                }
                //
                if (core.db.getRecordID("Library File Types", "PDF") == 0) {
                    VerifyRecord(core, "Library File Types", "PDF", "ExtensionList", "'PDF'", false);
                    VerifyRecord(core, "Library File Types", "PDF", "IsImage", "0", false);
                    VerifyRecord(core, "Library File Types", "PDF", "IsVideo", "0", false);
                    VerifyRecord(core, "Library File Types", "PDF", "IsDownload", "1", false);
                    VerifyRecord(core, "Library File Types", "PDF", "IsFlash", "0", false);
                }
                //
                if (core.db.getRecordID("Library File Types", "XLS") == 0) {
                    VerifyRecord(core, "Library File Types", "Excel", "ExtensionList", "'XLS'", false);
                    VerifyRecord(core, "Library File Types", "Excel", "IsImage", "0", false);
                    VerifyRecord(core, "Library File Types", "Excel", "IsVideo", "0", false);
                    VerifyRecord(core, "Library File Types", "Excel", "IsDownload", "1", false);
                    VerifyRecord(core, "Library File Types", "Excel", "IsFlash", "0", false);
                }
                //
                if (core.db.getRecordID("Library File Types", "PPT") == 0) {
                    VerifyRecord(core, "Library File Types", "Power Point", "ExtensionList", "'PPT,PPS'", false);
                    VerifyRecord(core, "Library File Types", "Power Point", "IsImage", "0", false);
                    VerifyRecord(core, "Library File Types", "Power Point", "IsVideo", "0", false);
                    VerifyRecord(core, "Library File Types", "Power Point", "IsDownload", "1", false);
                    VerifyRecord(core, "Library File Types", "Power Point", "IsFlash", "0", false);
                }
                //
                if (core.db.getRecordID("Library File Types", "Default") == 0) {
                    VerifyRecord(core, "Library File Types", "Default", "ExtensionList", "''", false);
                    VerifyRecord(core, "Library File Types", "Default", "IsImage", "0", false);
                    VerifyRecord(core, "Library File Types", "Default", "IsVideo", "0", false);
                    VerifyRecord(core, "Library File Types", "Default", "IsDownload", "1", false);
                    VerifyRecord(core, "Library File Types", "Default", "IsFlash", "0", false);
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //=========================================================================================
        //
        private static void VerifyState(CoreController core, string Name, string Abbreviation, double SaleTax, int CountryID, string FIPSState) {
            try {
                //
                int CS = 0;
                const string ContentName = "States";
                //
                CS = core.db.csOpen(ContentName, "name=" + core.db.encodeSQLText(Name),"", false);
                if (!core.db.csOk(CS)) {
                    //
                    // create new record
                    //
                    core.db.csClose(ref CS);
                    CS = core.db.csInsertRecord(ContentName, SystemMemberID);
                    core.db.csSet(CS, "NAME", Name);
                    core.db.csSet(CS, "ACTIVE", true);
                    core.db.csSet(CS, "Abbreviation", Abbreviation);
                    core.db.csSet(CS, "CountryID", CountryID);
                    core.db.csSet(CS, "FIPSState", FIPSState);
                } else {
                    //
                    // verify only fields needed for contensive
                    //
                    core.db.csSet(CS, "CountryID", CountryID);
                    core.db.csSet(CS, "Abbreviation", Abbreviation);
                }
                core.db.csClose(ref CS);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //=========================================================================================
        //
        public static void VerifyStates(CoreController core) {
            try {
                //
                appendUpgradeLogAddStep(core, core.appConfig.name, "VerifyStates", "Verify States");
                //
                int CountryID = 0;
                //
                VerifyCountry(core, "United States", "US");
                CountryID = core.db.getRecordID("Countries", "United States");
                //
                VerifyState(core, "Alaska", "AK", 0.0D, CountryID, "");
                VerifyState(core, "Alabama", "AL", 0.0D, CountryID, "");
                VerifyState(core, "Arizona", "AZ", 0.0D, CountryID, "");
                VerifyState(core, "Arkansas", "AR", 0.0D, CountryID, "");
                VerifyState(core, "California", "CA", 0.0D, CountryID, "");
                VerifyState(core, "Connecticut", "CT", 0.0D, CountryID, "");
                VerifyState(core, "Colorado", "CO", 0.0D, CountryID, "");
                VerifyState(core, "Delaware", "DE", 0.0D, CountryID, "");
                VerifyState(core, "District of Columbia", "DC", 0.0D, CountryID, "");
                VerifyState(core, "Florida", "FL", 0.0D, CountryID, "");
                VerifyState(core, "Georgia", "GA", 0.0D, CountryID, "");

                VerifyState(core, "Hawaii", "HI", 0.0D, CountryID, "");
                VerifyState(core, "Idaho", "ID", 0.0D, CountryID, "");
                VerifyState(core, "Illinois", "IL", 0.0D, CountryID, "");
                VerifyState(core, "Indiana", "IN", 0.0D, CountryID, "");
                VerifyState(core, "Iowa", "IA", 0.0D, CountryID, "");
                VerifyState(core, "Kansas", "KS", 0.0D, CountryID, "");
                VerifyState(core, "Kentucky", "KY", 0.0D, CountryID, "");
                VerifyState(core, "Louisiana", "LA", 0.0D, CountryID, "");
                VerifyState(core, "Massachusetts", "MA", 0.0D, CountryID, "");
                VerifyState(core, "Maine", "ME", 0.0D, CountryID, "");

                VerifyState(core, "Maryland", "MD", 0.0D, CountryID, "");
                VerifyState(core, "Michigan", "MI", 0.0D, CountryID, "");
                VerifyState(core, "Minnesota", "MN", 0.0D, CountryID, "");
                VerifyState(core, "Missouri", "MO", 0.0D, CountryID, "");
                VerifyState(core, "Mississippi", "MS", 0.0D, CountryID, "");
                VerifyState(core, "Montana", "MT", 0.0D, CountryID, "");
                VerifyState(core, "North Carolina", "NC", 0.0D, CountryID, "");
                VerifyState(core, "Nebraska", "NE", 0.0D, CountryID, "");
                VerifyState(core, "New Hampshire", "NH", 0.0D, CountryID, "");
                VerifyState(core, "New Mexico", "NM", 0.0D, CountryID, "");

                VerifyState(core, "New Jersey", "NJ", 0.0D, CountryID, "");
                VerifyState(core, "New York", "NY", 0.0D, CountryID, "");
                VerifyState(core, "Nevada", "NV", 0.0D, CountryID, "");
                VerifyState(core, "North Dakota", "ND", 0.0D, CountryID, "");
                VerifyState(core, "Ohio", "OH", 0.0D, CountryID, "");
                VerifyState(core, "Oklahoma", "OK", 0.0D, CountryID, "");
                VerifyState(core, "Oregon", "OR", 0.0D, CountryID, "");
                VerifyState(core, "Pennsylvania", "PA", 0.0D, CountryID, "");
                VerifyState(core, "Rhode Island", "RI", 0.0D, CountryID, "");
                VerifyState(core, "South Carolina", "SC", 0.0D, CountryID, "");

                VerifyState(core, "South Dakota", "SD", 0.0D, CountryID, "");
                VerifyState(core, "Tennessee", "TN", 0.0D, CountryID, "");
                VerifyState(core, "Texas", "TX", 0.0D, CountryID, "");
                VerifyState(core, "Utah", "UT", 0.0D, CountryID, "");
                VerifyState(core, "Vermont", "VT", 0.0D, CountryID, "");
                VerifyState(core, "Virginia", "VA", 0.045, CountryID, "");
                VerifyState(core, "Washington", "WA", 0.0D, CountryID, "");
                VerifyState(core, "Wisconsin", "WI", 0.0D, CountryID, "");
                VerifyState(core, "West Virginia", "WV", 0.0D, CountryID, "");
                VerifyState(core, "Wyoming", "WY", 0.0D, CountryID, "");
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //
        private static void VerifyCountry(CoreController core, string Name, string Abbreviation) {
            try {
                int CS;
                //
                CS = core.db.csOpen("Countries", "name=" + core.db.encodeSQLText(Name));
                if (!core.db.csOk(CS)) {
                    core.db.csClose(ref CS);
                    CS = core.db.csInsertRecord("Countries", SystemMemberID);
                    if (core.db.csOk(CS)) {
                        core.db.csSet(CS, "ACTIVE", true);
                    }
                }
                if (core.db.csOk(CS)) {
                    core.db.csSet(CS, "NAME", Name);
                    core.db.csSet(CS, "Abbreviation", Abbreviation);
                    if (genericController.vbLCase(Name) == "united states") {
                        core.db.csSet(CS, "DomesticShipping", "1");
                    }
                }
                core.db.csClose(ref CS);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //=========================================================================================
        //
        public static void VerifyCountries(CoreController core) {
            try {
                //
                appendUpgradeLogAddStep(core, core.appConfig.name, "VerifyCountries", "Verify Countries");
                //
                string list = core.appRootFiles.readFileText("cclib\\config\\isoCountryList.txt");
                string[] rows  = genericController.stringSplit(list, "\r\n");
                foreach( var row in rows) {
                    if (!string.IsNullOrEmpty(row)) {
                        string[] attrs = row.Split(';');
                        foreach (var attr in attrs) {
                            VerifyCountry(core, encodeInitialCaps(attr), attrs[1]);
                        }
                    }
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //=========================================================================================
        //
        //=========================================================================================
        //
        public static void VerifyDefaultGroups(CoreController core) {
            try {
                //
                int GroupID = 0;
                string SQL = null;
                //
                appendUpgradeLogAddStep(core, core.appConfig.name, "VerifyDefaultGroups", "Verify Default Groups");
                //
                GroupID = groupController.group_add(core, "Site Managers");
                SQL = "Update ccContent Set EditorGroupID=" + core.db.encodeSQLNumber(GroupID) + " where EditorGroupID is null;";
                core.db.executeQuery(SQL);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //
        //===================================================================================================================
        //   Verify all the core tables
        //       Moved to Sub because if an Addon upgrade from another site on the server distributes the
        //       CDef changes, this site could have to update it's ccContent and ccField records, and
        //       it will fail if they are not up to date.
        //===================================================================================================================
        //
        internal static void VerifyBasicTables(CoreController core) {
            try {
                //
                if (!false) {
                    appendUpgradeLogAddStep(core, core.appConfig.name, "VerifyCoreTables", "Verify Core SQL Tables");
                    //
                    core.db.createSQLTable("Default", "ccDataSources");
                    core.db.createSQLTableField("Default", "ccDataSources", "typeId", FieldTypeIdInteger);
                    core.db.createSQLTableField("Default", "ccDataSources", "address", FieldTypeIdText);
                    core.db.createSQLTableField("Default", "ccDataSources", "username", FieldTypeIdText);
                    core.db.createSQLTableField("Default", "ccDataSources", "password", FieldTypeIdText);
                    core.db.createSQLTableField("Default", "ccDataSources", "ConnString", FieldTypeIdText);
                    core.db.createSQLTableField("Default", "ccDataSources", "endpoint", FieldTypeIdText);
                    core.db.createSQLTableField("Default", "ccDataSources", "dbtypeid", FieldTypeIdLookup);
                    //
                    core.db.createSQLTable("Default", "ccTables");
                    core.db.createSQLTableField("Default", "ccTables", "DataSourceID", FieldTypeIdLookup);
                    //
                    core.db.createSQLTable("Default", "ccContent");
                    core.db.createSQLTableField("Default", "ccContent", "ContentTableID", FieldTypeIdInteger);
                    core.db.createSQLTableField("Default", "ccContent", "AuthoringTableID", FieldTypeIdInteger);
                    core.db.createSQLTableField("Default", "ccContent", "AllowAdd", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccContent", "AllowDelete", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccContent", "AllowWorkflowAuthoring", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccContent", "DeveloperOnly", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccContent", "AdminOnly", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccContent", "ParentID", FieldTypeIdInteger);
                    core.db.createSQLTableField("Default", "ccContent", "DefaultSortMethodID", FieldTypeIdInteger);
                    core.db.createSQLTableField("Default", "ccContent", "DropDownFieldList", FieldTypeIdText);
                    core.db.createSQLTableField("Default", "ccContent", "EditorGroupID", FieldTypeIdInteger);
                    core.db.createSQLTableField("Default", "ccContent", "AllowCalendarEvents", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccContent", "AllowContentTracking", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccContent", "AllowTopicRules", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccContent", "AllowContentChildTool", FieldTypeIdBoolean);
                    //Call core.db.createSQLTableField("Default", "ccContent", "AllowMetaContent", FieldTypeIdBoolean)
                    core.db.createSQLTableField("Default", "ccContent", "IconLink", FieldTypeIdLink);
                    core.db.createSQLTableField("Default", "ccContent", "IconHeight", FieldTypeIdInteger);
                    core.db.createSQLTableField("Default", "ccContent", "IconWidth", FieldTypeIdInteger);
                    core.db.createSQLTableField("Default", "ccContent", "IconSprites", FieldTypeIdInteger);
                    core.db.createSQLTableField("Default", "ccContent", "installedByCollectionId", FieldTypeIdInteger);
                    //Call core.app.csv_CreateSQLTableField("Default", "ccContent", "ccGuid", FieldTypeText)
                    core.db.createSQLTableField("Default", "ccContent", "IsBaseContent", FieldTypeIdBoolean);
                    //Call core.app.csv_CreateSQLTableField("Default", "ccContent", "WhereClause", FieldTypeText)
                    //
                    core.db.createSQLTable("Default", "ccFields");
                    core.db.createSQLTableField("Default", "ccFields", "ContentID", FieldTypeIdInteger);
                    core.db.createSQLTableField("Default", "ccFields", "Type", FieldTypeIdInteger);
                    core.db.createSQLTableField("Default", "ccFields", "Caption", FieldTypeIdText);
                    core.db.createSQLTableField("Default", "ccFields", "ReadOnly", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccFields", "NotEditable", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccFields", "LookupContentID", FieldTypeIdInteger);
                    core.db.createSQLTableField("Default", "ccFields", "RedirectContentID", FieldTypeIdInteger);
                    core.db.createSQLTableField("Default", "ccFields", "RedirectPath", FieldTypeIdText);
                    core.db.createSQLTableField("Default", "ccFields", "RedirectID", FieldTypeIdText);
                    core.db.createSQLTableField("Default", "ccFields", "HelpMessage", FieldTypeIdLongText); // deprecated but Im chicken to remove this
                    core.db.createSQLTableField("Default", "ccFields", "UniqueName", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccFields", "TextBuffered", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccFields", "Password", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccFields", "IndexColumn", FieldTypeIdInteger);
                    core.db.createSQLTableField("Default", "ccFields", "IndexWidth", FieldTypeIdText);
                    core.db.createSQLTableField("Default", "ccFields", "IndexSortPriority", FieldTypeIdInteger);
                    core.db.createSQLTableField("Default", "ccFields", "IndexSortDirection", FieldTypeIdInteger);
                    core.db.createSQLTableField("Default", "ccFields", "EditSortPriority", FieldTypeIdInteger);
                    core.db.createSQLTableField("Default", "ccFields", "AdminOnly", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccFields", "DeveloperOnly", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccFields", "DefaultValue", FieldTypeIdText);
                    core.db.createSQLTableField("Default", "ccFields", "Required", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccFields", "HTMLContent", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccFields", "Authorable", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccFields", "ManyToManyContentID", FieldTypeIdInteger);
                    core.db.createSQLTableField("Default", "ccFields", "ManyToManyRuleContentID", FieldTypeIdInteger);
                    core.db.createSQLTableField("Default", "ccFields", "ManyToManyRulePrimaryField", FieldTypeIdText);
                    core.db.createSQLTableField("Default", "ccFields", "ManyToManyRuleSecondaryField", FieldTypeIdText);
                    core.db.createSQLTableField("Default", "ccFields", "RSSTitleField", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccFields", "RSSDescriptionField", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccFields", "MemberSelectGroupID", FieldTypeIdInteger);
                    core.db.createSQLTableField("Default", "ccFields", "EditTab", FieldTypeIdText);
                    core.db.createSQLTableField("Default", "ccFields", "Scramble", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccFields", "LookupList", FieldTypeIdText);
                    core.db.createSQLTableField("Default", "ccFields", "IsBaseField", FieldTypeIdBoolean);
                    core.db.createSQLTableField("Default", "ccFields", "installedByCollectionId", FieldTypeIdInteger);
                    //
                    core.db.createSQLTable("Default", "ccFieldHelp");
                    core.db.createSQLTableField("Default", "ccFieldHelp", "FieldID", FieldTypeIdLookup);
                    core.db.createSQLTableField("Default", "ccFieldHelp", "HelpDefault", FieldTypeIdLongText);
                    core.db.createSQLTableField("Default", "ccFieldHelp", "HelpCustom", FieldTypeIdLongText);
                    //
                    core.db.createSQLTable("Default", "ccSetup");
                    core.db.createSQLTableField("Default", "ccSetup", "FieldValue", FieldTypeIdText);
                    core.db.createSQLTableField("Default", "ccSetup", "DeveloperOnly", FieldTypeIdBoolean);
                    //
                    core.db.createSQLTable("Default", "ccSortMethods");
                    core.db.createSQLTableField("Default", "ccSortMethods", "OrderByClause", FieldTypeIdText);
                    //
                    core.db.createSQLTable("Default", "ccFieldTypes");
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //===========================================================================
        //   Append Log File
        private static void appendUpgradeLog(CoreController core, string appName, string Method, string Message) {
            logController.logInfo(core, "app [" + appName + "], Method [" + Method + "], Message [" + Message + "]");
        }
        //
        //=============================================================================
        //   Get a ContentID from the ContentName using just the tables
        //=============================================================================
        //
        private static void appendUpgradeLogAddStep(CoreController core, string appName, string Method, string Message) {
            appendUpgradeLog(core, appName, Method, Message);
        }
        //
        //=============================================================================
        //   Verify an Admin Navigator Entry
        //       Entries are unique by their ccGuid
        //       Includes InstalledByCollectionID
        //       returns the entry id
        //=============================================================================
        //
        public static int verifyNavigatorEntry(CoreController core, string ccGuid, string menuNameSpace, string EntryName, string ContentName, string LinkPage, string SortOrder, bool AdminOnly, bool DeveloperOnly, bool NewWindow, bool Active, string AddonName, string NavIconType, string NavIconTitle, int InstalledByCollectionID) {
            int returnEntry = 0;
            try {
                if (!string.IsNullOrEmpty(EntryName.Trim())) {
                    int addonId = core.db.getRecordID(cnAddons, AddonName);
                    int parentId = verifyNavigatorEntry_getParentIdFromNameSpace(core, menuNameSpace);
                    int contentId = Models.Complex.cdefModel.getContentId(core, ContentName);
                    string listCriteria = "(name=" + core.db.encodeSQLText(EntryName) + ")and(Parentid=" + parentId + ")";
                    List<Models.DbModels.NavigatorEntryModel> entryList = NavigatorEntryModel.createList(core, listCriteria, "id");
                    NavigatorEntryModel entry = null;
                    if (entryList.Count == 0) {
                        entry = NavigatorEntryModel.add(core);
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
                logController.handleError( core,ex);
                throw;
            }
            return returnEntry;
        }
             //
        public static int verifyNavigatorEntry_getParentIdFromNameSpace(CoreController core, string menuNameSpace) {
            int parentRecordId = 0;
            try {
                if (!string.IsNullOrEmpty(menuNameSpace.Trim())) {
                    string[] parents = menuNameSpace.Trim().Split('.');
                    foreach( var parent in parents ) {
                        string recordName = parent.Trim();
                        if (!string.IsNullOrEmpty(recordName)) {
                            string Criteria = "(name=" + core.db.encodeSQLText(recordName) + ")";
                            if (parentRecordId == 0) {
                                Criteria += "and((Parentid is null)or(Parentid=0))";
                            } else {
                                Criteria += "and(Parentid=" + parentRecordId + ")";
                            }
                            int RecordID = 0;
                            int CS = core.db.csOpen(cnNavigatorEntries, Criteria, "ID", true, 0, false, false, "ID", 1);
                            if (core.db.csOk(CS)) {
                                RecordID = (core.db.csGetInteger(CS, "ID"));
                            }
                            core.db.csClose(ref CS);
                            if (RecordID == 0) {
                                CS = core.db.csInsertRecord(cnNavigatorEntries, SystemMemberID);
                                if (core.db.csOk(CS)) {
                                    RecordID = core.db.csGetInteger(CS, "ID");
                                    core.db.csSet(CS, "name", recordName);
                                    core.db.csSet(CS, "parentID", parentRecordId);
                                }
                                core.db.csClose(ref CS);
                            }
                            parentRecordId = RecordID;
                        }
                    }
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
            return parentRecordId;
        }
    }
}
