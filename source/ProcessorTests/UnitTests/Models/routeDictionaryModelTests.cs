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
    public class routeDictionaryModelTests {
        [TestMethod()]
        public void Models_RouteDictionary_NoRoutes() {
            using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(testAppName)) {
                // arrange
                cp.core.db.executeNonQuery("delete from " + AddonModel.contentTableName);
                cp.core.db.executeNonQuery("delete from " + LinkAliasModel.contentTableName);
                cp.core.db.executeNonQuery("delete from " + LinkForwardModel.contentTableName);
                // act
                var routes = RouteMapModel.create(cp.core);
                // assert only one route, matching the default admin route
                Assert.AreEqual(1, routes.routeDictionary.Count);
                Assert.AreEqual(GenericController.normalizeRoute(cp.core.appConfig.adminRoute), routes.routeDictionary.First().Key);
            }
        }

    }
}