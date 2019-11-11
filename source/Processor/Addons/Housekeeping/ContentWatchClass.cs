
using System;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.Processor;

namespace Contensive.Processor.Addons.Housekeeping {
    //
    public static class ContentWatchClass {
        public static void housekeep(CoreController core, HouseKeepEnvironmentModel env) {
            try {
                //
                // ContentWatch with bad CContentID
                //     must be deleted manually
                //
                LogController.logInfo(core, "Deleting Content Watch with bad ContentID.");
                using (var csData = new CsModel(core)) {
                    string sql = "Select ccContentWatch.ID"
                        + " From ccContentWatch LEFT JOIN ccContent on ccContent.ID=ccContentWatch.ContentID"
                        + " WHERE (ccContent.ID is null)or(ccContent.Active=0)or(ccContent.Active is null)";
                    csData.openSql(sql);
                    while (csData.ok()) {
                        MetadataController.deleteContentRecord(core, "Content Watch", csData.getInteger("ID"));
                        csData.goNext();
                    }
                }

            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
        }
    }
}