
using System;
using Contensive.Processor.Controllers;
using Contensive.Models.Db;
//
namespace Contensive.Processor.Addons.PageManager {
    public class getChildPageList : Contensive.BaseClasses.AddonBaseClass {
        //
        //====================================================================================================
        /// <summary>
        /// pageManager addon interface
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public override object Execute(Contensive.BaseClasses.CPBaseClass cp) {
            string result = "";
            try {
                CoreController core = ((CPClass)cp).core;
                result = PageContentController.getChildPageList(core, cp.Doc.GetText("List Name"), PageContentModel.tableMetadata.contentName, core.doc.pageController.page.id, true);
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
