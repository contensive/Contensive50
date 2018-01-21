
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
using Contensive.Core.Models.DbModels;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
//
namespace Contensive.Core.Models.Complex {
    //
    //====================================================================================================
    /// <summary>
    /// The metadata for a field
    /// public properties are those read/written to the XML file
    /// public methods translate the properties as needed
    /// For example, a lookup field stores the name of the content it joins in . The site runs with the id of 
    /// that content. This model has the content 'name'
    /// </summary>
    //
    [Serializable]
    public class cdefFieldModel : ICloneable, IComparable {
        //
        //====================================================================================================
        // name of the field, matches the database field name.
        public string nameLc { get; set; }
        //
        //====================================================================================================
        // id of the ccField record that holds this metadata
        public int id { get; set; }
        //
        //====================================================================================================
        // if the field is available in the admin area
        public bool active { get; set; }
        //
        //====================================================================================================
        // The type of data the field holds
        public int fieldTypeId { get; set; }
        //
        //====================================================================================================
        // The caption for displaying the field 
        public string caption { get; set; }
        //
        //====================================================================================================
        // if true value cannot be written back to the database 
        public bool readOnly { get; set; }
        //
        //====================================================================================================
        // if true a new record can save the fields value, but an edited record cannot be saved
        public bool notEditable { get; set; }
        //
        //====================================================================================================
        // if true the record cannot be saved if thie field has a null value
        public bool required { get; set; }
        //
        //====================================================================================================
        // string representation of the default value on a new record
        public string defaultValue { get; set; }
        //
        //====================================================================================================
        // if true saves will be blocked if other records have this value for this field. An error is thrown so this should be a last resort
        public bool uniqueName { get; set; }
        //
        //====================================================================================================
        // if true the value is run through RemoveControlCharacters() during rendering. (not during save)
        public bool textBuffered { get; set; }
        //
        //====================================================================================================
        // field is treated as a password when edited
        public bool password { get; set; }
        //
        //====================================================================================================
        // if type is REDIRECT, edit form creates a link, this is the field name that must match ID of this record. The edit screen will show a link 
        public string redirectID { get; set; }
        //
        //====================================================================================================
        // if type is REDIRECT, this is the path to the next page (if blank, current page is used)
        public string redirectPath { get; set; }
        //
        //====================================================================================================
        // if indexWidth>0, this is the column for the list form, 0 based
        public int indexColumn { get; set; }
        //
        //====================================================================================================
        // the width of the column in the list form for this field. 
        public string indexWidth { get; set; }
        //
        //====================================================================================================
        // alpha sort on index page
        public int indexSortOrder { get; set; }
        //
        //====================================================================================================
        // 1 sorts forward, -1 backward
        public int indexSortDirection { get; set; }
        //
        //====================================================================================================
        // if true, the edit and list forms only the field to admin
        public bool adminOnly { get; set; }
        //
        //====================================================================================================
        // if true, the edit and list forms only the field to developer
        public bool developerOnly { get; set; }
        //
        //====================================================================================================
        // todo deprecate
        // deprecated -- (was - Field Reused to keep binary compatiblity - "IsBaseField" - if true this is a CDefBase field)
        public bool blockAccess { get; set; }
        //
        //====================================================================================================
        // for html type, if true use a text editor, not a wysiwyg editor
        public bool htmlContent { get; set; }
        //
        //====================================================================================================
        // if true this field is avaialble in the edit and list forms
        public bool authorable { get; set; }
        //
        //====================================================================================================
        // todo research this?
        // if true this field takes its values from a parent, see ContentID
        public bool inherited { get; set; }
        //
        //====================================================================================================
        // todo this is a running metadata, not storage data. Should be contentName or guid
        // This is the ID of the Content Def that defines these properties
        public int contentId { get; set; }
        //
        //====================================================================================================
        // order for edit form
        public int editSortPriority { get; set; }
        //
        //====================================================================================================
        // if many-to-many type, the field name in the rule table that matches this record's id (the foreign-key in the rule table that points to this record's id)
        public string ManyToManyRulePrimaryField { get; set; }
        //
        //====================================================================================================
        // if many-to-many type, the field name in the rule table that matches the joining record (the foreign-key in the rule table that points to the joining table's record id)
        public string ManyToManyRuleSecondaryField { get; set; }
        //
        //====================================================================================================
        // if true an RSS creating process can consider this field as the RSS entity title
        public bool RSSTitleField { get; set; }
        //
        //====================================================================================================
        // if true an RSS creating process can consider this field as the RSS entity description
        public bool RSSDescriptionField { get; set; }
        //
        //====================================================================================================
        // in the edit form, this field should appear in this edit tab
        public string editTabName { get; set; }
        //
        //====================================================================================================
        // if true this field is saved in a two-way encoding format
        public bool Scramble { get; set; }
        //
        //====================================================================================================
        // for fieldtype lookup, if lookupcontent is null and this is not, this is a comma delimited list of options. The field's value is an index into the list, starting with 1
        public string lookupList { get; set; }
        //
        //====================================================================================================
        // todo research this
        // if true the field has changed and needs to be saved(?)
        public bool dataChanged { get; set; }
        //
        //====================================================================================================
        // todo remove this
        // deprecated -- represents that the field was created by aoBase collection. replace with installedByCollectionGuid
        public bool isBaseField { get; set; }
        //
        //====================================================================================================
        // todo research this
        public bool isModifiedSinceInstalled { get; set; }
        //
        //====================================================================================================
        // todo wrong. this is a storage model, so the collection is know and does not need to be in the field model
        // guid of collection that installed this field
        public string installedByCollectionGuid { get; set; }
        //
        //====================================================================================================
        // todo - research how to do help
        public string HelpDefault { get; set; }
        //
        //====================================================================================================
        // todo - research how to do help
        public string HelpCustom { get; set; }
        //
        //====================================================================================================
        // todo - research how to do help
        public bool HelpChanged { get; set; }
        //
        //====================================================================================================
        // todo - research how to do help
        public string helpMessage { get; set; }
        //
        //====================================================================================================
        // fields stored differently in xml collection files
        //
        public int redirectContentID { get; set; } // If TYPEREDIRECT, this is new contentID
        public string get_RedirectContentName(coreController cpCore) {
            if (_RedirectContentName == null) {
                if (redirectContentID > 0) {
                    _RedirectContentName = "";
                    DataTable dt = cpCore.db.executeQuery("select name from cccontent where id=" + redirectContentID.ToString());
                    if (dt.Rows.Count > 0) {
                        _RedirectContentName = genericController.encodeText(dt.Rows[0][0]);
                    }
                }
            }
            return _RedirectContentName;
        }
        public void set_RedirectContentName(coreController cpCore, string value) {
            _RedirectContentName = value;
        }
        private string _RedirectContentName = null;
        //
        //====================================================================================================
        //
        public int manyToManyContentID { get; set; } // Content containing Secondary Records
        public string get_ManyToManyContentName(coreController cpCore) {
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
        public void set_ManyToManyContentName(coreController cpCore, string value) {
            _ManyToManyContentName = value;
        }
        private string _ManyToManyContentName = null;
        //
        //====================================================================================================
        //
        public int manyToManyRuleContentID { get; set; }
        public string get_ManyToManyRuleContentName(coreController cpCore) {
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
        public void set_ManyToManyRuleContentName(coreController cpCore, string value) {
            _ManyToManyRuleContentName = value;
        }
        private string _ManyToManyRuleContentName = null;
        //
        //====================================================================================================
        //
        public int lookupContentID { get; set; }
        public string get_lookupContentName(coreController cpCore) {
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
        public void set_lookupContentName(coreController cpCore, string value) {
            _lookupContentName = value;
        }
        private string _lookupContentName = null;
        //
        //====================================================================================================
        // memberSelectGroup
        // name set by xml file load
        // name get for xml file save
        // id and name get and set in code
        public void memberSelectGroupName_set(coreController cpcore, string memberSelectGroupName) {
            if (_memberSelectGroupName != memberSelectGroupName) {
                _memberSelectGroupName = memberSelectGroupName;
                _memberSelectGroupId = null;
            }
        }
        public string memberSelectGroupName_get(coreController cpcore) {
            if (_memberSelectGroupName == null) {
                if (_memberSelectGroupId != null) {
                    _memberSelectGroupName = cpcore.db.getRecordName("groups", genericController.encodeInteger(_memberSelectGroupId));
                };
            }
            return (_memberSelectGroupName as string);
        }
        public void memberSelectGroupId_set(coreController cpcore, int memberSelectGroupId) {
            if (memberSelectGroupId != _memberSelectGroupId) {
                _memberSelectGroupId = memberSelectGroupId;
                _memberSelectGroupName = null;
            }
        }
        public int memberSelectGroupId_get(coreController cpcore) {
            if (_memberSelectGroupId == null) {
                if (_memberSelectGroupName != null) {
                    _memberSelectGroupId = cpcore.db.getRecordID("groups", genericController.encodeText(_memberSelectGroupName));
                };
            }
            return (genericController.encodeInteger(_memberSelectGroupId));
        }
        private string _memberSelectGroupName = null;
        private int? _memberSelectGroupId = null;
        //
        //
        //
        //public int MemberSelectGroupID_old { get; set; }
        //public string get_MemberSelectGroupName_old(coreClass cpCore) {
        //    if (_MemberSelectGroupName_old == null) {
        //        if (MemberSelectGroupID_old > 0) {
        //            _MemberSelectGroupName_old = "";
        //            DataTable dt = cpCore.db.executeQuery("select name from cccontent where id=" + MemberSelectGroupID_old.ToString());
        //            if (dt.Rows.Count > 0) {
        //                _MemberSelectGroupName_old = genericController.encodeText(dt.Rows[0][0]);
        //            }
        //        }
        //    }
        //    return _MemberSelectGroupName_old;
        //}
        //public void set_MemberSelectGroupName_old(coreClass cpCore, string value) {
        //    _MemberSelectGroupName_old = value;
        //}
        //private string _MemberSelectGroupName_old = null;
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
