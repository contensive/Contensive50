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
	public class emailLogModel : baseModel
	{
		//
		//====================================================================================================
		//-- const
		public const string contentName = "email log";
		public const string contentTableName = "ccEmailLog";
		private const string contentDataSource = "default";
		//
		//====================================================================================================
		// -- instance properties
		public DateTime DateBlockExpires {get; set;}
		public int EmailDropID {get; set;}
		public int EmailID {get; set;}
		public string FromAddress {get; set;}
		public int LogType {get; set;}
		public int MemberID {get; set;}
		public string SendStatus {get; set;}
		public string Subject {get; set;}
		public string ToAddress {get; set;}
		public int VisitID {get; set;}
		//
		//====================================================================================================
		public static emailLogModel add(coreClass cpCore)
		{
			return add<emailLogModel>(cpCore);
		}
		//
		//====================================================================================================
		public static emailLogModel add(coreClass cpCore, ref List<string> callersCacheNameList)
		{
			return add<emailLogModel>(cpCore, ref callersCacheNameList);
		}
		//
		//====================================================================================================
		public static emailLogModel create(coreClass cpCore, int recordId)
		{
			return create<emailLogModel>(cpCore, recordId);
		}
		//
		//====================================================================================================
		public static emailLogModel create(coreClass cpCore, int recordId, ref List<string> callersCacheNameList)
		{
			return create<emailLogModel>(cpCore, recordId, ref callersCacheNameList);
		}
		//
		//====================================================================================================
		public static emailLogModel create(coreClass cpCore, string recordGuid)
		{
			return create<emailLogModel>(cpCore, recordGuid);
		}
		//
		//====================================================================================================
		public static emailLogModel create(coreClass cpCore, string recordGuid, ref List<string> callersCacheNameList)
		{
			return create<emailLogModel>(cpCore, recordGuid, ref callersCacheNameList);
		}
		//
		//====================================================================================================
		public static emailLogModel createByName(coreClass cpCore, string recordName)
		{
			return createByName<emailLogModel>(cpCore, recordName);
		}
		//
		//====================================================================================================
		public static emailLogModel createByName(coreClass cpCore, string recordName, ref List<string> callersCacheNameList)
		{
			return createByName<emailLogModel>(cpCore, recordName, ref callersCacheNameList);
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
			delete<emailLogModel>(cpCore, recordId);
		}
		//
		//====================================================================================================
		public static void delete(coreClass cpCore, string ccGuid)
		{
			delete<emailLogModel>(cpCore, ccGuid);
		}
		//
		//====================================================================================================
		public static List<emailLogModel> createList(coreClass cpCore, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList)
		{
			return createList<emailLogModel>(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList);
		}
		//
		//====================================================================================================
		public static List<emailLogModel> createList(coreClass cpCore, string sqlCriteria, string sqlOrderBy)
		{
			return createList<emailLogModel>(cpCore, sqlCriteria, sqlOrderBy);
		}
		//
		//====================================================================================================
		public static List<emailLogModel> createList(coreClass cpCore, string sqlCriteria)
		{
			return createList<emailLogModel>(cpCore, sqlCriteria);
		}
		//
		//====================================================================================================
		public void invalidatePrimaryCache(coreClass cpCore, int recordId)
		{
			invalidateCacheSingleRecord<emailLogModel>(cpCore, recordId);
		}
		//
		//====================================================================================================
		public static string getRecordName(coreClass cpcore, int recordId)
		{
			return baseModel.getRecordName<emailLogModel>(cpcore, recordId);
		}
		//
		//====================================================================================================
		public static string getRecordName(coreClass cpcore, string ccGuid)
		{
			return baseModel.getRecordName<emailLogModel>(cpcore, ccGuid);
		}
		//
		//====================================================================================================
		public static int getRecordId(coreClass cpcore, string ccGuid)
		{
			return baseModel.getRecordId<emailLogModel>(cpcore, ccGuid);
		}
		//
		//====================================================================================================
		public static emailLogModel createDefault(coreClass cpcore)
		{
			return createDefault<emailLogModel>(cpcore);
		}
	}
}
