
using System;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.Processor.Models.Domain;

namespace Contensive.Processor.Addons.Housekeeping {
    //
    public static class UserProperyClass {

        //====================================================================================================
        /// <summary>
        /// delete orphan user properties
        /// </summary>
        /// <param name="core"></param>
        public static void housekeep(CoreController core) {
            try {
                //string sqlInner = "select p.id from ccProperties p left join ccmembers m on m.id=p.KeyID where (p.TypeID=" + PropertyModelClass.PropertyTypeEnum.user + ") and (m.ID is null)";
                string sql = "delete from ccProperties from ccProperties p left join ccmembers m on m.id=p.KeyID where (p.TypeID=" + (int)PropertyModelClass.PropertyTypeEnum.user + ") and (m.ID is null)";
                core.db.executeNonQuery(sql);
                //
                // Member Properties with no member
                //
                LogController.logInfo(core, "Deleting member properties with no member record.");
                sql = "delete ccProperties"
                    + " From ccProperties LEFT JOIN ccmembers on ccmembers.ID=ccProperties.KeyID"
                    + " WHERE (ccProperties.TypeID=0)"
                    + " AND (ccmembers.ID is null)";
                core.db.executeNonQuery(sql);

            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
        }
        //
    }
}