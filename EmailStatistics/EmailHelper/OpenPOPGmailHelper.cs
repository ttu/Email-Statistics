using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenPop.Pop3;
using OpenPop.Mime;

namespace EmailStatistics
{
    // TODO: Whole class
    public class OpenPOPGmailHelper : EmailHelper
    {
        Pop3Client _client;
        string _username;
        string _password;
        string _host;
        int _port;
        bool _useSSL;

        private string _inboxName = "INBOX";
        private string _sentFolderName = "[Gmail]";
        private string _sentSubFolderName = "Sent Mail";

        public OpenPOPGmailHelper(): base()
        {
            _client = new Pop3Client();
        }

        protected override bool logIn()
        {
            _client.Connect(_host, _port, _useSSL);
            _client.Authenticate(_username, _password);

            return true;
        }

        protected override bool logOut()
        {
            if (_client.Connected)
                _client.Disconnect();

            return false;
        }

        public override void SetConfig(string host, int port, bool useSSL, string username, string password)
        {
            _host = host;
            _port = port;
            _useSSL = useSSL;
            _username = username;
            _password = password;
        }

        public override List<Mail> GetAllInboxMessages()
        {
            throw new NotImplementedException();
        }

        public override List<Mail> GetMessages(string folder)
        {
            throw new NotImplementedException();
        }

        public override List<Mail> GetMessagesBySubject(string subject)
        {
            throw new NotImplementedException();
        }

        public int[] GetDateStatistics(StatType statType, string subject)
        {
            List<StatType> stats = new List<StatType>();
            stats.Add(statType);

            Dictionary<StatType, int[]> retVal = GetDateStatistics(stats, subject);

            return retVal[statType];
        }

        public Dictionary<StatType, int[]> GetDateStatistics(List<StatType> statTypes, string subject)
        {
            logIn();

            //List<string> uids = _client.

            //List<Message> messages = new List<Message>();

            //foreach(string uid in uids)
            //{
            //    Message message = _client.
            //}
            //Dictionary<StatType, int[]> dict_1 = getDateStatistics(inbox, statTypes, subject);

            //IMAPFolder SentFolder = _client.GetSingleFolder("[Gmail]");
            //IMAPFolder SentSubFolder = SentFolder.SubFolders["Sent Mail"];

            //Dictionary<StatType, int[]> dict_2 = getDateStatistics(SentSubFolder, statTypes, subject);

            Dictionary<StatType, int[]> retVal = new Dictionary<StatType,int[]>();// = base.CombineDictionarys(dict_1, dict_2);

            logOut();

            return retVal;
        }

        public override IEnumerable<Mail> GetMails(string subject, bool getInbox, bool getSent)
        {
            throw new NotImplementedException();
        }

        public override bool HasConnection()
        {
            throw new NotImplementedException();
        }

        public override int GetMessageCountBySubject(string subject, bool getInbox, bool getSent)
        {
            throw new NotImplementedException();
        }

        //private Mail convertToMail_2(Message m)
        //{
        //    Mail retVal = new Mail();
        //    retVal.From = convertoStringList(m.From);
        //    retVal.To = convertoStringList(m.To);
        //    retVal.Cc = m.Cc;

        //    retVal.Date = m.Date;

        //    retVal.Subject = m.Subject;
        //    retVal.Body = m.TextBody.TextData;

        //    return retVal;
        //}

        //private List<string> convertoStringList_2(List<MailAddress> list)
        //{
        //    List<string> retVal = new List<string>();

        //    foreach (MailAddress ma in list)
        //    {
        //        if (!retVal.Contains(ma.Address))
        //            retVal.Add(ma.Address);
        //    }

        //    return retVal;
    }

}
