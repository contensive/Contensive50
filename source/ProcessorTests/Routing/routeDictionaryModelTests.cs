

using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Tests.testConstants;
using Contensive.Processor;
using Contensive.Processor.Models.Domain;

namespace Contensive.ProcessorTests.UnitTests.ModelTests {
    [TestClass()]
    public class routeMapModelTests {
        [TestMethod]
        public void Models_RouteMap_DictionaryHasAdmin() {
            using (CPClass cp = new CPClass(testAppName)) {
                // arrange
                // act
                var routes = RouteMapModel.create(cp.core);
                // assert only one route, matching the default admin route
                Assert.IsTrue(routes.routeDictionary.ContainsKey(cp.core.appConfig.adminRoute));
            }
        }

    }
}