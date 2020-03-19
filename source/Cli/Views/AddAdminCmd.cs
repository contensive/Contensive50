
using System;
using System.Collections.Generic;
using Contensive.Models.Db;

namespace Contensive.CLI {
    //
    static class AddAdminCmd {
        //
        // ====================================================================================================
        /// <summary>
        /// help text for this command
        /// </summary>
        internal static readonly string helpText = ""
            + Environment.NewLine
            + Environment.NewLine + "--addAdmin adminEmail adminPassword"
            + Environment.NewLine + "    Create an admin account with email, username and password."
            + "";
        //
        // ====================================================================================================
        /// <summary>
        /// manage the task scheduler service
        /// </summary>
        /// <param name="cpServer"></param>
        /// <param name="adminEmail"></param>
        /// <param name="adminPassword">the argument following the command</param>
        public static void execute(Processor.CPClass cpServer, string appName, string adminEmail, string adminPassword) {
            using (var cp = new Contensive.Processor.CPClass(appName)) {
                string criteria = "(email=" + cp.Db.EncodeSQLText(adminEmail) + ")";
                List<PersonModel> adminUserList = DbBaseModel.createList<Models.Db.PersonModel>(cp, criteria);
                if (!adminUserList.Count.Equals(0)) {
                    foreach (PersonModel currentUser in adminUserList) {
                        currentUser.name = adminEmail;
                        currentUser.email = adminEmail;
                        currentUser.username = adminEmail;
                        currentUser.admin = true;
                        currentUser.password = adminPassword;
                        currentUser.save(cp);
                    }
                    return;
                }
                PersonModel newUser = DbBaseModel.addDefault<PersonModel>(cp);
                newUser.name = adminEmail;
                newUser.email = adminEmail;
                newUser.username = adminEmail;
                newUser.admin = true;
                newUser.password = adminPassword;
                newUser.save(cp);
                return;
            }
        }
    }
}
