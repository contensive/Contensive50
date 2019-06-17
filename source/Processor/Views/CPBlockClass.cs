
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
        private CPClass cp { get; set; }
        private string accum { get; set; }
        private Controllers.HtmlController htmlDoc { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// Constructor - Initialize the Main and Csv objects
        /// </summary>
        /// <param name="cp"></param>
        public CPBlockClass( CPClass cp) {
            try {
                accum = "";
                this.cp = cp;
                try {
                    htmlDoc = new Controllers.HtmlController(cp.core);
                } catch (Exception ex) {
                    LogController.logError( cp.core,ex, "Error creating object Controllers.htmlToolsController during cp.block constructor.");
                    throw;
                }
            } catch (Exception ex) {
                LogController.logError( cp.core,ex);
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
                LogController.logError( cp.core,ex);
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
                LogController.logError( cp.core,ex);
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
                LogController.logError( cp.core,ex);
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
        /// <summary>
        /// return the inner html of the element selected with findSelector (# for id, .for class)
        /// </summary>
        /// <param name="findSelector"></param>
        /// <returns></returns>
        public override string GetInner(string findSelector) {
            return HtmlParseStaticController.getInner(cp.core, accum, findSelector);
        }
        //
        //====================================================================================================
        /// <summary>
        /// return the outer html of the element selected with findSelector (# for id, .for class)
        /// </summary>
        /// <param name="findSelector"></param>
        /// <returns></returns>
        public override string GetOuter(string findSelector) {
            return HtmlParseStaticController.getOuter(cp.core, accum, findSelector);
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
                            foreach (string asset in stringSplit( headTags, Environment.NewLine )) {
                                cp.core.doc.htmlMetaContent_OtherTags.Add(new HtmlMetaClass() {
                                    addedByMessage = "block.importFile",
                                    content = asset
                                });
                            }
                        }
                        accum = HtmlParseStaticController.getTagInnerHTML(accum, "body", false);
                    }
                }
            } catch (Exception ex) {
                LogController.logError( cp.core,ex);
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
                    copy = CopyContentModel.create(cp.core, GenericController.encodeInteger(copyRecordNameOrGuid));
                } else if (GenericController.isGuid(copyRecordNameOrGuid)) {
                    //
                    // -- record guid
                    copy = CopyContentModel.create(cp.core, copyRecordNameOrGuid);
                } else {
                    //
                    // -- record name
                    copy = CopyContentModel.createByUniqueName(cp.core, copyRecordNameOrGuid);
                }
                if (copy != null ) {
                    accum = copy.copy;
                }
            } catch (Exception ex) {
                LogController.logError( cp.core,ex);
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
                LogController.logError( cp.core,ex);
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
                    layout = DbBaseModel.create<LayoutModel>(cp.core, GenericController.encodeInteger(layoutRecordNameOrGuid));
                } else if (GenericController.isGuid(layoutRecordNameOrGuid)) {
                    //
                    // -- record guid
                    layout = LayoutModel.create<LayoutModel>(cp.core, layoutRecordNameOrGuid);
                } else {
                    //
                    // -- record name
                    layout = LayoutModel.createByUniqueName<LayoutModel>(cp.core, layoutRecordNameOrGuid);
                }
                if (layout != null) {
                    accum = layout.layout.content;
                }
            } catch (Exception ex) {
                LogController.logError( cp.core,ex);
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
                LogController.logError( cp.core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override void SetInner(string findSelector, string htmlString) {
            try {
                accum = HtmlParseStaticController.setInner(cp.core, accum, findSelector, htmlString);
            } catch (Exception ex) {
                LogController.logError( cp.core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override void SetOuter(string findSelector, string htmlString) {
            try {
                accum = HtmlParseStaticController.setOuter(cp.core, accum, findSelector, htmlString);
            } catch (Exception ex) {
                LogController.logError( cp.core,ex);
                throw;
            }
        }
        //
        // Dispose Support
        //
        #region  IDisposable Support 
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
                }
                //
                // Add code here to release the unmanaged resource.
                //
            }
            this.disposed = true;
        }
        //
        protected bool disposed { get; set; } = false;
        //
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        public override void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        //
        ~CPBlockClass() {
            Dispose(false);
        }
        #endregion
        //
    }
}