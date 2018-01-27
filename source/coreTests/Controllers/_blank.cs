﻿
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Contensive.Core.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Contensive.Core.Tests.testConstants;

namespace Contensive.Core.Tests.Controllers {
    [TestClass()]
    public class _blank {
        [TestMethod()]
        public void Controllers_blank() {
            using (Contensive.Core.CPClass cp = new Contensive.Core.CPClass(testAppName)) {
                // arrange
                // act
                // assert
                Assert.AreEqual("", "");
            }
        }

    }
}