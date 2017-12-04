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
	public class addonTemplateRuleModel : baseModel
	{
		//
		//====================================================================================================
		//-- const
		public const string contentName = "add-on template rules";
		public const string contentTableName = "ccAddonTemplateRules";
		private const string contentDataSource = "default";
		//
		//====================================================================================================
		// -- instance properties
		public int addonId {get; set;}
		public int templateId {get; set;}
		//
		//====================================================================================================
		public static addonTemplateRuleModel add(coreClass cpCore)
		{
			return add<addonTemplateRuleModel>(cpCore);
		}
		//
		//====================================================================================================
		public static addonTemplateRuleModel add(coreClass cpCore, ref List<string> callersCacheNameList)
		{
			return add<addonTemplateRuleModel>(cpCore, ref callersCacheNameList);
		}
		//
		//====================================================================================================
		public static addonTemplateRuleModel create(coreClass cpCore, int recordId)
		{
			return create<addonTemplateRuleModel>(cpCore, recordId);
		}
		//
		//====================================================================================================
		public static addonTemplateRuleModel create(coreClass cpCore, int recordId, ref List<string> callersCacheNameList)
		{
			return create<addonTemplateRuleModel>(cpCore, recordId, ref callersCacheNameList);
		}
		//
		//====================================================================================================
		public static addonTemplateRuleModel create(coreClass cpCore, string recordGuid)
		{
			return create<addonTemplateRuleModel>(cpCore, recordGuid);
		}
		//
		//====================================================================================================
		public static addonTemplateRuleModel create(coreClass cpCore, string recordGuid, ref List<string> callersCacheNameList)
		{
			return create<addonTemplateRuleModel>(cpCore, recordGuid, ref callersCacheNameList);
		}
		//
		//====================================================================================================
		public static addonTemplateRuleModel createByName(coreClass cpCore, string recordName)
		{
			return createByName<addonTemplateRuleModel>(cpCore, recordName);
		}
		//
		//====================================================================================================
		public static addonTemplateRuleModel createByName(coreClass cpCore, string recordName, ref List<string> callersCacheNameList)
		{
			return createByName<addonTemplateRuleModel>(cpCore, recordName, ref callersCacheNameList);
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
			delete<addonTemplateRuleModel>(cpCore, recordId);
		}
		//
		//====================================================================================================
		public static void delete(coreClass cpCore, string ccGuid)
		{
			delete<addonTemplateRuleModel>(cpCore, ccGuid);
		}
		//
		//====================================================================================================
		public static List<addonTemplateRuleModel> createList(coreClass cpCore, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList)
		{
			return createList<addonTemplateRuleModel>(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList);
		}
		//
		//====================================================================================================
		public static List<addonTemplateRuleModel> createList(coreClass cpCore, string sqlCriteria, string sqlOrderBy)
		{
			return createList<addonTemplateRuleModel>(cpCore, sqlCriteria, sqlOrderBy);
		}
		//
		//====================================================================================================
		public static List<addonTemplateRuleModel> createList(coreClass cpCore, string sqlCriteria)
		{
			return createList<addonTemplateRuleModel>(cpCore, sqlCriteria);
		}
		//
		//====================================================================================================
		public void invalidatePrimaryCache(coreClass cpCore, int recordId)
		{
			invalidateCacheSingleRecord<addonTemplateRuleModel>(cpCore, recordId);
		}
		//
		//====================================================================================================
		public static string getRecordName(coreClass cpcore, int recordId)
		{
			return baseModel.getRecordName<addonTemplateRuleModel>(cpcore, recordId);
		}
		//
		//====================================================================================================
		public static string getRecordName(coreClass cpcore, string ccGuid)
		{
			return baseModel.getRecordName<addonTemplateRuleModel>(cpcore, ccGuid);
		}
		//
		//====================================================================================================
		public static int getRecordId(coreClass cpcore, string ccGuid)
		{
			return baseModel.getRecordId<addonTemplateRuleModel>(cpcore, ccGuid);
		}
		//
		//====================================================================================================
		public static addonTemplateRuleModel createDefault(coreClass cpcore)
		{
			return createDefault<addonTemplateRuleModel>(cpcore);
		}
	}
}
