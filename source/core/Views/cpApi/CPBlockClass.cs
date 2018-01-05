
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
namespace Contensive.Core {
    public class CPBlockClass : BaseClasses.CPBlockBaseClass, IDisposable {
        //
        #region COM GUIDs
        public const string ClassId = "9E4DF603-A94B-4E3A-BD06-E19BB9CB1B5F";
        public const string InterfaceId = "E4D5D9F0-DF96-492E-9CAC-1107F0187A40";
        public const string EventsId = "5911548D-7637-4021-BD08-C7676F3E12C6";
        #endregion
        //
        private Contensive.Core.coreClass cpCore { get; set; }
        private CPClass cp { get; set; }
        private string accum { get; set; }
        private Controllers.htmlController htmlDoc { get; set; }
        protected bool disposed { get; set; } = false;
        //
        //====================================================================================================
        /// <summary>
        /// Constructor - Initialize the Main and Csv objects
        /// </summary>
        /// <param name="cpParent"></param>
        public CPBlockClass( CPClass cpParent) : base() {
            try {
                accum = "";
                cp = cpParent;
                cpCore = cp.core;
                try {
                    htmlDoc = new Controllers.htmlController(cpCore);
                } catch (Exception ex) {
                    cpCore.handleException(ex, "Error creating object Controllers.htmlToolsController during cp.block constructor.");
                    throw;
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override void Load(string htmlString) {
            try {
                accum = htmlString;
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override void Append(string htmlString) {
            try {
                accum += htmlString;
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override void Clear() {
            try {
                accum = "";
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override string GetHtml() {
            return accum;
        }
        //
        //====================================================================================================
        //
        public override string GetInner(string findSelector) {
            string s = "";
            try {
                string a = accum;
                if (!string.IsNullOrEmpty(findSelector)) {
                    s = htmlParseStaticController.getInner(cpCore, a, findSelector);
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return s;
        }
        //
        //====================================================================================================
        //
        public override string GetOuter(string findSelector) {
            string s = "";
            try {
                string a = accum;
                if (!string.IsNullOrEmpty(findSelector)) {
                    s = htmlParseStaticController.getOuter(cpCore, a, findSelector);
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return s;
        }
        //
        //====================================================================================================
        //
        public override void ImportFile(string wwwFileName) {
            string headTags = "";
            try {
                if (!string.IsNullOrEmpty(wwwFileName)) {
                    accum = cp.wwwFiles.read(wwwFileName);
                    if (!string.IsNullOrEmpty(accum)) {
                        headTags = htmlParseStaticController.getTagInnerHTML(accum, "head", false);
                        if (!string.IsNullOrEmpty(headTags)) {
                            foreach (string asset in stringSplit( headTags, "\r\n" )) {
                                cpCore.doc.htmlMetaContent_OtherTags.Add(new htmlMetaClass() {
                                    addedByMessage = "block.importFile",
                                    content = asset
                                });
                            }
                        }
                        accum = htmlParseStaticController.getTagInnerHTML(accum, "body", false);
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override void OpenCopy(string copyRecordNameOrGuid) {
            try {
                accum = "";
                copyContentModel copy;
                if (copyRecordNameOrGuid.IsNumeric()) {
                    //
                    // -- recordId
                    copy = copyContentModel.create(cpCore, genericController.encodeInteger(copyRecordNameOrGuid));
                } else if (genericController.isGuid(copyRecordNameOrGuid)) {
                    //
                    // -- record guid
                    copy = copyContentModel.create(cpCore, copyRecordNameOrGuid);
                } else {
                    //
                    // -- record name
                    copy = copyContentModel.createByName(cpCore, copyRecordNameOrGuid);
                }
                if (copy != null ) {
                    accum = copy.copy;
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override void OpenFile(string wwwFileName) {
            try {
                accum = "";
                if (!string.IsNullOrEmpty(wwwFileName)) {
                    accum = cp.wwwFiles.read(wwwFileName);
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override void OpenLayout(string layoutRecordNameOrGuid) {
            try {
                accum = "";
                layoutModel copy;
                if (layoutRecordNameOrGuid.IsNumeric()) {
                    //
                    // -- recordId
                    copy = layoutModel.create(cpCore, genericController.encodeInteger(layoutRecordNameOrGuid));
                } else if (genericController.isGuid(layoutRecordNameOrGuid)) {
                    //
                    // -- record guid
                    copy = layoutModel.create(cpCore, layoutRecordNameOrGuid);
                } else {
                    //
                    // -- record name
                    copy = layoutModel.createByName(cpCore, layoutRecordNameOrGuid);
                }
                if (copy != null) {
                    accum = copy.Layout;
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override void Prepend(string htmlString) {
            try {
                accum = htmlString + accum;
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override void SetInner(string findSelector, string htmlString) {
            try {
                accum = htmlParseStaticController.setInner(cpCore, accum, findSelector, htmlString);
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override void SetOuter(string findSelector, string htmlString) {
            try {
                accum = htmlParseStaticController.setOuter(cpCore, accum, findSelector, htmlString);
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //====================================================================================================
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                if (disposing) {
                    //
                    // dispose managed objects, dereference local object pointers 
                    //
                    htmlDoc = null;
                    cp = null;
                    cpCore = null;
                }
                //
                // Add code here to release the unmanaged resource.
                //
            }
            this.disposed = true;
        }
        //
        // Dispose Support
        //
        #region  IDisposable Support 
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~CPBlockClass() {
            Dispose(false);
            //todo  NOTE: The base class Finalize method is automatically called from the destructor:
            //base.Finalize();
        }
        #endregion
        //
    }
}