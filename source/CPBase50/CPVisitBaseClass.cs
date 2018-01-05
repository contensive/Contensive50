//========================================================================



//========================================================================

//
// documentation should be in a new project that inherits these classes. The class names should be the object names in the actual cp project
//


using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Contensive.BaseClasses
{
	public abstract class CPVisitBaseClass
	{
		//Public Sub New(ByVal cmcObj As Contensive.Core.cpCoreClass, ByRef CPParent As CPBaseClass)
		public abstract bool CookieSupport {get;} //Implements BaseClasses.CPVisitBaseClass.CookieSupport
		public abstract string GetProperty(string PropertyName, string DefaultValue = "", int TargetVisitId = 0); //Implements BaseClasses.CPVisitBaseClass.GetProperty
		public abstract string GetText(string PropertyName, string DefaultValue = "");
		public abstract bool GetBoolean(string PropertyName, string DefaultValue = "");
		public abstract DateTime GetDate(string PropertyName, string DefaultValue = "");
		public abstract int GetInteger(string PropertyName, string DefaultValue = "");
		public abstract double GetNumber(string PropertyName, string DefaultValue = "");
		//Public MustOverride Function IsProperty(ByVal PropertyName As String) As Boolean
		public abstract int Id {get;} //Implements BaseClasses.CPVisitBaseClass.Id
		public abstract DateTime LastTime {get;} //Implements BaseClasses.CPVisitBaseClass.LastTime
		public abstract int LoginAttempts {get;} //Implements BaseClasses.CPVisitBaseClass.LoginAttempts
		public abstract string Name {get;} //Implements BaseClasses.CPVisitBaseClass.Name
		public abstract int Pages {get;} //Implements BaseClasses.CPVisitBaseClass.Pages
		public abstract string Referer {get;} //Implements BaseClasses.CPVisitBaseClass.Referer
		public abstract void SetProperty(string PropertyName, string Value, int TargetVisitId = 0); //Implements BaseClasses.CPVisitBaseClass.SetProperty
		public abstract int StartDateValue {get;} //Implements BaseClasses.CPVisitBaseClass.StartDateValue
		public abstract DateTime StartTime {get;} //Implements BaseClasses.CPVisitBaseClass.StartTime
	}

}

