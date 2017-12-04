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
	public class libraryFilesModel : baseModel
	{
		//
		//====================================================================================================
		//-- const
		public const string contentName = "library Files";
		public const string contentTableName = "cclibraryFiles";
		private const string contentDataSource = "default";
		//
		//====================================================================================================
		// -- instance properties
		public string AltSizeList {get; set;}
		public string AltText {get; set;}
		public int Clicks {get; set;}
		public string Description {get; set;}
		public string Filename {get; set;}
		public int FileSize {get; set;}
		public int FileTypeID {get; set;}
		public int FolderID {get; set;}
		public int pxHeight {get; set;}
		public int pxWidth {get; set;}
		//
		//====================================================================================================
		public static libraryFilesModel add(coreClass cpCore)
		{
			return add<libraryFilesModel>(cpCore);
		}
		//
		//====================================================================================================
		public static libraryFilesModel add(coreClass cpCore, ref List<string> callersCacheNameList)
		{
			return add<libraryFilesModel>(cpCore, ref callersCacheNameList);
		}
		//
		//====================================================================================================
		public static libraryFilesModel create(coreClass cpCore, int recordId)
		{
			return create<libraryFilesModel>(cpCore, recordId);
		}
		//
		//====================================================================================================
		public static libraryFilesModel create(coreClass cpCore, int recordId, ref List<string> callersCacheNameList)
		{
			return create<libraryFilesModel>(cpCore, recordId, ref callersCacheNameList);
		}
		//
		//====================================================================================================
		public static libraryFilesModel create(coreClass cpCore, string recordGuid)
		{
			return create<libraryFilesModel>(cpCore, recordGuid);
		}
		//
		//====================================================================================================
		public static libraryFilesModel create(coreClass cpCore, string recordGuid, ref List<string> callersCacheNameList)
		{
			return create<libraryFilesModel>(cpCore, recordGuid, ref callersCacheNameList);
		}
		//
		//====================================================================================================
		public static libraryFilesModel createByName(coreClass cpCore, string recordName)
		{
			return createByName<libraryFilesModel>(cpCore, recordName);
		}
		//
		//====================================================================================================
		public static libraryFilesModel createByName(coreClass cpCore, string recordName, ref List<string> callersCacheNameList)
		{
			return createByName<libraryFilesModel>(cpCore, recordName, ref callersCacheNameList);
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
			delete<libraryFilesModel>(cpCore, recordId);
		}
		//
		//====================================================================================================
		public static void delete(coreClass cpCore, string ccGuid)
		{
			delete<libraryFilesModel>(cpCore, ccGuid);
		}
		//
		//====================================================================================================
		public static List<libraryFilesModel> createList(coreClass cpCore, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList)
		{
			return createList<libraryFilesModel>(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList);
		}
		//
		//====================================================================================================
		public static List<libraryFilesModel> createList(coreClass cpCore, string sqlCriteria, string sqlOrderBy)
		{
			return createList<libraryFilesModel>(cpCore, sqlCriteria, sqlOrderBy);
		}
		//
		//====================================================================================================
		public static List<libraryFilesModel> createList(coreClass cpCore, string sqlCriteria)
		{
			return createList<libraryFilesModel>(cpCore, sqlCriteria);
		}
		//
		//====================================================================================================
		public void invalidatePrimaryCache(coreClass cpCore, int recordId)
		{
			invalidateCacheSingleRecord<libraryFilesModel>(cpCore, recordId);
		}
		//
		//====================================================================================================
		public static string getRecordName(coreClass cpcore, int recordId)
		{
			return baseModel.getRecordName<libraryFilesModel>(cpcore, recordId);
		}
		//
		//====================================================================================================
		public static string getRecordName(coreClass cpcore, string ccGuid)
		{
			return baseModel.getRecordName<libraryFilesModel>(cpcore, ccGuid);
		}
		//
		//====================================================================================================
		public static int getRecordId(coreClass cpcore, string ccGuid)
		{
			return baseModel.getRecordId<libraryFilesModel>(cpcore, ccGuid);
		}
		//
		//====================================================================================================
		public static libraryFilesModel createDefault(coreClass cpcore)
		{
			return createDefault<libraryFilesModel>(cpcore);
		}
	}
}
