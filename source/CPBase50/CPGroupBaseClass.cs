
using System;

namespace Contensive.BaseClasses
{
	public abstract class CPGroupBaseClass
	{
		//
		// documentation should be in a new project that inherits these classes. The class names should be the object names in the actual cp project
		//
		public abstract void Add(string GroupName, string GroupCaption = "");
        public abstract void AddUser(string GroupNameIdOrGuid);
        public abstract void AddUser(string GroupNameIdOrGuid, int UserId);
        public abstract void AddUser(string GroupNameIdOrGuid, int UserId, DateTime DateExpires);
        public abstract void Delete(string GroupNameIdOrGuid); 
		public abstract int GetId(string GroupNameIdOrGuid); 
		public abstract string GetName(string GroupNameIdOrGuid); 
		public abstract void RemoveUser(string GroupNameIdOrGuid, int UserId = 0); 
	}

}

