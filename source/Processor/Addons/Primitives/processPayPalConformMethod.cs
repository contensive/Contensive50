
//using System;
//using System.Reflection;
//using System.Xml;
//using System.Diagnostics;
//using System.Linq;
//using System.Text.RegularExpressions;
//using System.Collections.Generic;
//using System.Data;
//using System.Data.SqlClient;
//using Contensive.Core;
//using Contensive.Processor.Models.DbModels;
//using Contensive.Processor.Controllers;
//using static Contensive.Processor.Controllers.genericController;
//using static Contensive.Processor.constants;
//using Contensive.Processor.Models.Complex;
////
//namespace Contensive.Addons.Primitives {
//    public class processPayPalConformMethodClass : Contensive.BaseClasses.AddonBaseClass {
//        //
//        //====================================================================================================
//        /// <summary>
//        /// getFieldEditorPreference remote method
//        /// </summary>
//        /// <param name="cp"></param>
//        /// <returns></returns>
//        public override object Execute(Contensive.BaseClasses.CPBaseClass cp) {
//            string result = "";
//            try {
//                CPClass processor = (CPClass)cp;
//                coreClass core = processor.core;
//                //
//                // -- Should be a remote method in commerce
//                int ConfirmOrderID = core.docProperties.getInteger("item_name");
//                if (ConfirmOrderID != 0) {
//                    //
//                    // Confirm the order
//                    //
//                    int CS = core.db.csOpen("Orders", "(ID=" + ConfirmOrderID + ") and ((OrderCompleted=0)or(OrderCompleted is Null))");
//                    if (core.db.csOk(CS)) {
//                        core.db.csSet(CS, "OrderCompleted", true);
//                        core.db.csSet(CS, "DateCompleted", core.doc.profileStartTime);
//                        core.db.csSet(CS, "ccAuthCode", core.docProperties.getText("txn_id"));
//                        core.db.csSet(CS, "ccActionCode", core.docProperties.getText("payment_status"));
//                        core.db.csSet(CS, "ccRefCode", core.docProperties.getText("pending_reason"));
//                        core.db.csSet(CS, "PayMethod", "PayPal " + core.docProperties.getText("payment_type"));
//                        core.db.csSet(CS, "ShipName", core.docProperties.getText("first_name") + " " + core.docProperties.getText("last_name"));
//                        core.db.csSet(CS, "ShipAddress", core.docProperties.getText("address_street"));
//                        core.db.csSet(CS, "ShipCity", core.docProperties.getText("address_city"));
//                        core.db.csSet(CS, "ShipState", core.docProperties.getText("address_state"));
//                        core.db.csSet(CS, "ShipZip", core.docProperties.getText("address_zip"));
//                        core.db.csSet(CS, "BilleMail", core.docProperties.getText("payer_email"));
//                        core.db.csSet(CS, "ContentControlID", CdefController.getContentId(core, "Orders Completed"));
//                        core.db.csSave2(CS);
//                    }
//                    core.db.csClose(ref CS);
//                    //
//                    // Empty the cart
//                    //
//                    CS = core.db.csOpen("Visitors", "OrderID=" + ConfirmOrderID);
//                    if (core.db.csOk(CS)) {
//                        core.db.csSet(CS, "OrderID", 0);
//                        core.db.csSave2(CS);
//                    }
//                    core.db.csClose(ref CS);
//                    //
//                    // TEmp fix until HardCodedPage is complete
//                    //
//                    string Recipient = core.siteProperties.getText("EmailOrderNotifyAddress", core.siteProperties.emailAdmin);
//                    if (genericController.vbInstr(genericController.encodeText(Recipient), "@") == 0) {
//                        //throw new GenericException("Unexpected exception"); // todo - remove this - handleLegacyError12("Init", "PayPal confirmation Order Process Notification email was not sent because EmailOrderNotifyAddress SiteProperty is not valid")
//                    } else {
//                        string Sender = core.siteProperties.getText("EmailOrderFromAddress");
//                        string subject = core.webServer.requestDomain + " Online Order Pending, #" + ConfirmOrderID;
//                        string Message = "<p>An order confirmation has been recieved from PayPal for " + core.webServer.requestDomain + "</p>";
//                        string sendStatus = "";
//                        emailController.sendAdHoc( core, Recipient, Sender, subject, Message,"","","", false, true,0, ref sendStatus);
//                    }
//                }
//                core.doc.continueProcessing = false;
//            } catch (Exception ex) {
//                cp.Site.ErrorReport(ex);
//            }
//            return result;
//        }
//    }
//}
