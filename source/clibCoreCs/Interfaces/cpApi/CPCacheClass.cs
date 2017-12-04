﻿using System;

//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

using Contensive.BaseClasses;
using System.Runtime.InteropServices;
using Contensive.Core.Controllers;


namespace Contensive.Core
{
	//
	// comVisible to be activeScript compatible
	//
	[ComVisible(true), Microsoft.VisualBasic.ComClass(CPCacheClass.ClassId, CPCacheClass.InterfaceId, CPCacheClass.EventsId)]
	public class CPCacheClass : BaseClasses.CPCacheBaseClass, IDisposable
	{
		//
#region COM GUIDs
		public const string ClassId = "D522F0F5-53DF-4C6C-88E5-75CDAB91D286";
		public const string InterfaceId = "9FED1031-1637-4002-9B08-4A40FDF13236";
		public const string EventsId = "11B23802-CBD3-48E6-9C3E-1DC26ED8775A";
#endregion
		//
		private Contensive.Core.coreClass cpCore {get; set;}
		private CPClass cp {get; set;}
		//
		//====================================================================================================
		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="cpParent"></param>
		/// <remarks></remarks>
		public CPCacheClass(ref CPClass cpParent) : base()
		{
			cp = cpParent;
			cpCore = cp.core;
		}
		//
		//====================================================================================================
		/// <summary>
		/// Clear all cache values
		/// </summary>
		/// <remarks></remarks>
		public override void ClearAll()
		{
			cpCore.cache.invalidateAll();
		}
		//
		//====================================================================================================
		/// <summary>
		/// clear cacheDataSourceTag. A cache DataSource Tag is a tag that represents a source of data used to build a cache object, like a database table.
		/// </summary>
		/// <param name="ContentNameList"></param>
		/// <remarks></remarks>
		public override void Clear(string ContentNameList)
		{
			if (string.IsNullOrEmpty(ContentNameList))
			{
				foreach (var contentName in new List<string>(ContentNameList.Split(',')))
				{
					cpCore.cache.invalidateAllObjectsInContent(contentName);
				}
			}
		}
		//
		//====================================================================================================
		/// <summary>
		/// read a cache value
		/// </summary>
		/// <param name="Name"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		public override string Read(string Name)
		{
			return getText(Name);
		}
		//====================================================================================================
		/// <summary>
		/// save a cache value. Legacy. Use object value.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="Value"></param>
		/// <param name="invalidationTagCommaList"></param>
		/// <param name="ClearOnDate"></param>
		/// <remarks></remarks>
		public override void Save(string key, string Value)
		{
			Save(key, Value, "", DateTime.MinValue);
		}
		//
		public override void Save(string key, string Value, string invalidationTagCommaList)
		{
			Save(key, Value, invalidationTagCommaList, DateTime.MinValue);
		}
		//
		public override void Save(string key, string Value, string invalidationTagCommaList, DateTime invalidationDate)
		{
			try
			{
				List<string> invalidationTagList = new List<string>();
				if (string.IsNullOrEmpty(invalidationTagCommaList.Trim()))
				{
					//
				}
				else
				{
					invalidationTagList.AddRange(invalidationTagCommaList.Split(','));
				}
				if (invalidationDate == Convert.ToDateTime("12:00:00 AM"))
				{
					cpCore.cache.setContent(key, Value, invalidationTagList);
				}
				else
				{
					cpCore.cache.setContent(key, Value, invalidationDate, invalidationTagList);
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
		}
		//
		//====================================================================================================
		//
		public override object getObject(string key)
		{
			return cpCore.cache.getObject<object>(key);
		}
		//
		//====================================================================================================
		//
		public override int getInteger(string key)
		{
			return genericController.EncodeInteger(getObject(key));
		}
		//
		//====================================================================================================
		//
		public override bool getBoolean(string key)
		{
			return genericController.EncodeBoolean(getObject(key));
		}
		//
		//====================================================================================================
		//
		public override DateTime getDate(string key)
		{
			return genericController.EncodeDate(getObject(key));
		}
		//
		//====================================================================================================
		//
		public override double getNumber(string key)
		{
			return EncodeNumber(getObject(key));
		}
		//
		//====================================================================================================
		//
		public override string getText(string key)
		{
			return genericController.encodeText(getObject(key));
		}
		//
		//====================================================================================================
		//
		public override void InvalidateAll()
		{
			cpCore.cache.invalidateAll();
		}
		//
		//====================================================================================================
		//
		public override void InvalidateTag(string tag)
		{
			cpCore.cache.invalidateContent(tag);
		}
		//
		//====================================================================================================
		//
		public override void InvalidateTagList(List<string> tagList)
		{
			cpCore.cache.invalidateContent(tagList);
		}
		//
		//====================================================================================================
		/// <summary>
		/// Save a value to a cache key. It will invalidate after the default invalidation days
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		public override void SetKey(string key, object value)
		{
			cpCore.cache.setContent(key, value);
		}
		//
		//====================================================================================================
		/// <summary>
		/// Save a value to a cache key and specify when it will be invalidated.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <param name="invalidationDate"></param>
		public override void SetKey(string key, object value, DateTime invalidationDate)
		{
			cpCore.cache.setContent(key, value, invalidationDate, "");
		}
		//
		//====================================================================================================
		/// <summary>
		/// Save a value to a cachekey and associate it to one of more tags. This key will be invalidated if any of the tags are invalidated.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <param name="tagList"></param>
		public override void SetKey(string key, object value, List<string> tagList)
		{
			cpCore.cache.setContent(key, value, tagList);
		}
		//
		//====================================================================================================
		/// <summary>
		/// Save a value to a cachekey with an invalidate date, and associate it to one of more tags. This key will be invalidated if any of the tags are invalidated.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <param name="tagList"></param>
		/// <param name="invalidationDate"></param>
		public override void SetKey(string key, object value, DateTime invalidationDate, List<string> tagList)
		{
			cpCore.cache.setContent(key, value, invalidationDate, tagList);
		}
		//
		//====================================================================================================
		//
		public override void setKey(string key, object value, string tag)
		{
			cpCore.cache.setContent(key, value, tag);
		}
		//
		//====================================================================================================
		//
		public override void setKey(string key, object Value, DateTime invalidationDate, string tag)
		{
			cpCore.cache.setContent(key, Value, invalidationDate, tag);
		}
		//
		public override void InvalidateContentRecord(string contentName, int recordId)
		{
			cpCore.cache.invalidateContent_Entity(cp.core, contentName, recordId);
		}
#region  IDisposable Support 
		//
		// dispose
		//
		protected bool disposed = false;
		protected virtual void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
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
		// Do not change or add Overridable to these methods.
		// Put cleanup code in Dispose(ByVal disposing As Boolean).
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		~CPCacheClass()
		{
			Dispose(false);
//INSTANT C# NOTE: The base class Finalize method is automatically called from the destructor:
			//base.Finalize();
		}
#endregion
	}
}