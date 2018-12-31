
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
//                    csData.csOpen("Orders", "(ID=" + ConfirmOrderID + ") and ((OrderCompleted=0)or(OrderCompleted is Null))");
//                    if (csData.csOk()) {
//                        csData.csSet("OrderCompleted", true);
//                        csData.csSet("DateCompleted", core.doc.profileStartTime);
//                        csData.csSet("ccAuthCode", core.docProperties.getText("txn_id"));
//                        csData.csSet("ccActionCode", core.docProperties.getText("payment_status"));
//                        csData.csSet("ccRefCode", core.docProperties.getText("pending_reason"));
//                        csData.csSet("PayMethod", "PayPal " + core.docProperties.getText("payment_type"));
//                        csData.csSet("ShipName", core.docProperties.getText("first_name") + " " + core.docProperties.getText("last_name"));
//                        csData.csSet("ShipAddress", core.docProperties.getText("address_street"));
//                        csData.csSet("ShipCity", core.docProperties.getText("address_city"));
//                        csData.csSet("ShipState", core.docProperties.getText("address_state"));
//                        csData.csSet("ShipZip", core.docProperties.getText("address_zip"));
//                        csData.csSet("BilleMail", core.docProperties.getText("payer_email"));
//                        csData.csSet("ContentControlID", Models.Domain.MetaModel.getContentId(core, "Orders Completed"));
//                        csData.csSave2(CS);
//                    }
//                    csData.csClose();
//                    //
//                    // Empty the cart
//                    //
//                    csData.csOpen("Visitors", "OrderID=" + ConfirmOrderID);
//                    if (csData.csOk()) {
//                        csData.csSet("OrderID", 0);
//                        csData.csSave2(CS);
//                    }
//                    csData.csClose();
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
