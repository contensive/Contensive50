using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TestApps.Controllers {
    public class HomeController : Controller {
        public ActionResult Index() {
            using (var cp = new Contensive.Processor.CPClass("app20200120100129")) {
                ViewBag.Title = "Home Page - " + cp.Site.Name;
                cp.CdnFiles.Save("testFilename.txt", "This is the file content");
                string testContent = cp.CdnFiles.Read("testFilename.txt");
                return View();
            }
        }
    }
}
