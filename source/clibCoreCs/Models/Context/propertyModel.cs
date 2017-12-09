
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
namespace Contensive.Core.Models.Context {
    public class propertyModelClass {
        //
        private coreClass cpCore;
        //
        // visit property cache
        //
        private string[,] propertyCache;
        private keyPtrController propertyCache_nameIndex;
        private bool propertyCacheLoaded = false;
        private int propertyCacheCnt;
        private int propertyTypeId;
        //
        //==============================================================================================
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="propertyTypeId"></param>
        /// <remarks></remarks>
        public propertyModelClass(coreClass cpCore, int propertyTypeId) {
            this.cpCore = cpCore;
            this.propertyTypeId = propertyTypeId;
        }
        //
        //====================================================================================================
        /// <summary>
        /// set property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="PropertyValue"></param>
        public void setProperty(string propertyName, double PropertyValue) {
            setProperty(propertyName, PropertyValue.ToString());
        }
        //
        //====================================================================================================
        /// <summary>
        /// set property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="PropertyValue"></param>
        public void setProperty(string propertyName, bool PropertyValue) {
            setProperty(propertyName, PropertyValue.ToString());
        }
        //
        //====================================================================================================
        /// <summary>
        /// set property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="PropertyValue"></param>
        public void setProperty(string propertyName, DateTime PropertyValue) {
            setProperty(propertyName, PropertyValue.ToString());
        }
        //
        //====================================================================================================
        /// <summary>
        /// set property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="PropertyValue"></param>
        public void setProperty(string propertyName, int PropertyValue) {
            setProperty(propertyName, PropertyValue.ToString());
        }
        //
        //====================================================================================================
        /// <summary>
        /// set property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="PropertyValue"></param>
        public void setProperty(string propertyName, string PropertyValue) {
            switch (propertyTypeId) {
                case PropertyTypeVisit:
                    setProperty(propertyName, PropertyValue, cpCore.doc.authContext.visit.id);
                    break;
                case PropertyTypeVisitor:
                    setProperty(propertyName, PropertyValue, cpCore.doc.authContext.visitor.ID);
                    break;
                case PropertyTypeMember:
                    setProperty(propertyName, PropertyValue, cpCore.doc.authContext.user.id);
                    break;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// set property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="PropertyValue"></param>
        /// <param name="keyId">keyId is like vistiId, vistorId, userId</param>
        public void setProperty(string propertyName, string PropertyValue, int keyId) {
            try {
                //
                int Ptr = 0;
                int RecordID = 0;
                int CS = 0;
                string SQLNow = null;
                dbController db = cpCore.db;
                //
                Ptr = -1;
                if (!propertyCacheLoaded) {
                    loadFromDb(keyId);
                }
                //
                if (propertyCacheCnt > 0) {
                    Ptr = propertyCache_nameIndex.getPtr(propertyName);
                }
                if (Ptr < 0) {
                    Ptr = propertyCacheCnt;
                    propertyCacheCnt = propertyCacheCnt + 1;
                    //INSTANT C# NOTE: The following block reproduces what 'ReDim Preserve' does behind the scenes in VB:
                    //ORIGINAL LINE: ReDim Preserve propertyCache(2, Ptr)
                    string[,] tempVar = new string[3, Ptr + 1];
                    if (propertyCache != null) {
                        for (int Dimension0 = 0; Dimension0 < propertyCache.GetLength(0); Dimension0++) {
                            int CopyLength = Math.Min(propertyCache.GetLength(1), tempVar.GetLength(1));
                            for (int Dimension1 = 0; Dimension1 < CopyLength; Dimension1++) {
                                tempVar[Dimension0, Dimension1] = propertyCache[Dimension0, Dimension1];
                            }
                        }
                    }
                    propertyCache = tempVar;
                    propertyCache[0, Ptr] = propertyName;
                    propertyCache[1, Ptr] = PropertyValue;
                    propertyCache_nameIndex.setPtr(propertyName, Ptr);
                    //
                    // insert a new property record, get the ID back and save it in cache
                    //
                    CS = db.csInsertRecord("Properties", SystemMemberID);
                    if (db.csOk(CS)) {
                        propertyCache[2, Ptr] = db.csGetText(CS, "ID");
                        db.csSet(CS, "name", propertyName);
                        db.csSet(CS, "FieldValue", PropertyValue);
                        db.csSet(CS, "TypeID", propertyTypeId);
                        db.csSet(CS, "KeyID", keyId.ToString());
                    }
                    db.csClose(ref CS);
                } else if (propertyCache[1, Ptr] != PropertyValue) {
                    propertyCache[1, Ptr] = PropertyValue;
                    RecordID = genericController.EncodeInteger(propertyCache[2, Ptr]);
                    SQLNow = db.encodeSQLDate(DateTime.Now);
                    //
                    // save the value in the property that was found
                    //
                    db.executeQuery("update ccProperties set FieldValue=" + db.encodeSQLText(PropertyValue) + ",ModifiedDate=" + SQLNow + " where id=" + RecordID);
                }
                //

                //
                return;
                //
                // ----- Error Trap
                //
            } catch( Exception ex ) {
                cpCore.handleException(ex);
            }
            //ErrorTrap:
            cpCore.handleException(new Exception("Unexpected exception"));
        }
        //
        //====================================================================================================
        /// <summary>
        /// get a property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="keyId"></param>
        /// <returns></returns>
        public DateTime getDate(string propertyName) {
            return getDate(propertyName, DateTime.MinValue);
        }
        //
        //====================================================================================================
        /// <summary>
        /// get a property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="keyId"></param>
        /// <returns></returns>
        public DateTime getDate(string propertyName, DateTime defaultValue) {
            switch (propertyTypeId) {
                case PropertyTypeVisit:
                    return getDate(propertyName, defaultValue, cpCore.doc.authContext.visit.id);
                case PropertyTypeVisitor:
                    return getDate(propertyName, defaultValue, cpCore.doc.authContext.visitor.ID);
                case PropertyTypeMember:
                    return getDate(propertyName, defaultValue, cpCore.doc.authContext.user.id);
            }
            return DateTime.MinValue;
        }
        //
        //====================================================================================================
        /// <summary>
        /// get a property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="keyId"></param>
        /// <returns></returns>
        public DateTime getDate(string propertyName, DateTime defaultValue, int keyId) {
            return genericController.EncodeDate(getText(propertyName, genericController.encodeText(defaultValue), keyId));
        }
        //
        //====================================================================================================
        /// <summary>
        /// get a property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="keyId"></param>
        /// <returns></returns>
        public double getNumber(string propertyName) {
            return getNumber(propertyName, 0);
        }
        //
        //====================================================================================================
        /// <summary>
        /// get a property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="keyId"></param>
        /// <returns></returns>
        public double getNumber(string propertyName, double defaultValue) {
            switch (propertyTypeId) {
                case PropertyTypeVisit:
                    return getNumber(propertyName, defaultValue, cpCore.doc.authContext.visit.id);
                case PropertyTypeVisitor:
                    return getNumber(propertyName, defaultValue, cpCore.doc.authContext.visitor.ID);
                case PropertyTypeMember:
                    return getNumber(propertyName, defaultValue, cpCore.doc.authContext.user.id);
            }
            return 0;
        }
        //
        //====================================================================================================
        /// <summary>
        /// get a boolean property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="keyId"></param>
        /// <returns></returns>
        public double getNumber(string propertyName, double defaultValue, int keyId) {
            return EncodeNumber(getText(propertyName, genericController.encodeText(defaultValue), keyId));
        }
        //
        //====================================================================================================
        /// <summary>
        /// get a property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="keyId"></param>
        /// <returns></returns>
        public bool getBoolean(string propertyName) {
            return getBoolean(propertyName, false);
        }
        //
        //====================================================================================================
        /// <summary>
        /// get a property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="keyId"></param>
        /// <returns></returns>
        public bool getBoolean(string propertyName, bool defaultValue) {
            switch (propertyTypeId) {
                case PropertyTypeVisit:
                    return getBoolean(propertyName, defaultValue, cpCore.doc.authContext.visit.id);
                case PropertyTypeVisitor:
                    return getBoolean(propertyName, defaultValue, cpCore.doc.authContext.visitor.ID);
                case PropertyTypeMember:
                    return getBoolean(propertyName, defaultValue, cpCore.doc.authContext.user.id);
            }
            return false;
        }
        //
        //====================================================================================================
        /// <summary>
        /// get a boolean property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="keyId"></param>
        /// <returns></returns>
        public bool getBoolean(string propertyName, bool defaultValue, int keyId) {
            return genericController.EncodeBoolean(getText(propertyName, genericController.encodeText(defaultValue), keyId));
        }
        //
        //====================================================================================================
        /// <summary>
        /// get an integer property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="keyId"></param>
        /// <returns></returns>
        public int getInteger(string propertyName) {
            return getInteger(propertyName, 0);
        }
        //
        //====================================================================================================
        /// <summary>
        /// get an integer property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="keyId"></param>
        /// <returns></returns>
        public int getInteger(string propertyName, int defaultValue) {
            switch (propertyTypeId) {
                case PropertyTypeVisit:
                    return getInteger(propertyName, defaultValue, cpCore.doc.authContext.visit.id);
                case PropertyTypeVisitor:
                    return getInteger(propertyName, defaultValue, cpCore.doc.authContext.visitor.ID);
                case PropertyTypeMember:
                    return getInteger(propertyName, defaultValue, cpCore.doc.authContext.user.id);
            }
            return 0;
        }
        //
        //====================================================================================================
        /// <summary>
        /// get an integer property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="keyId"></param>
        /// <returns></returns>
        public int getInteger(string propertyName, int defaultValue, int keyId) {
            return genericController.EncodeInteger(getText(propertyName, genericController.encodeText(defaultValue), keyId));
        }
        //
        //====================================================================================================
        /// <summary>
        /// get a string property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="keyId"></param>
        /// <returns></returns>
        public string getText(string propertyName) {
            return getText(propertyName, "");
        }
        //
        //====================================================================================================
        /// <summary>
        /// get a string property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="keyId"></param>
        /// <returns></returns>
        public string getText(string propertyName, string defaultValue) {
            switch (propertyTypeId) {
                case PropertyTypeVisit:
                    return getText(propertyName, defaultValue, cpCore.doc.authContext.visit.id);
                case PropertyTypeVisitor:
                    return getText(propertyName, defaultValue, cpCore.doc.authContext.visitor.ID);
                case PropertyTypeMember:
                    return getText(propertyName, defaultValue, cpCore.doc.authContext.user.id);
            }
            return "";
        }
        //
        //====================================================================================================
        /// <summary>
        /// get a string property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="keyId"></param>
        /// <returns></returns>
        public string getText(string propertyName, string defaultValue, int keyId) {
            string returnString = "";
            try {
                int Ptr = 0;
                bool Found = false;
                //
                Ptr = -1;
                if (!propertyCacheLoaded) {
                    loadFromDb(keyId);
                }
                //
                if (propertyCacheCnt > 0) {
                    Ptr = propertyCache_nameIndex.getPtr(propertyName);
                    if (Ptr >= 0) {
                        returnString = genericController.encodeText(propertyCache[1, Ptr]);
                        Found = true;
                    }
                }
                //
                if (!Found) {
                    returnString = defaultValue;
                    setProperty(propertyName, defaultValue, keyId);
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnString;
        }
        //
        //
        //
        private void loadFromDb(int keyId) {
            try {
                //
                string Name = null;
                dbController db = cpCore.db;
                //
                propertyCache_nameIndex = new keyPtrController();
                propertyCacheCnt = 0;
                //
                using (DataTable dt = db.executeQuery("select Name,FieldValue,ID from ccProperties where (active<>0)and(TypeID=" + propertyTypeId + ")and(KeyID=" + keyId + ")")) {
                    if (dt.Rows.Count > 0) {
                        propertyCacheCnt = 0;
                        propertyCache = new string[3, dt.Rows.Count];
                        foreach (DataRow dr in dt.Rows) {
                            Name = genericController.encodeText(dr[0]);
                            propertyCache[0, propertyCacheCnt] = Name;
                            propertyCache[1, propertyCacheCnt] = genericController.encodeText(dr[1]);
                            propertyCache[2, propertyCacheCnt] = genericController.EncodeInteger(dr[2]).ToString();
                            propertyCache_nameIndex.setPtr(Name.ToLower(), propertyCacheCnt);
                            propertyCacheCnt += 1;
                        }
                        propertyCacheCnt = dt.Rows.Count;
                    }
                }
                //
                propertyCacheLoaded = true;
                //
                return;
                //
                // ----- Error Trap
                //
            } catch( Exception ex ) {
                cpCore.handleException(ex);
            }
            //ErrorTrap:
            cpCore.handleException(new Exception("Unexpected exception"));
        }
    }
}
