using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Contensive.Processor.Controllers;
using Contensive.Processor.Models;
using Contensive.Processor.Models.Complex;
using Contensive.Processor.Models.DbModels;
using static Contensive.Processor.Tests.testConstants;

namespace Contensive.Processor.Tests.UnitTests.Models {
    [TestClass()]
    public class routeDictionaryModelTests {
        [TestMethod()]
        public void Models_RouteDictionary_NoRoutes() {
            using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(testAppName)) {
                // arrange
                cp.core.db.executeNonQuery("delete from " + AddonModel.contentTableName);
                cp.core.db.executeNonQuery("delete from " + linkAliasModel.contentTableName);
                cp.core.db.executeNonQuery("delete from " + linkForwardModel.contentTableName);
                // act
                var routes = routeDictionaryModel.create(cp.core);
                // assert only one route, matching the default admin route
                Assert.AreEqual(1, routes.Count);
                Assert.AreEqual(genericController.normalizeRoute(cp.core.appConfig.adminRoute), routes.First().Key);
            }
        }

    }
}