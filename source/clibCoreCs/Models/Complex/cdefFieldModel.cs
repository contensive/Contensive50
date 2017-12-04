using System;

//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

using Models;
using Models.Context;
using Models.Entity;
using Contensive.Core.Controllers;
using System.Data;

namespace Contensive.Core.Models.Complex
{
	//
	// ---------------------------------------------------------------------------------------------------
	// ----- CDefFieldClass
	//       class not structure because it has to marshall to vb6
	// ---------------------------------------------------------------------------------------------------
	//
	[Serializable]
	public class CDefFieldModel : ICloneable, IComparable
	{
		public string nameLc {get; set;} // The name of the field
		public int id {get; set;} // the ID in the ccContentFields Table that this came from
		public bool active {get; set;} // if the field is available in the admin area
		public int fieldTypeId {get; set;} // The type of data the field holds
		public string caption {get; set;} // The caption for displaying the field
		public bool ReadOnly {get; set;} // was ReadOnly -- If true, this field can not be written back to the database
		public bool NotEditable {get; set;} // if true, you can only edit new records
		public bool Required {get; set;} // if true, this field must be entered
		public string defaultValue {get; set;} // default value on a new record
		public string HelpMessage {get; set;} // explaination of this field
		public bool UniqueName {get; set;}
		public bool TextBuffered {get; set;} // if true, the input is run through RemoveControlCharacters()
		public bool Password {get; set;} // for text boxes, sets the password attribute
		public string RedirectID {get; set;} // If TYPEREDIRECT, this is the field that must match ID of this record
		public string RedirectPath {get; set;} // New Field, If TYPEREDIRECT, this is the path to the next page (if blank, current page is used)
		public int indexColumn {get; set;} // the column desired in the admin index form
		public string indexWidth {get; set;} // either number or percentage, blank if not included
		public int indexSortOrder {get; set;} // alpha sort on index page
		public int indexSortDirection {get; set;} // 1 sorts forward, -1 backward
		public bool adminOnly {get; set;} // This field is only available to administrators
		public bool developerOnly {get; set;} // This field is only available to administrators
		public bool blockAccess {get; set;} // ***** Field Reused to keep binary compatiblity - "IsBaseField" - if true this is a CDefBase field
		public bool htmlContent {get; set;} // if true, the HTML editor (active edit) can be used
		public bool authorable {get; set;} // true if it can be seen in the admin form
		public bool inherited {get; set;} // if true, this field takes its values from a parent, see ContentID
		public int contentId {get; set;} // This is the ID of the Content Def that defines these properties
		public int editSortPriority {get; set;} // The Admin Edit Sort Order
		public string ManyToManyRulePrimaryField {get; set;} // Rule Field Name for Primary Table
		public string ManyToManyRuleSecondaryField {get; set;} // Rule Field Name for Secondary Table
		public bool RSSTitleField {get; set;} // When creating RSS fields from this content, this is the title
		public bool RSSDescriptionField {get; set;} // When creating RSS fields from this content, this is the description
		public string editTabName {get; set;} // Editing group - used for the tabs
		public bool Scramble {get; set;} // save the field scrambled in the Db
		public string lookupList {get; set;} // If TYPELOOKUP, and LookupContentID is null, this is a comma separated list of choices
		public bool dataChanged {get; set;}
		public bool isBaseField {get; set;}
		public bool isModifiedSinceInstalled {get; set;}
		public string installedByCollectionGuid {get; set;}
		public string HelpDefault {get; set;}
		public string HelpCustom {get; set;}
		public bool HelpChanged {get; set;}
		//
		// fields stored differently in xml collection files
		//   name is loaded from xml collection files 
		//   id is created during the cacheLoad process when loading from Db (and used in metaData)
		//
		public int lookupContentID {get; set;} // If TYPELOOKUP, (for Db controled sites) this is the content ID of the source table
		public int RedirectContentID {get; set;} // If TYPEREDIRECT, this is new contentID
		public int manyToManyContentID {get; set;} // Content containing Secondary Records
		public int manyToManyRuleContentID {get; set;} // Content with rules between Primary and Secondary
		public int MemberSelectGroupID {get; set;}
		//
		//====================================================================================================
		//
//INSTANT C# NOTE: C# does not support parameterized properties - the following property has been divided into two methods:
//ORIGINAL LINE: Public Property RedirectContentName(cpCore As coreClass) As String
		public string get_RedirectContentName(coreClass cpCore)
		{
			if (_RedirectContentName == null)
			{
				if (RedirectContentID > 0)
				{
					_RedirectContentName = "";
					DataTable dt = cpCore.db.executeQuery("select name from cccontent where id=" + RedirectContentID.ToString());
					if (dt.Rows.Count > 0)
					{
						_RedirectContentName = genericController.encodeText(dt.Rows(0).Item(0));
					}
				}
			}
			return _RedirectContentName;
		}
		public void set_RedirectContentName(coreClass cpCore, string value)
		{
			_RedirectContentName = value;
		}
		private string _RedirectContentName = null;
		//
		//====================================================================================================
		//
//INSTANT C# NOTE: C# does not support parameterized properties - the following property has been divided into two methods:
//ORIGINAL LINE: Public Property MemberSelectGroupName(cpCore As coreClass) As String
		public string get_MemberSelectGroupName(coreClass cpCore)
		{
			if (_MemberSelectGroupName == null)
			{
				if (MemberSelectGroupID > 0)
				{
					_MemberSelectGroupName = "";
					DataTable dt = cpCore.db.executeQuery("select name from cccontent where id=" + MemberSelectGroupID.ToString());
					if (dt.Rows.Count > 0)
					{
						_MemberSelectGroupName = genericController.encodeText(dt.Rows(0).Item(0));
					}
				}
			}
			return _MemberSelectGroupName;
		}
		public void set_MemberSelectGroupName(coreClass cpCore, string value)
		{
			_MemberSelectGroupName = value;
		}
		private string _MemberSelectGroupName = null;
		//
		//====================================================================================================
		//
//INSTANT C# NOTE: C# does not support parameterized properties - the following property has been divided into two methods:
//ORIGINAL LINE: Public Property ManyToManyContentName(cpCore As coreClass) As String
		public string get_ManyToManyContentName(coreClass cpCore)
		{
			if (_ManyToManyRuleContentName == null)
			{
				if (manyToManyContentID > 0)
				{
					_ManyToManyRuleContentName = "";
					DataTable dt = cpCore.db.executeQuery("select name from cccontent where id=" + manyToManyContentID.ToString());
					if (dt.Rows.Count > 0)
					{
						_ManyToManyContentName = genericController.encodeText(dt.Rows(0).Item(0));
					}
				}
			}
			return _ManyToManyContentName;
		}
		public void set_ManyToManyContentName(coreClass cpCore, string value)
		{
			_ManyToManyContentName = value;
		}
		private string _ManyToManyContentName = null;
		//
		//====================================================================================================
		//
//INSTANT C# NOTE: C# does not support parameterized properties - the following property has been divided into two methods:
//ORIGINAL LINE: Public Property ManyToManyRuleContentName(cpCore As coreClass) As String
		public string get_ManyToManyRuleContentName(coreClass cpCore)
		{
			if (_ManyToManyRuleContentName == null)
			{
				if (manyToManyRuleContentID > 0)
				{
					_ManyToManyRuleContentName = "";
					DataTable dt = cpCore.db.executeQuery("select name from cccontent where id=" + manyToManyRuleContentID.ToString());
					if (dt.Rows.Count > 0)
					{
						_ManyToManyRuleContentName = genericController.encodeText(dt.Rows(0).Item(0));
					}
				}
			}
			return _ManyToManyRuleContentName;
		}
		public void set_ManyToManyRuleContentName(coreClass cpCore, string value)
		{
			_ManyToManyRuleContentName = value;
		}
		private string _ManyToManyRuleContentName = null;
		//
		//====================================================================================================
		//
//INSTANT C# NOTE: C# does not support parameterized properties - the following property has been divided into two methods:
//ORIGINAL LINE: Public Property lookupContentName(cpCore As coreClass) As String
		public string get_lookupContentName(coreClass cpCore)
		{
			if (_lookupContentName == null)
			{
				if (lookupContentID > 0)
				{
					_lookupContentName = "";
					DataTable dt = cpCore.db.executeQuery("select name from cccontent where id=" + lookupContentID.ToString());
					if (dt.Rows.Count > 0)
					{
						_lookupContentName = genericController.encodeText(dt.Rows(0).Item(0));
					}
				}
			}
			return _lookupContentName;
		}
		public void set_lookupContentName(coreClass cpCore, string value)
		{
			_lookupContentName = value;
		}
		private string _lookupContentName = null;
		//
		//====================================================================================================
		//
		public object Clone()
		{
			return this.MemberwiseClone();
		}
		//
		//====================================================================================================
		//
		public int CompareTo(object obj)
		{
			Models.Complex.CDefFieldModel c = (Models.Complex.CDefFieldModel)obj;
			return string.Compare(this.nameLc.ToLower(), c.nameLc.ToLower());
		}
	}
}
