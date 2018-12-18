
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
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using System.Globalization;
//
namespace Contensive.Processor.Models.Domain {
    /// <summary>
    /// Manage User, Visit and Visitor properties
    /// </summary>
    public class PropertyModelClass {
        //
        private CoreController core;
        //
        public enum PropertyTypeEnum {
            user=0,
            visit=1,
            visitor=2
        }
        /// <summary>
        /// The propertyType for instance of PropertyModel 
        /// </summary>
        private readonly PropertyTypeEnum propertyType;
        /// <summary>
        /// The key used for property references from this instance (visitId, visitorId, or memberId)
        /// </summary>
        private readonly int propertyKeyId;
        //
        //
        // todo change array to dictionary
        private string[,] propertyCache;
        private KeyPtrController propertyCache_nameIndex;
        private bool propertyCacheLoaded = false;
        private int propertyCacheCnt;
        //
        //==============================================================================================
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="propertyType"></param>
        /// <remarks></remarks>
        public PropertyModelClass(CoreController core, PropertyTypeEnum propertyType) {
            this.core = core;
            this.propertyType = propertyType;
            switch (propertyType) {
                case PropertyTypeEnum.visit:
                    propertyKeyId = core.session.visit.id;
                    break;
                case PropertyTypeEnum.visitor:
                    propertyKeyId = core.session.visitor.id;
                    break;
                default:
                    propertyKeyId = core.session.user.id;
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
        public void setProperty(string propertyName, double PropertyValue) => setProperty(propertyName, PropertyValue.ToString(CultureInfo.InvariantCulture), propertyKeyId);
        //
        //====================================================================================================
        /// <summary>
        /// set property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="PropertyValue"></param>
        public void setProperty(string propertyName, bool PropertyValue) => setProperty(propertyName, PropertyValue.ToString(), propertyKeyId);
        //
        //====================================================================================================
        /// <summary>
        /// set property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="PropertyValue"></param>
        public void setProperty(string propertyName, DateTime PropertyValue) => setProperty(propertyName, PropertyValue.ToString(CultureInfo.InvariantCulture), propertyKeyId);
        //
        //====================================================================================================
        /// <summary>
        /// set property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="PropertyValue"></param>
        public void setProperty(string propertyName, int PropertyValue) => setProperty(propertyName, PropertyValue.ToString(), propertyKeyId);
        //
        //====================================================================================================
        /// <summary>
        /// set property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="PropertyValue"></param>
        public void setProperty(string propertyName, string PropertyValue) => setProperty(propertyName, PropertyValue, propertyKeyId);
        //
        //====================================================================================================
        /// <summary>
        /// set property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        /// <param name="keyId">keyId is like vistiId, vistorId, userId</param>
        public void setProperty(string propertyName, string propertyValue, int keyId) {
            try {
                if (!propertyCacheLoaded) {
                    loadFromDb(keyId);
                }
                int Ptr = -1;
                if (propertyCacheCnt > 0) { Ptr = propertyCache_nameIndex.getPtr(propertyName); }
                if (Ptr < 0) {
                    Ptr = propertyCacheCnt;
                    propertyCacheCnt += 1;
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
                    propertyCache[1, Ptr] = propertyValue;
                    propertyCache_nameIndex.setPtr(propertyName, Ptr);
                    //
                    // insert a new property record, get the ID back and save it in cache
                    //
                    int CS = core.db.csInsertRecord("Properties", SystemMemberID);
                    if (core.db.csOk(CS)) {
                        propertyCache[2, Ptr] = core.db.csGetText(CS, "ID");
                        core.db.csSet(CS, "name", propertyName);
                        core.db.csSet(CS, "FieldValue", propertyValue);
                        core.db.csSet(CS, "TypeID", (int)propertyType);
                        core.db.csSet(CS, "KeyID", keyId.ToString());
                    }
                    core.db.csClose(ref CS);
                } else if (propertyCache[1, Ptr] != propertyValue) {
                    propertyCache[1, Ptr] = propertyValue;
                    int RecordID = GenericController.encodeInteger(propertyCache[2, Ptr]);
                    string SQLNow = DbController.encodeSQLDate(DateTime.Now);
                    //
                    // save the value in the property that was found
                    //
                    core.db.executeQuery("update ccProperties set FieldValue=" + DbController.encodeSQLText(propertyValue) + ",ModifiedDate=" + SQLNow + " where id=" + RecordID);
                }
            } catch( Exception ex ) {
                LogController.handleError( core,ex);
            }
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
        public DateTime getDate(string propertyName) => encodeDate(getText(propertyName, encodeText(DateTime.MinValue), propertyKeyId)); 
        //
        //====================================================================================================
        /// <summary>
        /// get a property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="keyId"></param>
        /// <returns></returns>
        public DateTime getDate(string propertyName, DateTime defaultValue) => encodeDate(getText(propertyName, encodeText(defaultValue), propertyKeyId)); 
        //
        //====================================================================================================
        /// <summary>
        /// get a property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="keyId"></param>
        /// <returns></returns>
        public double getNumber(string propertyName) => encodeNumber(getText(propertyName, encodeText(0), propertyKeyId)); 
        //
        //====================================================================================================
        /// <summary>
        /// get a property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="keyId"></param>
        /// <returns></returns>
        public double getNumber(string propertyName, double defaultValue) => encodeNumber(getText(propertyName, encodeText(defaultValue), propertyKeyId));
        //
        //====================================================================================================
        /// <summary>
        /// get a property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="keyId"></param>
        /// <returns></returns>
        public bool getBoolean(string propertyName) => encodeBoolean(getText(propertyName, encodeText(false), propertyKeyId));
        //
        //====================================================================================================
        /// <summary>
        /// get a property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="keyId"></param>
        /// <returns></returns>
        public bool getBoolean(string propertyName, bool defaultValue) => encodeBoolean(getText(propertyName, encodeText(defaultValue), propertyKeyId));
        //
        //====================================================================================================
        /// <summary>
        /// get an integer property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="keyId"></param>
        /// <returns></returns>
        public int getInteger(string propertyName)  => encodeInteger(getText(propertyName, encodeText(0), propertyKeyId));
        //
        //====================================================================================================
        /// <summary>
        /// get an integer property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="keyId"></param>
        /// <returns></returns>
        public int getInteger(string propertyName, int defaultValue) => encodeInteger(getText(propertyName, encodeText(defaultValue), propertyKeyId));
        //
        //====================================================================================================
        /// <summary>
        /// get a string property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="keyId"></param>
        /// <returns></returns>
        public string getText(string propertyName) => getText(propertyName, "", propertyKeyId);
        //
        //====================================================================================================
        /// <summary>
        /// get a string property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="keyId"></param>
        /// <returns></returns>
        public string getText(string propertyName, string defaultValue) => getText(propertyName, defaultValue, propertyKeyId );
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
                //
                if (!propertyCacheLoaded) { loadFromDb(keyId); }
                //
                int Ptr = -1;
                bool Found = false;
                if (propertyCacheCnt > 0) {
                    Ptr = propertyCache_nameIndex.getPtr(propertyName);
                    if (Ptr >= 0) {
                        returnString = encodeText(propertyCache[1, Ptr]);
                        Found = true;
                    }
                }
                //
                if (!Found) {
                    returnString = defaultValue;
                    setProperty(propertyName, defaultValue, keyId);
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
                propertyCache_nameIndex = new KeyPtrController();
                propertyCacheCnt = 0;
                //
                using (DataTable dt = core.db.executeQuery("select Name,FieldValue,ID from ccProperties where (active<>0)and(TypeID=" + (int)propertyType + ")and(KeyID=" + keyId + ")")) {
                    if (dt.Rows.Count > 0) {
                        propertyCache = new string[3, dt.Rows.Count];
                        foreach (DataRow dr in dt.Rows) {
                            string Name = GenericController.encodeText(dr[0]);
                            propertyCache[0, propertyCacheCnt] = Name;
                            propertyCache[1, propertyCacheCnt] = GenericController.encodeText(dr[1]);
                            propertyCache[2, propertyCacheCnt] = GenericController.encodeInteger(dr[2]).ToString();
                            propertyCache_nameIndex.setPtr(Name.ToLowerInvariant(), propertyCacheCnt);
                            propertyCacheCnt += 1;
                        }
                        propertyCacheCnt = dt.Rows.Count;
                    }
                }
                propertyCacheLoaded = true;
            } catch( Exception ex ) {
                LogController.handleError( core,ex);
            }
        }
    }
}
