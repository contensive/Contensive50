
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Processor;
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
//
namespace Contensive.Addons.Primitives {
    public class DownloadClass : Contensive.BaseClasses.AddonBaseClass {
        //
        //====================================================================================================
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public override object Execute(Contensive.BaseClasses.CPBaseClass cp) {
            string result = "";
            try {
                CoreController core = ((CPClass)cp).core;
                //
                // -- Active Download hook
                LibraryFilesModel file = LibraryFilesModel.create(core, core.docProperties.getText(RequestNameDownloadFileGuid));
                //if (file == null) {
                //    //
                //    // -- compatibility mode, downloadid, this exposes all library files because it exposes the sequential id number
                //    int downloadId = core.docProperties.getInteger(RequestNameDownloadFileId);
                //    if ((downloadId > 0) && (core.siteProperties.getBoolean("Allow library file download by id", false))) {
                //        file = LibraryFilesModel.create(core, downloadId);
                //    }
                //}
                if (file != null) {
                    //
                    // -- lookup record and set clicks
                    if (file != null) {
                        file.clicks += 1;
                        file.save(core);
                        if (file.filename != "") {
                            //
                            // -- create log entry
                            LibraryFileLogModel log = LibraryFileLogModel.addEmpty(core);
                            if (log != null) {
                                log.name = DateTime.Now.ToString() + " user [#" + core.session.user.name + ", " + core.session.user.name + "]";
                                log.fileID = file.id;
                                log.visitID = core.session.visit.id;
                                log.memberID = core.session.user.id;
                                log.FromUrl = core.webServer.requestPageReferer;
                                log.save(core);
                            }
                            //
                            // -- and go
                            string link = GenericController.getCdnFileLink(core, file.filename);
                            //string link = core.webServer.requestProtocol + core.webServer.requestDomain + genericController.getCdnFileLink(core, file.Filename);
                            return core.webServer.redirect(link, "Redirecting because the active download request variable is set to a valid Library Files record. Library File Log has been appended.");
                        }
                    }
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
