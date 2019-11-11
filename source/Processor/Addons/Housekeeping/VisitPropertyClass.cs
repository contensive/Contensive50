
using System;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.Processor.Models.Domain;
using System.Threading.Tasks;

namespace Contensive.Processor.Addons.Housekeeping {
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
                {
                    //
                    // -- delete properties of visits over 1 hour old
                    string sql = "delete from ccproperties from ccproperties p left join  ccvisits v on (v.id=p.KeyID and p.TypeID=1) where v.LastVisitTime<DATEADD(hour, -1, GETDATE())";
                    Task.Run(() => core.db.executeNonQueryAsync(sql));

                }
                {
                    //
                    // old Properties
                    string sql = "delete from ccProperties where (TypeID=" + (int)PropertyModelClass.PropertyTypeEnum.visit + ")and(dateAdded<" + DbController.encodeSQLDate(DateTime.Now.AddDays(-1)) + ")";
                    Task.Run(() => core.db.executeNonQueryAsync(sql));
                }
                {
                    //
                    // Visit Properties with no visits
                    LogController.logInfo(core, "Deleting visit properties with no visit record.");
                    string sql = "delete ccProperties"
                        + " from ccProperties LEFT JOIN ccVisits on ccVisits.ID=ccProperties.KeyID"
                        + " WHERE (ccProperties.TypeID=" + (int)PropertyModelClass.PropertyTypeEnum.visit + ")"
                        + " AND (ccVisits.ID is null)";
                    Task.Run(() => core.db.executeNonQueryAsync(sql));
                }

            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
        }
        //
    }
}