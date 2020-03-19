
using System;
using System.Collections.Generic;
using Contensive.Models.Db;

namespace Contensive.CLI {
    //
    static class AddRootCmd {
        //
        // ====================================================================================================
        /// <summary>
        /// help text for this command
        /// </summary>
        internal static readonly string helpText = ""
            + Environment.NewLine
            + Environment.NewLine + "--addRoot"
            + Environment.NewLine + "    Deletes the current user with username:root and creates a new developer user with un:root, pw:contensive, expiration:1 hr"
            + "";
        //
        // ====================================================================================================
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cpServer"></param>
        /// <param name="adminEmail"></param>
        /// <param name="adminPassword">the argument following the command</param>
        public static void execute(Processor.CPClass cpServer, string appName) {
            using (var cp = new Contensive.Processor.CPClass(appName)) {
                DbBaseModel.deleteRows<PersonModel>(cp, "(username=" + cp.Db.EncodeSQLText("root") + ")");
                var currentUser = DbBaseModel.addDefault<PersonModel>(cp);
                currentUser.name = "root";
                currentUser.email = "";
                currentUser.username = "root";
                currentUser.admin = true;
                currentUser.developer = true;
                currentUser.password = "contensive";
                currentUser.dateExpires = DateTime.Now.AddHours(1);
                currentUser.save(cp);
                return;
            }
        }
    }
}
