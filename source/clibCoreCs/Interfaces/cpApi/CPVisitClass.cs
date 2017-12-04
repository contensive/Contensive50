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
	[ComVisible(true), Microsoft.VisualBasic.ComClass(CPVisitClass.ClassId, CPVisitClass.InterfaceId, CPVisitClass.EventsId)]
	public class CPVisitClass : BaseClasses.CPVisitBaseClass, IDisposable
	{
		//
#region COM GUIDs
		public const string ClassId = "3562FB08-178D-4AD1-A923-EAEAAF33FE84";
		public const string InterfaceId = "A1CC6FCB-810B-46C4-8232-D3166CACCBAD";
		public const string EventsId = "2AFEB1A8-5B27-45AC-A9DF-F99849BE1FAE";
#endregion
		//
		private Contensive.Core.coreClass cpCore;
		private CPClass cp;
		protected bool disposed = false;
		//
		public CPVisitClass(Contensive.Core.coreClass cpCoreObj, CPClass cpParent) : base()
		{
			this.cpCore = cpCoreObj;
			cp = cpParent;
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
					cp = null;
					cpCore = null;
				}
				//
				// Add code here to release the unmanaged resource.
				//
			}
			this.disposed = true;
		}

		public override bool CookieSupport //Inherits BaseClasses.CPVisitBaseClass.CookieSupport
		{
			get
			{
				if (true)
				{
					return cpCore.doc.authContext.visit.cookieSupport;
				}
				else
				{
					return false;
				}
			}
		}
		//
		//
		//
		public override string GetProperty(string PropertyName, string DefaultValue = "", int TargetVisitId = 0)
		{
			if (TargetVisitId == 0)
			{
				return cpCore.visitProperty.getText(PropertyName, DefaultValue);
			}
			else
			{
				return cpCore.visitProperty.getText(PropertyName, DefaultValue, TargetVisitId);
			}
		}
		//
		//
		//
		public override int Id
		{
			get
			{
				if (true)
				{
					return cpCore.doc.authContext.visit.id;
				}
				else
				{
					return 0;
				}
			}
		}

		public override DateTime LastTime //Inherits BaseClasses.CPVisitBaseClass.LastTime
		{
			get
			{
				if (true)
				{
					return cpCore.doc.authContext.visit.lastvisitTime;
				}
				else
				{
					return new DateTime();
				}
			}
		}

		public override int LoginAttempts //Inherits BaseClasses.CPVisitBaseClass.LoginAttempts
		{
			get
			{
				if (true)
				{
					return cpCore.doc.authContext.visit.loginAttempts;
				}
				else
				{
					return 0;
				}
			}
		}

		public override string Name //Inherits BaseClasses.CPVisitBaseClass.Name
		{
			get
			{
				if (true)
				{
					return cpCore.doc.authContext.visit.name;
				}
				else
				{
					return "";
				}
			}
		}

		public override int Pages //Inherits BaseClasses.CPVisitBaseClass.Pages
		{
			get
			{
				if (true)
				{
					return cpCore.doc.authContext.visit.pagevisits;
				}
				else
				{
					return 0;
				}
			}
		}

		public override string Referer //Inherits BaseClasses.CPVisitBaseClass.Referer
		{
			get
			{
				if (true)
				{
					return cpCore.doc.authContext.visit.http_referer;
				}
				else
				{
					return "";
				}
			}
		}
		//
		//
		//
		public override void SetProperty(string PropertyName, string Value, int TargetVisitId = 0)
		{
			if (TargetVisitId == 0)
			{
				cpCore.visitProperty.setProperty(PropertyName, Value);
			}
			else
			{
				cpCore.visitProperty.setProperty(PropertyName, Value, TargetVisitId);
			}
		}
		//
		//=======================================================================================================
		//
		//=======================================================================================================
		//
		public override bool GetBoolean(string PropertyName, string DefaultValue = "")
		{
			return genericController.EncodeBoolean(GetProperty(PropertyName, DefaultValue));
		}
		//
		//=======================================================================================================
		//
		//=======================================================================================================
		//
		public override DateTime GetDate(string PropertyName, string DefaultValue = "")
		{
			return genericController.EncodeDate(GetProperty(PropertyName, DefaultValue));
		}
		//
		//=======================================================================================================
		//
		//=======================================================================================================
		//
		public override int GetInteger(string PropertyName, string DefaultValue = "")
		{
			return cp.Utils.EncodeInteger(GetProperty(PropertyName, DefaultValue));
		}
		//
		//=======================================================================================================
		//
		//=======================================================================================================
		//
		public override double GetNumber(string PropertyName, string DefaultValue = "")
		{
			return cp.Utils.EncodeNumber(GetProperty(PropertyName, DefaultValue));
		}
		//
		//=======================================================================================================
		//
		//=======================================================================================================
		//
		public override string GetText(string FieldName, string DefaultValue = "")
		{
			return GetProperty(FieldName, DefaultValue);
		}

		public override int StartDateValue //Inherits BaseClasses.CPVisitBaseClass.StartDateValue
		{
			get
			{
				if (true)
				{
					return cpCore.doc.authContext.visit.startDateValue;
				}
				else
				{
					return 0;
				}
			}
		}

		public override DateTime StartTime //Inherits BaseClasses.CPVisitBaseClass.StartTime
		{
			get
			{
				if (true)
				{
					return cpCore.doc.authContext.visit.startTime;
				}
				else
				{
					return new DateTime();
				}
			}
		}
		//
		//
		//
		private void appendDebugLog(string copy)
		{
			//My.Computer.FileSystem.WriteAllText("c:\clibCpDebug.log", Now & " - cp.visit, " & copy & vbCrLf, True)
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
		~CPVisitClass()
		{
			Dispose(false);
//INSTANT C# NOTE: The base class Finalize method is automatically called from the destructor:
			//base.Finalize();
		}
#endregion
	}
}