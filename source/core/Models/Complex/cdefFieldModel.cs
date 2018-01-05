
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Core;
using Contensive.Core.Models.Entity;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
//
namespace Contensive.Core.Models.Complex {
    //
    // ---------------------------------------------------------------------------------------------------
    // ----- CDefFieldClass
    //       class not structure because it has to marshall to vb6
    // ---------------------------------------------------------------------------------------------------
    //
    [Serializable]
    public class cdefFieldModel : ICloneable, IComparable {
        //
        // The name of the field
        public string nameLc { get; set; }
        //
        // the ID in the ccContentFields Table that this came from
        public int id { get; set; }
        //
        // if the field is available in the admin area
        public bool active { get; set; }
        //
        // The type of data the field holds
        public int fieldTypeId { get; set; }
        //
        // The caption for displaying the field 
        public string caption { get; set; }
        //
        // was ReadOnly -- If true, this field can not be written back to the database 
        public bool ReadOnly { get; set; }
        //
        // if true, you can only edit new records
        public bool NotEditable { get; set; }
        //
        // if true, this field must be entered
        public bool Required { get; set; }
        //
        // default value on a new record
        public string defaultValue { get; set; }
        //
        // explaination of this field
        public string HelpMessage { get; set; } 
        //
        public bool UniqueName { get; set; }
        //
        // if true, the input is run through RemoveControlCharacters()
        public bool TextBuffered { get; set; }
        //
        // for text boxes, sets the password attribute
        public bool Password { get; set; }
        //
        // If TYPEREDIRECT, this is the field that must match ID of this record
        public string RedirectID { get; set; }
        //
        // New Field, If TYPEREDIRECT, this is the path to the next page (if blank, current page is used)
        public string RedirectPath { get; set; }
        //
        // the column desired in the admin index form
        public int indexColumn { get; set; }
        //
        // either number or percentage, blank if not included
        public string indexWidth { get; set; }
        //
        // alpha sort on index page
        public int indexSortOrder { get; set; }
        //
        // 1 sorts forward, -1 backward
        public int indexSortDirection { get; set; }
        //
        // This field is only available to administrators
        public bool adminOnly { get; set; }
        //
        // This field is only available to administrators
        public bool developerOnly { get; set; }
        //
        // ***** Field Reused to keep binary compatiblity - "IsBaseField" - if true this is a CDefBase field
        public bool blockAccess { get; set; }
        //
        // if true, the HTML editor (active edit) can be used
        public bool htmlContent { get; set; }
        //
        // true if it can be seen in the admin form
        public bool authorable { get; set; }
        //
        // if true, this field takes its values from a parent, see ContentID
        public bool inherited { get; set; }
        //
        // This is the ID of the Content Def that defines these properties
        public int contentId { get; set; }
        //
        // The Admin Edit Sort Order
        public int editSortPriority { get; set; }
        //
        // Rule Field Name for Primary Table
        public string ManyToManyRulePrimaryField { get; set; }
        //
        // Rule Field Name for Secondary Table
        public string ManyToManyRuleSecondaryField { get; set; }
        //
        // When creating RSS fields from this content, this is the title
        public bool RSSTitleField { get; set; }
        //
        // When creating RSS fields from this content, this is the description
        public bool RSSDescriptionField { get; set; }
        //
        // Editing group - used for the tabs
        public string editTabName { get; set; }
        //
        // save the field scrambled in the Db
        public bool Scramble { get; set; }
        //
        // If TYPELOOKUP, and LookupContentID is null, this is a comma separated list of choices
        public string lookupList { get; set; } 
        //
        public bool dataChanged { get; set; }
        //
        public bool isBaseField { get; set; }
        //
        public bool isModifiedSinceInstalled { get; set; }
        //
        public string installedByCollectionGuid { get; set; }
        //
        public string HelpDefault { get; set; }
        //
        public string HelpCustom { get; set; }
        //
        public bool HelpChanged { get; set; }
        //
        // fields stored differently in xml collection files
        //
        //   name is loaded from xml collection files 
        //   id is created during the cacheLoad process when loading from Db (and used in metaData)
        //
        public int lookupContentID { get; set; } // If TYPELOOKUP, (for Db controled sites) this is the content ID of the source table
        //
        public int RedirectContentID { get; set; } // If TYPEREDIRECT, this is new contentID
        //
        public int manyToManyContentID { get; set; } // Content containing Secondary Records
        //
        // Content with rules between Primary and Secondary
        public int manyToManyRuleContentID { get; set; }
        //
        // if type is memberselect, this is the group from which user's can be selected
        // NOTE: this field was originally in the xml as memberselectgroupId, not memberselectgroup. The name is a hold-over 
        public int MemberSelectGroupID { get; set; }
        //
        //====================================================================================================
        //
        //todo  NOTE: C# does not support parameterized properties - the following property has been divided into two methods:
        //ORIGINAL LINE: Public Property RedirectContentName(cpCore As coreClass) As String
        public string get_RedirectContentName(coreClass cpCore) {
            if (_RedirectContentName == null) {
                if (RedirectContentID > 0) {
                    _RedirectContentName = "";
                    DataTable dt = cpCore.db.executeQuery("select name from cccontent where id=" + RedirectContentID.ToString());
                    if (dt.Rows.Count > 0) {
                        _RedirectContentName = genericController.encodeText(dt.Rows[0][0]);
                    }
                }
            }
            return _RedirectContentName;
        }
        public void set_RedirectContentName(coreClass cpCore, string value) {
            _RedirectContentName = value;
        }
        private string _RedirectContentName = null;
        ////
        ////====================================================================================================
        ////
        ////todo  NOTE: C# does not support parameterized properties - the following property has been divided into two methods:
        ////ORIGINAL LINE: Public Property MemberSelectGroupName(cpCore As coreClass) As String
        //public string get_MemberSelectGroupName(coreClass cpCore) {
        //    if (_MemberSelectGroupName == null) {
        //        if (MemberSelectGroupID > 0) {
        //            _MemberSelectGroupName = "";
        //            DataTable dt = cpCore.db.executeQuery("select name from cccontent where id=" + MemberSelectGroupID.ToString());
        //            if (dt.Rows.Count > 0) {
        //                _MemberSelectGroupName = genericController.encodeText(dt.Rows[0][0]);
        //            }
        //        }
        //    }
        //    return _MemberSelectGroupName;
        //}
        //public void set_MemberSelectGroupName(coreClass cpCore, string value) {
        //    _MemberSelectGroupName = value;
        //}
        //private string _MemberSelectGroupName = null;
        //
        //====================================================================================================
        //
        //todo  NOTE: C# does not support parameterized properties - the following property has been divided into two methods:
        //ORIGINAL LINE: Public Property ManyToManyContentName(cpCore As coreClass) As String
        public string get_ManyToManyContentName(coreClass cpCore) {
            if (_ManyToManyRuleContentName == null) {
                if (manyToManyContentID > 0) {
                    _ManyToManyRuleContentName = "";
                    DataTable dt = cpCore.db.executeQuery("select name from cccontent where id=" + manyToManyContentID.ToString());
                    if (dt.Rows.Count > 0) {
                        _ManyToManyContentName = genericController.encodeText(dt.Rows[0][0]);
                    }
                }
            }
            return _ManyToManyContentName;
        }
        public void set_ManyToManyContentName(coreClass cpCore, string value) {
            _ManyToManyContentName = value;
        }
        private string _ManyToManyContentName = null;
        //
        //====================================================================================================
        //
        //todo  NOTE: C# does not support parameterized properties - the following property has been divided into two methods:
        //ORIGINAL LINE: Public Property ManyToManyRuleContentName(cpCore As coreClass) As String
        public string get_ManyToManyRuleContentName(coreClass cpCore) {
            if (_ManyToManyRuleContentName == null) {
                if (manyToManyRuleContentID > 0) {
                    _ManyToManyRuleContentName = "";
                    DataTable dt = cpCore.db.executeQuery("select name from cccontent where id=" + manyToManyRuleContentID.ToString());
                    if (dt.Rows.Count > 0) {
                        _ManyToManyRuleContentName = genericController.encodeText(dt.Rows[0][0]);
                    }
                }
            }
            return _ManyToManyRuleContentName;
        }
        public void set_ManyToManyRuleContentName(coreClass cpCore, string value) {
            _ManyToManyRuleContentName = value;
        }
        private string _ManyToManyRuleContentName = null;
        //
        //====================================================================================================
        //
        //todo  NOTE: C# does not support parameterized properties - the following property has been divided into two methods:
        //ORIGINAL LINE: Public Property lookupContentName(cpCore As coreClass) As String
        public string get_lookupContentName(coreClass cpCore) {
            if (_lookupContentName == null) {
                if (lookupContentID > 0) {
                    _lookupContentName = "";
                    DataTable dt = cpCore.db.executeQuery("select name from cccontent where id=" + lookupContentID.ToString());
                    if (dt.Rows.Count > 0) {
                        _lookupContentName = genericController.encodeText(dt.Rows[0][0]);
                    }
                }
            }
            return _lookupContentName;
        }
        public void set_lookupContentName(coreClass cpCore, string value) {
            _lookupContentName = value;
        }
        private string _lookupContentName = null;
        //
        //====================================================================================================
        //
        public object Clone() {
            return this.MemberwiseClone();
        }
        //
        //====================================================================================================
        //
        public int CompareTo(object obj) {
            Models.Complex.cdefFieldModel c = (Models.Complex.cdefFieldModel)obj;
            return string.Compare(this.nameLc.ToLower(), c.nameLc.ToLower());
        }
    }
}
