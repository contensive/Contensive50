
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Core;
using Contensive.Core.Models.Entity;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
using Contensive.Core.Models.Complex;
//
namespace Contensive.Addons.Core {
    public class processPayPalConformMethodClass : Contensive.BaseClasses.AddonBaseClass {
        //
        //====================================================================================================
        /// <summary>
        /// getFieldEditorPreference remote method
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public override object Execute(Contensive.BaseClasses.CPBaseClass cp) {
            string result = "";
            try {
                CPClass processor = (CPClass)cp;
                coreClass cpCore = processor.core;
                //
                // -- Should be a remote method in commerce
                int ConfirmOrderID = cpCore.docProperties.getInteger("item_name");
                if (ConfirmOrderID != 0) {
                    //
                    // Confirm the order
                    //
                    int CS = cpCore.db.csOpen("Orders", "(ID=" + ConfirmOrderID + ") and ((OrderCompleted=0)or(OrderCompleted is Null))");
                    if (cpCore.db.csOk(CS)) {
                        cpCore.db.csSet(CS, "OrderCompleted", true);
                        cpCore.db.csSet(CS, "DateCompleted", cpCore.doc.profileStartTime);
                        cpCore.db.csSet(CS, "ccAuthCode", cpCore.docProperties.getText("txn_id"));
                        cpCore.db.csSet(CS, "ccActionCode", cpCore.docProperties.getText("payment_status"));
                        cpCore.db.csSet(CS, "ccRefCode", cpCore.docProperties.getText("pending_reason"));
                        cpCore.db.csSet(CS, "PayMethod", "PayPal " + cpCore.docProperties.getText("payment_type"));
                        cpCore.db.csSet(CS, "ShipName", cpCore.docProperties.getText("first_name") + " " + cpCore.docProperties.getText("last_name"));
                        cpCore.db.csSet(CS, "ShipAddress", cpCore.docProperties.getText("address_street"));
                        cpCore.db.csSet(CS, "ShipCity", cpCore.docProperties.getText("address_city"));
                        cpCore.db.csSet(CS, "ShipState", cpCore.docProperties.getText("address_state"));
                        cpCore.db.csSet(CS, "ShipZip", cpCore.docProperties.getText("address_zip"));
                        cpCore.db.csSet(CS, "BilleMail", cpCore.docProperties.getText("payer_email"));
                        cpCore.db.csSet(CS, "ContentControlID", cdefModel.getContentId(cpCore, "Orders Completed"));
                        cpCore.db.csSave2(CS);
                    }
                    cpCore.db.csClose(ref CS);
                    //
                    // Empty the cart
                    //
                    CS = cpCore.db.csOpen("Visitors", "OrderID=" + ConfirmOrderID);
                    if (cpCore.db.csOk(CS)) {
                        cpCore.db.csSet(CS, "OrderID", 0);
                        cpCore.db.csSave2(CS);
                    }
                    cpCore.db.csClose(ref CS);
                    //
                    // TEmp fix until HardCodedPage is complete
                    //
                    string Recipient = cpCore.siteProperties.getText("EmailOrderNotifyAddress", cpCore.siteProperties.emailAdmin);
                    if (genericController.vbInstr(genericController.encodeText(Recipient), "@") == 0) {
                        //throw new ApplicationException("Unexpected exception"); // todo - remove this - handleLegacyError12("Init", "PayPal confirmation Order Process Notification email was not sent because EmailOrderNotifyAddress SiteProperty is not valid")
                    } else {
                        string Sender = cpCore.siteProperties.getText("EmailOrderFromAddress");
                        string subject = cpCore.webServer.requestDomain + " Online Order Pending, #" + ConfirmOrderID;
                        string Message = "<p>An order confirmation has been recieved from PayPal for " + cpCore.webServer.requestDomain + "</p>";
                        cpCore.email.send_Legacy(Recipient, Sender, subject, Message, 0, false, true);
                    }
                }
                cpCore.doc.continueProcessing = false;
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
