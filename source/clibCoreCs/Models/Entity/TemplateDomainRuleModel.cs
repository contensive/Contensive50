﻿//========================================================================
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
	public class TemplateDomainRuleModel : baseModel
	{
		//
		//====================================================================================================
		//-- const
		public const string contentName = "Template Domain Rules";
		public const string contentTableName = "ccDomainTemplateRules";
		private const string contentDataSource = "default";
		//
		//====================================================================================================
		// -- instance properties
		public int domainId {get; set;}
		public int templateId {get; set;}
		//
		//====================================================================================================
		public static TemplateDomainRuleModel add(coreClass cpCore)
		{
			return add<TemplateDomainRuleModel>(cpCore);
		}
		//
		//====================================================================================================
		public static TemplateDomainRuleModel add(coreClass cpCore, ref List<string> callersCacheNameList)
		{
			return add<TemplateDomainRuleModel>(cpCore, ref callersCacheNameList);
		}
		//
		//====================================================================================================
		public static TemplateDomainRuleModel create(coreClass cpCore, int recordId)
		{
			return create<TemplateDomainRuleModel>(cpCore, recordId);
		}
		//
		//====================================================================================================
		public static TemplateDomainRuleModel create(coreClass cpCore, int recordId, ref List<string> callersCacheNameList)
		{
			return create<TemplateDomainRuleModel>(cpCore, recordId, ref callersCacheNameList);
		}
		//
		//====================================================================================================
		public static TemplateDomainRuleModel create(coreClass cpCore, string recordGuid)
		{
			return create<TemplateDomainRuleModel>(cpCore, recordGuid);
		}
		//
		//====================================================================================================
		public static TemplateDomainRuleModel create(coreClass cpCore, string recordGuid, ref List<string> callersCacheNameList)
		{
			return create<TemplateDomainRuleModel>(cpCore, recordGuid, ref callersCacheNameList);
		}
		//
		//====================================================================================================
		public static TemplateDomainRuleModel createByName(coreClass cpCore, string recordName)
		{
			return createByName<TemplateDomainRuleModel>(cpCore, recordName);
		}
		//
		//====================================================================================================
		public static TemplateDomainRuleModel createByName(coreClass cpCore, string recordName, ref List<string> callersCacheNameList)
		{
			return createByName<TemplateDomainRuleModel>(cpCore, recordName, ref callersCacheNameList);
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
			delete<TemplateDomainRuleModel>(cpCore, recordId);
		}
		//
		//====================================================================================================
		public static void delete(coreClass cpCore, string ccGuid)
		{
			delete<TemplateDomainRuleModel>(cpCore, ccGuid);
		}
		//
		//====================================================================================================
		public static List<TemplateDomainRuleModel> createList(coreClass cpCore, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList)
		{
			return createList<TemplateDomainRuleModel>(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList);
		}
		//
		//====================================================================================================
		public static List<TemplateDomainRuleModel> createList(coreClass cpCore, string sqlCriteria, string sqlOrderBy)
		{
			return createList<TemplateDomainRuleModel>(cpCore, sqlCriteria, sqlOrderBy);
		}
		//
		//====================================================================================================
		public static List<TemplateDomainRuleModel> createList(coreClass cpCore, string sqlCriteria)
		{
			return createList<TemplateDomainRuleModel>(cpCore, sqlCriteria);
		}
		//
		//====================================================================================================
		public void invalidatePrimaryCache(coreClass cpCore, int recordId)
		{
			invalidateCacheSingleRecord<TemplateDomainRuleModel>(cpCore, recordId);
		}
		//
		//====================================================================================================
		public static string getRecordName(coreClass cpcore, int recordId)
		{
			return baseModel.getRecordName<TemplateDomainRuleModel>(cpcore, recordId);
		}
		//
		//====================================================================================================
		public static string getRecordName(coreClass cpcore, string ccGuid)
		{
			return baseModel.getRecordName<TemplateDomainRuleModel>(cpcore, ccGuid);
		}
		//
		//====================================================================================================
		public static int getRecordId(coreClass cpcore, string ccGuid)
		{
			return baseModel.getRecordId<TemplateDomainRuleModel>(cpcore, ccGuid);
		}
		//
		//====================================================================================================
		public static TemplateDomainRuleModel createDefault(coreClass cpcore)
		{
			return createDefault<TemplateDomainRuleModel>(cpcore);
		}
	}
}
