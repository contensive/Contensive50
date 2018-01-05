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
	public abstract class CPVisitorBaseClass
	{
		//Public Sub New(ByVal cmcObj As Contensive.Core.cpCoreClass, ByRef CPParent As CPBaseClass)
		public abstract bool ForceBrowserMobile {get;} //Implements BaseClasses.CPVisitorBaseClass.ForceBrowserMobile
		public abstract string GetProperty(string PropertyName, string DefaultValue = "", int TargetVisitorId = 0); //Implements BaseClasses.CPVisitorBaseClass.GetProperty
		public abstract string GetText(string PropertyName, string DefaultValue = "");
		public abstract bool GetBoolean(string PropertyName, string DefaultValue = "");
		public abstract DateTime GetDate(string PropertyName, string DefaultValue = "");
		public abstract int GetInteger(string PropertyName, string DefaultValue = "");
		public abstract double GetNumber(string PropertyName, string DefaultValue = "");
		//Public MustOverride Function IsProperty(ByVal PropertyName As String) As Boolean
		public abstract int Id {get;} //Implements BaseClasses.CPVisitorBaseClass.Id
		public abstract bool IsNew {get;} //Implements BaseClasses.CPVisitorBaseClass.IsNew
		public abstract void SetProperty(string PropertyName, string Value, int TargetVisitorid = 0); //Implements BaseClasses.CPVisitorBaseClass.SetProperty
		public abstract int UserId {get;} //Implements BaseClasses.CPVisitorBaseClass.UserId
	}

}

