using System;
using System.Collections.Generic;
using InterIMAP;

namespace EmailStatistics
{
	public class InterIMAPGmailHelper: EmailHelper
	{
		IMAPClient _client;

        private string _inboxName = "INBOX";
        private string _sentFolderName = "[Gmail]";
        private string _sentSubFolderName = "Sent Mail";

        public InterIMAPGmailHelper()
            : base()
		{
			_client = new IMAPClient();			
		}

        public InterIMAPGmailHelper(int id)
            : this()
        {
            base._id = id;
        }

        protected override bool logIn()
        {
             _client.Logon();

            return true;
        }

        protected override bool logOut()
        {
            if (_client.LoggedOn)
                _client.Logoff();

            return true;
        }

	public override void SetConfig(string host, int port, bool useSSL, string username, string password)
        {
            IMAPConfig conf = new IMAPConfig(host, username, password, useSSL, false, "");
            _client.Config = conf;
        }

        public override bool HasConnection()
        {
            return _client.LoggedOn;
        }
		
		#region List<Mail> methods
		
        public override List<Mail> GetAllInboxMessages()
        {
            return GetMessages(_inboxName);
        }

        public override List<Mail> GetMessages(string folder)
        {
            IMAPFolder inbox = _client.GetSingleFolder(folder);

            List<Mail> retVal = new List<Mail>();

            foreach (IMAPMessage m in inbox.Messages)
            {
                retVal.Add(convertToMail(m));
            }

            return retVal;
        }

        public override List<Mail> GetMessagesBySubject(string subject)
        {
            IMAPFolder inbox = _client.GetSingleFolder(_inboxName);

            List<Mail> retVal = convertMessagesToCollection(subject, inbox);

            IMAPFolder SentFolder = _client.GetSingleFolder(_sentFolderName);
            IMAPFolder SentSubFolder = SentFolder.SubFolders[_sentSubFolderName];

            List<Mail> retVal_2 = convertMessagesToCollection(subject, SentSubFolder);

            foreach (Mail m in retVal_2)
                retVal.Add(m);

            return retVal;
        }

        private List<Mail> convertMessagesToCollection(string subject, IMAPFolder folder)
        {
            IMAPSearchQuery sq = new IMAPSearchQuery();
            sq.Subject = subject;
            IMAPSearchResult result = folder.Search(sq);

            List<Mail> retVal = new List<Mail>();

            foreach (IMAPMessage m in result.Messages)
            {
                retVal.Add(convertToMail(m));
            }

            return retVal;
        }

		#endregion
		
		#region IEnumrable methods
		
		public override IEnumerable<Mail> GetMails(string subject, bool getInbox, bool getSent)
        {         
            IMAPSearchQuery sq = new IMAPSearchQuery();
            sq.Subject = subject;

            // InterIMAP remembers only lates result, so will use only 1 result and update it again later
            IMAPSearchResult result = null;
            IMAPMessageCollection messages = null;

            if (getInbox)
            {
				IMAPFolder inbox = _client.GetSingleFolder(_inboxName);

                if (!string.IsNullOrEmpty(subject))
                {
                    result = inbox.Search(sq);
                    messages = result.Messages;
                }
                else
                    messages = inbox.Messages;

                foreach (IMAPMessage m in messages)
                    yield return convertToMail(m);

            }

            if (getSent)
            {			
            	IMAPFolder sentFolder = _client.GetSingleFolder(_sentFolderName);
            	IMAPFolder sentSubFolder = sentFolder.SubFolders[_sentSubFolderName];

                if (!string.IsNullOrEmpty(subject))
                {
                    result = sentSubFolder.Search(sq);
                    messages = result.Messages;
                }
                else
                    messages = sentSubFolder.Messages;

                foreach (IMAPMessage m in result.Messages)
                    yield return convertToMail(m);
            }
        }
		
		#endregion
		
        #region Message count

        public int GetMessageCount(bool getInbox, bool getSent)
        {
			int retVal = 0;
			
            if (getInbox)
			{
	            IMAPFolder inbox = _client.GetSingleFolder(_inboxName);
	            retVal += inbox.Messages.Count;
			}
			
			if (getSent)
			{
				IMAPFolder SentFolder = _client.GetSingleFolder(_sentFolderName);
	            IMAPFolder SentSubFolder = SentFolder.SubFolders[_sentSubFolderName];
	            retVal += SentSubFolder.Messages.Count;
			}
			
            return retVal;
        }

        public override int GetMessageCountBySubject(string subject, bool getInbox, bool getSent)
        {
            if (string.IsNullOrEmpty(subject))
                return GetMessageCount(getInbox, getSent);

			IMAPSearchQuery sq = new IMAPSearchQuery();
            sq.Subject = subject;
            IMAPSearchResult result = null;

			int retVal = 0;
			
			if (getInbox)
			{
	            IMAPFolder inbox = _client.GetSingleFolder(_inboxName);
				result = inbox.Search(sq);
	            retVal += result.Messages.Count;
			}
			
			if (getSent)
			{
				IMAPFolder SentFolder = _client.GetSingleFolder(_sentFolderName);
	            IMAPFolder SentSubFolder = SentFolder.SubFolders[_sentSubFolderName];
				result = SentSubFolder.Search(sq);
	            retVal += result.Messages.Count;
			}
			
            return retVal;
        }

        #endregion

        #region int[] Statistics

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

            IMAPFolder inbox = _client.GetSingleFolder(_inboxName);

            Dictionary<StatType, int[]> dict_1 = getDateStatistics(inbox, statTypes, subject);

            IMAPFolder SentFolder = _client.GetSingleFolder(_sentFolderName);
            IMAPFolder SentSubFolder = SentFolder.SubFolders[_sentSubFolderName];

            Dictionary<StatType, int[]> dict_2 = getDateStatistics(SentSubFolder, statTypes, subject);

            Dictionary<StatType, int[]> retVal = base.CombineDictionarys(dict_1, dict_2);

            logOut();

            return retVal;
        }

        private Dictionary<StatType, int[]> getDateStatistics(IMAPFolder folder, List<StatType> statTypes, string subject)
        {
            IMAPSearchQuery sq = new IMAPSearchQuery();
            sq.Subject = subject;
            IMAPSearchResult result = folder.Search(sq);

            Dictionary<StatType, int[]> retVal = base.GetDateStatistics<IMAPMessage>(statTypes, result.Messages, "Date");

            return retVal;
        }

        public Dictionary<string, int> GetUserStatistics(string subject)
        {
            logIn();

            IMAPFolder inbox = _client.GetSingleFolder(_inboxName);

            Dictionary<string, int> dict_1 = getUserStatistics(inbox, subject);

            IMAPFolder SentFolder = _client.GetSingleFolder(_sentFolderName);
            IMAPFolder SentSubFolder = SentFolder.SubFolders[_sentSubFolderName];

            Dictionary<string, int> dict_2 = getUserStatistics(SentSubFolder, subject);

            Dictionary<string, int> retVal = base.CombineDictionarys(dict_1, dict_2);

            logOut();

            return retVal;
        }

        private Dictionary<string, int> getUserStatistics(IMAPFolder folder, string subject)
        {
            IMAPSearchQuery sq = new IMAPSearchQuery();
            sq.Subject = subject;
            IMAPSearchResult result = folder.Search(sq);

            Dictionary<string, int> retVal = base.GetUserSatistics<IMAPMessage, IMAPMailAddress>(result.Messages, "From", "Address");

            return retVal;
        }

        #endregion
		
        #region Mappers
        
        private Mail convertToMail(IMAPMessage m)
        {
            Mail retVal = new Mail();
            retVal.From = m.From.Count > 0 ? m.From[0].Address : string.Empty;
            retVal.To = convertoStringList(m.To);
            retVal.Cc = convertoStringList(m.Cc);

            retVal.Date = m.Date;

            retVal.Subject = m.Subject;
            retVal.Body = m.TextData != null ? m.TextData.TextData : string.Empty;

            return retVal;
        }

        private List<string> convertoStringList(List<IMAPMailAddress> list)
        {
            List<string> retVal = new List<string>();

            foreach (IMAPMailAddress ma in list)
            {
                if (!retVal.Contains(ma.Address))
                    retVal.Add(ma.Address);
            }

            return retVal;
        }

        #endregion

    }
}

