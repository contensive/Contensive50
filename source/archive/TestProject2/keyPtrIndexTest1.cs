using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Contensive.Core;

namespace unitTests
{
    [TestClass]
    public class ipAddressList
    {
        [TestMethod]
        public void common_getIpAddressList()
        {
            //cpCommonUtilsClass utils = new cpCommonUtilsClass();
            string list = ccCommonModule.getIpAddressList();
            Assert.AreNotEqual("", list);
        }
    }

    [TestClass]
    public class keyPtrIndex
    {
        [TestMethod]
        public void keyPtrIndex_addAndSort()
        {
            try
            {
                // 
                // setup test, setPointer
                //

                keyPtrIndexClass index = new keyPtrIndexClass();
                index.setPtr("9999", 9);
                index.setPtr("2222", 2);
                index.setPtr("8888", 8);
                index.setPtr("3333", 3);
                index.setPtr("7777", 7);
                index.setPtr("4444", 4);
                index.setPtr("6666", 6);
                index.setPtr("5555", 5);
                //
                // simple getPtr
                //
                Assert.AreEqual(7, index.getPtr("7777"));
                //
                // getFirstPointer, getNextPtr
                //
                Assert.AreEqual(index.getFirstPtr(), 2);
                Assert.AreEqual(index.getNextPtr(), 3);
                Assert.AreEqual(index.getNextPtr(), 4);
                Assert.AreEqual(index.getNextPtr(), 5);
                Assert.AreEqual(index.getNextPtr(), 6);
                Assert.AreEqual(index.getNextPtr(), 7);
                Assert.AreEqual(index.getNextPtr(), 8);
                Assert.AreEqual(index.getNextPtr(), 9);
                Assert.AreEqual(index.getNextPtr(), -1);
                //
                // exportPropertyBag, importPropertyBag
                //
                string bag = index.exportPropertyBag();
                keyPtrIndexClass index2 = new keyPtrIndexClass();
                index2.importPropertyBag(bag);
                Assert.AreEqual(7, index2.getPtr("7777"));
                //
            }
            catch (Exception ex)
            {
                Assert.Fail();
                string  throwaway = ex.ToString();

            }




        }
    }
}
