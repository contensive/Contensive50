
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
//
using System.Net.Mail;
using System.Net.Mime;
using System.IO;
//
namespace Contensive.Core.Controllers {
    public class smtpController {
        //
        //====================================================================================================
        /// <summary>
        /// Send email by SMTP. return 'ok' if success, else return a user compatible error message
        /// </summary>
        public static bool sendSmtp( coreClass cpCore, emailController.emailClass email, ref string returnErrorMessage, string AttachmentFilename = "") {
            bool status = false;
            returnErrorMessage = "";
            try {
                string smtpServer = cpCore.siteProperties.getText("SMTPServer", "127.0.0.1");
                SmtpClient client = new SmtpClient(smtpServer);
                MailMessage mailMessage = new MailMessage();
                MailAddress fromAddresses = new MailAddress(email.fromAddress.Trim());
                Attachment data = null;
                ContentDisposition disposition = null;
                ContentType mimeType = null;
                AlternateView alternate = null;
                //
                mailMessage.From = fromAddresses;
                mailMessage.To.Add(new MailAddress(email.toAddress.Trim()));
                mailMessage.Subject = email.subject;
                client.EnableSsl = false;
                client.UseDefaultCredentials = false;
                //
                if ((string.IsNullOrEmpty(email.textBody)) && (!string.IsNullOrEmpty(email.htmlBody))) {
                    //
                    // html only
                    //
                    mailMessage.Body = email.htmlBody;
                    mailMessage.IsBodyHtml = true;
                } else if ((!string.IsNullOrEmpty(email.textBody)) && (string.IsNullOrEmpty(email.htmlBody))) {
                    //
                    // text body only
                    //
                    mailMessage.Body = email.textBody;
                    mailMessage.IsBodyHtml = false;
                } else {
                    //
                    // both html and text
                    //
                    mailMessage.Body = email.textBody;
                    mailMessage.IsBodyHtml = false;
                    mimeType = new System.Net.Mime.ContentType("text/html");
                    alternate = AlternateView.CreateAlternateViewFromString(email.htmlBody, mimeType);
                    mailMessage.AlternateViews.Add(alternate);
                }
                //
                // Create  the file attachment for this e-mail message.
                //
                if (!string.IsNullOrEmpty(AttachmentFilename)) {
                    data = new Attachment(AttachmentFilename, MediaTypeNames.Application.Octet);
                    disposition = data.ContentDisposition;
                    disposition.CreationDate = System.IO.File.GetCreationTime(AttachmentFilename);
                    disposition.ModificationDate = System.IO.File.GetLastWriteTime(AttachmentFilename);
                    disposition.ReadDate = System.IO.File.GetLastAccessTime(AttachmentFilename);
                    mailMessage.Attachments.Add(data);
                }
                //
                // -- try-catch the server connection for a better return message
                try {
                    client.Send(mailMessage);
                    status = true;
                } catch (Exception ex) {
                    returnErrorMessage = "There was an error connecting to the email server [" + smtpServer + "]. The error from the server was [" + ex.ToString() + "]";
                    status = false;
                }
            } catch (Exception) {
                throw;
            }
            return status;
        }
    }
}
