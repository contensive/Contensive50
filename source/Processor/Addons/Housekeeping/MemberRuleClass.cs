
using System;
using Contensive.Processor.Controllers;

namespace Contensive.Processor.Addons.Housekeeping {
    //
    public static class MemberRuleClass {
        public static void housekeep(CoreController core, HouseKeepEnvironmentModel env) {
            try {
                //
                // -- delete rows with invalid columns
                core.db.executeNonQuery("delete from ccMemberRules where groupid is null or memberid is null");
                //
                // MemberRules with bad MemberID
                //
                LogController.logInfo(core, "Deleting Member Rules with bad MemberID.");
                string sql = "delete ccmemberrules"
                    + " From ccmemberrules"
                    + " LEFT JOIN ccmembers on ccmembers.ID=ccmemberrules.memberId"
                    + " WHERE (ccmembers.ID is null)";
                core.db.executeNonQuery(sql);
                //
                // MemberRules with bad GroupID
                //
                LogController.logInfo(core, "Deleting Member Rules with bad GroupID.");
                sql = "delete ccmemberrules"
                    + " From ccmemberrules"
                    + " LEFT JOIN ccgroups on ccgroups.ID=ccmemberrules.GroupID"
                    + " WHERE (ccgroups.ID is null)";
                core.db.executeNonQuery(sql);

            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
        }
    }
}