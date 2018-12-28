
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
//                    int csXfer.csOpen("Orders", "(ID=" + ConfirmOrderID + ") and ((OrderCompleted=0)or(OrderCompleted is Null))");
//                    if (csXfer.csOk()) {
//                        csXfer.csSet(CS, "OrderCompleted", true);
//                        csXfer.csSet(CS, "DateCompleted", core.doc.profileStartTime);
//                        csXfer.csSet(CS, "ccAuthCode", core.docProperties.getText("txn_id"));
//                        csXfer.csSet(CS, "ccActionCode", core.docProperties.getText("payment_status"));
//                        csXfer.csSet(CS, "ccRefCode", core.docProperties.getText("pending_reason"));
//                        csXfer.csSet(CS, "PayMethod", "PayPal " + core.docProperties.getText("payment_type"));
//                        csXfer.csSet(CS, "ShipName", core.docProperties.getText("first_name") + " " + core.docProperties.getText("last_name"));
//                        csXfer.csSet(CS, "ShipAddress", core.docProperties.getText("address_street"));
//                        csXfer.csSet(CS, "ShipCity", core.docProperties.getText("address_city"));
//                        csXfer.csSet(CS, "ShipState", core.docProperties.getText("address_state"));
//                        csXfer.csSet(CS, "ShipZip", core.docProperties.getText("address_zip"));
//                        csXfer.csSet(CS, "BilleMail", core.docProperties.getText("payer_email"));
//                        csXfer.csSet(CS, "ContentControlID", Models.Domain.MetaModel.getContentId(core, "Orders Completed"));
//                        csXfer.csSave2(CS);
//                    }
//                    csXfer.csClose();
//                    //
//                    // Empty the cart
//                    //
//                    csXfer.csOpen("Visitors", "OrderID=" + ConfirmOrderID);
//                    if (csXfer.csOk()) {
//                        csXfer.csSet(CS, "OrderID", 0);
//                        csXfer.csSave2(CS);
//                    }
//                    csXfer.csClose();
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
