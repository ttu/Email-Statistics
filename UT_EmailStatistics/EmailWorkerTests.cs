using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using EmailStatistics;

namespace UT_EmailStatistics
{
    /// <summary>
    /// Summary description for GmailWorkerTests
    /// </summary>
    [TestClass]
    public class EmailWorkerTests
    {
        IEmailWorker _worker;

        public EmailWorkerTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void Worker_Test()
        {
            var mockHelper = MockRepository.GenerateMock<IEmaillHelper>();

            List<Mail> mails = new List<Mail>();
            mails.Add(new Mail() { From = "tomi", Subject = "test" });
            mails.Add(new Mail() { From = "james", Subject = "test2" });

            mockHelper.Expect(hel => hel.GetMails("test", true, true)).Return(mails);

            _worker = new EmailWorker(mockHelper);

            _worker.EnableConnection();
        }
    }
}
