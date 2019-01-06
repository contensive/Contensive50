
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Contensive.Processor.Tests.testConstants;

namespace Contensive.Processor.Tests.UnitTests.Models {
    [TestClass()]
    public class DbModelTests {
        [TestMethod]
        public void dbModel_Add() {
            using (CPClass cp = new CPClass(testAppName)) {
                // arrange
                var person = cp.DbModelNew().AddEmpty<PersonModel>(cp);
                person.id = 10;
                Assert.AreEqual(10, person.id);
                person.Save(cp);
                // act
                // assert
            }
        }

    }
    //
    // todo - I need to expose all the actual classes that represent the tables within core, not make copies.
    //
    public class PersonModel : CPDbModelClass {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "people";
        public const string contentTableName = "ccmembers";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
        //
        //====================================================================================================
        // -- instance properties
        public string address { get; set; }
        public string address2 { get; set; }
        public bool admin { get; set; }
        public bool allowBulkEmail { get; set; }
        public bool allowToolsPanel { get; set; }
        public bool autoLogin { get; set; }
        public string billAddress { get; set; }
        public string billAddress2 { get; set; }
        public string billCity { get; set; }
        public string billCompany { get; set; }
        public string billCountry { get; set; }
        public string billEmail { get; set; }
        public string billFax { get; set; }
        public string billName { get; set; }
        public string billPhone { get; set; }
        public string billState { get; set; }
        public string billZip { get; set; }
        public int birthdayDay { get; set; }
        public int birthdayMonth { get; set; }
        public int birthdayYear { get; set; }
        public string city { get; set; }
        public string company { get; set; }
        public string country { get; set; }
        public bool createdByVisit { get; set; }
        public DateTime dateExpires { get; set; }
        public bool developer { get; set; }
        public string email { get; set; }
        public bool excludeFromAnalytics { get; set; }
        public string fax { get; set; }
        public string firstName { get; set; }
        public string imageFilename { get; set; }
        public int languageID { get; set; }
        public string lastName { get; set; }
        public DateTime LastVisit { get; set; }
        public string nickName { get; set; }
        public string notesFilename { get; set; }
        public int organizationID { get; set; }
        public string password { get; set; }
        public string phone { get; set; }
        public string resumeFilename { get; set; }
        public string shipAddress { get; set; }
        public string shipAddress2 { get; set; }
        public string shipCity { get; set; }
        public string shipCompany { get; set; }
        public string shipCountry { get; set; }
        public string shipName { get; set; }
        public string shipPhone { get; set; }
        public string shipState { get; set; }
        public string shipZip { get; set; }
        public string state { get; set; }
        public string thumbnailFilename { get; set; }
        public string title { get; set; }
        public string username { get; set; }
        public int visits { get; set; }
        public string zip { get; set; }
    }
}