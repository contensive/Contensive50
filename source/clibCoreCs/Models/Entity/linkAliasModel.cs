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
	public class linkAliasModel : baseModel
	{
		//
		//====================================================================================================
		//-- const
		public const string contentName = "link aliases";
		public const string contentTableName = "cclinkaliases";
		private const string contentDataSource = "default";
		//
		//====================================================================================================
		// -- instance properties
		//Public Property Link As String
		public int PageID {get; set;}
		public string QueryStringSuffix {get; set;}
		//
		//====================================================================================================
		public static linkAliasModel add(coreClass cpCore)
		{
			return add<linkAliasModel>(cpCore);
		}
		//
		//====================================================================================================
		public static linkAliasModel add(coreClass cpCore, ref List<string> callersCacheNameList)
		{
			return add<linkAliasModel>(cpCore, ref callersCacheNameList);
		}
		//
		//====================================================================================================
		public static linkAliasModel create(coreClass cpCore, int recordId)
		{
			return create<linkAliasModel>(cpCore, recordId);
		}
		//
		//====================================================================================================
		public static linkAliasModel create(coreClass cpCore, int recordId, ref List<string> callersCacheNameList)
		{
			return create<linkAliasModel>(cpCore, recordId, ref callersCacheNameList);
		}
		//
		//====================================================================================================
		public static linkAliasModel create(coreClass cpCore, string recordGuid)
		{
			return create<linkAliasModel>(cpCore, recordGuid);
		}
		//
		//====================================================================================================
		public static linkAliasModel create(coreClass cpCore, string recordGuid, ref List<string> callersCacheNameList)
		{
			return create<linkAliasModel>(cpCore, recordGuid, ref callersCacheNameList);
		}
		//
		//====================================================================================================
		public static linkAliasModel createByName(coreClass cpCore, string recordName)
		{
			return createByName<linkAliasModel>(cpCore, recordName);
		}
		//
		//====================================================================================================
		public static linkAliasModel createByName(coreClass cpCore, string recordName, ref List<string> callersCacheNameList)
		{
			return createByName<linkAliasModel>(cpCore, recordName, ref callersCacheNameList);
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
			delete<linkAliasModel>(cpCore, recordId);
		}
		//
		//====================================================================================================
		public static void delete(coreClass cpCore, string ccGuid)
		{
			delete<linkAliasModel>(cpCore, ccGuid);
		}
		//
		//====================================================================================================
		public static List<linkAliasModel> createList(coreClass cpCore, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList)
		{
			return createList<linkAliasModel>(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList);
		}
		//
		//====================================================================================================
		public static List<linkAliasModel> createList(coreClass cpCore, string sqlCriteria, string sqlOrderBy)
		{
			return createList<linkAliasModel>(cpCore, sqlCriteria, sqlOrderBy);
		}
		//
		//====================================================================================================
		public static List<linkAliasModel> createList(coreClass cpCore, string sqlCriteria)
		{
			return createList<linkAliasModel>(cpCore, sqlCriteria);
		}
		//
		//====================================================================================================
		public static void invalidateCache(coreClass cpCore, int recordId)
		{
			invalidateCacheSingleRecord<linkAliasModel>(cpCore, recordId);
			Models.Complex.routeDictionaryModel.invalidateCache(cpCore);
		}
		//
		//====================================================================================================
		public static string getRecordName(coreClass cpcore, int recordId)
		{
			return baseModel.getRecordName<linkAliasModel>(cpcore, recordId);
		}
		//
		//====================================================================================================
		public static string getRecordName(coreClass cpcore, string ccGuid)
		{
			return baseModel.getRecordName<linkAliasModel>(cpcore, ccGuid);
		}
		//
		//====================================================================================================
		public static int getRecordId(coreClass cpcore, string ccGuid)
		{
			return baseModel.getRecordId<linkAliasModel>(cpcore, ccGuid);
		}
		//
		//====================================================================================================
		public static linkAliasModel createDefault(coreClass cpcore)
		{
			return createDefault<linkAliasModel>(cpcore);
		}
		//
		//====================================================================================================
		public static List<linkAliasModel> createList(coreClass cpCore, int pageId, string queryStringSuffix)
		{
			if (string.IsNullOrEmpty(queryStringSuffix))
			{
				return createList<linkAliasModel>(cpCore, "(pageId=" + pageId + ")", "id desc");
			}
			else
			{
				return createList<linkAliasModel>(cpCore, "(pageId=" + pageId + ")and(QueryStringSuffix=" + cpCore.db.encodeSQLText(queryStringSuffix) + ")", "id desc");
			}
		}
	}
}
