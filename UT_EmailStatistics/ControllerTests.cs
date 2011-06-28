using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EmailStatistics;
using Rhino.Mocks;
using System.Reflection;
using System.Threading;

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

        string _host = "imap.gmail.com";
        int _port = 993;
        string _username = "";
        string _password = "";
        bool _useSSL = true;

        string _subject = "";
        bool _getInbox = true;
        bool _getSent = true;

        [TestMethod]
        [Description("This can be used to test functionality without UI")]
        public void Controller_FullTest()
        {
            var mockView = MockRepository.GenerateMock<IView>();

            EmailObjectFactory factory = new EmailObjectFactory();
            Model m = new Model(int.MaxValue, "");
            Controller c = new Controller(m, mockView, factory);
            c.SetConfig(ConnectionType.IMAP, _host, _port, _useSSL, _username, _password);
            IEmailWorker worker = getPriveteField<IEmailWorker>(c, "_emailWorker");
      
            c.EnableConnection();

            while (worker.IsBusy())
                Thread.Sleep(1000);

            c.GetData(_subject, _getInbox, _getSent);

            while (worker.IsBusy())
                Thread.Sleep(1000);

            string test = m.GetUserStats;
            string test2 = m.GetUserStatsNoText;
            string test3 = m.PrintDateStats;
        }

        private T getPriveteField<T>(object sender, string fieldName)
        {
            Type t = sender.GetType();

            FieldInfo field = t.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

            object retVal = field.GetValue(sender);

            return (T)retVal;
        }
    }
}
