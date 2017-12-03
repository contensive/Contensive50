//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

//
// documentation should be in a new project that inherits these classes. The class names should be the object names in the actual cp project
//

//INSTANT C# NOTE: Formerly VB project-level imports:
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Contensive.BaseClasses
{
	public abstract class CPUserErrorBaseClass
	{
		//Public Sub New(ByVal cmcObj As Contensive.Core.cpCoreClass, ByRef CPParent As CPBaseClass)
		public abstract void Add(string Message); //Implements BaseClasses.CPUserErrorBaseClass.Add
		public abstract string GetList(); //Implements BaseClasses.CPUserErrorBaseClass.GetList
		public abstract bool OK(); //Implements BaseClasses.CPUserErrorBaseClass.OK
	}

}

