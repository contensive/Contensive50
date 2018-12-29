
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
//                    csXfer.csOpen("Orders", "(ID=" + ConfirmOrderID + ") and ((OrderCompleted=0)or(OrderCompleted is Null))");
//                    if (csXfer.csOk()) {
//                        csXfer.csSet("OrderCompleted", true);
//                        csXfer.csSet("DateCompleted", core.doc.profileStartTime);
//                        csXfer.csSet("ccAuthCode", core.docProperties.getText("txn_id"));
//                        csXfer.csSet("ccActionCode", core.docProperties.getText("payment_status"));
//                        csXfer.csSet("ccRefCode", core.docProperties.getText("pending_reason"));
//                        csXfer.csSet("PayMethod", "PayPal " + core.docProperties.getText("payment_type"));
//                        csXfer.csSet("ShipName", core.docProperties.getText("first_name") + " " + core.docProperties.getText("last_name"));
//                        csXfer.csSet("ShipAddress", core.docProperties.getText("address_street"));
//                        csXfer.csSet("ShipCity", core.docProperties.getText("address_city"));
//                        csXfer.csSet("ShipState", core.docProperties.getText("address_state"));
//                        csXfer.csSet("ShipZip", core.docProperties.getText("address_zip"));
//                        csXfer.csSet("BilleMail", core.docProperties.getText("payer_email"));
//                        csXfer.csSet("ContentControlID", Models.Domain.MetaModel.getContentId(core, "Orders Completed"));
//                        csXfer.csSave2(CS);
//                    }
//                    csXfer.csClose();
//                    //
//                    // Empty the cart
//                    //
//                    csXfer.csOpen("Visitors", "OrderID=" + ConfirmOrderID);
//                    if (csXfer.csOk()) {
//                        csXfer.csSet("OrderID", 0);
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
