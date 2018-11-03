
using System;
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;

namespace Contensive.Processor {
    public class CPBlockClass : BaseClasses.CPBlockBaseClass, IDisposable {
        //
        #region COM GUIDs
        public const string ClassId = "9E4DF603-A94B-4E3A-BD06-E19BB9CB1B5F";
        public const string InterfaceId = "E4D5D9F0-DF96-492E-9CAC-1107F0187A40";
        public const string EventsId = "5911548D-7637-4021-BD08-C7676F3E12C6";
        #endregion
        //
        private CoreController core { get; set; }
        private CPClass cp { get; set; }
        private string accum { get; set; }
        private Controllers.HtmlController htmlDoc { get; set; }
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
                core = cp.core;
                try {
                    htmlDoc = new Controllers.HtmlController(core);
                } catch (Exception ex) {
                    LogController.handleError( core,ex, "Error creating object Controllers.htmlToolsController during cp.block constructor.");
                    throw;
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
                LogController.handleError( core,ex);
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
                LogController.handleError( core,ex);
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
                LogController.handleError( core,ex);
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
                    s = HtmlParseStaticController.getInner(core, a, findSelector);
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
                    s = HtmlParseStaticController.getOuter(core, a, findSelector);
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
                    accum = cp.WwwFiles.Read(wwwFileName);
                    if (!string.IsNullOrEmpty(accum)) {
                        headTags = HtmlParseStaticController.getTagInnerHTML(accum, "head", false);
                        if (!string.IsNullOrEmpty(headTags)) {
                            foreach (string asset in stringSplit( headTags, "\r\n" )) {
                                core.doc.htmlMetaContent_OtherTags.Add(new HtmlMetaClass() {
                                    addedByMessage = "block.importFile",
                                    content = asset
                                });
                            }
                        }
                        accum = HtmlParseStaticController.getTagInnerHTML(accum, "body", false);
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override void OpenCopy(string copyRecordNameOrGuid) {
            try {
                accum = "";
                CopyContentModel copy;
                if (copyRecordNameOrGuid.IsNumeric()) {
                    //
                    // -- recordId
                    copy = CopyContentModel.create(core, GenericController.encodeInteger(copyRecordNameOrGuid));
                } else if (GenericController.isGuid(copyRecordNameOrGuid)) {
                    //
                    // -- record guid
                    copy = CopyContentModel.create(core, copyRecordNameOrGuid);
                } else {
                    //
                    // -- record name
                    copy = CopyContentModel.createByUniqueName(core, copyRecordNameOrGuid);
                }
                if (copy != null ) {
                    accum = copy.copy;
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
                    accum = cp.WwwFiles.Read(wwwFileName);
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override void OpenLayout(string layoutRecordNameOrGuid) {
            try {
                accum = "";
                LayoutModel layout;
                if (layoutRecordNameOrGuid.IsNumeric()) {
                    //
                    // -- recordId
                    layout = LayoutModel.create<LayoutModel>(core, GenericController.encodeInteger(layoutRecordNameOrGuid));
                } else if (GenericController.isGuid(layoutRecordNameOrGuid)) {
                    //
                    // -- record guid
                    layout = LayoutModel.create<LayoutModel>(core, layoutRecordNameOrGuid);
                } else {
                    //
                    // -- record name
                    layout = LayoutModel.createByUniqueName<LayoutModel>(core, layoutRecordNameOrGuid);
                }
                if (layout != null) {
                    accum = layout.layout.content;
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override void SetInner(string findSelector, string htmlString) {
            try {
                accum = HtmlParseStaticController.setInner(core, accum, findSelector, htmlString);
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override void SetOuter(string findSelector, string htmlString) {
            try {
                accum = HtmlParseStaticController.setOuter(core, accum, findSelector, htmlString);
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
                    core = null;
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
            
            
        }
        #endregion
        //
    }
}