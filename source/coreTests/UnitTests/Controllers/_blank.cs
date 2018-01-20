
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Contensive.Core.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contensive.Core.Tests.UnitTests.Controllers {
    [TestClass()]
    public class _blank {
        [TestMethod()]
        public void Controllers_blank() {
            using (Contensive.Core.CPClass cp = new Contensive.Core.CPClass("testapp")) {
                // arrange
                // act
                // assert
                Assert.AreEqual("", "");
            }
        }

    }
}