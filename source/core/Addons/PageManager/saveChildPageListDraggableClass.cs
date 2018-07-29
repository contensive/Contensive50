
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
namespace Contensive.Addons.PageManager {
    public class saveChildPageListDraggableClass : Contensive.BaseClasses.AddonBaseClass {
        //
        //====================================================================================================
        /// <summary>
        /// pageManager addon interface. decode: "sortlist=childPageList_{parentId}_{listName},page{idOfChild},page{idOfChild},etc"
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public override object Execute(Contensive.BaseClasses.CPBaseClass cp) {
            string returnHtml = "";
            try {
                CoreController core = ((CPClass)cp).core;
                string pageCommaList = cp.Doc.GetText("sortlist");
                List<string> pageList = new List<string>(pageCommaList.Split(','));
                if (pageList.Count > 1) {
                    string[] ParentPageValues = pageList[0].Split('_');
                    if (ParentPageValues.Count() < 3) {
                        //
                        // -- parent page is not valid
                        cp.Site.ErrorReport(new ArgumentException("pageResort requires first value to identify the parent page"));
                    } else {
                        int parentPageId = encodeInteger(ParentPageValues[1]);
                        if (parentPageId == 0) {
                            //
                            // -- parent page is not valid
                            cp.Site.ErrorReport(new ArgumentException("pageResort requires a parent page id"));
                        } else {
                            //
                            // -- create childPageIdList
                            //Dim childListName As String = ParentPageValues(2)
                            List<int> childPageIdList = new List<int>();
                            foreach (string PageIDText in pageList) {
                                int pageId = encodeInteger(PageIDText.Replace("page", ""));
                                if (pageId > 0) {
                                    childPageIdList.Add(pageId);
                                }
                            }
                            //
                           pageContentModel parentPage = pageContentModel.create(core, parentPageId );
                            if (parentPage == null) {
                                //
                                // -- parent page is not valid
                                cp.Site.ErrorReport(new ArgumentException("pageResort requires a parent page id"));
                            } else {
                                //
                                // -- verify page set to required sort method Id
                                sortMethodModel sortMethod = sortMethodModel.createByName(core, "By Alpha Sort Order Field");
                                if (sortMethod == null) {
                                    sortMethod = sortMethodModel.createByName(core, "Alpha Sort Order Field");
                                }
                                if (sortMethod == null) {
                                    //
                                    // -- create the required sortMethod
                                    sortMethod = sortMethodModel.add(core);
                                    sortMethod.name = "By Alpha Sort Order Field";
                                    sortMethod.OrderByClause = "sortOrder";
                                    sortMethod.save(core);
                                }
                                if (parentPage.ChildListSortMethodID != sortMethod.id) {
                                    //
                                    // -- update page if not set correctly
                                    parentPage.ChildListSortMethodID = sortMethod.id;
                                    parentPage.save(core);
                                }
                                int pagePtr = 0;
                                foreach (var childPageId in childPageIdList) {
                                    if (childPageId == 0) {
                                        //
                                        // -- invalid child page
                                        cp.Site.ErrorReport(new ApplicationException("child page id is invalid from remote request [" + pageCommaList + "]"));
                                    } else {
                                        string SortOrder = (100000 + (pagePtr * 10)).ToString();
                                       pageContentModel childPage = pageContentModel.create(core, childPageId);
                                        if (childPage.sortOrder != SortOrder) {
                                            childPage.sortOrder = SortOrder;
                                            childPage.save(core);
                                        }
                                    }
                                    pagePtr += 1;
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return returnHtml;
        }
    }
}
