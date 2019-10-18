﻿
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

using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.Models.Db;
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
                LibraryFilesModel file = LibraryFilesModel.create<LibraryFilesModel>(core.cpParent, core.docProperties.getText(RequestNameDownloadFileGuid));
                if (file != null) {
                    //
                    // -- lookup record and set clicks
                    if (file != null) {
                        file.clicks += 1;
                        file.save(core.cpParent);
                        if (file.filename != "") {
                            //
                            // -- create log entry
                            LibraryFileLogModel log = LibraryFileLogModel.addEmpty<LibraryFileLogModel>(core.cpParent);
                            if (log != null) {
                                log.name = DateTime.Now.ToString() + " user [#" + core.session.user.name + ", " + core.session.user.name + "]";
                                log.fileId = file.id;
                                log.visitId = core.session.visit.id;
                                log.memberId = core.session.user.id;
                                log.fromUrl = core.webServer.requestPageReferer;
                                log.save(core.cpParent);
                            }
                            //
                            // -- and go
                            string link = GenericController.getCdnFileLink(core, file.filename);
                            return core.webServer.redirect(link, "Redirecting because the active download request variable is set to a valid Library Files record..");
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
