﻿
using System;
using System.Net.Mail;
using System.Net.Mime;
using System.Collections.Generic;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Amazon;
using Contensive.Processor.Models.Domain;
//
namespace Contensive.Processor.Controllers {
    public class EmailAmazonSESController {
        //
        //====================================================================================================
        /// <summary>
        /// Send email by SMTP. return 'ok' if success, else return a user compatible error message
        /// </summary>
        public static bool send(CoreController core, EmailController.EmailClass email, ref string returnErrorMessage, string awsAccessKeyId, string awsSecretAccessKey) {
            bool status = false;
            returnErrorMessage = "";
            try {
                if (core.mockEmail) {
                    //
                    // -- for unit tests, mock interface by adding email to core.mockSmptList
                    core.mockEmailList.Add(new MockEmailClass() {
                        email = email
                    });
                    status = true;
                } else {
                    //
                    // -- send email
                    Body messageBody = new Body { };
                    if (!string.IsNullOrEmpty(email.htmlBody)) {
                        messageBody.Html = new Content { Charset = "UTF-8", Data = email.htmlBody };
                    }
                    if (!string.IsNullOrEmpty(email.textBody)) {
                        messageBody.Text = new Content { Charset = "UTF-8", Data = email.textBody };
                    }
                    AmazonSimpleEmailServiceConfig sesConfig = new AmazonSimpleEmailServiceConfig();
                    using (var client = new AmazonSimpleEmailServiceClient(awsAccessKeyId, awsSecretAccessKey, RegionEndpoint.USEast1)) {
                        var sendRequest = new SendEmailRequest {
                            Source = email.fromAddress,
                            Destination = new Destination {
                                ToAddresses = new List<string> { email.toAddress }
                            },
                            Message = new Message {
                                Subject = new Content(email.subject),
                                Body = messageBody
                            }
                        };
                        try {
                            LogController.logInfo(core, "sendSmtp, to [" + email.toAddress + "], from [" + email.fromAddress + "], subject [" + email.subject + "]");
                            var response = client.SendEmail(sendRequest);
                            status = true;
                        } catch (Exception ex) {
                            returnErrorMessage = "There was an error sending email [" + ex.ToString() + "]";
                            LogController.logError(core, returnErrorMessage);
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.logError(core, "There was an error configuring smtp server ex [" + ex.ToString() + "]");
                throw;
            }
            return status;
        }
    }
}
