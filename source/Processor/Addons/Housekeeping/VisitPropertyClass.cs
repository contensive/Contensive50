
using System;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.Processor.Models.Domain;

namespace Contensive.Addons.Housekeeping {
    //
    public static class VisitPropertyClass {

        //
        //====================================================================================================
        /// <summary>
        /// Delete stale visit properties (older than 24 hrs)
        /// </summary>
        /// <param name="core"></param>
        public static void housekeep(CoreController core) {
            try {
            //
            // old Properties
            string sql = "delete from ccProperties where (TypeID=" + PropertyModelClass.PropertyTypeEnum.visit + ")and(dateAdded<" + DbController.encodeSQLDate(DateTime.Now.AddDays(-1)) + ")";
            core.db.executeNonQueryAsync(sql);
            //
            // Visit Properties with no visits
            LogController.logInfo(core, "Deleting visit properties with no visit record.");
            sql = "delete ccProperties"
                + " from ccProperties LEFT JOIN ccVisits on ccVisits.ID=ccProperties.KeyID"
                + " WHERE (ccProperties.TypeID=" + (int)PropertyModelClass.PropertyTypeEnum.visit + ")"
                + " AND (ccVisits.ID is null)";
            core.db.executeQuery(sql);

            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
        }
        //
    }
}