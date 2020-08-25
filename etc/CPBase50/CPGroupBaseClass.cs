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
	public abstract class CPGroupBaseClass
	{
		//
		// documentation should be in a new project that inherits these classes. The class names should be the object names in the actual cp project
		//
		public abstract void Add(string GroupName, string GroupCaption = ""); //Implements BaseClasses.CPGroupBaseClass.Add
        public abstract void AddUser(string GroupNameIdOrGuid);
        public abstract void AddUser(string GroupNameIdOrGuid, int UserId);
        public abstract void AddUser(string GroupNameIdOrGuid, int UserId, DateTime DateExpires);
        public abstract void Delete(string GroupNameIdOrGuid); //Implements BaseClasses.CPGroupBaseClass.Delete
		public abstract int GetId(string GroupNameIdOrGuid); //Implements BaseClasses.CPGroupBaseClass.GetId
		public abstract string GetName(string GroupNameIdOrGuid); //Implements BaseClasses.CPGroupBaseClass.GetName
		public abstract void RemoveUser(string GroupNameIdOrGuid, int UserId = 0); //Implements BaseClasses.CPGroupBaseClass.RemoveUser
	}

}

