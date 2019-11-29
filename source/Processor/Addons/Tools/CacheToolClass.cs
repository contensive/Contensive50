
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using Contensive.Processor;

using Contensive.Processor.Models.Domain;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.Processor.Addons.AdminSite.Controllers;
using Contensive.Models;
using Contensive.Models.Db;
//
namespace Contensive.Processor.Addons.Tools {
    //
    public class CacheToolClass : Contensive.BaseClasses.AddonBaseClass {
        //
        private static readonly string ButtonCacheGet = "Get Cache";
        private static readonly string ButtonCacheStore = "Store Cache";
        private static readonly string ButtonCacheInvalidate = "Invalidate Cache";
        private static readonly string ButtonCacheInvalidateAll = "Invalidate All Cache";
        //
        //====================================================================================================
        /// <summary>
        /// addon method, deliver complete Html admin site
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public override object Execute(Contensive.BaseClasses.CPBaseClass cpBase) {
            return getForm_CacheTool((CPClass)cpBase);
        }
        //
        //=============================================================================
        //   Print the manual query form
        //=============================================================================
        //
        public static string getForm_CacheTool(CPClass cp) {
            string returnHtml = "";
            CoreController core = cp.core;
            try {
                StringBuilderLegacyController Stream = new StringBuilderLegacyController();
                Stream.Add(AdminUIController.getHeaderTitleDescription("Cache Tool", "Use this tool to get/store/invalidate the application's cache."));
                //
                string cacheKey = core.docProperties.getText("cacheKey");
                string cacheValue = core.docProperties.getText("cacheValue");
                string button = core.docProperties.getText("button");
                //
                if (button == ButtonCacheGet) {
                    //
                    // -- Get Cache
                    Stream.Add("<div>" + DateTime.Now + " cache.getObject(" + cacheKey + ")</div>");
                    object resultObj = cp.Cache.GetObject(cacheKey);
                    if (resultObj == null) {
                        Stream.Add("<div>" + DateTime.Now + " NULL returned</div>");
                    } else {
                        try {
                            cacheValue = Newtonsoft.Json.JsonConvert.SerializeObject(resultObj);
                            Stream.Add("<div>" + DateTime.Now + "CacheValue object returned, json serialized, length [" + cacheValue.Length + "]</div>");
                        } catch (Exception ex) {
                            Stream.Add("<div>" + DateTime.Now + " exception during serialization, ex [" + ex + "]</div>");
                        }
                    }
                    Stream.Add("<p>" + DateTime.Now + " Done</p>");
                } else if (button == ButtonCacheStore) {
                    //
                    // -- Store Cache
                    Stream.Add("<div>" + DateTime.Now + " cache.store(" + cacheKey + "," + cacheValue + ")</div>");
                    cp.Cache.Store(cacheKey, cacheValue);
                    Stream.Add("<p>" + DateTime.Now + " Done</p>");
                } else if (button == ButtonCacheInvalidate) {
                    //
                    // -- Invalidate
                    cacheValue = "";
                    Stream.Add("<div>" + DateTime.Now + " cache.Invalidate(" + cacheKey + ")</div>");
                    cp.Cache.Invalidate(cacheKey);
                    Stream.Add("<p>" + DateTime.Now + " Done</p>");
                } else if (button == ButtonCacheInvalidateAll) {
                    //
                    // -- Store Cache
                    cacheValue = "";
                    Stream.Add("<div>" + DateTime.Now + " cache.InvalidateAll()</div>");
                    cp.Cache.InvalidateAll();
                    Stream.Add("<p>" + DateTime.Now + " Done</p>");
                }
                //
                // Display form
                {
                    //
                    // -- cache key
                    Stream.Add(cp.Html5.H4("Cache Key"));
                    Stream.Add(cp.Html5.Div(AdminUIController.getDefaultEditor_text(core, "cacheKey", cacheKey, false, "cacheKey")));
                }
                {
                    //
                    // -- cache value
                    Stream.Add(cp.Html5.H4("Cache Value"));
                    Stream.Add(cp.Html5.Div(AdminUIController.getDefaultEditor_TextArea(core, "cacheValue", cacheValue, false, "cacheValue")));
                }
                //
                // -- assemble form
                returnHtml = AdminUIController.getToolForm(core, Stream.Text, ButtonCancel + "," + ButtonCacheGet + "," + ButtonCacheStore + "," + ButtonCacheInvalidate + "," + ButtonCacheInvalidateAll);
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
            return returnHtml;
        }
    }
}

