
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Contensive.Processor.Models.Domain;
using static Contensive.Processor.Tests.testConstants;

namespace Contensive.Processor.Tests.UnitTests.Models {
    [TestClass()]
    public class routeMapModelTests {
        [TestMethod]
        public void Models_RouteMap_DictionaryHasAdmin() {
            using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(testAppName)) {
                // arrange
                // act
                var routes = RouteMapModel.create(cp.core);
                // assert only one route, matching the default admin route
                Assert.IsTrue(routes.routeDictionary.ContainsKey(cp.core.appConfig.adminRoute));
            }
        }
    }
}