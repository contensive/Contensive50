using System;

//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

using Contensive.BaseClasses;
using Contensive.Core.Controllers;
using System.Runtime.InteropServices;

namespace Contensive.Core
{
	//
	// comVisible to be activeScript compatible
	//
	[ComVisible(true), Microsoft.VisualBasic.ComClass(CPUserErrorClass.ClassId, CPUserErrorClass.InterfaceId, CPUserErrorClass.EventsId)]
	public class CPUserErrorClass : BaseClasses.CPUserErrorBaseClass, IDisposable
	{
		//
#region COM GUIDs
		public const string ClassId = "C175C292-0130-409E-9621-B618F89F4EEC";
		public const string InterfaceId = "C06DB080-41AE-4F1B-A477-B3CF74F61708";
		public const string EventsId = "B784BFEF-127B-48D5-8C99-B075984227DB";
#endregion
		//
		private Contensive.Core.coreClass cpCore;
		protected bool disposed = false;
		//
		public CPUserErrorClass(Contensive.Core.coreClass cpCoreObj) : base()
		{
			cpCore = cpCoreObj;
		}
		//
		// dispose
		//
		protected virtual void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				appendDebugLog(".dispose, dereference main, csv");
				if (disposing)
				{
					//
					// call .dispose for managed objects
					//
					cpCore = null;
				}
				//
				// Add code here to release the unmanaged resource.
				//
			}
			this.disposed = true;
		}
		//
		//
		//
		public override void Add(string Message) //Inherits BaseClasses.CPUserErrorBaseClass.Add
		{
			if (true)
			{
				errorController.error_AddUserError(cpCore,Message);
			}
		}

		public override string GetList() //Inherits BaseClasses.CPUserErrorBaseClass.GetList
		{
			if (true)
			{
				return errorController.error_GetUserError(cpCore);
			}
			else
			{
				return "";
			}
		}

		public override bool OK() //Inherits BaseClasses.CPUserErrorBaseClass.OK
		{
			if (true)
			{
				return !(cpCore.doc.debug_iUserError != "");
			}
			else
			{
				return true;
			}
		}
		//
		//
		//
		private void appendDebugLog(string copy)
		{
			//My.Computer.FileSystem.WriteAllText("c:\clibCpDebug.log", Now & " - cp.userError, " & copy & vbCrLf, True)
			// 'My.Computer.FileSystem.WriteAllText(System.AppDocmc.main_CurrentDocmc.main_BaseDirectory() & "cpLog.txt", Now & " - " & copy & vbCrLf, True)
		}
		//
		// testpoint
		//
		private void tp(string msg)
		{
			//Call appendDebugLog(msg)
		}
#region  IDisposable Support 
		// Do not change or add Overridable to these methods.
		// Put cleanup code in Dispose(ByVal disposing As Boolean).
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		~CPUserErrorClass()
		{
			Dispose(false);
//INSTANT C# NOTE: The base class Finalize method is automatically called from the destructor:
			//base.Finalize();
		}
#endregion
	}
}