

using System.Net.Mail;
using System.Net.Mime;

// 

namespace Controllers {
    
    public class smtpController {
        
        // 
        public int ErrorNumber;
        
        public string ErrorSource;
        
        public string ErrorDescription;
        
        // 
        // ========================================================================
        //  This page and its contents are copyright by Kidwell McGowan Associates.
        //    See Common Module for descriptions
        // 
        //    This module handles the common email interface for the Content Server
        // ========================================================================
        // 
        private coreClass cpCore;
        
        // 
        // ====================================================================================================
        // '' <summary>
        // '' constructor
        // '' </summary>
        // '' <param name="cp"></param>
        // '' <remarks></remarks>
        public smtpController(coreClass cpCore) {
            this.cpCore = cpCore;
        }
        
        // 
        // ========================================================================
        //    Compatibility
        // ========================================================================
        // 
        public bool SendEmail4(object ToAddress, object FromAddress, object SubjectMessage, object BodyMessage, object ResultLogPathPage, object SMTPServer, bool Immediate, bool HTML, string EmailOutPath, void =, void ) {
            return string.IsNullOrEmpty(this.sendEmail5(genericController.encodeText(ToAddress), genericController.encodeText(FromAddress), genericController.encodeText(SubjectMessage), genericController.encodeText(BodyMessage), "", "", genericController.encodeText(ResultLogPathPage), genericController.encodeText(SMTPServer), Immediate, HTML, encodeEmptyText(EmailOutPath, "")));
            // Warning!!! Optional parameters not supported
        }
        
        // 
        // ========================================================================
        //    Send an email, by queue or immediately
        // 
        //    Return all errors in the return string, and set the Error Publics if
        //    someone wants to know more.
        // ========================================================================
        // 
        public string sendEmail5(string EmailTo, string EmailFrom, string EmailSubject, string EmailBody, string BounceAddress, string ReplyToAddress, string ResultLogPathPage, string EmailSMTPServer, bool Immediate, bool HTML, string EmailOutPath) {
            try {
                string LogLine;
                htmlToTextControllers converthtmlToText;
                // Dim Mailer As SMTP5Class
                string EmailBodyText;
                string EmailBodyHTML;
                string SendResult;
                // 
                if (!this.CheckAddress(EmailTo)) {
                    sendEmail5 = ("The to-address [" 
                                + (EmailTo + "] is not valid"));
                }
                else if (!this.CheckAddress(EmailFrom)) {
                    sendEmail5 = ("The from-address [" 
                                + (EmailFrom + "] is not valid"));
                }
                else if (!this.CheckServer(EmailSMTPServer)) {
                    sendEmail5 = ("The email server [" 
                                + (EmailSMTPServer + "] is not valid"));
                }
                else if (!Immediate) {
                    // 
                    //  ----- Add the email to the queue
                    // 
                    sendEmail5 = this.addEmailQueue(EmailTo, EmailFrom, EmailSubject, EmailBody, BounceAddress, ReplyToAddress, ResultLogPathPage, EmailSMTPServer, HTML, EmailOutPath);
                }
                else {
                    // 
                    //  ----- Send the email now
                    // 
                    // kma() 'fs = New fileSystemClass
                    // Mailer = New SMTP5Class
                    ReplyToAddress = ReplyToAddress;
                    ReturnAddress = BounceAddress;
                    if (HTML) {
                        // 
                        //  ----- send HTML email (and plain text conversion)
                        // 
                        converthtmlToText = new htmlToTextControllers(cpCore);
                        EmailBodyHTML = EmailBody;
                        if ((genericController.vbInstr(1, EmailBodyHTML, "<BODY", vbTextCompare) == 0)) {
                            EmailBodyHTML = ("<BODY>" 
                                        + (EmailBodyHTML + "</BODY>"));
                        }
                        
                        if ((genericController.vbInstr(1, EmailBodyHTML, "<HTML>", vbTextCompare) == 0)) {
                            EmailBodyHTML = ("<HTML>" 
                                        + (EmailBodyHTML + "</HTML>"));
                        }
                        
                        EmailBodyText = converthtmlToText.convert(EmailBody);
                        sendEmail5 = this.sendEmail6(EmailSMTPServer, EmailTo, EmailFrom, EmailSubject, EmailBodyText, "", EmailBodyHTML);
                        // converthtmlToText = Nothing
                    }
                    else {
                        // 
                        //  ----- send plain text email
                        // 
                        sendEmail5 = this.sendEmail6(EmailSMTPServer, EmailTo, EmailFrom, EmailSubject, EmailBody, "");
                    }
                    
                    // Mailer = Nothing
                    // 
                    //  ----- clean up the result code for logging (change empty to "ok")
                    // 
                    sendEmail5 = genericController.vbReplace(sendEmail5, "\r\n", "");
                    if ((sendEmail5 == "")) {
                        SendResult = "ok";
                    }
                    else {
                        SendResult = sendEmail5;
                    }
                    
                    // 
                    //  ----- Update Email Result Log
                    // 
                    if ((ResultLogPathPage != "")) {
                        cpCore.appRootFiles.appendFile(ResultLogPathPage, (Now().ToString() + (" delivery attempted to " 
                                        + (EmailTo + ("," 
                                        + (SendResult + "\r\n"))))));
                    }
                    
                    // 
                    //  ----- Update the System Email Log
                    // 
                    LogLine = ("\"" 
                                + (Now().ToString() + ("\",\"To[" 
                                + (EmailTo + ("]\",\"From[" 
                                + (EmailFrom + ("]\",\"Bounce[" 
                                + (BounceAddress + ("]\",\"Subject[" 
                                + (EmailSubject + ("]\",\"Result[" 
                                + (SendResult + ("]\"" + "\r\n")))))))))))));
                    logController.appendLog(cpCore, LogLine, "email");
                }
                
                // 
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
        }
        
        // 
        // ========================================================================
        //    add this email to the email queue
        // ========================================================================
        // 
        private string addEmailQueue(object ToAddress, object FromAddress, object SubjectMessage, object BodyMessage, object BounceAddress, object ReplyToAddress, object ResultLogPathPage, object SMTPServer, bool HTML, string EmailOutPath, void =, void ) {
            addEmailQueue = "";
            // Warning!!! Optional parameters not supported
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 
            string Filename;
            string MethodName;
            string Copy;
            // Dim kmafs As fileSystemClass
            string iEmailOutPath;
            // 
            MethodName = "AddQueue";
            if ((EmailOutPath != "")) {
                if (((EmailOutPath.IndexOf("\\", 0) + 1) 
                            != EmailOutPath.Length)) {
                    iEmailOutPath = (EmailOutPath + "\\");
                }
                else {
                    iEmailOutPath = EmailOutPath;
                }
                
            }
            else {
                iEmailOutPath = "emailout\\";
            }
            
            // 
            //  ----- write the email to the email queue folder for delivery later
            // 
            Copy = "";
            Copy = (Copy + ("Contensive Handler " 
                        + (My.Application.Info.Version.Major + ("." 
                        + (My.Application.Info.Version.Minor + ("." 
                        + (My.Application.Info.Version.Revision + "\r\n")))))));
            Copy = (Copy 
                        + (genericController.encodeText(SMTPServer) + "\r\n"));
            Copy = (Copy 
                        + (genericController.encodeText(ResultLogPathPage) + "\r\n"));
            Copy = (Copy 
                        + (genericController.encodeText(ToAddress) + "\r\n"));
            Copy = (Copy 
                        + (genericController.encodeText(FromAddress) + "\r\n"));
            Copy = (Copy 
                        + (genericController.encodeText(BounceAddress) + "\r\n"));
            Copy = (Copy 
                        + (genericController.encodeText(ReplyToAddress) + "\r\n"));
            Copy = (Copy 
                        + (genericController.encodeText(SubjectMessage) + "\r\n"));
            Copy = (Copy 
                        + (genericController.encodeText(HTML) + "\r\n"));
            Copy = (Copy + genericController.encodeText(BodyMessage));
            Filename = ("Out" 
                        + (genericController.GetRandomInteger().ToString() + ".txt"));
            cpCore.appRootFiles.saveFile((iEmailOutPath + Filename), Copy);
            // 
            if ((Err.Number != 0)) {
                this.HandleClassTrapError("AddQueue", true);
                addEmailQueue = ("There was an unexpected error detected exiting the SMTPHandler AddQueue method [" 
                            + (Err.Description + "]."));
                Err.Clear();
            }
            
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
            // 
        ErrorTrap:
            ErrorNumber = Err.Number;
            ErrorSource = Err.Source;
            ErrorDescription = Err.Description;
            // 
            // 
            this.HandleClassTrapError(MethodName, true);
            Err.Clear();
            // 
            return ("There was an unexpected error saving the email to the email queue [" 
                        + (Err.Description + "]."));
        }
        
        // 
        // ========================================================================
        //    Send the emails in the current Queue
        // 
        //    Errors here should be logged, but do not bubblethe error up, as the host
        //    here is not the user, but the Service.
        // ========================================================================
        // 
        public void SendEmailQueue(string EmailOutPath, void =, void ) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // Warning!!! Optional parameters not supported
            // 
            // Dim SMTP As SMTP5Class
            bool HTML;
            object LogFile;
            string LogFilename;
            string iEmailOutPath;
            string MethodName;
            IO.FileInfo[] FileList;
            int EOL;
            int CommaPosition;
            string Filename = "";
            string Copy;
            string EmailSMTP;
            string EmailTo;
            string EmailFrom;
            string EmailSubject;
            string EmailBody;
            string ResultLogPathPage;
            string iiEmailOutPath;
            string BounceAddress;
            string ReplyToAddress;
            // 
            MethodName = "SendQueue";
            if ((EmailOutPath != "")) {
                if ((EmailOutPath.Substring((EmailOutPath.Length - 1)) != "\\")) {
                    iEmailOutPath = (EmailOutPath + "\\");
                }
                else {
                    iEmailOutPath = EmailOutPath;
                }
                
            }
            else {
                iEmailOutPath = "emailout\\";
            }
            
            // 
            FileList = cpCore.appRootFiles.getFileList(iEmailOutPath);
            foreach (IO.FileInfo file in FileList) {
                Copy = cpCore.appRootFiles.readFile((iEmailOutPath + Filename));
                // 
                //  No - no way to manage all the files for now. Later work up something
                // Call cpCore.app.publicFiles.CopyFile(iEmailOutPath & Filename, iEmailOutPath & "sent\" & Filename)
                cpCore.appRootFiles.deleteFile((iEmailOutPath + Filename));
                // 
                //  Decode the file into the email arguments
                // 
                string Line0;
                Line0 = this.ReadLine(Copy);
                if ((genericController.vbUCase(Line0.Substring(0, 11)) == "CONTENSIVE ")) {
                    // 
                    //  Email record (LINE0 IS CONENSIVE AND VERSION)
                    // 
                    EmailSMTP = this.ReadLine(Copy);
                    ResultLogPathPage = this.ReadLine(Copy);
                    EmailTo = this.ReadLine(Copy);
                    EmailFrom = this.ReadLine(Copy);
                    BounceAddress = this.ReadLine(Copy);
                    ReplyToAddress = this.ReadLine(Copy);
                    EmailSubject = this.ReadLine(Copy);
                    HTML = genericController.EncodeBoolean(this.ReadLine(Copy));
                    // 
                    //  removed this because the addqueue did not put it in
                    // 
                    //                 Call ReadLine(Copy)
                    EmailBody = Copy;
                }
                else {
                    // 
                    //  Legacy record
                    // 
                    EmailSMTP = Line0;
                    ResultLogPathPage = this.ReadLine(Copy);
                    EmailTo = this.ReadLine(Copy);
                    EmailFrom = this.ReadLine(Copy);
                    EmailSubject = this.ReadLine(Copy);
                    HTML = genericController.EncodeBoolean(this.ReadLine(Copy));
                    EmailBody = Copy;
                }
                
                if (((EmailSMTP != "") 
                            && ((EmailTo != "") 
                            && (EmailFrom != "")))) {
                    // 
                    //  Send email
                    // 
                    this.SendEmail4(EmailTo, EmailFrom, EmailSubject, EmailBody, ResultLogPathPage, EmailSMTP, true, HTML, EmailOutPath);
                }
                else {
                    // 
                    //  Error, log the problem
                    // 
                    this.HandleClassInternalError(ignoreInteger, "App.EXEName", ("Invalid email in send queue [" 
                                    + (Filename + "] was removed")), MethodName, true);
                }
                
            }
            
            // 
            if ((Err.Number != 0)) {
                this.HandleClassTrapError("SendQueue", true);
                Err.Clear();
            }
            
            return;
        ErrorTrap:
            this.HandleClassTrapError(MethodName, false);
        }
        
        // 
        // ========================================================================
        // 
        // ========================================================================
        // 
        private string ReadLine(ref string Body) {
            string line = "";
            try {
                int EOL = genericController.vbInstr(1, Body, "\r\n");
                if ((EOL != 0)) {
                    line = Body.Substring(0, (EOL - 1));
                    Body = Body.Substring((EOL + 1));
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return line;
        }
        
        // '
        // '
        // '
        // Public sub ErrorExit(ByVal ErrNumber As Integer, ByVal ErrSource As String, ByVal ErrDescription As String, ByVal MethodName As String, ByVal ErrorTrap As Boolean, ByVal ResumeNext As Boolean)
        //     cpCore.handleLegacyError3("", "unknown", "ccEmail4", "SMTPHandler", MethodName, ErrNumber, ErrSource, ErrDescription, ErrorTrap, ResumeNext, "")
        // End Function
        // 
        // 
        // 
        private void HandleClassTrapError(string MethodName, bool ResumeNext) {
            throw new ApplicationException("Unexpected exception");
        }
        
        // 
        // 
        // 
        private void HandleClassInternalError(int ErrNumber, string ErrSource, string ErrDescription, string MethodName, bool ResumeNext) {
            throw new ApplicationException("Unexpected exception");
        }
        
        // 
        // 
        // 
        private bool CheckAddress(string EmailAddress) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 
            string[] SplitArray;
            CheckAddress = false;
            if ((EmailAddress != "")) {
                if ((genericController.vbInstr(1, EmailAddress, "@") != 0)) {
                    SplitArray = EmailAddress.Split("@");
                    if ((UBound(SplitArray) == 1)) {
                        if ((SplitArray[0].Length > 0)) {
                            CheckAddress = this.CheckServer(SplitArray[1]);
                        }
                        
                    }
                    
                }
                
            }
            
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
            // 
        ErrorTrap:
            CheckAddress = false;
            Err.Clear();
        }
        
        // 
        //  Server must have at least 3 digits, and one dot in the middle
        // 
        private bool CheckServer(string EmailServer) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 
            string[] SplitArray;
            // 
            if ((EmailServer == "")) {
                CheckServer = false;
            }
            else if ((genericController.vbInstr(1, EmailServer, "SMTP.YourServer.Com", vbTextCompare) != 0)) {
                CheckServer = false;
            }
            else {
                SplitArray = EmailServer.Split(".");
                if ((UBound(SplitArray) > 0)) {
                    if (((SplitArray[0].Length > 0) 
                                && (SplitArray[1].Length > 0))) {
                        CheckServer = true;
                    }
                    
                }
                
            }
            
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
            // 
        ErrorTrap:
            CheckServer = false;
            Err.Clear();
        }
        
        // 
        // 
        // 
        // 
        // 
        private string LocalReturnAddress = "";
        
        public string ReturnAddress {
            get {
                return LocalReturnAddress;
            }
            set {
                LocalReturnAddress = value;
            }
        }
        
        private string LocalReplyToAddress = "";
        
        public string ReplyToAddress {
            get {
                return LocalReplyToAddress;
            }
            set {
                LocalReplyToAddress = value;
            }
        }
        
        public string sendEmail6(string SMTPServer, string ToAddress, string fromAddress, string subject, string Body, string AttachmentFilename, void =, void , string HTMLBody, void =, void ) {
            string status = "";
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            try {
                // this is an error
                SmtpClient client = new SmtpClient(SMTPServer);
                MailMessage mailMessage = new MailMessage();
                MailAddress fromAddresses = new MailAddress(fromAddress.Trim());
                Attachment data;
                ContentDisposition disposition;
                ContentType mimeType;
                AlternateView alternate;
                // 
                mailMessage.From = fromAddresses;
                mailMessage.To.Add(new MailAddress(ToAddress.Trim()));
                mailMessage.Subject = subject;
                client.EnableSsl = false;
                client.UseDefaultCredentials = false;
                if (((Body == "") 
                            && (HTMLBody != ""))) {
                    // 
                    //  html only
                    // 
                    mailMessage.Body = HTMLBody;
                    mailMessage.IsBodyHtml = true;
                }
                else if (((Body != "") 
                            && (HTMLBody == ""))) {
                    // 
                    //  text body only
                    // 
                    mailMessage.Body = Body;
                    mailMessage.IsBodyHtml = false;
                }
                else {
                    // 
                    //  both html and text
                    // 
                    mailMessage.Body = Body;
                    mailMessage.IsBodyHtml = false;
                    mimeType = new System.Net.Mime.ContentType("text/html");
                    alternate = AlternateView.CreateAlternateViewFromString(HTMLBody, mimeType);
                    mailMessage.AlternateViews.Add(alternate);
                }
                
                // 
                //  Create  the file attachment for this e-mail message.
                // 
                if ((AttachmentFilename != "")) {
                    data = new Attachment(AttachmentFilename, MediaTypeNames.Application.Octet);
                    disposition = data.ContentDisposition;
                    disposition.CreationDate = System.IO.File.GetCreationTime(AttachmentFilename);
                    disposition.ModificationDate = System.IO.File.GetLastWriteTime(AttachmentFilename);
                    disposition.ReadDate = System.IO.File.GetLastAccessTime(AttachmentFilename);
                    mailMessage.Attachments.Add(data);
                }
                
                // 
                //  Send the message.
                // 
                // Add credentials if the SMTP server requires them.
                // client.Credentials = CredentialCache.DefaultNetworkCredentials;
                // client.Credentials = basicCredential;
                // 
                //  if there is an error sending, an exception is thrown
                // 
                client.Send(mailMessage);
                status = "ok";
            }
            catch (Exception ex) {
                // 
                // 
                // 
                status = ex.Message;
            }
            
            return status;
        }
    }
}