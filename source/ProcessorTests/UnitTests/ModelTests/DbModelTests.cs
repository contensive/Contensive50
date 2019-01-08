
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Contensive.Processor.Tests.testConstants;
using Contensive.Processor.Models.Db;
using Contensive.BaseClasses;

namespace Contensive.Processor.Tests.UnitTests.Models {
    [TestClass()]
    public class DbModelTests {
        //
        [TestMethod]
        public void dbModel_Person_Add() {
            using (CPClass cp = new CPClass(testAppName)) {
                // arrange
                var person = Processor.Models.Db.DbModel.AddEmpty<Contensive.Processor.Models.Db.PersonModel>(cp.core);
                person.id = 10;
                Assert.AreEqual(10, person.id);
                person.save(cp.core);
                // act
                // assert
            }
        }
        //
        [TestMethod]
        public void dbModel_PageContent_Add() {
            using (CPClass cp = new CPClass(testAppName)) {
                // arrange
                var page = DbModel.AddEmpty<PageContentModel>(cp.core);
                Assert.AreNotEqual(0, page.id);
                //
                string htmlfileValue = cp.Utils.CreateGuid();
                int integerValue = cp.Utils.GetRandomInteger();
                DateTime dateValue = DateTime.Now.AddMinutes(cp.Utils.GetRandomInteger());
                bool boolValue = true;
                double numberValue = (double)(cp.Utils.GetRandomInteger()) / 100;
                // -- need a number field
                cp.Content.AddContentField(PageContentModel.contentName, "testNumberField", CPContentBaseClass.fileTypeIdEnum.Float);
                // 
                //
                Assert.AreNotEqual("", htmlfileValue);

                page.copyfilename.content = htmlfileValue;
                page.allowBrief = boolValue;
                page.clicks = integerValue;
                page.dateReviewed = dateValue;
                //
                page.save(cp.core);
                //
                // -- after save the filename should be set
                Assert.IsNotNull(page.copyfilename.filename);
                Assert.AreNotEqual("", page.copyfilename.filename);
                //
                // -- open a new model and the id should match
                var pageDup = DbModel.create<PageContentModel>(cp.core, page.id);
                Assert.AreEqual(pageDup.id, pageDup.id);
                //
                Assert.AreEqual(htmlfileValue, pageDup.copyfilename.content);
                Assert.AreEqual(page.copyfilename.content, pageDup.copyfilename.content);
                Assert.AreEqual(page.copyfilename.filename, pageDup.copyfilename.filename);
                Assert.AreEqual(".html", page.copyfilename.filename.Right(5));
                // act
                // assert
            }
        }
    }
}