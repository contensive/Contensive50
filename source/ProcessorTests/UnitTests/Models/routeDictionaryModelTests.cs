using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Contensive.Processor.Controllers;
using Contensive.Processor.Models;
using Contensive.Processor.Models.Domain;
using Contensive.Processor.Models.Db;
using static Contensive.Processor.Tests.testConstants;

namespace Contensive.Processor.Tests.UnitTests.Models {
    [TestClass()]
    public class routeMapModelTests {
        [TestMethod()]
        public void Models_RouteMap_DictionaryHasAdmin() {
            using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(testAppName)) {
                // arrange
                // act
                var routes = RouteMapModel.create(cp.core);
                // assert only one route, matching the default admin route
                //string routeList = "";
                Assert.IsTrue(routes.routeDictionary.ContainsKey(cp.core.appConfig.adminRoute));
                //foreach (KeyValuePair<string,RouteMapModel.routeClass> routeKvp in routes.routeDictionary ) {
                //    routeList += "," + routeKvp.Value.virtualRoute;
                //}
                //Assert.IsTrue(routes.routeDictionary.Count > 0, "should have been just admin, but was [" + routeList + "]");
                //Assert.AreEqual(GenericController.normalizeRoute(cp.core.appConfig.adminRoute), routes.routeDictionary.First().Key);
            }
        }

    }
}