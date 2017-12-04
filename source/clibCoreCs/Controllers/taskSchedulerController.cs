using System;

//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

//
using Contensive.Core;
using Contensive.Core.Controllers;
//
namespace Contensive.Core.Controllers
{
//INSTANT C# TODO TASK: C# compiler constants cannot be set to explicit values:
#Const includeTracing = false
	public class taskSchedulerController : IDisposable
	{
		//
		//Private cpCore As cpCoreClass
		//
		// ----- Log File
		//
		private const string LogMsg = "For more information, see the Contensive Trace Log.";
		public bool allowVerboseLogging = true;
		public bool allowConsoleWrite = false;
		//
		// ----- Task Timer
		//
		private System.Timers.Timer processTimer;
		private int ProcessTimerTickCnt;
		private const int ProcessTimerMsecPerTick = 5100; // Check processs every 5 seconds
		private bool ProcessTimerInProcess;
		private int ProcessTimerProcessCount;
		//
		// ----- Debugging
		//
		public bool StartServiceInProgress;
		//
		protected bool disposed = false;
		//'
		//'========================================================================================================
		//''' <summary>
		//''' constructor
		//''' </summary>
		//''' <param name="cpCore"></param>
		//''' <remarks></remarks>
		//Public Sub New()
		//    MyBase.New
		//End Sub
		//
		//========================================================================================================
		/// <summary>
		/// dispose
		/// </summary>
		/// <param name="disposing"></param>
		/// <remarks></remarks>
		protected virtual void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				this.disposed = true;
				if (disposing)
				{
					//
					// call .dispose for managed objects
					//
					//cpCore.dispose()
				}
				//
				// cp  creates and destroys cmc
				//
				GC.Collect();
			}
		}
		//
		//====================================================================================================
		/// <summary>
		/// Stop all activity through the content server, but do not unload
		/// </summary>
		public void stopTimerEvents()
		{
			try
			{
				//appendLog("taskScheduleServiceClass.stopService")
				processTimer.Enabled = false;
			}
			catch (Exception ex)
			{
				using (CPClass cp = new CPClass())
				{
					cp.core.handleException(ex);
				}
			}
		}
		//
		//====================================================================================================
		/// <summary>
		/// Process the Start signal from the Server Control Manager
		/// </summary>
		/// <param name="setVerbose"></param>
		/// <param name="singleThreaded"></param>
		/// <returns></returns>
		public bool startTimerEvents(bool setVerbose, bool singleThreaded)
		{
			bool returnStartedOk = false;
			try
			{
				//appendLog("taskScheduleServiceClass.startService")
				//
				if (StartServiceInProgress)
				{
					//appendLog("taskScheduleServiceClass.startService, startServiceInProgress true, skip.")
				}
				else
				{
					StartServiceInProgress = true;
					processTimer = new System.Timers.Timer(5000);
					processTimer.Elapsed += processTimerTick;
					processTimer.Interval = ProcessTimerMsecPerTick;
					processTimer.Enabled = true;
					returnStartedOk = true;
					StartServiceInProgress = false;
				}
			}
			catch (Exception ex)
			{
				using (CPClass cp = new CPClass())
				{
					cp.core.handleException(ex);
				}
			}
			return returnStartedOk;
		}
		//
		//====================================================================================================
		/// <summary>
		/// Timer tick
		/// </summary>
		public void processTimerTick(object sender, EventArgs e)
		{
			try
			{
				Console.WriteLine("taskScheduleServiceClass.processTimerTick");
				Stopwatch sw = new Stopwatch();
				sw.Start();
				if (ProcessTimerInProcess)
				{
					Console.WriteLine("tmp-taskScheduleServiceClass.processTimerTick, skipped because timerInProcess");
				}
				else
				{
					//
					// -- schedule tasks
					ProcessTimerInProcess = true;
					using (CPClass cp = new CPClass())
					{
						if (!cp.core.serverConfig.allowTaskSchedulerService)
						{
							Console.WriteLine("taskScheduleServiceClass.processTimerTick, skipped because serviceConfig.allowTaskSchedulerService false.");
						}
						else
						{
							Console.WriteLine("taskScheduleServiceClass.processTimerTick, call scheduleTasks.");
							scheduleTasks(cp.core);
						}
						//End Using
					}
					ProcessTimerInProcess = false;
				}
				Console.WriteLine("taskScheduleServiceClass.processTimerTick exit (" + sw.ElapsedMilliseconds + "ms)");
			}
			catch (Exception ex)
			{
				using (CPClass cp = new CPClass())
				{
					cp.core.handleException(ex);
				}
			}
		}
		//
		//====================================================================================================
		/// <summary>
		/// Iterate through all apps, find addosn that need to run and add them to the task queue
		/// </summary>
		private void scheduleTasks(coreClass cpClusterCore)
		{
			string hint = "";
			try
			{
				//
				logController.appendLog(cpClusterCore, "taskScheduler.scheduleTasks");
				//
				foreach (KeyValuePair<string, Models.Entity.serverConfigModel.appConfigModel> kvp in cpClusterCore.serverConfig.apps)
				{
					//
					// schedule tasks for this app
					//
					logController.appendLog(cpClusterCore, "taskScheduler.scheduleTasks, app=[" + kvp.Value.name + "]");
					//
					using (CPClass cpSite = new CPClass(kvp.Value.name))
					{
						if (!(cpSite.core.serverConfig.appConfig.appStatus == Models.Entity.serverConfigModel.appStatusEnum.OK))
						{
							//
							logController.appendLog(cpClusterCore, "taskScheduler.scheduleTasks, app status not ok");
							//
						}
						else if (!(cpSite.core.serverConfig.appConfig.appMode == Models.Entity.serverConfigModel.appModeEnum.normal))
						{
							//
							logController.appendLog(cpClusterCore, "taskScheduler.scheduleTasks, app mode not normal");
							//
						}
						else
						{
							//
							// Execute Processes
							//
							try
							{
								//
								logController.appendLog(cpClusterCore, "taskScheduler.scheduleTasks, search for addons to run");
								//
								DateTime RightNow = DateTime.Now;
								string SQLNow = cpSite.core.db.encodeSQLDate(RightNow);
								string sqlAddonsCriteria = ""
									+ "(Active<>0)"
									+ " and(name<>'')"
									+ " and("
									+ "  ((ProcessRunOnce is not null)and(ProcessRunOnce<>0))"
									+ "  or((ProcessInterval is not null)and(ProcessInterval<>0)and(ProcessNextRun is null))"
									+ "  or(ProcessNextRun<" + SQLNow + ")"
									+ " )";
								int CS = cpSite.core.db.csOpen(cnAddons, sqlAddonsCriteria);
								while (cpSite.core.db.csOk(CS))
								{
									int addonProcessInterval = cpSite.core.db.csGetInteger(CS, "ProcessInterval");
									string addonName = cpSite.core.db.csGetText(CS, "name");
									bool addonProcessRunOnce = cpSite.core.db.csGetBoolean(CS, "ProcessRunOnce");
									DateTime addonProcessNextRun = cpSite.core.db.csGetDate(CS, "ProcessNextRun");
									DateTime NextRun = DateTime.MinValue;
									hint += ",run addon " + addonName;
									if (addonProcessInterval > 0)
									{
										NextRun = RightNow.AddMinutes(addonProcessInterval);
									}
									if ((addonProcessNextRun < RightNow) || (addonProcessRunOnce))
									{
										//
										logController.appendLog(cpClusterCore, "taskScheduler.scheduleTasks, add task for addon [" + addonName + "], addonProcessRunOnce [" + addonProcessRunOnce + "], addonProcessNextRun [" + addonProcessNextRun + "]");
										//
										// -- resolve triggering state
										cpSite.core.db.csSet(CS, "ProcessRunOnce", false);
										if (addonProcessNextRun < RightNow)
										{
											cpSite.core.db.csSet(CS, "ProcessNextRun", NextRun);
										}
										cpSite.core.db.csSave2(CS);
										//
										// -- add task to queue for runner
										cmdDetailClass cmdDetail = new cmdDetailClass();
										cmdDetail.addonId = cpSite.core.db.csGetInteger(CS, "ID");
										cmdDetail.addonName = addonName;
										cmdDetail.docProperties = genericController.convertAddonArgumentstoDocPropertiesList(cpSite.core, cpSite.core.db.csGetText(CS, "argumentlist"));
										addTaskToQueue(cpSite.core, taskQueueCommandEnumModule.runAddon, cmdDetail, false);
									}
									else if (cpSite.core.db.csGetDate(CS, "ProcessNextRun") == DateTime.MinValue)
									{
										//
										logController.appendLog(cpClusterCore, "taskScheduler.scheduleTasks, addon [" + addonName + "], ProcessInterval set but no processNextRun, set processNextRun [" + NextRun + "]");
										//
										// -- Interval is OK but NextRun is 0, just set next run
										cpSite.core.db.csSet(CS, "ProcessNextRun", NextRun);
										cpSite.core.db.csSave2(CS);
									}
									cpSite.core.db.csGoNext(CS);
								}
								cpSite.core.db.csClose(CS);
							}
							catch (Exception ex)
							{
								//
								logController.appendLog(cpClusterCore, "taskScheduler.scheduleTasks, execption scheduling addon");
								//
								cpClusterCore.handleException(ex);
							}
						}
					}
					hint += ",app done";
				}
			}
			catch (Exception ex)
			{
				cpClusterCore.handleException(ex);
			}
		}
		//
		//====================================================================================================
		/// <summary>
		/// Add a command task to the taskQueue to be run by the taskRunner. Returns false if the task was already there (dups fround by command name and cmdDetailJson)
		/// </summary>
		/// <param name="cpSiteCore"></param>
		/// <param name="Command"></param>
		/// <param name="cmdDetail"></param>
		/// <param name="BlockDuplicates"></param>
		/// <returns></returns>
		public bool addTaskToQueue(coreClass cpSiteCore, string Command, cmdDetailClass cmdDetail, bool BlockDuplicates)
		{
			bool returnTaskAdded = true;
			try
			{
				string LcaseCommand = null;
				string sql = null;
				string cmdDetailJson = cpSiteCore.json.Serialize(cmdDetail);
				int cs = 0;
				//
				logController.appendLog(cpSiteCore, "taskScheduler.addTaskToQueue, application=[" + cpSiteCore.serverConfig.appConfig.name + "], command=[" + Command + "], cmdDetail=[" + cmdDetailJson + "]");
				//
				returnTaskAdded = true;
				LcaseCommand = genericController.vbLCase(Command);
				if (BlockDuplicates)
				{
					//
					// Search for a duplicate
					//
					sql = "select top 1 id from cctasks where ((command=" + cpSiteCore.db.encodeSQLText(Command) + ")and(cmdDetail=" + cmdDetailJson + ")and(datestarted is not null))";
					cs = cpSiteCore.db.csOpenSql(sql);
					if (cpSiteCore.db.csOk(cs))
					{
						returnTaskAdded = false;
					}
					cpSiteCore.db.csClose(cs);
				}
				//
				// Add it to the queue and shell out to the command
				//
				if (!returnTaskAdded)
				{
					//
					logController.appendLog(cpSiteCore, "taskScheduler.addTaskToQueue, command skipped because the unstarted command and details were already in the queue.");
					//
				}
				else
				{
					//
					logController.appendLog(cpSiteCore, "taskScheduler.addTaskToQueue, application=[" + cpSiteCore.serverConfig.appConfig.name + "], command=[" + Command + "], cmdDetail=[" + cmdDetailJson + "]");
					//
					cs = cpSiteCore.db.csInsertRecord("tasks");
					if (cpSiteCore.db.csOk(cs))
					{
						cpSiteCore.db.csSet(cs, "name", "command [" + Command + "], addon [#" + cmdDetail.addonId + "," + cmdDetail.addonName + "]");
						cpSiteCore.db.csSet(cs, "command", Command);
						cpSiteCore.db.csSet(cs, "cmdDetail", cmdDetailJson);
						returnTaskAdded = true;
					}
					cpSiteCore.db.csClose(cs);
				}
			}
			catch (Exception ex)
			{
				cpSiteCore.handleException(ex);
				throw;
			}
			return returnTaskAdded;
		}
		//'
		//Private Sub appendLog(cpCore As coreClass, ByVal logText As String, Optional isImportant As Boolean = False)
		//    If (isImportant Or allowVerboseLogging) Then
		//        logController.appendLog(cpCore, logText, "", "trace")
		//    End If
		//    If (allowConsoleWrite) Then
		//        Console.WriteLine(logText)
		//    End If
		//End Sub
		//
		//
		//
		public static void tasks_RequestTask(coreClass cpCore, string Command, string SQL, string ExportName, string Filename, int RequestedByMemberID)
		{
			int CS = 0;
			string TaskName = null;
			//
			if (string.IsNullOrEmpty(ExportName))
			{
				TaskName = Convert.ToString(DateTime.Now) + " snapshot of unnamed data";
			}
			else
			{
				TaskName = Convert.ToString(DateTime.Now) + " snapshot of " + genericController.vbLCase(ExportName);
			}
			CS = cpCore.db.csInsertRecord("Tasks", RequestedByMemberID);
			if (cpCore.db.csOk(CS))
			{
				cpCore.db.csGetFilename(CS, "Filename", Filename);
				cpCore.db.csSet(CS, "Name", TaskName);
				cpCore.db.csSet(CS, "Command", Command);
				cpCore.db.csSet(CS, "SQLQuery", SQL);
			}
			cpCore.db.csClose(CS);
		}
		public static void main_RequestTask(coreClass cpCore, string Command, string SQL, string ExportName, string Filename)
		{
			tasks_RequestTask(cpCore, genericController.encodeText(Command), genericController.encodeText(SQL), genericController.encodeText(ExportName), genericController.encodeText(Filename), genericController.EncodeInteger(cpCore.doc.authContext.user.id));
		}

#region  IDisposable Support 
		// Do not change or add Overridable to these methods.
		// Put cleanup code in Dispose(ByVal disposing As Boolean).
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		~taskSchedulerController()
		{
			Dispose(false);
//INSTANT C# NOTE: The base class Finalize method is automatically called from the destructor:
			//base.Finalize();
		}
#endregion
	}

}
