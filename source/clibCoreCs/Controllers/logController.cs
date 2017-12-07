using System;

//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

//
using System.Text.RegularExpressions;
using Contensive.Core.Controllers;
//
namespace Contensive.Core.Controllers
{
	//
	//====================================================================================================
	/// <summary>
	/// static class controller
	/// </summary>
	public class logController
	{
		//
		// ----- constants
		//
		//Private Const invalidationDaysDefault As Double = 365
		//
		// ----- private instance storage
		//
		//Private remoteCacheDisabled As Boolean
		//
		public static string LogFileCopyPrep(string Source)
		{
			string Copy = Source;
			Copy = genericController.vbReplace(Copy, Environment.NewLine, " ");
			Copy = genericController.vbReplace(Copy, "\n", " ");
			Copy = genericController.vbReplace(Copy, "\r", " ");
			return Copy;
		}
		//
		//=============================================================================
		/// <summary>
		/// add the log line to a log file with the folder and prefix
		/// </summary>
		/// <param name="cpCore"></param>
		/// <param name="LogLine"></param>
		/// <param name="LogFolder"></param>
		/// <param name="LogNamePrefix"></param>
		/// <param name="allowErrorHandling"></param>
		/// <remarks></remarks>
		public static void appendLog(coreClass cpCore, string LogLine, string LogFolder = "", string LogNamePrefix = "", bool allowErrorHandling = true)
		{
			try
			{
				//
				int threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
				string threadName = threadId.ToString("00000000");
				string absContent = LogFileCopyPrep(DateTime.Now.ToString("")) + "\t" + "thread:" + threadName + "\t" + LogLine + Environment.NewLine;
				Console.WriteLine(LogLine);
				//
				if (string.IsNullOrEmpty(LogFolder) || cpCore.serverConfig.enableLogging)
				{
					try
					{
						fileController fileSystem = null;
						if (cpCore.serverConfig != null)
						{
							if (cpCore.serverConfig.appConfig != null)
							{
								//
								// -- use app log space
								fileSystem = cpCore.privateFiles;
							}
						}
						if (fileSystem == null)
						{
							//
							// -- no app or no server, use program data files
							fileSystem = cpCore.programDataFiles;
						}
						string FilenameNoExt = getDateString(DateTime.Now);
						string logPath = LogFolder;
						if (!string.IsNullOrEmpty(logPath))
						{
							logPath = logPath + "\\";
						}
						logPath = "logs\\" + logPath;
						//
						// check for serverconfig, then for appConfig, else use programdata folder
						//
						int FileSize = 0;
						if (!fileSystem.pathExists(logPath))
						{
							fileSystem.createPath(logPath);
						}
						else
						{
							IO.FileInfo[] logFiles = fileSystem.getFileList(logPath);
							foreach (IO.FileInfo fileInfo in logFiles)
							{
								if (fileInfo.Name.ToLower() == FilenameNoExt.ToLower() + ".log")
								{
									FileSize = (int)fileInfo.Length;
									break;
								}
							}
						}
						string PathFilenameNoExt = logPath + FilenameNoExt;
						//
						// -- add to log file
						if (FileSize < 10000000)
						{
							int RetryCnt = 0;
							bool SaveOK = false;
							string FileSuffix = "";
							while ((!SaveOK) && (RetryCnt < 10))
							{
								SaveOK = true;
								try
								{
									fileSystem.appendFile(PathFilenameNoExt + FileSuffix + ".log", absContent);
								}
								catch (IO.IOException ex)
								{
									//
									// permission denied - happens when more then one process are writing at once, go to the next suffix
									//
									FileSuffix = "-" + (RetryCnt + 1).ToString();
									RetryCnt = RetryCnt + 1;
									SaveOK = false;
								}
								catch (Exception ex)
								{
									//
									// unknown error
									//
									FileSuffix = "-" + (RetryCnt + 1).ToString();
									RetryCnt = RetryCnt + 1;
									SaveOK = false;
								}
							}
						}
					}
					catch (Exception ex)
					{
						// -- ignore errors in error handling
					}
				}
			}
			catch (Exception ex)
			{
				// -- ignore errors in error handling
			}
			finally
			{
				//
			}
		}
		//
		//========================================================================
		/// <summary>
		/// Append log, use the legacy row with tab delimited context
		/// </summary>
		/// <param name="cpCore"></param>
		/// <param name="ContensiveAppName"></param>
		/// <param name="contextDescription"></param>
		/// <param name="processName"></param>
		/// <param name="ClassName"></param>
		/// <param name="MethodName"></param>
		/// <param name="ErrNumber"></param>
		/// <param name="ErrSource"></param>
		/// <param name="ErrDescription"></param>
		/// <param name="ErrorTrap"></param>
		/// <param name="ResumeNextAfterLogging"></param>
		/// <param name="URL"></param>
		/// <param name="LogFolder"></param>
		/// <param name="LogNamePrefix"></param>
		/// <remarks></remarks>
		public static void appendLogWithLegacyRow(coreClass cpCore, string ContensiveAppName, string contextDescription, string processName, string ClassName, string MethodName, int ErrNumber, string ErrSource, string ErrDescription, bool ErrorTrap, bool ResumeNextAfterLogging, string URL, string LogFolder, string LogNamePrefix)
		{
			try
			{
				string ErrorMessage = null;
				string LogLine = null;
				string ResumeMessage = null;
				//
				if (ErrorTrap)
				{
					ErrorMessage = "Error Trap";
				}
				else
				{
					ErrorMessage = "Log Entry";
				}
				//
				if (ResumeNextAfterLogging)
				{
					ResumeMessage = "Resume after logging";
				}
				else
				{
					ResumeMessage = "Abort after logging";
				}
				//
				LogLine = ""
					+ LogFileCopyPrep(ContensiveAppName) + "\t" + LogFileCopyPrep(processName) + "\t" + LogFileCopyPrep(ClassName) + "\t" + LogFileCopyPrep(MethodName) + "\t" + LogFileCopyPrep(contextDescription) + "\t" + LogFileCopyPrep(ErrorMessage) + "\t" + LogFileCopyPrep(ResumeMessage) + "\t" + LogFileCopyPrep(ErrSource) + "\t" + LogFileCopyPrep(ErrNumber.ToString()) + "\t" + LogFileCopyPrep(ErrDescription) + "\t" + LogFileCopyPrep(URL) + "";
				//
				appendLog(cpCore, LogLine, LogFolder, LogNamePrefix);
			}
			catch (Exception ex)
			{

			}
		}

		//
		//====================================================================================================
		/// <summary>
		/// Create a string with year, month, date in the form 20151206
		/// </summary>
		/// <param name="sourceDate"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		public static string getDateString(DateTime sourceDate)
		{
			return sourceDate.Year + sourceDate.Month.ToString().PadLeft(2, '0') + sourceDate.Day.ToString().PadLeft(2, '0');
		}
		//
		//=====================================================================================================
		//   Insert into the ActivityLog
		//=====================================================================================================
		//
		public static void logActivity(coreClass cpcore, string Message, int ByMemberID, int SubjectMemberID, int SubjectOrganizationID, string Link = "", int VisitorID = 0, int VisitID = 0)
		{
			try
			{
				//
				int CS;
				//
				CS = cpcore.db.csInsertRecord("Activity Log", ByMemberID);
				if (cpcore.db.csOk(CS))
				{
					cpcore.db.csSet(CS, "MemberID", SubjectMemberID);
					cpcore.db.csSet(CS, "OrganizationID", SubjectOrganizationID);
					cpcore.db.csSet(CS, "Message", Message);
					cpcore.db.csSet(CS, "Link", Link);
					cpcore.db.csSet(CS, "VisitorID", VisitorID);
					cpcore.db.csSet(CS, "VisitID", VisitID);
				}
				cpcore.db.csClose(ref CS);
				//
				return;
				//
			}
			catch
			{
				cpCore.handleException( ex );
			}
//ErrorTrap:
			throw (new Exception("Unexpected exception"));
		}
		//
		//
		public static void logActivity2(coreClass cpcore, string Message, int SubjectMemberID, int SubjectOrganizationID)
		{
			logActivity(cpcore, Message, cpcore.doc.authContext.user.id, SubjectMemberID, SubjectOrganizationID, cpcore.webServer.requestUrl, cpcore.doc.authContext.visitor.ID, cpcore.doc.authContext.visit.id);
		}
		//
		//
		//
		internal static void log_appendLogPageNotFound(coreClass cpCore, string PageNotFoundLink)
		{
			try
			{
				appendLog(cpCore, "\"" + cpCore.doc.profileStartTime.ToString("") + "\",\"App=" + cpCore.serverConfig.appConfig.name + "\",\"main_VisitId=" + cpCore.doc.authContext.visit.id + "\",\"" + PageNotFoundLink + "\",\"Referrer=" + cpCore.webServer.requestReferrer + "\"", "performance", "pagenotfound");
			}
			catch (Exception ex)
			{
				throw (ex);
			}
		}
		//
		//================================================================================================
		//   Report Warning
		//       A warning is logged in the site warnings log
		//           name - a generic description of the warning
		//               "bad link found on page"
		//           short description - a <255 character cause
		//               "bad link http://thisisabadlink.com"
		//           location - the URL, service or process that caused the problem
		//               "http://goodpageThankHasBadLink.com"
		//           pageid - the record id of the bad page.
		//               "http://goodpageThankHasBadLink.com"
		//           description - a specific description
		//               "link to http://www.this.com/pagename was found on http://www.this.com/About-us"
		//           generalKey - a generic string that describes the warning. the warning report
		//               will display one line for each generalKey (name matches guid)
		//               like "bad link"
		//           specificKey - a string created by the addon logging so it does not continue to log exactly the
		//               same warning over and over. If there are 100 different link not found warnings,
		//               there should be 100 entires with the same guid and name, but 100 different keys. If the
		//               an identical key is found the count increments.
		//               specifickey is like "link to http://www.this.com/pagename was found on http://www.this.com/About-us"
		//           count - the number of times the key was attempted to add. "This error was reported 100 times"
		//================================================================================================
		//
		public static void csv_reportWarning(coreClass cpcore, string Name, string shortDescription, string location, int PageID, string Description, string generalKey, string specificKey)
		{
			string SQL = null;
			int warningId = 0;
			int CS = 0;
			//
			warningId = 0;
			SQL = "select top 1 ID from ccSiteWarnings"
				+ " where (generalKey=" + cpcore.db.encodeSQLText(generalKey) + ")"
				+ " and(specificKey=" + cpcore.db.encodeSQLText(specificKey) + ")"
				+ "";
			DataTable dt = cpcore.db.executeQuery(SQL);
			if (dt.Rows.Count > 0)
			{
				warningId = genericController.EncodeInteger(dt.Rows[0].Item("id"));
			}
			//
			if (warningId != 0)
			{
				//
				// increment count for matching warning
				//
				SQL = "update ccsitewarnings set count=count+1,DateLastReported=" + cpcore.db.encodeSQLDate(DateTime.Now) + " where id=" + warningId;
				cpcore.db.executeQuery(SQL);
			}
			else
			{
				//
				// insert new record
				//
				CS = cpcore.db.csInsertRecord("Site Warnings", 0);
				if (cpcore.db.csOk(CS))
				{
					cpcore.db.csSet(CS, "name", Name);
					cpcore.db.csSet(CS, "description", Description);
					cpcore.db.csSet(CS, "generalKey", generalKey);
					cpcore.db.csSet(CS, "specificKey", specificKey);
					cpcore.db.csSet(CS, "count", 1);
					cpcore.db.csSet(CS, "DateLastReported", DateTime.Now);
					if (true)
					{
						cpcore.db.csSet(CS, "shortDescription", shortDescription);
						cpcore.db.csSet(CS, "location", location);
						cpcore.db.csSet(CS, "pageId", PageID);
					}
				}
				cpcore.db.csClose(ref CS);
			}
			//
		}
		//
		//====================================================================================================
		//
		internal static void appendInstallLog(coreClass cpCore, string message)
		{
			Console.WriteLine("install, " + message);
			appendLog(cpCore, message, "install");
		}
	}
}