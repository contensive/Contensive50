
using System;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Constants;
using Contensive.Processor.Addons.AdminSite.Controllers;
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
                Stream.add(AdminUIController.getHeaderTitleDescription("Cache Tool", "Use this tool to get/store/invalidate the application's cache."));
                //
                string cacheKey = core.docProperties.getText("cacheKey");
                string cacheValue = core.docProperties.getText("cacheValue");
                string button = core.docProperties.getText("button");
                //
                if (button == ButtonCacheGet) {
                    //
                    // -- Get Cache
                    Stream.add("<div>" + DateTime.Now + " cache.getObject(" + cacheKey + ")</div>");
                    object resultObj = cp.Cache.GetObject(cacheKey);
                    if (resultObj == null) {
                        Stream.add("<div>" + DateTime.Now + " NULL returned</div>");
                    } else {
                        try {
                            cacheValue = Newtonsoft.Json.JsonConvert.SerializeObject(resultObj);
                            Stream.add("<div>" + DateTime.Now + "CacheValue object returned, json serialized, length [" + cacheValue.Length + "]</div>");
                        } catch (Exception ex) {
                            Stream.add("<div>" + DateTime.Now + " exception during serialization, ex [" + ex + "]</div>");
                        }
                    }
                    Stream.add("<p>" + DateTime.Now + " Done</p>");
                } else if (button == ButtonCacheStore) {
                    //
                    // -- Store Cache
                    Stream.add("<div>" + DateTime.Now + " cache.store(" + cacheKey + "," + cacheValue + ")</div>");
                    cp.Cache.Store(cacheKey, cacheValue);
                    Stream.add("<p>" + DateTime.Now + " Done</p>");
                } else if (button == ButtonCacheInvalidate) {
                    //
                    // -- Invalidate
                    cacheValue = "";
                    Stream.add("<div>" + DateTime.Now + " cache.Invalidate(" + cacheKey + ")</div>");
                    cp.Cache.Invalidate(cacheKey);
                    Stream.add("<p>" + DateTime.Now + " Done</p>");
                } else if (button == ButtonCacheInvalidateAll) {
                    //
                    // -- Store Cache
                    cacheValue = "";
                    Stream.add("<div>" + DateTime.Now + " cache.InvalidateAll()</div>");
                    cp.Cache.InvalidateAll();
                    Stream.add("<p>" + DateTime.Now + " Done</p>");
                }
                //
                // Display form
                {
                    //
                    // -- cache key
                    Stream.add(cp.Html5.H4("Cache Key"));
                    Stream.add(cp.Html5.Div(AdminUIController.getDefaultEditor_text(core, "cacheKey", cacheKey, false, "cacheKey")));
                }
                {
                    //
                    // -- cache value
                    Stream.add(cp.Html5.H4("Cache Value"));
                    Stream.add(cp.Html5.Div(AdminUIController.getDefaultEditor_TextArea(core, "cacheValue", cacheValue, false, "cacheValue")));
                }
                //
                // -- assemble form
                returnHtml = AdminUIController.getToolForm(core, Stream.text, ButtonCancel + "," + ButtonCacheGet + "," + ButtonCacheStore + "," + ButtonCacheInvalidate + "," + ButtonCacheInvalidateAll);
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
            return returnHtml;
        }
    }
}

