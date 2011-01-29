using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EmailStatistics;
using Rhino.Mocks;

namespace UT_EmailStatistics
{
    /// <summary>
    /// Summary description for ControllerTests
    /// </summary>
    [TestClass]
    public class ControllerTests
    {
        IController _controller;

        public ControllerTests()
        {
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
        //[TestInitialize()]
        //public void MyTestInitialize() {}
        //
        // Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void Controller_Test()
        {
            // Actually nothing to test in here..

            var mockModel = MockRepository.GenerateMock<IModel>();
            var mockView = MockRepository.GenerateMock<IView>();
            var mockFactory = MockRepository.GenerateMock<IEmailObjectFactory>();
            var mockHelper = MockRepository.GenerateMock<IEmaillHelper>();
            var mockWorker = MockRepository.GenerateMock<IEmailWorker>();

            mockFactory.Expect(f => f.GetInterIMAPHelper()).Return(mockHelper);
            mockFactory.Expect(f => f.GetWorker(ConnectionType.IMAP)).Return(mockWorker);

            _controller = new Controller(mockModel, mockView, mockFactory);
            _controller.EnableConnection();           
        }
    }
}
