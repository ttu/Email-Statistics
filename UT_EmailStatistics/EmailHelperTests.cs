using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EmailStatistics;
using System.Collections;

namespace UT_EmailStatistics
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class EmailHelperTests
    {
        EmailHelper _helper;

        string _username = "";
        string _password = "";
        string _subject = "gmail";

        public EmailHelperTests()
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

        [TestInitialize()]
        public void MyTestInitialize()
        {
            //_helper = new InterIMAPGmailHelper();
            _helper = new LumiIMAPGmailHelper();
            _helper.SetConfig("imap.gmail.com", 993, true, _username, _password);
            _helper.EnableConnection();
        }


        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
            _helper.DisableConnection();
        }

        #endregion

        [TestMethod]
        public void Helper_TestConnection()
        {
            bool result = _helper.HasConnection();

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Helper_GetAllMessagesCount()
        {
            int result = _helper.GetMessageCountBySubject(string.Empty, false, true);
            int result2 = _helper.GetMessageCountBySubject(string.Empty, true, true);

            Assert.IsTrue(result2 > result);
        }

        [TestMethod]
        public void Helper_GetAllMessages()
        {
            List<Mail> messages = _helper.GetAllInboxMessages();

            Assert.IsNotNull(messages);
        }

        [TestMethod]
        public void Helper_GetMessagesBySubject()
        {
            List<Mail> messages = _helper.GetMessagesBySubject(_subject);

            Assert.IsNotNull(messages);
        }

        [TestMethod]
        public void Helper_GetMailsBySubjectIEnumerable()
        {
            foreach (Mail message in _helper.GetMails(_subject, true, true))
            {
                Assert.IsNotNull(message);
            }
        }

        [TestMethod]
        public void Helper_GetMailsBySubjectIEnumerableList()
        {
            foreach(List<Mail> messages in _helper.GetMails(_subject,true,true,3))
            {
                Assert.IsNotNull(messages);
            }         
        }

        [TestMethod]
        public void Helper_GetMessagesBySubjectCount()
        {
            int result = _helper.GetMessageCountBySubject(_subject, true, true);

            Assert.IsTrue(result > 0);
        }

        [TestMethod]
        public void Helper_GetDateStatistics_Hour1()
        {
            List<Mail> mailList = new List<Mail>();
            mailList.Add(new Mail() { Date = new DateTime(2000, 1, 1, 0, 1, 1) });
            mailList.Add(new Mail() { Date = new DateTime(2000, 1, 1, 3, 1, 1) });
            mailList.Add(new Mail() { Date = new DateTime(2000, 1, 1, 7, 1, 1) });
            mailList.Add(new Mail() { Date = new DateTime(2000, 1, 1, 19, 1, 1) });
            mailList.Add(new Mail() { Date = new DateTime(2000, 1, 1, 20, 1, 1) });
            mailList.Add(new Mail() { Date = new DateTime(2000, 1, 1, 23, 1, 1) });

            int[] result = _helper.GetDateStatistics<Mail>(StatType.Hour1, mailList, "Date");

            Assert.AreEqual(result[0], 1);
            Assert.AreEqual(result[3], 1);
            Assert.AreEqual(result[7], 1);
            Assert.AreEqual(result[19], 1);
            Assert.AreEqual(result[20], 1);
            Assert.AreEqual(result[23], 1);
        }

        [TestMethod]
        public void Helper_GetDateStatistics_Hour4()
        {
           List<Mail> mailList = new List<Mail>();
            mailList.Add(new Mail() { Date = new DateTime(2000, 1, 1, 0, 1, 1) });
            mailList.Add(new Mail() { Date = new DateTime(2000, 1, 1, 3, 1, 1) });
            mailList.Add(new Mail() { Date = new DateTime(2000, 1, 1, 7, 1, 1) });
            mailList.Add(new Mail() { Date = new DateTime(2000, 1, 1, 19, 1, 1) });
            mailList.Add(new Mail() { Date = new DateTime(2000, 1, 1, 20, 1, 1) });
            mailList.Add(new Mail() { Date = new DateTime(2000, 1, 1, 23, 1, 1) });

            int[] result = _helper.GetDateStatistics<Mail>(StatType.Hour4, mailList, "Date");

            Assert.AreEqual(result[0], 2);
            Assert.AreEqual(result[1], 1);
            Assert.AreEqual(result[2], 0);
            Assert.AreEqual(result[3], 0);
            Assert.AreEqual(result[4], 1);
            Assert.AreEqual(result[5], 2);
        }

        [TestMethod]
        public void Helper_GetDateStatistics_Day()
        {
            List<Mail> mailList = new List<Mail>();
            mailList.Add(new Mail() { Date = new DateTime(2011, 1, 3, 0, 1, 1) }); //mon
            mailList.Add(new Mail() { Date = new DateTime(2011, 1, 17, 3, 1, 1) }); //mon
            mailList.Add(new Mail() { Date = new DateTime(2011, 1, 12, 7, 1, 1) }); //wed
            mailList.Add(new Mail() { Date = new DateTime(2011, 1, 9, 19, 1, 1) }); //sun
            mailList.Add(new Mail() { Date = new DateTime(2011, 2, 20, 20, 1, 1) }); //sun
            mailList.Add(new Mail() { Date = new DateTime(2011, 2, 24, 23, 1, 1) }); //thu

            int[] result = _helper.GetDateStatistics<Mail>(StatType.Day, mailList, "Date");

            Assert.AreEqual(result[0], 2); // Sunday is first
            Assert.AreEqual(result[1], 2);
            Assert.AreEqual(result[2], 0);
            Assert.AreEqual(result[3], 1);
            Assert.AreEqual(result[4], 1);
            Assert.AreEqual(result[5], 0);
            Assert.AreEqual(result[6], 0);
        }

        [TestMethod]
        public void Helper_GetDateStatistics_Month()
        {
            List<Mail> mailList = new List<Mail>();
            mailList.Add(new Mail() { Date = new DateTime(2000, 1, 1, 0, 1, 1) });
            mailList.Add(new Mail() { Date = new DateTime(2000, 3, 1, 3, 1, 1) });
            mailList.Add(new Mail() { Date = new DateTime(2000, 12, 1, 7, 1, 1) });
            mailList.Add(new Mail() { Date = new DateTime(2000, 1, 1, 19, 1, 1) });
            mailList.Add(new Mail() { Date = new DateTime(2000, 11, 1, 20, 1, 1) });
            mailList.Add(new Mail() { Date = new DateTime(2000, 12, 1, 23, 1, 1) });
            mailList.Add(new Mail() { Date = new DateTime(2000, 1, 1, 20, 1, 1) });
            mailList.Add(new Mail() { Date = new DateTime(2000, 1, 1, 23, 1, 1) });
            mailList.Add(new Mail() { Date = new DateTime(2000, 4, 1, 20, 1, 1) });
            mailList.Add(new Mail() { Date = new DateTime(2000, 6, 1, 23, 1, 1) });

            int[] result = _helper.GetDateStatistics<Mail>(StatType.Month, mailList, "Date");

            Assert.AreEqual(result[0], 4);
            Assert.AreEqual(result[1], 0);
            Assert.AreEqual(result[2], 1);
            Assert.AreEqual(result[3], 1);
            Assert.AreEqual(result[4], 0);
            Assert.AreEqual(result[5], 1);
            Assert.AreEqual(result[6], 0);
            Assert.AreEqual(result[7], 0);
            Assert.AreEqual(result[8], 0);
            Assert.AreEqual(result[9], 0);
            Assert.AreEqual(result[10], 1);
            Assert.AreEqual(result[11], 2);
        }

        [TestMethod]
        public void Helper_GetMessagesBySubjectStatistic()
        {
            if (!(_helper is InterIMAPGmailHelper))
                return;

            List<StatType> stats = new List<StatType>();
            stats.Add(StatType.Month);
            stats.Add(StatType.Day);
            stats.Add(StatType.Hour4);
            stats.Add(StatType.Hour1);

            DateTime start = DateTime.Now;

            Dictionary<StatType, int[]> result = ((InterIMAPGmailHelper)_helper).GetDateStatistics(stats, _subject);

            DateTime stop = DateTime.Now;

            TimeSpan ts = stop.Subtract(start);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Helper_GetMessagesBySubjectUserStatistic()
        {
            if (!(_helper is InterIMAPGmailHelper))
                return;

            DateTime start = DateTime.Now;

            Dictionary<string, int> result = ((InterIMAPGmailHelper)_helper).GetUserStatistics(_subject);

            DateTime stop = DateTime.Now;

            TimeSpan ts = stop.Subtract(start);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Helper_GetMails()
        {
            int counter = 0;
            
            foreach (Mail m in _helper.GetMails(_subject, true, true))
            {
                counter++;
            }

            Assert.IsTrue(counter > 0);
        }
    }
}
