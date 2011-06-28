using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EmailStatistics
{
    public class Model : IModel
    {
        private int _updateInterval = 10;
        private int _inboxCounter;
        private int _sentCounter;
        private string _additionalWordCheck;

        public event EventHandler ModelUpdated;
        public event EventHandler CountersUpdated;

        public Model()
        {
            Reset();
        }

        public Model(int updateInterval, string additionalWordCheck = "")
            : this()
        {
            _updateInterval = updateInterval;
            _additionalWordCheck = additionalWordCheck;
        }

        public int InboxCount
        {
            get { return _inboxCounter; }
            set
            {
                _inboxCounter = value;

                if (CountersUpdated != null)
                    CountersUpdated(this, null);
            }
        }

        public int SentCount
        {
            get { return _sentCounter; }
            set
            {
                _sentCounter = value;

                if (CountersUpdated != null)
                    CountersUpdated(this, null);
            }
        }

        public int SelectedCount { get; set; }

        public int Process
        {
            get { return Mails.Count * 100 / SelectedCount; }
        }

        public int ProcessedCount
        {
            get { return Mails.Count; }
        }

        public List<Mail> Mails { get; set; }
        public List<User> Users { get; set; }

        // Update this all the time so can show what is happening on view
        public Dictionary<StatType, int[]> DateStats { get; set; }
        public Dictionary<string, int> UserStats { get; set; }

        public int TotalLenght
        {
            get
            {
                int result = 0;

                foreach (User u in Users)
                    result += u.TotalLength;

                return result;
            }
        }

        private int _updateCounter;


        public void Reset()
        {
            InboxCount = 0;
            SentCount = 0;
            SelectedCount = 0;

            Mails = new List<Mail>();
            Users = new List<User>();

            createStatsArrays();
        }

        public bool AddMail(Mail mail)
        {

            addMail(mail);

            _updateCounter++;

            if (_updateCounter >= _updateInterval)
            {
                _updateCounter = 0;

                if (ModelUpdated != null)
                    ModelUpdated(this, null);
            }

            return true;
        }

        public bool AddMails(List<Mail> mails)
        {
            foreach (Mail mail in mails)
            {
                addMail(mail);
            }

            if (ModelUpdated != null)
                ModelUpdated(this, null);


            return true;
        }

        public void DataReady()
        {
            // TODO: Need to do something in here?

            if (ModelUpdated != null)
                ModelUpdated(this, null);
        }

        private void addMail(Mail mail)
        {
            User user = Users.Where(u => u.EmailAddress.Equals(mail.From)).FirstOrDefault();
            if (user == null)
            {
                user = new User(mail, _additionalWordCheck);
                Users.Add(user);
            }
            else
                user.AddMail(mail);

            mail.Owner = user;
            Mails.Add(mail);

            updateStats(mail);
        }

        private void updateStats(Mail mail)
        {
            foreach (KeyValuePair<StatType, int[]> kvp in DateStats)
            {
                insertToStatsCollection(kvp.Key, kvp.Value, mail.Date);
            }

            if (UserStats.ContainsKey(mail.From))
                UserStats[mail.From]++;
            else
                UserStats.Add(mail.From, 1);
        }

        private void insertToStatsCollection(StatType statType, int[] stats, DateTime dt)
        {
            int index = 0;

            switch (statType)
            {
                case StatType.Month:
                    index = dt.Month - 1;
                    break;
                case StatType.Day:
                    index = (int)dt.DayOfWeek; //sunday is first, damn yanks
                    break;
                case StatType.Hour4:
                    index = dt.Hour / 4;
                    break;
                case StatType.Hour1:
                    index = dt.Hour;
                    break;
                default:
                    break;
            }

            stats[index]++;
        }

        private void createStatsArrays()
        {
            DateStats = new Dictionary<StatType, int[]>();
            DateStats.Add(StatType.Month, createStatsArray(StatType.Month));
            DateStats.Add(StatType.Day, createStatsArray(StatType.Day));
            //DateStats.Add(StatType.Hour4, createStatsArray(StatType.Hour4));
            DateStats.Add(StatType.Hour1, createStatsArray(StatType.Hour1));

            UserStats = new Dictionary<string, int>();
        }

        private int[] createStatsArray(StatType statType)
        {
            switch (statType)
            {
                case StatType.Month:
                    return new int[12];
                case StatType.Day:
                    return new int[7];
                case StatType.Hour4:
                    return new int[6];
                case StatType.Hour1:
                    return new int[24];
                default:
                    return new int[1];
            }
        }

        public string[] GetStatsHeaders(StatType statType)
        {
            string[] headers = new string[] { string.Empty };

            switch (statType)
            {
                case StatType.Month:
                    headers = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dev" };
                    break;
                case StatType.Day:
                    headers = new string[] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
                    break;
                case StatType.Hour4:
                    headers = new string[] { "0-4", "4-8", "8-12", "12-16", "16-20", "20-24" };
                    break;
                case StatType.Hour1:
                    headers = new string[] { "0-1", "1-2", "2-3", "3-4", "4-5", "5-6", "6-7", "7-8", "8-9", "9-10", "10-11", "11-12", "12-13", 
                        "13-14", "14-15", "15-16", "16-17", "17-18", "18-19", "19-20", "20-21","21-22","22-23","23-24"};
                    break;
                default:
                    break;
            }

            return headers;
        }

        public string GetUserStats
        {
            get
            {
                string retVal = "";

                foreach (User user in Users)
                {
                    retVal += user.GetUserStats();
                }

                return retVal;
            }
        }

        public string GetUserStatsNoText
        {
            get
            {
                string retVal = "";

                foreach (User user in Users)
                {
                    retVal += user.GetUserStatsNoText();
                }

                return retVal;
            }
        }

        public string PrintDateStats
        {
            get 
            {
                StringBuilder sb = new StringBuilder(); 

                foreach (KeyValuePair<StatType, int[]> kvp in DateStats)
                {
                    sb.Append(kvp.Key.ToString() + "\r\n");

                    foreach (int i in kvp.Value)
                    {
                        sb.Append(i + "\r\n"); 
                    }

                    sb.Append("\r\n");
                }

                return sb.ToString();
            }
        }
    }

    public class User
    {
        public List<Mail> Mails { get; set; }
        public string EmailAddress { get; set; }

        private string _additionalWordCheck;
        private int _additionalWordMailsCount;
        private int _additionalWordTotalCount;

        private int _underXWords = 3;

        private int _totalLength;
        private int _wordCount;
        private int _underXWordsCount;

        private int _linkCount;
        private int _mailsWithLink;

        public double AvgLength
        {
            get { return _totalLength / Mails.Count; }
        }

        public double AvgWordCount
        {
            get { return _wordCount / Mails.Count; }
        }

        public int TotalLength
        {
            get { return _totalLength; }
        }

        private Regex _regex = new Regex("<a href=\".*\">");

        public User()
        {
            Mails = new List<Mail>();
        }

        public User(Mail mail, string additionalWordCheck = "")
        {
            Mails = new List<Mail>();
            Mails.Add(mail);
            EmailAddress = mail.From;

            _additionalWordCheck = additionalWordCheck;
        }

        internal void AddMail(Mail mail)
        {
            Mails.Add(mail);

            if (!string.IsNullOrEmpty(mail.Body))
            {
                _totalLength += mail.Body.Trim().Length;
                string[] words = mail.Body.Trim().Split(' ');
                _wordCount += words.Length;

                if (!string.IsNullOrEmpty(_additionalWordCheck))
                {
                    Regex r = new Regex(_additionalWordCheck, RegexOptions.IgnoreCase);
                    MatchCollection matches = r.Matches(mail.Body);
                    _additionalWordTotalCount += matches.Count;

                    if (matches.Count > 0)
                        _additionalWordMailsCount += 1;
                }

                MatchCollection mCol = _regex.Matches(mail.BodyHtml);
                _linkCount += mCol.Count;
                _mailsWithLink += mCol.Count > 0 ? 1 : 0;

                // If mail has link, but still under x words, do not count it. Links are important.
                if (mCol.Count == 0 && words.Length < _underXWords)
                    _underXWordsCount++;
            }
        }

        public string GetUserStats()
        {
            string retVal = "User: {0}\r\nMails: {1}\r\n" +
                            "Length: {2}\r\nAvg. length: {3}\r\n" +
                            "Word count: {4}\r\nAvg. Word count: {5}\r\n" +
                            "Total links: {6}\r\nMails with links: {7}\r\n" +
                            "Mails with under {8} words: {9}\r\n";

            if (!string.IsNullOrEmpty(_additionalWordCheck))
            {
                retVal += "Additional word check: {10}\r\nMails with additional words: {11}\r\n";
                retVal = string.Format(retVal, EmailAddress, Mails.Count, _totalLength, AvgLength, _wordCount,
                    AvgWordCount, _linkCount, _mailsWithLink, _underXWords, _underXWordsCount, _additionalWordTotalCount, _additionalWordMailsCount);
            }
            else
                retVal = string.Format(retVal, EmailAddress, Mails.Count, _totalLength, AvgLength, _wordCount, AvgWordCount, _linkCount, _mailsWithLink, _underXWords, _underXWordsCount);

            retVal += "-------------------------------------------------\r\n";

            return retVal;
        }

        public string GetUserStatsNoText()
        {
            string retVal = "{0}\r\n{1}\r\n" +
                            "{2}\r\n{3}\r\n" +
                            "{4}\r\n{5}\r\n" +
                            "{6}\r\n{7}\r\n" +
                            "{8}\r\n"; ;

            if (!string.IsNullOrEmpty(_additionalWordCheck))
            {
                retVal += "{9}\r\n{10}\r\n";
                retVal = string.Format(retVal, EmailAddress, Mails.Count, _totalLength, AvgLength, _wordCount,
                    AvgWordCount, _linkCount, _mailsWithLink, _underXWordsCount, _additionalWordTotalCount, _additionalWordMailsCount);
            }
            else
                retVal = string.Format(retVal, EmailAddress, Mails.Count, _totalLength, AvgLength, _wordCount, AvgWordCount, _linkCount, _mailsWithLink, _underXWordsCount);

            retVal += "-------------------------------------------------\r\n";

            return retVal;
        }
    }
}
