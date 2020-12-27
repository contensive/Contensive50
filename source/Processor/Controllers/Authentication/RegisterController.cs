
using Contensive.BaseClasses;
using Contensive.Models.Db;
using Contensive.Processor.Models.Domain;
using System;
using static Contensive.Processor.Constants;
using static Contensive.Processor.Controllers.GenericController;
//
namespace Contensive.Processor.Controllers {
    //
    //========================================================================
    /// <summary>
    /// Simple registration (join) form to setup the user's people record and if approporate, authenticate
    /// </summary>
    public static class RegisterController {
        //
        //========================================================================
        //
        public static void processRegisterForm(CoreController core) {
            try {
                string ErrorMessage = "";
                string FirstName = null;
                string LastName = null;
                string FullName = null;
                string Email = null;
                int errorCode = 0;
                //
                string loginForm_Username = "";
                string loginForm_Password = "";
                loginForm_Username = core.docProperties.getText("username");
                loginForm_Password = core.docProperties.getText("password");
                //
                if (!GenericController.encodeBoolean(core.siteProperties.getBoolean("AllowMemberJoin", false))) {
                    ErrorController.addUserError(core, "This site does not accept public main_MemberShip.");
                } else {
                    if (!core.session.isNewCredentialOK(loginForm_Username, loginForm_Password, ref ErrorMessage, ref errorCode)) {
                        ErrorController.addUserError(core, ErrorMessage);
                    } else {
                        if (!(!core.doc.userErrorList.Count.Equals(0))) {
                            using (var csData = new CsModel(core)) {
                                csData.open("people", "ID=" + DbController.encodeSQLNumber(core.session.user.id));
                                if (!csData.ok()) {
                                    LogController.logError(core, new Exception("Could not open the current members account to set the username and password."));
                                } else {
                                    if ((!string.IsNullOrEmpty(csData.getText("username"))) || !string.IsNullOrEmpty(csData.getText("password")) || csData.getBoolean("admin") || csData.getBoolean("developer")) {
                                        //
                                        // if the current account can be logged into, you can not join 'into' it
                                        //
                                        core.session.logout();
                                    }
                                    FirstName = core.docProperties.getText("firstname");
                                    LastName = core.docProperties.getText("lastname");
                                    FullName = FirstName + " " + LastName;
                                    Email = core.docProperties.getText("email");
                                    csData.set("FirstName", FirstName);
                                    csData.set("LastName", LastName);
                                    csData.set("Name", FullName);
                                    csData.set("username", loginForm_Username);
                                    csData.set("password", loginForm_Password);
                                    core.session.authenticateById(core.session.user.id, core.session);
                                }
                                csData.close();
                            }
                        }
                    }
                }
                PersonModel.invalidateCacheOfRecord<PersonModel>(core.cpParent, core.session.user.id);
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
        }
    }
}
