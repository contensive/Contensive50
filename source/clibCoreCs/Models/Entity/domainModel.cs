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
	public class domainModel : baseModel
	{
		//
		//====================================================================================================
		//-- const
		public const string contentName = "domains";
		public const string contentTableName = "ccdomains";
		private const string contentDataSource = "default";
		//
		//====================================================================================================
		// -- instance properties
		public int DefaultTemplateId {get; set;}
		public int forwardDomainId {get; set;}
		public string ForwardURL {get; set;}
		public bool NoFollow {get; set;}
		public int PageNotFoundPageID {get; set;}
		public int RootPageID {get; set;}
		public int TypeID {get; set;}
		public bool Visited {get; set;}
		//
		//====================================================================================================
		public static domainModel add(coreClass cpCore)
		{
			return add<domainModel>(cpCore);
		}
		//
		//====================================================================================================
		public static domainModel add(coreClass cpCore, ref List<string> callersCacheNameList)
		{
			return add<domainModel>(cpCore, ref callersCacheNameList);
		}
		//
		//====================================================================================================
		public static domainModel create(coreClass cpCore, int recordId)
		{
			return create<domainModel>(cpCore, recordId);
		}
		//
		//====================================================================================================
		public static domainModel create(coreClass cpCore, int recordId, ref List<string> callersCacheNameList)
		{
			return create<domainModel>(cpCore, recordId, ref callersCacheNameList);
		}
		//
		//====================================================================================================
		public static domainModel create(coreClass cpCore, string recordGuid)
		{
			return create<domainModel>(cpCore, recordGuid);
		}
		//
		//====================================================================================================
		public static domainModel create(coreClass cpCore, string recordGuid, ref List<string> callersCacheNameList)
		{
			return create<domainModel>(cpCore, recordGuid, ref callersCacheNameList);
		}
		//
		//====================================================================================================
		public static domainModel createByName(coreClass cpCore, string recordName)
		{
			return createByName<domainModel>(cpCore, recordName);
		}
		//
		//====================================================================================================
		public static domainModel createByName(coreClass cpCore, string recordName, ref List<string> callersCacheNameList)
		{
			return createByName<domainModel>(cpCore, recordName, ref callersCacheNameList);
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
			delete<domainModel>(cpCore, recordId);
		}
		//
		//====================================================================================================
		public static void delete(coreClass cpCore, string ccGuid)
		{
			delete<domainModel>(cpCore, ccGuid);
		}
		//
		//====================================================================================================
		public static List<domainModel> createList(coreClass cpCore, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList)
		{
			return createList<domainModel>(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList);
		}
		//
		//====================================================================================================
		public static List<domainModel> createList(coreClass cpCore, string sqlCriteria, string sqlOrderBy)
		{
			return createList<domainModel>(cpCore, sqlCriteria, sqlOrderBy);
		}
		//
		//====================================================================================================
		public static List<domainModel> createList(coreClass cpCore, string sqlCriteria)
		{
			return createList<domainModel>(cpCore, sqlCriteria);
		}
		//
		//====================================================================================================
		public void invalidatePrimaryCache(coreClass cpCore, int recordId)
		{
			invalidateCacheSingleRecord<domainModel>(cpCore, recordId);
		}
		//
		//====================================================================================================
		public static string getRecordName(coreClass cpcore, int recordId)
		{
			return baseModel.getRecordName<domainModel>(cpcore, recordId);
		}
		//
		//====================================================================================================
		public static string getRecordName(coreClass cpcore, string ccGuid)
		{
			return baseModel.getRecordName<domainModel>(cpcore, ccGuid);
		}
		//
		//====================================================================================================
		public static int getRecordId(coreClass cpcore, string ccGuid)
		{
			return baseModel.getRecordId<domainModel>(cpcore, ccGuid);
		}
		//
		//====================================================================================================
		public static domainModel createDefault(coreClass cpcore)
		{
			return createDefault<domainModel>(cpcore);
		}
		//
		public enum domainTypeEnum
		{
			Normal = 1,
			ForwardToUrl = 2,
			ForwardToReplacementDomain = 3
		}
	}
}
