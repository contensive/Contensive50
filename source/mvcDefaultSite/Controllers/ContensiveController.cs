using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;

namespace mvcDefaultSite.Controllers {
    public class ContensiveController : Controller {
        // GET: Contensive
        public ActionResult Index() {
            using (var cp = new Contensive.Processor.CPClass("mvc20190803v51", System.Web.HttpContext.Current)) {
                return Content("Hello world!, cp.version " + cp.Version);
            }
        }
    }
}
