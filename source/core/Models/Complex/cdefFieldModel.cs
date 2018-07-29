
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Processor;
using Contensive.Processor.Models.DbModels;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.genericController;
using static Contensive.Processor.constants;
//
namespace Contensive.Processor.Models.Complex {
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
        /// <summary>
        /// name of the field, matches the database field name.
        /// </summary>
        public string nameLc { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// id of the ccField record that holds this metadata
        /// </summary>
        public int id { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// if the field is available in the admin area
        /// </summary>
        public bool active { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// The type of data the field holds
        /// </summary>
        public int fieldTypeId { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// The caption for displaying the field 
        /// </summary>
        public string caption { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// if true value cannot be written back to the database 
        /// </summary>
        public bool readOnly { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// if true a new record can save the fields value, but an edited record cannot be saved
        /// </summary>
        public bool notEditable { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// if true the record cannot be saved if thie field has a null value
        /// </summary>
        public bool required { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// string representation of the default value on a new record
        /// </summary>
        public string defaultValue { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// if true saves will be blocked if other records have this value for this field. An error is thrown so this should be a last resort
        /// </summary>
        public bool uniqueName { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// if true the value is run through RemoveControlCharacters() during rendering. (not during save)
        /// </summary>
        public bool textBuffered { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// field is treated as a password when edited
        /// </summary>
        public bool password { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// if type is REDIRECT, edit form creates a link, this is the field name that must match ID of this record. The edit screen will show a link 
        /// </summary>
        public string redirectID { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// if type is REDIRECT, this is the path to the next page (if blank, current page is used)
        /// </summary>
        public string redirectPath { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// if indexWidth>0, this is the column for the list form, 0 based
        /// </summary>
        public int indexColumn { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// the width of the column in the list form for this field. 
        /// </summary>
        public string indexWidth { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// alpha sort on index page
        /// </summary>
        public int indexSortOrder { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// 1 sorts forward, -1 backward
        /// </summary>
        public int indexSortDirection { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// if true, the edit and list forms only the field to admin
        /// </summary>
        public bool adminOnly { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// if true, the edit and list forms only the field to developer
        /// </summary>
        public bool developerOnly { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// deprecate -- (was - Field Reused to keep binary compatiblity - "IsBaseField" - if true this is a CDefBase field)
        /// </summary>
        public bool blockAccess { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// for html type, if true use a text editor, not a wysiwyg editor
        /// </summary>
        public bool htmlContent { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// if true this field is avaialble in the edit and list forms
        /// </summary>
        public bool authorable { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// if true this field takes its values from a parent, see ContentID
        /// </summary>
        public bool inherited { get; set; }
        //
        // todo this is a running metadata, not storage data. Should be contentName or guid
        //====================================================================================================
        /// <summary>
        /// This is the ID of the Content Def that defines these properties
        /// </summary>
        public int contentId { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// order for edit form
        /// </summary>
        public int editSortPriority { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// if many-to-many type, the field name in the rule table that matches this record's id (the foreign-key in the rule table that points to this record's id)
        /// </summary>
        public string ManyToManyRulePrimaryField { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// if many-to-many type, the field name in the rule table that matches the joining record (the foreign-key in the rule table that points to the joining table's record id)
        /// </summary>
        public string ManyToManyRuleSecondaryField { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// if true an RSS creating process can consider this field as the RSS entity title
        /// </summary>
        public bool RSSTitleField { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// if true an RSS creating process can consider this field as the RSS entity description
        /// </summary>
        public bool RSSDescriptionField { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// in the edit form, this field should appear in this edit tab
        /// </summary>
        public string editTabName { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// if true this field is saved in a two-way encoding format
        /// </summary>
        public bool Scramble { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// for fieldtype lookup, if lookupcontent is null and this is not, this is a comma delimited list of options. The field's value is an index into the list, starting with 1
        /// </summary>
        public string lookupList { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// if true the field has changed and needs to be saved(?)
        /// </summary>
        public bool dataChanged { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// deprecate -- represents that the field was created by aoBase collection. replace with installedByCollectionGuid
        /// </summary>
        public bool isBaseField { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// Legacy - true if the field has been modified sinced it was installed
        /// </summary>
        public bool isModifiedSinceInstalled { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// todo wrong. this is a storage model, so the collection is know and does not need to be in the field model, guid of collection that installed this field
        /// </summary>
        public string installedByCollectionGuid { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// Help text for this field. This text is displayed with the field in the editor
        /// </summary>
        public string helpDefault { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// Legacy help field, deprecate
        /// </summary>
        public string helpCustom { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// Legacy help flag, deprecate
        /// </summary>
        public bool HelpChanged { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// Legacy
        /// </summary>
        public string helpMessage { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// For redirect types, the content id where the field will redirect.
        /// </summary>
        public int redirectContentID { get; set; } // If TYPEREDIRECT, this is new contentID
        public string get_redirectContentName(CoreController core) {
            if (_redirectContentName == null) {
                if (redirectContentID > 0) {
                    _redirectContentName = "";
                    DataTable dt = core.db.executeQuery("select name from cccontent where id=" + redirectContentID.ToString());
                    if (dt.Rows.Count > 0) {
                        _redirectContentName = genericController.encodeText(dt.Rows[0][0]);
                    }
                }
            }
            return _redirectContentName;
        }
        public void set_redirectContentName(CoreController core, string value) {
            _redirectContentName = value;
        }
        private string _redirectContentName = null;
        //
        //====================================================================================================
        /// <summary>
        /// For many-to-many type fields, this is the contentid of the secondary data table.
        /// Primary content is the content that contains the field being definied.
        /// Secondary content is the data table the primary is being connected to.
        /// Rule content is the table that has two foreign keys, one for the primary and one for the secondary
        /// </summary>
        public int manyToManyContentID { get; set; } // Content containing Secondary Records
        public string get_manyToManyContentName(CoreController core) {
            if (_manyToManyRuleContentName == null) {
                if (manyToManyContentID > 0) {
                    _manyToManyRuleContentName = "";
                    DataTable dt = core.db.executeQuery("select name from cccontent where id=" + manyToManyContentID.ToString());
                    if (dt.Rows.Count > 0) {
                        _manyToManyContentName = genericController.encodeText(dt.Rows[0][0]);
                    }
                }
            }
            return _manyToManyContentName;
        }
        public void set_manyToManyContentName(CoreController core, string value) {
            _manyToManyContentName = value;
        }
        private string _manyToManyContentName = null;
        //
        //====================================================================================================
        /// <summary>
        /// For many-to-many fields, the contentid of the rule table. 
        /// Primary content is the content that contains the field being definied.
        /// Secondary content is the data table the primary is being connected to.
        /// Rule content is the table that has two foreign keys, one for the primary and one for the secondary
        /// </summary>
        public int manyToManyRuleContentID { get; set; }
        public string get_manyToManyRuleContentName(CoreController core) {
            if (_manyToManyRuleContentName == null) {
                if (manyToManyRuleContentID > 0) {
                    _manyToManyRuleContentName = "";
                    DataTable dt = core.db.executeQuery("select name from cccontent where id=" + manyToManyRuleContentID.ToString());
                    if (dt.Rows.Count > 0) {
                        _manyToManyRuleContentName = genericController.encodeText(dt.Rows[0][0]);
                    }
                }
            }
            return _manyToManyRuleContentName;
        }
        public void set_manyToManyRuleContentName(CoreController core, string value) {
            _manyToManyRuleContentName = value;
        }
        private string _manyToManyRuleContentName = null;
        //
        //====================================================================================================
        /// <summary>
        /// For lookup types, this is the contentid for the connected table. This represents a foreignKey in this content
        /// </summary>
        public int lookupContentID { get; set; }
        public string get_lookupContentName(CoreController core) {
            if (_lookupContentName == null) {
                if (lookupContentID > 0) {
                    _lookupContentName = "";
                    DataTable dt = core.db.executeQuery("select name from cccontent where id=" + lookupContentID.ToString());
                    if (dt.Rows.Count > 0) {
                        _lookupContentName = genericController.encodeText(dt.Rows[0][0]);
                    }
                }
            }
            return _lookupContentName;
        }
        public void set_lookupContentName(CoreController core, string value) {
            _lookupContentName = value;
        }
        private string _lookupContentName = null;
        //
        //====================================================================================================
        /// <summary>
        /// For memberSelect type content. memberSelectGroup, name set by xml file load, name get for xml file save, id and name get and set in code
        /// </summary>
        public void memberSelectGroupName_set(CoreController core, string memberSelectGroupName) {
            if (_memberSelectGroupName != memberSelectGroupName) {
                _memberSelectGroupName = memberSelectGroupName;
                _memberSelectGroupId = null;
            }
        }
        /// <summary>
        /// For memberSelect type content. memberSelectGroup, name set by xml file load, name get for xml file save, id and name get and set in code
        /// </summary>
        public string memberSelectGroupName_get(CoreController core) {
            if (_memberSelectGroupName == null) {
                if (_memberSelectGroupId != null) {
                    _memberSelectGroupName = core.db.getRecordName("groups", genericController.encodeInteger(_memberSelectGroupId));
                };
            }
            return (_memberSelectGroupName as string);
        }
        /// <summary>
        /// For memberSelect type content. memberSelectGroup, name set by xml file load, name get for xml file save, id and name get and set in code
        /// </summary>
        public void memberSelectGroupId_set(CoreController core, int memberSelectGroupId) {
            if (memberSelectGroupId != _memberSelectGroupId) {
                _memberSelectGroupId = memberSelectGroupId;
                _memberSelectGroupName = null;
            }
        }
        /// <summary>
        /// For memberSelect type content. memberSelectGroup, name set by xml file load, name get for xml file save, id and name get and set in code
        /// </summary>
        public int memberSelectGroupId_get(CoreController core) {
            if (_memberSelectGroupId == null) {
                if (_memberSelectGroupName != null) {
                    _memberSelectGroupId = core.db.getRecordID("groups", genericController.encodeText(_memberSelectGroupName));
                };
            }
            return (genericController.encodeInteger(_memberSelectGroupId));
        }
        private string _memberSelectGroupName = null;
        private int? _memberSelectGroupId = null;
        //
        //====================================================================================================
        /// <summary>
        /// Create a clone of this object
        /// </summary>
        public object Clone() {
            return this.MemberwiseClone();
        }
        //
        //====================================================================================================
        /// <summary>
        /// true if the object being comparied is the same object typea and the name field matches
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj) {
            Models.Complex.cdefFieldModel c = (Models.Complex.cdefFieldModel)obj;
            return string.Compare(this.nameLc.ToLower(), c.nameLc.ToLower());
        }
    }
}
