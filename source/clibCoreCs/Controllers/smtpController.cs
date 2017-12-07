using System;

//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

using System.Net.Mail;
using System.Net.Mime;
using static Contensive.Core.Controllers.genericController;
//
namespace Contensive.Core.Controllers
{
	public class smtpController
	{
		//
		public int ErrorNumber;
		public string ErrorSource;
		public string ErrorDescription;
		//
		//========================================================================
		// This page and its contents are copyright by Kidwell McGowan Associates.
		//   See Common Module for descriptions
		//
		//   This module handles the common email interface for the Content Server
		//========================================================================
		//
		private coreClass cpCore;
		//
		//====================================================================================================
		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="cp"></param>
		/// <remarks></remarks>
		public smtpController(coreClass cpCore) : base()
		{
			this.cpCore = cpCore;
		}
		//
		//========================================================================
		//   Compatibility
		//========================================================================
		//
		public bool SendEmail4(object ToAddress, object FromAddress, object SubjectMessage, object BodyMessage, object ResultLogPathPage, object SMTPServer, bool Immediate, bool HTML, string EmailOutPath = "")
		{
			return string.IsNullOrEmpty(sendEmail5(genericController.encodeText(ToAddress), genericController.encodeText(FromAddress), genericController.encodeText(SubjectMessage), genericController.encodeText(BodyMessage), "", "", genericController.encodeText(ResultLogPathPage), genericController.encodeText(SMTPServer), Immediate, HTML, encodeEmptyText(EmailOutPath, "")));
		}
		//
		//========================================================================
		//   Send an email, by queue or immediately
		//
		//   Return all errors in the return string, and set the Error Publics if
		//   someone wants to know more.
		//========================================================================
		//
		public string sendEmail5(string EmailTo, string EmailFrom, string EmailSubject, string EmailBody, string BounceAddress, string ReplyToAddress, string ResultLogPathPage, string EmailSMTPServer, bool Immediate, bool HTML, string EmailOutPath)
		{
			string tempsendEmail5 = null;
			try
			{
				string LogLine = null;
				htmlToTextControllers converthtmlToText = null;
				//Dim Mailer As SMTP5Class
				string EmailBodyText = null;
				string EmailBodyHTML = null;
				string SendResult = null;
				//
				if (!CheckAddress(EmailTo))
				{
					tempsendEmail5 = "The to-address [" + EmailTo + "] is not valid";
				}
				else if (!CheckAddress(EmailFrom))
				{
					tempsendEmail5 = "The from-address [" + EmailFrom + "] is not valid";
				}
				else if (!CheckServer(EmailSMTPServer))
				{
					tempsendEmail5 = "The email server [" + EmailSMTPServer + "] is not valid";
				}
				else
				{
					if (!Immediate)
					{
						//
						// ----- Add the email to the queue
						//
						tempsendEmail5 = addEmailQueue(EmailTo, EmailFrom, EmailSubject, EmailBody, BounceAddress, ReplyToAddress, ResultLogPathPage, EmailSMTPServer, HTML, EmailOutPath);
					}
					else
					{
						//
						// ----- Send the email now
						//
						//kma() 'fs = New fileSystemClass
						//Mailer = New SMTP5Class
						ReplyToAddress = ReplyToAddress;
						ReturnAddress = BounceAddress;
						if (HTML)
						{
							//
							// ----- send HTML email (and plain text conversion)
							//
							converthtmlToText = new htmlToTextControllers(cpCore);
							EmailBodyHTML = EmailBody;
							if (genericController.vbInstr(1, EmailBodyHTML, "<BODY", Microsoft.VisualBasic.Constants.vbTextCompare) == 0)
							{
								EmailBodyHTML = "<BODY>" + EmailBodyHTML + "</BODY>";
							}
							if (genericController.vbInstr(1, EmailBodyHTML, "<HTML>", Microsoft.VisualBasic.Constants.vbTextCompare) == 0)
							{
								EmailBodyHTML = "<HTML>" + EmailBodyHTML + "</HTML>";
							}
							EmailBodyText = converthtmlToText.convert(EmailBody);
							tempsendEmail5 = sendEmail6(EmailSMTPServer, EmailTo, EmailFrom, EmailSubject, EmailBodyText, "", EmailBodyHTML);
							//converthtmlToText = Nothing
						}
						else
						{
							//
							// ----- send plain text email
							//
							tempsendEmail5 = sendEmail6(EmailSMTPServer, EmailTo, EmailFrom, EmailSubject, EmailBody, "");
						}
						//Mailer = Nothing
						//
						// ----- clean up the result code for logging (change empty to "ok")
						//
						tempsendEmail5 = genericController.vbReplace(tempsendEmail5, Environment.NewLine, "");
						if (string.IsNullOrEmpty(tempsendEmail5))
						{
							SendResult = "ok";
						}
						else
						{
							SendResult = tempsendEmail5;
						}
						//
						// ----- Update Email Result Log
						//
						if (!string.IsNullOrEmpty(ResultLogPathPage))
						{
							cpCore.appRootFiles.appendFile(ResultLogPathPage, Convert.ToString(DateTime.Now) + " delivery attempted to " + EmailTo + "," + SendResult + Environment.NewLine);
						}
						//
						// ----- Update the System Email Log
						//
						LogLine = "\"" + Convert.ToString(DateTime.Now) + "\",\"To[" + EmailTo + "]\",\"From[" + EmailFrom + "]\",\"Bounce[" + BounceAddress + "]\",\"Subject[" + EmailSubject + "]\",\"Result[" + SendResult + "]\"" + Environment.NewLine;
						logController.appendLog(cpCore, LogLine, "email");
					}
				}
				//
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
				//Mailer = Nothing
				//converthtmlToText = Nothing
				tempsendEmail5 = "There was an unexpected error sending the email.";
			}
			return tempsendEmail5;
		}
		//
		//========================================================================
		//   add this email to the email queue
		//========================================================================
		//
		private string addEmailQueue(object ToAddress, object FromAddress, object SubjectMessage, object BodyMessage, object BounceAddress, object ReplyToAddress, object ResultLogPathPage, object SMTPServer, bool HTML, string EmailOutPath = "")
		{
				string tempaddEmailQueue = null;
			try
			{
				tempaddEmailQueue = "";
				//
				string Filename = null;
				string MethodName = null;
				string Copy = null;
				//Dim kmafs As fileSystemClass
				string iEmailOutPath = null;
				//
				MethodName = "AddQueue";
				//
				// ----- Get the email folder
				//
				if (!string.IsNullOrEmpty(EmailOutPath))
				{
					if (EmailOutPath.IndexOf("\\") + 1 != EmailOutPath.Length)
					{
						iEmailOutPath = EmailOutPath + "\\";
					}
					else
					{
						iEmailOutPath = EmailOutPath;
					}
				}
				else
				{
					iEmailOutPath = "emailout\\";
				}
				//
				// ----- write the email to the email queue folder for delivery later
				//
				Copy = "";
				Copy = Copy + "Contensive Handler " + My.MyApplication.Application.Info.Version.Major + "." + My.MyApplication.Application.Info.Version.Minor + "." + My.MyApplication.Application.Info.Version.Revision + Environment.NewLine;
				Copy = Copy + genericController.encodeText(SMTPServer) + Environment.NewLine;
				Copy = Copy + genericController.encodeText(ResultLogPathPage) + Environment.NewLine;
				Copy = Copy + genericController.encodeText(ToAddress) + Environment.NewLine;
				Copy = Copy + genericController.encodeText(FromAddress) + Environment.NewLine;
				Copy = Copy + genericController.encodeText(BounceAddress) + Environment.NewLine;
				Copy = Copy + genericController.encodeText(ReplyToAddress) + Environment.NewLine;
				Copy = Copy + genericController.encodeText(SubjectMessage) + Environment.NewLine;
				Copy = Copy + genericController.encodeText(HTML) + Environment.NewLine;
				Copy = Copy + genericController.encodeText(BodyMessage);
				Filename = "Out" + Convert.ToString(genericController.GetRandomInteger()) + ".txt";
				//
				cpCore.appRootFiles.saveFile(iEmailOutPath + Filename, Copy);
				//
//INSTANT C# TODO TASK: Calls to the VB 'Err' function are not converted by Instant C#:
				if (Microsoft.VisualBasic.Information.Err().Number != 0)
				{
					HandleClassTrapError("AddQueue", true);
//INSTANT C# TODO TASK: Calls to the VB 'Err' function are not converted by Instant C#:
					tempaddEmailQueue = "There was an unexpected error detected exiting the SMTPHandler AddQueue method [" + Microsoft.VisualBasic.Information.Err().Description + "].";
//INSTANT C# TODO TASK: Calls to the VB 'Err' function are not converted by Instant C#:
					//Microsoft.VisualBasic.Information.Err().Clear();
				}
				return tempaddEmailQueue;
				//
			}
			catch
			{
				cpCore.handleException( ex );
			}
//ErrorTrap:
//INSTANT C# TODO TASK: Calls to the VB 'Err' function are not converted by Instant C#:
			ErrorNumber = Microsoft.VisualBasic.Information.Err().Number;
//INSTANT C# TODO TASK: Calls to the VB 'Err' function are not converted by Instant C#:
			ErrorSource = Microsoft.VisualBasic.Information.Err().Source;
//INSTANT C# TODO TASK: Calls to the VB 'Err' function are not converted by Instant C#:
			ErrorDescription = Microsoft.VisualBasic.Information.Err().Description;
			//
			//
			HandleClassTrapError(MethodName, true);
//INSTANT C# TODO TASK: Calls to the VB 'Err' function are not converted by Instant C#:
			//Microsoft.VisualBasic.Information.Err().Clear();
			//
//INSTANT C# TODO TASK: Calls to the VB 'Err' function are not converted by Instant C#:
			return "There was an unexpected error saving the email to the email queue [" + Microsoft.VisualBasic.Information.Err().Description + "].";
		}
		//
		//========================================================================
		//   Send the emails in the current Queue
		//
		//   Errors here should be logged, but do not bubblethe error up, as the host
		//   here is not the user, but the Service.
		//========================================================================
		//
		public void SendEmailQueue(string EmailOutPath = "")
		{
			try
			{
				//
				//Dim SMTP As SMTP5Class
				bool HTML = false;
				object LogFile = null;
				string LogFilename = null;
				string iEmailOutPath = null;
				string MethodName = null;
				IO.FileInfo[] FileList = null;
				int EOL = 0;
				int CommaPosition = 0;
				string Filename = "";
				string Copy = null;
				string EmailSMTP = null;
				string EmailTo = null;
				string EmailFrom = null;
				string EmailSubject = null;
				string EmailBody = null;
				string ResultLogPathPage = null;
				string iiEmailOutPath = null;
				string BounceAddress = null;
				string ReplyToAddress = null;
				//
				MethodName = "SendQueue";
				//
				// ----- Get the email folder
				//
				if (!string.IsNullOrEmpty(EmailOutPath))
				{
					if (EmailOutPath.Substring(EmailOutPath.Length - 1) != "\\")
					{
						iEmailOutPath = EmailOutPath + "\\";
					}
					else
					{
						iEmailOutPath = EmailOutPath;
					}
				}
				else
				{
					iEmailOutPath = "emailout\\";
				}
				//
				FileList = cpCore.appRootFiles.getFileList(iEmailOutPath);
				foreach (IO.FileInfo file in FileList)
				{
					Copy = cpCore.appRootFiles.readFile(iEmailOutPath + Filename);
					//
					// No - no way to manage all the files for now. Later work up something
					//Call cpCore.app.publicFiles.CopyFile(iEmailOutPath & Filename, iEmailOutPath & "sent\" & Filename)
					cpCore.appRootFiles.deleteFile(iEmailOutPath + Filename);
					//
					// Decode the file into the email arguments
					//
					string Line0 = ReadLine(ref Copy);
					if (genericController.vbUCase(Line0.Substring(0, 11)) == "CONTENSIVE ")
					{
						//
						// Email record (LINE0 IS CONENSIVE AND VERSION)
						//
						EmailSMTP = ReadLine(ref Copy);
						ResultLogPathPage = ReadLine(ref Copy);
						EmailTo = ReadLine(ref Copy);
						EmailFrom = ReadLine(ref Copy);
						BounceAddress = ReadLine(ref Copy);
						ReplyToAddress = ReadLine(ref Copy);
						EmailSubject = ReadLine(ref Copy);
						HTML = genericController.EncodeBoolean(ReadLine(ref Copy));
						//
						// removed this because the addqueue did not put it in
						//
						//                Call ReadLine(Copy)
						EmailBody = Copy;
					}
					else
					{
						//
						// Legacy record
						//
						EmailSMTP = Line0;
						ResultLogPathPage = ReadLine(ref Copy);
						EmailTo = ReadLine(ref Copy);
						EmailFrom = ReadLine(ref Copy);
						EmailSubject = ReadLine(ref Copy);
						HTML = genericController.EncodeBoolean(ReadLine(ref Copy));
						EmailBody = Copy;
					}
					if ((!string.IsNullOrEmpty(EmailSMTP)) & (!string.IsNullOrEmpty(EmailTo)) & (!string.IsNullOrEmpty(EmailFrom)))
					{
						//
						// Send email
						//
						SendEmail4(EmailTo, EmailFrom, EmailSubject, EmailBody, ResultLogPathPage, EmailSMTP, true, HTML, EmailOutPath);
					}
					else
					{
						//
						// Error, log the problem
						//
						HandleClassInternalError(ignoreInteger, "App.EXEName", "Invalid email in send queue [" + Filename + "] was removed", MethodName, true);
					}
				}
				//
//INSTANT C# TODO TASK: Calls to the VB 'Err' function are not converted by Instant C#:
				if (Microsoft.VisualBasic.Information.Err().Number != 0)
				{
					HandleClassTrapError("SendQueue", true);
//INSTANT C# TODO TASK: Calls to the VB 'Err' function are not converted by Instant C#:
					//Microsoft.VisualBasic.Information.Err().Clear();
				}
				return;
			}
			catch
			{
				cpCore.handleException( ex );
			}
//ErrorTrap:
			//kmafs = Nothing
			HandleClassTrapError(MethodName, false);
		}
		//
		//========================================================================
		//
		//========================================================================
		//
		private string ReadLine(ref string Body)
		{
			string line = "";
			try
			{
				int EOL = genericController.vbInstr(1, Body, Environment.NewLine);
				if (EOL != 0)
				{
					line = Body.Substring(0, EOL - 1);
					Body = Body.Substring(EOL + 1);
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return line;
		}
		//'
		//'
		//'
		//Public sub ErrorExit(ByVal ErrNumber As Integer, ByVal ErrSource As String, ByVal ErrDescription As String, ByVal MethodName As String, ByVal ErrorTrap As Boolean, ByVal ResumeNext As Boolean)
		//    cpCore.handleLegacyError3("", "unknown", "ccEmail4", "SMTPHandler", MethodName, ErrNumber, ErrSource, ErrDescription, ErrorTrap, ResumeNext, "")
		//End Function
		//
		//
		//
		private void HandleClassTrapError(string MethodName, bool ResumeNext)
		{
			throw (new ApplicationException("Unexpected exception")); //cpCore.handleLegacyError3("", "trap error", "ccEmail4", "SMTPHandlerClass", MethodName, Err.Number, Err.Source, Err.Description, True, ResumeNext, "unknown")
		}
		//
		//
		//
		private void HandleClassInternalError(int ErrNumber, string ErrSource, string ErrDescription, string MethodName, bool ResumeNext)
		{
			throw (new ApplicationException("Unexpected exception")); //cpCore.handleLegacyError3("", "internal error", "ccEmail4", "SMTPHandlerClass", MethodName, ErrNumber, ErrSource, ErrDescription, True, ResumeNext, "unknown")
		}
		//
		//
		//
		private bool CheckAddress(string EmailAddress)
		{
				bool tempCheckAddress = false;
			try
			{
				//
				string[] SplitArray = null;
				tempCheckAddress = false;
				if (!string.IsNullOrEmpty(EmailAddress))
				{
					if (genericController.vbInstr(1, EmailAddress, "@") != 0)
					{
						SplitArray = EmailAddress.Split('@');
						if (SplitArray.GetUpperBound(0) == 1)
						{
							if (SplitArray[0].Length > 0)
							{
								tempCheckAddress = CheckServer(SplitArray[1]);
							}
						}
					}
				}
				return tempCheckAddress;
				//
			}
			catch
			{
				cpCore.handleException( ex );
			}
//ErrorTrap:
			tempCheckAddress = false;
//INSTANT C# TODO TASK: Calls to the VB 'Err' function are not converted by Instant C#:
			//Microsoft.VisualBasic.Information.Err().Clear();
			return tempCheckAddress;
		}
		//
		// Server must have at least 3 digits, and one dot in the middle
		//
		private bool CheckServer(string EmailServer)
		{
				bool tempCheckServer = false;
			try
			{
				//
				string[] SplitArray = null;
				//
				if (string.IsNullOrEmpty(EmailServer))
				{
					tempCheckServer = false;
				}
				else if (genericController.vbInstr(1, EmailServer, "SMTP.YourServer.Com", Microsoft.VisualBasic.Constants.vbTextCompare) != 0)
				{
					tempCheckServer = false;
				}
				else
				{
					SplitArray = EmailServer.Split('.');
					if (SplitArray.GetUpperBound(0) > 0)
					{
						if ((SplitArray[0].Length > 0) && (SplitArray[1].Length > 0))
						{
							tempCheckServer = true;
						}
					}
				}
				return tempCheckServer;
				//
			}
			catch
			{
				cpCore.handleException( ex );
			}
//ErrorTrap:
			tempCheckServer = false;
//INSTANT C# TODO TASK: Calls to the VB 'Err' function are not converted by Instant C#:
			//Microsoft.VisualBasic.Information.Err().Clear();
			return tempCheckServer;
		}
		//
		//
		//
		//
		//
		private string LocalReturnAddress = "";
		public string ReturnAddress
		{
			get
			{
				return LocalReturnAddress;
			}
			set
			{
				LocalReturnAddress = value;
			}
		}
		//
		private string LocalReplyToAddress = "";
		public string ReplyToAddress
		{
			get
			{
				return LocalReplyToAddress;
			}
			set
			{
				LocalReplyToAddress = value;
			}
		}
		//
		//Public ReplyToAddress As String
		//
		public string sendEmail6(string SMTPServer, string ToAddress, string fromAddress, string subject, string Body, string AttachmentFilename = "", string HTMLBody = "")
		{
			string status = "";
			try
			{
				//this is an error
				SmtpClient client = new SmtpClient(SMTPServer);
				MailMessage mailMessage = new MailMessage();
				MailAddress fromAddresses = new MailAddress(fromAddress.Trim());
				Attachment data = null;
				ContentDisposition disposition = null;
				ContentType mimeType = null;
				AlternateView alternate = null;
				//
				mailMessage.From = fromAddresses;
				mailMessage.To.Add(new MailAddress(ToAddress.Trim()));
				mailMessage.Subject = subject;
				client.EnableSsl = false;
				client.UseDefaultCredentials = false;
				//
				if ((string.IsNullOrEmpty(Body)) && (!string.IsNullOrEmpty(HTMLBody)))
				{
					//
					// html only
					//
					mailMessage.Body = HTMLBody;
					mailMessage.IsBodyHtml = true;
				}
				else if ((!string.IsNullOrEmpty(Body)) && (string.IsNullOrEmpty(HTMLBody)))
				{
					//
					// text body only
					//
					mailMessage.Body = Body;
					mailMessage.IsBodyHtml = false;
				}
				else
				{
					//
					// both html and text
					//
					mailMessage.Body = Body;
					mailMessage.IsBodyHtml = false;
					mimeType = new System.Net.Mime.ContentType("text/html");
					alternate = AlternateView.CreateAlternateViewFromString(HTMLBody, mimeType);
					mailMessage.AlternateViews.Add(alternate);
				}
				//
				// Create  the file attachment for this e-mail message.
				//
				if (!string.IsNullOrEmpty(AttachmentFilename))
				{
					data = new Attachment(AttachmentFilename, MediaTypeNames.Application.Octet);
					disposition = data.ContentDisposition;
					disposition.CreationDate = System.IO.File.GetCreationTime(AttachmentFilename);
					disposition.ModificationDate = System.IO.File.GetLastWriteTime(AttachmentFilename);
					disposition.ReadDate = System.IO.File.GetLastAccessTime(AttachmentFilename);
					mailMessage.Attachments.Add(data);
				}
				//
				// Send the message.
				//
				//Add credentials if the SMTP server requires them.
				//client.Credentials = CredentialCache.DefaultNetworkCredentials;
				//client.Credentials = basicCredential;
				//
				// if there is an error sending, an exception is thrown
				//
				client.Send(mailMessage);
				status = "ok";
			}
			catch (Exception ex)
			{
				//
				//
				//
				status = ex.Message;
			}
			return status;
		}
		//
	}
}
