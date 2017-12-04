//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

using System;
using System.Collections.Generic;
using System.Text;
using Contensive.BaseClasses;
using Contensive.Core.Controllers;
using Newtonsoft.Json;

namespace Contensive.Core.Models.Entity
{
	public class libraryFileLogModel : baseModel
	{
		//
		//====================================================================================================
		//-- const
		public const string contentName = "library File log";
		public const string contentTableName = "cclibraryFileLog";
		private const string contentDataSource = "default";
		//
		//====================================================================================================
		// -- instance properties
		public int FileID {get; set;}
		public int MemberID {get; set;}
		public int VisitID {get; set;}
		//
		//====================================================================================================
		public static libraryFileLogModel add(coreClass cpCore)
		{
			return add<libraryFileLogModel>(cpCore);
		}
		//
		//====================================================================================================
		public static libraryFileLogModel add(coreClass cpCore, ref List<string> callersCacheNameList)
		{
			return add<libraryFileLogModel>(cpCore, ref callersCacheNameList);
		}
		//
		//====================================================================================================
		public static libraryFileLogModel create(coreClass cpCore, int recordId)
		{
			return create<libraryFileLogModel>(cpCore, recordId);
		}
		//
		//====================================================================================================
		public static libraryFileLogModel create(coreClass cpCore, int recordId, ref List<string> callersCacheNameList)
		{
			return create<libraryFileLogModel>(cpCore, recordId, ref callersCacheNameList);
		}
		//
		//====================================================================================================
		public static libraryFileLogModel create(coreClass cpCore, string recordGuid)
		{
			return create<libraryFileLogModel>(cpCore, recordGuid);
		}
		//
		//====================================================================================================
		public static libraryFileLogModel create(coreClass cpCore, string recordGuid, ref List<string> callersCacheNameList)
		{
			return create<libraryFileLogModel>(cpCore, recordGuid, ref callersCacheNameList);
		}
		//
		//====================================================================================================
		public static libraryFileLogModel createByName(coreClass cpCore, string recordName)
		{
			return createByName<libraryFileLogModel>(cpCore, recordName);
		}
		//
		//====================================================================================================
		public static libraryFileLogModel createByName(coreClass cpCore, string recordName, ref List<string> callersCacheNameList)
		{
			return createByName<libraryFileLogModel>(cpCore, recordName, ref callersCacheNameList);
		}
		//
		//====================================================================================================
		public void save(coreClass cpCore)
		{
			base.save(cpCore);
		}
		//
		//====================================================================================================
		public static void delete(coreClass cpCore, int recordId)
		{
			delete<libraryFileLogModel>(cpCore, recordId);
		}
		//
		//====================================================================================================
		public static void delete(coreClass cpCore, string ccGuid)
		{
			delete<libraryFileLogModel>(cpCore, ccGuid);
		}
		//
		//====================================================================================================
		public static List<libraryFileLogModel> createList(coreClass cpCore, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList)
		{
			return createList<libraryFileLogModel>(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList);
		}
		//
		//====================================================================================================
		public static List<libraryFileLogModel> createList(coreClass cpCore, string sqlCriteria, string sqlOrderBy)
		{
			return createList<libraryFileLogModel>(cpCore, sqlCriteria, sqlOrderBy);
		}
		//
		//====================================================================================================
		public static List<libraryFileLogModel> createList(coreClass cpCore, string sqlCriteria)
		{
			return createList<libraryFileLogModel>(cpCore, sqlCriteria);
		}
		//
		//====================================================================================================
		public void invalidatePrimaryCache(coreClass cpCore, int recordId)
		{
			invalidateCacheSingleRecord<libraryFileLogModel>(cpCore, recordId);
		}
		//
		//====================================================================================================
		public static string getRecordName(coreClass cpcore, int recordId)
		{
			return baseModel.getRecordName<libraryFileLogModel>(cpcore, recordId);
		}
		//
		//====================================================================================================
		public static string getRecordName(coreClass cpcore, string ccGuid)
		{
			return baseModel.getRecordName<libraryFileLogModel>(cpcore, ccGuid);
		}
		//
		//====================================================================================================
		public static int getRecordId(coreClass cpcore, string ccGuid)
		{
			return baseModel.getRecordId<libraryFileLogModel>(cpcore, ccGuid);
		}
		//
		//====================================================================================================
		public static libraryFileLogModel createDefault(coreClass cpcore)
		{
			return createDefault<libraryFileLogModel>(cpcore);
		}
	}
}
