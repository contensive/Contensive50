using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace mvcDefaultSite.Controllers {
    public class HomeController : Controller {
        public ActionResult Index() {
            using(var cp = new Contensive.Processor.CPClass("mvc20190803v51", System.Web.HttpContext.Current)) {
                cp.Utils.AppendLog("Index action in home controllers");
            }
            return View();
        }

        public ActionResult About() {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact() {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}