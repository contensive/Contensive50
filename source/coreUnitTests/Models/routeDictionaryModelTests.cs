using Microsoft.VisualStudio.TestTools.UnitTesting;
using Contensive.Core.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contensive.Core.Controllers.Tests {
    [TestClass()]
    public class routeDictionaryModelTests {
        [TestMethod()]
        public void Models_RouteDictionary_NoRoutes() {
            using (Contensive.Core.CPClass cp = new Contensive.Core.CPClass("testapp")) {
                // arrange
                cp.core.db.executeNonQuery("delete from " + Models.Entity.addonModel.contentTableName);
                cp.core.db.executeNonQuery("delete from " + Models.Entity.linkAliasModel.contentTableName);
                cp.core.db.executeNonQuery("delete from " + Models.Entity.linkForwardModel.contentTableName);
                // act
                var routes = Models.Complex.routeDictionaryModel.create(cp.core);
                // assert only one route, matching the default admin route
                Assert.AreEqual(1, routes.Count);
                Assert.AreEqual(genericController.normalizeRoute(cp.core.serverConfig.appConfig.adminRoute), routes.First().Key);
            }
        }

    }
}