using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Contensive.Core.Controllers;
using Contensive.Core.Models;
using Contensive.Core.Models.Complex;
using Contensive.Core.Models.DbModels;
using static Contensive.Core.Tests.testConstants;

namespace Contensive.Core.Tests.UnitTests.Models {
    [TestClass()]
    public class routeDictionaryModelTests {
        [TestMethod()]
        public void Models_RouteDictionary_NoRoutes() {
            using (Contensive.Core.CPClass cp = new Contensive.Core.CPClass(testAppName)) {
                // arrange
                cp.core.db.executeNonQuery("delete from " + addonModel.contentTableName);
                cp.core.db.executeNonQuery("delete from " + linkAliasModel.contentTableName);
                cp.core.db.executeNonQuery("delete from " + linkForwardModel.contentTableName);
                // act
                var routes = routeDictionaryModel.create(cp.core);
                // assert only one route, matching the default admin route
                Assert.AreEqual(1, routes.Count);
                Assert.AreEqual(genericController.normalizeRoute(cp.core.serverConfig.appConfig.adminRoute), routes.First().Key);
            }
        }

    }
}