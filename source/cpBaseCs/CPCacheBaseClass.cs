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
	/// <summary>
	/// CP.Cache - contains features to perform simple caching functions
	/// </summary>
	/// <remarks></remarks>
	public abstract class CPCacheBaseClass
	{
        //private const DateTime nothing;

        //
        /// <summary>
        /// clear all cache based on any content in a list provided
        /// </summary>
        /// <param name="ContentNameList"></param>
        /// <remarks></remarks>
        public abstract void Clear(string ContentNameList);
		/// <summary>
		/// Read the value of a cache. If the cache is cleared, an empty string is returned.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract string Read(string key);
		public abstract object getObject(string key);
		public abstract string getText(string key);
		public abstract int getInteger(string key);
		public abstract double getNumber(string key);
		public abstract DateTime getDate(string key);
		public abstract bool getBoolean(string key);
        /// <summary>
        /// Save a string to a name. If a change is made to any of the content is the given list or if the clearbydate is passed, the cache is cleared.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="Value"></param>
        /// <param name="tagCommaList"></param>
        /// <param name="ClearOnDate"></param>
        /// <remarks></remarks>
        //public abstract void Save(string key, string Value);
        //public abstract void Save(string key, string Value, string tagCommaList);
       
        public abstract void Save(string key, string Value);
        public abstract void Save(string key, string Value, string tagCommaList);
        public abstract void Save(string key, string Value , string tagCommaList , DateTime ClearOnDate );
        public abstract void setKey(string key, object Value);
		public abstract void setKey(string key, object Value, DateTime invalidationDate);
		public abstract void setKey(string key, object Value, List<string> tagList);
		public abstract void setKey(string key, object Value, DateTime invalidationDate, List<string> tagList);
		public abstract void setKey(string key, object Value, string tag);
		public abstract void setKey(string key, object Value, DateTime invalidationDate, string tag);
		/// <summary>
		/// Clear all system caches. Use this call to flush all internal caches.
		/// </summary>
		/// <remarks></remarks>
		public abstract void ClearAll();
		//
		public abstract void InvalidateTag(string tag);
		public abstract void InvalidateAll();
		public abstract void InvalidateTagList(List<string> tagList);
		public abstract void InvalidateContentRecord(string contentName, int recordId);
	}

}

