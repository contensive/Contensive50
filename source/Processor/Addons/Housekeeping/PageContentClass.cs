
using System;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.Processor;
using System.Xml;
using Contensive.Exceptions;

namespace Contensive.Processor.Addons.Housekeeping {
    //
    public static class PageContentClass {

        //====================================================================================================
        //
        public static void housekeep(CoreController core, HouseKeepEnvironmentModel env) {
            try {
                string SQL = "";
                {
                    //
                    // Move Archived pages from their current parent to their archive parent
                    //
                    bool NeedToClearCache = false;
                    LogController.logInfo(core, "Archive update for pages on [" + core.appConfig.name + "]");
                    SQL = "select * from ccpagecontent where (( DateArchive is not null )and(DateArchive<" + env.sQLNow + "))and(active<>0)";
                    using (var csData = new CsModel(core)) {
                        csData.openSql(SQL, "Default");
                        while (csData.ok()) {
                            int RecordId = csData.getInteger("ID");
                            int ArchiveParentId = csData.getInteger("ArchiveParentID");
                            if (ArchiveParentId == 0) {
                                SQL = "update ccpagecontent set DateArchive=null where (id=" + RecordId + ")";
                                core.db.executeQuery(SQL);
                            } else {
                                SQL = "update ccpagecontent set ArchiveParentID=null,DateArchive=null,parentid=" + ArchiveParentId + " where (id=" + RecordId + ")";
                                core.db.executeQuery(SQL);
                                NeedToClearCache = true;
                            }
                            csData.goNext();
                        }
                        csData.close();
                    }
                    //
                    // Clear caches
                    //
                    if (NeedToClearCache) {
                        object emptyData = null;
                        core.cache.invalidate("Page Content");
                        core.cache.storeObject("PCC", emptyData);
                    }
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
        }
    }
}