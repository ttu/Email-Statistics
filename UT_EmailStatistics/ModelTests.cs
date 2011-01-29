using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EmailStatistics;

namespace UT_EmailStatistics
{
    /// <summary>
    /// Summary description for ModelTests
    /// </summary>
    [TestClass]
    public class ModelTests
    {
        IModel _model;

        bool _modelUpdated = false;

        public ModelTests()
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
        [TestInitialize()]
        public void MyTestInitialize()
        {
            _model = new Model();
        }

        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void Model_GetDateStatistics()
        {
            List<Mail> mailList = new List<Mail>();
            mailList.Add(new Mail() { From = "U1", Date = new DateTime(2000, 1, 1, 0, 1, 1) });
            mailList.Add(new Mail() { From = "U1", Date = new DateTime(2000, 1, 1, 3, 1, 1) });
            mailList.Add(new Mail() { From = "U3", Date = new DateTime(2000, 1, 1, 7, 1, 1) });
            mailList.Add(new Mail() { From = "U1", Date = new DateTime(2000, 1, 1, 19, 1, 1) });
            mailList.Add(new Mail() { From = "U3", Date = new DateTime(2000, 1, 1, 20, 1, 1) });
            mailList.Add(new Mail() { From = "U2", Date = new DateTime(2000, 1, 1, 23, 1, 1) });

            foreach (Mail m in mailList)
                _model.AddMail(m);

            Dictionary<StatType, int[]> retVal = _model.DateStats;

            int[] result = retVal[StatType.Hour1];

            Assert.AreEqual(result[0], 1);
            Assert.AreEqual(result[3], 1);
            Assert.AreEqual(result[7], 1);
            Assert.AreEqual(result[19], 1);
            Assert.AreEqual(result[20], 1);
            Assert.AreEqual(result[23], 1);
        }

        [TestMethod]
        public void Model_GetUserStatistics()
        {
            List<Mail> mailList = new List<Mail>();
            mailList.Add(new Mail() { From = "U1", Date = new DateTime(2000, 1, 1, 0, 1, 1) });
            mailList.Add(new Mail() { From = "U1", Date = new DateTime(2000, 1, 1, 3, 1, 1) });
            mailList.Add(new Mail() { From = "U3", Date = new DateTime(2000, 1, 1, 7, 1, 1) });
            mailList.Add(new Mail() { From = "U1", Date = new DateTime(2000, 1, 1, 19, 1, 1) });
            mailList.Add(new Mail() { From = "U3", Date = new DateTime(2000, 1, 1, 20, 1, 1) });
            mailList.Add(new Mail() { From = "U2", Date = new DateTime(2000, 1, 1, 23, 1, 1) });

            foreach (Mail m in mailList)
                _model.AddMail(m);

            Dictionary<string, int> retVal = _model.UserStats;

            Assert.AreEqual(retVal["U1"], 3);
            Assert.AreEqual(retVal["U2"], 1);
            Assert.AreEqual(retVal["U3"], 2);

        }

        [TestMethod]
        public void Model_ModelUpdated()
        {
            _model = new Model(3);
            _model.ModelUpdated += new EventHandler(_model_ModelUpdated);

            List<Mail> mailList = new List<Mail>();
            mailList.Add(new Mail() { From = "U1", Date = new DateTime(2000, 1, 1, 0, 1, 1) });
            mailList.Add(new Mail() { From = "U1", Date = new DateTime(2000, 1, 1, 3, 1, 1) });

            foreach (Mail m in mailList)
                _model.AddMail(m);

            Assert.IsFalse(_modelUpdated);

            Mail last = new Mail() { From = "U3", Date = new DateTime(2000, 1, 1, 7, 1, 1) };
            _model.AddMail(last);

            Assert.IsTrue(_modelUpdated);
        }

        void _model_ModelUpdated(object sender, EventArgs e)
        {
            _modelUpdated = true;
        }
    }
}
